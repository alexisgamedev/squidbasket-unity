using UnityEngine;

namespace Squidbasket.Player
{
    /// <summary>
    /// Input abstraction consumed by gameplay code (ADR-0003). Gameplay has no dependency on
    /// Unity's Input System — only a concrete adapter (e.g. in Squidbasket.Input) does.
    /// </summary>
    public interface IPlayerInputSource
    {
        Vector2 MoveInput { get; }
        Vector2 LookInput { get; }
        bool ShootHeld { get; }

        /// <summary>True for exactly the frame Reset was pressed.</summary>
        bool ResetPressed { get; }
    }
}
