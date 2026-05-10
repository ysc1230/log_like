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
        [SerializeField] private GameObject root;
        [SerializeField] private LevelSystem levelSystem;
        [SerializeField] private WeaponManager weaponManager;
        [SerializeField] private Button[] choiceButtons;

        private CanvasGroup rootCanvasGroup;

        private void Awake()
        {
            if (root == null)
            {
                root = gameObject;
            }

            rootCanvasGroup = root.GetComponent<CanvasGroup>();
            if (rootCanvasGroup == null)
            {
                rootCanvasGroup = root.AddComponent<CanvasGroup>();
            }

            SetVisible(false);
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
            SetVisible(false);
            Time.timeScale = 1f;
        }

        private void Show()
        {
            SetVisible(true);
            UpdateButtons();
        }

        private void SetVisible(bool visible)
        {
            if (rootCanvasGroup == null) return;
            rootCanvasGroup.alpha = visible ? 1f : 0f;
            rootCanvasGroup.interactable = visible;
            rootCanvasGroup.blocksRaycasts = visible;
        }

        private void UpdateButtons()
        {
            if (levelSystem == null || choiceButtons == null) return;

            var choices = levelSystem.PendingChoices;
            for (int i = 0; i < choiceButtons.Length; i++)
            {
                var button = choiceButtons[i];
                if (button == null) continue;

                if (i >= choices.Count)
                {
                    button.gameObject.SetActive(false);
                    continue;
                }

                button.gameObject.SetActive(true);

                var weaponType = choices[i];
                var owned = weaponManager != null ? weaponManager.GetOwnedWeapons().Find(w => w.Type == weaponType) : null;
                int currentLevel = owned != null ? owned.Level : 0;

                var txt = button.GetComponentInChildren<TMP_Text>();
                if (txt == null) continue;

                string levelStr = currentLevel == 0 ? "New" : $"Lv {currentLevel + 1}";
                txt.text = $"{WeaponStatus.GetName(weaponType)} {levelStr}\n<size=24>{WeaponStatus.GetDescription(weaponType, currentLevel == 0)}</size>";
            }
        }

        public void SelectChoice(int index)
        {
            if (levelSystem == null) return;
            levelSystem.ApplyWeaponChoice(index);
            HideImmediate();
        }

        public void SelectChoice0() => SelectChoice(0);
        public void SelectChoice1() => SelectChoice(1);
        public void SelectChoice2() => SelectChoice(2);
    }
}
