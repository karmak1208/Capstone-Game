using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class Node
{
    public Vector3Int Position;
    public float GCost;
    public float HCost;
    public float FCost => GCost + HCost;
    public Node Parent;
    public Node(Vector3Int position)
    {
        Position = position;
    }
}

public class PathfindingSystem : MonoBehaviour
{

    public static PathfindingSystem Instance { get; private set; }
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
        DoorController[] allDoors = FindObjectsByType<DoorController>(FindObjectsSortMode.None);
        foreach (DoorController door in allDoors)
        {
            doors[floormap.WorldToCell(door.transform.position)] = door;
            Debug.Log($"[Pathfinding] Added {door.transform.position} to doors with state {door.IsOpen}");
        }

        bounds = floormap.cellBounds;
        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            nodes[pos] = new Node(pos);
        }
    }

    [SerializeField] Tilemap floormap;
    [SerializeField] Tilemap wallmap;
    private BoundsInt bounds;
    public Dictionary<Vector3Int, Node> nodes = new Dictionary<Vector3Int, Node>();

    private Dictionary<Vector3Int, DoorController> doors = new();

    void Start()
    {

    }

    /// <summary>
    /// Determines if a tile is walkable by checking if it has a floor tile and does not have a wall tile or closed door.
    /// </summary>
    /// <param name="pos">The position of the tile to check.</param>
    /// <returns>True if the tile is walkable, false otherwise.</returns>
    bool IsTileWalkable(Vector3Int pos)
    {
        bool hasFloor = floormap.HasTile(pos);
        bool hasWall = wallmap.HasTile(pos);
        bool hasClosedDoor = doors.TryGetValue(pos, out DoorController door) && !door.IsOpen;

        return hasFloor && !hasWall && !hasClosedDoor;
    }

    /// <summary>
    /// Gets the neighboring nodes of a given node, considering only the four cardinal directions (up, down, left, right).
    /// </summary>
    /// <param name="node">The node for which to find neighbors.</param>
    /// <returns>A list of neighboring nodes.</returns>
    private List<Node> GetNeighbors(Node node)
    {
        List<Node> neighbors = new List<Node>();

        Vector3Int[] cardinalDirs = new Vector3Int[]
        {
        new Vector3Int(0, 1, 0),    // Up
        new Vector3Int(0, -1, 0),   // Down
        new Vector3Int(-1, 0, 0),   // Left
        new Vector3Int(1, 0, 0),    // Right
        };

        foreach (Vector3Int dir in cardinalDirs)
        {
            Vector3Int neighborPos = node.Position + dir;
            if (nodes.TryGetValue(neighborPos, out Node neighbor))
                neighbors.Add(neighbor);
        }

        return neighbors;
    }

    /// <summary>
    /// Finds a path from the start position to the target position using the A* algorithm.
    /// </summary>
    /// <param name="startPos">The starting position of the path.</param>
    /// <param name="targetPos">The target position of the path.</param>
    /// <returns>A list of positions representing the path, or null if no path is found.</returns>
    public List<Vector3Int> FindPath(Vector3Int startPos, Vector3Int targetPos)
    {
        bool Try(bool result, string label) { if (!result) Debug.Log($"[PATHFINDING] Missing node: {label}"); return result; }

        if (!Try(nodes.TryGetValue(startPos, out Node startNode), "start") |
            !Try(nodes.TryGetValue(targetPos, out Node targetNode), "target"))
        {
            Debug.LogError("[PATHFINDING] Start or target position is out of bounds of the tilemap.");
            return null;
        }

        List<Node> openList = new List<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();

        // Reset costs on all nodes
        foreach (var n in nodes.Values)
        {
            n.GCost = float.MaxValue;
            n.HCost = 0;
            n.Parent = null;
        }

        startNode.GCost = 0;
        startNode.HCost = GetHeuristic(startNode, targetNode);
        openList.Add(startNode);

        while (openList.Count > 0)
        {
            // Pick node with lowest FCost (ties broken by HCost)
            Node current = openList[0];
            for (int i = 1; i < openList.Count; i++)
            {
                if (openList[i].FCost < current.FCost ||
                   (openList[i].FCost == current.FCost && openList[i].HCost < current.HCost))
                    current = openList[i];
            }

            openList.Remove(current);
            closedSet.Add(current);

            // Path found
            if (current == targetNode)
                return RetracePath(startNode, targetNode);

            foreach (Node neighbor in GetNeighbors(current))
            {
                if (!IsTileWalkable(neighbor.Position) || closedSet.Contains(neighbor))
                    continue;

                float tentativeG = current.GCost + GetMovementCost(current, neighbor);

                if (tentativeG < neighbor.GCost)
                {
                    neighbor.Parent = current;
                    neighbor.GCost = tentativeG;
                    neighbor.HCost = GetHeuristic(neighbor, targetNode);

                    if (!openList.Contains(neighbor))
                        openList.Add(neighbor);
                }
            }
        }

        Debug.LogWarning("[PATHFINDING] No path found between start and target positions.");
        return null; // No path found
    }

    /// <summary>
    /// Retraces the path from the end node to the start node by following the parent references to construct the list of positions that form the path.
    /// </summary>
    /// <param name="startNode">The starting node of the path.</param>
    /// <param name="endNode">The ending node of the path.</param>
    /// <returns>A list of positions representing the path from start to end.</returns>
    private List<Vector3Int> RetracePath(Node startNode, Node endNode)
    {
        List<Vector3Int> path = new List<Vector3Int>();
        Node current = endNode;

        while (current != startNode)
        {
            path.Add(current.Position);
            current = current.Parent;
        }

        path.Add(startNode.Position);
        if (path.Count > 1)
        {
            path.Reverse();
            return path;
        }

        Debug.LogWarning("[PATHFINDING] Start and target positions are the same. No path needed.");
        return null;
    }

    private float GetMovementCost(Node a, Node b)
    {
        bool isDiagonal = a.Position.x != b.Position.x &&
                          a.Position.y != b.Position.y;
        return isDiagonal ? 1.414f : 1f;
    }

    private float GetHeuristic(Node a, Node b)
    {
        int dx = Mathf.Abs(a.Position.x - b.Position.x);
        int dy = Mathf.Abs(a.Position.y - b.Position.y);
        return dx + dy; // Manhattan distance
    }
}