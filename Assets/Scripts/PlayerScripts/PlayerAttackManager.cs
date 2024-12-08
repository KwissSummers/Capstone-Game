using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttackHandler : MonoBehaviour
{
    [Header("Attack Prefabs")]
    [SerializeField] private GameObject basicAttackPrefab;
    [SerializeField] private GameObject heavyAttackPrefab;
    [SerializeField] private GameObject dashAttackPrefab;
    [SerializeField] private GameObject upwardSlashPrefab;
    [SerializeField] private GameObject downwardSlashPrefab;
    [SerializeField] private GameObject rangedAttackPrefab;
    [SerializeField] private GameObject parryEffectPrefab; // Visual effect for successful parry.

    [Header("Attack Settings")]
    [SerializeField] private float attackCD = 0.5f; // Cooldown between attacks
    [SerializeField] private int parryCost = 10; // Stamina cost for parrying.
    private bool isParrying; 
    private float lastAttackTime; // Tracks the time of the last attack
    public bool isAttacking; // Tracks if the player is currently attacking

    private StaminaManager staminaManager; // Reference to the player's stamina manager

    private void Start()
    {
        // Find the player's stamina manager on the same GameObject
        staminaManager = GetComponent<StaminaManager>();
    }

    private void Update()
    {
        PerformAttack();
    }

    private void PerformAttack()
    {
        if (isAttacking || Time.time < lastAttackTime + attackCD)
            return; // Exit if on cooldown or already attacking

        if (Input.GetButtonDown("BasicAttack"))
        {
            StartCoroutine(ExecuteAttack(basicAttackPrefab, 20));
        }
        else if (Input.GetButtonDown("HeavyAttack"))
        {
            StartCoroutine(ExecuteAttack(heavyAttackPrefab, 70));
        }
        else if (Input.GetButtonDown("RangedAttack"))
        {
            StartCoroutine(ExecuteAttack(rangedAttackPrefab, 50));
        }
        else if (Input.GetButtonDown("DashAttack"))
        {
            StartCoroutine(ExecuteAttack(dashAttackPrefab, 25));
        }
        else if (Input.GetButtonDown("UpwardSlash"))
        {
            StartCoroutine(ExecuteAttack(upwardSlashPrefab, 20));
        }
        else if (Input.GetButtonDown("DownwardSlash"))
        {
            StartCoroutine(ExecuteAttack(downwardSlashPrefab, 20));
        }
        else if (Input.GetButtonDown("Parry"))
        {
            PerformParry();
        }
    }

    private IEnumerator ExecuteAttack(GameObject attackPrefab, int damage)
    {
        isAttacking = true;
        lastAttackTime = Time.time;

        // Spawn the damage instance
        Vector3 attackPosition = transform.position + new Vector3(1, 0, 0) * transform.localScale.x;
        GameObject attack = Instantiate(attackPrefab, attackPosition, Quaternion.identity);
        attack.GetComponent<DamageManager>().SetDamage(damage);

        // Wait for attack cooldown
        yield return new WaitForSeconds(attackCD);

        isAttacking = false;
    }

    private void PerformParry()
    {
        if (staminaManager.stamina < parryCost)
        {
            Debug.Log("Not enough stamina to parry.");
            return;
        }

        // Deduct stamina for parry
        staminaManager.UseStamina(parryCost);

        // Spawn a parry object in front of the player
        Vector3 parryPosition = transform.position + new Vector3(1, 0, 0) * transform.localScale.x;
        GameObject parryObject = new GameObject("ParryCollider");
        parryObject.transform.position = parryPosition;

        // Add collider and destroy after a short time
        BoxCollider2D collider = parryObject.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        Destroy(parryObject, 0.2f);

        Debug.Log("Parry performed!");
    }
}
