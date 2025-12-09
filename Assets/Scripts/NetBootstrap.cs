using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class NetBootstrap : MonoBehaviourPunCallbacks
{
    [SerializeField] string gameVersion = "0.1";

    // Drag HeadAvatar (cube-only) here
    [SerializeField] GameObject headAvatarPrefab;

    void Start()
    {
        // Don't connect automatically anymore
        // MenuFlowController will call StartConnection() when ready
        Debug.Log("[NetBootstrap] Ready to connect (waiting for menu choice).");
    }

    /// <summary>
    /// Called by MenuFlowController after player chooses a path
    /// </summary>
    public void StartConnection()
    {
        Debug.Log("[NetBootstrap] Start() - initializing Photon connection...");

        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.GameVersion = gameVersion;

        Debug.Log($"[NetBootstrap] Connecting using settings. GameVersion={gameVersion}, AppId={(PhotonNetwork.PhotonServerSettings != null ? PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime : "<null>")}");
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("[NetBootstrap] OnConnectedToMaster() - joining/creating room 'Room1'.");
        PhotonNetwork.JoinOrCreateRoom("Room1", new RoomOptions { MaxPlayers = 20 }, null);
    }


    public override void OnJoinedRoom()
    {
        Debug.Log($"[NetBootstrap] OnJoinedRoom() - joined '{PhotonNetwork.CurrentRoom.Name}'. PlayerCount={PhotonNetwork.CurrentRoom.PlayerCount}");

        if (!headAvatarPrefab)
        {
            Debug.LogError("[NetBootstrap] No HeadAvatar prefab assigned in NetBootstrap.");
            return;
        }

        int i = PhotonNetwork.CurrentRoom.PlayerCount - 1;
        Vector3[] spawns = {
        new Vector3(2.5f, -1f, 0f),
        new Vector3(4f, -1f, 0f),
        new Vector3(2.5f, -1f, 2.5f),
        new Vector3(4f, -1f, 2.5f),
    };
        Vector3 pos = spawns[Mathf.Clamp(i, 0, spawns.Length - 1)];

        Debug.Log($"[NetBootstrap] Spawning head avatar for player index {i} at {pos}.");

        // Spawn your network head
        GameObject myHead = PhotonNetwork.Instantiate(headAvatarPrefab.name, pos, Quaternion.identity);
        Debug.Log("[NetBootstrap] PhotonNetwork.Instantiate completed for head avatar.");

        // Snap *local rig* (parent of Main Camera) to spawn
        var pv = myHead.GetComponent<PhotonView>();
        if (pv != null && pv.IsMine)
        {
            Debug.Log("[NetBootstrap] Spawned head avatar is mine. Snapping local rig to spawn.");
            var cam = Camera.main;
            if (cam != null)
            {
                Transform rig = cam.transform.parent ? cam.transform.parent : cam.transform; // <-- use parent if present
                rig.SetPositionAndRotation(pos, Quaternion.identity);
                // (optional) point forward into the room:
                rig.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);

                Debug.Log($"[NetBootstrap] Local rig snapped to {pos}.");
            }
            else
            {
                Debug.LogWarning("[NetBootstrap] Camera.main is null when trying to snap rig.");
            }
        }
        else if (pv == null)
        {
            Debug.LogWarning("[NetBootstrap] Spawned head avatar has no PhotonView component.");
        }
        else
        {
            Debug.Log($"[NetBootstrap] Spawned head avatar is not mine (IsMine={pv.IsMine}).");
        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarning($"[NetBootstrap] OnDisconnected() - cause={cause}");
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"[NetBootstrap] OnJoinRoomFailed() - code={returnCode}, message={message}");
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"[NetBootstrap] OnCreateRoomFailed() - code={returnCode}, message={message}");
    }
}
