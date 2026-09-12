using UnityEngine;

namespace Squidbasket.Scoring
{
    /// <summary>
    /// Pure math for Shot outcomes: which Zone a release point falls in, its point value, and
    /// the launch velocity for a given camera direction and Power (ADR-0002/ADR-0003).
    /// </summary>
    public static class ShotCalculator
    {
        public static Zone DetermineZone(Vector3 releasePosition, Vector3 hoopPosition, float threePointRadius)
        {
            Vector3 flatOffset = releasePosition - hoopPosition;
            flatOffset.y = 0f;
            return flatOffset.magnitude >= threePointRadius ? Zone.ThreePoint : Zone.TwoPoint;
        }

        public static int PointsForZone(Zone zone) => zone == Zone.ThreePoint ? 3 : 2;

        /// <summary>
        /// Combines the camera's forward direction with an Up vector scaled by Power to produce
        /// the throwing arc described in CONTEXT.md's "Shooting" entry.
        /// </summary>
        public static Vector3 ComputeLaunchVelocity(Vector3 cameraForward, float power, float upArcScale, float speed)
        {
            Vector3 direction = cameraForward.normalized + Vector3.up * (power * upArcScale);
            return direction.normalized * speed;
        }
    }
}
