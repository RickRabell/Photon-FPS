using Unity.VisualScripting;
using UnityEngine;

public class Ammo : MonoBehaviour
{
    GameObject _player;
    Camera _playerCamera;

    [SerializeField] public int magAmmount = 1;
    [SerializeField] public int maxMag = 5;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _player = other.gameObject;
            _playerCamera = _player.GetComponentInChildren<Camera>();
            var _weapon = _playerCamera.GetComponentInChildren<Weapon>();
            //var _weapon = _player.GetComponentInChildren<Weapon>();

            if (_weapon.mag < maxMag)
            {
                AddAmmo(_weapon);
                Destroy(gameObject);
            }
        }
    }

    private void AddAmmo(Weapon weapon)
    {
        //_player.GetComponentInChildren<Weapon>().mag += magAmmount;
        //_player.GetComponentInChildren<Weapon>().magText.text = _player.GetComponent<Weapon>().mag.ToString();

        _playerCamera.GetComponentInChildren<Weapon>().mag += magAmmount;
        _playerCamera.GetComponentInChildren<Weapon>().magText.text = _playerCamera.GetComponentInChildren<Weapon>().mag.ToString();
    }
}
