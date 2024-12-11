using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
public class StaminaManager : MonoBehaviour
{
    [Header("Stamina Settings")]
    [SerializeField] public float stamina = 100;
    [SerializeField] public float maxStamina = 100;
    [SerializeField] public float staminaRegen = 2f; // Amount of stamina regenerated per second
    [SerializeField] public Image staminaBar; // UI stamina bar

    private void Start()
    {
        stamina = maxStamina; // Initialize stamina to max
    }

    private void Update()
    {
        RegenerateStamina();
        stamina = Mathf.Clamp(stamina, 0, maxStamina);
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
        UpdateStaminaUI();  // Ensure UI is updated after using stamina
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
        UpdateStaminaUI();  // Ensure UI is updated after gaining stamina
    }

    public void Heal(float healAmount)
    {
        // Heal the player using stamina (consumes stamina)
        if (stamina >= healAmount)
        {
            stamina -= healAmount;
            Debug.Log($"Healed {healAmount} stamina. Remaining stamina: {stamina}");
            UpdateStaminaUI();  // Update UI after healing
        }
        else
        {
            Debug.Log("Not enough stamina to heal!");
        }
    }
}
