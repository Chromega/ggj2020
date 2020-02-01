using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Repairable : MonoBehaviour
{
    // What is the state of the current object
    public bool _broken = false;

    // What objects can repair this
    public RepairType repairType;

    public GameObject fixedObject;
    public GameObject brokenObject;

    public bool broken
    {
        get
        {
            return _broken;
        }
    }

    private void Awake()
    {
        fixedObject.SetActive(true);
        brokenObject.SetActive(false);
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    // Set the state of the object to broken
    public void Break()
    {
        _broken = true;
        fixedObject.SetActive(false);
        brokenObject.SetActive(true);
        List<RepairType> currentAvailableItems = PickupableFactory.Instance.CurrentAvailableItemRepairTypes();
        int index = Random.Range(0, currentAvailableItems.Count - 1);
        if (currentAvailableItems.Count > 0)
        {
            repairType = currentAvailableItems[index];
        }
        else
        {
            // Temp handle no items existing
            repairType = RepairType.Engine;
        }
    }

    // Set the state of the object to fixed
    public bool Repair(Pickupable pickupable)
    {
        if (pickupable.item.repairType == repairType)
        {
            _broken = false;
            fixedObject.SetActive(true);
            brokenObject.SetActive(false);
            return true;
        }
        return false;
    }

    public void OnCollisionEnter(Collision collision)
    {
        Pickupable pickupable = collision.transform.GetComponent<Pickupable>();
        if (pickupable != null)
        {
            Debug.Log("Pickup collision");
            if (Repair(pickupable))
            {
                pickupable.Consume();
            }
        }
    }
}
