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
    public GameObject nameTagPrefab;
    public float nameTagHeightOffset = 2.5f;

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

    // Only one SpawnPlayers method, using RPC for skin instantiation
    public void SpawnPlayers()
    {
        if (spawned) return;

        Transform point = GetSpawnPoint();
        string skinName = Singleton.Instance.GetPlayerSkin();
        object[] instantiationData = new object[] { skinName };
        GameObject _player = PhotonNetwork.Instantiate(playerPrefabName, point.position, point.rotation, 0, instantiationData);
        spawned = true;

        if (_player.GetComponent<PhotonView>().IsMine)
        {
            _player.GetComponent<Health>().isLocalPlayer = true;

            GameObject skinPrefab = Resources.Load<GameObject>(skinName);
            if (skinPrefab != null)
            {
                GameObject skinInstance = Instantiate(skinPrefab, _player.transform);
                skinInstance.transform.localPosition = new Vector3(0f, -1f, 0f);
                skinInstance.transform.localRotation = Quaternion.identity;
            }
        }

        alivePlayers.Add(_player);
        Debug.Log($"Player spawned: {_player.name} at {point.position} with {PhotonNetwork.LocalPlayer.ActorNumber}");
        _player.GetComponent<Health>().onDeath += OnPlayerDeath;

        // Use RPC to instantiate skin for all clients
        photonView.RPC("InstantiatePlayerSkinRPC", RpcTarget.AllBuffered, _player.GetComponent<PhotonView>().ViewID, skinName);

        // Use RPC to instantiate name tag for all clients
        photonView.RPC("SpawnNameTag", RpcTarget.AllBuffered, _player.GetComponent<PhotonView>().ViewID, PhotonNetwork.LocalPlayer.NickName);

        roomCam.GetComponentInChildren<Canvas>().enabled = false;
    }

    [PunRPC]
    private void SpawnNameTag(int playerViewID, string playerName)
    {
        PhotonView playerView = PhotonView.Find(playerViewID);
        if (playerView == null) return;

        GameObject nameTagInstance = Instantiate(nameTagPrefab, playerView.transform);
        nameTagInstance.transform.localPosition = new Vector3(0, nameTagHeightOffset, 0);

        TextMeshProUGUI nameText = nameTagInstance.GetComponentInChildren<TextMeshProUGUI>();
        if (nameText != null)
        {
            nameText.text = playerName;
        }
    }

    // RPC for skin instantiation
    [PunRPC]
    private void InstantiatePlayerSkinRPC(int playerViewID, string skinName)
    {
        PhotonView playerView = PhotonView.Find(playerViewID);
        if (playerView == null) return;

        if (string.IsNullOrEmpty(skinName))
        {
            Debug.LogWarning("No skin selected from the lobby.");
            return;
        }

        GameObject skinPrefab = Resources.Load<GameObject>(skinName);

        if (skinPrefab == null)
        {
            Debug.LogError($"Skin prefab '{skinName}' not found in Resources folder.");
            return;
        }

        GameObject skinInstance = Instantiate(skinPrefab, playerView.transform);
        skinInstance.transform.localPosition = new Vector3(0f, -1f, 0f);
        skinInstance.transform.localRotation = Quaternion.identity;

        Debug.Log($"Skin '{skinName}' has been instantiated on the player object with an offset.");
    }

    // Remove local skin instantiation, only use RPC
    // private void InstantiatePlayerSkin(GameObject playerObject) { ... } // REMOVE THIS METHOD

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
            victoryText.text = $"Winner: {winner}";
        }
    }
}