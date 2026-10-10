namespace Reihitsu.Cli.Diff;

/// <summary>
/// Represents a single line of diff input, carrying its terminal-newline state out of band from its text so the
/// state can participate in line-equality comparisons without being forgeable by authored content
/// </summary>
/// <param name="Text">The line's authored text, without any encoded termination state</param>
/// <param name="IsTerminated">Whether the line is followed by a line break in its source content</param>
internal readonly record struct DiffLine(string Text, bool IsTerminated);