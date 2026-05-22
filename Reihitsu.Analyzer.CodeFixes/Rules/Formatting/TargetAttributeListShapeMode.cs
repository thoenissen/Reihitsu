namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// List-shape mode used by target-based attribute code fixes
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