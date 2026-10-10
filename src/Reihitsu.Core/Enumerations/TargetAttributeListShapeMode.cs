namespace Reihitsu.Core.Enumerations;

/// <summary>
/// List-shape policy for target attribute rules
/// </summary>
public enum TargetAttributeListShapeMode
{
    /// <summary>
    /// Require one attribute per list
    /// </summary>
    SplitLists,

    /// <summary>
    /// Require merged lists when multiple attributes are present
    /// </summary>
    MergedList
}