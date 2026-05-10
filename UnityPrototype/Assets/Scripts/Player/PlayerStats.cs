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
        [SerializeField] private float attackCooldown = 1.6f;

        public int CurrentHealth { get; private set; }
        public int MaxHealth => maxHealth;
        public float MoveSpeed => moveSpeed;
        public int AttackDamage => attackDamage;
        public float AttackCooldown => attackCooldown;

        public void ResetRuntimeState()
        {
            CurrentHealth = maxHealth;
            GameEvents.RaiseHealthChanged(CurrentHealth, maxHealth);
            GameEvents.RaiseStatsChanged(attackDamage, attackCooldown, moveSpeed);
        }

        [SerializeField] private float iFrameDuration = 0.2f;
        private float iFrameTimer;

        public void TakeDamage(int amount)
        {
            if (CurrentHealth <= 0 || iFrameTimer > 0) return;
            
            CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
            iFrameTimer = iFrameDuration;
            
            Debug.Log($"[PlayerStats] Player took {amount} damage. HP: {CurrentHealth}");
            Survivor2D.UI.DamageTextManager.Instance?.ShowDamage(transform.position, amount, true);
            GameEvents.RaiseHealthChanged(CurrentHealth, maxHealth);
            if (CurrentHealth == 0) GameEvents.RaiseGameOver();
        }

        private void Update()
        {
            if (iFrameTimer > 0) iFrameTimer -= Time.deltaTime;
        }

        public void AddMoveSpeed(float value) 
        { 
            moveSpeed += value; 
            GameEvents.RaiseStatsChanged(attackDamage, attackCooldown, moveSpeed);
        }
        
        public void AddAttackDamage(int value) 
        { 
            attackDamage += value; 
            GameEvents.RaiseStatsChanged(attackDamage, attackCooldown, moveSpeed);
        }
        
        public void AddAttackRateMultiplier(float multiplier) 
        { 
            attackCooldown = Mathf.Max(0.1f, attackCooldown * multiplier);
            GameEvents.RaiseStatsChanged(attackDamage, attackCooldown, moveSpeed);
        }
    }
}
