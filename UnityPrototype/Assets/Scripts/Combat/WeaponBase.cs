using UnityEngine;

namespace Survivor2D.Combat
{
    public abstract class WeaponBase : MonoBehaviour
    {
        protected int level = 0;
        protected float cooldownTimer;
        private float attackRateMultiplier = 1f;

        public virtual void SetLevel(int newLevel)
        {
            if (level == 0 && newLevel > 0)
            {
                // First activation: stagger initial cooldown
                cooldownTimer = Random.Range(0f, 0.5f);
            }
            level = newLevel;
        }

        protected abstract void Attack();

        public void SetAttackRateMultiplier(float multiplier)
        {
            attackRateMultiplier = Mathf.Max(0.1f, multiplier);
        }

        protected virtual void Update()
        {
            if (level <= 0) return;

            cooldownTimer -= Time.deltaTime * attackRateMultiplier;
            if (cooldownTimer <= 0)
            {
                Attack();
                ResetCooldown();
            }
        }

        protected abstract void ResetCooldown();
    }
}
