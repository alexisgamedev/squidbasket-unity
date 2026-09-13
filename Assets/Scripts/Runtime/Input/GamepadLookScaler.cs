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
        public static Vector2 Scale(Vector2 rawStickInput, float lookSpeed, float deltaTime)
        {
            return rawStickInput * lookSpeed * deltaTime;
        }
    }
}
