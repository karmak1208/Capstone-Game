using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;

public class MovementController : MonoBehaviour, IResettable, IActivatable
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
        Root.OnFinishedLoading.AddListener(Initialize);
        remainingMovementRange = maxMovementRange;
    }

    void Initialize()
    {
        maxMovementRange = Root.Data.MaxMovementRange;
        moveSpeed = Root.Data.MoveSpeed;
    }
    void Start()
    {
        moveRangeText = FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None).FirstOrDefault(t => t.gameObject.name == "ActiveMoveRange");
        if (moveRangeText == null) Debug.LogError($"[MOVEMENT] Failed to find TextMeshProUGUI with name 'ActiveMoveRangeText' in the scene.");

        floormap = FindObjectsByType<Tilemap>(FindObjectsSortMode.None).FirstOrDefault(t => t.gameObject.name == "Floor");
        if (floormap == null) Debug.LogError($"[MOVEMENT] Failed to find floormap in the scene.");
    }

    public void SetActive(bool enabled)
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

    /// <summary>
    /// Starts the process of moving the character from its current cell position to the target tile position. 
    /// If a valid path is found and it is within the remaining movement range, it starts MovePlayerAlongPath coroutine.
    /// </summary>
    public void MoveTo(Vector3Int characterCellPos, Vector3Int targetTilePos)
    {
        if (!inputEnabled || isMoving) { Debug.Log("[MOVEMENT] Input is currently disabled for this character."); return; }

        if (IsMoveTooFar(characterCellPos, targetTilePos)) { Debug.Log($"[MOVEMENT] Target tile is too far."); return; }

        List<Vector3Int> pathList = PathfindingSystem.Instance.FindPath(characterCellPos, targetTilePos);
        int pathDistance = pathList.Count - 1;
        if (pathList != null && pathList.Count > 0)
        {
            if (pathDistance <= remainingMovementRange)
            {
                StartCoroutine(MovePlayerAlongPath(pathList));
                remainingMovementRange -= pathDistance;
                moveRangeText.text = $"Move Range: {remainingMovementRange}";
            }
            else { Debug.Log($"[MOVEMENT] Path found but exceeds remaining movement range. Path steps: {pathDistance}, Remaining Range: {remainingMovementRange}"); }
        }
        else { Debug.Log("[MOVEMENT] No valid path found to the target tile."); }
    }

    /// <summary>
    /// Moves the player along the given path list, which is a list of tile positions. 
    /// It moves towards each tile in sequence, waiting until it reaches each tile before moving to the next one.
    /// </summary>
    /// <param name="pathList">The list of tile positions to move along.</param>
    public IEnumerator MovePlayerAlongPath(List<Vector3Int> pathList)
    {
        isMoving = true;
        CharacterManager.Instance.OnCharacterStartedMove.Invoke();
        foreach (Vector3Int tile in pathList)
        {
            // Keep moving each frame until we reach the tile
            while (transform.position != (Vector3)tile)
            {
                transform.position = Vector3.MoveTowards(transform.position, tile, moveSpeed * Time.deltaTime);

                yield return null; // Wait one frame, then continue the while loop
            }
            Root.Highlighter.ClearTile(tile); // Hide the tile highlight after reaching it
        }
        isMoving = false;
        CharacterManager.Instance.OnCharacterEndedMove.Invoke();
    }

    public void StartTurnReset()
    {
        ResetMovementRange();
    }

    private void ResetMovementRange()
    {
        remainingMovementRange = maxMovementRange;
        moveRangeText.text = $"Move Range: {remainingMovementRange}";
    }
}
