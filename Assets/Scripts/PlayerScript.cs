using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    public int playerNum;
    public PlayerControllerBase playerControllerBase;

    public Inventory inventory;

    public int speed = 10;
    public float slowThreshold = .55f;
    public bool lockMovement = false;
    public bool usingItem = false;
    private Quaternion rotateTo;

    private void Awake()
    {
        inventory = new Inventory();
        if (!playerControllerBase)
            gameObject.SetActive(false);
    }
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        // check for movement input
        if (!lockMovement && playerControllerBase) {
            Vector2 input = playerControllerBase.GetInput();
            if (input.magnitude > 1) {
                input = input.normalized;
            }
            float inputX = input.x;
            float inputY = input.y;

            Vector3 convertedX = Camera.main.transform.right * inputX;
            convertedX.y = 0;
            float oldXMagnitude = Mathf.Abs(inputX);
            float newXMagnitude = convertedX.magnitude;
            if (newXMagnitude == 0) {
                convertedX = Vector3.zero;
            } else {
                convertedX *= oldXMagnitude / newXMagnitude;
            }


            Vector3 convertedY = Camera.main.transform.up * inputY;
            convertedY.y = 0;
            float oldYMagnitude = Mathf.Abs(inputY);
            float newYMagnitude = convertedY.magnitude;
            if (newYMagnitude == 0) {
                convertedY = Vector3.zero;
            } else {
                convertedY *= oldYMagnitude / newYMagnitude;
            }

            this.Move(convertedX + convertedY);
        }
    }

    public void Move(Vector3 movement) {
        float waterDebuff = 1f;
        if (Game.Instance.ship?.HullPercentage() > slowThreshold) {
            waterDebuff = .5f;
        }
        transform.position += speed * waterDebuff * movement * Time.deltaTime;
        if (movement != Vector3.zero) {
            rotateTo = Quaternion.LookRotation(movement);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotateTo, Time.deltaTime * 20);
        }
    }

    public void UseItem(int index) {
        if (Time.timeScale == 0) {
            // Do nothing when time is frozen
            return;
        }
        if (!usingItem) {
            Item item = inventory.items[index];
            if (item != null) {
                usingItem = true;
                lockMovement = true;
                Debug.Log("Used item " + index);
                Pickupable pickupable = PickupableFactory.Instance.Activate(item);
                pickupable.DisablePhysics();
                pickupable.player = this;
                pickupable.playerInventorySlot = index;
                pickupable.transform.parent = transform;
                pickupable.Deactivate(this.OnItemDeactivated);
            }
        }
    }

    public void InformRepairType(RepairType repairType) {
        Debug.Log(repairType);
    }

    public void Reset() {
        inventory.Reset();
    }

    void OnItemDeactivated() {
        this.lockMovement = false;
        this.usingItem = false;
    }

    public void SetPlayerController(PlayerControllerBase p) {
        playerControllerBase = p;
        inventory.player = p;
        p.onItemUsed += this.UseItem;
        gameObject.SetActive(true);
    }
}
