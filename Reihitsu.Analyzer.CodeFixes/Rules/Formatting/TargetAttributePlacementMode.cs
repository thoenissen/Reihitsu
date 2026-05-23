namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// Placement mode used by target-based attribute code fixes
/// </summary>
public enum TargetAttributePlacementMode
{
    /// <summary>
    /// Attribute list and following token must be on separate lines
    /// </summary>
    SeparateLine,

    /// <summary>
    /// Attribute list and following token must be on the same line
    /// </summary>
    SingleLine
}