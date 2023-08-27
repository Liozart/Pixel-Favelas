using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerState
{
    public int health;
    public List<Item> inventory;
}

public class TurnManager : MonoBehaviour
{
    public GameObject mapManager;
    MapGenerator mapGenerator;

    int currentLevel = 0;
    int currentTurn = 0;

    PlayerState playerState;

    // Start is called before the first frame update
    void Start()
    {
        mapGenerator = mapManager.GetComponent<MapGenerator>();
        mapGenerator.GenerateTestLevel();

        playerState = new PlayerState();
    }
    
    //Called when the player got the key and is on the trapdoor
    public void GoToNextLevel()
    {
        currentLevel++;
        mapGenerator.ClearCurrentMap();
        mapGenerator.GenerateLevel(currentLevel);
    }

    //Save player state between levels
    public void SavePlayerState(Player player)
    {
        playerState.health = player.health;
        playerState.inventory = player.inventory;
    }

    //Resolve actions list of all the entities
    public void Resolve()
    {
        //Sort actors by speed
        mapGenerator.currentActors = mapGenerator.currentActors.OrderBy(e => e.initiative).ToList();

        //Foreach actor on the map, resolve actions
        foreach (Actor g in mapGenerator.currentActors)
            g.ResolveTurn();

        //Restore actors AP
        foreach (Actor g in mapGenerator.currentActors)
        {
            g.actionPoints += g.maxActionPoints;
            if (g.actionPoints > g.maxActionPoints)
                g.actionPoints = g.maxActionPoints;
        }

        currentTurn++;
    }
}
