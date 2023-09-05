using System.Linq;
using UnityEngine;

public class Gun : Item
{
    public int optiRange;
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
        if (currentAmmo == 0)
        {
            Reload();
            return;
        }
        int layerMask = 1 << 6;
        currentAmmo--;
        this.audioSource.clip = ShootSound[Random.Range(0, ShootSound.Count())];
        this.audioSource.Play();
        RaycastHit hit;
        Vector3 dir = ((Player)this.owner).selectedObject.transform.position - this.owner.transform.position;
        dir.Normalize();
        if (Physics.Raycast(this.owner.transform.position, dir, out hit, MaxRange * MapGenerator.GRID_SIZE, layerMask))
            switch (hit.transform.tag)
            {
                case "Enemy":
                    hit.transform.GetComponent<Enemy>().TakeDamage(Random.Range(MinDamage, MaxDamage + 1));
                    textEventGen.AddTextEvent("Feu avec " + entityName + ".", EventTextType.Combat);
                    break;
            }
    }

    public void Reload()
    {
        this.audioSource.clip = reloadSound;
        this.audioSource.Play();
        textEventGen.AddTextEvent("Rechargement", EventTextType.Combat);
        currentAmmo = ammoCapacity;
    }
}
