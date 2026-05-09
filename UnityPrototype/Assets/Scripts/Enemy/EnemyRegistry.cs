using System.Collections.Generic;
using UnityEngine;

namespace Survivor2D.Enemy
{
    public class EnemyRegistry : MonoBehaviour
    {
        [SerializeField] private float searchRange = 8f;

        private readonly HashSet<EnemyController> enemies = new();

        public void Register(EnemyController enemy)
        {
            if (enemy != null) enemies.Add(enemy);
        }

        public void Unregister(EnemyController enemy)
        {
            if (enemy != null) enemies.Remove(enemy);
        }

        public bool TryGetNearest(Vector3 from, out EnemyController nearest)
        {
            nearest = null;
            var rangeSqr = searchRange * searchRange;
            var nearestSqr = float.MaxValue;

            foreach (var enemy in enemies)
            {
                if (enemy == null || !enemy.gameObject.activeInHierarchy) continue;
                var sqr = (enemy.transform.position - from).sqrMagnitude;
                if (sqr > rangeSqr || sqr >= nearestSqr) continue;
                nearestSqr = sqr;
                nearest = enemy;
            }

            return nearest != null;
        }
    }
}
