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

        public CameraPose GetDesiredPose(Transform pivot, Vector2 lookInput, float deltaTime)
        {
            _yaw += lookInput.x * sensitivity;
            _pitch = Mathf.Clamp(_pitch - lookInput.y * sensitivity, minPitch, maxPitch);

            Quaternion rotation = Quaternion.Euler(_pitch, _yaw, 0f);
            Vector3 position = pivot.position + Vector3.up * eyeHeight;

            return new CameraPose(position, rotation);
        }
    }
}
