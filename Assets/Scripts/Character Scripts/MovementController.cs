using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MovementController : MonoBehaviour
{
    [Header("Components")]
    public Rigidbody2D rb;
    public LayerMask obstacleLayer;
    public Tilemap floormap;
    private CharacterRoot Root;
    public TextMeshProUGUI moveRangeText;

    [Header("Character Settings")]
    [SerializeField] private float moveSpeed;
    public float maxMovementRange;
    public float remainingMovementRange;

    public bool isMoving = false;
    [SerializeField] private int tileSize;
    public bool inputEnabled { get; private set; }

    private void Awake()
    {
        Root = GetComponent<CharacterRoot>();
        remainingMovementRange = maxMovementRange;
    }

    public void SetInputEnabled(bool enabled)
    {
        inputEnabled = enabled;
        Debug.Log($"[MOVEMENT] Movement Input for {Root.CharacterName} set to: {inputEnabled}");
    }

    bool IsMoveTooFar(Vector3Int characterCellPos, Vector3Int targetTilePos)
    {
        //float distance = Vector3Int.Distance(characterCellPos, targetTilePos) * (tileSize * Mathf.Sqrt(2));
        //return distance > remainingMovementRange;
        return false; // Temporarily return false
    }

    public void MoveTo(Vector3Int characterCellPos, Vector3Int targetTilePos)
    {
        Debug.Log($"[MOVEMENT] Attempting to move from {characterCellPos} to {targetTilePos} for {Root.CharacterName}.");
        if (!inputEnabled || isMoving) { Debug.Log("[MOVEMENT] Input is currently disabled for this character."); return; }

        if (IsMoveTooFar(characterCellPos, targetTilePos)) { Debug.Log($"[MOVEMENT] Target tile is too far."); return; }

        List<Vector3Int> pathList = PathfindingSystem.Instance.FindPath(characterCellPos, targetTilePos);
        Debug.Log($"[MOVEMENT] Pathfinding following path: {string.Join(" -> ", pathList)}");
        int pathDistance = pathList.Count - 1;
        if (pathList != null && pathList.Count > 0)
        {
            if (pathDistance <= remainingMovementRange)
            {
                Debug.Log($"[MOVEMENT] Path found with {pathDistance} steps. Starting movement.");
                StartCoroutine(MovePlayerAlongPath(pathList));
                remainingMovementRange -= pathDistance;
                moveRangeText.text = $"Move Range: {remainingMovementRange}";
            }
            else { Debug.Log($"[MOVEMENT] Path found but exceeds remaining movement range. Path steps: {pathDistance}, Remaining Range: {remainingMovementRange}"); }
        }
        else { Debug.Log("[MOVEMENT] No valid path found to the target tile."); }
    }

    public IEnumerator MovePlayerAlongPath(List<Vector3Int> pathList)
    {
        isMoving = true;
        foreach (Vector3Int tile in pathList)
        {
            // Keep moving each frame until we reach the tile
            while (transform.position != (Vector3)tile)
            {
                transform.position = Vector3.MoveTowards(transform.position, tile, moveSpeed * Time.deltaTime);
                yield return null; // Wait one frame, then continue the while loop
            }
            Root.Highlighter.SetCellVisible(tile, false); // Hide the tile highlight after reaching it
        }
        isMoving = false;
    }
}
