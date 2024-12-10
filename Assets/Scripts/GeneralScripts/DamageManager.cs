using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageManager : MonoBehaviour
{
    //[SerializeField] private int damage; // Damage dealt by this instance
    //[SerializeField] private float lifespan = 0.5f; // Time before destruction
    //[SerializeField] private float speed = 5f; // Movement speed for damage instances
    [SerializeField] private Ability ability;

    private Vector3 direction; // Direction of movement

    void Start()
    {
        Destroy(gameObject, ability.lifespan); // Destroy automatically after lifespan
        direction = transform.right; // Default direction
    }

    void Update()
    {
        transform.position += direction * ability.speed * Time.deltaTime;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        bool isVulnerable = false;

        // Check if the object is "vulnerable" by tag 
        if (other.gameObject.CompareTag("Vulnerable"))
        {
            isVulnerable = true;
        }

        // Reduce health if the object has a HealthManager
        if (other.gameObject.TryGetComponent(out HealthManager healthManager))
        {
            healthManager.currentHealth -= ability.damage;
            Debug.Log($"Dealt {ability.damage} damage to {other.gameObject.name}");
        }

        // Trigger recoil only if the object is vulnerable
        if (isVulnerable)
        {
            PlayerAttackManager player = FindObjectOfType<PlayerAttackManager>(); // Get reference to PlayerAttackManager
            if (player != null)
            {
                player.TriggerRecoil(ability.damage); // Trigger recoil
            }
        }

        // Destroy the damage instance on collision
        Destroy(gameObject);
    }


    public void SetDamage(int value)
    {
        ability.damage = value; // Allow damage to be set dynamically
    }

    public void SetDirection(Vector3 dir)
    {
        direction = dir; // Allow direction to be modified
    }

}
