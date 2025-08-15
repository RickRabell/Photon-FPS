using UnityEngine;
using Photon.Pun;

public class PlayerSetup : MonoBehaviourPun
{
    public Movement movement;
    public GameObject camera;

    public string skinPrefabName
    {
        get
        {
            return Singleton.Instance.GetPlayerSkin();
        }
    }

    void Start()
    {
        if (photonView.IsMine)
        {
            Debug.Log("Soy el jugador local, activando controles y cámara");
            movement.enabled = true;
            camera.SetActive(true);
        }
        else
        {
            Debug.Log("Jugador remoto, desactivando controles y cámara");
            movement.enabled = false;
            camera.SetActive(false);
        }

        SetupSkin();
    }

    void SetupSkin()
    {
        // Destroy previous skin if exists
        Transform skinTransform = transform.Find("Skin");
        if (skinTransform != null)
        {
            Destroy(skinTransform.gameObject);
        }

        // Instantiate skin prefab as child of the player's main camera
        GameObject skinPrefab = Resources.Load<GameObject>(skinPrefabName);
        if (skinPrefab != null)
        {
            if (camera != null)
            {
                GameObject skinInstance = Instantiate(skinPrefab, camera.transform);
                skinInstance.name = "Skin";
            }
            else
            {
                Debug.LogWarning("Player camera reference is missing.");
            }
        }
        else
        {
            Debug.LogWarning($"Skin prefab '{skinPrefabName}' not found in Resources.");
        }
    }
}
