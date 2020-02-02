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

    public void RegisterPlayer(PlayerControllerBase p, bool preExisting=false)
    {
        int index = p.GetPlayerIndex();
        if (!preExisting)
        {
            index = 0;
            while (true)
            {
                if (!PlayerControllerBase.sExistingControllers.ContainsKey(index))
                    break;
                ++index;
            }
        }
        p.SetPlayerIndex(index);
        playerControllers.Add(p);
        Debug.Log(index);
        players[index].SetPlayerController(p);
        //create character for p
    }
    // Start is called before the first frame update
    void Start()
    {
        PhotonNetwork.Instantiate("HostController", Vector3.zero, Quaternion.identity, 0);

        if (!PhotonNetwork.InRoom)
        {
            //We didn't enter through the lobby, so make local players
            foreach (PlayerScript p in players) {
                LocalPlayerController lpc = Instantiate(localPlayerControllerPrefab);
                RegisterPlayer(lpc);
            }
        }

        Dictionary<int, PlayerControllerBase> dictCopy = new Dictionary<int, PlayerControllerBase>(PlayerControllerBase.sExistingControllers);
        foreach (var kvp in dictCopy)
        {
            RegisterPlayer(kvp.Value, true);
        }

        //Could make extra local players here too if lobby says we need to
    }
}
