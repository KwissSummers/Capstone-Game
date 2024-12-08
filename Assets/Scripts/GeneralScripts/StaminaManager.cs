using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StaminaManager : MonoBehaviour
{
    [Header("Stamina Settings")]
    [SerializeField] public float stamina = 100;
    [SerializeField] public float maxStamina = 100;
    [SerializeField] public float staminaRegen = 5f; // Amount of stamina regenerated per second
    [SerializeField] public Image staminaBar; // UI stamina bar

    private void Start()
    {
        stamina = maxStamina; // Initialize stamina to max
    }

    private void Update()
    {
        // Regenerate stamina over time
        RegenerateStamina();

        // Prevent stamina from exceeding max or going below 0
        stamina = Mathf.Clamp(stamina, 0, maxStamina);

        // Update stamina UI
        UpdateStaminaUI();
    }

    public bool UseStamina(float amount)
    {
        if (stamina < amount)
        {
            Debug.Log("Not enough stamina!");
            return false;
        }

        stamina -= amount;
        Debug.Log($"Used {amount} stamina. Remaining stamina: {stamina}");
        return true;
    }

    private void RegenerateStamina()
    {
        if (stamina < maxStamina)
        {
            stamina += staminaRegen * Time.deltaTime;
        }
    }

    private void UpdateStaminaUI()
    {
        if (staminaBar != null)
        {
            staminaBar.fillAmount = stamina / maxStamina;
        }
    }

    public void ConvertDamageToStamina(float damage)
    {
        float staminaGained = damage * 0.5f; // Gain 50% of damage dealt as stamina
        stamina = Mathf.Clamp(stamina + staminaGained, 0, maxStamina);
        Debug.Log($"Gained {staminaGained} stamina from damage dealt.");
    }
}
