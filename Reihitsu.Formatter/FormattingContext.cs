using Microsoft.CodeAnalysis;

namespace Reihitsu.Formatter;

/// <summary>
/// Carries state through the formatting pipeline.
/// Immutable — each phase receives a fresh context snapshot.
/// </summary>
internal sealed class FormattingContext
{
    #region Constants

    /// <summary>
    /// Indentation unit: 4 spaces (non-configurable).
    /// </summary>
    public const int IndentSize = 4;

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="endOfLine">End-of-line sequence.</param>
    /// <param name="document">The original document (maybe null when formatting a standalone SyntaxTree).</param>
    /// <param name="baseIndentLevel">The base indentation level for isolated node formatting.</param>
    public FormattingContext(string endOfLine, Document document = null, int baseIndentLevel = 0)
    {
        EndOfLine = endOfLine;
        Document = document;
        BaseIndentLevel = baseIndentLevel;
    }

    #endregion // Constructor

    #region Properties

    /// <summary>
    /// End-of-line sequence.
    /// </summary>
    public string EndOfLine { get; }

    /// <summary>
    /// The original document (maybe null when formatting a standalone SyntaxTree).
    /// </summary>
    public Document Document { get; }

    /// <summary>
    /// The base indentation level for isolated node formatting.
    /// When formatting a node that is detached from its original tree
    /// (e.g., after a structural transform), this offset accounts for the
    /// parent context that is no longer reachable via the syntax tree.
    /// </summary>
    public int BaseIndentLevel { get; }

    #endregion // Properties
}