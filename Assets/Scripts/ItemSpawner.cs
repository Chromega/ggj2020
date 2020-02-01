using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    public List<Item> items;

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
        pick.transform.position = new Vector3(0f, 0f, 0f);
    }

    public Item PickRandomItem()
    {
        int roll = Random.Range(0, items.Count - 1);
        return items[roll];
    }
}
