using Survivor2D.Core;
using Survivor2D.Progression;
using Survivor2D.Combat;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Survivor2D.UI
{
    public class LevelUpPanel : MonoBehaviour
    {
        [SerializeField] private GameObject root; // The visual panel root
        [SerializeField] private LevelSystem levelSystem;
        [SerializeField] private WeaponManager weaponManager;
        [SerializeField] private Button[] choiceButtons;
        [SerializeField] private TMP_Text titleText;

        private void Awake()
        {
            if (root != null)
            {
                root.SetActive(false);
            }
        }

        private void OnEnable()
        {
            GameEvents.OnLevelUpChoicesRequested += Show;
        }

        private void OnDisable()
        {
            GameEvents.OnLevelUpChoicesRequested -= Show;
        }

        public void HideImmediate()
        {
            if (root != null) root.SetActive(false);
            Time.timeScale = 1f;
        }

        private void Show()
        {
            if (root != null) root.SetActive(true);

            UpdateButtons();
        }

        private void UpdateButtons()
        {
            if (levelSystem == null) return;
            var choices = levelSystem.PendingChoices;

            for (int i = 0; i < choiceButtons.Length; i++)
            {
                if (i < choices.Count)
                {
                    choiceButtons[i].gameObject.SetActive(true);
                    var weaponType = choices[i];
                    var owned = weaponManager != null ? weaponManager.GetOwnedWeapons().Find(w => w.Type == weaponType) : null;
                    int currentLevel = owned != null ? owned.Level : 0;

                    var txt = choiceButtons[i].GetComponentInChildren<TMP_Text>();
                    if (txt != null)
                    {
                        string levelStr = currentLevel == 0 ? "New" : $"Lv {currentLevel + 1}";
                        txt.text = $"{WeaponStatus.GetName(weaponType)} {levelStr}\n" +
                                   $"<size=24>{WeaponStatus.GetDescription(weaponType, currentLevel == 0)}</size>";
                    }
                }
                else
                {
                    choiceButtons[i].gameObject.SetActive(false);
                }
            }
        }

        public void SelectChoice(int index)
        {
            levelSystem.ApplyWeaponChoice(index);
            HideImmediate();
        }

        public void SelectChoice0() => SelectChoice(0);
        public void SelectChoice1() => SelectChoice(1);
        public void SelectChoice2() => SelectChoice(2);
    }
}
