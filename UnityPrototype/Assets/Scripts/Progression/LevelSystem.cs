using Survivor2D.Core;
using Survivor2D.Player;
using UnityEngine;

namespace Survivor2D.Progression
{
    public class LevelSystem : MonoBehaviour
    {
        [SerializeField] private PlayerStats playerStats;
        [SerializeField] private int baseRequiredExp = 5;

        private int level = 1;
        private int currentExp;

        public int Level => level;
        public int CurrentExp => currentExp;
        public int RequiredExp => baseRequiredExp + (level - 1) * 3;

        public void ResetRuntimeState()
        {
            level = 1;
            currentExp = 0;
            GameEvents.RaiseLevelChanged(level);
            GameEvents.RaiseExpChanged(currentExp, RequiredExp, level);
        }

        public void AddExp(int amount)
        {
            currentExp += amount;
            while (currentExp >= RequiredExp)
            {
                currentExp -= RequiredExp;
                level++;
                GameEvents.RaiseLevelChanged(level);
                PauseForLevelUp();
            }
            GameEvents.RaiseExpChanged(currentExp, RequiredExp, level);
        }

        private void PauseForLevelUp()
        {
            Time.timeScale = 0f;
            GameEvents.RaiseLevelUpChoicesRequested();
        }

        public void ApplyUpgrade(UpgradeType type)
        {
            if (playerStats == null) return;
            switch (type)
            {
                case UpgradeType.AttackDamage:
                    playerStats.AddAttackDamage(2);
                    break;
                case UpgradeType.AttackSpeed:
                    playerStats.AddAttackRateMultiplier(0.9f);
                    break;
                case UpgradeType.MoveSpeed:
                    playerStats.AddMoveSpeed(0.3f);
                    break;
            }
            Time.timeScale = 1f;
        }
    }

    public enum UpgradeType
    {
        AttackDamage,
        AttackSpeed,
        MoveSpeed
    }
}
