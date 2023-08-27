using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ActorType
{
    Player, Enemy
}

public class Actor : Entity
{
    public int health;
    public int maxHealth;
    public int initiative;
    public int moveSpeed;
    public int actionPoints;
    public int maxActionPoints;
    public List<TurnAction> waitingActionsList = new List<TurnAction>();
    public ActorType actorType;

    public TurnManager turnManager;
    public MapGenerator mapGenerator;
    public AudioSource audioSource;

    public AudioClip attackSound;
    public AudioClip deathSound;

    public void Awake()
    {
        textEventGen = GameObject.Find("Text Generator").GetComponent<TextEventGeneration>();
        this.entityType = EntityType.Actor;
        mapGenerator = GameObject.Find("Map Manager").GetComponent<MapGenerator>();
        turnManager = GameObject.Find("Turn Manager").GetComponent<TurnManager>();
        audioSource = GetComponent<AudioSource>();
        this.selectionColor = Color.red;
    }

    //Do entity action
    public void ResolveTurn()
    {
        List<TurnAction> tmp = waitingActionsList;
        for (int i = 0; i < tmp.Count; i++)
        {
            if ((actionPoints - tmp[i].AP) >= 0)
            {
                actionPoints -= tmp[i].AP;
                tmp[i].action();
                waitingActionsList.RemoveAt(i);
            }
            else break;
        }
    }
}

public class TurnAction
{
    public Action action;
    public int AP;

    public TurnAction(Action action, int aP)
    {
        this.action = action;
        this.AP = aP;
    }
}