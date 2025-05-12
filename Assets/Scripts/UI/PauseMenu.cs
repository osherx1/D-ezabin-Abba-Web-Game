using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utilities;

public class PauseMenu : MonoBehaviour
{
    public static bool GameIsPaused = false;
    
    [SerializeField] private GameObject pauseMenuUI;
    public static event Action OnResumeGame;
    public static event Action OnPauseGame;
    public static event Action OnLoadMenu;
    
    // add me singelton such that it will work for all scenes
    private static PauseMenu _instance;
    public static PauseMenu Instance => _instance;
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    
    
    // Update is called once per frame
    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Escape)) return;
        if (GameIsPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }
    
    public void ResumeGame()
    {
        OnResumeGame?.Invoke();
        Time.timeScale = 1f;
        GameIsPaused = false;
        pauseMenuUI.SetActive(false);
    }
    
    public void PauseGame()
    {
        OnPauseGame?.Invoke();
        Time.timeScale = 0f;
        GameIsPaused = true;
        pauseMenuUI.SetActive(true);
    }
    
    public void LoadMenu()
    {
        OnLoadMenu?.Invoke();
        Time.timeScale = 1f;
        GameIsPaused = false;
        pauseMenuUI.SetActive(false);
        // Load the main menu scene
        GameEvents.RestartGame.Invoke();
        
        SceneManager.LoadScene("Start Menu");
    }
    
    public void QuitGame()
    {
        Application.Quit();
    }
}
