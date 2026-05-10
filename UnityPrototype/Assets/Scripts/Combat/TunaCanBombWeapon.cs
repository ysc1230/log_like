using Survivor2D.Enemy;
using UnityEngine;

namespace Survivor2D.Combat
{
    public class TunaCanBombWeapon : WeaponBase
    {
        private GameObject bombPrefab;
        private GameObject explosionVfxPrefab;

        private int baseDamage = 30;
        private float baseCooldown = 3.5f;
        private float spawnRadius = 3f;
        private float bombLifetime = 20f;

        public void Initialize(GameObject bPrefab, GameObject vfx)
        {
            bombPrefab = bPrefab;
            explosionVfxPrefab = vfx;
            ResetCooldown();
        }

        protected override void Attack()
        {
            if (bombPrefab == null) return;

            int count = 1 + (level >= 3 ? 1 : 0) + (level >= 5 ? 1 : 0);
            int damage = baseDamage + (level >= 2 ? 15 : 0) + (level >= 5 ? 15 : 0);
            float lifetime = bombLifetime + (level >= 4 ? 2f : 0);

            for (int i = 0; i < count; i++)
            {
                Vector3 spawnOffset = (Vector3)Random.insideUnitCircle * spawnRadius;
                Vector3 spawnPos = transform.position + spawnOffset;

                var bombGo = Instantiate(bombPrefab, spawnPos, Quaternion.identity);
                var bomb = bombGo.AddComponent<TunaCanBomb>();
                bomb.Initialize(damage, lifetime, explosionVfxPrefab);
            }
        }

        protected override void ResetCooldown()
        {
            cooldownTimer = baseCooldown * (level >= 4 ? 0.7f : 1f);
        }
    }
}
