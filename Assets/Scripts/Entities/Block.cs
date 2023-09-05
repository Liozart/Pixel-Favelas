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
        base.EntityStart();
        this.entityType = EntityType.Block;
        isDestroyed = false;
        discoverState = DiscoverState.Unknown;
        SetDiscoverState(discoverState, Vector3.zero);
    }
}
