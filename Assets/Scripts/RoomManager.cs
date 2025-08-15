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

    // ------------------------------------------------------------------------------------------------
    [Header("UI")]
    public GameObject victoryScreen;
    public TextMeshProUGUI victoryText;

    private List<GameObject> alivePlayers = new List<GameObject>();
    private bool gameEnded = false;
    // ------------------------------------------------------------------------------------------------

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

    public override void OnJoinedRoom() => SpawnPlayers();

    public void SpawnPlayers()
    {
        /*
        GameObject _player = PhotonNetwork.Instantiate(player.name, spawnPoint.position, Quaternion.identity);

        if (_player.GetComponent<PhotonView>().IsMine)
        {
            _player.GetComponent<Health>().isLocalPlayer = true;
        }
        */
        if (spawned) return;

        Transform point = GetSpawnPoint();
        GameObject _player = PhotonNetwork.Instantiate(playerPrefabName, point.position, point.rotation);
        spawned = true;

        if (_player.GetComponent<PhotonView>().IsMine)
        {
            _player.GetComponent<Health>().isLocalPlayer = true;
        }

        // ------------------------------------------------------------------------------------------------

        alivePlayers.Add(_player);
        Debug.Log($"Player spawned: {_player.name} at {point.position} with {PhotonNetwork.LocalPlayer.ActorNumber}");
        _player.GetComponent<Health>().onDeath += OnPlayerDeath;

        roomCam.GetComponentInChildren<Canvas>().enabled = false;
    }

    public Transform GetSpawnPoint()
    {
        if(spawnPoints == null || spawnPoints.Length == 0)
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
            //victoryText.text = $"Winner: {winner}";
        }
    }
}
