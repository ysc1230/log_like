using Survivor2D.Player;
using Survivor2D.Progression;
using UnityEngine;

namespace Survivor2D.Enemy
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class EnemyController : MonoBehaviour
    {
        [SerializeField] private int maxHealth = 30; // Increased from 10
        [SerializeField] private float moveSpeed = 3.0f;
        [SerializeField] private int contactDamage = 5;
        [SerializeField] private ExpOrb expOrbPrefab;

        private Transform target;
        private EnemyRegistry registry;
        private LevelSystem levelSystem;
        private SpriteRenderer spriteRenderer;
        private Rigidbody2D rb;
        private float runtimeSpeed;
        private int currentHealth;
        private Vector2 moveDirection;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            rb = GetComponent<Rigidbody2D>();
            
            // Set optimal physics settings for enemies
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
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
            if (target == null)
            {
                moveDirection = Vector2.zero;
                return;
            }

            var delta = target.position - transform.position;
            moveDirection = delta.normalized;

            if (spriteRenderer != null && moveDirection.x != 0)
            {
                spriteRenderer.flipX = moveDirection.x < 0;
            }
        }

        private void FixedUpdate()
        {
            if (rb == null) return;
            rb.linearVelocity = moveDirection * runtimeSpeed;
        }

        public void ApplyDamage(int amount)
        {
            currentHealth -= amount;
            Debug.Log($"[EnemyController] {gameObject.name} took {amount} damage. HP: {currentHealth}");
            Survivor2D.UI.DamageTextManager.Instance?.ShowDamage(transform.position, amount, false);
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

        private void OnTriggerStay2D(Collider2D other)
        {
            if (other.CompareTag("Player") || other.name == "Hitbox")
            {
                var player = other.GetComponentInParent<PlayerStats>();
                if (player != null)
                {
                    player.TakeDamage(contactDamage);
                }
            }
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            // Keep for physical pushing if needed, but damage moved to trigger
        }
}
}
