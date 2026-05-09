using Survivor2D.Core;
using Survivor2D.Progression;
using UnityEngine;

namespace Survivor2D.UI
{
    public class LevelUpPanel : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private LevelSystem levelSystem;

        private void OnEnable() => GameEvents.OnLevelUpChoicesRequested += Show;
        private void OnDisable() => GameEvents.OnLevelUpChoicesRequested -= Show;

        public void HideImmediate() => root.SetActive(false);

        private void Show() => root.SetActive(true);

        public void SelectAttackDamage() => SelectUpgrade(UpgradeType.AttackDamage);
        public void SelectAttackSpeed() => SelectUpgrade(UpgradeType.AttackSpeed);
        public void SelectMoveSpeed() => SelectUpgrade(UpgradeType.MoveSpeed);

        private void SelectUpgrade(UpgradeType type)
        {
            levelSystem.ApplyUpgrade(type);
            root.SetActive(false);
        }
    }
}
