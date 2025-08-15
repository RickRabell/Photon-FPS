using Photon.Pun;
using UnityEngine;
using UnityEngine.Audio;

public class PlayShotSound : MonoBehaviour
{
    [Header("Sound")]
    public AudioSource audioSource;
    public AudioClip fireSound;

    public void PlayShot()
    {
        if (!fireSound || !audioSource) return;

        audioSource.PlayOneShot(fireSound);

        if (GetComponent<PhotonView>().IsMine)
        {
            GetComponent<PhotonView>().RPC(nameof(RPC_PlayShot), RpcTarget.Others);
        }
    }

    [PunRPC]
    public void RPC_PlayShot()
    {
        if (fireSound && audioSource) audioSource.PlayOneShot(fireSound);
    }
}
