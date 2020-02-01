using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RepairType
{
    Engine,
    Floor,
    Mast,
    Rudder,
}

[CreateAssetMenu(menuName = "Items/Item")]
public class Item : ScriptableObject
{
    // Name of the item
    public string displayName;
    // What types of objects can be repaired
    public RepairType repairType;
    // Display icon
    public Sprite itemIcon;
    // The prefab of the object to spawn
    public GameObject prefab;
}
