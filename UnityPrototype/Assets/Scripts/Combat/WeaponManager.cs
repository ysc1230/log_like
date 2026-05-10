using System.Collections.Generic;
using UnityEngine;
using Survivor2D.Core;
using Survivor2D.Enemy;
using Survivor2D.Player;

namespace Survivor2D.Combat
{
    public enum WeaponType
    {
        FishBone,
        CatClaw,
        TunaCanBomb
    }

    [System.Serializable]
    public class WeaponStatus
    {
        public WeaponType Type;
        public int Level; // 1-5, 0 if not owned
        public const int MaxLevel = 5;

        public string Name => GetName(Type);
        public string Description => GetDescription(Type, Level == 0);

        public static string GetName(WeaponType type)
        {
            switch (type)
            {
                case WeaponType.FishBone: return "Fish Bone";
                case WeaponType.CatClaw: return "Cat Claw";
                case WeaponType.TunaCanBomb: return "Tuna Can Bomb";
                default: return type.ToString();
            }
        }

        public static string GetDescription(WeaponType type, bool isNew)
        {
            if (isNew)
            {
                switch (type)
                {
                    case WeaponType.FishBone: return "Throw fish bones at nearby enemies.";
                    case WeaponType.CatClaw: return "Scratch nearby enemies with claw slashes.";
                    case WeaponType.TunaCanBomb: return "Drop tuna can bombs that explode in an area.";
                }
            }
            else
            {
                switch (type)
                {
                    case WeaponType.FishBone: return "Stronger projectile and more projectiles.";
                    case WeaponType.CatClaw: return "Larger slash radius and more damage.";
                    case WeaponType.TunaCanBomb: return "Larger explosion radius and more damage.";
                }
            }
            return "";
        }
    }

    public class WeaponManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ProjectilePool projectilePool;
        [SerializeField] private EnemyRegistry enemyRegistry;
        [SerializeField] private Transform firePoint;
        [SerializeField] private PlayerStats playerStats;

        [Header("Prefabs")]
        [SerializeField] private GameObject catClawVfxPrefab;
        [SerializeField] private GameObject tunaCanBombPrefab;
        [SerializeField] private GameObject explosionVfxPrefab;


        private Dictionary<WeaponType, WeaponStatus> ownedWeapons = new Dictionary<WeaponType, WeaponStatus>();
        private Dictionary<WeaponType, WeaponBase> activeWeapons = new Dictionary<WeaponType, WeaponBase>();

        private float baseAttackCooldown = 1f;
        private float currentAttackRateMultiplier = 1f;

        private void Awake()
        {
            if (playerStats != null)
            {
                baseAttackCooldown = Mathf.Max(0.1f, playerStats.AttackCooldown);
            }
        }

        private void OnEnable()
        {
            GameEvents.OnStatsChanged += HandleStatsChanged;
        }

        private void OnDisable()
        {
            GameEvents.OnStatsChanged -= HandleStatsChanged;
        }

        private void HandleStatsChanged(int attackDamage, float attackCooldown, float moveSpeed)
        {
            if (attackCooldown <= 0f) return;

            currentAttackRateMultiplier = Mathf.Max(0.1f, baseAttackCooldown / attackCooldown);
            foreach (var weapon in activeWeapons.Values)
            {
                if (weapon != null)
                {
                    weapon.SetAttackRateMultiplier(currentAttackRateMultiplier);
                }
            }
        }

        public List<WeaponStatus> GetOwnedWeapons()
        {
            return new List<WeaponStatus>(ownedWeapons.Values);
        }

        public void AddOrUpgradeWeapon(WeaponType type)
        {
            if (ownedWeapons.TryGetValue(type, out var status))
            {
                if (status.Level < WeaponStatus.MaxLevel)
                {
                    status.Level++;
                }
            }
            else
            {
                status = new WeaponStatus { Type = type, Level = 1 };
                ownedWeapons[type] = status;
                ActivateWeapon(status);
            }

            if (activeWeapons.TryGetValue(type, out var weaponComponent))
            {
                weaponComponent.SetLevel(status.Level);
            }

            GameEvents.RaiseWeaponsChanged(GetOwnedWeapons());
        }

        private void ActivateWeapon(WeaponStatus status)
        {
            WeaponBase weapon = null;
            switch (status.Type)
            {
                case WeaponType.FishBone:
                    var fb = gameObject.AddComponent<FishBoneWeapon>();
                    fb.Initialize(projectilePool, enemyRegistry, firePoint);
                    weapon = fb;
                    break;
                case WeaponType.CatClaw:
                    var cc = gameObject.AddComponent<CatClawWeapon>();
                    cc.Initialize(catClawVfxPrefab, enemyRegistry);
                    weapon = cc;
                    break;
                case WeaponType.TunaCanBomb:
                    var tb = gameObject.AddComponent<TunaCanBombWeapon>();
                    tb.Initialize(tunaCanBombPrefab, explosionVfxPrefab);
                    weapon = tb;
                    break;
            }

            if (weapon != null)
            {
                weapon.SetLevel(status.Level);
                weapon.SetAttackRateMultiplier(currentAttackRateMultiplier);
                activeWeapons[status.Type] = weapon;
            }
        }

        public List<WeaponType> GetAvailableUpgrades()
        {
            List<WeaponType> available = new List<WeaponType>();
            foreach (WeaponType type in System.Enum.GetValues(typeof(WeaponType)))
            {
                if (!ownedWeapons.ContainsKey(type) || ownedWeapons[type].Level < WeaponStatus.MaxLevel)
                {
                    available.Add(type);
                }
            }
            return available;
        }
    }
}

