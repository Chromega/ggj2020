using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class NetPlayerController : PlayerControllerBase
{
    public static NetPlayerController LocalInstance { get; private set; }
    private PhotonView photonView;

    public Vector2 input;
    public string[] inventory;

    // Start is called before the first frame update
    void Awake()
    {
        photonView = GetComponent<PhotonView>();
        if (photonView.IsMine)
        {
            LocalInstance = this;
        }
     }

    void Start()
    {
        if (HostSceneMgr.Instance)
        {
            HostSceneMgr.Instance.RegisterPlayer(this);
        }
    }

    // Update is called once per frame
    void Update()
    {
        //EVERYTHING HERE IS TEMP AND FOR DEMO PURPOSES ONLY
        if (photonView.IsMine)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                SetInput(new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)));
            }
        }
        if (PhotonNetwork.IsMasterClient)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                string[] inventory = { "hammer", "wrench" };
                SetInventory(inventory);
            }
        }
    }

    //The client sets position from the UI here!
    public override void SetInput(Vector2 pos)
    {
        this.input = pos;
        photonView.RPC("SetInputRpc", RpcTarget.MasterClient, pos);
    }

    //The host sets the inventory.  Should call this after picking up/using an item.
    //Could make 'add inventory item' and 'remove inventory item' as useful helpers
    //Has to be an array...List<> is unsupported out of the box
    public override void SetInventory(string[] inventory)
    {
        this.inventory = inventory;
        photonView.RPC("SetInventoryRpc", RpcTarget.Others, inventory);
    }

    public override string[] GetInventory()
    {
        return inventory;
    }

    public override Vector2 GetInput()
    {
        return input;
    }

    [PunRPC]
    void SetInputRpc(Vector2 pos)
    {
        this.input = pos;
        Debug.Log("Client input is " + pos.x + ", " + pos.y);
    }

    [PunRPC]
    void SetInventoryRpc(string[] inventory)
    {
        this.inventory = inventory;
        Debug.Log("Inventory is " + inventory.Length + " items");
    }
}
