using Survivor2D.Enemy;
using System.Collections.Generic;
using UnityEngine;

namespace Survivor2D.Combat
{
    public class CatClawWeapon : WeaponBase
    {
        private GameObject vfxPrefab;
        private EnemyRegistry registry;
        private float searchRange = 5f;
        private int baseDamage = 15;
        private float baseCooldown = 2.0f;

        public void Initialize(GameObject vfx, EnemyRegistry reg)
        {
            vfxPrefab = vfx;
            registry = reg;
            ResetCooldown();
        }

        protected override void Attack()
        {
            if (registry == null) return;

            int targetCount = 1 + (level >= 3 ? 1 : 0) + (level >= 5 ? 1 : 0);
            int damage = baseDamage + (level >= 2 ? 10 : 0) + (level >= 5 ? 10 : 0);

            var targets = registry.GetNearestMultiple(transform.position, targetCount, searchRange);
            
            foreach (var target in targets)
            {
                // Visual effect on enemy
                if (vfxPrefab != null)
                {
                    var vfx = Instantiate(vfxPrefab, target.transform.position, Quaternion.identity);
                    vfx.transform.localScale = Vector3.one * 0.05f; 
                    Destroy(vfx, 0.3f);
                }

                target.ApplyDamage(damage);
            }
        }

        protected override void ResetCooldown()
        {
            cooldownTimer = baseCooldown * (level >= 4 ? 0.7f : 1f);
        }
    }
}
