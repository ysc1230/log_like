using Survivor2D.Enemy;
using UnityEngine;

namespace Survivor2D.Combat
{
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private float speed = 8f;
        [SerializeField] private float lifeTime = 2f;

        private Vector2 direction;
        private int damage;
        private float lifeTimer;
        private ProjectilePool pool;
        private bool isPiercing;
        private int remainingPierces;

        public void SetPool(ProjectilePool ownerPool) => pool = ownerPool;

        public void Initialize(Vector2 dir, int attackDamage, bool piercing = false)
        {
            direction = dir.normalized;
            damage = attackDamage;
            lifeTimer = 0f;
            isPiercing = piercing;
            remainingPierces = isPiercing ? 1 : 0; // Simple pierce: 1 extra hit

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        private void Update()
        {
            transform.position += (Vector3)(direction * (speed * Time.deltaTime));
            lifeTimer += Time.deltaTime;
            if (lifeTimer >= lifeTime) Release();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.TryGetComponent<Survivor2D.Enemy.EnemyController>(out var enemy)) return;
            enemy.ApplyDamage(damage);
            
            if (isPiercing && remainingPierces > 0)
            {
                remainingPierces--;
            }
            else
            {
                Release();
            }
        }

        private void Release()
        {
            if (pool != null) pool.Return(this);
            else Destroy(gameObject);
        }
    }
}
