using Photon.Pun;
using TMPro;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public int damage;

    public Camera camera;

    public float fireRate;

    [Header("VFX")]
    public GameObject hitVFX;

    private float nextFire;

    [Header("Ammo")]
    public int mag = 5;

    public int ammo = 30;
    public int magAmmo = 30;

    [Header("UI")]
    public TextMeshProUGUI magText;
    public TextMeshProUGUI ammoText;

    [Header("Animation")]
    public Animation animation;
    public AnimationClip reload;

    //[Header("Sound")]
    //public AudioSource audioSource;
    //public AudioClip fireSound;


    void Start()
    {
        magText.text = mag.ToString();
        ammoText.text = ammo + "/" + magAmmo;
    }

    // Update is called once per frame
    void Update()
    {
        if (nextFire > 0)
            nextFire -= Time.deltaTime;

        if (Input.GetButton("Fire1") && nextFire <= 0 && ammo > 0 && animation.isPlaying == false )
        {
            nextFire = 1/ fireRate;

            ammo--;

            magText.text = mag.ToString();
            ammoText.text = ammo + "/" + magAmmo;

            Fire();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            Reload();
        }
    }

    void Reload()
    {
        animation.Play(reload.name);

        if(mag >0)
        {
            mag--;

            ammo = magAmmo;
        }

        magText.text = mag.ToString();
        ammoText.text = ammo + "/" + magAmmo;
    }


    void Fire()
    {
        Ray ray = new Ray(origin: camera.transform.position, direction: camera.transform.forward);
        PlayShotSound playShotSound = GetComponent<PlayShotSound>();

        RaycastHit hit;

        if (Physics.Raycast(ray.origin, ray.direction, out hit, maxDistance: 100f))
        {
            PhotonNetwork.Instantiate(hitVFX.name, hit.point, Quaternion.identity);


            if(hit.transform.gameObject.GetComponent<Health>())
            {
                hit.transform.gameObject.GetComponent<PhotonView>().RPC(methodName: "TakeDamage", RpcTarget.All, damage);
            }
        }

        if (playShotSound != null)
        {
            playShotSound.PlayShot();
            // O para llamar el RPC directamente:
            playShotSound.GetComponent<PhotonView>().RPC("RPC_PlayShot", RpcTarget.All);
        }
    }

    /*
    public void PlayShot()
    {
        if(!fireSound || !audioSource) return;

        audioSource.PlayOneShot(fireSound);

        if(GetComponent<PhotonView>().IsMine)
        {
            GetComponent<PhotonView>().RPC(nameof(RPC_PlayShot), RpcTarget.Others);
        }
    }

    [PunRPC]
    public void RPC_PlayShot()
    {
        if (fireSound && audioSource) audioSource.PlayOneShot(fireSound);
    }
    */
}
