using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

//Types of map tiles
public enum MapTileTypes
{
    Floor, Wall, Door, Trapdoor, Table
}

public class MapGenerator : MonoBehaviour
{
    //Maps utils
    public static float GRID_SIZE = 0.32f;
    public int currentLevel = 0;

    //Floor prefab
    public GameObject PrefabFloor;
    public GameObject PrefabTrapdoor_close;
    public Sprite PrefabTrapdoor_open;
    //Walls prefab
    public GameObject PrefabWall;
    public GameObject PrefabDoor;
    //Entities prefab
    public GameObject PrefabPlayer;
    public GameObject PrefabEnemy_1;
    //Items prefabs
    public GameObject PrefabFists;
    public GameObject PrefabMakarov;
    public GameObject PrefabShiv;

    //JSON data of the map
    GeneratedMapJSONContent JSONMap;
    GameObject currentMap;
    //Lists of generated gameobjects
    List<GameObject> gameobjectTilesFloors = new List<GameObject>();
    List<GameObject> gameobjectTilesWalls = new List<GameObject>();

    //List of the spawned entities
    public GameObject mainPlayerGameobject = null;
    public List<Actor> currentActors = new List<Actor>();
    public List<Item> currentItems = new List<Item>();

    //Pathfinding grid and vars
    public bool[,] tilesmap;
    public int maxX = 0, maxY = 0, minX = 0, minY = 0;
    public NesScripts.Controls.PathFind.Grid pathFindGrid;

    public void GenerateTestLevel()
    {
        currentMap = new GameObject("Map");
        //Read JSON file
        string path = "Assets\\Favelas\\gen.json";
        using (StreamReader r = new StreamReader(path))
        {
            string json = r.ReadToEnd();
            JSONMap = JsonUtility.FromJson<GeneratedMapJSONContent>(json);
        }
        //Add the tiles
        foreach (Tile tile in JSONMap.Blocks)
        {
            AddTile((tile.x), (tile.y), tile.tiletype);
        }

        //Check offsets
        maxX = JSONMap.Blocks.OrderByDescending(t => t.x).First().x;
        maxY = JSONMap.Blocks.OrderByDescending(t => t.y).First().y;
        minX = JSONMap.Blocks.OrderBy(t => t.x).First().x;
        minY = JSONMap.Blocks.OrderBy(t => t.y).First().y;

        #region AITileMapInit
        //Create tilemap with size
        tilesmap = new bool[(Math.Abs(minX) + Math.Abs(maxX) + 1), (Math.Abs(minY) + Math.Abs(maxY) + 1)];
        //Assign values
        foreach (Tile t in JSONMap.Blocks.FindAll(t => t.tiletype == MapTileTypes.Floor).ToList())
            tilesmap[(t.x + Math.Abs(minX)), (t.y + Math.Abs(minY))] = true;
        foreach (Tile t in JSONMap.Blocks.FindAll(t => t.tiletype == MapTileTypes.Wall).ToList())
            tilesmap[(t.x + Math.Abs(minX)), (t.y + Math.Abs(minY))] = false;
        //Create grid
        pathFindGrid = new NesScripts.Controls.PathFind.Grid(tilesmap);
        #endregion

        GameObject pos = gameobjectTilesFloors[UnityEngine.Random.Range(0, gameobjectTilesFloors.Count)];
        while (GetEntitiesAt(pos.transform.position).Count > 1)
            pos = gameobjectTilesFloors[UnityEngine.Random.Range(0, gameobjectTilesFloors.Count)];

        mainPlayerGameobject = Instantiate(PrefabPlayer, pos.transform.position, Quaternion.identity, currentMap.transform);
        currentActors.Add(mainPlayerGameobject.GetComponent<Actor>());

        AddRandomOnFloor(PrefabEnemy_1, EntityType.Actor);
        AddRandomOnFloor(PrefabEnemy_1, EntityType.Actor);
        AddRandomOnFloor(PrefabEnemy_1, EntityType.Actor);
        AddRandomOnFloor(PrefabShiv, EntityType.Item);
        AddRandomOnFloor(PrefabMakarov, EntityType.Item);
    }

    public void AddTile(int x, int y, MapTileTypes tiletype)
    {
        switch (tiletype)
        {
            case MapTileTypes.Wall:
                gameobjectTilesWalls.Add(Instantiate(PrefabWall, new Vector3(x * GRID_SIZE, y * GRID_SIZE, 0),
                   Quaternion.identity, currentMap.transform)); 
                break;
            case MapTileTypes.Floor:
                gameobjectTilesFloors.Add(Instantiate(PrefabFloor, new Vector3(x * GRID_SIZE, y * GRID_SIZE, 0),
                    Quaternion.identity, currentMap.transform)); ;
                break;
        }
    }

    public void AddRandomOnFloor(GameObject gameobj, EntityType typ)
    {
        GameObject pos = gameobjectTilesFloors[UnityEngine.Random.Range(0, gameobjectTilesFloors.Count)];
        while (GetEntitiesAt(pos.transform.position).Count > 1)
            pos = gameobjectTilesFloors[UnityEngine.Random.Range(0, gameobjectTilesFloors.Count)];

        GameObject g = Instantiate(gameobj, pos.transform.position, Quaternion.identity, currentMap.transform);
        switch (typ)
        {
            case EntityType.Actor:
                currentActors.Add(g.GetComponent<Actor>());
                break;
            case EntityType.Item:
                currentItems.Add(g.GetComponent<Item>());
                break;
        }    
    }

    public List<Entity> GetEntitiesAt(Vector3 where)
    {
        List<Entity> res = new List<Entity>();
        where.z = 1;
        Ray r = new Ray(where, Vector3.back);
        RaycastHit[] hit;
        hit = Physics.RaycastAll(r, 2);
        foreach (RaycastHit h in hit)
            res.Add(h.transform.GetComponent<Entity>());
        return res;
    }

    public Vector3 GetFreeTileAround(Vector3 pos)
    {
        List<Entity> tiles;
        int tries = 0, max = 0;
        while (tries < (Math.Abs(maxX) - (Math.Abs(minX)) + (Math.Abs(maxY) - Math.Abs(minY))))
        {
            max++;

            for(float i = -(GRID_SIZE * max); i <= (GRID_SIZE * max); i += GRID_SIZE)
            {
                for (float j = -(GRID_SIZE * max); j <= (GRID_SIZE * max); j += GRID_SIZE)
                {
                    if (i == 0 && j == 0) continue;
                    tiles = GetEntitiesAt(new Vector3(pos.x + i, pos.y + j, pos.z));
                    if (tiles.Count != 1) continue;
                    if (((Block)tiles[0]).blockType != BlockType.Floor) continue;
                    return new Vector3(pos.x + i, pos.y + j, pos.z);
                }
            }
        }
        return pos;
    }

    //Add walls all around the generated tiles----------------------------------
   /* void AddTilesWallsToMap(List<Tile> target)
    {
        //Check the sides of all the floor tiles and put a wall where there isn't anything
        int cnt = target.Count;
        for (int i = 0; i < cnt; i++)
        {
            Tile tile = target[i];
            bool W = false, E = false, N = false, S = false;
            foreach (Tile neib in JSONMap.Blocks.FindAll(t => t.tiletype == MapTileTypes.Floor).ToList())
            {
                //Check every directions
                if ((tile.x + 1) == neib.x && tile.y == neib.y)
                    W = true;
                if ((tile.x - 1) == neib.x && tile.y == neib.y)
                    E = true;
                if ((tile.y + 1) == neib.y && tile.x == neib.x)
                    N = true;
                if ((tile.y - 1) == neib.y && tile.x == neib.x)
                    S = true;
            }
            foreach (Tile neib in JSONMap.Blocks.FindAll(t => t.tiletype == MapTileTypes.Wall).ToList())
            {
                //Check every directions
                if ((tile.x + 1) == neib.x && tile.y == neib.y)
                    W = true;
                if ((tile.x - 1) == neib.x && tile.y == neib.y)
                    E = true;
                if ((tile.y + 1) == neib.y && tile.x == neib.x)
                    N = true;
                if ((tile.y - 1) == neib.y && tile.x == neib.x)
                    S = true;
            }
            if (!W)
            {
                JSONMap.Blocks.Add(new Tile((tile.x + 1), tile.y, MapTileTypes.Wall));
                AddTileWall((tile.x + 1), tile.y, tile.tiletype);
            }
            if (!E)
            {
                JSONMap.Blocks.Add(new Tile((tile.x - 1), tile.y, MapTileTypes.Wall));
                AddTileWall((tile.x - 1), tile.y, tile.tiletype);
            }
            if (!N)
            {
                JSONMap.Blocks.Add(new Tile(tile.x, (tile.y + 1), MapTileTypes.Wall));
                AddTileWall(tile.x, (tile.y + 1), tile.tiletype);
            }
            if (!S)
            {
                JSONMap.Blocks.Add(new Tile(tile.x, (tile.y - 1), MapTileTypes.Wall));
                AddTileWall(tile.x, (tile.y - 1), tile.tiletype);
            }
        }
    }
    void AddTileDoor(int x, int y, MapTileTypes t)
    {
        gameobjectTilesDoors.Add(Instantiate(PrefabDoor, new Vector3(x * GRID_SIZE, y * GRID_SIZE, 0),
            Quaternion.identity, currentMap.transform));
    }*/
    /*void AddTileTrapDoor()
    {
        int trgt = (int)(UnityEngine.Random.value * JSONMap.floors.Count);
        gameobjectTileTrapdoor = Instantiate(PrefabTrapdoor_close, new Vector3(JSONMap.floors[trgt].x * GRID_SIZE, JSONMap.floors[trgt].y * GRID_SIZE, 0),
            Quaternion.identity, currentMap.transform);
    }
    void AddTileKey()
    {
        int ke = (int)((UnityEngine.Random.value * JSONMap.floors.Count) / 2) + (JSONMap.floors.Count / 2);
        GameObject lekey = Instantiate(PrefabItem_key, new Vector3(JSONMap.floors[ke].x * GRID_SIZE, JSONMap.floors[ke].y * GRID_SIZE, 0),
            Quaternion.identity, currentMap.transform);
        currentItems.Add(lekey.GetComponent<Item>());
    }*/

    //Set the trapdoor sprite open
    public void SetTrapDoorOpen()
    {
        //gameobjectTileTrapdoor.GetComponent<SpriteRenderer>().sprite = PrefabTrapdoor_open.GetComponent<SpriteRenderer>().sprite;
    }

    //Clear all the variables used to generate the map
    public void ClearCurrentMap()
    {
        //Destroy all generated gameobjects
        Destroy(currentMap);
        //Clear others
        JSONMap = null;
        gameobjectTilesFloors = new List<GameObject>();
        gameobjectTilesWalls = new List<GameObject>();
        mainPlayerGameobject = null;
        currentActors = new List<Actor>();
        currentItems = new List<Item>();
    }
}

[System.Serializable]
public class GeneratedMapJSONContent
{   
    public List<Tile> Blocks;
}

[System.Serializable]
public class Tile
{
    public int x;
    public int y;
    public MapTileTypes tiletype;

    public Tile(int x, int y, MapTileTypes t)
    {
        this.x = x;
        this.y = y;
        this.tiletype = t;
    }

    public Tile(int x, int y)
    {
        this.x = x;
        this.y = y;
    }
}