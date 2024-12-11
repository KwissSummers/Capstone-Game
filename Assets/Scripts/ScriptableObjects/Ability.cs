using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "New Ability")]
public class Ability : ScriptableObject
{
    // New Multi-Phase Properties
    [System.Serializable]
    public class AbilityPhase
    {
        public string phaseName; // Optional name for the phase
        public AnimationClip animation; // Animation for this phase
        public AudioClip sound; // Sound for this phase
        public float phaseDuration; // Duration of this phase
        public GameObject damageInstancePrefab; // Damage instance for this phase (optional)

        // Hitbox Customization
        public Vector2 hitboxSize = Vector2.one; // Width and height of the hitbox
        public Vector3 hitboxOffset = Vector3.zero; // Offset from the prefab's origin
        public int damageAmount; // Damage specific to this phase
    }

    public List<AbilityPhase> phases = new List<AbilityPhase>(); // List of all phases in this ability
    public float defaultPhaseDuration = 1f; // Default duration if not specified

    // Method to Execute Ability
    public IEnumerator ExecuteAbility(Transform userTransform)
    {
        foreach (var phase in phases)
        {
            Debug.Log($"Executing Phase: {phase.phaseName}");

            // Trigger Animation
            PlayAnimation(userTransform, phase.animation);

            // Play Sound
            PlaySound(userTransform, phase.sound);

            // Spawn and Configure Damage Instance
            if (phase.damageInstancePrefab != null)
            {
                SpawnDamageInstance(userTransform, phase);
            }

            // Wait for the phase duration
            yield return new WaitForSeconds(phase.phaseDuration > 0 ? phase.phaseDuration : defaultPhaseDuration);
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

    // Helper to Spawn Damage Instance
    private void SpawnDamageInstance(Transform userTransform, AbilityPhase phase)
    {
        Vector3 spawnPosition = userTransform.position + userTransform.right * phase.hitboxOffset.x +
                                userTransform.up * phase.hitboxOffset.y;
        GameObject instance = Instantiate(phase.damageInstancePrefab, spawnPosition, Quaternion.identity);

        // Configure the hitbox
        DamageManager damageManager = instance.GetComponent<DamageManager>();
        if (damageManager != null)
        {
            damageManager.SetPhase(phase);
            damageManager.SetHitbox(phase.hitboxSize, phase.hitboxOffset);
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
