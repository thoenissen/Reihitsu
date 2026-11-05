using System.Collections.Generic;
using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Core;

namespace Reihitsu.Formatter.Data;

/// <summary>
/// The facts about the token that precedes another token, which the blank-line decisions, the documentation phase, and the
/// comment alignment of the indentation phase read. They are plain values (FMT-PIPE-024): the token's kind, whether it is
/// the colon of a switch label, and the kind and text of each of its trailing trivia. A node-level formatting root has no
/// parent (FMT-PIPE-023), so its first token reaches no preceding token through <see cref="SyntaxToken.GetPreviousToken"/>;
/// for that token the facts come from <see cref="FormattingContext.RootPrecedingToken"/>, which the entry point captures
/// from the document before the pipeline runs
/// </summary>
internal readonly struct PrecedingTokenFacts
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="kind">The kind of the preceding token</param>
    /// <param name="isSwitchLabelColon">Whether the preceding token is the colon of a switch label</param>
    /// <param name="trailingTrivia">The kind and text of each trailing trivia of the preceding token, in order</param>
    /// <param name="endsLine">Whether a trailing trivia of the preceding token ends its line</param>
    /// <param name="hasTrailingComment">Whether a trailing trivia of the preceding token is a comment</param>
    private PrecedingTokenFacts(SyntaxKind kind, bool isSwitchLabelColon, ImmutableArray<TriviaFact> trailingTrivia, bool endsLine, bool hasTrailingComment)
    {
        Kind = kind;
        IsSwitchLabelColon = isSwitchLabelColon;
        TrailingTriviaFacts = trailingTrivia;
        EndsLine = endsLine;
        HasTrailingComment = hasTrailingComment;
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
    /// The kind and text of each trailing trivia of the preceding token, in order
    /// </summary>
    public ImmutableArray<TriviaFact> TrailingTriviaFacts { get; }

    /// <summary>
    /// The trailing trivia of the preceding token, created from <see cref="TrailingTriviaFacts"/> as trivia of no tree, for
    /// the gap analyses that take a trivia list
    /// </summary>
    public SyntaxTriviaList TrailingTrivia
    {
        get
        {
            if (TrailingTriviaFacts.IsDefaultOrEmpty)
            {
                return SyntaxFactory.TriviaList();
            }

            var trivia = new List<SyntaxTrivia>(TrailingTriviaFacts.Length);

            foreach (var fact in TrailingTriviaFacts)
            {
                trivia.AddRange(fact.ToTrivia());
            }

            return SyntaxFactory.TriviaList(trivia);
        }
    }

    /// <summary>
    /// Whether the trailing trivia of the preceding token contains a trivia that ends its line, which means the preceding
    /// token ends its line (<see cref="SyntaxTriviaUtilities.EndsLine"/>)
    /// </summary>
    public bool EndsLine { get; }

    /// <summary>
    /// Whether the trailing trivia of the preceding token contains a comment
    /// </summary>
    public bool HasTrailingComment { get; }

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

        var trailingTrivia = previousToken.TrailingTrivia;
        var trailingTriviaFacts = trailingTrivia.Count == 0
                                      ? ImmutableArray<TriviaFact>.Empty
                                      : trailingTrivia.Select(TriviaFact.From).ToImmutableArray();

        return new PrecedingTokenFacts(previousToken.Kind(),
                                       previousToken.IsKind(SyntaxKind.ColonToken) && previousToken.Parent is SwitchLabelSyntax,
                                       trailingTriviaFacts,
                                       trailingTrivia.Any(SyntaxTriviaUtilities.EndsLine),
                                       trailingTrivia.Any(SyntaxTriviaUtilities.IsCommentTrivia));
    }

    /// <summary>
    /// Resolves the facts about the token that precedes a visited token. Only the first token of the visited root has no
    /// previous token in its tree; for that token the facts come from <see cref="FormattingContext.RootPrecedingToken"/>,
    /// which holds the facts about the root's preceding token in its document when the caller supplied them
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