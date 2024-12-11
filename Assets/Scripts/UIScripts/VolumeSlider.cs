using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeSlider : MonoBehaviour
{
    [SerializeField] private AudioMixer mixer;
    [SerializeField] private Slider slider;
    [SerializeField] private string exposedParameter;
    
    // Start is called before the first frame update
    void Start()
    {
        // grab the current music volume
        float value = 0;
        bool no_error = mixer.GetFloat(exposedParameter, out value);

        // set the slider to the current value
        if (no_error) slider.value = Mathf.Pow(10, value / 20);
        else slider.value = 1;
    }
    public void SetLevel(float sliderValue)
    {
        // convert from linear to log because sound
        if (sliderValue == 0) sliderValue = -80;
        else sliderValue = Mathf.Log10(sliderValue) * 20;

        // set the value
        mixer.SetFloat(exposedParameter, sliderValue);
    }
}