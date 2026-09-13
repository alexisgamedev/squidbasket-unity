using UnityEngine;
using UnityEngine.InputSystem;

namespace Squidbasket.Input
{
    /// <summary>
    /// Reads player input via Unity's Input System, exposing it as plain properties for gameplay
    /// code to consume directly (ADR-0003).
    /// </summary>
    public sealed class PlayerInputSource : MonoBehaviour
    {
        [SerializeField] private InputActionReference moveAction;
        [SerializeField] private InputActionReference lookAction;
        [SerializeField] private InputActionReference shootAction;
        [SerializeField] private InputActionReference resetAction;
        [SerializeField] private InputActionReference restartAction;
        [SerializeField] private InputActionReference jumpAction;

        // Mouse delta is already a per-frame pixel delta, but a stick-based device (gamepad or
        // joystick) instead holds a constant-magnitude direction while tilted, so it needs this
        // deltaTime-scaled rate to behave like a comparable per-frame delta rather than turning
        // faster at higher framerates.
        [SerializeField] private float gamepadLookSpeed = 180f;

        private InputActionReference[] _actions;

        public Vector2 MoveInput => moveAction != null && moveAction.action != null
            ? moveAction.action.ReadValue<Vector2>() : Vector2.zero;

        public Vector2 LookInput
        {
            get
            {
                if (lookAction == null || lookAction.action == null)
                {
                    return Vector2.zero;
                }

                Vector2 raw = lookAction.action.ReadValue<Vector2>();
                InputDevice device = lookAction.action.activeControl?.device;

                // Unscaled: aim must not be damped by Bullet Time's Time.timeScale (see
                // FirstPersonCamera's own note that lookInput is deliberately timescale-independent,
                // and ShootingState's PowerBar for the same unscaledDeltaTime pattern).
                return device is Gamepad || device is Joystick
                    ? GamepadLookScaler.Scale(raw, gamepadLookSpeed, Time.unscaledDeltaTime)
                    : raw;
            }
        }

        public bool ShootHeld => shootAction != null && shootAction.action != null && shootAction.action.IsPressed();

        /// <summary>True for exactly the frame Reset was pressed.</summary>
        public bool ResetPressed => resetAction != null && resetAction.action != null && resetAction.action.WasPressedThisFrame();

        /// <summary>True for exactly the frame Restart was pressed.</summary>
        public bool RestartPressed => restartAction != null && restartAction.action != null && restartAction.action.WasPressedThisFrame();

        /// <summary>True for exactly the frame Jump was pressed.</summary>
        public bool JumpPressed => jumpAction != null && jumpAction.action != null && jumpAction.action.WasPressedThisFrame();

        private void Awake()
        {
            _actions = new[] { moveAction, lookAction, shootAction, resetAction, restartAction, jumpAction };
            string[] fieldNames =
            {
                nameof(moveAction), nameof(lookAction), nameof(shootAction), nameof(resetAction),
                nameof(restartAction), nameof(jumpAction)
            };

            for (int i = 0; i < _actions.Length; i++)
            {
                WarnIfMissing(_actions[i], fieldNames[i]);
            }
        }

        private void WarnIfMissing(InputActionReference reference, string fieldName)
        {
            if (reference == null)
            {
                Debug.LogError($"{nameof(PlayerInputSource)} has no {fieldName} assigned.", this);
            }
            else if (reference.action == null)
            {
                Debug.LogError(
                    $"{nameof(PlayerInputSource)}'s {fieldName} references an action that no longer " +
                    "exists in its InputActionAsset.", this);
            }
        }

        private void OnEnable()
        {
            foreach (InputActionReference reference in _actions)
            {
                reference?.action?.Enable();
            }
        }

        private void OnDisable()
        {
            foreach (InputActionReference reference in _actions)
            {
                reference?.action?.Disable();
            }
        }

        /// <summary>
        /// Enables/disables every tracked gameplay action at once — used by the pause menu
        /// (MenuWindow) so Move/Look/Shoot/Reset/Restart/Jump go dead while it's open, rather
        /// than racing the player's clicks on menu controls (the Menu action itself lives outside
        /// this set, so Esc still closes the menu regardless of this state).
        /// </summary>
        public void SetInputEnabled(bool enabled)
        {
            foreach (InputActionReference reference in _actions)
            {
                if (enabled)
                {
                    reference?.action?.Enable();
                }
                else
                {
                    reference?.action?.Disable();
                }
            }
        }
    }
}
