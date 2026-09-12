using UnityEngine;

namespace Squidbasket.Camera
{
    /// <summary>A desired world-space position/rotation for the single shared Camera (ADR-0003).</summary>
    public readonly struct CameraPose
    {
        public CameraPose(Vector3 position, Quaternion rotation)
        {
            Position = position;
            Rotation = rotation;
        }

        public Vector3 Position { get; }
        public Quaternion Rotation { get; }
    }
}
