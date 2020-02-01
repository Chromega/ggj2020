using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory
{
    public Item[] items = new Item[SIZE];

    public int size
    {
        get
        {
            return items.Length;
        }
    }

    const int SIZE = 4;

    // Remove an item from the inventory and return it
    public Item GetItem(int index)
    {
        Item item = items[index];
        items[index] = null;
        return item;
    }

    // Add an item. Returns whether successful or not
    public bool AddItem(Item item)
    {
        for (var i = 0; i < items.Length; i++)
        {
            if (items[i] == null)
            {
                items[i] = item;
                return true;
            }
        }
        return false;
    }

    // Use an item. Returns whether successful or not
    public bool Use(int index) {
        return true;
    }
}
