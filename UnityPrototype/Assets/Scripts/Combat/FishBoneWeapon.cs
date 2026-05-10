using Survivor2D.Enemy;
using UnityEngine;

namespace Survivor2D.Combat
{
    public class FishBoneWeapon : WeaponBase
    {
        private ProjectilePool pool;
        private EnemyRegistry registry;
        private Transform firePoint;

        private float baseCooldown = 1.2f;
        private int baseDamage = 10;

        public void Initialize(ProjectilePool p, EnemyRegistry r, Transform fp)
        {
            pool = p;
            registry = r;
            firePoint = fp;
            ResetCooldown();
        }

        protected override void Attack()
        {
            if (registry == null || pool == null || firePoint == null) return;
            if (!registry.TryGetNearest(transform.position, out var target)) return;

            int count = level >= 3 ? 2 : 1;
            int damage = baseDamage + (level >= 2 ? 5 : 0) + (level >= 5 ? 5 : 0);
            bool piercing = level >= 5;

            for (int i = 0; i < count; i++)
            {
                var spawnPos = firePoint.position;
                if (count > 1) spawnPos += (Vector3)Random.insideUnitCircle * 0.2f;
                
                var dir = (target.transform.position - spawnPos).normalized;
                var projectile = pool.Get(spawnPos);
                projectile.Initialize(dir, damage, piercing);
            }
        }

        protected override void ResetCooldown()
        {
            cooldownTimer = baseCooldown * (level >= 4 ? 0.7f : 1f);
        }
    }
}
