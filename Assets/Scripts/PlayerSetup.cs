using UnityEngine;
using Photon.Pun;

public class PlayerSetup : MonoBehaviourPun
{
    public Movement movement;
    public GameObject camera;

    void Start()
    {
        if (photonView.IsMine)
        {
            Debug.Log("Soy el jugador local, activando controles y c�mara");
            movement.enabled = true;
            camera.SetActive(true);
        }
        else
        {
            Debug.Log("Jugador remoto, desactivando controles y c�mara");
            movement.enabled = false;
            camera.SetActive(false);
        }
    }
}
