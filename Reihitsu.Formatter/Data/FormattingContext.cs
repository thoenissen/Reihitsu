using Reihitsu.Core;

namespace Reihitsu.Formatter.Data;

/// <summary>
/// Carries state through the formatting pipeline.
/// Immutable — a single shared instance is passed to every phase
/// </summary>
internal record FormattingContext
{
    #region Constants

    /// <summary>
    /// Indentation unit: 4 spaces (non-configurable)
    /// </summary>
    public const int IndentSize = SyntaxIndentationUtilities.IndentSize;

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="endOfLine">End-of-line sequence</param>
    /// <param name="baseIndentLevel">The base indentation level for isolated node formatting</param>
    /// <param name="preserveRootDocumentationBoundary">Whether node-scoped formatting should preserve one line break before root documentation</param>
    public FormattingContext(string endOfLine, int baseIndentLevel = 0, bool preserveRootDocumentationBoundary = false)
    {
        EndOfLine = endOfLine;
        BaseIndentLevel = baseIndentLevel;
        PreserveRootDocumentationBoundary = preserveRootDocumentationBoundary;
    }

    #endregion // Constructor

    #region Properties

    /// <summary>
    /// End-of-line sequence
    /// </summary>
    public string EndOfLine { get; }

    /// <summary>
    /// The base indentation level for isolated node formatting.
    /// When formatting a node that is detached from its original tree
    /// (e.g., after a structural transform), this offset accounts for the
    /// parent context that is no longer reachable via the syntax tree
    /// </summary>
    public int BaseIndentLevel { get; }

    /// <summary>
    /// Whether node-scoped formatting should preserve one serialized line break before documentation at the formatting-root boundary
    /// </summary>
    public bool PreserveRootDocumentationBoundary { get; }

    #endregion // Properties
}