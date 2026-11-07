using Microsoft.CodeAnalysis;

namespace Reihitsu.Formatter.Pipeline.Indentation.Utilities;

/// <summary>
/// Describes a multi-line raw string literal whose lines the indentation phase moves
/// </summary>
/// <param name="Node">The literal expression or interpolated string expression</param>
/// <param name="OpeningLine">The 0-based line on which the literal starts</param>
/// <param name="ClosingLine">The 0-based line on which the literal's closing marker stands</param>
/// <param name="ClosingColumn">The width of the whitespace in front of the closing marker, each whitespace character counting as one column</param>
/// <param name="QuoteOffset">The offset of the first opening quote from the literal's first character</param>
internal readonly record struct RawStringLiteral(SyntaxNode Node, int OpeningLine, int ClosingLine, int ClosingColumn, int QuoteOffset)
{
    #region Properties

    /// <summary>
    /// The literal expression or interpolated string expression
    /// </summary>
    public SyntaxNode Node { get; } = Node;

    /// <summary>
    /// The 0-based line on which the literal starts
    /// </summary>
    public int OpeningLine { get; } = OpeningLine;

    /// <summary>
    /// The 0-based line on which the literal's closing marker stands
    /// </summary>
    public int ClosingLine { get; } = ClosingLine;

    /// <summary>
    /// The width of the whitespace in front of the closing marker, each whitespace character counting as one column
    /// </summary>
    public int ClosingColumn { get; } = ClosingColumn;

    /// <summary>
    /// The offset of the first opening quote from the literal's first character
    /// </summary>
    public int QuoteOffset { get; } = QuoteOffset;

    #endregion // Properties
}