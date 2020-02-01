using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class WebSceneMgr : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        PhotonNetwork.Instantiate("NetPlayerController", Vector3.zero, Quaternion.identity, 0) ;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
