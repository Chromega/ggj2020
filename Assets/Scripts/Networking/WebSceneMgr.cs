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
        }
    }
}
