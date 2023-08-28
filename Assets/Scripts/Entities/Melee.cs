using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;

public class Melee : Item
{
    public int MinDamage;
    public int MaxDamage;

    // Start is called before the first frame update
    void Start()
    {
        this.itemType = ItemTypes.Melee;
    }

    public void Paquis()
    {
        ((Player)this.owner).selectedObject.transform.GetComponent<Enemy>().TakeDamage(MaxDamage);
        this.audioSource.clip = this.useSound;
        this.audioSource.Play();
        textEventGen.AddTextEvent("Paquis avec " + entityName + ".", EventTextType.Combat);
    }
}
