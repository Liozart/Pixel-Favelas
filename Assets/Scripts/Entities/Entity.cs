using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using UnityEngine;

public enum EntityType
{
    Block, Actor, Item
}

public enum DiscoverState
{
    Unknown, Discovered, InView
}

public class Entity : MonoBehaviour
{
    [HideInInspector]
    public int posX;
    [HideInInspector]
    public int posY;
    public string entityName;
    public string entityDesc;
    public GameObject selectionCanvasPrefab;
    [HideInInspector]
    public Canvas selectionCanvas;
    [HideInInspector]
    public EntityType entityType;
    [HideInInspector]
    public DiscoverState discoverState;

    [HideInInspector]
    public TextEventGeneration textEventGen;

    public void EntityStart()
    {
        textEventGen = GameObject.Find("Text Generator").GetComponent<TextEventGeneration>();
        selectionCanvas = Instantiate(selectionCanvasPrefab, this.gameObject.transform).GetComponent<Canvas>();
        selectionCanvas.enabled = false;
    }

    public void Select()
    {
        selectionCanvas.enabled = true;
        if (discoverState != DiscoverState.Unknown)
            textEventGen.AddTextEvent(entityDesc, EventTextType.Normal);
    }

    public void UnSelect()
    {
        if (entityType == EntityType.Block)
            SetDiscoverState(discoverState, Vector3.zero);
        selectionCanvas.enabled = false;
    }

    public void SetDiscoverState(DiscoverState s, Vector3 futurePlayerPos)
    {
        discoverState = s;
        switch (discoverState)
        {
            case DiscoverState.Unknown:
                GetComponent<SpriteRenderer>().color = Color.black; break;
            case DiscoverState.Discovered:
                GetComponent<SpriteRenderer>().color = Color.grey; break;
            case DiscoverState.InView:
                GetComponent<SpriteRenderer>().color = Color.white; break;
        }
        if (entityType == EntityType.Actor)
        {
            if (!TryGetComponent(typeof(Player), out _))
            {
                if (discoverState == DiscoverState.InView)
                {
                    GetComponentInChildren<Canvas>().enabled = true;
                    GetComponent<SpriteRenderer>().enabled = true;
                    GetComponent<Enemy>().RefreshCover(futurePlayerPos);
                }
                else
                {
                    GetComponentInChildren<Canvas>().enabled = false;
                    GetComponent<SpriteRenderer>().enabled = false;
                }
            }
        }
    }
}
