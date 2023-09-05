using NesScripts.Controls.PathFind;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy1 : Enemy
{
    bool isPlayerInSight;

    public void Update()
    {
        if (this.health > 0)
            CallNextMove();
    }

    private void CallNextMove()
    {
        if (actionPoints > 0 && !isTurnFinished)
            this.waitingAction = new TurnAction(MakeMove, 100, null);
    }

    public void MakeMove()
    {
        int floorLayer = 1 << 7;
        //Get mob tile pos
        int px = (int)Math.Round(transform.position.x / MapGenerator.GRID_SIZE) + Math.Abs(mapGenerator.minX);
        int py = (int)Math.Round(transform.position.y / MapGenerator.GRID_SIZE) + Math.Abs(mapGenerator.minY);

        RaycastHit hit;
        Vector3 dir = mapGenerator.mainPlayerGameobject.transform.position - transform.position;
        dir.Normalize();
        bool hasTarget = false;
        int tx, ty;
        List<Point> tpath;
        if (Physics.Raycast(transform.position, dir, out hit, this.vision * MapGenerator.GRID_SIZE, ~floorLayer))
        {
            if (hit.transform.tag == "Player")
            {
                hasTarget = true;
                hasTargetText.text = "!";
                //Get player tile pos
                tx = (int)Math.Round(mapGenerator.mainPlayerGameobject.transform.position.x / MapGenerator.GRID_SIZE) + Math.Abs(mapGenerator.minX);
                ty = (int)Math.Round(mapGenerator.mainPlayerGameobject.transform.position.y / MapGenerator.GRID_SIZE) + Math.Abs(mapGenerator.minY);
                //Get path to target tile
                tpath = Pathfinding.FindPath(this.mapGenerator.pathFindGrid,
                    new Point(px, py), new Point(tx, ty));
                //If not in range
                if (tpath.Count > range) {
                    StartCoroutine(MoveToPosition(new Vector3((tpath[0].x + mapGenerator.minX) * MapGenerator.GRID_SIZE,
                        (tpath[0].y + mapGenerator.minY) * MapGenerator.GRID_SIZE, 0), 0.2f));
                }
                else
                {
                    this.audioSource.clip = this.attackSound;
                    this.audioSource.Play();
                    if (UnityEngine.Random.Range(1, tpath.Count + 1) == 1)
                        mapGenerator.mainPlayerGameobject.GetComponent<Player>().TakeDamage(UnityEngine.Random.Range(minDamage, maxDamage + 1));
                    else
                        textEventGen.AddTextEvent(this.entityName + " rate.", EventTextType.Combat);
                }
            }
        }
        if (!hasTarget)
        {
            hasTargetText.text = "";
            //Roam around
            int randx = UnityEngine.Random.Range(-1, 2);
            int randy = UnityEngine.Random.Range(-1, 2);
            tx = px + randx;
            ty = py + randy;
            tpath = Pathfinding.FindPath(this.mapGenerator.pathFindGrid,
                new Point(px, py), new Point(tx, ty));
            if (tpath.Count > 0)
                StartCoroutine(MoveToPosition(new Vector3((tpath[0].x + mapGenerator.minX) * MapGenerator.GRID_SIZE,
                    (tpath[0].y + mapGenerator.minY) * MapGenerator.GRID_SIZE, 0), 0.2f));
        }
    }

    public IEnumerator MoveToPosition(Vector3 end, float timeToGo)
    {
        var startRotation = transform.position;
        var t = 0f;
        while (t <= 1f)
        {
            t += Time.deltaTime / timeToGo;
            transform.position = Vector3.Lerp(startRotation, end, t);
            yield return null;
        }
        transform.position = end;
        RefreshCover(mapGenerator.mainPlayerGameobject.transform.position);
    }
}
