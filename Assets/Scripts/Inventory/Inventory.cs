using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory
{
    public Item[] items = new Item[SIZE];
    public PlayerControllerBase player;

    public int size
    {
        get
        {
            return items.Length;
        }
    }

    const int SIZE = 4;

    // Remove an item from the inventory and return it
    public Item RemoveItem(int index)
    {
        Item item = items[index];
        items[index] = null;
        return item;
    }

    // Add an item. Returns whether successful or not
    public bool AddItem(Item item)
    {
        bool itemAdded = false;

        for (var i = 0; i < items.Length; i++)
        {
            if (items[i] == null)
            {
                items[i] = item;
                itemAdded = true;
                break;
            }
        }

        // sync the inventory with the mobile view
        NetInventorySync();

        // return whether the add succeeded
        return itemAdded;
    }

    public void NetInventorySync() {
      // make the network call so the phone knows what inventory we have
      string[] netInventory = new string[items.Length];
      for (var i = 0; i < items.Length; i++)
      {
          if (items[i] != null)
          {
              netInventory[i] = items[i].displayName;
          } else {
              netInventory[i] = "";
          }

      }
      if (player != null) {
          player.SetInventory(netInventory);
      }
    }

    // Use an item. Returns whether successful or not
    public bool Use(int index) {
        return true;
    }

    // Total number of items owned by player
    public int NumItems()
    {
        int total = 0;
        for (var i = 0; i < items.Length; i++)
        {
            if (items[i] != null)
            {
                total++;
            }
        }
        return total;
    }
}
