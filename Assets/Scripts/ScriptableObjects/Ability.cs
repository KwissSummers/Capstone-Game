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
