using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelCreator : MonoBehaviour
{
    public Grid grid;
    public Tilemap tilemap;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void CreateLevel()
    {
        List<Tile> tiles = new List<Tile>();
        for (int i = -1000; i < 1000; i++)
        {
            for (int j = -1000; j < 1000; j++)
            {
                if (tilemap.GetSprite(new Vector3Int(i, j, 0)) != null)
                {
                    Tile t = new Tile(i, j);
                    switch (tilemap.GetSprite(new Vector3Int(i, j, 0)).name)
                    {
                        case "asphalt": t.tiletype = MapTileTypes.Floor; break;
                        case "brick": t.tiletype = MapTileTypes.Wall; break;
                    }
                    tiles.Add(t);
                }
            }
        }
        GeneratedMapJSONContent res = new GeneratedMapJSONContent();
        res.Blocks = tiles;
        string rjson = JsonUtility.ToJson(res); 
        string path = "Assets\\Favelas\\gen.json";
        using (StreamWriter r = new StreamWriter(path))
        {
            r.Write(rjson);
            Debug.Log("OK");
        }
    }
}
