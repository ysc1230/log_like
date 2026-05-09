using UnityEngine;

namespace Survivor2D.Core
{
    public class GameBootstrap : MonoBehaviour
    {
        [SerializeField] private Player.PlayerStats playerStats;
        [SerializeField] private Progression.LevelSystem levelSystem;
        [SerializeField] private UI.LevelUpPanel levelUpPanel;

        private void Awake()
        {
            Time.timeScale = 1f;
            if (playerStats != null) playerStats.ResetRuntimeState();
            if (levelSystem != null) levelSystem.ResetRuntimeState();
            if (levelUpPanel != null) levelUpPanel.HideImmediate();
        }
    }
}
