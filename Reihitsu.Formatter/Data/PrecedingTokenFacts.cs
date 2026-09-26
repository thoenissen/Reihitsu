using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Formatter.Utilities;

namespace Reihitsu.Formatter.Data;

/// <summary>
/// The position-free facts about the token that precedes another token, which the blank-line decisions read.
/// A phase that replaces the formatting root detaches it, so the root's first token no longer reaches its preceding
/// token through <see cref="SyntaxToken.GetPreviousToken"/>. These facts are captured before the pipeline runs and carry
/// no position, so they stay valid across trees, whereas line or span arithmetic against the original token would not
/// </summary>
internal readonly struct PrecedingTokenFacts
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="kind">The kind of the preceding token</param>
    /// <param name="isSwitchLabelColon">Whether the preceding token is the colon of a switch label</param>
    /// <param name="trailingTrivia">The trailing trivia of the preceding token</param>
    private PrecedingTokenFacts(SyntaxKind kind, bool isSwitchLabelColon, SyntaxTriviaList trailingTrivia)
    {
        Kind = kind;
        IsSwitchLabelColon = isSwitchLabelColon;
        TrailingTrivia = trailingTrivia;
    }

    #endregion // Constructor

    #region Properties

    /// <summary>
    /// The kind of the preceding token, or <see cref="SyntaxKind.None"/> when there is none
    /// </summary>
    public SyntaxKind Kind { get; }

    /// <summary>
    /// Whether a preceding token exists
    /// </summary>
    public bool Exists => Kind != SyntaxKind.None;

    /// <summary>
    /// Whether the preceding token is the colon of a switch label
    /// </summary>
    public bool IsSwitchLabelColon { get; }

    /// <summary>
    /// The trailing trivia of the preceding token. Only its kinds and text may be read, never its positions
    /// </summary>
    public SyntaxTriviaList TrailingTrivia { get; }

    /// <summary>
    /// Whether the trailing trivia of the preceding token contains a line break, which means the preceding token ends its line
    /// </summary>
    public bool EndsLine => TrailingTrivia.Any(SyntaxKind.EndOfLineTrivia);

    /// <summary>
    /// Whether the trailing trivia of the preceding token contains a comment
    /// </summary>
    public bool HasTrailingComment => TrailingTrivia.Any(ReihitsuFormatterHelpers.IsCommentTrivia);

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Captures the facts about the specified preceding token
    /// </summary>
    /// <param name="previousToken">The preceding token, or <see langword="default"/> when there is none</param>
    /// <returns>The facts about the preceding token</returns>
    public static PrecedingTokenFacts From(SyntaxToken previousToken)
    {
        if (previousToken.IsKind(SyntaxKind.None))
        {
            return default;
        }

        return new PrecedingTokenFacts(previousToken.Kind(),
                                       previousToken.IsKind(SyntaxKind.ColonToken) && previousToken.Parent is SwitchLabelSyntax,
                                       previousToken.TrailingTrivia);
    }

    /// <summary>
    /// Resolves the facts about the token that precedes a visited token. Only the first token of the visited root has no
    /// previous token in its tree; for that token the facts come from <see cref="FormattingContext.RootPrecedingToken"/>,
    /// which holds the root's preceding token in its document when the caller supplied one
    /// </summary>
    /// <param name="previousToken">The result of <see cref="SyntaxToken.GetPreviousToken"/> on the visited token</param>
    /// <param name="context">The formatting context</param>
    /// <returns>The facts about the preceding token</returns>
    public static PrecedingTokenFacts Resolve(SyntaxToken previousToken, FormattingContext context)
    {
        return previousToken.IsKind(SyntaxKind.None)
                   ? context.RootPrecedingToken
                   : From(previousToken);
    }

    #endregion // Methods
}