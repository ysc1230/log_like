using Survivor2D.Enemy;
using UnityEngine;

namespace Survivor2D.Combat
{
    public class TunaCanBomb : MonoBehaviour
    {
        private int damage;
        private float lifetime;
        private GameObject explosionVfxPrefab;
        private bool triggered;

        public void Initialize(int d, float life, GameObject vfx)
        {
            damage = d;
            lifetime = life;
            explosionVfxPrefab = vfx;
            Destroy(gameObject, lifetime);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (triggered) return;

            if (other.TryGetComponent<EnemyController>(out var enemy))
            {
                triggered = true;
                Explode(enemy);
            }
        }

        private void Explode(EnemyController target)
        {
            float explosionRadius = 2.5f;
            if (explosionVfxPrefab != null)
            {
                var vfx = Instantiate(explosionVfxPrefab, transform.position, Quaternion.identity);
                vfx.transform.localScale = Vector3.one * 2.5f; // Increased from 0.05f
                Debug.Log($"Tuna Can Bomb Exploded at {transform.position}, Scale: {vfx.transform.localScale}");
                Destroy(vfx, 0.5f);
            }

            // Area of Effect damage
            var colliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
            foreach (var col in colliders)
            {
                if (col.TryGetComponent<EnemyController>(out var enemy))
                {
                    enemy.ApplyDamage(damage);
                }
            }
            
            Destroy(gameObject);
        }
    }
}
