﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using UnityEngine.UI;
using Photon.Realtime;

public class WebSceneMgr : MonoBehaviourPunCallbacks
{
    public FixedJoystick joystick;
    public Canvas uiCanvas;
    public Button buttonPrefab;
    public GameObject panelToAttach;
    public List<Button> buttonList = new List<Button>();
    public TextMeshProUGUI repairHelperTextUI;
    public List<GameObject> animalImages;
    public Camera cameraToChangeBgColor;
    public List<Color> bgColors;
    string[] inv;

    // Start is called before the first frame update
    void Start()
    {
        PhotonNetwork.Instantiate("NetPlayerController", Vector3.zero, Quaternion.identity, 0) ;
    }

    public void updateInventoryButtons(string[] inv)
    {
      // instantiate enough buttons
      /* buttons are pre-instantiated
      for (var i = buttonList.Count; i < inv.Length; i++)
      {
            Button button = Instantiate(buttonPrefab, panelToAttach.transform, false);
            button.transform.position = panelToAttach.transform.position;
            Vector3 currentPos = button.transform.position;
            currentPos.y += -50 + i*60;
            button.transform.position = currentPos;
            //button.GetComponent<RectTransform>().SetParent(panelToAttach.transform);
        
            buttonList.Add(button);
      }*/

      for (var i = 0; i < inv.Length; i++)
      {
            Button button = buttonList[i];
            button.GetComponentInChildren<TextMeshProUGUI>().text = inv[i];
            int buttonIdx = i;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => {
                NetPlayerController.LocalInstance.UseItem(buttonIdx);
            });
            if (inv[i] != "")
            {
                button.gameObject.SetActive(true);
            } else
            {
                button.gameObject.SetActive(false);
            }
            
      }

    }

    // Update is called once per frame
    void Update()
    {
        if (NetPlayerController.LocalInstance)
        {
            // Set the background color
            cameraToChangeBgColor.backgroundColor = bgColors[NetPlayerController.LocalInstance.GetPlayerIndex()];

            // Pass the input
            NetPlayerController.LocalInstance.SetInput(new Vector2(joystick.Horizontal, joystick.Vertical));

            // check inventory and show buttons
            string[] netInventory = NetPlayerController.LocalInstance.GetInventory();
            string netInventoryStr = ConvertStringArrayToStringJoin(netInventory);
            updateInventoryButtons(netInventory);

            // show the character image on the screen
            if (netInventory.Length == 0)
            {
                ShowAnimalChar(NetPlayerController.LocalInstance.GetPlayerIndex());
            } else
            {
                HideAnimalChar();
            }

            // check if there are any tool tips to display
            //Debug.Log(NetPlayerController.LocalInstance.repairTypeHelperText);
            if (NetPlayerController.LocalInstance.repairTypeHelperText != "")
            {
                repairHelperTextUI.text = "Use " + NetPlayerController.LocalInstance.repairTypeHelperText;
                repairHelperTextUI.gameObject.SetActive(true);
            }
        }
        
    }

    public string ConvertStringArrayToStringJoin(string[] array)
    {
        // Use string Join to concatenate the string elements.
        string result = string.Join(".", array);
        return result;
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("LobbyClient");
    }

    private void ShowAnimalChar(int index)
    {
        animalImages[index].SetActive(true);
        for (var i = 0; i < animalImages.Count; i++)
        {
            if (i == index)
            {
                animalImages[i].SetActive(true);
            } else
            {
                animalImages[i].SetActive(false);
            }
            
        }
    }

    private void HideAnimalChar()
    {
        for (var i=0; i<animalImages.Count; i++)
        {
            animalImages[i].SetActive(false);
        }
    }
}
