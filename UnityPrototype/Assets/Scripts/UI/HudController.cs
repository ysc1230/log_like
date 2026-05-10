using Survivor2D.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Survivor2D.UI
{
    public class HudController : MonoBehaviour
    {
        [SerializeField] private Slider hpSlider;
        [SerializeField] private Slider expSlider;
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private TMP_Text timeText;
        [SerializeField] private TMP_Text statsText;
        [SerializeField] private TMP_Text weaponListText;

        private float elapsed;

        private void OnEnable()
        {
            GameEvents.OnHealthChanged += HandleHealthChanged;
            GameEvents.OnExpChanged += HandleExpChanged;
            GameEvents.OnLevelChanged += HandleLevelChanged;
            GameEvents.OnStatsChanged += HandleStatsChanged;
            GameEvents.OnWeaponsChanged += HandleWeaponsChanged;
        }

        private void OnDisable()
        {
            GameEvents.OnHealthChanged -= HandleHealthChanged;
            GameEvents.OnExpChanged -= HandleExpChanged;
            GameEvents.OnLevelChanged -= HandleLevelChanged;
            GameEvents.OnStatsChanged -= HandleStatsChanged;
            GameEvents.OnWeaponsChanged -= HandleWeaponsChanged;
        }

        private void Update()
        {
            elapsed += Time.unscaledDeltaTime;
            GameEvents.RaiseSurvivalTimeChanged(elapsed);
            timeText.text = $"Time {elapsed:0.0}s";
        }

        private void HandleHealthChanged(int current, int max)
        {
            hpSlider.maxValue = max;
            hpSlider.value = current;
        }

        private void HandleExpChanged(int current, int required, int level)
        {
            expSlider.maxValue = required;
            expSlider.value = current;
            levelText.text = $"Lv {level}";
        }

        private void HandleLevelChanged(int level) => levelText.text = $"Lv {level}";

        private void HandleStatsChanged(int atk, float aspd, float mspd)
        {
            statsText.text = $"ATK: {atk}\nASPD: {1f/aspd:0.0}/s\nMSPD: {mspd:0.0}";
        }

        private void HandleWeaponsChanged(System.Collections.Generic.List<Survivor2D.Combat.WeaponStatus> weapons)
        {
            if (weaponListText == null) return;
            string text = "";
            foreach (var w in weapons)
            {
                text += $"{w.Name} Lv {w.Level}\n";
            }
            weaponListText.text = text;
        }
        }
        }
