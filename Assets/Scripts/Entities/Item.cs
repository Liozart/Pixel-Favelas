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
        textEventGen = GameObject.Find("Text Generator").GetComponent<TextEventGeneration>();
        this.entityType = EntityType.Item;
        this.selectionColor = Color.yellow;
        audioSource = gameObject.AddComponent<AudioSource>();
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
