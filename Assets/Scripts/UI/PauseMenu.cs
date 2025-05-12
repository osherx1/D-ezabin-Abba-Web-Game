using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utilities;

namespace UI
{
    public class PauseMenu : MonoBehaviour
    {
        private static bool _gameIsPaused = false;
    
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
            if (_gameIsPaused)
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
            _gameIsPaused = false;
            pauseMenuUI.SetActive(false);
        }
    
        public void PauseGame()
        {
            OnPauseGame?.Invoke();
            Time.timeScale = 0f;
            _gameIsPaused = true;
            pauseMenuUI.SetActive(true);
        }
    
        public void LoadMenu()
        {
            OnLoadMenu?.Invoke();
            Time.timeScale = 1f;
            _gameIsPaused = false;
            pauseMenuUI.SetActive(false);
            // Load the main menu scene
            GameEvents.RestartGame.Invoke();
        
            SceneManager.LoadScene(1);
        }
    
        public void QuitGame()
        {
            Application.Quit();
        }
    }
}
