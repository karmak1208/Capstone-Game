using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileData
{
    public Vector3Int Pos;
    public TileBase baseTile;
    public Tilemap Map;
    public bool Hidden;

    public TileData(Vector3Int pos, TileBase tile, Tilemap map)
    {
        Pos = pos;
        baseTile = tile;
        Map = map;
    }

    public void HideTile()
    {
        Map.SetTile(Pos, null);
        Hidden = true;
    }

    public void ShowTile()
    {
        Map.SetTile(Pos, baseTile);
        Hidden = false;
    }
}

public class MapVisibility : MonoBehaviour
{
    private Dictionary<string, Dictionary<Vector3Int, TileData>> tiles = new();

    private bool characterMoving;

    [SerializeField] private int visibilityRadius;

    void Start()
    {
        Tilemap[] maps = GetComponentsInChildren<Tilemap>();
        foreach (Tilemap map in maps)
        {
            Dictionary<Vector3Int, TileData> dict = new();
            BoundsInt bounds = map.cellBounds;
            foreach (Vector3Int pos in bounds.allPositionsWithin)
            {
                var tile = map.GetTile(pos);
                if (tile != null)
                    dict[pos] = new TileData(pos, tile, map);
            }
            tiles[map.name] = dict;
        }
        HideAllTiles();
        StartCoroutine(UpdateMapVisibility());
        CharacterManager.Instance.OnCharacterStartedMove.AddListener(() => characterMoving = true);
        CharacterManager.Instance.OnCharacterEndedMove.AddListener(() => characterMoving = false);
    }



    IEnumerator UpdateMapVisibility()
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        float budgetMs = 4f;

        while (true)
        {
            foreach (var (_, dict) in tiles)
            {
                foreach (var (pos, tile) in dict.Where(t => t.Value.Hidden).ToList())
                {
                    if (IsTileVisible(pos))
                    {
                        tile.ShowTile();
                    }

                    if (stopwatch.Elapsed.TotalMilliseconds >= budgetMs)
                    {
                        yield return null;
                        stopwatch.Restart();
                    }
                }
            }
        }
    }

    bool IsTileVisible(Vector3Int pos)
    {
        LightManager lm = LightManager.Instance;
        bool inLight = lm.IsObjectInLight(pos, "Enemy");
        bool inLOS = lm.IsInLOSOfCharacter(pos);
        return inLight && inLOS;
    }

    void HideAllTiles()
    {
        foreach (var outerKvp in tiles)
        {
            Dictionary<Vector3Int, TileData> innerDict = outerKvp.Value;

            foreach (var (pos, tile) in innerDict)
            {
                tile.HideTile();
            }
        }
    }
}
