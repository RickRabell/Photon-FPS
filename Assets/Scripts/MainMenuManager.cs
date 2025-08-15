using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private Button PlayButton;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (PlayButton != null)
        {
            PlayButton.interactable = false;
        }
        else
        {
            Debug.LogError("Lobby Button is not assigned in the inspector.");
            return;
        }

        PhotonNetwork.ConnectUsingSettings();
    }

    // Called by Photon when connection to master is successful
    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Photon Master Server");

        if (PlayButton != null)
        {
            PlayButton.interactable = true;

        }
        else
        {
            Debug.LogError("Lobby Button is not assigned in the inspector.");
            return;

        }    
    }

    // Called by Photon when connection to master fails
    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogError($"Failed to connect to Photon Master Server: {cause}");
        if (PlayButton != null)
        {
            PlayButton.interactable = false;
        }
    }

    // Function to be called when the button is pressed
    public void GoToLobby()
    {
        SceneManager.LoadScene("Lobby");
    }
}
