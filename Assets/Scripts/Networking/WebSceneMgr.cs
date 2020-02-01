using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class WebSceneMgr : MonoBehaviour
{
    public FixedJoystick joystick;

    // Start is called before the first frame update
    void Start()
    {
        PhotonNetwork.Instantiate("NetPlayerController", Vector3.zero, Quaternion.identity, 0) ;
    }

    // Update is called once per frame
    void Update()
    {
        if (NetPlayerController.LocalInstance)
        {
            NetPlayerController.LocalInstance.SetInput(new Vector2(joystick.Horizontal, joystick.Vertical));

            // check inventory
            string[] netInventory = NetPlayerController.LocalInstance.GetInventory();
            string netInventoryStr = ConvertStringArrayToStringJoin(netInventory);
            Debug.Log(netInventoryStr);

        }
    }

    public string ConvertStringArrayToStringJoin(string[] array)
    {
        // Use string Join to concatenate the string elements.
        string result = string.Join(".", array);
        return result;
    }
}
