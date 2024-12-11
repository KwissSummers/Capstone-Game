using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FadeScript : MonoBehaviour
{
    [SerializeField] private Animator animator;
    private bool fadingOut = false;
    private string nextLevel = "MainMenu";

    public void FadeOutToScene(string levelName) {
        // do nothing if already fading out
        if (fadingOut) return;

        // set the next level to the given ID
        nextLevel = levelName;

        // tell animator to play fade out animation
        animator.SetTrigger("FadeOut");

        // I am fading out already. Prevent anything from messing with that
        fadingOut = true;
    }

    public void LoadNextLevel() {
        Time.timeScale = 1;
        SceneManager.LoadScene(nextLevel);
    }
}
