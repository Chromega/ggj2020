using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class HostSceneMgr : MonoBehaviour
{
    private List<PlayerControllerBase> playerControllers = new List<PlayerControllerBase>();
    public List<PlayerScript> players = new List<PlayerScript>();

    public LocalPlayerController localPlayerControllerPrefab;

    public static HostSceneMgr Instance { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    public void RegisterPlayer(PlayerControllerBase p)
    {
        int index = playerControllers.Count;
        p.SetPlayerIndex(index);
        playerControllers.Add(p);
        players[index].SetPlayerController(p);
        //create character for p
    }
    // Start is called before the first frame update
    void Start()
    {
        PhotonNetwork.Instantiate("HostController", Vector3.zero, Quaternion.identity, 0);

        if (!LobbyMgr.Instance)
        {
            //We didn't enter through the lobby, so make local players
            foreach (PlayerScript p in players) {
                LocalPlayerController lpc = Instantiate(localPlayerControllerPrefab);
                RegisterPlayer(lpc);
            }
        }

        //Could make extra local players here too if lobby says we need to
    }
}
