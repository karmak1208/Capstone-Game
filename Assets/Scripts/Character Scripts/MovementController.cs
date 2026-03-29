using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MovementController : MonoBehaviour
{
    [Header("Components")]
    public Rigidbody2D rb;
    public LayerMask obstacleLayer;
    public Tilemap floormap;
    private CharacterRoot Root;

    [Header("Character Settings")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float movementRange;

    public bool isMoving = false;
    [SerializeField] private int tileSize;
    public bool inputEnabled { get; private set; }

    private void Awake()
    {
        Root = GetComponent<CharacterRoot>();

    }

    public void SetInputEnabled(bool enabled)
    {
        inputEnabled = enabled;
        Debug.Log($"[MOVEMENT] Movement Input for {Root.CharacterName} set to: {inputEnabled}");
    }

    public void MoveTo(Vector3Int characterCellPos, Vector3Int targetTilePos)
    {
        Debug.Log($"[MOVEMENT] Active character: {CharacterManager.Instance.ActiveCharacter.CharacterName}");
        if (!inputEnabled || isMoving)
        {
            Debug.Log("[MOVEMENT] Input is currently disabled for this character.");
            return;
        }
        List<Vector3Int> pathList = PathfindingSystem.Instance.FindPath(characterCellPos, targetTilePos);
        if (pathList != null && pathList.Count > 0)
        {
            Debug.Log($"[MOVEMENT] Path found with {pathList.Count} steps. Starting movement.");
            StartCoroutine(MovePlayerAlongPath(pathList));
        }
        else
        {
            Debug.Log("[MOVEMENT] No valid path found to the target tile.");
        }
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
