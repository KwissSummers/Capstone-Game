using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthManager : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] public float currentHealth = 100; // Current health of the player
    [SerializeField] public float maxHealth = 100; // Maximum health of the player
    [SerializeField] public Image healthBar; // UI health bar

    [Header("Invincibility Settings")]
    [SerializeField] public float invincibilityTime = 1.0f; // Time during which the player is invincible after taking damage
    private bool isInvincible;

    private Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth; // Initialize health to max
    }

    private void Update()
    {
        // Prevent health from exceeding max or going below 0
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            HandleDeath();
        }
    }

    private void UpdateHealthUI()
    {
        if (healthBar != null)
        {
            healthBar.fillAmount = currentHealth / maxHealth;
        }
    }

    public void TakeDamage(float damage, Vector2 knockbackDirection, float knockbackForce)
    {
        if (isInvincible) return; // Ignore damage if invincible

        // Apply damage
        currentHealth -= damage;
        Debug.Log($"Player took {damage} damage. Current health: {currentHealth}");

        // Trigger invincibility
        StartCoroutine(TriggerInvincibility());

        // Apply knockback
        rb.velocity = Vector2.zero; // Reset velocity to ensure knockback is consistent
        rb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
    }

    private IEnumerator TriggerInvincibility()
    {
        isInvincible = true;
        yield return new WaitForSeconds(invincibilityTime);
        isInvincible = false;
    }

    private void HandleDeath()
    {
        Debug.Log("Player has died!");
        // Optionally trigger death animation or respawn logic
        Destroy(gameObject); // Remove the player from the scene
    }
}
