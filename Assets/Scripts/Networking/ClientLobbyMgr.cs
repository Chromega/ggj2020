using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using ExitGames.Client.Photon;
using Photon.Pun;

public class ClientLobbyMgr : MonoBehaviourPunCallbacks
{
    public static ClientLobbyMgr Instance { get; private set; }

    private Dictionary<string, RoomInfo> cachedRoomList;

    public Canvas clientLobbyCanvas;

    public UnityEngine.UI.Button roomButtonPrefab;
    public Transform roomButtonList;
    public Camera lobbyCamera;

    void Awake()
    {
        Instance = this;
        cachedRoomList = new Dictionary<string, RoomInfo>();

        roomButtonPrefab.gameObject.SetActive(false);
    }

    // Start is called before the first frame update
    void Start()
    {
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

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined room!");
        lobbyCamera.gameObject.SetActive(false);
        clientLobbyCanvas.gameObject.SetActive(false);
        UnityEngine.SceneManagement.SceneManager.LoadScene("PhoneScene", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }

    private void UpdateCachedRoomList(List<RoomInfo> roomList)
    {
        // list current rooms
        /*int maxRoom = 0;
        string maxRoomString = "";
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
        Debug.Log("biggest room "+maxRoom.ToString());*/

        //PhotonNetwork.JoinRandomRoom();
        //PhotonNetwork.JoinRoom(maxRoomString);

        for (int i = 0; i < roomButtonList.childCount; ++i)
        {
            if (roomButtonList.GetChild(i).gameObject == roomButtonPrefab.gameObject)
                continue;
            Destroy(roomButtonList.GetChild(i).gameObject);
        }
        foreach (RoomInfo info in roomList)
        {
            string roomName = info.Name;
            UnityEngine.UI.Button button = Instantiate(roomButtonPrefab, roomButtonList, false);
            button.gameObject.SetActive(true);
            //button.transform.SetParent(roomButtonList);
            button.GetComponentInChildren<UnityEngine.UI.Text>().text = roomName;
            button.onClick.AddListener(() => { PhotonNetwork.JoinRoom(roomName); });
        }
    }
}
