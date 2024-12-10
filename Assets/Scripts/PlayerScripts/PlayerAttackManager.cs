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

    [Header("Attack Objects")]
    [SerializeField] private List<Ability> abilityList; // Add all your attack objects to this list

    // Recoil variables
    [SerializeField] private float recoilXSpeed = 25f; // Horizontal recoil speed
    [SerializeField] private float recoilYSpeed = 25f; // Vertical recoil speed
    private bool isRecoilingX = false;
    private bool isRecoilingY = false;
    private Vector2 recoilDirection = Vector2.zero; // To store direction of recoil

    [Header("Attack Settings")]
    [SerializeField] private float attackCD = 0.5f; // Cooldown between attacks
    [SerializeField] private int parryCost = 10; // Stamina cost for parrying.
    [SerializeField] private Transform attackSpawnPos;
    private bool isParrying;
    private float lastAttackTime; // Tracks the time of the last attack
    public bool isAttacking; // Tracks if the player is currently attacking

    private PlayerMovement playerMovement;
    private StaminaManager staminaManager; // Reference to the player's stamina manager

    private void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
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
            return;

        if (Input.GetKeyDown(KeyCode.G)) // Example input for ability 0
        {
            UseAbility(0);
        }
    }

    private void UseAbility(int index)
    {
        if (index < 0 || index >= abilityList.Count) return;

        Ability ability = abilityList[index];
        StartCoroutine(ExecuteAttack(ability));
    }


    private IEnumerator ExecuteAttack(Ability ability)
    {
        isAttacking = true;
        lastAttackTime = Time.time;

        // Calculate the spawn position with offset based on the player's direction
        //Vector3 offset = new Vector3(ability.spawnDistance, 0, 0);
        //if (transform.localScale.x < 0) // Check the player's facing direction
        //{
        //    offset.x = -offset.x; // Reverse offset if player is facing left
        //}
        //Vector3 attackPosition = transform.position + offset;

        // Spawn the ability prefab at the calculated position
        if (playerMovement.facingLeft)
        {
            GameObject attack = Instantiate(ability.attackPrefab, attackSpawnPos.position, Quaternion.AngleAxis(90, new Vector3(0,0,1)));
        }
        else
        {
            GameObject attack = Instantiate(ability.attackPrefab, attackSpawnPos.position, Quaternion.identity);
        }

        yield return new WaitForSeconds(ability.cooldown);

        isAttacking = false;
    }



    public void ApplyRecoil(float damage)
    {
        // Apply recoil direction based on attack type (horizontal or vertical)
        if (damage >= 50) // For heavy attacks, apply a strong recoil
        {
            recoilDirection = new Vector2(transform.localScale.x * -0.5f, 0); // Horizontal recoil
        }
        else if (damage < 50 && damage >= 20) // For lighter attacks
        {
            recoilDirection = new Vector2(transform.localScale.x * -0.25f, 0); // Lighter horizontal recoil
        }

        // Activate recoil effects
        isRecoilingX = true;
        StartCoroutine(RecoilX());

        // If it's a vertical attack, apply vertical recoil
        if (damage == 70) // Example for up attack
        {
            isRecoilingY = true;
            StartCoroutine(RecoilY());
        }
    }

    private IEnumerator RecoilX()
    {
        float recoilTime = 0.25f; // How long the recoil lasts
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

    public void TriggerRecoil(float damage)
    {
        // Only apply recoil if the damage is above a threshold or if the attack hit something
        if (damage > 0)
        {
            ApplyRecoil(damage);  // Apply recoil based on attack damage
        }
    }

}
