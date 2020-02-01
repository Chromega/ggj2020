using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    public List<Item> items;

    const int X_BOUND = 37 * 7;
    const int Z_BOUND = 17 * 7;

    const float X_OFFSET = 1.5f;
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

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            SpawnItem(PickRandomItem());
        }
    }

    public void SpawnItem(Item item)
    {
        Pickupable pick = PickupableFactory.Instance.Activate(item);
        float x = (Random.Range(0, X_BOUND) - (X_BOUND / 2)) / 10.0f + X_OFFSET;
        float z = (Random.Range(0, Z_BOUND) - (Z_BOUND / 2)) / 10.0f + Z_OFFSET;
        pick.transform.position = new Vector3(x, 1f, z);
    }

    public Item PickRandomItem()
    {
        int roll = Random.Range(0, items.Count - 1);
        return items[roll];
    }
}
