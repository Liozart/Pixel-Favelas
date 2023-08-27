using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemTypes
{
    Key, Gun, Armor, Grenade, Aid, Gadget
}

public class Item : Entity
{
    public ItemTypes itemType;
    public AudioSource audioSource;
    public AudioClip lootedClip;
    public Player owner;
    public int APCost;

    public void Awake()
    {
        textEventGen = GameObject.Find("Text Generator").GetComponent<TextEventGeneration>();
        this.entityType = EntityType.Item;
        this.selectionColor = Color.yellow;
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    public void Loot(Player e)
    {
        owner = e;
        if (e.minActionCost > APCost)
            e.minActionCost = APCost;
        audioSource.clip = lootedClip;
        audioSource.Play();
    }

    public void Drop()
    {
        //TODO : CHANGER OWNER MINACTIONCOST
        owner = null;
    }
}
