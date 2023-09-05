using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
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
    public EnemyTypes enemyType;
    public TMP_Text healthText;
    public TMP_Text hitChanceText;
    public TMP_Text hasTargetText;

    public void Start()
    {
        this.actorType = ActorType.Enemy;
        healthText.text = health.ToString();
        hitChanceText.text = 100 + "%";
        hasTargetText.text = "";
        discoverState = DiscoverState.Unknown;
        SetDiscoverState(discoverState, mapGenerator.mainPlayerGameobject.transform.position);
    }

    public void Say()
    {
        this.textEventGen.AddTextEvent("Isso é chato", EventTextType.Combat);
    }

    public void TakeDamage(int damage)
    {
        if (Random.Range(0, 100) > this.cover)
            this.textEventGen.AddTextEvent("Raté !", EventTextType.Combat);
        else
        {
            health -= damage;
            healthText.text = health.ToString();
            this.textEventGen.AddTextEvent(damage + " dégats infligés.", EventTextType.Combat);
            if (health <= 0)
            {
                this.audioSource.clip = this.deathSound;
                this.audioSource.Play();
                mapGenerator.mainPlayerGameobject.GetComponent<Player>().selectedObject = null;
                GetComponent<SpriteRenderer>().enabled = false;
                mapGenerator.currentActors.Remove(this);
                healthText.text = "";
                hitChanceText.text = "";
                hasTargetText.text = "";
                StartCoroutine(DestroySelf());
            }
        }
    }

    public void RefreshCover(Vector3 futurePlayerPos)
    {
        this.cover = 0;
        float r = Vector3.Distance(transform.position / MapGenerator.GRID_SIZE, futurePlayerPos / MapGenerator.GRID_SIZE);
        if (r > 1.5f && mapGenerator.mainPlayerGameobject.GetComponent<Player>().equipmentGun != null)
        {
            if (r <= mapGenerator.mainPlayerGameobject.GetComponent<Player>().equipmentGun.MaxRange)
            {
                float hitchance = 100 - (r * 100 / mapGenerator.mainPlayerGameobject.GetComponent<Player>().equipmentGun.MaxRange);
                hitChanceText.text = Mathf.RoundToInt(hitchance) + "%";
                this.cover = Mathf.RoundToInt(hitchance);
            }
            else
                hitChanceText.text = "0%";
        }
        else
        {
            if (r <= 1.5f)
            {
                hitChanceText.text = "100%";
                this.cover = 100;
            }
            else
                hitChanceText.text = "0%";
        }
        
    }

    IEnumerator DestroySelf()
    {
        yield return new WaitForSeconds(1);
        Destroy(gameObject);
    }
}
