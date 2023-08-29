using NesScripts.Controls.PathFind;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

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

    public Gun equipmentGun;
    public Melee equipmentMelee;
    public Item equipmentArmor;
    public Item equipmentGadget1;
    public Item equipmentGadget2;
    public int inventorySize = 10;
    public bool isInventoryOpen;

    //Current selected tile or object
    public GameObject selectedObject = null;
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
        this.cover = 0;
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
        inventoryManager.RefreshUI();

        RefreshCover();
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
                                this.waitingAction = new TurnAction(Wait, actionPoints);
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
                                        this.waitingAction = new TurnAction(Move, moveSpeed);
                                    else
                                        textEventGen.AddTextEvent("Pas assez d'AP", EventTextType.Skill);
                                }
                            }
                            break;
                        case "Enemy":
                            Actor m = CheckNextTile();
                            if (m != null)
                            {
                                if (((Melee)equipmentMelee).APCost <= actionPoints)
                                    this.waitingAction = new TurnAction(((Melee)equipmentMelee).Paquis, ((Melee)equipmentMelee).APCost);
                                else
                                    textEventGen.AddTextEvent("Pas assez d'AP", EventTextType.Skill);
                            }
                            else
                            {
                                if (equipmentMelee != null)
                                {
                                    if (((Gun)equipmentGun).APCost <= actionPoints)
                                        this.waitingAction = new TurnAction(((Gun)equipmentGun).Shoot, ((Gun)equipmentGun).APCost);
                                    else
                                        textEventGen.AddTextEvent("Pas assez d'AP", EventTextType.Skill);
                                }
                            }
                            break;
                }
            }
            else
            {
                //Wait
                this.waitingAction = new TurnAction(Wait, actionPoints);
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

    public void RefreshCover()
    {

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
                LootItem(other.gameObject.GetComponent<Item>());
                break;
            //Open the door
            /*case "Door":
                //Play sound
                audioSource.clip = audioClip_openDoor;
                audioSource.Play();
                //Remove door
                Destroy(other.gameObject);
                break;
            //Door to next level
            case "Trapdoor":
                //If the key was aquired
                foreach (Item t in inventory)
                {
                    if (t.itemType == ItemTypes.Key)
                    {
                        //Play sound
                        audioSource.clip = audioClip_openTrapDoor;
                        audioSource.Play();
                        //Save player state
                        turnManager.SavePlayerState(this);
                        //Generate next level
                        turnManager.GoToNextLevel();
                    }
                }
                break;*/
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
        RefreshCover();
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
