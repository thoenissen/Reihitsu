using System.Collections.Generic;
using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Core;

namespace Reihitsu.Formatter.Data;

/// <summary>
/// The facts about the token that precedes another token, which the blank-line decisions, the documentation phase, and the
/// comment alignment of the indentation phase read: the token's kind, whether it opens content, whether it is the colon of
/// a switch label, and the kind and text of each of its trailing trivia. A node-level formatting root has no parent (FMT-PIPE-023), so its first
/// token reaches no preceding token through <see cref="SyntaxToken.GetPreviousToken"/>, and its facts say that no token
/// precedes it: the node-level entry points write back the root's leading trivia as it stands in the document, so no fact
/// about the code in front of the root is captured (FMT-PIPE-018, FMT-PIPE-024)
/// </summary>
internal readonly struct PrecedingTokenFacts
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="kind">The kind of the preceding token</param>
    /// <param name="opensContent">Whether the preceding token opens content (<see cref="SyntaxTriviaUtilities.IsContentOpeningToken"/>)</param>
    /// <param name="isSwitchLabelColon">Whether the preceding token is the colon of a switch label</param>
    /// <param name="trailingTrivia">The kind and text of each trailing trivia of the preceding token, in order</param>
    /// <param name="endsLine">Whether a trailing trivia of the preceding token ends its line</param>
    /// <param name="hasTrailingComment">Whether a trailing trivia of the preceding token is a comment</param>
    private PrecedingTokenFacts(SyntaxKind kind, bool opensContent, bool isSwitchLabelColon, ImmutableArray<TriviaFact> trailingTrivia, bool endsLine, bool hasTrailingComment)
    {
        Kind = kind;
        OpensContent = opensContent;
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
    /// Whether the preceding token opens content, so that a comment behind it is the first content behind an opening token
    /// (<see cref="SyntaxTriviaUtilities.IsContentOpeningToken"/>); the parent kind that decides it for a <c>&lt;</c> is not
    /// part of the other facts
    /// </summary>
    public bool OpensContent { get; }

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
                                       SyntaxTriviaUtilities.IsContentOpeningToken(previousToken),
                                       previousToken.IsKind(SyntaxKind.ColonToken) && previousToken.Parent is SwitchLabelSyntax,
                                       trailingTriviaFacts,
                                       trailingTrivia.Any(SyntaxTriviaUtilities.EndsLine),
                                       trailingTrivia.Any(SyntaxTriviaUtilities.IsCommentTrivia));
    }

    #endregion // Methods
}