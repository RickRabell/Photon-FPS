using UnityEngine;

public class MedKit : MonoBehaviour
{
    GameObject _player;

    [SerializeField] private int playerHealth = 100;
    [SerializeField] public int healAmount = 25;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _player = other.gameObject;
            playerHealth = _player.GetComponent<Health>().health;
            var _maxPlayerHealth = _player.GetComponent<Health>().maxHealth;

            if (playerHealth < _maxPlayerHealth && playerHealth > 0)
            {
                HealPlayer();
                Destroy(gameObject);
            }
        }
    }
    public void HealPlayer()
    {
        _player.GetComponent<Health>().health += healAmount;
        _player.GetComponent<Health>().healthText.text = _player.GetComponent<Health>().health.ToString();
    }
}
