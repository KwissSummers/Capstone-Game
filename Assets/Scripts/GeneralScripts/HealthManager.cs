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

    [Header("Animator")]
    [SerializeField] private Animator animator;

    [Header("Boss Health Settings")]
    [SerializeField] public Image bossHealthBar;
    [SerializeField] private BossController bossController;

    [Header("Invincibility Settings")]
    [SerializeField] public float invincibilityTime = 1.0f;
    private bool isInvincible;

    [Header("Healing Cooldown")]
    [SerializeField] private float healCooldown = 0.5f; // 0.5 seconds cooldown for healing
    private float healCooldownTimer = 0f; // Timer to track cooldown time
    public bool isHealing = false; // To check if a heal is already in progress

    [Header("Knockback Settings")]
    [SerializeField] private float knockbackDuration = 0.5f; // Duration of input disable
    private PlayerMovement playerMovement; // Reference to movement script
    private Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerMovement = GetComponent<PlayerMovement>(); // Cache PlayerMovement reference
        currentHealth = maxHealth;

        if (bossController == null)
        {
            bossController = FindObjectOfType<BossController>();
        }
    }

    private void Update()
    {
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthUI();

        if (bossController != null)
        {
            UpdateBossHealthUI();
        }

        if (currentHealth <= 0)
        {
            HandleDeath();
        }

        // Update the cooldown timer each frame
        if (healCooldownTimer > 0)
        {
            healCooldownTimer -= Time.deltaTime;
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

        StartCoroutine(HandleInterruption(knockbackDirection, knockbackForce));
    }

    private IEnumerator HandleInterruption(Vector2 knockbackDirection, float knockbackForce)
    {
        // Temporarily disable player input
        if (playerMovement != null)
        {
            playerMovement.DisableInput(knockbackDuration);
        }

        // Apply knockback
        rb.velocity = Vector2.zero; // Reset velocity
        rb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);

        // Wait for the knockback duration
        yield return new WaitForSeconds(knockbackDuration);

        // Trigger invincibility after knockback
        StartCoroutine(TriggerInvincibility());
    }

    private IEnumerator TriggerInvincibility()
    {
        isInvincible = true;
        yield return new WaitForSeconds(invincibilityTime);
        isInvincible = false;
    }

    public void Heal(float amount, StaminaManager staminaManager)
    {
        if (!isHealing && staminaManager.UseStamina(25f)) // If not healing already
        {
            currentHealth += amount;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
            Debug.Log($"Player healed for {amount}. Current health: {currentHealth}");

            isHealing = true; // Mark healing as in progress
            healCooldownTimer = healCooldown; // Start cooldown timer after healing

            // Allow healing to occur again only after cooldown
            StartCoroutine(ResetHealing());
        }
        else
        {
            Debug.Log("Not enough stamina to heal.");
        }
    }

    private IEnumerator ResetHealing()
    {
        yield return new WaitForSeconds(healCooldown);
        isHealing = false; // Reset the healing flag
    }

    private void HandleDeath()
    {
        Debug.Log("Player has died!");
        // Destroy(gameObject); // Optionally trigger death animation or respawn logic
    }

    public IEnumerator TriggerInvincibilityDuringParry()
    {
        isInvincible = true;  // Enable invincibility
        yield return new WaitForSeconds(invincibilityTime);  // Keep invincible for the parry duration
        isInvincible = false;  // Disable invincibility after the duration
    }
}
