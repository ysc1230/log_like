using Survivor2D.Input;
using Survivor2D.CameraControl;
using UnityEngine;

namespace Survivor2D.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Animator))]
    public class PlayerMover : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerStats playerStats;
        [SerializeField] private TouchJoystickInput input;
        [SerializeField] private Camera mainCamera;

        [Header("Movement Settings")]
        [SerializeField] private float acceleration = 50f;
        [SerializeField] private float deceleration = 60f;

        private Rigidbody2D rb;
        private Animator animator;
        private SpriteRenderer spriteRenderer;
        private Vector2 moveInput;
        private Vector2 currentVelocity;

        // Animator Parameter Hashes
        private static readonly int MoveSpeedHash = Animator.StringToHash("MoveSpeed");
        private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
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

            UpdateAnimations();
        }

        private void UpdateAnimations()
        {
            float speedPercent = rb.linearVelocity.magnitude / (playerStats != null ? playerStats.MoveSpeed : 1f);
            animator.SetBool(IsMovingHash, moveInput.sqrMagnitude > 0.01f || rb.linearVelocity.sqrMagnitude > 0.1f);
            animator.SetFloat(MoveSpeedHash, speedPercent);
        }

        private void FixedUpdate()
        {
            float maxSpeed = playerStats != null ? playerStats.MoveSpeed : 0f;
            Vector2 targetVelocity = moveInput * maxSpeed;

            float accelRate = (moveInput.sqrMagnitude > 0.01f) ? acceleration : deceleration;
            
            currentVelocity = rb.linearVelocity;
            currentVelocity = Vector2.MoveTowards(currentVelocity, targetVelocity, accelRate * Time.fixedDeltaTime);
            
            rb.linearVelocity = currentVelocity;
        }
    }
}


