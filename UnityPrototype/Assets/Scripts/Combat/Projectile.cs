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

        public void SetPool(ProjectilePool ownerPool) => pool = ownerPool;

        public void Initialize(Vector2 dir, int attackDamage)
        {
            direction = dir.normalized;
            damage = attackDamage;
            lifeTimer = 0f;
        }

        private void Update()
        {
            transform.position += (Vector3)(direction * (speed * Time.deltaTime));
            lifeTimer += Time.deltaTime;
            if (lifeTimer >= lifeTime) Release();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.TryGetComponent<EnemyController>(out var enemy)) return;
            enemy.ApplyDamage(damage);
            Release();
        }

        private void Release()
        {
            if (pool != null) pool.Return(this);
            else Destroy(gameObject);
        }
    }
}
