using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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
    public int minActionCost;
    [HideInInspector]
    public bool isTurnFinished;
    [HideInInspector]
    public TurnAction waitingAction;
    [HideInInspector]
    public ActorType actorType;

    [HideInInspector]
    public TurnManager turnManager;
    [HideInInspector]
    public MapGenerator mapGenerator;
    [HideInInspector]
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
        isTurnFinished = false;
    }

    //Do entity action
    public void ResolveTurn()
    {
        if (isTurnFinished) return;

        if ((actionPoints - waitingAction.AP) >= 0)
        {
            actionPoints -= waitingAction.AP;
            waitingAction.action();
            waitingAction = null;
        }
        else isTurnFinished = true;

        if (minActionCost > actionPoints)
            isTurnFinished = true;
        else return;

        turnManager.Resolve();
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