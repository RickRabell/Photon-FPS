using Photon.Pun;
using UnityEngine;
using System.Collections.Generic;

public class Singleton
{
    private static Singleton _instance;
    public static Singleton Instance
    {
        get
        {
            if (_instance == null)
                _instance = new Singleton();
            return _instance;
        }
    }

    // NOTE: The player name list logic is removed as Photon handles this.
    // We only keep the skin-related logic here.

    private string localPlayerName;
    private string _selectedSkin;

    public void SetLocalPlayerName(string name)
    {
        PhotonNetwork.NickName = name;
        this.localPlayerName = name;
    }

    public string GetLocalPlayerName()
    {
        return PhotonNetwork.NickName;
    }

    public void SetPlayerSkin(string skin)
    {
        _selectedSkin = skin;
    }

    public string GetPlayerSkin()
    {
        return _selectedSkin;
    }
}