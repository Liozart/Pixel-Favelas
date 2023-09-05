using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum ItemTypes
{
    Melee, Gun, Armor, Grenade, Aid, Gadget, Key
}

public class Item : Entity
{
    [HideInInspector]
    public ItemTypes itemType;
    [HideInInspector]
    public AudioSource audioSource;
    public AudioClip lootedClip;
    public AudioClip droppedClip;
    public AudioClip useSound;
    public Actor owner;
    public int APCost;

    public void Awake()
    {
        base.EntityStart();
        this.entityType = EntityType.Item;
        audioSource = gameObject.AddComponent<AudioSource>();
        discoverState = DiscoverState.Unknown;
        SetDiscoverState(discoverState, Vector3.zero);
    }

    public void Loot(Actor e)
    {
        transform.parent = e.transform;
        owner = e;
        audioSource.clip = lootedClip;
        audioSource.Play();
        GetComponent<SpriteRenderer>().enabled = false;
        GetComponent<Collider>().enabled = false;
        if (e.actorType == ActorType.Player)
        {
            ((Player)owner).RefreshMinActionCost();
            textEventGen.AddTextEvent(this.entityName + " looté. ", EventTextType.Loot);
        }
    }

    public void Drop()
    {
        transform.parent = null;
        audioSource.clip = droppedClip;
        audioSource.Play();
        GetComponent<SpriteRenderer>().enabled = true;
        GetComponent<Collider>().enabled = true;
        transform.position = owner.transform.position;
        if (owner.actorType == ActorType.Player)
        {
            ((Player)owner).RefreshMinActionCost();
            //Drop next to the player
            Vector3 pos = ((Player)owner).mapGenerator.GetFreeTileAround(transform.position);
            if (pos == transform.position)
                textEventGen.AddTextEvent("Pas de place pour lâcher ça.", EventTextType.Normal);
            else
                transform.position = pos;
        }
        owner = null;
    }
}
