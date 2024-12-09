using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class PlayerAttackManager : MonoBehaviour
{
    /*
    [Header("Attack Prefabs")]
    [SerializeField] private GameObject basicAttackPrefab;
    [SerializeField] private GameObject heavyAttackPrefab;
    [SerializeField] private GameObject dashAttackPrefab;
    [SerializeField] private GameObject upwardSlashPrefab;
    [SerializeField] private GameObject downwardSlashPrefab;
    [SerializeField] private GameObject rangedAttackPrefab;
    [SerializeField] private GameObject parryEffectPrefab; // Visual effect for successful parry.
    */

    //[Header("Attack Objects")]
    [SerializeField] private List<GameObject> abilityList; // Add all your attack objects to this list

    // Recoil variables
    [SerializeField] private float recoilXSpeed = 45f; // Horizontal recoil speed
    [SerializeField] private float recoilYSpeed = 45f; // Vertical recoil speed
    private bool isRecoilingX = false;
    private bool isRecoilingY = false;
    private Vector2 recoilDirection = Vector2.zero; // To store direction of recoil

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

        
        // GetKeyDown is for when a key is pressed
        // GetKey is for when a key is held
        if (Input.GetKeyDown(KeyCode.G)) // Set to actual input
        {
            Instantiate(abilityList[0]);
            //StartCoroutine(ExecuteAttack(basicAttackPrefab, 20));
        }
        else if (Input.GetButtonDown("HeavyAttack"))
        {
            //StartCoroutine(ExecuteAttack(heavyAttackPrefab, 70));
        }
        else if (Input.GetButtonDown("RangedAttack"))
        {
            //StartCoroutine(ExecuteAttack(rangedAttackPrefab, 50));
        }
        else if (Input.GetButtonDown("DashAttack"))
        {
            //StartCoroutine(ExecuteAttack(dashAttackPrefab, 25));
        }
        else if (Input.GetButtonDown("UpwardSlash"))
        {
            //StartCoroutine(ExecuteAttack(upwardSlashPrefab, 20));
        }
        else if (Input.GetButtonDown("DownwardSlash"))
        {
            //StartCoroutine(ExecuteAttack(downwardSlashPrefab, 20));
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

        // Apply recoil after attack based on attack type
        ApplyRecoil(damage);

        // Wait for attack cooldown
        yield return new WaitForSeconds(attackCD);

        isAttacking = false;
    }

    private void ApplyRecoil(int attackDamage)
    {
        // Apply recoil direction based on attack type (horizontal or vertical)
        if (attackDamage >= 50) // For heavy attacks, apply a strong recoil
        {
            recoilDirection = new Vector2(transform.localScale.x * -1, 0); // Horizontal recoil
        }
        else if (attackDamage < 50 && attackDamage >= 20) // For lighter attacks
        {
            recoilDirection = new Vector2(transform.localScale.x * -0.5f, 0); // Lighter horizontal recoil
        }

        // Activate recoil effects
        isRecoilingX = true;
        StartCoroutine(RecoilX());

        // If it's a vertical attack, apply vertical recoil
        if (attackDamage == 70) // Example for up attack
        {
            isRecoilingY = true;
            StartCoroutine(RecoilY());
        }
    }

    private IEnumerator RecoilX()
    {
        float recoilTime = 0.5f; // How long the recoil lasts
        float elapsedTime = 0f;

        while (elapsedTime < recoilTime)
        {
            elapsedTime += Time.deltaTime;
            // Apply recoil speed in the X direction
            transform.Translate(recoilDirection.x * recoilXSpeed * Time.deltaTime, 0, 0);
            yield return null;
        }

        isRecoilingX = false; // Stop horizontal recoil
    }

    private IEnumerator RecoilY()
    {
        float recoilTime = 0.5f; // How long the recoil lasts
        float elapsedTime = 0f;

        while (elapsedTime < recoilTime)
        {
            elapsedTime += Time.deltaTime;
            // Apply recoil speed in the Y direction
            transform.Translate(0, recoilYSpeed * Time.deltaTime, 0);
            yield return null;
        }

        isRecoilingY = false; // Stop vertical recoil
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

        // Apply recoil on successful parry
        ApplyParryRecoil();

        Debug.Log("Parry performed!");
    }

    private void ApplyParryRecoil()
    {
        // Apply recoil direction after parry
        recoilDirection = new Vector2(transform.localScale.x * -0.5f, 0); // Lighter recoil

        // Activate recoil effects
        isRecoilingX = true;
        StartCoroutine(RecoilX());
    }
}
