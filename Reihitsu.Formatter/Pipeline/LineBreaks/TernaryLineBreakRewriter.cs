using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Formatter.Pipeline.LineBreaks;

/// <summary>
/// Applies line-break rules for ternary conditional operator placement
/// </summary>
internal sealed class TernaryLineBreakRewriter : CSharpSyntaxRewriter
{
    #region Fields

    /// <summary>
    /// The formatting context
    /// </summary>
    private readonly FormattingContext _context;

    /// <summary>
    /// The cancellation token
    /// </summary>
    private readonly CancellationToken _cancellationToken;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="context">The formatting context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public TernaryLineBreakRewriter(FormattingContext context,
                                    CancellationToken cancellationToken)
    {
        _context = context;
        _cancellationToken = cancellationToken;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Recursively collects all <c>?</c> and <c>:</c> tokens from a nested ternary expression tree
    /// </summary>
    /// <param name="node">The conditional expression to collect tokens from</param>
    /// <param name="tokens">The list to populate with operator tokens</param>
    private static void CollectTernaryOperatorTokens(ConditionalExpressionSyntax node,
                                                     List<SyntaxToken> tokens)
    {
        tokens.Add(node.QuestionToken);
        tokens.Add(node.ColonToken);

        if (node.WhenTrue is ConditionalExpressionSyntax nestedTrue)
        {
            CollectTernaryOperatorTokens(nestedTrue, tokens);
        }

        if (node.WhenFalse is ConditionalExpressionSyntax nestedFalse)
        {
            CollectTernaryOperatorTokens(nestedFalse, tokens);
        }
    }

    /// <summary>
    /// Normalizes ternary operator placement
    /// </summary>
    /// <param name="node">The conditional expression node</param>
    /// <returns>The conditional expression with ternary operators on new lines</returns>
    private ConditionalExpressionSyntax NormalizeTernaryOperatorPosition(ConditionalExpressionSyntax node)
    {
        if (LineBreakDetection.IsMultiLine(node) == false)
        {
            if (node.WhenFalse is ConditionalExpressionSyntax == false
                && node.WhenTrue is ConditionalExpressionSyntax == false)
            {
                return node;
            }

            var operatorTokens = new List<SyntaxToken>();

            CollectTernaryOperatorTokens(node, operatorTokens);

            node = node.ReplaceTokens(operatorTokens, (original, _) => LineBreakTriviaUtilities.PrependEndOfLine(original, _context.EndOfLine));
        }

        node = NormalizeQuestionTokenPosition(node);

        return NormalizeColonTokenPosition(node);
    }

    /// <summary>
    /// Normalizes placement of the <c>?</c> token in a ternary expression
    /// </summary>
    /// <param name="node">The conditional expression node</param>
    /// <returns>The updated conditional expression</returns>
    private ConditionalExpressionSyntax NormalizeQuestionTokenPosition(ConditionalExpressionSyntax node)
    {
        var questionToken = node.QuestionToken;

        if (LineBreakTriviaUtilities.HasTrailingEndOfLine(questionToken))
        {
            return MoveQuestionTokenToNextLine(node, questionToken);
        }

        if (LineBreakTriviaUtilities.HasLeadingEndOfLine(questionToken))
        {
            return node;
        }

        return node.WithQuestionToken(LineBreakTriviaUtilities.PrependEndOfLine(questionToken, _context.EndOfLine));
    }

    /// <summary>
    /// Moves the ternary <c>?</c> token from line-end position to line-start position
    /// </summary>
    /// <param name="node">The conditional expression node</param>
    /// <param name="questionToken">The question mark token</param>
    /// <returns>The updated conditional expression</returns>
    private ConditionalExpressionSyntax MoveQuestionTokenToNextLine(ConditionalExpressionSyntax node,
                                                                    SyntaxToken questionToken)
    {
        var conditionLastToken = node.Condition.GetLastToken();

        if (LineBreakTriviaUtilities.WouldJoinIntoComment(questionToken, node.WhenTrue.GetFirstToken()))
        {
            return node;
        }

        var newQuestionTrailing = LineBreakTriviaUtilities.RemoveTrailingEndOfLineTrivia(questionToken.TrailingTrivia);
        var newConditionTrailing = LineBreakTriviaUtilities.AppendEndOfLine(conditionLastToken.TrailingTrivia, _context.EndOfLine);
        var newConditionLastToken = conditionLastToken.WithTrailingTrivia(newConditionTrailing);
        var newQuestionToken = questionToken.WithTrailingTrivia(newQuestionTrailing);
        var whenTrueFirstToken = node.WhenTrue.GetFirstToken();
        var newWhenTrueFirstToken = LineBreakTriviaUtilities.RemoveLeadingEndOfLineAndWhitespace(whenTrueFirstToken);

        if (newWhenTrueFirstToken.LeadingTrivia.Any(SyntaxKind.WhitespaceTrivia) == false)
        {
            newWhenTrueFirstToken = newWhenTrueFirstToken.WithLeadingTrivia(newWhenTrueFirstToken.LeadingTrivia.Add(SyntaxFactory.Space));
        }

        return node.ReplaceTokens([conditionLastToken, questionToken, whenTrueFirstToken],
                                  (original, _) =>
                                  {
                                      if (original == conditionLastToken)
                                      {
                                          return newConditionLastToken;
                                      }

                                      if (original == questionToken)
                                      {
                                          return newQuestionToken;
                                      }

                                      return newWhenTrueFirstToken;
                                  });
    }

    /// <summary>
    /// Normalizes placement of the <c>:</c> token in a ternary expression
    /// </summary>
    /// <param name="node">The conditional expression node</param>
    /// <returns>The updated conditional expression</returns>
    private ConditionalExpressionSyntax NormalizeColonTokenPosition(ConditionalExpressionSyntax node)
    {
        var colonToken = node.ColonToken;

        if (LineBreakTriviaUtilities.HasTrailingEndOfLine(colonToken))
        {
            if (LineBreakTriviaUtilities.WouldJoinIntoComment(colonToken, node.WhenFalse.GetFirstToken()))
            {
                return node;
            }

            var whenTrueLastToken = node.WhenTrue.GetLastToken();
            var newColonTrailing = LineBreakTriviaUtilities.RemoveTrailingEndOfLineTrivia(colonToken.TrailingTrivia);
            var newWhenTrueTrailing = LineBreakTriviaUtilities.AppendEndOfLine(whenTrueLastToken.TrailingTrivia, _context.EndOfLine);
            var newWhenTrueLastToken = whenTrueLastToken.WithTrailingTrivia(newWhenTrueTrailing);
            var newColonToken = colonToken.WithTrailingTrivia(newColonTrailing);

            return node.ReplaceTokens([whenTrueLastToken, colonToken],
                                      (original, _) =>
                                      {
                                          if (original == whenTrueLastToken)
                                          {
                                              return newWhenTrueLastToken;
                                          }

                                          return newColonToken;
                                      });
        }

        if (LineBreakTriviaUtilities.HasLeadingEndOfLine(colonToken))
        {
            return node;
        }

        return node.WithColonToken(LineBreakTriviaUtilities.PrependEndOfLine(colonToken, _context.EndOfLine));
    }

    #endregion // Methods

    #region CSharpSyntaxVisitor

    /// <inheritdoc/>
    public override SyntaxNode VisitConditionalExpression(ConditionalExpressionSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (ConditionalExpressionSyntax)base.VisitConditionalExpression(node);

        if (node == null)
        {
            return null;
        }

        return NormalizeTernaryOperatorPosition(node);
    }

    #endregion // CSharpSyntaxVisitor
}