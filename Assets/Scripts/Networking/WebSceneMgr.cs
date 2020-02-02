using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using UnityEngine.UI;

public class WebSceneMgr : MonoBehaviour
{
    public FixedJoystick joystick;
    public Canvas uiCanvas;
    public Button buttonPrefab;
    public GameObject panelToAttach;
    public List<Button> buttonList = new List<Button>();
    string[] inv;

    // Start is called before the first frame update
    void Start()
    {
        PhotonNetwork.Instantiate("NetPlayerController", Vector3.zero, Quaternion.identity, 0) ;
    }

    public void updateInventoryButtons(string[] inv)
    {
      // instantiate enough buttons
      for (var i = buttonList.Count; i < inv.Length; i++)
      {
        Button button = Instantiate( buttonPrefab ) ;
        button.transform.position = panelToAttach.transform.position;
        Vector3 currentPos = button.transform.position;
        currentPos.y += i*50;
        button.transform.position = currentPos;
        button.GetComponent<RectTransform>().SetParent(panelToAttach.transform);
        int buttonIdx = i;
        button.onClick.AddListener(() => {
          NetPlayerController.LocalInstance.UseItem(buttonIdx);
        });
        buttonList.Add(button);
      }

      for (var i = 0; i < inv.Length; i++)
      {
        Button button = buttonList[i];
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
            updateInventoryButtons(netInventory);

            // check if there are any tool tips to display
            //Debug.Log(NetPlayerController.LocalInstance.repairTypeHelperText);
        }
    }

    public string ConvertStringArrayToStringJoin(string[] array)
    {
        // Use string Join to concatenate the string elements.
        string result = string.Join(".", array);
        return result;
    }
}
