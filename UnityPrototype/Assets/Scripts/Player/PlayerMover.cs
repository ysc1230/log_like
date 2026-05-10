using Survivor2D.Input;
using Survivor2D.CameraControl;
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
        private SpriteRenderer spriteRenderer;
        private Vector2 moveInput;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();

            if (mainCamera != null)
            {
                var follow = mainCamera.GetComponent<CameraFollow>();
                if (follow != null)
                {
                    follow.SetTarget(transform);
                }
            }
        }

        private void Update()
        {
            moveInput = input != null ? input.MoveVector : Vector2.zero;
            
            if (spriteRenderer != null && moveInput.x != 0)
            {
                spriteRenderer.flipX = moveInput.x < 0;
            }
        }

        private void FixedUpdate()
        {
            var speed = playerStats != null ? playerStats.MoveSpeed : 0f;
            rb.linearVelocity = moveInput * speed;
        }
    }
}

