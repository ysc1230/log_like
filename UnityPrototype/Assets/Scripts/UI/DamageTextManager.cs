using UnityEngine;
using System.Collections.Generic;

namespace Survivor2D.UI
{
    public class DamageTextManager : MonoBehaviour
    {
        public static DamageTextManager Instance { get; private set; }

        [SerializeField] private DamageText prefab;
        [SerializeField] private int poolSize = 20;

        private List<DamageText> pool = new List<DamageText>();

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            for (int i = 0; i < poolSize; i++)
            {
                var dt = Instantiate(prefab, transform);
                dt.gameObject.SetActive(false);
                pool.Add(dt);
            }
        }

        public void ShowDamage(Vector3 position, int amount, bool isPlayer)
        {
            var text = GetFromPool();
            if (text == null) return;

            text.transform.position = position + Vector3.up * 0.5f;
            Color color = isPlayer ? Color.red : Color.yellow;
            text.Initialize(amount.ToString(), color);
        }

        private DamageText GetFromPool()
        {
            foreach (var dt in pool)
            {
                if (!dt.gameObject.activeSelf) return dt;
            }
            return null;
        }
    }
}
