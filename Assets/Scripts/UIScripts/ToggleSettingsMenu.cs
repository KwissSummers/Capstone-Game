using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleSettingsMenu : MonoBehaviour
{
    [SerializeField] private GameObject togglee;
    private float prevTimeScale = 1;
    public void ToggleVisibility() {
        bool paused = !togglee.active;
        togglee.active = paused;

        if (paused) {
            prevTimeScale = Time.timeScale;
            Time.timeScale = 0;
        } else {
            Time.timeScale = prevTimeScale;
        }
    }
}
