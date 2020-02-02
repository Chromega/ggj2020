using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickupable : MonoBehaviour
{
    // The scriptable object reference
    public Item item;
    public PlayerScript player;
    public int playerInventorySlot = -1;
    private bool inUse = false;
    private System.Action inUseCallback;

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
        if (player == null && collision.collider.CompareTag("Player"))
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
            player.inventory.RemoveItem(playerInventorySlot);
            player.inventory.NetInventorySync();
        }
        player = null;
        playerInventorySlot = -1;
    }

    public void Update()
    {
        if (inUse) {
            transform.localPosition += new Vector3(0, -0.5f * Time.deltaTime / deactivationTime, 0);
            if (transform.localPosition.y < 0) {
                PickupableFactory.Instance.Deactivate(this);
                transform.parent = PickupableFactory.Instance.transform;
                inUseCallback?.Invoke();
            }
        }
    }

    public void Deactivate(System.Action callback)
    {
        transform.localPosition = new Vector3(0, .5f, 1);
        inUse = true;
        inUseCallback = callback;
    }

    public void Reset()
    {
        item = null;
        collided = false;
        player = null;
        playerInventorySlot = -1;
        inUse = false;
        EnablePhysics();
        Destroy(model);
    }
}
