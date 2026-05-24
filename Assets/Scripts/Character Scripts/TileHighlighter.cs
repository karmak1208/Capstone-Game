using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class TileHighlighter : MonoBehaviour, IActivatable
{
    [SerializeField] Tilemap floormap;
    [SerializeField] Tilemap highlightMap;
    [SerializeField] float tileSize;

    [SerializeField] TileBase upRight;
    [SerializeField] TileBase upLeft;
    [SerializeField] TileBase downRight;
    [SerializeField] TileBase downLeft;
    [SerializeField] TileBase upStraight;
    [SerializeField] TileBase sideStraight;
    [SerializeField] TileBase empty;

    private List<Vector3Int> highlightedCells = new List<Vector3Int>();
    private Vector3Int mouseCellPos;

    private CharacterRoot Root;
    private bool isActive = false;
    private float updateInterval = 0.1f; // Update every 0.1 seconds
    private float timeSinceLastUpdate = 0f;

    void Awake()
    {
        
    }

    public void SetActive(bool active)
    {
        isActive = active;
        if (!isActive)
        {
            ClearAllHighlight();
        }

    }

    void Start()
    {
        Root = GetComponent<CharacterRoot>();
        Root.OnFinishedLoading.AddListener(Initialize);

        highlightMap = FindObjectsByType<Tilemap>(FindObjectsSortMode.None).FirstOrDefault(t => t.gameObject.name == "Highlight");
        if (highlightMap == null) Debug.LogError("[TileHighlighter] Failed to find HighlightMap in the scene.");

        floormap = FindObjectsByType<Tilemap>(FindObjectsSortMode.None).FirstOrDefault(t => t.gameObject.name == "Floor");

        highlightMap.gameObject.SetActive(true);
    }

    void Initialize()
    {

    }

    List<TileBase> GetHighlightTileBase(List<Vector3Int> path)
    {
        List<TileBase> tiles = new List<TileBase>();
        TileBase GetCurrentSprite(int index)
        {
            if (path.Count < 2) return empty;
            if (index == 0) return empty;

            if (index == path.Count - 1) return empty;

            Vector3Int current = path[index];
            Vector3Int next = path[index + 1];
            Vector3Int previous = path[index - 1];
            Vector3Int directionToNext = next - current;
            Vector3Int directionFromPrevious = current - previous;


            // Check corners first (compound conditions)
            if (directionToNext == Vector3Int.right && directionFromPrevious == Vector3Int.up) return upRight;
            if (directionToNext == Vector3Int.left && directionFromPrevious == Vector3Int.up) return upLeft;
            if (directionToNext == Vector3Int.right && directionFromPrevious == Vector3Int.down) return downRight;
            if (directionToNext == Vector3Int.left && directionFromPrevious == Vector3Int.down) return downLeft;

            if (directionToNext == Vector3Int.up && directionFromPrevious == Vector3Int.right) return downLeft;
            if (directionToNext == Vector3Int.up && directionFromPrevious == Vector3Int.left) return downRight;
            if (directionToNext == Vector3Int.down && directionFromPrevious == Vector3Int.right) return upLeft;
            if (directionToNext == Vector3Int.down && directionFromPrevious == Vector3Int.left) return upRight;

            // Then straight directions
            if (directionToNext == Vector3Int.up) return upStraight;
            if (directionToNext == Vector3Int.down) return upStraight;
            if (directionToNext == Vector3Int.right) return sideStraight;
            if (directionToNext == Vector3Int.left) return sideStraight;

            Debug.LogError($"[TileHighlighter] Invalid direction combo: toNext={directionToNext}, fromPrev={directionFromPrevious}");
            return empty; // safe fallback instead of null
        }

        for (int i = 0; i < path.Count; i++)
        {
            TileBase _tile = GetCurrentSprite(i);
            if (_tile is not TileBase)
            {
                Debug.LogWarning($"[TileHighlighter] GetCurrentSprite returned null for path index {i}. Defaulting to upStraight.");
                _tile = upStraight; // safe fallback
            }
            tiles.Add(_tile);
        }

        return tiles;
    }

    void HighlightPath(List<Vector3Int> pathList)
    {
        List<TileBase> tileBases = GetHighlightTileBase(pathList);
        if (pathList == null) return;

        for (int i = 0; i < pathList.Count; i++)
        {
            highlightMap.SetTile(pathList[i], tileBases[i]);
            highlightedCells.Add(pathList[i]);
        }
    }
    public void ClearTile(Vector3Int cellPos)
    {
        highlightMap.SetTile(cellPos, null);
    }

    void ClearAllHighlight()
    {
        foreach (Vector3Int pos in highlightedCells)
        {
            ClearTile(pos);
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
            ClearAllHighlight();
            return; 
        }

        if (Root.Movement.isMoving) return; // Don't update highlight while character is moving

        Vector3Int newMouseCellPos = GetMouseCellPosition();
        if (newMouseCellPos != mouseCellPos && timeSinceLastUpdate >= updateInterval)
        {
            ClearAllHighlight();
            mouseCellPos = newMouseCellPos;
            timeSinceLastUpdate = 0f;
            Vector3Int characterCellPos = floormap.WorldToCell(Root.Position);
            List<Vector3Int> pathList = PathfindingSystem.Instance.FindPath(characterCellPos, mouseCellPos);
            if (pathList != null && pathList.Count > 0)
                HighlightPath(pathList);
        }
        timeSinceLastUpdate += Time.deltaTime;
    }
}
