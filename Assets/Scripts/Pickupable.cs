using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickupable : MonoBehaviour
{
    // The scriptable object reference
    public Item item;
    public PlayerScript player;
    public int playerInventorySlot = -1;

    public bool collided = false;
    public float deactivationTime = 0.25f;

    private GameObject model;
    public Rigidbody rigidBody;

    private void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
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

    public void DisablePhysics()
    {
        rigidBody.constraints = RigidbodyConstraints.FreezeAll;
    }

    public void EnablePhysics()
    {
        rigidBody.constraints = RigidbodyConstraints.None;
    }

    public void Consume()
    {
        if (player != null) {
            player.inventory.GetItem(playerInventorySlot);
        }
        player = null;
        playerInventorySlot = -1;
    }

    public IEnumerator Deactivate(System.Action callback)
    {
        yield return new WaitForSeconds(deactivationTime);
        PickupableFactory.Instance.Deactivate(this);
        transform.parent = PickupableFactory.Instance.transform;
        this.EnablePhysics();
        callback?.Invoke();
    }

    public void Reset()
    {
        item = null;
        collided = false;
        player = null;
        playerInventorySlot = -1;
        EnablePhysics();
        Destroy(model);
    }
}
