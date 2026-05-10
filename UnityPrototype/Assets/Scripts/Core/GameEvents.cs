using System;

namespace Survivor2D.Core
{
    public static class GameEvents
    {
        public static event Action<int, int> OnHealthChanged;
        public static event Action<int, int, int> OnExpChanged;
        public static event Action<int> OnLevelChanged;
        public static event Action<float> OnSurvivalTimeChanged;
        public static event Action OnGameOver;
        public static event Action OnLevelUpChoicesRequested;
        public static event Action<int, float, float> OnStatsChanged;
        public static event Action<System.Collections.Generic.List<Survivor2D.Combat.WeaponStatus>> OnWeaponsChanged;

        public static void RaiseHealthChanged(int current, int max) => OnHealthChanged?.Invoke(current, max);
        public static void RaiseExpChanged(int current, int required, int level) => OnExpChanged?.Invoke(current, required, level);
        public static void RaiseLevelChanged(int level) => OnLevelChanged?.Invoke(level);
        public static void RaiseSurvivalTimeChanged(float seconds) => OnSurvivalTimeChanged?.Invoke(seconds);
        public static void RaiseGameOver() => OnGameOver?.Invoke();
        public static void RaiseLevelUpChoicesRequested() => OnLevelUpChoicesRequested?.Invoke();
        public static void RaiseStatsChanged(int atk, float aspd, float mspd) => OnStatsChanged?.Invoke(atk, aspd, mspd);
        public static void RaiseWeaponsChanged(System.Collections.Generic.List<Survivor2D.Combat.WeaponStatus> weapons) => OnWeaponsChanged?.Invoke(weapons);
        }
        }
