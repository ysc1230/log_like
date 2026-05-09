using System.Collections.Generic;
using UnityEngine;

namespace Survivor2D.Combat
{
    public class ProjectilePool : MonoBehaviour
    {
        [SerializeField] private Projectile projectilePrefab;
        [SerializeField] private int preloadCount = 20;

        private readonly Queue<Projectile> pool = new();

        private void Awake()
        {
            for (int i = 0; i < preloadCount; i++)
            {
                var p = Create();
                Return(p);
            }
        }

        public Projectile Get(Vector3 position)
        {
            var projectile = pool.Count > 0 ? pool.Dequeue() : Create();
            projectile.transform.position = position;
            projectile.gameObject.SetActive(true);
            projectile.SetPool(this);
            return projectile;
        }

        public void Return(Projectile projectile)
        {
            if (projectile == null) return;
            projectile.gameObject.SetActive(false);
            pool.Enqueue(projectile);
        }

        private Projectile Create()
        {
            var projectile = Instantiate(projectilePrefab, transform);
            projectile.gameObject.SetActive(false);
            projectile.SetPool(this);
            return projectile;
        }
    }
}
