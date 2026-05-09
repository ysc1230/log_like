using Survivor2D.Input;
using UnityEngine;

namespace Survivor2D.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerMover : MonoBehaviour
    {
        [SerializeField] private PlayerStats playerStats;
        [SerializeField] private TouchJoystickInput input;
        [SerializeField] private Camera mainCamera;

        private Rigidbody2D rb;
        private Vector2 moveInput;

        private void Awake() => rb = GetComponent<Rigidbody2D>();

        private void Update() => moveInput = input != null ? input.MoveVector : Vector2.zero;

        private void FixedUpdate()
        {
            var speed = playerStats != null ? playerStats.MoveSpeed : 0f;
            rb.velocity = moveInput * speed;
            ClampToScreen();
        }

        private void ClampToScreen()
        {
            if (mainCamera == null) return;
            var pos = transform.position;
            var min = mainCamera.ViewportToWorldPoint(new Vector3(0f, 0f, 0f));
            var max = mainCamera.ViewportToWorldPoint(new Vector3(1f, 1f, 0f));

            pos.x = Mathf.Clamp(pos.x, min.x + 0.3f, max.x - 0.3f);
            pos.y = Mathf.Clamp(pos.y, min.y + 0.3f, max.y - 0.3f);
            transform.position = pos;
        }
    }
}
