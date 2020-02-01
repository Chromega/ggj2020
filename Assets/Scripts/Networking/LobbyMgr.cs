using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using ExitGames.Client.Photon;
using Photon.Pun;

public class LobbyMgr : MonoBehaviourPunCallbacks
{
    public static LobbyMgr Instance { get; private set; }

    private Dictionary<string, RoomInfo> cachedRoomList;

    void Awake()
    {
        Instance = this;
        cachedRoomList = new Dictionary<string, RoomInfo>();
    }

    // Start is called before the first frame update
    void Start()
    {
        // hard code this since git keeps dropping it
        PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime = "66c28b24-7787-47a5-ba65-ebaf45f5c125";

        PhotonNetwork.AutomaticallySyncScene = false;
        PhotonNetwork.LocalPlayer.NickName = Random.Range(0,1000).ToString();
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        UpdateCachedRoomList(roomList);
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
      #if UNITY_STANDALONE
              // generate the current time
              System.DateTime epochStart = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
              int currTime = (int)(System.DateTime.UtcNow - epochStart).TotalSeconds;
              string currTimeStr = currTime.ToString();
              string roomName = currTimeStr;

              Debug.Log("Creating room "+roomName);
              RoomOptions options = new RoomOptions { MaxPlayers = 4 };
              PhotonNetwork.CreateRoom(roomName, options, null);
      #endif
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("Join random room failed, retrying");
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log("Joining the room failed, retrying a random room");
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
        //UnityEngine.SceneManagement.SceneManager.LoadScene("TestHost", UnityEngine.SceneManagement.LoadSceneMode.Additive);
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainScene", UnityEngine.SceneManagement.LoadSceneMode.Additive);
#else
        UnityEngine.SceneManagement.SceneManager.LoadScene("PhoneScene", UnityEngine.SceneManagement.LoadSceneMode.Additive);
#endif
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void UpdateCachedRoomList(List<RoomInfo> roomList)
    {
#if UNITY_STANDALONE
#else
        // list current rooms
        int maxRoom = 0;
        string maxRoomString = "0";
        Debug.Log("Current rooms open:");
        foreach (RoomInfo info in roomList)
        {
            int roomNameInt = int.Parse(info.Name);
            if (roomNameInt > maxRoom) {
                maxRoom = roomNameInt;
                maxRoomString = info.Name;
            }
            Debug.Log(info.Name);
        }
        Debug.Log("biggest room "+maxRoom.ToString());

        //PhotonNetwork.JoinRandomRoom();
        PhotonNetwork.JoinRoom(maxRoomString);
#endif
    }
}
