using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickupable : MonoBehaviour
{
    // The scriptable object reference
    public Item item;

    public bool collided = false;

    private GameObject model;

    private void Awake()
    {
    }

    // Pick up items when triggere by player colliders
    public void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Collided");
        if (collision.collider.CompareTag("Player"))
        {
            PlayerScript player = collision.collider.gameObject.GetComponent<PlayerScript>();
            // Remove the item if we successfully pick it up
            if (player.inventory.AddItem(item) && collided == false)
            {
                collided = true;
                PickupableFactory.Instance.Deactivate(this);
            }
        }
    }

    public void SetItem(Item item)
    {
        Reset();
        this.item = item;
        model = Instantiate(item.prefab, this.transform);
    }

    public void Consume()
    {

    }

    IEnumerator Deactivate()
    {
        yield return new WaitForSeconds(1f);
        PickupableFactory.Instance.Deactivate(this);
    }

    public void BeginDeactivate()
    {
        StartCoroutine("Deactivate");
    }

    public void Reset()
    {
        item = null;
        collided = false;
        Destroy(model);
    }
}
