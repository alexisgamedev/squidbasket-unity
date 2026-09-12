using Squidbasket.Player;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Squidbasket.Input
{
    /// <summary>
    /// Thin adapter over Unity's Input System implementing <see cref="IPlayerInputSource"/> so
    /// gameplay code itself has no Input System dependency (ADR-0003).
    /// </summary>
    public sealed class UnityInputPlayerInputSource : MonoBehaviour, IPlayerInputSource
    {
        [SerializeField] private InputActionAsset actions;
        [SerializeField] private string actionMapName = "Player";
        [SerializeField] private string moveActionName = "Move";
        [SerializeField] private string lookActionName = "Look";
        [SerializeField] private string shootActionName = "Shoot";
        [SerializeField] private string resetActionName = "Reset";
        [SerializeField] private string restartActionName = "Restart";

        private InputAction _moveAction;
        private InputAction _lookAction;
        private InputAction _shootAction;
        private InputAction _resetAction;
        private InputAction _restartAction;

        public Vector2 MoveInput => _moveAction != null ? _moveAction.ReadValue<Vector2>() : Vector2.zero;
        public Vector2 LookInput => _lookAction != null ? _lookAction.ReadValue<Vector2>() : Vector2.zero;
        public bool ShootHeld => _shootAction != null && _shootAction.IsPressed();
        public bool ResetPressed => _resetAction != null && _resetAction.WasPressedThisFrame();
        public bool RestartPressed => _restartAction != null && _restartAction.WasPressedThisFrame();

        private void Awake()
        {
            if (actions == null)
            {
                Debug.LogError($"{nameof(UnityInputPlayerInputSource)} has no {nameof(InputActionAsset)} assigned.", this);
                return;
            }

            InputActionMap map = actions.FindActionMap(actionMapName, throwIfNotFound: true);
            _moveAction = map.FindAction(moveActionName, throwIfNotFound: true);
            _lookAction = map.FindAction(lookActionName, throwIfNotFound: true);
            _shootAction = map.FindAction(shootActionName, throwIfNotFound: true);
            _resetAction = map.FindAction(resetActionName, throwIfNotFound: true);
            _restartAction = map.FindAction(restartActionName, throwIfNotFound: true);
        }

        private void OnEnable()
        {
            _moveAction?.Enable();
            _lookAction?.Enable();
            _shootAction?.Enable();
            _resetAction?.Enable();
            _restartAction?.Enable();
        }

        private void OnDisable()
        {
            _moveAction?.Disable();
            _lookAction?.Disable();
            _shootAction?.Disable();
            _resetAction?.Disable();
            _restartAction?.Disable();
        }
    }
}
