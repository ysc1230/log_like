using UnityEngine;

namespace Survivor2D.CameraControl
{
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private float smoothTime = 0.125f;
        [SerializeField] private Vector3 offset = new Vector3(0, 0, -10);

        private Vector3 _currentVelocity;

        private void LateUpdate()
        {
            if (target == null) return;

            Vector3 targetPosition = target.position + offset;
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref _currentVelocity, smoothTime);
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }
    }
}
