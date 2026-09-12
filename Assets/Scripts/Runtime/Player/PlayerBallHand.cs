using UnityEngine;

namespace Squidbasket.Player
{
    /// <summary>Marker + anchor exposed on the player so a loose Ball can attach itself on Retrieval.</summary>
    public sealed class PlayerBallHand : MonoBehaviour
    {
        [SerializeField] private Transform handAnchor;

        public Transform HandAnchor => handAnchor;
    }
}
