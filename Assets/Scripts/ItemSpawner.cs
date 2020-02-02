using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    public List<Item> items;

    const int MAX_ITEMS = 30;

    const int X_BOUND = 15 * 7;
    const int Z_BOUND = 15 * 7;

    const float X_OFFSET = 2.0f;
    const float Z_OFFSET = -1.2f;

    void Awake()
    {
        items = new List<Item>();
        Object[] objs = Resources.LoadAll("Items", typeof(Item));
        foreach (var item in objs)
        {
            items.Add((Item)item);
        }
    }

    private void Start()
    {
        // Start with 2 random items
        SpawnRandomItem();
        SpawnRandomItem();
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            SpawnRandomItem();
        }
    }

    // Spawn a number of items if under max item count
    public void SpawnItemCount(int num)
    {
        Debug.Log(TotalItemsInGame());
        if (TotalItemsInGame() <= MAX_ITEMS)
        {
            for (int i = 0; i < num; i++)
            {
                SpawnRandomItem();
            }
        }
    }
    public void SpawnItem(Item item)
    {
        Pickupable pick = PickupableFactory.Instance.Activate(item);
        float x = (Random.Range(0, X_BOUND) - (X_BOUND / 2)) / 10.0f + X_OFFSET;
        float z = (Random.Range(0, Z_BOUND) - (Z_BOUND / 2)) / 10.0f + Z_OFFSET;
        pick.transform.parent = Game.Instance.ship.transform;
        pick.transform.localPosition = new Vector3(x, 1f, z);
    }

    public void SpawnRandomItem()
    {
        SpawnItem(PickRandomItem());
    }

    public Item PickRandomItem()
    {
        int roll = Random.Range(0, items.Count);
        return items[roll];
    }

    public int TotalItemsInGame()
    {
        int total = 0;
        if (Game.Instance.player1 != null)
        {
            total += Game.Instance.player1.inventory.NumItems();
        }
        if (Game.Instance.player2 != null)
        {
            total += Game.Instance.player2.inventory.NumItems();
        }
        if (Game.Instance.player3 != null)
        {
            total += Game.Instance.player3.inventory.NumItems();
        }
        if (Game.Instance.player4 != null)
        {
            total += Game.Instance.player4.inventory.NumItems();
        }
        total += PickupableFactory.Instance.TotalActiveItems();
        return total;
    }
}
