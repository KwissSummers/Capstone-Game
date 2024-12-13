using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttackManager : MonoBehaviour
{
    private ParryCollider parryCollider;

    [Header("Animator")]
    [SerializeField] private Animator animator;

    [Header("Abilities")]
    [SerializeField] private List<Ability> abilityList; // List of all abilities available to the player

    [Header("Recoil Settings")]
    [SerializeField] private float recoilXSpeed = 25f; // Horizontal recoil speed
    [SerializeField] private float recoilYSpeed = 25f; // Vertical recoil speed
    private bool isRecoilingX = false;
    private bool isRecoilingY = false;
    private Vector2 recoilDirection = Vector2.zero; // To store direction of recoil

    [Header("Attack Settings")]
    [SerializeField] public float attackCooldown = 0.4f; // Global cooldown between attacks
    [SerializeField] public int parryCost = 10; // Stamina cost for parrying
    [SerializeField] private Transform attackSpawnPos; // Position where attacks are spawned
    [SerializeField] public float rangedAttackCost = 25;
    private bool isAttacking;
    private float lastAttackTime; // Tracks the time of the last attack

    private PlayerMovement playerMovement;
    private StaminaManager staminaManager; // Reference to the player's stamina manager

    private void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        staminaManager = GetComponent<StaminaManager>();
        parryCollider = GetComponentInChildren<ParryCollider>(); // Or reference it directly if instantiated
 
    }

    private void Update()
    {
        PerformAttack();
        //HandlePlayerFacing(); // This is already handled in the PlayerMovement script
    }

    private void PerformAttack()
    {
        if (isAttacking || Time.time < lastAttackTime + attackCooldown)
            return;

        // Basic Attack (X)
        if (Input.GetKeyDown(KeyCode.X)) // X key for basic attack
        {
            UseAbility(0);
        }

        // Heavy Attack (Hold X)
        if (Input.GetKey(KeyCode.X) && !isAttacking && Time.time > lastAttackTime + attackCooldown)
        {
            StartCoroutine(HeavyAttack());
        }

        // Ranged Attack (F)
        if (Input.GetKeyDown(KeyCode.F)) // F key for ranged attack
        {
            if (staminaManager.stamina >= rangedAttackCost) // Check if enough stamina
            {
                UseAbility(2); // Perform ranged attack ability
                staminaManager.UseStamina(rangedAttackCost); // Deduct stamina
                Debug.Log("Ranged attack performed! Stamina used.");
            }
            else
            {
                Debug.Log("Not enough stamina for ranged attack.");
            }
        }

        // Dash Attack (C + X)
        if (Input.GetKeyDown(KeyCode.C) && Input.GetKeyDown(KeyCode.X)) // C + X
        {
            UseAbility(3);
        }

        // Downward Slash (X + Down Arrow)
        if (Input.GetKeyDown(KeyCode.X) && Input.GetKeyDown(KeyCode.DownArrow)) // X + Down Arrow
        {
            UseAbility(4);
        }

        // Parry (Space Bar)
        if (Input.GetKeyDown(KeyCode.Space)) // Space Bar for parry
        {
            PerformParry();
        }

        //unecessary atm
        /*// Upward Slash (X + Up Arrow)
        if (Input.GetKeyDown(KeyCode.X) && Input.GetKeyDown(KeyCode.UpArrow)) // X + Up Arrow
        {
            UseAbility(4);
        }*/
    }

    private void UseAbility(int index)
    {
        if (index < 0 || index >= abilityList.Count) return;

        Ability ability = abilityList[index];
        StartCoroutine(ExecuteAbility(ability));
    }

    private IEnumerator ExecuteAbility(Ability ability)
    {
        isAttacking = true;
        lastAttackTime = Time.time;

        foreach (var phase in ability.phases)
        {
            Debug.Log($"Executing Phase: {phase.phaseName}");

            // Spawn damage instance for this phase
            if (phase.damageInstancePrefab != null)
            {
                Vector3 spawnPosition = attackSpawnPos.position +
                                        new Vector3(phase.hitboxOffset.x * (playerMovement.IsFacingLeft() ? -1 : 1),
                                                    phase.hitboxOffset.y, 0);
                GameObject damageInstance = Instantiate(phase.damageInstancePrefab, spawnPosition, Quaternion.identity);

                // Configure the damage instance
                DamageManager damageManager = damageInstance.GetComponent<DamageManager>();
                if (damageManager != null)
                {
                    damageManager.SetPhase(phase);
                    damageManager.SetDirection(playerMovement.IsFacingLeft() ? Vector3.left : Vector3.right);
                }
            }

            // Wait for the duration of the current phase
            yield return new WaitForSeconds(phase.phaseDuration > 0 ? phase.phaseDuration : ability.defaultPhaseDuration);
        }

        isAttacking = false;
    }

    private IEnumerator HeavyAttack()
    {
        isAttacking = true;
        lastAttackTime = Time.time;
        yield return new WaitForSeconds(1f); // Hold for 1 second to trigger heavy attack
        UseAbility(1); // Use heavy attack ability
    }

    public void TriggerRecoil(float damage)
    {
        if (damage > 0)
        {
            ApplyRecoil(damage);
        }
    }

    private void ApplyRecoil(float damage)
    {
        // Set horizontal recoil direction based on player facing direction
        recoilDirection = new Vector2(playerMovement.IsFacingLeft() ? 0.5f : -0.5f, 0);

        // Trigger horizontal recoil
        isRecoilingX = true;
        StartCoroutine(RecoilX());

        // Trigger vertical recoil for specific damage thresholds (e.g., upward attacks)
        if (damage == 70) // Example condition for a specific attack type
        {
            isRecoilingY = true;
            StartCoroutine(RecoilY());
        }
    }

    private IEnumerator RecoilX()
    {
        float recoilTime = 0.25f; // Duration of horizontal recoil
        float elapsedTime = 0f;

        while (elapsedTime < recoilTime)
        {
            elapsedTime += Time.deltaTime;
            transform.Translate(recoilDirection.x * recoilXSpeed * Time.deltaTime, 0, 0);
            yield return null;
        }

        isRecoilingX = false;
    }

    private IEnumerator RecoilY()
    {
        float recoilTime = 0.5f; // Duration of vertical recoil
        float elapsedTime = 0f;

        while (elapsedTime < recoilTime)
        {
            elapsedTime += Time.deltaTime;
            transform.Translate(0, recoilYSpeed * Time.deltaTime, 0);
            yield return null;
        }

        isRecoilingY = false;
    }

    private void PerformParry()
    {
        if (staminaManager.stamina < parryCost)
        {
            Debug.Log("Not enough stamina to parry.");
            return;
        }

        // Trigger invincibility for the player during the parry
        HealthManager healthManager = GetComponent<HealthManager>();
        if (healthManager != null)
        {
            StartCoroutine(healthManager.TriggerInvincibilityDuringParry());
        }

        staminaManager.UseStamina(parryCost);

        // Spawn a parry object
        Vector3 parryPosition = transform.position + new Vector3(1, 0, 0) * (playerMovement.IsFacingLeft() ? -1 : 1);
        GameObject parryObject = new GameObject("ParryCollider");
        parryObject.transform.position = parryPosition;

        // Add a BoxCollider2D (trigger)
        BoxCollider2D collider = parryObject.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;

        // Attach the existing ParryCollider script
        ParryCollider parryCollider = parryObject.AddComponent<ParryCollider>();
        parryCollider.SetRecoilDirection(new Vector2(transform.localScale.x * -0.5f, 0));

        // Destroy the parry object after a short duration
        Destroy(parryObject, 0.5f);

        Debug.Log("Parry performed!");
    }

    public void TriggerParryRecoil(Vector2 direction)
    {
        recoilDirection = direction; // Use the direction provided by the ParryCollider
        isRecoilingX = true;
        StartCoroutine(RecoilX());
    }


}
