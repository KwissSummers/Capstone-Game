using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManagerScript : MonoBehaviour
{
    [Header("Menu Panels")]
    [SerializeField] private GameObject pauseMenu; // Reference to the pause menu panel
    [SerializeField] private GameObject gameOverMenu; // Reference to the game over menu panel
    [SerializeField] private GameObject mainMenu; // Reference to the main menu panel

    private bool isPaused = false;

    private void Start()
    {
        // Ensure menus are hidden at the start of the game
        if (pauseMenu != null) pauseMenu.SetActive(false);
        if (gameOverMenu != null) gameOverMenu.SetActive(false);
        if (mainMenu != null) mainMenu.SetActive(false);
    }

    private void Update()
    {
        HandlePause();
    }

    public void ShowMenu(string menuName)
    {
        switch (menuName)
        {
            case "Pause":
                if (pauseMenu != null) pauseMenu.SetActive(true);
                break;
            case "GameOver":
                if (gameOverMenu != null) gameOverMenu.SetActive(true);
                break;
            case "Main":
                if (mainMenu != null) mainMenu.SetActive(true);
                break;
        }
    }

    public void HideMenu(string menuName)
    {
        switch (menuName)
        {
            case "Pause":
                if (pauseMenu != null) pauseMenu.SetActive(false);
                break;
            case "GameOver":
                if (gameOverMenu != null) gameOverMenu.SetActive(false);
                break;
            case "Main":
                if (mainMenu != null) mainMenu.SetActive(false);
                break;
        }
    }

    private void HandlePause()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) // Escape key toggles pause
        {
            isPaused = !isPaused;

            if (isPaused)
            {
                Time.timeScale = 0f; // Pause game time
                ShowMenu("Pause");
            }
            else
            {
                Time.timeScale = 1f; // Resume game time
                HideMenu("Pause");
            }
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f; // Ensure time is resumed
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Reload current scene
    }

    public void ExitGame()
    {
        Debug.Log("Exiting game...");
        Application.Quit();
    }

    public void StartGame(string sceneName)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName); // Load the specified scene
    }
}
