using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : Item
{
    public int APCost;
    public int MaxRange;
    public int MinDamage;
    public int MaxDamage;

    public AudioClip ShootSound;

    // Start is called before the first frame update
    void Start()
    {
        this.itemType = ItemTypes.Gun;
    }

    public void Shoot()
    {
        int layerMask = 1 << 6;

        this.audioSource.clip = ShootSound;
        this.audioSource.Play();
        RaycastHit hit;
        Vector3 dir = this.owner.selectedObject.transform.position - this.owner.transform.position;
        dir.Normalize();
        Debug.DrawRay(this.owner.transform.position, dir * MaxRange, Color.red, 1000);
        if (Physics.Raycast(this.owner.transform.position, dir, out hit, MaxRange, layerMask))
            switch (hit.transform.tag)
            {
                case "Enemy":
                    hit.transform.GetComponent<Enemy>().TakeDamage(MaxDamage);
                    break;
            }
    }
}
