using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Formatter.Pipeline.LineBreaks;

/// <summary>
/// Syntax rewriter that adds and removes line breaks in a single pass.
/// Only manipulates <see cref="SyntaxKind.EndOfLineTrivia"/>; does not set indentation
/// </summary>
internal sealed class LineBreakRewriter : CSharpSyntaxRewriter
{
    #region Fields

    /// <summary>
    /// The formatting context
    /// </summary>
    private readonly FormattingContext _context;

    /// <summary>
    /// Cancellation token
    /// </summary>
    private readonly CancellationToken _cancellationToken;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="context">The formatting context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public LineBreakRewriter(FormattingContext context, CancellationToken cancellationToken)
    {
        _context = context;
        _cancellationToken = cancellationToken;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Collapses the first <see cref="MemberBindingExpressionSyntax"/> in the
    /// <see cref="ConditionalAccessExpressionSyntax.WhenNotNull"/> subtree onto the
    /// same line as the <c>?</c> operator token, so that <c>?\n.Member()</c> becomes <c>?.Member()</c>
    /// </summary>
    /// <param name="node">The conditional access expression to process</param>
    /// <returns>The modified node with the member binding collapsed</returns>
    private static ConditionalAccessExpressionSyntax CollapseMemberBindingToQuestionToken(ConditionalAccessExpressionSyntax node)
    {
        var memberBinding = node.WhenNotNull
                                .DescendantNodesAndSelf()
                                .OfType<MemberBindingExpressionSyntax>()
                                .FirstOrDefault();

        if (memberBinding == null)
        {
            return node;
        }

        if (HasLeadingEndOfLine(memberBinding.OperatorToken) == false)
        {
            return node;
        }

        return CollapseTokenToSameLine(node, memberBinding.OperatorToken);
    }

    /// <summary>
    /// Determines whether an accessor list belongs to an auto-property
    /// (all accessors have neither a body nor an expression body)
    /// </summary>
    /// <param name="accessorList">The accessor list to inspect</param>
    /// <returns><see langword="true"/> if the accessor list is part of an auto-property; otherwise, <see langword="false"/></returns>
    private static bool IsAutoPropertyAccessorList(AccessorListSyntax accessorList)
    {
        foreach (var accessor in accessorList.Accessors)
        {
            if (accessor.Body != null || accessor.ExpressionBody != null)
            {
                return false;
            }
        }

        return true;
    }

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

        if (HasLeadingEndOfLine(firstToken) == false)
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

        if (HasLeadingEndOfLine(firstToken) == false)
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

        if (HasLeadingEndOfLine(firstToken) == false)
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

        if (HasLeadingEndOfLine(firstToken) == false)
        {
            return node;
        }

        return CollapseTokenToSameLine(node, firstToken);
    }

    /// <summary>
    /// Ensures that all arguments in a multi-line argument list start on their own line.
    /// If the argument list spans multiple lines but some arguments share a line,
    /// line breaks are inserted after each separator that lacks one
    /// </summary>
    /// <param name="node">The argument list node</param>
    /// <param name="endOfLine">The end-of-line sequence to insert when splitting arguments</param>
    /// <returns>The argument list with arguments on separate lines</returns>
    private static ArgumentListSyntax EnsureArgumentsOnSeparateLines(ArgumentListSyntax node, string endOfLine)
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
    private static BracketedArgumentListSyntax EnsureBracketedArgumentsOnSeparateLines(BracketedArgumentListSyntax node, string endOfLine)
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
    /// <returns>The argument list with arguments on separate lines</returns>
    private static AttributeArgumentListSyntax EnsureAttributeArgumentsOnSeparateLines(AttributeArgumentListSyntax node, string endOfLine)
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
    private static ParameterListSyntax EnsureParametersOnSeparateLines(ParameterListSyntax node, string endOfLine)
    {
        if (node.Parameters.Count <= 1)
        {
            return node;
        }

        return EnsureSeparatorsHaveEndOfLine(node, node.Parameters, endOfLine);
    }

    /// <summary>
    /// Ensures that each separator in a separated syntax list has a trailing end-of-line trivia.
    /// Separators that already have a trailing end-of-line (or whose following element has a leading
    /// end-of-line) are left unchanged
    /// </summary>
    /// <typeparam name="TNode">The type of the containing syntax node</typeparam>
    /// <typeparam name="TElement">The type of the elements in the separated list</typeparam>
    /// <param name="node">The containing syntax node</param>
    /// <param name="list">The separated syntax list to process</param>
    /// <param name="endOfLine">The end-of-line sequence to add after separators that need splitting</param>
    /// <returns>The node with updated separators</returns>
    private static TNode EnsureSeparatorsHaveEndOfLine<TNode, TElement>(TNode node, SeparatedSyntaxList<TElement> list, string endOfLine)
        where TNode : SyntaxNode
        where TElement : SyntaxNode
    {
        var hasEndOfLine = false;
        var tokensToReplace = new List<SyntaxToken>();
        var replacementMap = new Dictionary<SyntaxToken, SyntaxToken>();

        for (var separatorIndex = 0; separatorIndex < list.SeparatorCount; separatorIndex++)
        {
            var separator = list.GetSeparator(separatorIndex);
            var nextElement = list[separatorIndex + 1];
            var nextFirstToken = nextElement.GetFirstToken();

            if (HasTrailingEndOfLine(separator) || HasLeadingEndOfLine(nextFirstToken))
            {
                hasEndOfLine = true;

                continue;
            }

            var newTrailing = separator.TrailingTrivia
                                       .Where(trivia => trivia.IsKind(SyntaxKind.WhitespaceTrivia) == false)
                                       .ToList();

            newTrailing.Add(SyntaxFactory.EndOfLine(endOfLine));

            tokensToReplace.Add(separator);
            replacementMap[separator] = separator.WithTrailingTrivia(SyntaxFactory.TriviaList(newTrailing));
        }

        if (hasEndOfLine == false || tokensToReplace.Count == 0)
        {
            return node;
        }

        return node.ReplaceTokens(tokensToReplace, (original, _) => replacementMap[original]);
    }

    /// <summary>
    /// Collapses a multi-line expression-bodied property to a single line
    /// </summary>
    /// <param name="node">The property declaration with an expression body</param>
    /// <returns>The property declaration collapsed to a single line</returns>
    private static PropertyDeclarationSyntax CollapseExpressionBodiedProperty(PropertyDeclarationSyntax node)
    {
        if (node?.ExpressionBody == null)
        {
            return null;
        }

        var updatedNode = node;
        var arrowToken = updatedNode.ExpressionBody.ArrowToken;

        if (HasLeadingEndOfLine(arrowToken) || HasTrailingEndOfLine(arrowToken.GetPreviousToken()))
        {
            updatedNode = CollapseTokenToSameLine(updatedNode, arrowToken);
            arrowToken = updatedNode.ExpressionBody.ArrowToken;
        }

        if (arrowToken.LeadingTrivia.Any(SyntaxKind.WhitespaceTrivia) == false)
        {
            updatedNode = updatedNode.ReplaceToken(arrowToken, arrowToken.WithLeadingTrivia(arrowToken.LeadingTrivia.Add(SyntaxFactory.Space)));
        }

        var firstExpressionToken = updatedNode.ExpressionBody.Expression.GetFirstToken();

        if (HasLeadingEndOfLine(firstExpressionToken) || HasTrailingEndOfLine(firstExpressionToken.GetPreviousToken()))
        {
            updatedNode = CollapseTokenToSameLine(updatedNode, firstExpressionToken);
        }

        arrowToken = updatedNode.ExpressionBody.ArrowToken;
        firstExpressionToken = updatedNode.ExpressionBody.Expression.GetFirstToken();

        var previousToken = arrowToken.GetPreviousToken();
        var replacementMap = new Dictionary<SyntaxToken, SyntaxToken>
                             {
                                 [arrowToken] = arrowToken.WithLeadingTrivia(SyntaxFactory.Space)
                                                          .WithTrailingTrivia(SyntaxFactory.Space),
                                 [firstExpressionToken] = firstExpressionToken.WithLeadingTrivia(SyntaxFactory.TriviaList()),
                             };

        if (previousToken != default && previousToken.IsKind(SyntaxKind.None) == false)
        {
            replacementMap[previousToken] = previousToken.WithTrailingTrivia(RemoveTrailingWhitespace(RemoveTrailingEndOfLineTrivia(previousToken.TrailingTrivia)));
        }

        return updatedNode.ReplaceTokens(replacementMap.Keys, (original, _) => replacementMap[original]);
    }

    /// <summary>
    /// Determines whether an invocation expression is the outermost node in a method chain.
    /// An invocation is outermost if it is not an inner link of a larger chain
    /// and not nested inside a conditional access expression
    /// </summary>
    /// <param name="node">The invocation expression to check</param>
    /// <returns><see langword="true"/> if the invocation is the outermost chain node; otherwise, <see langword="false"/></returns>
    private static bool IsOutermostChainInvocation(InvocationExpressionSyntax node)
    {
        if (node.Expression is not MemberAccessExpressionSyntax
            && node.Expression is not MemberBindingExpressionSyntax)
        {
            return false;
        }

        if (node.Parent is MemberAccessExpressionSyntax parentAccess
            && parentAccess.Parent is InvocationExpressionSyntax)
        {
            return false;
        }

        return IsInsideConditionalAccess(node) == false;
    }

    /// <summary>
    /// Collects all chain link dot tokens from a method chain or conditional access chain.
    /// Only invoked member accesses count as chain links.
    /// For conditional access, the <c>?</c> operator token is collected (not the binding dot)
    /// </summary>
    /// <param name="node">The chain node to walk</param>
    /// <param name="dots">The list to accumulate dot tokens into</param>
    private static void CollectChainLinkDots(SyntaxNode node, List<SyntaxToken> dots)
    {
        switch (node)
        {
            case InvocationExpressionSyntax invocation when invocation.Expression is MemberAccessExpressionSyntax memberAccess:
                {
                    if (memberAccess.Expression is InvocationExpressionSyntax innerInvocation)
                    {
                        CollectChainLinkDots(innerInvocation, dots);
                    }
                    else if (memberAccess.Expression is ConditionalAccessExpressionSyntax innerConditional)
                    {
                        CollectChainLinkDots(innerConditional, dots);
                    }

                    dots.Add(memberAccess.OperatorToken);

                    break;
                }

            case ConditionalAccessExpressionSyntax conditionalAccess:
                {
                    if (conditionalAccess.Expression is InvocationExpressionSyntax innerInvocation)
                    {
                        CollectChainLinkDots(innerInvocation, dots);
                    }
                    else if (conditionalAccess.Expression is ConditionalAccessExpressionSyntax innerConditional)
                    {
                        CollectChainLinkDots(innerConditional, dots);
                    }

                    dots.Add(conditionalAccess.OperatorToken);

                    CollectWhenNotNullChainDots(conditionalAccess.WhenNotNull, dots);

                    break;
                }
        }
    }

    /// <summary>
    /// Collects chain link dot tokens from the <c>WhenNotNull</c> part of a conditional access expression.
    /// Skips <see cref="MemberBindingExpressionSyntax"/> dots since they are represented
    /// by the <c>?</c> operator token of the parent conditional access
    /// </summary>
    /// <param name="node">The WhenNotNull expression to walk</param>
    /// <param name="dots">The list to accumulate dot tokens into</param>
    private static void CollectWhenNotNullChainDots(SyntaxNode node, List<SyntaxToken> dots)
    {
        if (node is InvocationExpressionSyntax invocation
            && invocation.Expression is MemberAccessExpressionSyntax memberAccess)
        {
            CollectWhenNotNullChainDots(memberAccess.Expression, dots);

            dots.Add(memberAccess.OperatorToken);
        }
    }

    /// <summary>
    /// Determines whether an expression contains an invocation expression.
    /// Used to decide whether a conditional access chain qualifies for chain normalization
    /// </summary>
    /// <param name="expression">The expression to inspect</param>
    /// <returns><see langword="true"/> if the expression contains an invocation; otherwise, <see langword="false"/></returns>
    private static bool ContainsInvocation(ExpressionSyntax expression)
    {
        if (expression is InvocationExpressionSyntax)
        {
            return true;
        }

        foreach (var child in expression.ChildNodes())
        {
            if (child is ExpressionSyntax childExpression && ContainsInvocation(childExpression))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Determines whether a syntax node is nested inside a <see cref="ConditionalAccessExpressionSyntax"/>.
    /// Walks up the parent chain until a statement or member declaration is found
    /// </summary>
    /// <param name="node">The node to check</param>
    /// <returns><see langword="true"/> if the node is inside a conditional access expression; otherwise, <see langword="false"/></returns>
    private static bool IsInsideConditionalAccess(SyntaxNode node)
    {
        var current = node.Parent;

        while (current != null)
        {
            if (current is ConditionalAccessExpressionSyntax)
            {
                return true;
            }

            if (current is StatementSyntax || current is MemberDeclarationSyntax)
            {
                return false;
            }

            current = current.Parent;
        }

        return false;
    }

    /// <summary>
    /// Determines whether a chain dot token has intermediate member accesses between
    /// the dot and the chain root. This indicates the chain has property accesses
    /// (e.g., <c>source.Items.Where(...)</c>) and a single invocation link should not
    /// be collapsed
    /// </summary>
    /// <param name="dotToken">The dot token from a member access expression</param>
    /// <returns><see langword="true"/> if there are intermediate member accesses; otherwise, <see langword="false"/></returns>
    private static bool HasIntermediateMemberAccess(SyntaxToken dotToken)
    {
        if (dotToken.Parent is MemberAccessExpressionSyntax memberAccess)
        {
            return memberAccess.Expression is MemberAccessExpressionSyntax
                   || memberAccess.Expression is ConditionalAccessExpressionSyntax;
        }

        return false;
    }

    /// <summary>
    /// Recursively collects all <c>?</c> and <c>:</c> tokens from a nested ternary expression tree
    /// </summary>
    /// <param name="node">The conditional expression to collect tokens from</param>
    /// <param name="tokens">The list to populate with operator tokens</param>
    private static void CollectTernaryOperatorTokens(ConditionalExpressionSyntax node, List<SyntaxToken> tokens)
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
    /// Determines whether a token's leading trivia contains an end-of-line trivia,
    /// either directly or preceded only by whitespace
    /// </summary>
    /// <param name="token">The token to inspect</param>
    /// <returns><see langword="true"/> if the token has a leading end-of-line trivia; otherwise, <see langword="false"/></returns>
    private static bool HasLeadingEndOfLine(SyntaxToken token)
    {
        if (token.LeadingTrivia.Any(static trivia => trivia.IsKind(SyntaxKind.EndOfLineTrivia)))
        {
            return true;
        }

        var previousToken = token.GetPreviousToken();

        if (previousToken != default && previousToken.IsKind(SyntaxKind.None) == false)
        {
            return previousToken.TrailingTrivia.Any(static trivia => trivia.IsKind(SyntaxKind.EndOfLineTrivia));
        }

        // When previous token is unavailable (standalone subtree after rewriting),
        // if the token has leading whitespace it is likely indentation on a new line.
        if (token.LeadingTrivia.Any(SyntaxKind.WhitespaceTrivia))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Determines whether a token's trailing trivia contains an end-of-line trivia
    /// </summary>
    /// <param name="token">The token to inspect</param>
    /// <returns><see langword="true"/> if the token has a trailing end-of-line trivia; otherwise, <see langword="false"/></returns>
    private static bool HasTrailingEndOfLine(SyntaxToken token)
    {
        return token.TrailingTrivia.Any(static trivia => trivia.IsKind(SyntaxKind.EndOfLineTrivia));
    }

    /// <summary>
    /// Removes all leading end-of-line and whitespace trivia from a token.
    /// Preserves other trivia such as comments and preprocessor directives
    /// </summary>
    /// <param name="token">The token to modify</param>
    /// <returns>The token with leading end-of-line and whitespace trivia removed</returns>
    private static SyntaxToken RemoveLeadingEndOfLineAndWhitespace(SyntaxToken token)
    {
        var newLeading = new List<SyntaxTrivia>();
        var skipping = true;

        foreach (var trivia in token.LeadingTrivia)
        {
            if (skipping
                && (trivia.IsKind(SyntaxKind.EndOfLineTrivia) || trivia.IsKind(SyntaxKind.WhitespaceTrivia)))
            {
                continue;
            }

            skipping = false;

            newLeading.Add(trivia);
        }

        return token.WithLeadingTrivia(SyntaxFactory.TriviaList(newLeading));
    }

    /// <summary>
    /// Removes trailing end-of-line trivia (and any whitespace immediately before it) from a trivia list
    /// </summary>
    /// <param name="triviaList">The trivia list to modify</param>
    /// <returns>The trivia list with trailing end-of-line trivia removed</returns>
    private static SyntaxTriviaList RemoveTrailingEndOfLineTrivia(SyntaxTriviaList triviaList)
    {
        var result = new List<SyntaxTrivia>();

        foreach (var trivia in triviaList)
        {
            if (trivia.IsKind(SyntaxKind.EndOfLineTrivia))
            {
                // Remove any trailing whitespace before the EndOfLine
                while (result.Count > 0 && result[result.Count - 1].IsKind(SyntaxKind.WhitespaceTrivia))
                {
                    result.RemoveAt(result.Count - 1);
                }

                continue;
            }

            result.Add(trivia);
        }

        return SyntaxFactory.TriviaList(result);
    }

    /// <summary>
    /// Collapses a token to the same line as the previous token by removing any
    /// end-of-line trivia from both the token's leading trivia and the previous
    /// token's trailing trivia
    /// </summary>
    /// <typeparam name="TNode">The syntax node type containing the token</typeparam>
    /// <param name="node">The node containing the token</param>
    /// <param name="token">The token to collapse to the previous line</param>
    /// <returns>The node with the token collapsed to the same line</returns>
    private static TNode CollapseTokenToSameLine<TNode>(TNode node, SyntaxToken token)
        where TNode : SyntaxNode
    {
        var newToken = RemoveLeadingEndOfLineAndWhitespace(token);
        var previousToken = token.GetPreviousToken();

        if (previousToken != default
            && previousToken.IsKind(SyntaxKind.None) == false
            && HasTrailingEndOfLine(previousToken))
        {
            var newPreviousToken = previousToken.WithTrailingTrivia(RemoveTrailingEndOfLineTrivia(previousToken.TrailingTrivia));

            return node.ReplaceTokens([previousToken, token],
                                      (original, _) =>
                                      {
                                          if (original == previousToken)
                                          {
                                              return newPreviousToken;
                                          }

                                          return newToken;
                                      });
        }

        return node.ReplaceToken(token, newToken);
    }

    /// <summary>
    /// Removes trailing whitespace trivia from a trivia list
    /// </summary>
    /// <param name="triviaList">The trivia list to modify</param>
    /// <returns>The trivia list with trailing whitespace removed</returns>
    private static SyntaxTriviaList RemoveTrailingWhitespace(SyntaxTriviaList triviaList)
    {
        var result = triviaList.ToList();

        while (result.Count > 0 && result[result.Count - 1].IsKind(SyntaxKind.WhitespaceTrivia))
        {
            result.RemoveAt(result.Count - 1);
        }

        return SyntaxFactory.TriviaList(result);
    }

    /// <summary>
    /// Determines whether a syntax node spans multiple lines
    /// </summary>
    /// <param name="node">The node to inspect</param>
    /// <returns><see langword="true"/> if the node spans multiple lines; otherwise, <see langword="false"/></returns>
    private static bool IsMultiLine(SyntaxNode node)
    {
        var text = node.GetText();

        return text.Lines.Count > 1;
    }

    /// <summary>
    /// Gets the constraint clauses from a syntax node if it supports them
    /// </summary>
    /// <param name="node">The syntax node to inspect</param>
    /// <returns>The list of constraint clauses, or <see langword="null"/> if not applicable</returns>
    private static SyntaxList<TypeParameterConstraintClauseSyntax>? GetConstraintClauses(SyntaxNode node)
    {
        switch (node)
        {
            case ClassDeclarationSyntax classDecl:
                return classDecl.ConstraintClauses;

            case StructDeclarationSyntax structDecl:
                return structDecl.ConstraintClauses;

            case InterfaceDeclarationSyntax interfaceDecl:
                return interfaceDecl.ConstraintClauses;

            case RecordDeclarationSyntax recordDecl:
                return recordDecl.ConstraintClauses;

            case MethodDeclarationSyntax methodDecl:
                return methodDecl.ConstraintClauses;

            case DelegateDeclarationSyntax delegateDecl:
                return delegateDecl.ConstraintClauses;

            case LocalFunctionStatementSyntax localFunc:
                return localFunc.ConstraintClauses;

            default:
                return null;
        }
    }

    /// <summary>
    /// Sets the constraint clauses on a syntax node
    /// </summary>
    /// <typeparam name="TNode">The syntax node type</typeparam>
    /// <param name="node">The syntax node to modify</param>
    /// <param name="constraintClauses">The new constraint clauses</param>
    /// <returns>The node with updated constraint clauses</returns>
    private static TNode SetConstraintClauses<TNode>(TNode node, SyntaxList<TypeParameterConstraintClauseSyntax> constraintClauses)
        where TNode : SyntaxNode
    {
        switch (node)
        {
            case ClassDeclarationSyntax classDecl:
                return (TNode)(SyntaxNode)classDecl.WithConstraintClauses(constraintClauses);

            case StructDeclarationSyntax structDecl:
                return (TNode)(SyntaxNode)structDecl.WithConstraintClauses(constraintClauses);

            case InterfaceDeclarationSyntax interfaceDecl:
                return (TNode)(SyntaxNode)interfaceDecl.WithConstraintClauses(constraintClauses);

            case RecordDeclarationSyntax recordDecl:
                return (TNode)(SyntaxNode)recordDecl.WithConstraintClauses(constraintClauses);

            case MethodDeclarationSyntax methodDecl:
                return (TNode)(SyntaxNode)methodDecl.WithConstraintClauses(constraintClauses);

            case DelegateDeclarationSyntax delegateDecl:
                return (TNode)(SyntaxNode)delegateDecl.WithConstraintClauses(constraintClauses);

            case LocalFunctionStatementSyntax localFunc:
                return (TNode)(SyntaxNode)localFunc.WithConstraintClauses(constraintClauses);

            default:
                return node;
        }
    }

    /// <summary>
    /// Removes trailing whitespace from the token immediately before an initializer's close brace
    /// when the close brace has been moved to a new line
    /// </summary>
    /// <param name="node">The initializer expression</param>
    /// <returns>The initializer with trailing whitespace cleaned up</returns>
    private static InitializerExpressionSyntax CleanupTrailingWhitespaceBeforeCloseBrace(InitializerExpressionSyntax node)
    {
        var closeBrace = node.CloseBraceToken;

        if (closeBrace.LeadingTrivia.Any(SyntaxKind.EndOfLineTrivia) == false)
        {
            return node;
        }

        var previousToken = closeBrace.GetPreviousToken();

        if (previousToken == default
            || previousToken.IsKind(SyntaxKind.None)
            || HasTrailingEndOfLine(previousToken)
            || previousToken.TrailingTrivia.Any(SyntaxKind.WhitespaceTrivia) == false)
        {
            return node;
        }

        var newTrailing = RemoveTrailingWhitespace(previousToken.TrailingTrivia);
        var newPreviousToken = previousToken.WithTrailingTrivia(newTrailing);

        return node.ReplaceToken(previousToken, newPreviousToken);
    }

    /// <summary>
    /// Removes trailing whitespace from the token immediately before an initializer's open brace
    /// when the brace has been moved to a new line
    /// </summary>
    /// <typeparam name="TNode">The parent syntax node type</typeparam>
    /// <param name="node">The parent node containing the initializer</param>
    /// <param name="initializer">The initializer expression, or <see langword="null"/></param>
    /// <returns>The node with trailing whitespace cleaned up</returns>
    private static TNode CleanupTrailingWhitespaceBeforeInitializerBrace<TNode>(TNode node, InitializerExpressionSyntax initializer)
        where TNode : SyntaxNode
    {
        if (initializer == null)
        {
            return node;
        }

        var openBrace = initializer.OpenBraceToken;

        if (openBrace.LeadingTrivia.Any(SyntaxKind.EndOfLineTrivia) == false)
        {
            return node;
        }

        var previousToken = openBrace.GetPreviousToken();

        if (previousToken == default
            || previousToken.IsKind(SyntaxKind.None)
            || HasTrailingEndOfLine(previousToken)
            || previousToken.TrailingTrivia.Any(SyntaxKind.WhitespaceTrivia) == false)
        {
            return node;
        }

        var newTrailing = RemoveTrailingWhitespace(previousToken.TrailingTrivia);
        var newPreviousToken = previousToken.WithTrailingTrivia(newTrailing);

        return node.ReplaceToken(previousToken, newPreviousToken);
    }

    /// <summary>
    /// Normalizes a chain containing a single dot token
    /// </summary>
    /// <param name="node">The chain node</param>
    /// <param name="chainDot">The chain dot token</param>
    /// <returns>The updated chain node</returns>
    private static SyntaxNode NormalizeSingleChainDot(SyntaxNode node, SyntaxToken chainDot)
    {
        if (HasLeadingEndOfLine(chainDot)
            && HasIntermediateMemberAccess(chainDot) == false)
        {
            return CollapseTokenToSameLine(node, chainDot);
        }

        return node;
    }

    /// <summary>
    /// Collapses the first chain dot onto the root line when it starts on a continuation line
    /// </summary>
    /// <param name="firstDot">The first chain dot token</param>
    /// <param name="replacements">The token replacement map to populate</param>
    private static void TryCollapseFirstChainDot(SyntaxToken firstDot, Dictionary<SyntaxToken, SyntaxToken> replacements)
    {
        if (HasLeadingEndOfLine(firstDot) == false
            || HasIntermediateMemberAccess(firstDot))
        {
            return;
        }

        replacements[firstDot] = RemoveLeadingEndOfLineAndWhitespace(firstDot);

        var previousToken = firstDot.GetPreviousToken();

        if (previousToken != default
            && previousToken.IsKind(SyntaxKind.None) == false
            && HasTrailingEndOfLine(previousToken))
        {
            replacements[previousToken] = previousToken.WithTrailingTrivia(RemoveTrailingEndOfLineTrivia(previousToken.TrailingTrivia));
        }
    }

    /// <summary>
    /// Moves a token to a new line by prepending an end-of-line trivia to its leading trivia.
    /// Also strips any trailing whitespace from the previous token to avoid orphaned spaces
    /// </summary>
    /// <typeparam name="TNode">The syntax node type containing the token</typeparam>
    /// <param name="node">The node containing the token</param>
    /// <param name="token">The token to move to a new line</param>
    /// <returns>The node with the token moved to a new line</returns>
    private TNode MoveTokenToNewLine<TNode>(TNode node, SyntaxToken token)
        where TNode : SyntaxNode
    {
        var newToken = PrependEndOfLine(token);
        var previousToken = token.GetPreviousToken();

        if (previousToken != default
            && previousToken.IsKind(SyntaxKind.None) == false
            && HasTrailingEndOfLine(previousToken) == false
            && previousToken.TrailingTrivia.Any(SyntaxKind.WhitespaceTrivia))
        {
            var newPreviousToken = previousToken.WithTrailingTrivia(RemoveTrailingWhitespace(previousToken.TrailingTrivia));

            return node.ReplaceTokens([previousToken, token],
                                      (original, _) =>
                                      {
                                          if (original == previousToken)
                                          {
                                              return newPreviousToken;
                                          }

                                          return newToken;
                                      });
        }

        return node.ReplaceToken(token, newToken);
    }

    /// <summary>
    /// Normalizes a method chain or conditional access chain.
    /// For multi-line chains: collapses the first chain link to the root line
    /// and ensures all subsequent links start on their own line.
    /// Single-line chains are not modified
    /// </summary>
    /// <param name="node">The outermost chain node (invocation or conditional access)</param>
    /// <returns>The node with normalized chain line breaks</returns>
    private SyntaxNode NormalizeChain(SyntaxNode node)
    {
        var chainDots = new List<SyntaxToken>();

        CollectChainLinkDots(node, chainDots);

        if (chainDots.Count == 0)
        {
            return node;
        }

        if (chainDots.Count == 1)
        {
            return NormalizeSingleChainDot(node, chainDots[0]);
        }

        if (chainDots.Exists(HasLeadingEndOfLine) == false)
        {
            return node;
        }

        var replacements = new Dictionary<SyntaxToken, SyntaxToken>();

        TryCollapseFirstChainDot(chainDots[0], replacements);
        EnsureContinuationDotsStartOnNewLine(chainDots, replacements);

        if (replacements.Count == 0)
        {
            return node;
        }

        return node.ReplaceTokens(replacements.Keys,
                                  (original, _) => replacements[original]);
    }

    /// <summary>
    /// Ensures continuation dots in a chain start on their own lines
    /// </summary>
    /// <param name="chainDots">The chain dot tokens</param>
    /// <param name="replacements">The token replacement map to populate</param>
    private void EnsureContinuationDotsStartOnNewLine(List<SyntaxToken> chainDots, Dictionary<SyntaxToken, SyntaxToken> replacements)
    {
        var endOfLine = SyntaxFactory.EndOfLine(_context.EndOfLine);

        for (var dotIndex = 1; dotIndex < chainDots.Count; dotIndex++)
        {
            if (HasLeadingEndOfLine(chainDots[dotIndex]))
            {
                continue;
            }

            var newLeading = chainDots[dotIndex].LeadingTrivia.Insert(0, endOfLine);
            replacements[chainDots[dotIndex]] = chainDots[dotIndex].WithLeadingTrivia(newLeading);

            var previousToken = chainDots[dotIndex].GetPreviousToken();

            if (previousToken != default
                && previousToken.IsKind(SyntaxKind.None) == false
                && replacements.ContainsKey(previousToken) == false
                && previousToken.TrailingTrivia.Any(SyntaxKind.WhitespaceTrivia))
            {
                replacements[previousToken] = previousToken.WithTrailingTrivia(RemoveTrailingWhitespace(previousToken.TrailingTrivia));
            }
        }
    }

    /// <summary>
    /// Ensures each expression in a collection or object initializer starts on its own line.
    /// Also strips trailing whitespace from the previous token (typically a comma or open brace)
    /// </summary>
    /// <param name="node">The initializer expression node</param>
    /// <returns>The initializer with each item on a separate line</returns>
    private InitializerExpressionSyntax EnsureInitializerItemsOnSeparateLines(InitializerExpressionSyntax node)
    {
        for (var expressionIndex = 0; expressionIndex < node.Expressions.Count; expressionIndex++)
        {
            var expression = node.Expressions[expressionIndex];
            var firstToken = expression.GetFirstToken();

            if (HasLeadingEndOfLine(firstToken) == false)
            {
                node = MoveTokenToNewLine(node, firstToken);
            }
        }

        return node;
    }

    /// <summary>
    /// Ensures each member in an anonymous object creation expression starts on its own line
    /// </summary>
    /// <param name="node">The anonymous object creation expression node</param>
    /// <returns>The node with each member on a separate line</returns>
    private AnonymousObjectCreationExpressionSyntax EnsureAnonymousObjectMembersOnSeparateLines(AnonymousObjectCreationExpressionSyntax node)
    {
        for (var memberIndex = 0; memberIndex < node.Initializers.Count; memberIndex++)
        {
            var member = node.Initializers[memberIndex];
            var firstToken = member.GetFirstToken();

            if (HasLeadingEndOfLine(firstToken) == false)
            {
                node = MoveTokenToNewLine(node, firstToken);
            }
        }

        return node;
    }

    /// <summary>
    /// Ensures an opening brace is on its own line by prepending an end-of-line trivia if missing.
    /// Also ensures the closing brace is on its own line
    /// </summary>
    /// <typeparam name="TNode">The syntax node type containing the braces</typeparam>
    /// <param name="node">The node containing the braces</param>
    /// <param name="openBrace">The open brace token</param>
    /// <param name="withOpenBrace">Function to replace the open brace on the node</param>
    /// <param name="closeBrace">The close brace token</param>
    /// <param name="withCloseBrace">Function to replace the close brace on the node</param>
    /// <returns>The node with braces placed on their own lines</returns>
    private TNode EnsureBraceOnOwnLine<TNode>(TNode node, SyntaxToken openBrace, Func<TNode, SyntaxToken, TNode> withOpenBrace, SyntaxToken closeBrace, Func<TNode, SyntaxToken, TNode> withCloseBrace)
        where TNode : SyntaxNode
    {
        if (openBrace.IsMissing == false && HasLeadingEndOfLine(openBrace) == false)
        {
            var newOpenBrace = PrependEndOfLine(openBrace);

            node = withOpenBrace(node, newOpenBrace);
        }

        if (closeBrace.IsMissing == false && HasLeadingEndOfLine(closeBrace) == false)
        {
            var newCloseBrace = PrependEndOfLine(closeBrace);

            node = withCloseBrace(node, newCloseBrace);
        }

        return node;
    }

    /// <summary>
    /// Ensures the first token after an opening brace is on a new line.
    /// Also strips trailing whitespace from the open brace token
    /// </summary>
    /// <typeparam name="TNode">The syntax node type</typeparam>
    /// <param name="node">The node containing the opening brace</param>
    /// <param name="openBrace">The opening brace token</param>
    /// <returns>The node with the first content token on a new line</returns>
    private TNode EnsureFirstContentOnNewLine<TNode>(TNode node, SyntaxToken openBrace)
        where TNode : SyntaxNode
    {
        if (openBrace.IsMissing)
        {
            return node;
        }

        var nextToken = openBrace.GetNextToken();

        if (nextToken == default || nextToken.IsMissing)
        {
            return node;
        }

        if (HasLeadingEndOfLine(nextToken))
        {
            return node;
        }

        // If the trailing trivia of the open brace already contains an end of line, no action needed
        if (HasTrailingEndOfLine(openBrace))
        {
            return node;
        }

        return MoveTokenToNewLine(node, nextToken);
    }

    /// <summary>
    /// Ensures a line break after a closing brace unless the next token is <c>;</c>, <c>,</c>, or <c>)</c>
    /// </summary>
    /// <typeparam name="TNode">The syntax node type</typeparam>
    /// <param name="node">The node containing the closing brace</param>
    /// <param name="closeBrace">The closing brace token</param>
    /// <returns>The node with correct close-brace continuation</returns>
    private TNode EnsureCloseBraceContinuation<TNode>(TNode node, SyntaxToken closeBrace)
        where TNode : SyntaxNode
    {
        if (closeBrace.IsMissing)
        {
            return node;
        }

        var nextToken = closeBrace.GetNextToken();

        if (nextToken == default || nextToken.IsMissing)
        {
            return node;
        }

        // These tokens are allowed on the same line after '}'
        if (nextToken.IsKind(SyntaxKind.SemicolonToken)
            || nextToken.IsKind(SyntaxKind.CommaToken)
            || nextToken.IsKind(SyntaxKind.CloseParenToken))
        {
            return node;
        }

        if (HasLeadingEndOfLine(nextToken) || HasTrailingEndOfLine(closeBrace))
        {
            return node;
        }

        var newNextToken = PrependEndOfLine(nextToken);

        return node.ReplaceToken(nextToken, newNextToken);
    }

    /// <summary>
    /// Normalizes binary operator position: if the operator is at the end of a line
    /// (trailing trivia contains EndOfLine), moves it to the beginning of the next line.
    /// This only moves the line break; indentation is handled separately
    /// </summary>
    /// <param name="node">The binary expression node</param>
    /// <returns>The binary expression with the operator at the beginning of the continuation line</returns>
    private BinaryExpressionSyntax NormalizeBinaryOperatorPosition(BinaryExpressionSyntax node)
    {
        var operatorToken = node.OperatorToken;

        if (HasTrailingEndOfLine(operatorToken) == false)
        {
            return node;
        }

        // The operator is at the end of a line. Move the line break so the operator
        // starts the next line instead.
        // 1. Remove EndOfLine from operator trailing trivia
        // 2. Append EndOfLine to left operand's last token trailing trivia
        // 3. Clean leading EOL/whitespace from right operand's first token
        var leftLastToken = node.Left.GetLastToken();
        var newOperatorTrailing = RemoveTrailingEndOfLineTrivia(operatorToken.TrailingTrivia);
        var newLeftTrailing = AppendEndOfLine(leftLastToken.TrailingTrivia);

        var newLeftLastToken = leftLastToken.WithTrailingTrivia(newLeftTrailing);
        var newOperatorToken = operatorToken.WithTrailingTrivia(newOperatorTrailing);

        var rightFirstToken = node.Right.GetFirstToken();
        var newRightFirstToken = RemoveLeadingEndOfLineAndWhitespace(rightFirstToken);

        node = node.ReplaceTokens([leftLastToken, operatorToken, rightFirstToken],
                                  (original, _) =>
                                  {
                                      if (original == leftLastToken)
                                      {
                                          return newLeftLastToken;
                                      }

                                      if (original == operatorToken)
                                      {
                                          return newOperatorToken;
                                      }

                                      return newRightFirstToken;
                                  });

        return node;
    }

    /// <summary>
    /// For multi-line conditional expressions, ensures <c>?</c> and <c>:</c> are placed on new lines.
    /// If the operator is at the end of a line (trailing EndOfLine), it is moved to the beginning
    /// of the next line instead
    /// </summary>
    /// <param name="node">The conditional expression node</param>
    /// <returns>The conditional expression with ternary operators on new lines</returns>
    private ConditionalExpressionSyntax NormalizeTernaryOperatorPosition(ConditionalExpressionSyntax node)
    {
        if (IsMultiLine(node) == false)
        {
            // Single-line nested ternaries must be broken to multi-line
            if (node.WhenFalse is ConditionalExpressionSyntax == false
                && node.WhenTrue is ConditionalExpressionSyntax == false)
            {
                return node;
            }

            var operatorTokens = new List<SyntaxToken>();

            CollectTernaryOperatorTokens(node, operatorTokens);

            node = node.ReplaceTokens(operatorTokens, (original, _) => PrependEndOfLine(original));
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

        if (HasTrailingEndOfLine(questionToken))
        {
            return MoveQuestionTokenToNextLine(node, questionToken);
        }

        if (HasLeadingEndOfLine(questionToken))
        {
            return node;
        }

        return node.WithQuestionToken(PrependEndOfLine(questionToken));
    }

    /// <summary>
    /// Moves the ternary <c>?</c> token from line-end position to line-start position
    /// </summary>
    /// <param name="node">The conditional expression node</param>
    /// <param name="questionToken">The question mark token</param>
    /// <returns>The updated conditional expression</returns>
    private ConditionalExpressionSyntax MoveQuestionTokenToNextLine(ConditionalExpressionSyntax node, SyntaxToken questionToken)
    {
        // ? is at end of condition line — move line break so ? starts the next line.
        var conditionLastToken = node.Condition.GetLastToken();
        var newQuestionTrailing = RemoveTrailingEndOfLineTrivia(questionToken.TrailingTrivia);
        var newConditionTrailing = AppendEndOfLine(conditionLastToken.TrailingTrivia);
        var newConditionLastToken = conditionLastToken.WithTrailingTrivia(newConditionTrailing);
        var newQuestionToken = questionToken.WithTrailingTrivia(newQuestionTrailing);

        // Also collapse the true expression onto the same line as ?.
        var whenTrueFirstToken = node.WhenTrue.GetFirstToken();
        var newWhenTrueFirstToken = RemoveLeadingEndOfLineAndWhitespace(whenTrueFirstToken);

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

        if (HasTrailingEndOfLine(colonToken))
        {
            // : is at end of line — move line break so : starts the next line.
            var whenTrueLastToken = node.WhenTrue.GetLastToken();
            var newColonTrailing = RemoveTrailingEndOfLineTrivia(colonToken.TrailingTrivia);
            var newWhenTrueTrailing = AppendEndOfLine(whenTrueLastToken.TrailingTrivia);
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

        if (HasLeadingEndOfLine(colonToken))
        {
            return node;
        }

        return node.WithColonToken(PrependEndOfLine(colonToken));
    }

    /// <summary>
    /// Ensures the constructor initializer (<c>: base()</c> or <c>: this()</c>) starts on a new line
    /// </summary>
    /// <param name="node">The constructor declaration node</param>
    /// <returns>The constructor declaration with the initializer on a new line</returns>
    private ConstructorDeclarationSyntax EnsureConstructorInitializerOnNewLine(ConstructorDeclarationSyntax node)
    {
        if (node.Initializer == null)
        {
            return node;
        }

        var colonToken = node.Initializer.ColonToken;

        if (HasLeadingEndOfLine(colonToken))
        {
            return node;
        }

        var newColonToken = PrependEndOfLine(colonToken);

        return node.WithInitializer(node.Initializer.WithColonToken(newColonToken));
    }

    /// <summary>
    /// Ensures all <c>where</c> constraint clauses in a type declaration start on new lines
    /// </summary>
    /// <typeparam name="TNode">The type declaration syntax type</typeparam>
    /// <param name="node">The type declaration node</param>
    /// <returns>The node with <c>where</c> clauses on new lines</returns>
    private TNode EnsureGenericConstraintsOnNewLines<TNode>(TNode node)
        where TNode : SyntaxNode
    {
        var constraintClauses = GetConstraintClauses(node);

        if (constraintClauses == null || constraintClauses.Value.Count == 0)
        {
            return node;
        }

        var modified = false;
        var newClauses = new List<TypeParameterConstraintClauseSyntax>();

        foreach (var clause in constraintClauses.Value)
        {
            var whereKeyword = clause.WhereKeyword;

            if (HasLeadingEndOfLine(whereKeyword) == false)
            {
                var newWhereKeyword = PrependEndOfLine(whereKeyword);

                newClauses.Add(clause.WithWhereKeyword(newWhereKeyword));

                modified = true;
            }
            else
            {
                newClauses.Add(clause);
            }
        }

        if (modified == false)
        {
            return node;
        }

        var newConstraintList = SyntaxFactory.List(newClauses);

        return SetConstraintClauses(node, newConstraintList);
    }

    /// <summary>
    /// Prepends an end-of-line trivia to a token's leading trivia
    /// </summary>
    /// <param name="token">The token to modify</param>
    /// <returns>The token with an end-of-line trivia prepended to its leading trivia</returns>
    private SyntaxToken PrependEndOfLine(SyntaxToken token)
    {
        var endOfLine = SyntaxFactory.EndOfLine(_context.EndOfLine);
        var newLeading = token.LeadingTrivia.Insert(0, endOfLine);

        return token.WithLeadingTrivia(newLeading);
    }

    /// <summary>
    /// Appends an end-of-line trivia to a trivia list
    /// </summary>
    /// <param name="triviaList">The trivia list to extend</param>
    /// <returns>The trivia list with an end-of-line trivia appended</returns>
    private SyntaxTriviaList AppendEndOfLine(SyntaxTriviaList triviaList)
    {
        return triviaList.Add(SyntaxFactory.EndOfLine(_context.EndOfLine));
    }

    #endregion // Methods

    #region CSharpSyntaxRewriter

    /// <inheritdoc/>
    public override SyntaxNode VisitBlock(BlockSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (BlockSyntax)base.VisitBlock(node);

        if (node == null)
        {
            return null;
        }

        node = EnsureBraceOnOwnLine(node, node.OpenBraceToken, (n, t) => n.WithOpenBraceToken(t), node.CloseBraceToken, (n, t) => n.WithCloseBraceToken(t));
        node = EnsureFirstContentOnNewLine(node, node.OpenBraceToken);
        node = EnsureCloseBraceContinuation(node, node.CloseBraceToken);

        return node;
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (ClassDeclarationSyntax)base.VisitClassDeclaration(node);

        if (node == null)
        {
            return null;
        }

        node = EnsureGenericConstraintsOnNewLines(node);
        node = EnsureBraceOnOwnLine(node, node.OpenBraceToken, (n, t) => n.WithOpenBraceToken(t), node.CloseBraceToken, (n, t) => n.WithCloseBraceToken(t));
        node = EnsureFirstContentOnNewLine(node, node.OpenBraceToken);
        node = EnsureCloseBraceContinuation(node, node.CloseBraceToken);

        return node;
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitStructDeclaration(StructDeclarationSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (StructDeclarationSyntax)base.VisitStructDeclaration(node);

        if (node == null)
        {
            return null;
        }

        node = EnsureGenericConstraintsOnNewLines(node);
        node = EnsureBraceOnOwnLine(node, node.OpenBraceToken, (n, t) => n.WithOpenBraceToken(t), node.CloseBraceToken, (n, t) => n.WithCloseBraceToken(t));
        node = EnsureFirstContentOnNewLine(node, node.OpenBraceToken);
        node = EnsureCloseBraceContinuation(node, node.CloseBraceToken);

        return node;
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitInterfaceDeclaration(InterfaceDeclarationSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (InterfaceDeclarationSyntax)base.VisitInterfaceDeclaration(node);

        if (node == null)
        {
            return null;
        }

        node = EnsureGenericConstraintsOnNewLines(node);
        node = EnsureBraceOnOwnLine(node, node.OpenBraceToken, (n, t) => n.WithOpenBraceToken(t), node.CloseBraceToken, (n, t) => n.WithCloseBraceToken(t));
        node = EnsureFirstContentOnNewLine(node, node.OpenBraceToken);
        node = EnsureCloseBraceContinuation(node, node.CloseBraceToken);

        return node;
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitRecordDeclaration(RecordDeclarationSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (RecordDeclarationSyntax)base.VisitRecordDeclaration(node);

        if (node == null)
        {
            return null;
        }

        node = EnsureGenericConstraintsOnNewLines(node);

        if (node.OpenBraceToken.IsMissing == false)
        {
            node = EnsureBraceOnOwnLine(node, node.OpenBraceToken, (n, t) => n.WithOpenBraceToken(t), node.CloseBraceToken, (n, t) => n.WithCloseBraceToken(t));
            node = EnsureFirstContentOnNewLine(node, node.OpenBraceToken);
            node = EnsureCloseBraceContinuation(node, node.CloseBraceToken);
        }

        return node;
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitEnumDeclaration(EnumDeclarationSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (EnumDeclarationSyntax)base.VisitEnumDeclaration(node);

        if (node == null)
        {
            return null;
        }

        node = EnsureBraceOnOwnLine(node, node.OpenBraceToken, (n, t) => n.WithOpenBraceToken(t), node.CloseBraceToken, (n, t) => n.WithCloseBraceToken(t));
        node = EnsureFirstContentOnNewLine(node, node.OpenBraceToken);
        node = EnsureCloseBraceContinuation(node, node.CloseBraceToken);

        return node;
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitNamespaceDeclaration(NamespaceDeclarationSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (NamespaceDeclarationSyntax)base.VisitNamespaceDeclaration(node);

        if (node == null)
        {
            return null;
        }

        node = EnsureBraceOnOwnLine(node, node.OpenBraceToken, (n, t) => n.WithOpenBraceToken(t), node.CloseBraceToken, (n, t) => n.WithCloseBraceToken(t));
        node = EnsureFirstContentOnNewLine(node, node.OpenBraceToken);
        node = EnsureCloseBraceContinuation(node, node.CloseBraceToken);

        return node;
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitAccessorList(AccessorListSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (AccessorListSyntax)base.VisitAccessorList(node);

        if (node == null)
        {
            return null;
        }

        if (IsAutoPropertyAccessorList(node))
        {
            return node;
        }

        node = EnsureBraceOnOwnLine(node, node.OpenBraceToken, (n, t) => n.WithOpenBraceToken(t), node.CloseBraceToken, (n, t) => n.WithCloseBraceToken(t));
        node = EnsureFirstContentOnNewLine(node, node.OpenBraceToken);
        node = EnsureCloseBraceContinuation(node, node.CloseBraceToken);

        return node;
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitSwitchStatement(SwitchStatementSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (SwitchStatementSyntax)base.VisitSwitchStatement(node);

        if (node == null)
        {
            return null;
        }

        node = EnsureBraceOnOwnLine(node, node.OpenBraceToken, (n, t) => n.WithOpenBraceToken(t), node.CloseBraceToken, (n, t) => n.WithCloseBraceToken(t));
        node = EnsureFirstContentOnNewLine(node, node.OpenBraceToken);
        node = EnsureCloseBraceContinuation(node, node.CloseBraceToken);

        return node;
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitInitializerExpression(InitializerExpressionSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        var originalParent = node.Parent;

        node = (InitializerExpressionSyntax)base.VisitInitializerExpression(node);

        if (node == null)
        {
            return null;
        }

        // Array initializers (new int[] { 1 }, new[] { 1 }) stay inline — no Allman placement
        if (node.IsKind(SyntaxKind.ArrayInitializerExpression))
        {
            return node;
        }

        // For collection/object initializers with multiple items, ensure each item is on its own line
        if (node.Expressions.Count > 1)
        {
            node = EnsureInitializerItemsOnSeparateLines(node);
        }

        // Collection initializers inside property assignments keep brace inline (e.g., Validators = { ... })
        if (node.IsKind(SyntaxKind.CollectionInitializerExpression)
            && originalParent is AssignmentExpressionSyntax)
        {
            node = node.WithOpenBraceToken(RemoveLeadingEndOfLineAndWhitespace(node.OpenBraceToken));

            if (node.CloseBraceToken.IsMissing == false && HasLeadingEndOfLine(node.CloseBraceToken) == false)
            {
                node = node.WithCloseBraceToken(PrependEndOfLine(node.CloseBraceToken));
            }
        }
        else if (node != null)
        {
            node = EnsureBraceOnOwnLine(node, node.OpenBraceToken, (n, t) => n.WithOpenBraceToken(t), node.CloseBraceToken, (n, t) => n.WithCloseBraceToken(t));
        }

        if (node != null)
        {
            node = EnsureFirstContentOnNewLine(node, node.OpenBraceToken);
        }

        if (node != null)
        {
            node = EnsureCloseBraceContinuation(node, node.CloseBraceToken);
        }

        // Clean trailing whitespace before close brace when it was moved to a new line
        if (node != null)
        {
            node = CleanupTrailingWhitespaceBeforeCloseBrace(node);
        }

        return node;
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitAssignmentExpression(AssignmentExpressionSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (AssignmentExpressionSyntax)base.VisitAssignmentExpression(node);

        if (node == null)
        {
            return null;
        }

        // Collection initializers in property assignments keep brace inline — strip trailing EOL from '='
        if (node.Right is InitializerExpressionSyntax init
            && init.IsKind(SyntaxKind.CollectionInitializerExpression)
            && HasTrailingEndOfLine(node.OperatorToken))
        {
            var newTrivia = RemoveTrailingEndOfLineTrivia(node.OperatorToken.TrailingTrivia);

            node = node.WithOperatorToken(node.OperatorToken.WithTrailingTrivia(newTrivia));
        }

        // Collection expressions in property assignments keep bracket on the '=' line
        if (node.Right is CollectionExpressionSyntax)
        {
            var operatorToken = node.OperatorToken;
            var openBracket = node.Right.GetFirstToken();

            if (HasTrailingEndOfLine(operatorToken))
            {
                var newOperatorTrivia = RemoveTrailingEndOfLineTrivia(operatorToken.TrailingTrivia);

                if (newOperatorTrivia.Any(SyntaxKind.WhitespaceTrivia) == false)
                {
                    newOperatorTrivia = newOperatorTrivia.Add(SyntaxFactory.Space);
                }

                var newOpenBracket = RemoveLeadingEndOfLineAndWhitespace(openBracket);

                node = node.ReplaceTokens([operatorToken, openBracket],
                                          (original, _) =>
                                          {
                                              if (original == operatorToken)
                                              {
                                                  return operatorToken.WithTrailingTrivia(newOperatorTrivia);
                                              }

                                              return newOpenBracket;
                                          });
            }
        }

        // Rules for simple assignment (=): enforce same-line placement of operator and value.
        if (node.IsKind(SyntaxKind.SimpleAssignmentExpression))
        {
            // Rule 1: The equals operator must be on the same line as the assignment target.
            var operatorToken = node.OperatorToken;

            if (HasLeadingEndOfLine(operatorToken))
            {
                var newOperatorToken = RemoveLeadingEndOfLineAndWhitespace(operatorToken);

                if (newOperatorToken.LeadingTrivia.Any(SyntaxKind.WhitespaceTrivia) == false)
                {
                    newOperatorToken = newOperatorToken.WithLeadingTrivia(newOperatorToken.LeadingTrivia.Add(SyntaxFactory.Space));
                }

                var previousToken = operatorToken.GetPreviousToken();

                if (previousToken != default
                    && previousToken.IsKind(SyntaxKind.None) == false
                    && HasTrailingEndOfLine(previousToken))
                {
                    var newPreviousToken = previousToken.WithTrailingTrivia(RemoveTrailingEndOfLineTrivia(previousToken.TrailingTrivia));

                    node = node.ReplaceTokens([previousToken, operatorToken],
                                              (original, _) =>
                                              {
                                                  if (original == previousToken)
                                                  {
                                                      return newPreviousToken;
                                                  }

                                                  return newOperatorToken;
                                              });
                }
                else
                {
                    node = node.WithOperatorToken(newOperatorToken);
                }
            }

            // Rule 2: The right-hand side must start on the same line as the equals operator.
            // Collection expressions and collection initializers are already handled above.
            if (node.Right is not CollectionExpressionSyntax
                && node.Right is not InitializerExpressionSyntax
                && HasTrailingEndOfLine(node.OperatorToken))
            {
                var rightFirstToken = node.Right.GetFirstToken();

                if (rightFirstToken != default
                    && HasLeadingEndOfLine(rightFirstToken))
                {
                    var currentOperatorToken = node.OperatorToken;
                    var newOperatorTrivia = RemoveTrailingEndOfLineTrivia(currentOperatorToken.TrailingTrivia);

                    if (newOperatorTrivia.Any(SyntaxKind.WhitespaceTrivia) == false)
                    {
                        newOperatorTrivia = newOperatorTrivia.Add(SyntaxFactory.Space);
                    }

                    var newRightFirstToken = RemoveLeadingEndOfLineAndWhitespace(rightFirstToken);

                    node = node.ReplaceTokens([currentOperatorToken, rightFirstToken],
                                              (original, _) =>
                                              {
                                                  if (original == currentOperatorToken)
                                                  {
                                                      return currentOperatorToken.WithTrailingTrivia(newOperatorTrivia);
                                                  }

                                                  return newRightFirstToken;
                                              });
                }
            }
        }

        return node;
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitEqualsValueClause(EqualsValueClauseSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (EqualsValueClauseSyntax)base.VisitEqualsValueClause(node);

        if (node == null)
        {
            return null;
        }

        // Collection expressions in field/variable declarations keep bracket on the '=' line
        if (node.Value is CollectionExpressionSyntax)
        {
            var equalsToken = node.EqualsToken;
            var openBracket = node.Value.GetFirstToken();

            if (HasTrailingEndOfLine(equalsToken))
            {
                var newEqualsTrivia = RemoveTrailingEndOfLineTrivia(equalsToken.TrailingTrivia);

                if (newEqualsTrivia.Any(SyntaxKind.WhitespaceTrivia) == false)
                {
                    newEqualsTrivia = newEqualsTrivia.Add(SyntaxFactory.Space);
                }

                var newOpenBracket = RemoveLeadingEndOfLineAndWhitespace(openBracket);

                node = node.ReplaceTokens([equalsToken, openBracket],
                                          (original, _) =>
                                          {
                                              if (original == equalsToken)
                                              {
                                                  return equalsToken.WithTrailingTrivia(newEqualsTrivia);
                                              }

                                              return newOpenBracket;
                                          });
            }
        }

        // Rule 2: The value must start on the same line as the equals operator.
        // Collection expressions are already handled above.
        if (node.Value is not CollectionExpressionSyntax
            && HasTrailingEndOfLine(node.EqualsToken))
        {
            var equalsToken = node.EqualsToken;
            var valueFirstToken = node.Value.GetFirstToken();

            if (valueFirstToken != default
                && HasLeadingEndOfLine(valueFirstToken))
            {
                var newEqualsTrivia = RemoveTrailingEndOfLineTrivia(equalsToken.TrailingTrivia);

                if (newEqualsTrivia.Any(SyntaxKind.WhitespaceTrivia) == false)
                {
                    newEqualsTrivia = newEqualsTrivia.Add(SyntaxFactory.Space);
                }

                var newValueFirstToken = RemoveLeadingEndOfLineAndWhitespace(valueFirstToken);

                node = node.ReplaceTokens([equalsToken, valueFirstToken],
                                          (original, _) =>
                                          {
                                              if (original == equalsToken)
                                              {
                                                  return equalsToken.WithTrailingTrivia(newEqualsTrivia);
                                              }

                                              return newValueFirstToken;
                                          });
            }
        }

        return node;
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitVariableDeclarator(VariableDeclaratorSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (VariableDeclaratorSyntax)base.VisitVariableDeclarator(node);

        if (node == null)
        {
            return null;
        }

        if (node.Initializer == null)
        {
            return node;
        }

        // Rule 1: The equals token must be on the same line as the variable/field name.
        // Rule 2 (value on same line as =) is handled in VisitEqualsValueClause.
        var equalsToken = node.Initializer.EqualsToken;

        if (HasLeadingEndOfLine(equalsToken))
        {
            var newEqualsToken = RemoveLeadingEndOfLineAndWhitespace(equalsToken);

            if (newEqualsToken.LeadingTrivia.Any(SyntaxKind.WhitespaceTrivia) == false)
            {
                newEqualsToken = newEqualsToken.WithLeadingTrivia(newEqualsToken.LeadingTrivia.Add(SyntaxFactory.Space));
            }

            var previousToken = equalsToken.GetPreviousToken();

            if (previousToken != default
                && previousToken.IsKind(SyntaxKind.None) == false
                && HasTrailingEndOfLine(previousToken))
            {
                var newPreviousToken = previousToken.WithTrailingTrivia(RemoveTrailingEndOfLineTrivia(previousToken.TrailingTrivia));

                node = node.ReplaceTokens([previousToken, equalsToken],
                                          (original, _) =>
                                          {
                                              if (original == previousToken)
                                              {
                                                  return newPreviousToken;
                                              }

                                              return newEqualsToken;
                                          });
            }
            else
            {
                node = node.ReplaceToken(equalsToken, newEqualsToken);
            }
        }

        return node;
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (ObjectCreationExpressionSyntax)base.VisitObjectCreationExpression(node);

        if (node == null)
        {
            return null;
        }

        return CleanupTrailingWhitespaceBeforeInitializerBrace(node, node.Initializer);
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitImplicitObjectCreationExpression(ImplicitObjectCreationExpressionSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (ImplicitObjectCreationExpressionSyntax)base.VisitImplicitObjectCreationExpression(node);

        if (node == null)
        {
            return null;
        }

        return CleanupTrailingWhitespaceBeforeInitializerBrace(node, node.Initializer);
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitAnonymousObjectCreationExpression(AnonymousObjectCreationExpressionSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (AnonymousObjectCreationExpressionSyntax)base.VisitAnonymousObjectCreationExpression(node);

        if (node == null)
        {
            return null;
        }

        // For anonymous objects with multiple members, ensure each member is on its own line
        if (node.Initializers.Count > 1)
        {
            node = EnsureAnonymousObjectMembersOnSeparateLines(node);
        }

        node = EnsureBraceOnOwnLine(node, node.OpenBraceToken, (n, t) => n.WithOpenBraceToken(t), node.CloseBraceToken, (n, t) => n.WithCloseBraceToken(t));
        node = EnsureFirstContentOnNewLine(node, node.OpenBraceToken);
        node = EnsureCloseBraceContinuation(node, node.CloseBraceToken);

        return node;
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitArgumentList(ArgumentListSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (ArgumentListSyntax)base.VisitArgumentList(node);

        if (node == null)
        {
            return null;
        }

        node = CollapseFirstArgumentToSameLine(node);

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

        node = CollapseFirstBracketedArgumentToSameLine(node);

        return EnsureBracketedArgumentsOnSeparateLines(node, _context.EndOfLine);
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

        node = CollapseFirstAttributeArgumentToSameLine(node);

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

        node = CollapseFirstParameterToSameLine(node);

        return EnsureParametersOnSeparateLines(node, _context.EndOfLine);
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitBinaryExpression(BinaryExpressionSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (BinaryExpressionSyntax)base.VisitBinaryExpression(node);

        if (node == null)
        {
            return null;
        }

        return NormalizeBinaryOperatorPosition(node);
    }

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

    /// <inheritdoc/>
    public override SyntaxNode VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (ConstructorDeclarationSyntax)base.VisitConstructorDeclaration(node);

        if (node == null)
        {
            return null;
        }

        if (node.Initializer != null)
        {
            node = EnsureConstructorInitializerOnNewLine(node);
        }

        return node;
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (MethodDeclarationSyntax)base.VisitMethodDeclaration(node);

        if (node == null)
        {
            return null;
        }

        node = EnsureGenericConstraintsOnNewLines(node);

        return node;
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitDelegateDeclaration(DelegateDeclarationSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (DelegateDeclarationSyntax)base.VisitDelegateDeclaration(node);

        if (node == null)
        {
            return null;
        }

        node = EnsureGenericConstraintsOnNewLines(node);

        return node;
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitLocalFunctionStatement(LocalFunctionStatementSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (LocalFunctionStatementSyntax)base.VisitLocalFunctionStatement(node);

        if (node == null)
        {
            return null;
        }

        node = EnsureGenericConstraintsOnNewLines(node);

        return node;
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitPropertyDeclaration(PropertyDeclarationSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (PropertyDeclarationSyntax)base.VisitPropertyDeclaration(node);

        if (node == null)
        {
            return null;
        }

        if (node.ExpressionBody != null)
        {
            node = CollapseExpressionBodiedProperty(node);
        }

        return node;
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitInvocationExpression(InvocationExpressionSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        var isOutermost = IsOutermostChainInvocation(node);

        node = (InvocationExpressionSyntax)base.VisitInvocationExpression(node);

        if (node == null)
        {
            return null;
        }

        if (isOutermost)
        {
            return NormalizeChain(node);
        }

        return node;
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitConditionalAccessExpression(ConditionalAccessExpressionSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        var isOutermost = node.Parent is not ConditionalAccessExpressionSyntax;

        node = (ConditionalAccessExpressionSyntax)base.VisitConditionalAccessExpression(node);

        if (node == null)
        {
            return null;
        }

        if (isOutermost && ContainsInvocation(node.WhenNotNull))
        {
            node = (ConditionalAccessExpressionSyntax)NormalizeChain(node);

            node = CollapseMemberBindingToQuestionToken(node);
        }

        return node;
    }

    #endregion // CSharpSyntaxRewriter
}