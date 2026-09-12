namespace Squidbasket.Scoring
{
    /// <summary>
    /// The two court regions a Shot can be released from (CONTEXT.md: "Zone"). There is no
    /// outer boundary — every point beyond the three-point arc counts as ThreePoint.
    /// </summary>
    public enum Zone
    {
        TwoPoint,
        ThreePoint
    }
}
