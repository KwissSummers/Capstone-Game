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

    public IEnumerator ExecuteAbility(Transform userTransform)
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
        // Determine the direction the player is facing
        Vector3 direction = userTransform.right;  // Default to right-facing direction

        Debug.Log(direction);

        if (userTransform.localScale.x < 0) // If facing left
        {
            direction = -userTransform.right;  // Reverse direction
        }

        // Adjust the spawn position based on the direction
        Vector3 spawnPosition = userTransform.position + direction * phase.hitboxOffset.x +
                                userTransform.up * phase.hitboxOffset.y;

        // Instantiate the damage instance prefab
        GameObject instance = Instantiate(phase.damageInstancePrefab, spawnPosition, Quaternion.identity);

        // Set the direction for the damage instance (projectile or other effect)
        DamageManager damageManager = instance.GetComponent<DamageManager>();
        if (damageManager != null)
        {
            damageManager.SetDirection(direction);  // Pass the correct direction
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
        // Determine the direction the player is facing
        Vector3 direction = userTransform.right;  // Default to right-facing direction

        // If the player is facing left, reverse the direction
        if (userTransform.localScale.x < 0)
        {
            direction = -userTransform.right;  // Reverse the direction if facing left
        }

        // Adjust spawn position based on direction
        Vector3 spawnPosition = userTransform.position + direction * phase.hitboxOffset.x +
                                userTransform.up * phase.hitboxOffset.y;

        // Instantiate the PlayerProjectile (Energy Ball)
        GameObject energyBall = Instantiate(energyBallPrefab, spawnPosition, Quaternion.identity);

        // Get the PlayerProjectile component and initialize it
        PlayerProjectile playerProjectile = energyBall.GetComponent<PlayerProjectile>();
        if (playerProjectile != null)
        {
            // Initialize the energy ball with the direction, speed, and lifetime
            playerProjectile.Initialize(direction, energyBallSpeed, energyBallLifetime, phase);
        }
        else
        {
            Debug.LogError("EnergyBall prefab does not have a PlayerProjectile component!");
        }
    }


    // Editor Validation
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
