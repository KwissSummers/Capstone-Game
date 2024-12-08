using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageManager : MonoBehaviour
{
    [SerializeField] private int damage; // Damage dealt by this instance
    [SerializeField] private float lifespan = 0.5f; // Time before destruction
    [SerializeField] private float speed = 5f; // Movement speed for damage instances

    private Vector3 direction; // Direction of movement

    void Start()
    {
        Destroy(gameObject, lifespan); // Destroy automatically after lifespan
        direction = transform.right; // Default direction
    }

    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        // Reduce health of collided object if it has HealthManager
        if (other.gameObject.TryGetComponent(out HealthManager healthManager))
        {
            healthManager.currentHealth -= damage;
            Debug.Log($"Dealt {damage} damage to {other.gameObject.name}");
        }

        // Destroy the damage instance on collision
        Destroy(gameObject);
    }

    public void SetDamage(int value)
    {
        damage = value; // Allow damage to be set dynamically
    }

    public void SetDirection(Vector3 dir)
    {
        direction = dir; // Allow direction to be modified
    }
}
