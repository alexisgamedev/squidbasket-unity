using Squidbasket.Input;
using Squidbasket.Scoring;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Squidbasket.UI
{
    /// <summary>
    /// Esc-toggled pause menu. Flips the sibling <see cref="Canvas"/>'s enabled flag rather than
    /// deactivating this GameObject, so this script (and its Esc poll) keeps running while the
    /// menu is hidden. The GraphicRaycaster is toggled alongside it so the invisible menu can't
    /// intercept clicks while closed, and the player's gameplay actions are disabled alongside it
    /// so Move/Look/Shoot don't keep running underneath the menu (the Menu action itself lives
    /// outside PlayerInputSource's tracked set, so Esc still closes the menu regardless).
    /// Individual controls wire their own persistent UnityEvent calls in the Inspector (e.g.
    /// Restart button -> ScoreSystem.Restart, Sensitivity slider ->
    /// ThirdPersonCamera/FirstPersonCamera.SetSensitivity) rather than routing through this
    /// script — Quit is the one exception, since Application.Quit needs a live instance method.
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    public sealed class MenuWindow : MonoBehaviour
    {
        [SerializeField] private InputActionReference toggleMenuAction;

        [SerializeField] private Button restartButton;
        [SerializeField] private Text sensitivityText;
        [SerializeField] private Slider sensitivitySlider;

        [Space]
        [SerializeField] private ScoreSystem scoreSystem;
        [SerializeField] private PlayerInputSource playerInput;

        private Canvas _canvas;
        private GraphicRaycaster _raycaster;

        private void Awake()
        {
            _canvas = GetComponent<Canvas>();
            _raycaster = GetComponent<GraphicRaycaster>();

            if (toggleMenuAction == null || toggleMenuAction.action == null)
            {
                Debug.LogError(
                    $"{nameof(MenuWindow)} has no {nameof(toggleMenuAction)} assigned — Esc will not " +
                    "open/close the menu.",
                    this);
            }

            if (playerInput == null)
            {
                Debug.LogWarning(
                    $"{nameof(MenuWindow)} has no {nameof(playerInput)} assigned — gameplay input will " +
                    "keep running underneath the menu while it's open.",
                    this);
            }

            restartButton.onClick.AddListener(() =>
            {
                scoreSystem.Restart();
                SetOpen(false);
            });

            sensitivitySlider.onValueChanged.AddListener(SetCameraSensitivity);
            SetCameraSensitivity(sensitivitySlider.value);
        }

        private void OnEnable() => toggleMenuAction?.action?.Enable();

        private void OnDisable() => toggleMenuAction?.action?.Disable();

        private void Update()
        {
            if (toggleMenuAction != null && toggleMenuAction.action != null
                && toggleMenuAction.action.WasPressedThisFrame())
            {
                SetOpen(!_canvas.enabled);
            }
        }

        private void SetOpen(bool open)
        {
            _canvas.enabled = open;

            if (_raycaster != null)
            {
                _raycaster.enabled = open;
            }

            playerInput?.SetInputEnabled(!open);

            if (open)
            {
                if(restartButton != null && EventSystem.current != null)
                    restartButton.Select();
            }
            else
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
        }

        public void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void SetCameraSensitivity(float newValue)
        {
            sensitivityText.text = $"Camera Sensitivity ({newValue.ToString("0.00")})";
        }
    }
}
