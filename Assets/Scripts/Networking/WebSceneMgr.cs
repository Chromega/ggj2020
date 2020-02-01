using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;

public class WebSceneMgr : MonoBehaviour
{
    public FixedJoystick joystick;
    public Canvas uiCanvas;
    public GameObject buttonPrefab;
    public GameObject panelToAttach;

    // Start is called before the first frame update
    void Start()
    {
        PhotonNetwork.Instantiate("NetPlayerController", Vector3.zero, Quaternion.identity, 0) ;


        string[] inv = new string[4];
        inv[0] = "hammer";
        inv[1] = "hammer2";
        inv[2] = "hammer3";
        inv[3] = "hammer4";

        for (var i = 0; i < inv.Length; i++)
        {
          // Instantiate (clone) the prefab
          GameObject button = (GameObject) Instantiate( buttonPrefab ) ;
          button.transform.position = panelToAttach.transform.position;
          Vector3 currentPos = button.transform.position;
          currentPos.y += i*100;
          button.transform.position = currentPos;
          button.GetComponent<RectTransform>().SetParent(panelToAttach.transform);
          button.GetComponentInChildren<TextMeshProUGUI>().text = inv[i];
        }


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
            //Debug.Log(netInventoryStr);
        }
    }

    public string ConvertStringArrayToStringJoin(string[] array)
    {
        // Use string Join to concatenate the string elements.
        string result = string.Join(".", array);
        return result;
    }
}
