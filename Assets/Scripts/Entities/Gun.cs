using System.Linq;
using UnityEngine;

public class Gun : Item
{
    public int MaxRange;
    public int MinDamage;
    public int MaxDamage;
    public int ammoCapacity;
    public int currentAmmo;

    public AudioClip[] ShootSound;
    public AudioClip reloadSound;

    // Start is called before the first frame update
    void Start()
    {
        this.itemType = ItemTypes.Gun;
    }

    public void Shoot()
    {
        int layerMask = 1 << 6;

        this.audioSource.clip = ShootSound[Random.Range(0, ShootSound.Count())];
        this.audioSource.Play();
        RaycastHit hit;
        Vector3 dir = ((Player)this.owner).selectedObject.transform.position - this.owner.transform.position;
        dir.Normalize();
        Debug.DrawRay(this.owner.transform.position, dir * MaxRange * MapGenerator.GRID_SIZE, Color.red, 1000);
        if (Physics.Raycast(this.owner.transform.position, dir, out hit, MaxRange * MapGenerator.GRID_SIZE, layerMask))
            switch (hit.transform.tag)
            {
                case "Enemy":
                    hit.transform.GetComponent<Enemy>().TakeDamage(MaxDamage);
                    textEventGen.AddTextEvent("Feu avec " + entityName + ".", EventTextType.Combat);
                    break;
            }
    }
}
