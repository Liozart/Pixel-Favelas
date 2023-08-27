using NesScripts.Controls.PathFind;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security;
using System.Transactions;
using TMPro;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Animations;
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

    //Audio
    public AudioClip audioClip_openDoor;
    public AudioClip audioClip_openTrapDoor;
    public AudioClip audioClip_getKey;
    public AudioClip audioClip_getLoot;

    //Stats
    public List<Item> inventory = new List<Item>();

    //Current selected tile or object
    public GameObject selectedObject = null;
    //Auto-play vars
    public float delayBeforeAutoThresold = 0.5f;
    float delayBeforeAuto = 0;

    //UI
    public TMP_Text healthText;
    public TMP_Text speedText;
    public TMP_Text APText;
    public GameObject Equipped1;

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
        this.actorType = ActorType.Player;
        switch (playerType)
        {
            case PlayerClass.Base:
                gameObject.GetComponent<SpriteRenderer>().sprite = playerSpriteBase;
                break;
        }
        playerCamera = GameObject.FindGameObjectWithTag("MainCamera");
        //Center camera on player
        playerCamera.transform.position = new Vector3(transform.position.x, transform.position.y, 3);
        //UI
        healthText = GameObject.Find("HealthText").GetComponent<TMP_Text>();
        speedText = GameObject.Find("SpeedText").GetComponent<TMP_Text>();
        APText = GameObject.Find("APText").GetComponent<TMP_Text>();
        Equipped1 = GameObject.Find("Equipped1");

        inventory.Add(Instantiate(mapGenerator.PrefabMakarov, transform).GetComponent<Item>());
        inventory[0].Loot(this);

        RefreshUI();
    }

    // Update is called once per frame
    void Update()
    {
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
            if (selectedObject != null) switch (selectedObject.tag)
            {
                case "Floor":
                case "Trapdoor":
                case "Door":
                case "Item": 
                        this.waitingActionsList.Add(new TurnAction(MoveToSelection, moveSpeed)); break;
                case "Enemy":
                        this.waitingActionsList.Add(new TurnAction(((Gun)inventory[0]).Shoot, ((Gun)inventory[0]).APCost));
                        break;
            }
            //Finish the turn
            this.turnManager.Resolve();
            RefreshUI();
        }

        //Auto turn
        if (Input.GetMouseButton((int)MouseButton.RightMouse))
            if (delayBeforeAuto < delayBeforeAutoThresold)
                delayBeforeAuto += Time.deltaTime;
            else
            {
                if (selectedObject != null) switch (selectedObject.tag)
                    {
                        case "Floor":
                        case "Trapdoor":
                        case "Door":
                        case "Item": this.waitingActionsList.Add(new TurnAction(MoveToSelection, moveSpeed)); break;
                    }
                //Finish the turn
                this.turnManager.Resolve();
                RefreshUI();
                delayBeforeAuto = 0;
            }
        else
            delayBeforeAuto = 0;
    }

    public void RefreshUI()
    {
        healthText.text = "Health : " + health.ToString();
        speedText.text = "Speed :" + initiative.ToString();
        APText.text = "AP :" + actionPoints.ToString();
        //Equipped1.GetComponent<TMP_Text>().text = inventory[0].entityName;
    }

    public void ToggleInventory()
    {
        if (Equipped1.activeSelf)
            Equipped1.SetActive(false);
        else
            Equipped1.SetActive(true);
    }

    //Player collides with something
    private void OnTriggerEnter2D(Collider2D other)
    {
        switch (other.transform.tag)
        {
            //Item
            case "Item":
                other.gameObject.GetComponent<Item>().Loot(this);
                inventory.Add(other.gameObject.GetComponent<Item>());
                other.gameObject.transform.parent = transform;
                break;




            //Open the door
            case "Door":
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
                break;
        }
        RefreshUI();
    }

    void MoveToSelection()
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
        //Move toward the first point of the list
        StartCoroutine(MoveToPosition(new Vector3((tpath[0].x + mapGenerator.minX) * MapGenerator.GRID_SIZE,
            (tpath[0].y + mapGenerator.minY) * MapGenerator.GRID_SIZE, 0), 0.2f));
        //Unselect tile if destination
        if (tpath.Count == 1)
        {
            selectedObject.GetComponent<Entity>().UnSelect();
            selectedObject = null;
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
            //Center camera on player
            playerCamera.transform.position = new Vector3(transform.position.x, transform.position.y, 3);
            yield return null;
        }
        transform.position = end;
        playerCamera.transform.position = new Vector3(transform.position.x, transform.position.y, 3);
    }
}
