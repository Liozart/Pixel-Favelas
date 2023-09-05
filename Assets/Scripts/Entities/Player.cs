using NesScripts.Controls.PathFind;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.Rendering.VirtualTexturing.Debugging;

public enum PlayerClass
{
    Base
}

public class Player : Actor
{
    //Player types and sprites
    public PlayerClass playerType;
    public Sprite playerSpriteBase;

    //Camera
    GameObject playerCamera;
    int zoomLevel = -4;

    //Audio
    public AudioClip audioClip_openDoor;
    public AudioClip audioClip_openTrapDoor;
    public AudioClip audioClip_getKey;
    public AudioClip audioClip_getLoot;
    public List<AudioClip> audioClip_walk;

    public Gun equipmentGun;
    public Melee equipmentMelee;
    public Item equipmentArmor;
    public Item equipmentGadget1;
    public Item equipmentGadget2;
    public int inventorySize = 10;
    public bool isInventoryOpen;

    //Current selected tile or object
    public GameObject selectedObject = null;
    int wallLayerMask = 1 << 6;
    //Auto-play vars
    public float delayBeforeAutoThresold = 0.5f;
    float delayBeforeAuto = 0;

    //UI
    InventoryManager inventoryManager;

    // Start is called before the first frame update
    void Start()
    {
        //Entity init
        this.entityName = "Josué";
        this.entityDesc = "It's you.";
        this.health = this.maxHealth = 20;
        this.initiative = 10;
        this.moveSpeed = 100;
        this.actionPoints = this.maxActionPoints = 100;
        this.minActionCost = moveSpeed;
        this.cover = 100;
        this.vision = 6;
        this.actorType = ActorType.Player;
        isInventoryOpen = false;
        switch (playerType)
        {
            case PlayerClass.Base:
                gameObject.GetComponent<SpriteRenderer>().sprite = playerSpriteBase;
                break;
        }
        playerCamera = GameObject.FindGameObjectWithTag("MainCamera");
        playerCamera.transform.position = new Vector3(transform.position.x, transform.position.y, zoomLevel);

        //UI
        inventoryManager = GameObject.FindGameObjectsWithTag("InvCanvas")[0].GetComponent<InventoryManager>();
        inventoryManager.SetPlayer(this);

        //Fists
        GameObject fist = Instantiate(mapGenerator.PrefabFists);
        inventoryManager.FistsInstance = fist.GetComponent<Fists>();
        LootItem(fist.GetComponent<Item>());
        LootItem(Instantiate(mapGenerator.PrefabMakarov).GetComponent<Item>());
        inventoryManager.RefreshUI();

        DiscoverAround(transform.position);
        RefreshCover(transform.position);
    }

    // Update is called once per frame
    void Update()
    {
        if (isInventoryOpen) return;

        //Get player selection
        if (Input.GetMouseButtonDown((int)MouseButton.LeftMouse))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Physics.Raycast(ray, out hit);
            bool isSame = false;
            if (hit.transform != null)
            {
                if (selectedObject != null)
                {
                    if (selectedObject.Equals(hit.collider.gameObject))
                        isSame = true;
                    selectedObject.GetComponent<Entity>().UnSelect();
                    selectedObject = null;
                    waitingAction = null;

                }
                if (!isSame)
                {
                    selectedObject = hit.collider.gameObject;
                    selectedObject.GetComponent<Entity>().Select();
                }
            }
        }
        //Player turn accept
        if (Input.GetMouseButtonDown((int)MouseButton.RightMouse))
        {
            //Add selected action
            if (selectedObject != null)
            {
                switch (selectedObject.tag)
                {
                    case "Floor":
                    case "Trapdoor":
                    case "Door":
                    case "Item":
                        if (selectedObject.transform.position == transform.position)
                            this.waitingAction = new TurnAction(Wait, actionPoints, null);
                        else
                        {
                            Actor n = CheckNextTile();
                            if (n != null)
                            {
                                textEventGen.AddTextEvent("Bloqué!", EventTextType.Skill);
                                return;
                            }
                            else
                            {
                                if (moveSpeed <= actionPoints)
                                    this.waitingAction = new TurnAction(Move, moveSpeed, null);
                                else
                                    textEventGen.AddTextEvent("Pas assez d'AP", EventTextType.Skill);
                            }
                        }
                        break;
                    case "Enemy":
                        Actor m = CheckNextTile();
                        if (m != null)
                        {
                            if (equipmentMelee.APCost <= actionPoints)
                                this.waitingAction = new TurnAction(equipmentMelee.Paquis, equipmentMelee.APCost, null);
                            else
                                textEventGen.AddTextEvent("Pas assez d'AP", EventTextType.Skill);
                        }
                        else
                        {
                            if (equipmentGun != null)
                            {
                                if (equipmentGun.APCost <= actionPoints)
                                    this.waitingAction = new TurnAction(equipmentGun.Shoot, equipmentGun.APCost, null);
                                else
                                    textEventGen.AddTextEvent("Pas assez d'AP", EventTextType.Skill);
                            }
                            else
                                textEventGen.AddTextEvent("Pas d'arme à distance", EventTextType.Skill);
                        }
                        break;
                    default:
                        if (equipmentGun != null)
                        {
                            if (equipmentGun.APCost <= actionPoints && equipmentGun.currentAmmo < equipmentGun.ammoCapacity)
                                this.waitingAction = new TurnAction(equipmentGun.Reload, equipmentGun.APCost, null);
                            else
                                this.waitingAction = new TurnAction(Wait, actionPoints, null);
                        }
                        else
                            this.waitingAction = new TurnAction(Wait, actionPoints, null);
                        break;
                }
            }
            else
            {
                //Wait
                if (equipmentGun != null)
                {
                    if (equipmentGun.APCost <= actionPoints && equipmentGun.currentAmmo < equipmentGun.ammoCapacity)
                        this.waitingAction = new TurnAction(equipmentGun.Reload, equipmentGun.APCost, null);
                    else
                        this.waitingAction = new TurnAction(Wait, actionPoints, null);
                }
                else
                    this.waitingAction = new TurnAction(Wait, actionPoints, null);
            }
            //Finish the turn
            if (waitingAction != null)
                this.turnManager.Resolve();
            inventoryManager.RefreshUI();
        }

        //Auto turn
        /*if (Input.GetMouseButton((int)MouseButton.RightMouse))
            if (delayBeforeAuto < delayBeforeAutoThresold)
                delayBeforeAuto += Time.deltaTime;
            else
            {
                if (selectedObject != null) switch (selectedObject.tag)
                    {
                        case "Floor":
                        case "Trapdoor":
                        case "Door":
                        case "Item":
                            if (selectedObject.transform.position == transform.position)
                                this.waitingAction = new TurnAction(Wait, actionPoints);
                            else
                            {
                                if (moveSpeed <= actionPoints)
                                    this.waitingAction = new TurnAction(Move, moveSpeed);
                                else
                                    textEventGen.AddTextEvent("Pas assez d'AP", EventTextType.Skill);
                            }
                            break;
                        case "Enemy":
                            if (((Gun)inventory[0]).APCost <= actionPoints)
                                this.waitingAction = new TurnAction(((Gun)inventory[0]).Shoot, ((Gun)inventory[0]).APCost);
                            else
                                textEventGen.AddTextEvent("Pas assez d'AP", EventTextType.Skill);
                            break;
                    }
                //Finish the turn
                this.turnManager.Resolve();
                RefreshUI();
                delayBeforeAuto = 0;
            }
        else
            delayBeforeAuto = 0;*/

        //Zoom
        if (Input.mouseScrollDelta.y > 0)
        {
            zoomLevel--; 
            playerCamera.transform.position = new Vector3(playerCamera.transform.position.x, playerCamera.transform.position.y, playerCamera.transform.position.z - 1);
        }
        else if (Input.mouseScrollDelta.y < 0)
        {
            zoomLevel++; 
            playerCamera.transform.position = new Vector3(playerCamera.transform.position.x, playerCamera.transform.position.y, playerCamera.transform.position.z + 1);
        }
    }

    public void RefreshMinActionCost()
    {
        minActionCost = 100;
        if (equipmentGun != null)
            if (equipmentGun.APCost < minActionCost)
                minActionCost = equipmentGun.APCost;
        if (equipmentMelee != null)
            if (equipmentMelee.APCost < minActionCost)
                minActionCost = equipmentMelee.APCost;
        if (equipmentGadget1 != null)
            if (equipmentGadget1.APCost < minActionCost)
                minActionCost = equipmentGadget1.APCost;
        if (equipmentGadget2 != null)
            if (equipmentGadget2.APCost < minActionCost)
                minActionCost = equipmentGadget2.APCost;
        if (inventory.Count != 0)
        {
            int min = inventory.Min(i => i.APCost);
            if (min < minActionCost)
                minActionCost = min;
        }
    }

    public void RefreshCover(Vector3 pos)
    {

    }

    internal void TakeDamage(int dam)
    {
        if (UnityEngine.Random.Range(0, 100) > this.cover)
            this.textEventGen.AddTextEvent("Raté !", EventTextType.Combat);
        else
        {
            health -= dam;
            this.textEventGen.AddTextEvent(dam + " dégats subits.", EventTextType.Combat);
            if (health <= 0)
            {
                this.audioSource.clip = this.deathSound;
                this.audioSource.Play();
                this.textEventGen.AddTextEvent("DEAD", EventTextType.Combat);
            }
        }
    }

    List<Entity> lastDiscoverEntites = new List<Entity>();
    public void DiscoverAround(Vector3 pos)
    {
        int itemLayer = 1 << 6 | 1 << 8;
        //Grey old viewed tiles
        foreach (Entity c in lastDiscoverEntites)
            try
            {
                c.SetDiscoverState(DiscoverState.Discovered, pos);
            }
            //handle destroyed ennemies
            catch (MissingReferenceException) { }

        lastDiscoverEntites.Clear();

        //View player tile
        Collider[] pl = Physics.OverlapSphere(pos, 0.05f);
        foreach (Collider collider in pl)
            collider.GetComponent<Entity>().SetDiscoverState(DiscoverState.InView, pos);

        //Get entites in vision
        List<RaycastHit[]> hits = new List<RaycastHit[]>();
        for (float i = -1; i <= 1; i += 0.25f)
            for (float j = -1; j <= 1; j += 0.25f)
            {
                hits.Add(Physics.RaycastAll(pos, new Vector3(i, j, 0), vision * MapGenerator.GRID_SIZE));
                hits.Add(Physics.RaycastAll(pos, new Vector3(i, j, 0), vision * MapGenerator.GRID_SIZE, itemLayer));
            }

        bool walld;
        foreach (RaycastHit[] c in hits)
        {
            walld = false;
            foreach (RaycastHit r in c)
            {
                if (!walld)
                {
                    r.collider.GetComponent<Entity>().SetDiscoverState(DiscoverState.InView, pos);
                    lastDiscoverEntites.Add(r.collider.GetComponent<Entity>());
                }
                if (r.collider.GetComponent<Entity>().entityType == EntityType.Block)
                    if  (r.collider.GetComponent<Block>().blockType == BlockType.Wall)
                        walld = true;
            }
        }
    }

    public Actor CheckNextTile()
    {
        //Get player tile pos
        int px = (int)Math.Round(transform.position.x / MapGenerator.GRID_SIZE) + Math.Abs(mapGenerator.minX);
        int py = (int)Math.Round(transform.position.y / MapGenerator.GRID_SIZE) + Math.Abs(mapGenerator.minY);
        //Get selected object tile pos
        int tx = (int)Math.Round(selectedObject.transform.position.x / MapGenerator.GRID_SIZE) + Math.Abs(mapGenerator.minX);
        int ty = (int)Math.Round(selectedObject.transform.position.y / MapGenerator.GRID_SIZE) + Math.Abs(mapGenerator.minY);
        //Get path to target tile
        List<Point> tpath = Pathfinding.FindPath(this.mapGenerator.pathFindGrid,
            new Point(px, py), new Point(tx, ty));

        //Check if someone is in the way switch to melee
        foreach (Entity t in mapGenerator.GetEntitiesAt(new Vector3((tpath[0].x + mapGenerator.minX) * MapGenerator.GRID_SIZE,
            (tpath[0].y + mapGenerator.minY) * MapGenerator.GRID_SIZE, 0)))
        {
            if (t.entityType == EntityType.Actor)
                return (Actor)t;
        }
        return null;
    }

    //Player collides with something
    private void OnTriggerEnter(Collider other)
    {
        switch (other.transform.tag)
        {
            //Item
            case "Item":
                if (inventory.Count < inventorySize)
                    LootItem(other.gameObject.GetComponent<Item>());
                break;
        }
        inventoryManager.RefreshUI();
    }

    private void LootItem(Item item)
    {
        if (inventory.Count == inventorySize)
        {
            textEventGen.AddTextEvent("Inventaire plein.", EventTextType.Combat);
        }
        else
        {
            switch (item.itemType)
            {
                case ItemTypes.Gun:
                    if (equipmentGun == null)
                        equipmentGun = (Gun)item;
                    else
                        inventory.Add(item);
                    break;
                case ItemTypes.Melee:
                    if (equipmentMelee == null || equipmentMelee.GetType() == typeof(Fists))
                        equipmentMelee = (Melee)item;
                    else
                        inventory.Add(item);
                    break;
                case ItemTypes.Armor:
                    if (equipmentArmor == null)
                        equipmentArmor = item;
                    else
                        inventory.Add(item);
                    break;
                case ItemTypes.Gadget:
                    if (equipmentGadget1 == null) equipmentGadget1 = item;
                    else
                    {
                        if (equipmentGadget2 == null) equipmentGadget2 = item;
                        else
                            inventory.Add(item);
                    }
                    break;
                default:
                    inventory.Add(item);
                    break;
            }
            item.Loot(this);
        }
    }

    public void Wait()
    {
        DiscoverAround(transform.position);
        textEventGen.AddTextEvent("Attente", EventTextType.Normal);
    }

    void Move()
    {
        //Get player tile pos
        int px = (int)Math.Round(transform.position.x / MapGenerator.GRID_SIZE) + Math.Abs(mapGenerator.minX);
        int py = (int)Math.Round(transform.position.y / MapGenerator.GRID_SIZE) + Math.Abs(mapGenerator.minY);
        //Get selected object tile pos
        int tx = (int)Math.Round(selectedObject.transform.position.x / MapGenerator.GRID_SIZE) + Math.Abs(mapGenerator.minX);
        int ty = (int)Math.Round(selectedObject.transform.position.y / MapGenerator.GRID_SIZE) + Math.Abs(mapGenerator.minY);
        //Get path to target tile
        List<Point> tpath = Pathfinding.FindPath(this.mapGenerator.pathFindGrid,
            new Point(px, py), new Point(tx, ty));

        if (tpath.Count == 1)
        {
            //Unselect tile if at destination
            selectedObject.GetComponent<Entity>().UnSelect();
            selectedObject = null;
        }
        //Move toward the first point of the list
        StartCoroutine(MoveToPosition(new Vector3((tpath[0].x + mapGenerator.minX) * MapGenerator.GRID_SIZE,
            (tpath[0].y + mapGenerator.minY) * MapGenerator.GRID_SIZE, 0), 0.2f));
        
        DiscoverAround(new Vector3((tpath[0].x + mapGenerator.minX) * MapGenerator.GRID_SIZE, 
            (tpath[0].y + mapGenerator.minY) * MapGenerator.GRID_SIZE, 0));
        RefreshCover(new Vector3((tpath[0].x + mapGenerator.minX) * MapGenerator.GRID_SIZE,
            (tpath[0].y + mapGenerator.minY) * MapGenerator.GRID_SIZE, 0));

        audioSource.clip = audioClip_walk[UnityEngine.Random.Range(0, audioClip_walk.Count())];
        audioSource.Play();
    }

    public IEnumerator MoveToPosition(Vector3 end, float timeToGo)
    {
        var startRotation = transform.position;
        var t = 0f;
        while (t <= 1f)
        {
            t += Time.deltaTime / timeToGo;
            transform.position = Vector3.Lerp(startRotation, end, t);
            //Center camera on player
            playerCamera.transform.position = new Vector3(transform.position.x, transform.position.y, zoomLevel);
            yield return null;
        }
        transform.position = end;
        playerCamera.transform.position = new Vector3(transform.position.x, transform.position.y, zoomLevel);
    }
}
