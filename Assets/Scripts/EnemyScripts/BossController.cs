using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossController : MonoBehaviour
{
    [Header("Health and States")]
    [SerializeField] public float maxHealth = 1000;
    [SerializeField] public float staggerDuration = 3.0f;
    [SerializeField] public float bossSpeed = 5.0f;
    [SerializeField] private float currentHealth;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;

    public bool isStaggered;
    public bool isShielded;

    [Header("Abilities")]
    [SerializeField] private List<Ability> abilityList;
    [SerializeField] private GameObject shieldingPrefab;

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
        float playerDistance = Vector2.Distance(player.position, transform.position);

        // Move closer or retreat based on distance and attack cooldowns
        if (playerDistance > 5f)
        {
            rb.velocity = new Vector2(bossSpeed, rb.velocity.y);
        }
        else if (playerDistance < 3f)
        {
            rb.velocity = new Vector2(-bossSpeed, rb.velocity.y);
        }
        else
        {
            rb.velocity = Vector2.zero;
        }
    }

    private void PerformAttackIfNeeded()
    {
        if (Time.time < lastAttackTime + attackCooldown) return;

        float playerDistance = Vector2.Distance(player.position, transform.position);

        if (playerDistance < 3f)
        {
            PerformNormalAttack(abilityList[0]);
        }
        else if (playerDistance < 6f)
        {
            PerformHeavyAttack(abilityList[0]);
        }
        else
        {
            PerformRangedAttack(abilityList[0]);
        }
    }

    private void PerformNormalAttack(Ability ability)
    {
        Ability.AbilityPhase phase = ability.phases[0];
        StartAttackCoroutine(phase);
    }

    private void PerformHeavyAttack(Ability ability)
    {
        Ability.AbilityPhase phase = ability.phases[1];
        StartAttackCoroutine(phase);
    }

    private void PerformRangedAttack(Ability ability)
    {
        Ability.AbilityPhase phase = ability.phases[2];
        StartAttackCoroutine(phase);
    }

    private void StartAttackCoroutine(Ability.AbilityPhase phase)
    {
        if (phase.damageInstancePrefab == null) return;
        StartCoroutine(ExecuteAttack(phase.damageInstancePrefab, phase, phase.phaseName));
    }


    private void HandleShield()
    {
        // Face the player
        transform.localScale = new Vector3(player.position.x > transform.position.x ? 3 : -3, 5, 0);

        // Timer to disable shield after 5 seconds
        StartCoroutine(DisableShield());
    }

    private IEnumerator DisableShield()
    {
        yield return new WaitForSeconds(5);
        isShielded = false;
    }

    private void HandleStagger()
    {
        // Remain staggered for the set duration
        StartCoroutine(DisableStagger());
    }

    private IEnumerator DisableStagger()
    {
        yield return new WaitForSeconds(staggerDuration);
        isStaggered = false;
    }

    private IEnumerator ExecuteAttack(GameObject attackPrefab, Ability.AbilityPhase phase, string attackName)
    {
        lastAttackTime = Time.time;
        Debug.Log($"Boss performing {attackName}");

        // Lock the boss in place during the attack
        rb.velocity = Vector2.zero;

        // Instantiate the attack prefab
        Vector3 attackPosition = transform.position + transform.right;

        GameObject attack = null;
        Vector3 distance = (player.position - transform.position).normalized;
        if (Vector3.Dot(distance, transform.right.normalized) < 0)
        {
            attack = Instantiate(attackPrefab, attackPosition, Quaternion.AngleAxis(90, new Vector3(0,0,1)));
        }
        else
        {
            attack = Instantiate(attackPrefab, attackPosition, Quaternion.identity);
        }

        // Configure the damage instance with the given phase
        DamageManager damageManager = attack.GetComponent<DamageManager>();
        if (damageManager != null)
        {
            damageManager.SetPhase(phase); // Pass the entire AbilityPhase
        }

        // Wait for the attack animation to complete
        yield return new WaitForSeconds(phase.phaseDuration > 0 ? phase.phaseDuration : 1);

        Debug.Log($"{attackName} completed");
    }


    public void TakeDamage(int damage)
    {
        if (isShielded)
        {
            Debug.Log("Boss is shielded. No damage taken.");
            return; // Ignore damage if shielded
        }

        currentHealth -= damage;
        Debug.Log($"Boss took {damage} damage. Current health: {currentHealth}");

        // Play damage or stagger animations here (optional)
        // animator.SetTrigger("Damage"); // Example: trigger damage animation

        if (currentHealth <= 0)
        {
            Debug.Log("Boss defeated!");
            Destroy(gameObject); // Destroy the boss if health is 0 or below
            return;
        }

        if (currentHealth <= maxHealth * 0.5f && !isStaggered)
        {
            TriggerStagger(); // Trigger stagger if health is below 50%
        }

        // Trigger shield at specific health thresholds
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

            // Optionally trigger a stagger animation
            // animator.SetTrigger("Stagger");
        }
    }

    public void TriggerShield()
    {
        isShielded = true;
        GameObject shield = Instantiate(shieldingPrefab, transform.position + transform.right, Quaternion.identity);
        Destroy(shield, 5); // Shield lasts 5 seconds
        Debug.Log("Boss is shielded!");
    }

    public void ApplyParrySpeedReduction()
    {
        bossSpeed /= 2;
        StartCoroutine(RecoverSpeed(5f)); // Speed will recover after 5 seconds
        Debug.Log("Boss speed reduced to: " + bossSpeed);
    }


    private IEnumerator RecoverSpeed(float delay)
    {
        yield return new WaitForSeconds(delay);
        bossSpeed *= 2;  // Restore the speed back to normal
        Debug.Log("Boss speed restored to: " + bossSpeed);
    }



}
