using Survivor2D.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Survivor2D.UI
{
    public class GameOverPanel : MonoBehaviour
    {
        [SerializeField] private GameObject root;

        private void OnEnable() => GameEvents.OnGameOver += Show;
        private void OnDisable() => GameEvents.OnGameOver -= Show;

        private void Start() => root.SetActive(false);

        private void Show()
        {
            root.SetActive(true);
            Time.timeScale = 0f;
        }

        public void Restart()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("MainScene");
        }
    }
}
