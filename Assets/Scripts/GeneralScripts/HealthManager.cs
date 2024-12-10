// HealthManager.cs

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HealthManager : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] public float currentHealth = 100;
    [SerializeField] public float maxHealth = 100;
    [SerializeField] public Image healthBar;

    [Header("Boss Health Settings")]
    [SerializeField] public Image bossHealthBar;
    [SerializeField] private BossController bossController;

    [Header("Invincibility Settings")]
    [SerializeField] public float invincibilityTime = 1.0f;
    private bool isInvincible;

    private Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;

        if (bossController == null)
        {
            bossController = FindObjectOfType<BossController>();
        }
    }

    private void Update()
    {
        // Prevent health from exceeding max or going below 0
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthUI();

        // Update boss health UI (if the boss exists)
        if (bossController != null)
        {
            UpdateBossHealthUI();
        }

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

    private void UpdateBossHealthUI()
    {
        if (bossHealthBar != null)
        {
            bossHealthBar.fillAmount = bossController.CurrentHealth / bossController.MaxHealth;
        }
    }

    public void TakeDamage(float damage, Vector2 knockbackDirection, float knockbackForce)
    {
        if (isInvincible) return;

        currentHealth -= damage;
        Debug.Log($"Player took {damage} damage. Current health: {currentHealth}");

        StartCoroutine(TriggerInvincibility());

        rb.velocity = Vector2.zero;
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
        Destroy(gameObject); // Optionally trigger death animation or respawn logic
    }
}
