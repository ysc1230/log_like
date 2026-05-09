using Survivor2D.Core;
using UnityEngine;

namespace Survivor2D.Player
{
    public class PlayerStats : MonoBehaviour
    {
        [Header("Base")]
        [SerializeField] private int maxHealth = 100;
        [SerializeField] private float moveSpeed = 4f;
        [SerializeField] private int attackDamage = 10;
        [SerializeField] private float attackCooldown = 0.8f;

        public int CurrentHealth { get; private set; }
        public int MaxHealth => maxHealth;
        public float MoveSpeed => moveSpeed;
        public int AttackDamage => attackDamage;
        public float AttackCooldown => attackCooldown;

        public void ResetRuntimeState()
        {
            CurrentHealth = maxHealth;
            GameEvents.RaiseHealthChanged(CurrentHealth, maxHealth);
        }

        public void TakeDamage(int amount)
        {
            if (CurrentHealth <= 0) return;
            CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
            GameEvents.RaiseHealthChanged(CurrentHealth, maxHealth);
            if (CurrentHealth == 0) GameEvents.RaiseGameOver();
        }

        public void AddMoveSpeed(float value) => moveSpeed += value;
        public void AddAttackDamage(int value) => attackDamage += value;
        public void AddAttackRateMultiplier(float multiplier) => attackCooldown = Mathf.Max(0.1f, attackCooldown * multiplier);
    }
}
