using System.Collections;
using UnityEngine;

public class PlayerProjectile : MonoBehaviour
{
    private Vector3 direction; // Direction the energy ball is moving
    private float lifetime; // How long the energy ball exists
    private float moveSpeed; // Movement speed of the energy ball
    private DamageManager damageManager; // Reference to the DamageManager for collision and damage logic
    private Animator animator; // Animator for handling animation

    private void Awake()
    {
        // Get the damage manager attached to the energy ball
        damageManager = GetComponent<DamageManager>();
        if (damageManager == null)
        {
            Debug.LogError("EnergyBall does not have a DamageManager attached!");
        }

        // Get the Animator component to play animations
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("EnergyBall does not have an Animator attached!");
        }
    }

    // Initialize the energy ball's movement and lifetime
    public void Initialize(Vector3 initialDirection, float speed, float lifespan, Ability.AbilityPhase phase)
    {
        direction = initialDirection;
        moveSpeed = speed;
        lifetime = lifespan;

        // Set the damage phase for the energy ball (optional)
        if (damageManager != null)
        {
            damageManager.SetPhase(phase); // Set the phase so damage can be applied
        }

        // Destroy the energy ball after its lifetime expires
        Destroy(gameObject, lifetime);

        // Play the firing animation immediately after spawning
        if (animator != null)
        {
            animator.Play("Fire"); // Replace "Fire" with the actual name of your firing animation
        }
    }

    private void Update()
    {
        // Move the energy ball forward
        if (damageManager != null)
        {
            transform.Translate(direction * moveSpeed * Time.deltaTime);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (damageManager != null)
        {
            // Let DamageManager handle collision and damage application
            damageManager.OnCollisionEnter2D(collision); // Handle collision
        }

        // Destroy the energy ball after collision
        Destroy(gameObject);
    }
}
