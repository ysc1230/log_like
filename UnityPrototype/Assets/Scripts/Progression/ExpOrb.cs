using UnityEngine;

namespace Survivor2D.Progression
{
    public class ExpOrb : MonoBehaviour
    {
        [SerializeField] private float magnetRange = 3.0f;
        [SerializeField] private float magnetSpeed = 6.0f;
        [SerializeField] private float pickupRadius = 0.5f;

        private LevelSystem levelSystem;
        private Transform player;
        private int expAmount = 1;
        private bool isFlying;

        public void SetContext(Transform playerTransform, LevelSystem targetLevelSystem)
        {
            player = playerTransform;
            levelSystem = targetLevelSystem;
        }

        public void Initialize(int amount) => expAmount = amount;

        private void Update()
        {
            if (levelSystem == null) return;
            
            // Try to find player if not assigned
            if (player == null)
            {
                var p = GameObject.FindGameObjectWithTag("Player");
                if (p != null) player = p.transform;
                else return;
            }

            var dist = Vector2.Distance(transform.position, player.position);

            // Start flying if within magnet range
            if (!isFlying && dist <= magnetRange)
            {
                isFlying = true;
            }

            if (isFlying)
            {
                // Move towards player
                transform.position = Vector3.MoveTowards(transform.position, player.position, magnetSpeed * Time.deltaTime);
                
                // Acceleration effect
                magnetSpeed += Time.deltaTime * 5f;

                // Collect if close enough
                if (dist <= pickupRadius)
                {
                    Collect();
                }
            }
        }

        private void Collect()
        {
            levelSystem.AddExp(expAmount);
            Destroy(gameObject);
        }
    }
}

