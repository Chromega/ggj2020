using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public Item[] items;

    public int size
    {
        get
        {
            return items.Length;
        }
    }

    private void Awake()
    {
        items = new Item[4];
    }

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
}
