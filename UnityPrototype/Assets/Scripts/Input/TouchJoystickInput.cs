using UnityEngine;

namespace Survivor2D.Input
{
    public class TouchJoystickInput : MonoBehaviour
    {
        [SerializeField] private RectTransform joystickArea;
        [SerializeField] private RectTransform handle;
        [SerializeField] private float deadZone = 0.1f;
        [SerializeField] private float handleRange = 100f;

        public Vector2 MoveVector { get; private set; }

        private int activeFingerId = -1;
        private Vector2 _initialPosition;

        private void Awake()
        {
            if (joystickArea != null)
                _initialPosition = joystickArea.anchoredPosition;
        }

        private void Update()
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            var x = UnityEngine.Input.GetAxisRaw("Horizontal");
            var y = UnityEngine.Input.GetAxisRaw("Vertical");
            var editorInput = new Vector2(x, y);
            MoveVector = editorInput.sqrMagnitude < deadZone * deadZone ? Vector2.zero : editorInput.normalized;
            UpdateVisuals(MoveVector);
#else
            HandleTouchInput();
#endif
        }

        private void HandleTouchInput()
        {
            if (UnityEngine.Input.touchCount == 0)
            {
                ResetJoystick();
                return;
            }

            if (activeFingerId == -1)
            {
                for (int i = 0; i < UnityEngine.Input.touchCount; i++)
                {
                    var touch = UnityEngine.Input.GetTouch(i);
                    if (RectTransformUtility.RectangleContainsScreenPoint(joystickArea, touch.position))
                    {
                        activeFingerId = touch.fingerId;
                        break;
                    }
                }
            }

            if (activeFingerId == -1)
            {
                ResetJoystick();
                return;
            }

            for (int i = 0; i < UnityEngine.Input.touchCount; i++)
            {
                var touch = UnityEngine.Input.GetTouch(i);
                if (touch.fingerId != activeFingerId) continue;

                if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                {
                    ResetJoystick();
                    return;
                }

                var center = (Vector2)joystickArea.position;
                var delta = (touch.position - center);
                var normalizedDelta = delta / (joystickArea.rect.width * 0.5f);
                
                MoveVector = normalizedDelta.magnitude < deadZone ? Vector2.zero : Vector2.ClampMagnitude(normalizedDelta, 1f);
                UpdateVisuals(MoveVector);
                return;
            }

            ResetJoystick();
        }

        private void ResetJoystick()
        {
            MoveVector = Vector2.zero;
            activeFingerId = -1;
            UpdateVisuals(Vector2.zero);
        }

        private void UpdateVisuals(Vector2 move)
        {
            if (handle != null)
            {
                handle.anchoredPosition = move * handleRange;
            }
        }
    }
}

