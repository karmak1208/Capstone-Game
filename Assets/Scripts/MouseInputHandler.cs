using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class MouseInputHandler : MonoBehaviour
{
    [SerializeField] Tilemap floormap;
    [SerializeField] Transform highlightmapTransform;
    [SerializeField] float tileSize;
    [SerializeField] Transform PlayerTransform;
    [SerializeField] PlayerController playerController;
    [SerializeField] PathfindingSystem pathfindingSystem;
    public InputAction clickAction;

    void Start()
    {
        clickAction = InputSystem.actions["Click"];
        clickAction.performed += ctx => MovePlayerToHighlightedTile();
    }

    void Update()
    {
        HighlightTile();
    }

    void HighlightTile()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector3Int cellPos = floormap.WorldToCell(mouseWorldPos);
        highlightmapTransform.position = floormap.GetCellCenterWorld(cellPos) - new Vector3(tileSize/2, tileSize/2, 0);
    }

    void MovePlayerToHighlightedTile()
    {
        Debug.Log("Click detected, attempting to move player.");
        if (playerController.isMoving) return; // Prevent starting a new move while already moving
        Vector3Int targetCellPos = floormap.WorldToCell(Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue()));
        Vector3Int playerCellPos = floormap.WorldToCell(PlayerTransform.position);
        List<Vector3Int> pathList = pathfindingSystem.FindPath(playerCellPos, targetCellPos);
        if (pathList != null && pathList.Count > 0)
        {
                // Start moving the player along the path
            Debug.Log($"Path found with {pathList.Count} steps. Starting movement.");
            StartCoroutine(playerController.MovePlayerAlongPath(pathList));
        }
    }
}
