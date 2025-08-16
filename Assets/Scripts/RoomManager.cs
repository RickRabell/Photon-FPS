using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;
using TMPro;

public class RoomManager : MonoBehaviourPunCallbacks
{
    public static RoomManager instance;

    [Header("Player Setup")]
    public GameObject player;
    public Transform[] spawnPoints;
    private string playerPrefabName = "Player";
    private bool spawned = false;

    [Header("Camera")]
    public GameObject roomCam;

    [Header("Name Tag")]
    public GameObject nameTagPrefab; // Referencia a tu prefab de la etiqueta de nombre.
    public float nameTagHeightOffset = 2.5f; // Altura sobre el jugador.

    // ------------------------------------------------------------------------------------------------
    [Header("UI")]
    public GameObject victoryScreen;
    public TextMeshProUGUI victoryText;

    private List<GameObject> alivePlayers = new List<GameObject>();
    private bool gameEnded = false;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        Debug.Log("Connecting...");

        if (PhotonNetwork.InRoom && PhotonNetwork.IsConnectedAndReady)
        {
            SpawnPlayers();
        }
    }

    public void SpawnPlayers()
    {
        if (spawned) return;

        Transform point = GetSpawnPoint();
        // Pasa el nombre del skin como dato de instanciación ------------------
        string skinName = Singleton.Instance.GetPlayerSkin();
        GameObject _player = PhotonNetwork.Instantiate(playerPrefabName, point.position, point.rotation);
        spawned = true;

        if (_player.GetComponent<PhotonView>().IsMine)
        {
            _player.GetComponent<Health>().isLocalPlayer = true;

            InstantiatePlayerSkin(_player, skinName);
        }

        alivePlayers.Add(_player);
        Debug.Log($"Player spawned: {_player.name} at {point.position} with {PhotonNetwork.LocalPlayer.ActorNumber}");
        _player.GetComponent<Health>().onDeath += OnPlayerDeath;

        roomCam.GetComponentInChildren<Canvas>().enabled = false;
    }

    private void InstantiatePlayerSkin(GameObject playerObject, string skinName)
    {
        GameObject skinPrefab = Resources.Load<GameObject>(skinName);

        if (skinPrefab == null)
        {
            Debug.LogError($"Skin prefab '{skinName}' not found in Resources folder.");
            return;
        }

        // Instancia el skin en la misma posición y rotación que el jugador, pero -1 en Y
        Vector3 skinPosition = playerObject.transform.position;
        skinPosition.y -= 1f;

        GameObject skinInstance = PhotonNetwork.Instantiate(skinName, skinPosition, playerObject.transform.rotation, 0, new object[] { playerObject.GetComponent<PhotonView>().ViewID });

        // Haz que el skin sea hijo del objeto jugador en todos los clientes
        if (skinInstance != null && playerObject != null)
        {
            skinInstance.transform.SetParent(playerObject.transform, true);
            photonView.RPC("SetSkinParent", RpcTarget.AllBuffered, skinInstance.GetComponent<PhotonView>().ViewID, playerObject.GetComponent<PhotonView>().ViewID);
        }

        Debug.Log($"Skin '{skinName}' has been instantiated and parented to the player's GameObject.");
    }

    [PunRPC]
    private void SetSkinParent(int skinViewID, int playerViewID)
    {
        PhotonView skinPV = PhotonView.Find(skinViewID);
        PhotonView playerPV = PhotonView.Find(playerViewID);

        if (skinPV != null && playerPV != null)
        {
            skinPV.transform.SetParent(playerPV.transform, true);
        }
    }

    public Transform GetSpawnPoint()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("No spawn points defined!");
            return transform;
        }

        int i = (PhotonNetwork.LocalPlayer.ActorNumber - 1) % spawnPoints.Length;
        return spawnPoints[i];
    }

    private void OnPlayerDeath(GameObject deadPlayer)
    {
        alivePlayers.Remove(deadPlayer);

        if (PhotonNetwork.IsMasterClient)
        {
            CheckForWinner();
        }
    }

    public void OnDestroy()
    {
        foreach (var player in alivePlayers)
        {
            if (player != null)
            {
                var playerHealth = player.GetComponent<Health>();
                if (playerHealth != null)
                {
                    playerHealth.onDeath -= OnPlayerDeath;
                }
            }
        }
    }

    private void CheckForWinner()
    {
        int aliveCount = 0;
        string winnerName = "";

        foreach (var livePlayer in alivePlayers)
        {
            if (livePlayer != null)
            {
                var playerNickname = livePlayer.GetComponent<PhotonView>();
                if (playerNickname != null && !livePlayer.GetComponent<Health>().isDead)
                {
                    aliveCount++;
                    winnerName = playerNickname.Owner.NickName;
                }
            }
        }

        if ((aliveCount == 1) && !gameEnded)
        {
            photonView.RPC("ShowVictoryScreen", RpcTarget.All, winnerName);
            gameEnded = true;
            Debug.Log($"Game ended. Winner: {winnerName}");
        }
    }

    [PunRPC]
    private void ShowVictoryScreen(string winner)
    {
        if (victoryScreen != null && victoryText != null)
        {
            victoryScreen.SetActive(true);
        }
    }
}