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

        private InputActionReference[] _actions;

        public Vector2 MoveInput => moveAction != null && moveAction.action != null
            ? moveAction.action.ReadValue<Vector2>() : Vector2.zero;
        public Vector2 LookInput => lookAction != null && lookAction.action != null
            ? lookAction.action.ReadValue<Vector2>() : Vector2.zero;
        public bool ShootHeld => shootAction != null && shootAction.action != null && shootAction.action.IsPressed();

        /// <summary>True for exactly the frame Reset was pressed.</summary>
        public bool ResetPressed => resetAction != null && resetAction.action != null && resetAction.action.WasPressedThisFrame();

        /// <summary>True for exactly the frame Restart was pressed.</summary>
        public bool RestartPressed => restartAction != null && restartAction.action != null && restartAction.action.WasPressedThisFrame();

        private void Awake()
        {
            _actions = new[] { moveAction, lookAction, shootAction, resetAction, restartAction };
            string[] fieldNames = { nameof(moveAction), nameof(lookAction), nameof(shootAction), nameof(resetAction), nameof(restartAction) };

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
    }
}
