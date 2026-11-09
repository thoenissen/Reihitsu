using Reihitsu.Formatter;

namespace Reihitsu.Playground;

/// <summary>
/// Represents the outcome of formatting one playground input
/// </summary>
/// <param name="Text">The formatted text, or the original input when it could not be formatted</param>
/// <param name="Formatted">Whether the formatter was able to format the input</param>
/// <param name="Unsupported">The shape the formatter refused the input for as unsupported, or <see langword="null"/></param>
public readonly record struct PlaygroundFormatResult(string Text, bool Formatted, UnsupportedSyntax Unsupported = null);