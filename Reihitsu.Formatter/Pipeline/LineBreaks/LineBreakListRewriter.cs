using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Formatter.Pipeline.LineBreaks;

/// <summary>
/// Applies line-break rules for argument and parameter lists
/// </summary>
internal sealed class LineBreakListRewriter : CSharpSyntaxRewriter
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
    public LineBreakListRewriter(FormattingContext context,
                                 CancellationToken cancellationToken)
    {
        _context = context;
        _cancellationToken = cancellationToken;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Collapses the first element of a list to the same line as the opening delimiter
    /// when it currently starts on a new line
    /// </summary>
    /// <typeparam name="TNode">The list syntax node type</typeparam>
    /// <param name="node">The list node</param>
    /// <param name="firstElementToken">The first token of the first element, or <see langword="default"/> when the list is empty</param>
    /// <returns>The list with the first element collapsed</returns>
    private static TNode CollapseFirstElementToSameLine<TNode>(TNode node,
                                                               SyntaxToken firstElementToken)
        where TNode : SyntaxNode
    {
        if (firstElementToken == default
            || firstElementToken.IsKind(SyntaxKind.None)
            || LineBreakTriviaUtilities.HasLeadingEndOfLine(firstElementToken) == false)
        {
            return node;
        }

        return LineBreakTriviaUtilities.CollapseTokenToSameLine(node, firstElementToken);
    }

    /// <summary>
    /// Collapses the opening parenthesis of a parameter list onto the declaration line
    /// </summary>
    /// <param name="node">The parameter list node</param>
    /// <returns>The updated parameter list</returns>
    private static ParameterListSyntax CollapseOpenParenToDeclarationLine(ParameterListSyntax node)
    {
        if (TokenLocator.TryGetPreviousToken(node, node.OpenParenToken, out var previousToken) == false
            || TokenGapUtilities.HasLineBreakBetween(previousToken, node.OpenParenToken) == false)
        {
            return node;
        }

        if (LineBreakTriviaUtilities.WouldJoinAcrossUnjoinableTrivia(previousToken, node.OpenParenToken))
        {
            return node;
        }

        var newPreviousToken = previousToken.WithTrailingTrivia(LineBreakTriviaUtilities.RemoveTrailingWhitespace(LineBreakTriviaUtilities.RemoveTrailingEndOfLineTrivia(previousToken.TrailingTrivia)));
        var newOpenParen = LineBreakTriviaUtilities.RemoveLeadingEndOfLineAndWhitespace(node.OpenParenToken);

        if (TokenLocator.ContainsToken(node, previousToken) == false)
        {
            return node.WithOpenParenToken(newOpenParen);
        }

        return node.ReplaceTokens([previousToken, node.OpenParenToken],
                                  (original, _) => original == previousToken
                                                       ? newPreviousToken
                                                       : newOpenParen);
    }

    /// <summary>
    /// Collapses misplaced commas onto the previous parameter line
    /// </summary>
    /// <param name="node">The parameter list node</param>
    /// <param name="endOfLine">The end-of-line sequence to keep after a moved separator</param>
    /// <returns>The updated parameter list</returns>
    private static ParameterListSyntax CollapseSeparatorsToPreviousParameterLine(ParameterListSyntax node,
                                                                                 string endOfLine)
    {
        for (var separatorIndex = 0; separatorIndex < node.Parameters.SeparatorCount; separatorIndex++)
        {
            var previousToken = node.Parameters[separatorIndex].GetLastToken();
            var separator = node.Parameters.GetSeparator(separatorIndex);
            var nextParameter = node.Parameters[separatorIndex + 1].GetFirstToken();

            if (TokenGapUtilities.HasLineBreakBetween(previousToken, separator) == false)
            {
                continue;
            }

            if (LineBreakTriviaUtilities.WouldJoinAcrossUnjoinableTrivia(previousToken, separator))
            {
                continue;
            }

            var newPreviousToken = previousToken.WithTrailingTrivia(LineBreakTriviaUtilities.RemoveTrailingWhitespace(LineBreakTriviaUtilities.RemoveTrailingEndOfLineTrivia(previousToken.TrailingTrivia)));
            var newSeparator = LineBreakTriviaUtilities.RemoveLeadingEndOfLineAndWhitespace(separator);

            if (LineBreakTriviaUtilities.HasTrailingEndOfLine(newSeparator) == false && LineBreakTriviaUtilities.HasLeadingEndOfLine(nextParameter) == false)
            {
                var newTrailing = newSeparator.TrailingTrivia
                                              .Where(trivia => trivia.IsKind(SyntaxKind.WhitespaceTrivia) == false)
                                              .ToList();

                newTrailing.Add(SyntaxFactory.EndOfLine(endOfLine));
                newSeparator = newSeparator.WithTrailingTrivia(SyntaxFactory.TriviaList(newTrailing));
            }

            node = node.ReplaceTokens([previousToken, separator],
                                      (original, _) => original == previousToken
                                                           ? newPreviousToken
                                                           : newSeparator);
        }

        return node;
    }

    /// <summary>
    /// Collapses the closing parenthesis onto the final parameter line
    /// </summary>
    /// <param name="node">The parameter list node</param>
    /// <returns>The updated parameter list</returns>
    private static ParameterListSyntax CollapseCloseParenToParameterLine(ParameterListSyntax node)
    {
        if (TokenLocator.TryGetPreviousToken(node, node.CloseParenToken, out var previousToken) == false
            || TokenGapUtilities.HasLineBreakBetween(previousToken, node.CloseParenToken) == false)
        {
            return node;
        }

        if (LineBreakTriviaUtilities.WouldJoinAcrossUnjoinableTrivia(previousToken, node.CloseParenToken))
        {
            return node;
        }

        var newPreviousToken = previousToken.WithTrailingTrivia(LineBreakTriviaUtilities.RemoveTrailingWhitespace(LineBreakTriviaUtilities.RemoveTrailingEndOfLineTrivia(previousToken.TrailingTrivia)));
        var newCloseParen = LineBreakTriviaUtilities.RemoveLeadingEndOfLineAndWhitespace(node.CloseParenToken);

        return node.ReplaceTokens([previousToken, node.CloseParenToken],
                                  (original, _) => original == previousToken
                                                       ? newPreviousToken
                                                       : newCloseParen);
    }

    /// <summary>
    /// Ensures that all arguments in a multi-line argument list start on their own line
    /// </summary>
    /// <param name="node">The argument list node</param>
    /// <param name="endOfLine">The end-of-line sequence to insert when splitting arguments</param>
    /// <returns>The argument list with arguments on separate lines</returns>
    private static ArgumentListSyntax EnsureArgumentsOnSeparateLines(ArgumentListSyntax node,
                                                                     string endOfLine)
    {
        if (node.Arguments.Count <= 1)
        {
            return node;
        }

        return EnsureSeparatorsHaveEndOfLine(node, node.Arguments, endOfLine);
    }

    /// <summary>
    /// Determines whether a bracketed argument list can be safely collapsed to one line
    /// </summary>
    /// <param name="node">The bracketed argument list node</param>
    /// <returns><see langword="true"/> if collapsing is safe; otherwise, <see langword="false"/></returns>
    private static bool CanSafelyCollapseBracketedArguments(BracketedArgumentListSyntax node)
    {
        if (node.Parent is not ElementAccessExpressionSyntax and not ImplicitElementAccessSyntax)
        {
            return false;
        }

        if (node.DescendantTrivia(descendIntoTrivia: true)
                .Any(trivia => trivia.IsDirective || trivia.IsKind(SyntaxKind.SingleLineCommentTrivia) || trivia.IsKind(SyntaxKind.MultiLineCommentTrivia)))
        {
            return false;
        }

        return node.Arguments.Any(LineBreakDetection.IsMultiLine) == false;
    }

    /// <summary>
    /// Collapses bracketed indexer arguments onto a single line when safe
    /// </summary>
    /// <param name="node">The bracketed argument list node</param>
    /// <returns>The updated bracketed argument list</returns>
    private static BracketedArgumentListSyntax CollapseBracketedArgumentsToSingleLine(BracketedArgumentListSyntax node)
    {
        if (LineBreakDetection.IsMultiLine(node) == false || CanSafelyCollapseBracketedArguments(node) == false)
        {
            return node;
        }

        if (node.Arguments.Count > 0)
        {
            node = CollapseFirstElementToSameLine(node, node.Arguments[0].GetFirstToken());
        }

        for (var argumentIndex = 1; argumentIndex < node.Arguments.Count; argumentIndex++)
        {
            var firstToken = node.Arguments[argumentIndex].GetFirstToken();

            if (LineBreakTriviaUtilities.HasLeadingEndOfLine(firstToken))
            {
                node = LineBreakTriviaUtilities.CollapseTokenToSameLine(node, firstToken);
            }
        }

        for (var separatorIndex = 0; separatorIndex < node.Arguments.SeparatorCount; separatorIndex++)
        {
            var separator = node.Arguments.GetSeparator(separatorIndex);

            if (LineBreakTriviaUtilities.HasLeadingEndOfLine(separator) || LineBreakTriviaUtilities.HasTrailingEndOfLine(separator))
            {
                node = LineBreakTriviaUtilities.CollapseTokenToSameLine(node, separator);
            }
        }

        if (LineBreakTriviaUtilities.HasLeadingEndOfLine(node.CloseBracketToken))
        {
            node = LineBreakTriviaUtilities.CollapseTokenToSameLine(node, node.CloseBracketToken);
        }

        return node;
    }

    /// <summary>
    /// Ensures that all arguments in a multi-line attribute argument list start on their own line
    /// </summary>
    /// <param name="node">The attribute argument list node</param>
    /// <param name="endOfLine">The end-of-line sequence to insert when splitting arguments</param>
    /// <returns>The attribute argument list with arguments on separate lines</returns>
    private static AttributeArgumentListSyntax EnsureAttributeArgumentsOnSeparateLines(AttributeArgumentListSyntax node,
                                                                                       string endOfLine)
    {
        if (node.Arguments.Count <= 1)
        {
            return node;
        }

        return EnsureSeparatorsHaveEndOfLine(node, node.Arguments, endOfLine);
    }

    /// <summary>
    /// Ensures that all parameters in a multi-line parameter list start on their own line
    /// </summary>
    /// <param name="node">The parameter list node</param>
    /// <param name="endOfLine">The end-of-line sequence to insert when splitting parameters</param>
    /// <returns>The parameter list with parameters on separate lines</returns>
    private static ParameterListSyntax EnsureParametersOnSeparateLines(ParameterListSyntax node,
                                                                       string endOfLine)
    {
        if (node.Parameters.Count <= 1)
        {
            return node;
        }

        return EnsureSeparatorsHaveEndOfLine(node, node.Parameters, endOfLine);
    }

    /// <summary>
    /// Ensures that each separator in a separated syntax list has a trailing end-of-line trivia
    /// once the list is already multi-line
    /// </summary>
    /// <typeparam name="TNode">The type of the containing syntax node</typeparam>
    /// <typeparam name="TElement">The type of the elements in the separated list</typeparam>
    /// <param name="node">The containing syntax node</param>
    /// <param name="list">The separated syntax list to process</param>
    /// <param name="endOfLine">The end-of-line sequence to add after separators that need splitting</param>
    /// <returns>The node with updated separators</returns>
    private static TNode EnsureSeparatorsHaveEndOfLine<TNode, TElement>(TNode node,
                                                                        SeparatedSyntaxList<TElement> list,
                                                                        string endOfLine)
        where TNode : SyntaxNode
        where TElement : SyntaxNode
    {
        var hasElementSplitSignal = HasElementSplitSignal(list);

        if (LineBreakDetection.IsMultiLine(node) == false && hasElementSplitSignal == false)
        {
            return node;
        }

        var hasExistingLineBreak = hasElementSplitSignal;
        var tokensToReplace = new List<SyntaxToken>();
        var replacementMap = new Dictionary<SyntaxToken, SyntaxToken>();

        for (var separatorIndex = 0; separatorIndex < list.SeparatorCount; separatorIndex++)
        {
            var separator = list.GetSeparator(separatorIndex);
            var nextElement = list[separatorIndex + 1];
            var nextFirstToken = nextElement.GetFirstToken();

            if (LineBreakTriviaUtilities.HasTrailingEndOfLine(separator) || LineBreakTriviaUtilities.HasLeadingEndOfLine(nextFirstToken))
            {
                hasExistingLineBreak = true;

                continue;
            }

            var newTrailing = separator.TrailingTrivia
                                       .Where(trivia => trivia.IsKind(SyntaxKind.WhitespaceTrivia) == false)
                                       .ToList();

            newTrailing.Add(SyntaxFactory.EndOfLine(endOfLine));

            tokensToReplace.Add(separator);
            replacementMap[separator] = separator.WithTrailingTrivia(SyntaxFactory.TriviaList(newTrailing));
        }

        if (hasExistingLineBreak == false || tokensToReplace.Count == 0)
        {
            return node;
        }

        return node.ReplaceTokens(tokensToReplace, (original, _) => replacementMap[original]);
    }

    /// <summary>
    /// Determines whether any element in a separated list already signals that the outer list should split
    /// </summary>
    /// <typeparam name="TElement">The type of the elements in the separated list</typeparam>
    /// <param name="list">The separated syntax list to inspect</param>
    /// <returns><see langword="true"/> if any element already signals an outer split; otherwise, <see langword="false"/></returns>
    private static bool HasElementSplitSignal<TElement>(SeparatedSyntaxList<TElement> list)
        where TElement : SyntaxNode
    {
        return list.Any(element => LineBreakTriviaUtilities.HasLeadingEndOfLine(element.GetFirstToken())
                                   || LineBreakDetection.IsMultiLine(element));
    }

    #endregion // Methods

    #region CSharpSyntaxVisitor

    /// <inheritdoc/>
    public override SyntaxNode VisitArgumentList(ArgumentListSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (ArgumentListSyntax)base.VisitArgumentList(node);

        if (node == null)
        {
            return null;
        }

        node = CollapseFirstElementToSameLine(node, node.Arguments.Count > 0 ? node.Arguments[0].GetFirstToken() : default);

        return EnsureArgumentsOnSeparateLines(node, _context.EndOfLine);
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitBracketedArgumentList(BracketedArgumentListSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (BracketedArgumentListSyntax)base.VisitBracketedArgumentList(node);

        if (node == null)
        {
            return null;
        }

        node = CollapseBracketedArgumentsToSingleLine(node);

        return node;
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitAttributeArgumentList(AttributeArgumentListSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (AttributeArgumentListSyntax)base.VisitAttributeArgumentList(node);

        if (node == null)
        {
            return null;
        }

        node = CollapseFirstElementToSameLine(node, node.Arguments.Count > 0 ? node.Arguments[0].GetFirstToken() : default);

        return EnsureAttributeArgumentsOnSeparateLines(node, _context.EndOfLine);
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitParameterList(ParameterListSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (ParameterListSyntax)base.VisitParameterList(node);

        if (node == null)
        {
            return null;
        }

        node = CollapseOpenParenToDeclarationLine(node);
        node = CollapseFirstElementToSameLine(node, node.Parameters.Count > 0 ? node.Parameters[0].GetFirstToken() : default);
        node = CollapseSeparatorsToPreviousParameterLine(node, _context.EndOfLine);
        node = CollapseCloseParenToParameterLine(node);

        return EnsureParametersOnSeparateLines(node, _context.EndOfLine);
    }

    #endregion // CSharpSyntaxVisitor
}