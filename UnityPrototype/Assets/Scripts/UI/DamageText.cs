using UnityEngine;
using TMPro;

namespace Survivor2D.UI
{
    public class DamageText : MonoBehaviour
    {
        [SerializeField] private TextMeshPro textMesh;
        [SerializeField] private float moveSpeed = 1f;
        [SerializeField] private float fadeDuration = 0.5f;
        [SerializeField] private float lifeTime = 0.8f;

        private float timer;
        private Color startColor;

        public void Initialize(string value, Color color)
        {
            textMesh.text = value;
            textMesh.color = color;
            startColor = color;
            timer = 0;
            gameObject.SetActive(true);
        }

        private void Update()
        {
            timer += Time.deltaTime;
            transform.position += Vector3.up * (moveSpeed * Time.deltaTime);

            if (timer > lifeTime - fadeDuration)
            {
                float alpha = Mathf.Lerp(1, 0, (timer - (lifeTime - fadeDuration)) / fadeDuration);
                textMesh.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            }

            if (timer >= lifeTime)
            {
                gameObject.SetActive(false);
            }
        }
    }
}
