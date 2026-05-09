using Survivor2D.Enemy;
using Survivor2D.Player;
using UnityEngine;

namespace Survivor2D.Combat
{
    public class AutoAttacker : MonoBehaviour
    {
        [SerializeField] private PlayerStats playerStats;
        [SerializeField] private ProjectilePool projectilePool;
        [SerializeField] private Transform firePoint;
        [SerializeField] private EnemyRegistry enemyRegistry;

        private float cooldownTimer;

        private void Update()
        {
            if (playerStats == null || projectilePool == null || firePoint == null || enemyRegistry == null) return;
            cooldownTimer += Time.deltaTime;
            if (cooldownTimer < playerStats.AttackCooldown) return;
            if (!enemyRegistry.TryGetNearest(transform.position, out var target)) return;

            cooldownTimer = 0f;
            var dir = (target.transform.position - firePoint.position).normalized;
            var projectile = projectilePool.Get(firePoint.position);
            projectile.Initialize(dir, playerStats.AttackDamage);
        }
    }
}
