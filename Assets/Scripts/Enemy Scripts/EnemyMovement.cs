using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class EnemyMovement : MonoBehaviour
{
    private EnemyRoot Root;
    public Tilemap floormap;

    private List<Vector3Int> patrolPath = new();
    [SerializeField] private Vector3Int patrolStart;
    [SerializeField] private Vector3Int patrolEnd;

    [SerializeField] private Vector3Int idlePos;

    public Vector3Int CellPos => floormap.WorldToCell(transform.position);
    [SerializeField] private float moveSpeed = 2f;
    public bool isMoving = false;
    private bool newPlayerSpotted = false;
    private Vector3Int lastPatrolPoint;
    private void Start()
    {
        Root = GetComponent<EnemyRoot>();
        if (Root == null) { Debug.LogError($"[ENEMY MOVEMENT] No EnemyRoot component found on {gameObject.name}."); }

        floormap = FindObjectsByType<Tilemap>(FindObjectsSortMode.None).FirstOrDefault(t => t.gameObject.name == "Floor");

        if (Root.initialState == startingState.Patrol)
        {
            patrolStart = CellPos;
            Transform end = transform.Find("PatrolEnd");
            patrolEnd = floormap.WorldToCell(end.position);
            Destroy(end.gameObject);
            if (patrolStart != Vector3Int.zero && patrolEnd != Vector3Int.zero)
            {
                Debug.Log($"[ENEMY MOVEMENT] {Root.EnemyName} is calculating patrol path from {patrolStart} to {patrolEnd}.");
                patrolPath = PathfindingSystem.Instance.FindPath(patrolStart, patrolEnd);
                if (patrolPath == null) { Debug.LogWarning($"[ENEMY MOVEMENT] No path found between patrol start and end for {gameObject.name}."); }

                transform.position = patrolStart; // Start at the patrol start position
            }
        }
        if (Root.initialState == startingState.Idle)
        {
            Debug.Log($"[ENEMY MOVEMENT] {Root.EnemyName} is setting idle position at {CellPos}.");
            idlePos = CellPos;
        }
        Root.Sight.OnPlayerSpotted.AddListener(PlayerSpotted);
    }

    public void PlayerSpotted() => newPlayerSpotted = isMoving? true: false;

    public void ReturnToIdlePos()
    {
        TurnManager.Instance.RegisterEndTurnTask();
        IEnumerator MoveAndComplete()
        {
            if (CellPos != idlePos)
            {
                List<Vector3Int> pathToIdle = PathfindingSystem.Instance.FindPath(CellPos, idlePos);
                yield return MoveAlongPath(pathToIdle);
            }
            Root.Sight.SetFacingDirection(Root.Sight.IdleFacingDir);
            TurnManager.Instance.CompleteEndTurnTask();
        }
        StartCoroutine(MoveAndComplete());
    }

    public void StartPatrolMove()
    {
        TurnManager.Instance.RegisterEndTurnTask();
        IEnumerator MoveAndComplete()
        {
            if (CellPos != lastPatrolPoint && lastPatrolPoint != Vector3Int.zero)
            {
                List<Vector3Int> pathToLastPatrol = PathfindingSystem.Instance.FindPath(CellPos, lastPatrolPoint);
                yield return MoveAlongPath(pathToLastPatrol);
            }
            else
            {
                if (patrolPath == null || patrolPath.Count == 0) { Debug.LogWarning("[ENEMY MOVEMENT] No path found between patrol start and end."); yield break; }
                yield return MoveAlongPath(patrolPath);
            }

            lastPatrolPoint = patrolPath[patrolPath.Count - 1];
            patrolPath.Reverse();
            TurnManager.Instance.CompleteEndTurnTask();
        }
        StartCoroutine(MoveAndComplete());
    }

    public void StartChaseMove()
    {
        Debug.Log($"[ENEMY MOVEMENT] Starting chase move");
        TurnManager.Instance.RegisterEndTurnTask();
        IEnumerator MoveAndComplete()
        {
            Vector3Int playerCellPos = floormap.WorldToCell(Root.Sight.TargetPos);
            List<Vector3Int> path = PathfindingSystem.Instance.FindPath(CellPos, playerCellPos);
            Vector3Int targetTile = path[path.Count - 1];
            path.RemoveAt(path.Count - 1); // Remove tile player is on
            yield return MoveAlongPath(path);
            Root.Sight.LookAt(targetTile);
            Root.Sight.SightUpdate();
            
            TurnManager.Instance.CompleteEndTurnTask();
        }
        StartCoroutine(MoveAndComplete());
    }

    IEnumerator MoveAlongPath(List<Vector3Int> path)
    {
        Debug.Log($"[ENEMY MOVEMENT] {Root.EnemyName} started moving along path from {path[0]} to {path[path.Count-1]}");
        isMoving = true;
        foreach (Vector3Int tile in path)
        {
            Root.Sight.SetFacingDirection((tile - transform.position).normalized); // Update facing direction towards the next tile
            // Keep moving each frame until we reach the tile
            while (transform.position != (Vector3)tile)
            {
                transform.position = Vector3.MoveTowards(transform.position, tile, moveSpeed * Time.deltaTime);
                yield return null;
            }
            if (newPlayerSpotted)
            {
                Debug.Log($"[ENEMY MOVEMENT] New Player Spotted, stopping movement.");
                isMoving = false;
                newPlayerSpotted = false;
                yield break;
            }
        }
        isMoving = false;
    }
}