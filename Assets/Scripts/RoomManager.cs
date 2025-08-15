using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;
using TMPro;

public class RoomManager : MonoBehaviourPunCallbacks
{
    public static RoomManager instance;

    [Header("Player Setup")]
    public GameObject player;
    public Transform spawnPoint;

    [Header("Camera")]
    public GameObject roomCam;

    [Header("Room")]
    public string roomName = "test";

    // ------------------------------------------------------------------------------------------------
    [Header("UI")]
    public GameObject victoryScreen;
    public TextMeshPro victoryText;

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
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        Debug.Log("Connected to Server");

        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();
        Debug.Log("We're in the lobby");

        PhotonNetwork.JoinOrCreateRoom(roomName, null, null);
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        Debug.Log("We're connected and in a room now!");

        if (roomCam != null) roomCam.SetActive(false);

        SpawnPlayer();
    }

    public void SpawnPlayer()
    {
        GameObject _player = PhotonNetwork.Instantiate(player.name, spawnPoint.position, Quaternion.identity);

        if (_player.GetComponent<PhotonView>().IsMine)
        {
            _player.GetComponent<Health>().isLocalPlayer = true;
        }

        // ------------------------------------------------------------------------------------------------

        alivePlayers.Add(_player);
        _player.GetComponent<Health>().onDeath += OnPlayerDeath;
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

        if (aliveCount == 1 && !gameEnded)
        {
            photonView.RPC("ShowVictoryScreen", RpcTarget.All, winnerName);
            gameEnded = true;
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
