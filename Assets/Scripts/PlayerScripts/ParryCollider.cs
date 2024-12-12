using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParryCollider : MonoBehaviour
{
    private PlayerAttackManager playerAttackManager;
    private Vector2 recoilDirection;

    private void Awake()
    {
        playerAttackManager = FindObjectOfType<PlayerAttackManager>();  // Get PlayerAttackManager instance
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if it's an enemy attack

        if (other.CompareTag("EnemyAttack"))
        {
            // Negate the damage (optional logic for negating damage)
            DamageManager enemyDamageManager = other.GetComponent<DamageManager>();
            if (enemyDamageManager != null)
            {
                Debug.Log("Parried an enemy attack! Damage negated.");

                // Apply recoil to the enemy
                enemyDamageManager.ApplyRecoilToEnemy(new Vector2(transform.localScale.x * -0.5f, 0), 5f);  // You can adjust the recoil force
            }

            // Apply speed reduction to the boss
            BossController bossController = other.GetComponentInParent<BossController>();
            if (bossController != null)
            {
                bossController.ApplyParrySpeedReduction();  // Call the method to reduce the boss speed
            }

            // Destroy the parry object
            Destroy(gameObject);
        }
    }


    public void SetRecoilDirection(Vector2 direction)
    {
        recoilDirection = direction;
    }
}
