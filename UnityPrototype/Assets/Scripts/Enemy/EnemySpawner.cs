using UnityEngine;

namespace Survivor2D.Enemy
{
    public class EnemySpawner : MonoBehaviour
    {
        [SerializeField] private EnemyController enemyPrefab;
        [SerializeField] private Transform player;
        [SerializeField] private Camera mainCamera;
        [SerializeField] private EnemyRegistry enemyRegistry;
        [SerializeField] private Survivor2D.Progression.LevelSystem levelSystem;
        [SerializeField] private float startSpawnInterval = 0.25f;
        [SerializeField] private float minSpawnInterval = 0.0625f;
        [SerializeField] private float intervalReducePerSecond = 0.08f;

        private float timer;
        private float elapsed;

        private void Update()
        {
            elapsed += Time.deltaTime;
            timer += Time.deltaTime;
            var currentInterval = Mathf.Max(minSpawnInterval, startSpawnInterval - elapsed * intervalReducePerSecond);
            if (timer < currentInterval) return;
            timer = 0f;
            SpawnEnemy();
        }

        private void SpawnEnemy()
        {
            if (enemyPrefab == null || player == null || mainCamera == null) return;

            var viewportPoint = RandomSpawnEdgePoint();
            float distance = Mathf.Abs(mainCamera.transform.position.z);
            var worldPos = mainCamera.ViewportToWorldPoint(new Vector3(viewportPoint.x, viewportPoint.y, distance));
            worldPos.z = 0f;

            var enemy = Instantiate(enemyPrefab, worldPos, Quaternion.identity);
            enemy.Initialize(player, 1f + elapsed * 0.01f, enemyRegistry, levelSystem);
            // Debug.Log($"[EnemySpawner] Spawned {enemy.name} at {worldPos}");
            }

        private Vector2 RandomSpawnEdgePoint()
        {
            var side = Random.Range(0, 4);
            return side switch
            {
                0 => new Vector2(Random.value, -0.1f),
                1 => new Vector2(Random.value, 1.1f),
                2 => new Vector2(-0.1f, Random.value),
                _ => new Vector2(1.1f, Random.value)
            };
        }
    }
}
