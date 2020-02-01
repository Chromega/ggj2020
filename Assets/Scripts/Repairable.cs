using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Repairable : MonoBehaviour
{
    // What is the state of the current object
    public bool _broken = false;

    // What objects can repair this
    public RepairType repairType;

    public bool broken
    {
        get
        {
            return _broken;
        }
    }

    public MeshRenderer mesh;

    private void Awake()
    {
        mesh = GetComponent<MeshRenderer>();
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (broken)
        {
            mesh.material.color = Color.red;
        }
        else
        {
            mesh.material.color = Color.white;
        }
    }

    // Set the state of the object to broken
    public void Break()
    {
        _broken = true;
        List<RepairType> currentAvailableItems = PickupableFactory.Instance.CurrentAvailableItemRepairTypes();
        int index = Random.Range(0, currentAvailableItems.Count - 1);
        repairType = currentAvailableItems[index];
    }

    // Set the state of the object to fixed
    public bool Repair(Item item)
    {
        if (item.repairType == repairType)
        {
            _broken = false;
            return true;
        }
        return false;
    }

    public void OnCollisionEnter(Collision collision)
    {
        Item item = collision.transform.GetComponent<Item>();
        if (item != null)
        {
            if (Repair(item))
            {
                Destroy(item);
            }
        }
    }
}
