using JetBrains.Annotations;
using Photon.Pun;
using System;
using TMPro;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] public int health = 100;
    [SerializeField] public int maxHealth = 100;

    public bool isDead { get; private set; } = false;

    public bool isLocalPlayer = false;

    [Header("UI")]
    public TextMeshProUGUI healthText;

    public event Action<GameObject> onDeath;

    /*
    public bool isDeath()
    {
        return health <= 0;
    }

    public void onDeath()
    {
        if (isLocalPlayer)
        {
            // RoomManager.instance.SpawnPlayer();
        }
        Destroy(gameObject);
    }
    */

    [PunRPC]
    public void TakeDamage(int _damage)
    {
        if (isDead) return;

        health -= _damage;

        healthText.text = health.ToString();

        if (health <= 0)
        {
            Die();
        }
    }

    public void Die()
    { 
        if (isDead) return;
        isDead = true;
        //Destroy(gameObject);
        onDeath?.Invoke(gameObject);

        // En Health.cs
        void OnDestroy()
        {
            if (RoomManager.instance != null)
            {
                RoomManager.instance.OnDestroy();
            }
        }
        Destroy(gameObject);
    }
}
