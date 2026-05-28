namespace Reihitsu.Core.Enumerations;

/// <summary>
/// Placement policy for target attribute rules
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