using UnityEngine;

namespace Squidbasket.Input
{
    /// <summary>
    /// Turns a raw analog-stick Vector2 (a constant-magnitude direction while the stick is held
    /// over, independent of framerate) into a per-frame delta comparable in scale to a mouse
    /// delta, so ThirdPersonCamera/FirstPersonCamera's mouse-tuned sensitivity fields produce a
    /// consistent turn rate regardless of which device drove Look (PlayerInputSource applies this
    /// before exposing LookInput).
    /// </summary>
    public static class GamepadLookScaler
    {
        // Caps the deltaTime this scales by so a frame hitch (GC pause, load stall) can't turn a
        // held-over stick deflection into a single huge, uncorrectable snap-turn — unlike mouse
        // look's raw per-frame Pointer delta, this path multiplies by deltaTime and has no other
        // clamp between here and the camera's yaw/pitch accumulation.
        private const float MaxDeltaTime = 1f / 15f;

        public static Vector2 Scale(Vector2 rawStickInput, float lookSpeed, float deltaTime)
        {
            return rawStickInput * lookSpeed * Mathf.Min(deltaTime, MaxDeltaTime);
        }
    }
}
