using System.Collections.Generic;

using Microsoft.CodeAnalysis;

using Reihitsu.Formatter;

namespace Reihitsu.Playground;

/// <summary>
/// Represents the outcome of formatting one playground input
/// </summary>
/// <param name="Text">The formatted text, or the original input when it could not be formatted</param>
/// <param name="Formatted">Whether the formatter was able to format the input</param>
/// <param name="Unsupported">The shape the formatter refused the input for as unsupported, or <see langword="null"/></param>
/// <param name="HoldsHiddenCode">Whether the input was left unchanged because it holds code that another build configuration hides (PLAY-009)</param>
/// <param name="Errors">The parser's errors for the input, ordered by the start of their span and, at one start, in the order the parser reports them; empty when the input has none (PLAY-006)</param>
public readonly record struct PlaygroundFormatResult(string Text, bool Formatted, UnsupportedSyntax Unsupported = null, bool HoldsHiddenCode = false, IReadOnlyList<Diagnostic> Errors = null);