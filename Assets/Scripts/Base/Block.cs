using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BlockType
{
    Floor, Wall, Door, Trapdoor, Table
}

public class Block : Entity
{
    public bool isDestroyed;
    public BlockType blockType;

    public void Awake()
    {
        this.entityType = EntityType.Block;
        textEventGen = GameObject.Find("Text Generator").GetComponent<TextEventGeneration>();
        isDestroyed = false;
        this.selectionColor = Color.green;
    }
}
