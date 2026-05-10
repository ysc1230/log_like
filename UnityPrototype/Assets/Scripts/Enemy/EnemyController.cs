using Survivor2D.Player;
using Survivor2D.Progression;
using UnityEngine;

namespace Survivor2D.Enemy
{
    public class EnemyController : MonoBehaviour
    {
        [SerializeField] private int maxHealth = 20;
        [SerializeField] private float moveSpeed = 1.5f;
        [SerializeField] private int contactDamage = 5;
        [SerializeField] private ExpOrb expOrbPrefab;

        private Transform target;
        private EnemyRegistry registry;
        private LevelSystem levelSystem;
        private SpriteRenderer spriteRenderer;
        private float runtimeSpeed;
        private int currentHealth;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void Initialize(Transform playerTarget, float speedMultiplier, EnemyRegistry enemyRegistry, LevelSystem targetLevelSystem)
        {
            target = playerTarget;
            registry = enemyRegistry;
            runtimeSpeed = moveSpeed * speedMultiplier;
            levelSystem = targetLevelSystem;
            currentHealth = maxHealth;
            registry?.Register(this);
            gameObject.SetActive(true);
        }

        private void Update()
        {
            if (target == null) return;
            var dir = (target.position - transform.position).normalized;
            transform.position += dir * (runtimeSpeed * Time.deltaTime);

            if (spriteRenderer != null && dir.x != 0)
            {
                spriteRenderer.flipX = dir.x < 0;
            }
        }

        public void ApplyDamage(int amount)
        {
            currentHealth -= amount;
            if (currentHealth <= 0) Die();
        }

        private void Die()
        {
            registry?.Unregister(this);
            if (expOrbPrefab != null)
            {
                var orb = Instantiate(expOrbPrefab, transform.position, Quaternion.identity);
                orb.SetContext(target, levelSystem);
                orb.Initialize(1);
            }
            Destroy(gameObject);
        }

        private void OnDisable() => registry?.Unregister(this);

        private void OnCollisionStay2D(Collision2D collision)
        {
            if (collision.gameObject.TryGetComponent<PlayerStats>(out var player))
            {
                player.TakeDamage(contactDamage);
            }
        }
    }
}
