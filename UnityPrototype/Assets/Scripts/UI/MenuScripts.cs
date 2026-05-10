using UnityEngine;
using UnityEngine.SceneManagement;
using Survivor2D.Core;

namespace Survivor2D.UI
{
    public class MainMenuManager : MonoBehaviour
    {
        public void StartGame()
        {
            SceneManager.LoadScene("MainScene");
        }

        public void OpenSettings()
        {
            Debug.Log("Settings Opened");
        }

        public void ExitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }

    public class GameOverUI : MonoBehaviour
    {
        [SerializeField] private GameObject gameOverPanel;

        private void Awake()
        {
            // Ensure we listen even if the panel starts inactive
            GameEvents.OnGameOver += ShowGameOver;
        }

        private void OnDestroy()
        {
            GameEvents.OnGameOver -= ShowGameOver;
        }

        private void ShowGameOver()
        {
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);
                Time.timeScale = 0f; 
            }
        }

        public void ReturnToMainMenu()
        {
            Time.timeScale = 1f; 
            SceneManager.LoadScene("MainMenuScene");
        }
    }
}
