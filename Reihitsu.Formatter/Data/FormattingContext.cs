using Microsoft.CodeAnalysis.CSharp;

using Reihitsu.Core;
using Reihitsu.Formatter.Pipeline.StructuralTransforms.Enumerations;
using Reihitsu.Formatter.Utilities;

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
    /// <param name="disabledStructuralTransforms">The configurable structural transforms this run must skip</param>
    /// <param name="languageVersion">
    /// The C# language version the source targets. It is resolved through <see cref="LanguageVersionResolver.Resolve(Microsoft.CodeAnalysis.CSharp.LanguageVersion)"/>,
    /// so <see cref="LanguageVersion.Default"/> stands for <see cref="LanguageVersionResolver.MaxSupportedLanguageVersion"/>
    /// </param>
    /// <param name="rootPrecedingTokenKind">
    /// The kind of the token that precedes the formatting root in its document, or <see cref="SyntaxKind.None"/> when the root has no preceding token
    /// </param>
    /// <param name="rootPrecedingTokenEndsLine">Whether the trailing trivia of the token that precedes the formatting root contains a line break</param>
    public FormattingContext(string endOfLine,
                             int baseIndentLevel = 0,
                             bool preserveRootDocumentationBoundary = false,
                             ConfigurableStructuralTransforms disabledStructuralTransforms = ConfigurableStructuralTransforms.None,
                             LanguageVersion languageVersion = LanguageVersion.Default,
                             SyntaxKind rootPrecedingTokenKind = SyntaxKind.None,
                             bool rootPrecedingTokenEndsLine = false)
    {
        EndOfLine = endOfLine;
        BaseIndentLevel = baseIndentLevel;
        PreserveRootDocumentationBoundary = preserveRootDocumentationBoundary;
        DisabledStructuralTransforms = disabledStructuralTransforms;
        LanguageVersion = LanguageVersionResolver.Resolve(languageVersion);
        RootPrecedingTokenKind = rootPrecedingTokenKind;
        RootPrecedingTokenEndsLine = rootPrecedingTokenEndsLine;
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

    /// <summary>
    /// The configurable structural transforms this run must skip. Every transform runs unless it is listed here
    /// </summary>
    public ConfigurableStructuralTransforms DisabledStructuralTransforms { get; }

    /// <summary>
    /// The concrete C# language version the source targets, never newer than <see cref="LanguageVersionResolver.MaxSupportedLanguageVersion"/>
    /// and never a symbolic value such as <see cref="LanguageVersion.Latest"/> or <see cref="LanguageVersion.Preview"/>.
    /// Version-dependent rules read it instead of a node's parse options, which an earlier rewrite can reset to the defaults
    /// </summary>
    public LanguageVersion LanguageVersion { get; }

    /// <summary>
    /// The kind of the token that precedes the formatting root in its document, or <see cref="SyntaxKind.None"/> when the root
    /// starts its document or has no document. A phase that replaces the root detaches it, so the root's first token no longer
    /// reaches this token through <see cref="Microsoft.CodeAnalysis.SyntaxToken.GetPreviousToken"/>; it is captured before the
    /// pipeline runs so that detachment does not turn the root into the start of a file
    /// </summary>
    public SyntaxKind RootPrecedingTokenKind { get; }

    /// <summary>
    /// Whether the trailing trivia of the token that precedes the formatting root contains a line break.
    /// Only meaningful when <see cref="RootPrecedingTokenKind"/> is not <see cref="SyntaxKind.None"/>
    /// </summary>
    public bool RootPrecedingTokenEndsLine { get; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Determines whether the given configurable structural transform runs in this formatting run
    /// </summary>
    /// <param name="transform">The transform to check</param>
    /// <returns><see langword="true"/> if the transform runs; otherwise, <see langword="false"/></returns>
    public bool IsStructuralTransformEnabled(ConfigurableStructuralTransforms transform)
    {
        return (DisabledStructuralTransforms & transform) == ConfigurableStructuralTransforms.None;
    }

    #endregion // Methods
}