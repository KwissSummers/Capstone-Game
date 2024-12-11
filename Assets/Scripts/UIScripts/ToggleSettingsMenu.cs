using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleSettingsMenu : MonoBehaviour
{
    [SerializeField] private GameObject togglee;
    public void ToggleVisibility() {
        togglee.active = !togglee.active;
    }
}
