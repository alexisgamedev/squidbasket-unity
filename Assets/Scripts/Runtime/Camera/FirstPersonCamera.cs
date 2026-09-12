using UnityEngine;

namespace Squidbasket.Camera
{
    /// <summary>
    /// Hand-rolled first-person aiming camera used only during Shooting (ADR-0003). Rotates
    /// directly from the player's eye position with no orbit lag, for precision aiming.
    /// </summary>
    public sealed class FirstPersonCamera : MonoBehaviour
    {
        [SerializeField] private float eyeHeight = 1.7f;
        [SerializeField] private float sensitivity = 2f;

        // Bullet Time (ADR-0002) doesn't naturally damp aim: lookInput is a per-frame device
        // delta, not something multiplied by deltaTime, so it isn't affected by Time.timeScale
        // the way movement/physics are. This multiplier is Shooting's dedicated, independently
        // tunable answer to "should aim feel slowed too" rather than an accident of the timescale.
        [SerializeField] private float bulletTimeSensitivityMultiplier = 1f;

        [SerializeField] private float minPitch = -60f;
        [SerializeField] private float maxPitch = 80f;

        private float _yaw;
        private float _pitch;

        public float Yaw => _yaw;
        public float Pitch => _pitch;

        public void SetOrientation(float yaw, float pitch)
        {
            _yaw = yaw;
            _pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        }

        // lookInput is a per-frame mouse delta (or already-scaled stick input), not a rate, so it
        // is applied directly with no deltaTime multiply.
        public CameraPose GetDesiredPose(Transform pivot, Vector2 lookInput)
        {
            float effectiveSensitivity = sensitivity * bulletTimeSensitivityMultiplier;
            _yaw += lookInput.x * effectiveSensitivity;
            _pitch = Mathf.Clamp(_pitch - lookInput.y * effectiveSensitivity, minPitch, maxPitch);

            Quaternion rotation = Quaternion.Euler(_pitch, _yaw, 0f);
            Vector3 position = pivot.position + Vector3.up * eyeHeight;

            return new CameraPose(position, rotation);
        }
    }
}
