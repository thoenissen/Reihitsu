namespace Reihitsu.Cli.Diff;

/// <summary>
/// Represents the kind of an edit operation in a diff.
/// </summary>
internal enum EditKind
{
    /// <summary>
    /// The line is unchanged.
    /// </summary>
    Equal,

    /// <summary>
    /// The line was deleted from the original.
    /// </summary>
    Delete,

    /// <summary>
    /// The line was inserted in the formatted version.
    /// </summary>
    Insert
}