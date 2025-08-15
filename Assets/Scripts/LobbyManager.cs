using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private Canvas nameCanvas;

    [SerializeField]
    private TMP_InputField nameInput;

    [SerializeField]
    private Button confirmButton;

    [SerializeField]
    private List<TMP_Text> playerNameTexts;

    [SerializeField]
    private Button skinButton1;
    [SerializeField]
    private Button skinButton2;
    [SerializeField]
    private Button skinButton3;
    [SerializeField]
    private Button skinButton4;

    [SerializeField]
    private Button playButton;

    private string sceneName;

    [SerializeField]
    private GameObject skinView;

    private bool isLeavingForGame = false;

    private void Start()
    {
        PhotonNetwork.JoinLobby();

        playButton.interactable = false;
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Master Server.");
        if (!isLeavingForGame)
        {
            PhotonNetwork.JoinLobby();
        }
        else
        {
            RoomOptions roomOptions = new RoomOptions()
            {
                MaxPlayers = 10,
                IsVisible = true,
                IsOpen = true
            };
            PhotonNetwork.JoinOrCreateRoom("GameRoom", roomOptions, TypedLobby.Default);
            isLeavingForGame = false;
        }
    }

    private void ShowNamePanel()
    {
        if (nameCanvas != null)
        {
            nameCanvas.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogError("Name Canvas reference not set in LobbyManager.");
        }
    }

    public void OnConfirmName()
    {
        string playerName = nameInput.text;
        if (!string.IsNullOrEmpty(playerName) && Singleton.Instance != null)
        {
            Singleton.Instance.SetLocalPlayerName(playerName);
            nameCanvas.gameObject.SetActive(false);
            UpdatePlayerNameTexts();
        }
    }

    public void OnSelectSkin(string skinName)
    {
        playButton.interactable = true;

        if (skinView != null)
        {
            for (int i = skinView.transform.childCount - 1; i >= 0; i--)
            {
                Destroy(skinView.transform.GetChild(i).gameObject);
            }
        }

        GameObject skinPrefab = Resources.Load<GameObject>(skinName);
        if (skinPrefab != null)
        {
            GameObject skinInstance = Instantiate(skinPrefab, skinView.transform);
            skinInstance.transform.localPosition = Vector3.zero;
            skinInstance.transform.localEulerAngles = Vector3.zero;
        }
        else
        {
            Debug.LogWarning($"Skin prefab '{skinName}' not found in Resources.");
        }

        // Call SetPlayerSkin on Singleton
        if (Singleton.Instance != null)
        {
            Singleton.Instance.SetPlayerSkin(skinName);
        }
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Joined Lobby");
        ShowNamePanel();

        PhotonNetwork.JoinOrCreateRoom("LobbyRoom", new RoomOptions() { MaxPlayers = 10, IsVisible = true, IsOpen = true }, TypedLobby.Default);
    }

    public override void OnLeftLobby()
    {
        Debug.Log("Disconnected from Lobby");
        ClearPlayerNameTexts();
    }

    public void PlayGame(string SceneName)
    {
        sceneName = SceneName;
        isLeavingForGame = true;
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        Debug.Log("Left the current room. Attempting to rejoin Master Server.");
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined Room: " + PhotonNetwork.CurrentRoom.Name);

        if (PhotonNetwork.CurrentRoom.Name == "LobbyRoom")
        {
            UpdatePlayerNameTexts();
        }
        else if (PhotonNetwork.CurrentRoom.Name == "GameRoom")
        {
            PhotonNetwork.LoadLevel(sceneName);
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdatePlayerNameTexts();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdatePlayerNameTexts();
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"Failed to join room: {message}");
        isLeavingForGame = false;
        PhotonNetwork.JoinLobby();
    }

    private void UpdatePlayerNameTexts()
    {
        for (int i = 0; i < playerNameTexts.Count; i++)
        {
            playerNameTexts[i].text = "";
        }

        Player[] players = PhotonNetwork.PlayerList;
        for (int i = 0; i < players.Length && i < playerNameTexts.Count; i++)
        {
            playerNameTexts[i].text = players[i].NickName;
        }
    }

    private void ClearPlayerNameTexts()
    {
        for (int i = 0; i < playerNameTexts.Count; i++)
        {
            playerNameTexts[i].text = "";
        }
    }
}