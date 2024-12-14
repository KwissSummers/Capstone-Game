using System;
using System.Collections;
using UnityEngine;

public class DamageManager : MonoBehaviour
{
    private Ability.AbilityPhase currentPhase; // Reference to the current phase of the ability
    private BoxCollider2D hitbox;
    private Vector3 direction = Vector3.right; // Direction of movement

    private void Awake()
    {
        hitbox = GetComponent<BoxCollider2D>();
        if (hitbox == null)
        {
            hitbox = gameObject.AddComponent<BoxCollider2D>();
        }
    }

    private void Start()
    {
        if (currentPhase != null)
        {
            // Destroy automatically after the phase duration
            Destroy(gameObject, currentPhase.phaseDuration);
        }
        else
        {
            Debug.LogError("DamageManager: Current phase is not set. DamageManager will not function properly.");
        }

        //Debug.Log("manager instantiated");
    }

    private void Update()
    {
        if (currentPhase != null)
        {
            // Move the damage instance according to the phase speed
            transform.Translate(direction * currentPhase.damageAmount * Time.deltaTime);
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

    public void OnCollisionEnter2D(Collision2D other)
    {
        bool isVulnerable = false;

        // Check if the object is "vulnerable" by tag 
        if (other.gameObject.CompareTag("Vulnerable"))
        {
            isVulnerable = true;
        }

        // Apply damage if the object has a HealthManager or is the Boss
        if (other.gameObject.TryGetComponent(out HealthManager healthManager))
        {
            healthManager.currentHealth -= currentPhase.damageAmount;
            Debug.Log($"Dealt {currentPhase.damageAmount} damage to {other.gameObject.name}");
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
                    bossController.TakeDamage(currentPhase.damageAmount);

                    if (isVulnerable && currentPhase.damageAmount >= 50) // Stagger on high damage
                    {
                        bossController.TriggerStagger();
                    }
                }
            }
        }

        // Trigger recoil if the object is vulnerable
        if (isVulnerable)
        {
            PlayerAttackManager player = FindObjectOfType<PlayerAttackManager>();
            if (player != null)
            {
                player.TriggerRecoil(currentPhase.damageAmount);
            }
        }

        // Destroy the damage instance on collision
        Destroy(gameObject);
    }

    public void SetPhase(Ability.AbilityPhase phase)
    {
        currentPhase = phase; // Assign the current phase for this damage instance
    }

    public void SetDirection(Vector3 dir)
    {
        direction = dir; // Allow direction to be modified
    }

    public void ApplyRecoilToEnemy(Vector2 direction, float recoilForce)
    {
        // Check if the enemy has a Rigidbody2D to apply force to
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.AddForce(direction * recoilForce, ForceMode2D.Impulse);  // Apply recoil force
        }
    }

}
