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
    private int damage;
    private BoxCollider2D hitbox;

    private Vector3 direction; // Direction of movement

    void Start()
    {
        Destroy(gameObject, ability.lifespan); // Destroy automatically after lifespan
        direction = transform.right; // Default direction
    }

    void Update()
    {
        transform.Translate(direction * ability.speed * Time.deltaTime);
    }
    private void Awake()
    {
        hitbox = GetComponent<BoxCollider2D>();
        if (hitbox == null)
        {
            hitbox = gameObject.AddComponent<BoxCollider2D>();
        }
    }


    public void SetHitbox(Vector2 size, Vector3 offset)
    {
        if (hitbox != null)
        {
            hitbox.size = size;
            hitbox.offset = offset;
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        bool isVulnerable = false;

        // Check if the object is "vulnerable" by tag 
        if (other.gameObject.CompareTag("Vulnerable"))
        {
            isVulnerable = true;
        }

        // Reduce health if the object has a HealthManager or is the Boss
        if (other.gameObject.TryGetComponent(out HealthManager healthManager))
        {
            healthManager.currentHealth -= ability.damage;
            Debug.Log($"Dealt {ability.damage} damage to {other.gameObject.name}");
        }
        else if (other.gameObject.TryGetComponent(out BossController bossController))
        {
            if (bossController.CurrentHealth > 0)
            {
                if (bossController.isShielded)
                {
                    Debug.Log("Attack blocked! The boss is shielded.");
                }
                else
                {
                    bossController.TakeDamage((int)ability.damage); // Apply damage to the boss

                    if (isVulnerable && ability.damage >= 50) // Stagger on high damage
                    {
                        bossController.TriggerStagger();
                    }
                }
            }
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
