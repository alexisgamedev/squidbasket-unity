using UnityEngine;

namespace Squidbasket.Scoring
{
    /// <summary>
    /// Pure math for Shot outcomes: which Zone a release point falls in, its point value, the
    /// launch velocity for a given camera direction and Power, and the resulting flight arc
    /// (ADR-0002/ADR-0003).
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

        /// <summary>
        /// Samples the same physics that will govern the ball once released (constant-gravity
        /// projectile motion, no drag — matching the Ball Rigidbody's own settings), for drawing
        /// a Shot's trajectory preview. Writes into <paramref name="buffer"/> (no per-call
        /// allocation, since this runs every frame while Shooting) and returns how many of its
        /// entries were filled. Stops as soon as the arc reaches <paramref name="groundY"/> (or
        /// the buffer is full, whichever comes first) — deliberately the true ground height and
        /// not release height, since <paramref name="origin"/> is typically well above the
        /// ground (e.g. hand height) and a shot with little/no upward velocity would otherwise
        /// dip below its own release height on the very first sample, collapsing the preview to
        /// a stub.
        /// </summary>
        public static int SampleTrajectory(
            Vector3 origin, Vector3 velocity, Vector3 gravity, float groundY, float timeStep, Vector3[] buffer)
        {
            if (buffer.Length == 0)
            {
                return 0;
            }

            buffer[0] = origin;
            int count = 1;

            for (int i = 1; i < buffer.Length; i++)
            {
                float t = i * timeStep;
                Vector3 point = origin + velocity * t + 0.5f * gravity * (t * t);
                buffer[count++] = point;

                if (point.y < groundY)
                {
                    break;
                }
            }

            return count;
        }
    }
}
