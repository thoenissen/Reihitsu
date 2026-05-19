using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Formatter.Pipeline.LineBreaks;

/// <summary>
/// Applies line-break rules for argument and parameter lists
/// </summary>
internal sealed class LineBreakListRewriter : LineBreakRewriter
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="context">The formatting context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public LineBreakListRewriter(FormattingContext context,
                                 CancellationToken cancellationToken)
        : base(context,
               cancellationToken)
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Collapses the first argument to the same line as the opening parenthesis
    /// if it is currently on a new line
    /// </summary>
    /// <param name="node">The argument list node</param>
    /// <returns>The argument list with the first argument collapsed</returns>
    private static ArgumentListSyntax CollapseFirstArgumentToSameLine(ArgumentListSyntax node)
    {
        if (node.Arguments.Count == 0)
        {
            return node;
        }

        var firstArgument = node.Arguments[0];
        var firstToken = firstArgument.GetFirstToken();

        if (LineBreakTriviaUtilities.HasLeadingEndOfLine(firstToken) == false)
        {
            return node;
        }

        return CollapseTokenToSameLine(node, firstToken);
    }

    /// <summary>
    /// Collapses the first argument to the same line as the opening bracket
    /// if it is currently on a new line
    /// </summary>
    /// <param name="node">The bracketed argument list node</param>
    /// <returns>The argument list with the first argument collapsed</returns>
    private static BracketedArgumentListSyntax CollapseFirstBracketedArgumentToSameLine(BracketedArgumentListSyntax node)
    {
        if (node.Arguments.Count == 0)
        {
            return node;
        }

        var firstArgument = node.Arguments[0];
        var firstToken = firstArgument.GetFirstToken();

        if (LineBreakTriviaUtilities.HasLeadingEndOfLine(firstToken) == false)
        {
            return node;
        }

        return CollapseTokenToSameLine(node, firstToken);
    }

    /// <summary>
    /// Collapses the first attribute argument to the same line as the opening parenthesis
    /// if it is currently on a new line
    /// </summary>
    /// <param name="node">The attribute argument list node</param>
    /// <returns>The attribute argument list with the first argument collapsed</returns>
    private static AttributeArgumentListSyntax CollapseFirstAttributeArgumentToSameLine(AttributeArgumentListSyntax node)
    {
        if (node.Arguments.Count == 0)
        {
            return node;
        }

        var firstArgument = node.Arguments[0];
        var firstToken = firstArgument.GetFirstToken();

        if (LineBreakTriviaUtilities.HasLeadingEndOfLine(firstToken) == false)
        {
            return node;
        }

        return CollapseTokenToSameLine(node, firstToken);
    }

    /// <summary>
    /// Collapses the first parameter to the same line as the opening parenthesis
    /// if it is currently on a new line
    /// </summary>
    /// <param name="node">The parameter list node</param>
    /// <returns>The parameter list with the first parameter collapsed</returns>
    private static ParameterListSyntax CollapseFirstParameterToSameLine(ParameterListSyntax node)
    {
        if (node.Parameters.Count == 0)
        {
            return node;
        }

        var firstParameter = node.Parameters[0];
        var firstToken = firstParameter.GetFirstToken();

        if (LineBreakTriviaUtilities.HasLeadingEndOfLine(firstToken) == false)
        {
            return node;
        }

        return CollapseTokenToSameLine(node, firstToken);
    }

    /// <summary>
    /// Collapses the opening parenthesis of a parameter list onto the declaration line
    /// </summary>
    /// <param name="node">The parameter list node</param>
    /// <returns>The updated parameter list</returns>
    private static ParameterListSyntax CollapseOpenParenToDeclarationLine(ParameterListSyntax node)
    {
        if (TryGetPreviousToken(node, node.OpenParenToken, out var previousToken) == false
            || TokenGapUtilities.HasLineBreakBetween(previousToken, node.OpenParenToken) == false)
        {
            return node;
        }

        var newPreviousToken = previousToken.WithTrailingTrivia(LineBreakTriviaUtilities.RemoveTrailingWhitespace(LineBreakTriviaUtilities.RemoveTrailingEndOfLineTrivia(previousToken.TrailingTrivia)));
        var newOpenParen = LineBreakTriviaUtilities.RemoveLeadingEndOfLineAndWhitespace(node.OpenParenToken);

        if (ContainsToken(node, previousToken) == false)
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
        if (TryGetPreviousToken(node, node.CloseParenToken, out var previousToken) == false
            || TokenGapUtilities.HasLineBreakBetween(previousToken, node.CloseParenToken) == false)
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
    /// Ensures that all arguments in a multi-line bracketed argument list start on their own line
    /// </summary>
    /// <param name="node">The bracketed argument list node</param>
    /// <param name="endOfLine">The end-of-line sequence to insert when splitting arguments</param>
    /// <returns>The argument list with arguments on separate lines</returns>
    private static BracketedArgumentListSyntax EnsureBracketedArgumentsOnSeparateLines(BracketedArgumentListSyntax node,
                                                                                       string endOfLine)
    {
        if (node.Arguments.Count <= 1)
        {
            return node;
        }

        return EnsureSeparatorsHaveEndOfLine(node, node.Arguments, endOfLine);
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

        if (IsMultiLine(node) == false && hasElementSplitSignal == false)
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
                                   || IsMultiLine(element));
    }

    #endregion // Methods

    #region CSharpSyntaxVisitor

    /// <inheritdoc/>
    public override SyntaxNode VisitArgumentList(ArgumentListSyntax node)
    {
        CancellationToken.ThrowIfCancellationRequested();

        node = (ArgumentListSyntax)base.VisitArgumentList(node);

        if (node == null)
        {
            return null;
        }

        node = CollapseFirstArgumentToSameLine(node);

        return EnsureArgumentsOnSeparateLines(node, Context.EndOfLine);
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitBracketedArgumentList(BracketedArgumentListSyntax node)
    {
        CancellationToken.ThrowIfCancellationRequested();

        node = (BracketedArgumentListSyntax)base.VisitBracketedArgumentList(node);

        if (node == null)
        {
            return null;
        }

        node = CollapseFirstBracketedArgumentToSameLine(node);

        return EnsureBracketedArgumentsOnSeparateLines(node, Context.EndOfLine);
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitAttributeArgumentList(AttributeArgumentListSyntax node)
    {
        CancellationToken.ThrowIfCancellationRequested();

        node = (AttributeArgumentListSyntax)base.VisitAttributeArgumentList(node);

        if (node == null)
        {
            return null;
        }

        node = CollapseFirstAttributeArgumentToSameLine(node);

        return EnsureAttributeArgumentsOnSeparateLines(node, Context.EndOfLine);
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitParameterList(ParameterListSyntax node)
    {
        CancellationToken.ThrowIfCancellationRequested();

        node = (ParameterListSyntax)base.VisitParameterList(node);

        if (node == null)
        {
            return null;
        }

        node = CollapseOpenParenToDeclarationLine(node);
        node = CollapseFirstParameterToSameLine(node);
        node = CollapseSeparatorsToPreviousParameterLine(node, Context.EndOfLine);
        node = CollapseCloseParenToParameterLine(node);

        return EnsureParametersOnSeparateLines(node, Context.EndOfLine);
    }

    #endregion // CSharpSyntaxVisitor
}