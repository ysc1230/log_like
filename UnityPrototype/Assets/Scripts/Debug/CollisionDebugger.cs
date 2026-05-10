using UnityEngine;

namespace Project.DebugTools
{
    [ExecuteInEditMode]
    public class CollisionDebugger : MonoBehaviour
    {
        public Color colliderColor = Color.green;
        public bool showOnlySelected = false;

        private void OnDrawGizmos()
        {
            if (showOnlySelected) return;
            DrawColliders();
        }

        private void OnDrawGizmosSelected()
        {
            if (!showOnlySelected) return;
            DrawColliders();
        }

        private void DrawColliders()
        {
            var colliders = GetComponentsInChildren<Collider2D>();
            foreach (var col in colliders)
            {
                if (!col.enabled) continue;

                Gizmos.color = col.isTrigger ? Color.yellow : colliderColor;
                
                if (col is BoxCollider2D box)
                {
                    Matrix4x4 rotationMatrix = Matrix4x4.TRS(box.transform.position, box.transform.rotation, box.transform.lossyScale);
                    Gizmos.matrix = rotationMatrix;
                    Gizmos.DrawWireCube((Vector3)box.offset, (Vector3)box.size);
                }
                else if (col is CircleCollider2D circle)
                {
                    Gizmos.matrix = Matrix4x4.identity;
                    Vector3 worldCenter = col.transform.TransformPoint(circle.offset);
                    float worldRadius = circle.radius * Mathf.Max(col.transform.lossyScale.x, col.transform.lossyScale.y);
                    Gizmos.DrawWireSphere(worldCenter, worldRadius);
                }
                else if (col is CapsuleCollider2D capsule)
                {
                    // Approximation for capsule
                    Gizmos.matrix = Matrix4x4.TRS(capsule.transform.position, capsule.transform.rotation, capsule.transform.lossyScale);
                    Gizmos.DrawWireCube((Vector3)capsule.offset, (Vector3)capsule.size);
                }
            }
            Gizmos.matrix = Matrix4x4.identity;
        }
    }
}
