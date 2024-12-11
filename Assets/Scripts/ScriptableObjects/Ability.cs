using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "New Ability")]
public class Ability : ScriptableObject
{
    // Existing Properties
    public GameObject attackPrefab; // Prefab with animations and sound
    public float damage; // Damage ability does
    public float cooldown; // Cooldown before re-use
    public float lifespan; // How long object lasts for
    public float speed; // Speed the object travels
    public float spawnDistance; // Distance from the user's origin point

    // New Properties for Multi-Phase Abilities
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
        public int damageAmount; // Damage specific to this phase (overrides general damage)
    }

    public List<AbilityPhase> phases = new List<AbilityPhase>(); // List of all phases in this ability

    public float defaultPhaseDuration = 1f; // Default duration if not specified

    // Method to Execute Ability
    public IEnumerator ExecuteAbility(Transform userTransform)
    {
        for (int i = 0; i < phases.Count; i++)
        {
            AbilityPhase phase = phases[i];
            Debug.Log($"Executing Phase {i + 1}: {phase.phaseName}");

            // Trigger Animation
            if (phase.animation != null)
            {
                Animator animator = userTransform.GetComponent<Animator>();
                if (animator != null)
                {
                    animator.Play(phase.animation.name);
                }
            }

            // Play Sound
            if (phase.sound != null)
            {
                AudioSource.PlayClipAtPoint(phase.sound, userTransform.position);
            }

            // Spawn and Customize Damage Instance
            if (phase.damageInstancePrefab != null)
            {
                Vector3 spawnPosition = userTransform.position + userTransform.right * spawnDistance;
                GameObject instance = Instantiate(phase.damageInstancePrefab, spawnPosition, Quaternion.identity);

                // Configure the hitbox
                DamageManager damageManager = instance.GetComponent<DamageManager>();
                if (damageManager != null)
                {
                    damageManager.SetDamage((int)(phase.damageAmount > 0 ? phase.damageAmount : damage));
                    damageManager.SetHitbox(phase.hitboxSize, phase.hitboxOffset);
                }
            }

            // Wait for the phase duration
            yield return new WaitForSeconds(phase.phaseDuration > 0 ? phase.phaseDuration : defaultPhaseDuration);
        }

        Debug.Log("Ability execution complete.");
    }
}
