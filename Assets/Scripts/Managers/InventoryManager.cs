using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    //UI
    public TMP_Text ammoText;
    public TMP_Text healthText;
    public TMP_Text initiativeText;
    public TMP_Text moveSpeedText;
    public TMP_Text APText;

    public GameObject inventoryCanvas;
    List<Image> inventorySlots;
    public Sprite EmptySlot;
    public Image equippedGun;
    public Image equippedMelee;
    public Image equippedArmor;
    public Image equippedGadget1;
    public Image equippedGadget2;

    public GameObject descriptionCanvas;
    public TMP_Text itemNameText;
    public TMP_Text itemDescriptionText;
    public TMP_Text itemDamageText;
    public TMP_Text itemRangeText;
    public TMP_Text itemAPCostText;
    public Image itemSprite;
    public Button itemUseButton;
    public Button itemDropButton;
    public Button itemCancelButton;

    public Item selectedSlot;
    public Fists FistsInstance;

    Player player;

    private void Start()
    {
        SetInventoryUI(false);
        SetDescriptionUI(false);
        inventorySlots = new List<Image>();
        for (int i = 0; i < inventoryCanvas.transform.childCount; i++)
        {
            inventorySlots.Add(inventoryCanvas.transform.GetChild(i).GetChild(0).GetComponent<Image>());
        }
    }

    public void SetPlayer(Player player)
    {
        this.player = player;
    }

    public void RefreshUI()
    {
        healthText.text = "Health : " + player.health.ToString();
        initiativeText.text = "Initative : " + player.initiative.ToString();
        moveSpeedText.text = "Move speed : " + player.moveSpeed.ToString();
        APText.text = "AP : " + player.actionPoints.ToString();

        if (player.equipmentGun != null)
        {
            ammoText.text = "Ammo : " + player.equipmentGun.currentAmmo;
            equippedGun.sprite = player.equipmentGun.GetComponent<SpriteRenderer>().sprite;
        }
        else
        {
            ammoText.text = "Ammo : 0";
            equippedGun.sprite = EmptySlot;
        }

        equippedMelee.sprite = player.equipmentMelee != null ? player.equipmentMelee.GetComponent<SpriteRenderer>().sprite : EmptySlot;
        equippedArmor.sprite = player.equipmentArmor != null ? player.equipmentArmor.GetComponent<SpriteRenderer>().sprite : EmptySlot;
        equippedGadget1.sprite = player.equipmentGadget1 != null ? player.equipmentGadget1.GetComponent<SpriteRenderer>().sprite : EmptySlot;
        equippedGadget2.sprite = player.equipmentGadget2 != null ? player.equipmentGadget2.GetComponent<SpriteRenderer>().sprite : EmptySlot;

        for (int i = 0; i < player.inventorySize; i++)
        {
            if (i < player.inventory.Count)
                inventorySlots[i].sprite = player.inventory[i].gameObject.GetComponent<SpriteRenderer>().sprite;
            else
                inventorySlots[i].sprite = EmptySlot;
        }    
    }

    public void ToggleInventory()
    {
        player.isInventoryOpen = !player.isInventoryOpen;
        SetInventoryUI(player.isInventoryOpen);
        if (!player.isInventoryOpen)
            SetDescriptionUI(false);
    }

    public void SetInventoryUI(bool state)
    {
        for (int i = 0; i < inventoryCanvas.transform.childCount; i++)
            inventoryCanvas.transform.GetChild(i).gameObject.SetActive(state);
    }

    public void SetDescriptionUI(bool state)
    {
        for (int i = 0; i < descriptionCanvas.transform.childCount; i++)
            descriptionCanvas.transform.GetChild(i).gameObject.SetActive(state);
    }

    public void ItemClick(int id)
    {
        if (!player.isInventoryOpen) return;
        Item target = null;
        itemDamageText.text = "";
        itemRangeText.text = "";
        itemUseButton.onClick.RemoveAllListeners();
        itemDropButton.onClick.RemoveAllListeners();
        if (id < 0)
        {
            switch (id)
            {
                case -1: target = player.equipmentGun; break;
                case -2: target = player.equipmentMelee; break;
                case -3: target = player.equipmentArmor; break;
                case -4: target = player.equipmentGadget1; break;
                case -5: target = player.equipmentGadget2; break;
            }
            if (target == null)
            {
                SetDescriptionUI(false);
                return;
            }
            if (target.GetType() != typeof(Fists))
            {
                itemUseButton.GetComponentInChildren<TMP_Text>().text = "Unequip";
                itemUseButton.onClick.AddListener(ItemUnequip);
                itemDropButton.GetComponentInChildren<TMP_Text>().text = "Drop";
                itemDropButton.onClick.AddListener(ItemEquipmentDrop);
            }
            else
            {
                itemUseButton.GetComponentInChildren<TMP_Text>().text = "";
                itemDropButton.GetComponentInChildren<TMP_Text>().text = "";
            }
        }
        else
        {
            target = player.inventory.ElementAtOrDefault(id);
            if (target == null)
            {
                SetDescriptionUI(false);
                return;
            }
            itemUseButton.GetComponentInChildren<TMP_Text>().text = "Equip";
            itemUseButton.onClick.AddListener(ItemEquip);
            itemDropButton.GetComponentInChildren<TMP_Text>().text = "Drop";
            itemDropButton.onClick.AddListener(ItemDrop);
        }

        itemNameText.text = target.entityName;
        itemDescriptionText.text = target.entityDesc;
        itemSprite.sprite = target.GetComponent<SpriteRenderer>().sprite;
        itemAPCostText.text = "AP cost : " + target.APCost.ToString();
        switch (target.itemType)
        {
            case ItemTypes.Gun:
                itemDamageText.text = "Damage : " + ((Gun)target).MinDamage + " - " + ((Gun)target).MaxDamage;
                itemRangeText.text = "Max range : " + ((Gun)target).MaxRange + ", optimal : " + ((Gun)target).optiRange;
                break;
            case ItemTypes.Melee:
                itemDamageText.text = "Damage : " + ((Melee)target).MinDamage + " - " + ((Melee)target).MaxDamage;
                break;
            case ItemTypes.Aid:
                itemDamageText.text = "Heal : ";
                break;
        }

        SetDescriptionUI(true);
        selectedSlot = target;
    }

    public void ItemEquip()
    {
        itemUseButton.onClick.RemoveAllListeners();
        itemDropButton.onClick.RemoveAllListeners();
        player.inventory.Remove(selectedSlot);
        switch (selectedSlot.itemType)
        {
            case ItemTypes.Gun: 
                if (player.equipmentGun != null)
                    player.inventory.Add(player.equipmentGun); 
                player.equipmentGun = (Gun)selectedSlot; 
                break;
            case ItemTypes.Melee:
                if (player.equipmentMelee != null)
                {
                    if (player.equipmentMelee.GetType() != typeof(Fists))
                        player.inventory.Add(player.equipmentMelee);
                }
                player.equipmentMelee = (Melee)selectedSlot;
                break;
            case ItemTypes.Armor:
                if (player.equipmentArmor != null)
                    player.inventory.Add(player.equipmentArmor); 
                player.equipmentArmor = selectedSlot; 
                break;
            case ItemTypes.Gadget:
                if (player.equipmentGadget1 == null)
                    player.equipmentGadget1 = selectedSlot;
                else
                {
                    if (player.equipmentGadget2 == null)
                        player.inventory.Add(player.equipmentGadget2);
                    player.equipmentGadget2 = selectedSlot;
                }
            break;
        }
        selectedSlot = null;
        SetDescriptionUI(false);
        RefreshUI();
    }

    public void ItemUnequip()
    {
        itemUseButton.onClick.RemoveAllListeners();
        itemDropButton.onClick.RemoveAllListeners();
        switch (selectedSlot.itemType)
        {
            case ItemTypes.Gun: player.equipmentGun = null; break;
            case ItemTypes.Melee: player.equipmentMelee = FistsInstance; break;
            case ItemTypes.Armor: player.equipmentArmor = null; break;
            case ItemTypes.Gadget:
                if (player.equipmentGadget1 == selectedSlot)
                    player.equipmentGadget1 = null;
                else player.equipmentGadget2 = null;
            break;
        }
        player.inventory.Add(selectedSlot);
        selectedSlot = null;
        SetDescriptionUI(false);
        RefreshUI();
    }

    public void ItemCancel()
    {
        itemUseButton.onClick.RemoveAllListeners();
        itemDropButton.onClick.RemoveAllListeners();
        selectedSlot = null;
        SetDescriptionUI(false);
    }

    public void ItemDrop()
    {
        itemUseButton.onClick.RemoveAllListeners();
        itemDropButton.onClick.RemoveAllListeners();
        player.inventory.Remove(selectedSlot);
        selectedSlot.Drop();
        selectedSlot = null;
        SetDescriptionUI(false);
        RefreshUI();
    }

    public void ItemEquipmentDrop()
    {
        itemUseButton.onClick.RemoveAllListeners();
        itemDropButton.onClick.RemoveAllListeners();
        switch (selectedSlot.itemType)
        {
            case ItemTypes.Gun: player.equipmentGun = null; break;
            case ItemTypes.Melee: player.equipmentMelee = FistsInstance; break;
            case ItemTypes.Armor: player.equipmentArmor = null; break;
            case ItemTypes.Gadget:
                if (player.equipmentGadget1 == selectedSlot)
                    player.equipmentGadget1 = null;
                else player.equipmentGadget2 = null;
                break;
        }
        selectedSlot.Drop();
        selectedSlot = null;
        SetDescriptionUI(false);
        RefreshUI();
    }
}
