namespace Reihitsu.Cli.Diff;

/// <summary>
/// Represents a single edit operation in a diff.
/// </summary>
/// <param name="Kind">The kind of edit operation.</param>
/// <param name="OriginalIndex">The index in the original lines array, or -1 if not applicable.</param>
/// <param name="FormattedIndex">The index in the formatted lines array, or -1 if not applicable.</param>
internal readonly record struct EditOperation(EditKind Kind, int OriginalIndex, int FormattedIndex);