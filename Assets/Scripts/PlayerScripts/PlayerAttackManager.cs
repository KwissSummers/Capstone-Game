using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttackManager : MonoBehaviour
{
    [Header("Abilities")]
    [SerializeField] private List<Ability> abilityList; // List of all abilities available to the player

    [Header("Recoil Settings")]
    [SerializeField] private float recoilXSpeed = 25f; // Horizontal recoil speed
    [SerializeField] private float recoilYSpeed = 25f; // Vertical recoil speed
    private bool isRecoilingX = false;
    private bool isRecoilingY = false;
    private Vector2 recoilDirection = Vector2.zero; // To store direction of recoil

    [Header("Attack Settings")]
    [SerializeField] private float attackCooldown = 0.5f; // Cooldown between attacks
    [SerializeField] private int parryCost = 10; // Stamina cost for parrying
    [SerializeField] private Transform attackSpawnPos; // Position where attacks are spawned
    private bool isAttacking;
    private float lastAttackTime; // Tracks the time of the last attack

    private PlayerMovement playerMovement;
    private StaminaManager staminaManager; // Reference to the player's stamina manager

    private void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        staminaManager = GetComponent<StaminaManager>();
    }

    private void Update()
    {
        PerformAttack();
    }

    private void PerformAttack()
    {
        if (isAttacking || Time.time < lastAttackTime + attackCooldown)
            return;

        if (Input.GetKeyDown(KeyCode.Mouse0)) // Example input for ability 0
        {
            UseAbility(0);
        }
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
                                        new Vector3(phase.hitboxOffset.x * (playerMovement.facingLeft ? -1 : 1), 
                                                    phase.hitboxOffset.y, 0);
                GameObject damageInstance = Instantiate(phase.damageInstancePrefab, spawnPosition, Quaternion.identity);

                // Configure the damage instance
                DamageManager damageManager = damageInstance.GetComponent<DamageManager>();
                if (damageManager != null)
                {
                    damageManager.SetPhase(phase);
                    damageManager.SetDirection(playerMovement.facingLeft ? Vector3.left : Vector3.right);
                }
            }

            // Wait for the duration of the current phase
            yield return new WaitForSeconds(phase.phaseDuration > 0 ? phase.phaseDuration : ability.defaultPhaseDuration);
        }

        isAttacking = false;
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
        recoilDirection = new Vector2(playerMovement.facingLeft ? 0.5f : -0.5f, 0);

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

        staminaManager.UseStamina(parryCost);

        // Spawn a parry object
        Vector3 parryPosition = transform.position + new Vector3(1, 0, 0) * transform.localScale.x;
        GameObject parryObject = new GameObject("ParryCollider");
        parryObject.transform.position = parryPosition;

        BoxCollider2D collider = parryObject.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        Destroy(parryObject, 0.2f);

        ApplyParryRecoil();
        Debug.Log("Parry performed!");
    }

    private void ApplyParryRecoil()
    {
        recoilDirection = new Vector2(transform.localScale.x * -0.5f, 0);
        isRecoilingX = true;
        StartCoroutine(RecoilX());
    }
}
