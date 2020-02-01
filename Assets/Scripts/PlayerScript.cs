using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    public int playerNum;
    public PlayerControllerBase playerControllerBase;

    public Inventory inventory;

    public int speed = 10;
    public bool lockMovement = false;
    public bool usingItem = false;
    private Quaternion rotateTo;

    private void Awake()
    {
        inventory = new Inventory();
    }
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        // check for movement input
        if (!lockMovement) {
            Vector2 input = playerControllerBase.GetInput();
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
        transform.position += speed * movement * Time.deltaTime;
        if (movement != Vector3.zero) {
            rotateTo = Quaternion.LookRotation(movement);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotateTo, Time.deltaTime * 20);
        }
    }

    public void UseItem(int index) {
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
                pickupable.transform.localPosition = new Vector3(0, 0, 1);
                IEnumerator coroutine = pickupable.Deactivate(this.OnItemDeactivated);
                StartCoroutine(coroutine);
            }
        }
    }

    void OnItemDeactivated() {
        this.lockMovement = false;
        this.usingItem = false;
    }

    public void SetPlayerController(PlayerControllerBase p) {
        playerControllerBase = p;
        inventory.player = p;
        p.onItemUsed += this.UseItem;
    }
}
