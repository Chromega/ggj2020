using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using ExitGames.Client.Photon;
using Photon.Pun;

public class LobbyMgr : MonoBehaviourPunCallbacks
{
    public static LobbyMgr Instance { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = false;

        PhotonNetwork.LocalPlayer.NickName = Random.Range(0,1000).ToString();
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to master");

#if UNITY_STANDALONE
        string roomName = Random.Range(0, 1000).ToString();
        RoomOptions options = new RoomOptions { MaxPlayers = 4 };
        PhotonNetwork.CreateRoom(roomName, options, null);
#else
        PhotonNetwork.JoinRandomRoom();
#endif
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("Join random room failed, retrying");
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log("Create room failed");
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined room!");
#if UNITY_STANDALONE
        UnityEngine.SceneManagement.SceneManager.LoadScene("TestHost", UnityEngine.SceneManagement.LoadSceneMode.Additive);
#else
        UnityEngine.SceneManagement.SceneManager.LoadScene("PhoneScene", UnityEngine.SceneManagement.LoadSceneMode.Additive);
#endif
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
