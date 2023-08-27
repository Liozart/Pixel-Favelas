using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum EnemyTypes
{
    Enemy1, Enemy2, Enemy3, Enemy4
}

public class Enemy : Actor
{
    public int minDamage;
    public int maxDamage;
    public int range;
    public int vision;
    public EnemyTypes enemyType;

    public void Start()
    {
        this.actorType = ActorType.Enemy;
    }

    public void Say()
    {
        this.textEventGen.AddTextEvent("Isso é chato", EventTextType.Combat);
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            this.audioSource.clip = this.deathSound;
            this.audioSource.Play();
            mapGenerator.mainPlayerGameobject.GetComponent<Player>().selectedObject = null;
            GetComponent<SpriteRenderer>().enabled = false;
            mapGenerator.currentActors.Remove(this);
            StartCoroutine(DestroySelf());
        }
    }

    IEnumerator DestroySelf()
    {
        yield return new WaitForSeconds(1);
        Destroy(gameObject);
    }
}
