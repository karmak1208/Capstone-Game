using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class TileHighlighter : MonoBehaviour
{
    [SerializeField] Tilemap floormap;
    [SerializeField] Tilemap highlightMap;
    [SerializeField] float tileSize;

    private List<Vector3Int> highlightedCells = new List<Vector3Int>();
    private Vector3Int mouseCellPos;

    private CharacterRoot Root;
    private bool isActive = false;

    void Awake()
    {
        highlightMap.gameObject.SetActive(false);
    }

    public void SetActive(bool active)
    {
        isActive = active;
        if (!isActive)
        {
            ClearHighlight();
        }
    }

    void Start()
    {
        Root = GetComponent<CharacterRoot>();

        foreach (Vector3Int pos in highlightMap.cellBounds.allPositionsWithin)
        {
            if (highlightMap.HasTile(pos))
            {
                highlightMap.SetTileFlags(pos, TileFlags.None);
                highlightMap.SetColor(pos, new Color(1f, 1f, 1f, 0f));
            }
        }
        highlightMap.gameObject.SetActive(true);
    }

    public void SetCellVisible(Vector3Int cellPos, bool visible)
    {
        highlightMap.SetTileFlags(cellPos, TileFlags.None);
        highlightMap.SetColor(cellPos, visible ? new Color(1f, 1f, 1f, 0.5f) : new Color(1f, 1f, 1f, 0f));
    }

    void HighlightPath(Vector3Int startCell, Vector3Int targetCell)
    {
        List<Vector3Int> pathList = PathfindingSystem.Instance.FindPath(startCell, targetCell);
        if (pathList == null) return;

        foreach (Vector3Int cell in pathList)
        {
            SetCellVisible(cell, true);
            highlightedCells.Add(cell);
        }
    }

    void ClearHighlight()
    {
        foreach (Vector3Int pos in highlightedCells)
        {
            SetCellVisible(pos, false);
        }
        highlightedCells.Clear();
    }

    Vector3Int GetMouseCellPosition()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        return floormap.WorldToCell(mouseWorldPos);
    }

    void Update()
    {
        if (!isActive) return; // Don't update if not active
        if (Root.Movement.inputEnabled == false) // Don't update highlight if input is disabled
        {
            ClearHighlight();
            return; 
        }

        if (Root.Movement.isMoving) return; // Don't update highlight while character is moving

        Vector3Int newMouseCellPos = GetMouseCellPosition();
        if (newMouseCellPos != mouseCellPos)
        {
            ClearHighlight();
            mouseCellPos = newMouseCellPos;
            
            Vector3Int characterCellPos = floormap.WorldToCell(Root.Position);
            HighlightPath(characterCellPos, mouseCellPos);
        }
    }
}
