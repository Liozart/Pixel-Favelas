using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using UnityEngine;

public enum EntityType
{
    Block, Actor, Item
}

public class Entity : MonoBehaviour
{
    public int posX; 
    public int posY;
    public string entityName;
    public string entityDesc;
    public EntityType entityType;

    public Color selectionColor;
    public TextEventGeneration textEventGen;

    public void Select()
    {
        GetComponent<SpriteRenderer>().color = selectionColor;
        textEventGen.AddTextEvent(entityDesc, EventTextType.Normal);
    }

    public void UnSelect()
    {
        GetComponent<SpriteRenderer>().color = Color.white;
    }
}
