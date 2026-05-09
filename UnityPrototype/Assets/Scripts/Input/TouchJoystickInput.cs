using UnityEngine;

namespace Survivor2D.Input
{
    public class TouchJoystickInput : MonoBehaviour
    {
        [SerializeField] private RectTransform joystickArea;
        [SerializeField] private float deadZone = 0.1f;

        public Vector2 MoveVector { get; private set; }

        private int activeFingerId = -1;

        private void Update()
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            var x = UnityEngine.Input.GetAxisRaw("Horizontal");
            var y = UnityEngine.Input.GetAxisRaw("Vertical");
            var editorInput = new Vector2(x, y);
            MoveVector = editorInput.sqrMagnitude < deadZone * deadZone ? Vector2.zero : editorInput.normalized;
#else
            HandleTouchInput();
#endif
        }

        private void HandleTouchInput()
        {
            if (UnityEngine.Input.touchCount == 0)
            {
                MoveVector = Vector2.zero;
                activeFingerId = -1;
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
                MoveVector = Vector2.zero;
                return;
            }

            for (int i = 0; i < UnityEngine.Input.touchCount; i++)
            {
                var touch = UnityEngine.Input.GetTouch(i);
                if (touch.fingerId != activeFingerId) continue;

                if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                {
                    MoveVector = Vector2.zero;
                    activeFingerId = -1;
                    return;
                }

                var center = (Vector2)joystickArea.position;
                var delta = (touch.position - center) / (joystickArea.rect.width * 0.5f);
                MoveVector = delta.magnitude < deadZone ? Vector2.zero : Vector2.ClampMagnitude(delta, 1f);
                return;
            }

            MoveVector = Vector2.zero;
            activeFingerId = -1;
        }
    }
}
