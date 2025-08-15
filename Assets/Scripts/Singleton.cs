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

    private List<string> _playerNames = new List<string>();
    private const int MaxPlayers = 10;
    private string _selectedSkin;

    public bool AddPlayerName(string name)
    {
        if (_playerNames.Count >= MaxPlayers || string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        _playerNames.Add(name);
        return true;
    }

    public bool RemovePlayerName(string name)
    {
        return _playerNames.Remove(name);
    }

    public void ClearPlayerNames()
    {
        _playerNames.Clear();
    }
    
    public List<string> GetPlayerNames()
    {
        return new List<string>(_playerNames);
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
