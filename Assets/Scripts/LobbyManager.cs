using Photon.Pun;
using Photon.Pun.Demo.Cockpit;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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
    private List<TMP_Text> playerNameTexts; // List of TMP_Text elements to display player names

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

    private void Start()
    {
        PhotonNetwork.JoinLobby();
        playButton.interactable = false;
    }

    private void ShowNamePanel()
    {
        if (nameCanvas == null)
        {
            Debug.LogError("Name Canvas reference not set in LobbyManager.");
            return;
        }
        else
        {
            nameCanvas.gameObject.SetActive(true);
        }
    }

    public void OnConfirmName()
    {
        string playerName = nameInput.text;
        if (!string.IsNullOrEmpty(playerName))
        {
            Singleton.Instance.AddPlayerName(playerName);
            nameCanvas.gameObject.SetActive(false);

            // Get the updated player names list from the singleton
            List<string> playerNames = Singleton.Instance.GetPlayerNames();

            // Assign the newly added name to the next empty TMP_Text element
            for (int i = 0; i < playerNameTexts.Count; i++)
            {
                if (string.IsNullOrEmpty(playerNameTexts[i].text))
                {
                    playerNameTexts[i].text = playerNames[playerNames.Count - 1];
                    break;
                }
            }
        }
    }

    public void OnSelectSkin(string skinName)
    {
        Singleton.Instance.SetPlayerSkin(skinName);
        playButton.interactable = true;
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Joined Lobby");
        ShowNamePanel();
    }

    public override void OnLeftLobby()
    {
        Debug.Log("Disconnected");
    }

    public void PlayGame()
    {
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 10;
        roomOptions.IsVisible = true;
        roomOptions.IsOpen = true;
        roomOptions.PublishUserId = true;
        PhotonNetwork.JoinOrCreateRoom("GameRoom", roomOptions, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined Room");
        PhotonNetwork.LoadLevel("SampleScene");
    }

    // Called when the player fails to join a room
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"Failed to join room: {message}");
    }
}