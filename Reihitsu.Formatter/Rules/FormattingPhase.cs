namespace Reihitsu.Formatter.Rules;

/// <summary>
/// Phases of the formatting pipeline, executed in order.
/// </summary>
internal enum FormattingPhase
{
    /// <summary>
    /// Structural transforms (e.g., expression body → block body).
    /// </summary>
    StructuralTransform = 0,

    /// <summary>
    /// Indentation normalization and continuation-line alignment.
    /// </summary>
    Indentation = 1,

    /// <summary>
    /// Blank line insertion and removal.
    /// </summary>
    BlankLineManagement = 2,

    /// <summary>
    /// Horizontal spacing normalization.
    /// </summary>
    Spacing = 3,

    /// <summary>
    /// Region formatting.
    /// </summary>
    RegionFormatting = 4,

    /// <summary>
    /// Final cleanup (trailing whitespace, final newline).
    /// </summary>
    Cleanup = 5
}