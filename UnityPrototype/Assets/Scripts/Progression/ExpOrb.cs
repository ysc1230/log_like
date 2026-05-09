using UnityEngine;

namespace Survivor2D.Progression
{
    public class ExpOrb : MonoBehaviour
    {
        [SerializeField] private float pickupRadius = 0.6f;

        private LevelSystem levelSystem;
        private Transform player;
        private int expAmount = 1;

        public void SetContext(Transform playerTransform, LevelSystem targetLevelSystem)
        {
            player = playerTransform;
            levelSystem = targetLevelSystem;
        }

        public void Initialize(int amount) => expAmount = amount;

        private void Update()
        {
            if (player == null || levelSystem == null) return;
            var dist = Vector2.Distance(transform.position, player.position);
            if (dist > pickupRadius) return;

            levelSystem.AddExp(expAmount);
            Destroy(gameObject);
        }
    }
}
