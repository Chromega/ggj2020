using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIInventory : MonoBehaviour
{
    public Inventory inventory;

    public UIInventorySlot[] inventorySlots;
    public UIInventorySlot inventorySlotPrefab;


    public void AssignInventory(Inventory inv)
    {
        inventory = inv;
        inventorySlots = new UIInventorySlot[inventory.size];
        for (var i = 0; i < inventory.size; i++)
        {
            inventorySlots[i] = Instantiate(inventorySlotPrefab, transform);
            inventorySlots[i].SetItem(inventory.items[i]);
        }
    }

    private void Update()
    {
        // Keep the inventory updated
        if (inventory != null)
        {
            for (var i = 0; i < inventory.size; i++)
            {
                inventorySlots[i].SetItem(inventory.items[i]);
            }
        }
    }
}
