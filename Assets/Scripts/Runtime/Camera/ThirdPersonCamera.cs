using UnityEngine;

namespace Squidbasket.Camera
{
    /// <summary>
    /// Hand-rolled orbit camera for Walking (ADR-0003 — a deliberate deviation from Cinemachine).
    /// PlayerStateController lerps the single shared Camera toward the pose this returns.
    /// </summary>
    public sealed class ThirdPersonCamera : MonoBehaviour
    {
        [SerializeField] private float distance = 4f;
        [SerializeField] private float height = 1.6f;
        [SerializeField] private float sensitivity = 2f;
        [SerializeField] private float minPitch = -30f;
        [SerializeField] private float maxPitch = 60f;

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
            Vector3 position = pivot.position + Vector3.up * height - rotation * Vector3.forward * distance;

            return new CameraPose(position, rotation);
        }
    }
}
