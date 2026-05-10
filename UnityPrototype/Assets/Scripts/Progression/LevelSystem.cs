using Survivor2D.Core;
using Survivor2D.Player;
using Survivor2D.Combat;
using UnityEngine;
using System.Collections.Generic;

namespace Survivor2D.Progression
{
    public class LevelSystem : MonoBehaviour
    {
        [SerializeField] private PlayerStats playerStats;
        [SerializeField] private WeaponManager weaponManager;
        [SerializeField] private int baseRequiredExp = 5;

        private int level = 1;
        private int currentExp;
        private List<WeaponType> pendingChoices = new List<WeaponType>();

        public int Level => level;
        public int CurrentExp => currentExp;
        public int RequiredExp => baseRequiredExp + (level - 1) * 3;
        public List<WeaponType> PendingChoices => pendingChoices;

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
            GenerateWeaponChoices();
            Time.timeScale = 0f;
            GameEvents.RaiseLevelUpChoicesRequested();
        }

        private void GenerateWeaponChoices()
        {
            pendingChoices.Clear();
            if (weaponManager == null) return;

            var available = weaponManager.GetAvailableUpgrades();
            // Shuffle and pick 3
            for (int i = 0; i < available.Count; i++)
            {
                WeaponType temp = available[i];
                int randomIndex = Random.Range(i, available.Count);
                available[i] = available[randomIndex];
                available[randomIndex] = temp;
            }

            for (int i = 0; i < Mathf.Min(3, available.Count); i++)
            {
                pendingChoices.Add(available[i]);
            }
        }

        public void ApplyWeaponChoice(int index)
        {
            if (index >= 0 && index < pendingChoices.Count)
            {
                weaponManager.AddOrUpgradeWeapon(pendingChoices[index]);
            }
            Time.timeScale = 1f;
        }

        // Deprecated
        public void ApplyUpgrade(UpgradeType type)
        {
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
