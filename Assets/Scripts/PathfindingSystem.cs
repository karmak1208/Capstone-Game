using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class Node
{
    public Vector3Int Position;
    public bool IsWalkable;
    public float GCost;
    public float HCost;
    public float FCost => GCost + HCost;
    public Node Parent;
    public Node(Vector3Int position, bool isWalkable)
    {
        Position = position;
        IsWalkable = isWalkable;
    }
}

public class PathfindingSystem : MonoBehaviour
{
    [SerializeField] Tilemap tilemap;
    private BoundsInt bounds;
    public Dictionary<Vector3Int, Node> nodes = new Dictionary<Vector3Int, Node>();

    void Start()
    {
        bounds = tilemap.cellBounds;
        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            if (!tilemap.HasTile(pos)) continue; // Skip empty tiles
            bool isWalkable = true; // All tiles are walkable
            nodes[pos] = new Node(pos, isWalkable);
        }
    }

    private List<Node> GetNeighbors(Node node)
    {
        List<Node> neighbors = new List<Node>();

        // Check cardinal directions first
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

        // For diagonals, both adjacent cardinal neighbors must be walkable
        Vector3Int[] diagonalDirs = new Vector3Int[]
        {
        new Vector3Int(1, 1, 0),    // Up-Right
        new Vector3Int(-1, 1, 0),   // Up-Left
        new Vector3Int(1, -1, 0),   // Down-Right
        new Vector3Int(-1, -1, 0),  // Down-Left
        };

        foreach (Vector3Int dir in diagonalDirs)
        {
            // The two orthogonal tiles that form the "corner" of this diagonal
            Vector3Int side1 = node.Position + new Vector3Int(dir.x, 0, 0); // Horizontal neighbor
            Vector3Int side2 = node.Position + new Vector3Int(0, dir.y, 0); // Vertical neighbor

            bool side1Walkable = nodes.TryGetValue(side1, out Node s1) && s1.IsWalkable;
            bool side2Walkable = nodes.TryGetValue(side2, out Node s2) && s2.IsWalkable;

            // Only allow diagonal if both sides are clear
            if (!side1Walkable || !side2Walkable) continue;

            Vector3Int neighborPos = node.Position + dir;
            if (nodes.TryGetValue(neighborPos, out Node neighbor))
                neighbors.Add(neighbor);
        }

        return neighbors;
    }

    public List<Vector3Int> FindPath(Vector3Int startPos, Vector3Int targetPos)
    {
        if (!nodes.TryGetValue(startPos, out Node startNode) ||
            !nodes.TryGetValue(targetPos, out Node targetNode))
            return null;

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
                if (!neighbor.IsWalkable || closedSet.Contains(neighbor))
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

        return null; // No path found
    }

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
        path.Reverse();
        Debug.Log($"Path found: {path}");
        return path;
    }

    private float GetMovementCost(Node a, Node b)
    {
        bool isDiagonal = a.Position.x != b.Position.x &&
                          a.Position.y != b.Position.y;
        return isDiagonal ? 1.414f : 1f;
    }

    private float GetHeuristic(Node a, Node b)
    {
        // Chebyshev distance (ideal for 8-directional grid movement)
        int dx = Mathf.Abs(a.Position.x - b.Position.x);
        int dy = Mathf.Abs(a.Position.y - b.Position.y);
        return Mathf.Max(dx, dy);
    }
}