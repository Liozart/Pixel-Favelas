using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    //UI
    public TMP_Text healthText;
    public TMP_Text initiativeText;
    public TMP_Text moveSpeedText;
    public TMP_Text APText;
    public Canvas inventoryCanvas;
    public Image equippedGun;
    public Image equippedMelee;
    public Image equippedArmor;
    public Image equippedGadget1;
    public Image equippedGadget2;

    public Canvas descriptionCanvas;
    public TMP_Text itemNameText;
    public TMP_Text itemDescriptionText;
    public Image itemSprite;
    public Button itemUseButton;
    public Button itemDropButton;
    public Button itemCancelButton;

    List<Image> inventorySlots;
    public Sprite EmptySlot;

    public Item selectedSlot;

    Player player;

    private void Start()
    {
        inventoryCanvas.enabled = false;

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

        equippedGun.sprite = player.equipmentGun != null ? player.equipmentGun.GetComponent<SpriteRenderer>().sprite : EmptySlot;
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
        inventoryCanvas.enabled = !inventoryCanvas.enabled;
        player.isInventoryOpen = !player.isInventoryOpen;
    }

    public void ItemClick(int id)
    {
        Item target = null;
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
            if (target == null) return;
            itemUseButton.GetComponentInChildren<TMP_Text>().text = "Unequip";
            itemUseButton.onClick.AddListener(ItemUnequip);
            itemDropButton.onClick.AddListener(ItemEquipmentDrop);
        }
        else
        {
            target = player.inventory[id];
            if (target == null) return;
            itemUseButton.GetComponentInChildren<TMP_Text>().text = "Equip";
            itemUseButton.onClick.AddListener(ItemEquip);
            itemDropButton.onClick.AddListener(ItemDrop);
        }

        descriptionCanvas.enabled = true;

        itemNameText.text = target.entityName;
        itemDescriptionText.text = target.entityDesc;
        itemSprite.sprite = target.GetComponent<SpriteRenderer>().sprite;
        selectedSlot = target;
    }

    public void ItemEquip()
    {
        itemUseButton.onClick.RemoveListener(ItemEquip);
        player.inventory.Remove(selectedSlot);
        switch (selectedSlot.itemType)
        {
            case ItemTypes.Gun: player.equipmentGun = selectedSlot; break;
            case ItemTypes.Melee: player.equipmentMelee = selectedSlot; break;
            case ItemTypes.Armor: player.equipmentArmor = selectedSlot; break;
            case ItemTypes.Gadget:
                if (player.equipmentGadget1 == null)
                    player.equipmentGadget1 = selectedSlot;
                else player.equipmentGadget2 = selectedSlot;
            break;
        }
        descriptionCanvas.enabled = false;
        RefreshUI();
    }

    public void ItemUnequip()
    {
        itemUseButton.onClick.RemoveListener(ItemUnequip);
        switch (selectedSlot.itemType)
        {
            case ItemTypes.Gun: player.equipmentGun = null; break;
            case ItemTypes.Melee: player.equipmentMelee = null; break;
            case ItemTypes.Armor: player.equipmentArmor = null; break;
            case ItemTypes.Gadget:
                if (player.equipmentGadget1 == selectedSlot)
                    player.equipmentGadget1 = null;
                else player.equipmentGadget2 = null;
            break;
        }
        player.inventory.Add(selectedSlot);
        selectedSlot = null;
        descriptionCanvas.enabled = false;
        RefreshUI();
    }

    public void ItemCancel()
    {
        itemUseButton.onClick.RemoveAllListeners();
        itemDropButton.onClick.RemoveAllListeners();
        selectedSlot = null;
        descriptionCanvas.enabled = true;
    }

    public void ItemDrop()
    {
        itemDropButton.onClick.AddListener(ItemDrop);
        player.inventory.Remove(selectedSlot);
        selectedSlot.Drop();
        selectedSlot = null;
        inventoryCanvas.enabled = false;
        RefreshUI();
    }

    public void ItemEquipmentDrop()
    {
        itemDropButton.onClick.AddListener(ItemEquipmentDrop);
        switch (selectedSlot.itemType)
        {
            case ItemTypes.Gun: player.equipmentGun = null; break;
            case ItemTypes.Melee: player.equipmentMelee = null; break;
            case ItemTypes.Armor: player.equipmentArmor = null; break;
            case ItemTypes.Gadget:
                if (player.equipmentGadget1 == selectedSlot)
                    player.equipmentGadget1 = null;
                else player.equipmentGadget2 = null;
                break;
        }
        selectedSlot.Drop();
        selectedSlot = null;
        inventoryCanvas.enabled = false;
        RefreshUI();
    }
}
