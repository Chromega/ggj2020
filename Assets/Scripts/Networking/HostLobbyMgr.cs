using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using ExitGames.Client.Photon;
using Photon.Pun;

public class HostLobbyMgr : MonoBehaviourPunCallbacks
{
    public static HostLobbyMgr Instance { get; private set; }

    public Canvas hostLobbyCanvas;

    public Camera lobbyCamera;
    public LobbyCharacter[] lobbyCharacters;

    public bool gameStarted;

    public UnityEngine.UI.Text startText;

    void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = 1;
        if (!PhotonNetwork.IsConnected)
        {
            // hard code this since git keeps dropping it
            PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime = "66c28b24-7787-47a5-ba65-ebaf45f5c125";
            PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = "usw";

            PhotonNetwork.AutomaticallySyncScene = false;
            PhotonNetwork.LocalPlayer.NickName = Random.Range(0, 1000).ToString();
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to master");

        if (!PhotonNetwork.InLobby)
        {
            PhotonNetwork.JoinLobby();
        }
    }

    public override void OnJoinedLobby()
    {
              // generate the current time
              System.DateTime epochStart = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
              int currTime = (int)(System.DateTime.UtcNow - epochStart).TotalSeconds;
              string currTimeStr = currTime.ToString();
        //string roomName = currTimeStr;
        string roomName = SystemInfo.deviceName + currTimeStr;

              Debug.Log("Creating room "+roomName);
              RoomOptions options = new RoomOptions { MaxPlayers = 4 };
              PhotonNetwork.CreateRoom(roomName, options, null);
    }


    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log("Create room failed");
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined room!");
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        /*int idx = PhotonNetwork.CurrentRoom.PlayerCount - 2;
        if (idx < lobbyCharacters.Length && idx >= 0)
        {
            lobbyCharacters[idx].Appear();
        }*/
    }

    // Update is called once per frame
    void Update()
    {
        if (Photon.Pun.PhotonNetwork.InRoom)
        {
            if (Input.GetKeyDown(KeyCode.Space) && PhotonNetwork.CurrentRoom.PlayerCount > 1)
            {
                if (!gameStarted) {
                    StartCoroutine(StartGameCoroutine());
               }
            }
        }

        for (int i = 0; i < lobbyCharacters.Length; ++i)
        {
            if (PlayerControllerBase.sExistingControllers.ContainsKey(i))
            {
                if (!lobbyCharacters[i].GetAppeared())
                {
                    lobbyCharacters[i].Appear();
                }
            }
            else
            {
                if (lobbyCharacters[i].GetAppeared())
                {
                    lobbyCharacters[i].Hide();
                }
            }
        }

        if (PhotonNetwork.CurrentRoom!= null && PhotonNetwork.CurrentRoom.PlayerCount > 1)
        {
            startText.text = "Press SPACE to start";
        }
        else
        {
            startText.text = "Waiting for players to connect...";
        }
    }

    IEnumerator StartGameCoroutine()
    {
        gameStarted = true;
        foreach (var c in lobbyCharacters)
        {
            c.Exit();
        }
        yield return new WaitForSeconds(.5f);
        lobbyCamera.gameObject.SetActive(false);
        hostLobbyCanvas.gameObject.SetActive(false);
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainScene", UnityEngine.SceneManagement.LoadSceneMode.Single);

    }
}
