using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "New Ability")]
public class Ability : ScriptableObject
{
    [System.Serializable]
    public class AbilityPhase
    {
        public string phaseName; // Optional name for the phase
        public List<AnimationClip> animations = new List<AnimationClip>(); // Multiple animations for this phase
        public AudioClip sound; // Sound for this phase
        public float phaseDuration; // Duration of this phase
        public GameObject damageInstancePrefab; // Damage instance for this phase (optional)
        public bool requiresMovement; // New: Does this phase require movement?
        public float movementSpeed; // New: Speed of movement during this phase

        public Vector2 hitboxSize = Vector2.one; // Width and height of the hitbox
        public Vector3 hitboxOffset = Vector3.zero; // Offset from the prefab's origin
        public int damageAmount; // Damage specific to this phase
    }

    public GameObject energyBallPrefab; // The energy ball prefab
    public float energyBallSpeed = 10f; // Speed at which the energy ball moves
    public float energyBallLifetime = 5f; // How long the energy ball lasts before disappearing

    public List<AbilityPhase> phases = new List<AbilityPhase>(); // List of all phases in this ability
    public float defaultPhaseDuration = 1f; // Default duration if not specified
    public float abilityCooldown = 0.4f; // Cooldown for the ability as a whole
    private float lastUsedTime = -Mathf.Infinity;

    public IEnumerator ExecuteAbility(Transform userTransform, Rigidbody2D rb)
    {
        if (Time.time < lastUsedTime + abilityCooldown)
        {
            Debug.Log("Ability on cooldown!");
            yield break; // Exit if the ability is on cooldown
        }

        lastUsedTime = Time.time;

        foreach (var phase in phases)
        {
            Debug.Log($"Executing Phase: {phase.phaseName}");

            // Play animations
            if (phase.animations.Count > 0)
            {
                foreach (var animation in phase.animations)
                {
                    PlayAnimation(userTransform, animation);
                }
            }

            // Play sound
            PlaySound(userTransform, phase.sound);

            // Handle movement if required
            if (phase.requiresMovement)
            {
                yield return ExecuteMovementPhase(userTransform, rb, phase);
            }

            // Spawn and configure damage instance
            if (phase.damageInstancePrefab != null)
            {
                SpawnDamageInstance(userTransform, phase);
            }

            // If this phase is the energy ball phase, spawn the energy ball
            if (phase.damageInstancePrefab == energyBallPrefab)
            {
                SpawnEnergyBall(userTransform, phase);
            }

            // Wait for the phase duration before starting the next phase
            float duration = phase.phaseDuration > 0 ? phase.phaseDuration : defaultPhaseDuration;
            yield return new WaitForSeconds(duration);
        }

        Debug.Log("Ability execution complete.");
    }

    // New: Handle movement during a phase
    private IEnumerator ExecuteMovementPhase(Transform userTransform, Rigidbody2D rb, AbilityPhase phase)
    {
        float timer = 0f;

        while (timer < phase.phaseDuration)
        {
            // Move in the direction the user is facing
            Vector2 movementDirection = userTransform.localScale.x > 0 ? Vector2.right : Vector2.left;
            rb.velocity = movementDirection * phase.movementSpeed;

            timer += Time.deltaTime;
            yield return null;
        }

        // Stop movement after the phase
        rb.velocity = Vector2.zero;
    }

    // Helper to Play Animation
    private void PlayAnimation(Transform userTransform, AnimationClip animation)
    {
        if (animation != null)
        {
            Animator animator = userTransform.GetComponent<Animator>();
            if (animator != null)
            {
                animator.Play(animation.name);
            }
            else
            {
                Debug.LogWarning("Animator not found on user transform.");
            }
        }
    }

    // Helper to Play Sound
    private void PlaySound(Transform userTransform, AudioClip sound)
    {
        if (sound != null)
        {
            AudioSource.PlayClipAtPoint(sound, userTransform.position);
        }
    }

    private void SpawnDamageInstance(Transform userTransform, AbilityPhase phase)
    {
        Vector3 direction = userTransform.right;

        if (userTransform.localScale.x < 0) // If facing left
        {
            direction = -userTransform.right;
        }

        Vector3 spawnPosition = userTransform.position + direction * phase.hitboxOffset.x +
                                userTransform.up * phase.hitboxOffset.y;

        GameObject instance = Instantiate(phase.damageInstancePrefab, spawnPosition, Quaternion.identity);

        DamageManager damageManager = instance.GetComponent<DamageManager>();
        if (damageManager != null)
        {
            damageManager.SetDirection(direction);
            damageManager.SetPhase(phase);
            damageManager.SetHitbox(phase.hitboxSize, phase.hitboxOffset);
        }
        else
        {
            Debug.LogError("DamageManager not found on the damage instance prefab.");
        }
    }

    private void SpawnEnergyBall(Transform userTransform, AbilityPhase phase)
    {
        Vector3 direction = userTransform.right;

        if (userTransform.localScale.x < 0)
        {
            direction = -userTransform.right;
        }

        Vector3 spawnPosition = userTransform.position + direction * phase.hitboxOffset.x +
                                userTransform.up * phase.hitboxOffset.y;

        GameObject energyBall = Instantiate(energyBallPrefab, spawnPosition, Quaternion.identity);

        PlayerProjectile playerProjectile = energyBall.GetComponent<PlayerProjectile>();
        if (playerProjectile != null)
        {
            playerProjectile.Initialize(direction, energyBallSpeed, energyBallLifetime, phase);
        }
        else
        {
            Debug.LogError("EnergyBall prefab does not have a PlayerProjectile component!");
        }
    }

    private void OnValidate()
    {
        if (phases.Count == 0)
        {
            Debug.LogWarning($"Ability '{name}' has no phases defined!");
        }

        foreach (var phase in phases)
        {
            if (phase.damageInstancePrefab == null)
            {
                Debug.LogWarning($"Phase '{phase.phaseName}' in ability '{name}' is missing a damage instance prefab.");
            }
            if (phase.phaseDuration <= 0)
            {
                Debug.LogWarning($"Phase '{phase.phaseName}' in ability '{name}' has an invalid duration. Using default duration.");
            }
        }
    }
}
