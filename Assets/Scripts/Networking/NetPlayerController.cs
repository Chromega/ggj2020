using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class NetPlayerController : PlayerControllerBase
{
    public static NetPlayerController LocalInstance { get; private set; }
    private PhotonView photonView;

    public Vector2 input;
    public string[] netInventory;
    public string repairTypeHelperText;

    // Start is called before the first frame update
    void Awake()
    {
        photonView = GetComponent<PhotonView>();
        if (photonView.IsMine)
        {
            LocalInstance = this;
        }
     }

    IEnumerator Start()
    {
        while (!HostSceneMgr.Instance)
            yield return null;
        
        HostSceneMgr.Instance.RegisterPlayer(this);
    }

    // Update is called once per frame
    void Update()
    {
        /*
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
                string[] netInventory = { "hammer", "wrench" };
                SetInventory(netInventory);
            }
        }*/
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
    public override void SetInventory(string[] netInventory)
    {
        this.netInventory = netInventory;
        photonView.RPC("SetInventoryRpc", RpcTarget.Others, netInventory);
    }

    public override string[] GetInventory()
    {
        return netInventory;
    }

    public override Vector2 GetInput()
    {
        return input;
    }

    public void UseItem(int index) {
        photonView.RPC("UseItemRpc", RpcTarget.MasterClient, index);
    }

    public override void SetRepairTypeHelperText(string repairTypeHelperText)
    {
        photonView.RPC("SetRepairTypeHelperTextRpc", RpcTarget.Others, repairTypeHelperText);
    }

    [PunRPC]
    void SetInputRpc(Vector2 pos)
    {
        this.input = pos;
        //Debug.Log("Client input is " + pos.x + ", " + pos.y);
    }

    [PunRPC]
    void UseItemRpc(int index)
    {
        triggerItemUsed(index);
    }

    [PunRPC]
    void SetInventoryRpc(string[] netInventory)
    {
        this.netInventory = netInventory;
        //Debug.Log("Inventory is " + netInventory.Length + " items");
    }

    [PunRPC]
    void SetRepairTypeHelperTextRpc(string repairTypeHelperText)
    {
        this.repairTypeHelperText = repairTypeHelperText;
    }
}
