using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossController : MonoBehaviour
{
    [Header("Health and States")]
    [SerializeField] public float maxHealth = 1000;
    [SerializeField] public float staggerDuration = 5.0f;
    [SerializeField] public float bossSpeed = 5.0f;
    [SerializeField] private float currentHealth;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    [SerializeField] private float criticalHealthThreshold1 = 66f; // Percentage of HP for first critical attack
    [SerializeField] private float criticalHealthThreshold2 = 33f; // Percentage of HP for second critical attack

    public bool isStaggered;
    public bool isShielded;

    [Header("Abilities")]
    [SerializeField] private List<Ability> abilityList; // Ability list for dynamic attacks
    [SerializeField] private GameObject shieldingPrefab;

    [Header("Health Thresholds")]
    [SerializeField] private float criticalHealthThreshold = 30f; // Percentage of HP for critical attack
    [SerializeField] private int criticalAttackIndex = 4; // Index of critical ability in the list

    [Header("Attack Cooldowns")]
    [SerializeField] private float attackCooldown = 2f; // Minimum cooldown between attacks
    private float lastAttackTime;

    private Transform player; // Reference to the player
    private Rigidbody2D rb;

    private void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindWithTag("Player").transform;
    }

    private void Update()
    {
        if (isStaggered)
        {
            HandleStagger();
            return;
        }

        if (isShielded)
        {
            HandleShield();
            return;
        }

        TrackPlayer();
        PerformAttackIfNeeded();
    }

    private void TrackPlayer()
    {
        // Calculate the distance and direction to the player
        Vector2 directionToPlayer = (player.position - transform.position).normalized;
        float playerDistance = Vector2.Distance(player.position, transform.position);

        // Move closer or retreat based on distance thresholds
        if (playerDistance > 5f)
        {
            // Move towards the player
            rb.velocity = new Vector2(directionToPlayer.x * bossSpeed, rb.velocity.y);
        }
        else if (playerDistance < 3f)
        {
            // Move away from the player
            rb.velocity = new Vector2(-directionToPlayer.x * bossSpeed, rb.velocity.y);
        }
        else
        {
            // Stop moving when in an acceptable range
            rb.velocity = Vector2.zero;
        }

        // Ensure facing the player
        if (directionToPlayer.x > 0 && transform.localScale.x < 0)
        {
            Flip();
        }
        else if (directionToPlayer.x < 0 && transform.localScale.x > 0)
        {
            Flip();
        }
    }

    // Flips the boss to face the player
    private void Flip()
    {
        Vector3 scale = transform.localScale;
        scale.x *= -1; // Flip the x-axis
        transform.localScale = scale;
    }


    private void PerformAttackIfNeeded()
    {
        if (Time.time < lastAttackTime + attackCooldown) return;

        float playerDistance = Vector2.Distance(player.position, transform.position);

        // HP-Based Critical Attacks (66% and 33% HP)
        if (currentHealth / maxHealth <= criticalHealthThreshold2 / 100f)
        {
            // Trigger the 33% critical attack
            UseAbility(2);
            return;
        }
        else if (currentHealth / maxHealth <= criticalHealthThreshold1 / 100f)
        {
            // Trigger the 66% critical attack
            UseAbility(2);
            return;
        }

        // Distance-Based Attacks
        if (playerDistance < 6f)
        {
            UseAbility(0); // Normal attack
            UseAbility(1); // Heavy Attack
        }
        else
        {
            UseAbility(3); // Charge Attack
            UseAbility(4); // Long-range attack
        }

        lastAttackTime = Time.time;
    }

    private void UseAbility(int index)
    {
        if (index < 0 || index >= abilityList.Count) return;

        Ability ability = abilityList[index];
        StartCoroutine(ExecuteAbility(ability));
    }

    private IEnumerator ExecuteAbility(Ability ability)
    {
        foreach (var phase in ability.phases)
        {
            //Debug.Log($"Executing Phase: {phase.phaseName}");

            if (phase.damageInstancePrefab != null)
            {
                Vector3 spawnPosition = transform.position +
                                        new Vector3(phase.hitboxOffset.x * (player.position.x < transform.position.x ? -1 : 1),
                                                    phase.hitboxOffset.y, 0);
                GameObject damageInstance = Instantiate(phase.damageInstancePrefab, spawnPosition, Quaternion.identity);

                DamageManager damageManager = damageInstance.GetComponent<DamageManager>();
                if (damageManager != null)
                {
                    damageManager.SetPhase(phase);
                    damageManager.SetDirection(player.position.x < transform.position.x ? Vector3.left : Vector3.right);
                }
            }

            yield return new WaitForSeconds(phase.phaseDuration > 0 ? phase.phaseDuration : ability.defaultPhaseDuration);
        }
    }

    private void HandleShield()
    {
        if (!isShielded)
        {
            isShielded = true; // Mark the boss as shielded
            PlayShieldAnimation(); // Play shield animation
            StartCoroutine(DisableShield()); // Start the countdown to disable the shield
        }
    }
    private void PlayShieldAnimation()
    {
        // Get the animator and play the shield animation (assuming it's named "RaiseShield")
        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.Play("RaiseShield"); // Replace with your actual shield animation name
        }
    }


    private IEnumerator DisableShield()
    {
        // Make the boss invincible
        SetInvincible(true);

        // Wait for the shield duration (e.g., 5 seconds)
        yield return new WaitForSeconds(5f);

        // Make the boss vulnerable again
        SetInvincible(false);
        isShielded = false; // Shield is disabled
    }

    private void SetInvincible(bool invincible)
    {
        // Set invincibility flag to prevent or allow damage
        isShielded = invincible;

        // If invincible, prevent damage from being applied
        if (invincible)
        {
            Debug.Log("Boss is now invincible.");
        }
        else
        {
            Debug.Log("Boss is now vulnerable.");
        }
    }

    private void HandleStagger()
    {
        StartCoroutine(DisableStagger());
    }

    private IEnumerator DisableStagger()
    {
        yield return new WaitForSeconds(staggerDuration);
        isStaggered = false;
    }

    public void TakeDamage(int damage)
    {
        if (isShielded)
        {
            Debug.Log("Boss is shielded. No damage taken.");
            return;
        }

        currentHealth -= damage;
        Debug.Log($"Boss took {damage} damage. Current health: {currentHealth}");

        if (currentHealth <= 0)
        {
            Debug.Log("Boss defeated!");
            // Play death animation here

            Destroy(gameObject);
            return;
        }

        if (currentHealth <= maxHealth * 0.5f && !isStaggered)
        {
            TriggerStagger();
        }

        if ((currentHealth <= maxHealth * 0.75f || currentHealth <= maxHealth * 0.25f) && !isShielded)
        {
            TriggerShield();
        }
    }

    public void TriggerStagger()
    {
        if (!isStaggered)
        {
            isStaggered = true;
            Debug.Log("Boss is staggered!");
        }
    }

    public void TriggerShield()
    {
        isShielded = true;
        GameObject shield = Instantiate(shieldingPrefab, transform.position + transform.right, Quaternion.identity);
        Destroy(shield, 5);
        Debug.Log("Boss is shielded!");
    }

    public void ApplyParrySpeedReduction()
    {
        bossSpeed /= 2;
        StartCoroutine(RecoverSpeed(5f));
        Debug.Log("Boss speed reduced to: " + bossSpeed);
    }

    private IEnumerator RecoverSpeed(float delay)
    {
        yield return new WaitForSeconds(delay);
        bossSpeed *= 2;
        Debug.Log("Boss speed restored to: " + bossSpeed);
    }
}
