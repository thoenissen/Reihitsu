using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Formatter.Rules.Structural;

/// <summary>
/// Places block and initializer braces on dedicated lines.
/// </summary>
internal sealed class BracePlacementRule : FormattingRuleBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="context">The formatting context.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public BracePlacementRule(FormattingContext context, CancellationToken cancellationToken)
        : base(context, cancellationToken)
    {
    }

    #endregion // Constructor

    #region FormattingRuleBase

    /// <inheritdoc/>
    public override SyntaxToken VisitToken(SyntaxToken token)
    {
        token = base.VisitToken(token);

        if (token.IsKind(SyntaxKind.None))
        {
            return token;
        }

        if (ShouldFormatOpenBrace(token))
        {
            token = EnsureLineBreakBeforeToken(token);
            token = EnsureLineBreakAfterToken(token);

            return token;
        }

        if (ShouldFormatCloseBrace(token))
        {
            token = EnsureLineBreakBeforeToken(token);

            if (ShouldForceLineBreakAfterCloseBrace(token))
            {
                token = EnsureLineBreakAfterToken(token);
            }
        }

        return token;
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
    {
        var visited = (ClassDeclarationSyntax)base.VisitClassDeclaration(node);

        if (visited == null || visited.Members.Count == 0)
        {
            return visited;
        }

        return EnsureTokenStartsOnNewLineAfterToken(visited, visited.OpenBraceToken, visited.Members[0].GetFirstToken());
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitBlock(BlockSyntax node)
    {
        var visited = (BlockSyntax)base.VisitBlock(node);

        if (visited == null || visited.Statements.Count == 0)
        {
            return visited;
        }

        return EnsureTokenStartsOnNewLineAfterToken(visited, visited.OpenBraceToken, visited.Statements[0].GetFirstToken());
    }

    #endregion // FormattingRuleBase

    #region IFormattingRule

    /// <inheritdoc/>
    public override FormattingPhase Phase => FormattingPhase.StructuralTransform;

    #endregion // IFormattingRule

    #region Methods

    /// <summary>
    /// Determines whether an open brace token should be placed on a dedicated line.
    /// </summary>
    /// <param name="token">The token to inspect.</param>
    /// <returns><c>true</c> if the open brace should be moved to a new line; otherwise, <c>false</c>.</returns>
    private static bool ShouldFormatOpenBrace(SyntaxToken token)
    {
        if (token.IsKind(SyntaxKind.OpenBraceToken) == false)
        {
            return false;
        }

        if (token.Parent is AccessorListSyntax accessorList)
        {
            return IsAutoPropertyAccessorList(accessorList) == false;
        }

        if (token.Parent is InitializerExpressionSyntax initializer)
        {
            return IsSupportedInitializer(initializer);
        }

        return token.Parent is BlockSyntax
                            or TypeDeclarationSyntax
                            or NamespaceDeclarationSyntax
                            or SwitchStatementSyntax
                            or AnonymousObjectCreationExpressionSyntax;
    }

    /// <summary>
    /// Determines whether a close brace token should be placed on a dedicated line.
    /// </summary>
    /// <param name="token">The token to inspect.</param>
    /// <returns><c>true</c> if the close brace should be moved to a new line; otherwise, <c>false</c>.</returns>
    private static bool ShouldFormatCloseBrace(SyntaxToken token)
    {
        if (token.IsKind(SyntaxKind.CloseBraceToken) == false)
        {
            return false;
        }

        if (token.Parent is AccessorListSyntax accessorList)
        {
            return IsAutoPropertyAccessorList(accessorList) == false;
        }

        if (token.Parent is InitializerExpressionSyntax initializer)
        {
            return IsSupportedInitializer(initializer);
        }

        return token.Parent is BlockSyntax
                            or TypeDeclarationSyntax
                            or NamespaceDeclarationSyntax
                            or SwitchStatementSyntax
                            or AnonymousObjectCreationExpressionSyntax;
    }

    /// <summary>
    /// Determines whether an initializer is supported for brace line-break formatting.
    /// </summary>
    /// <param name="initializer">The initializer expression to inspect.</param>
    /// <returns><c>true</c> if the initializer braces should be formatted; otherwise, <c>false</c>.</returns>
    private static bool IsSupportedInitializer(InitializerExpressionSyntax initializer)
    {
        return initializer.IsKind(SyntaxKind.ObjectInitializerExpression)
               || initializer.IsKind(SyntaxKind.CollectionInitializerExpression);
    }

    /// <summary>
    /// Determines whether an accessor list belongs to an auto-property.
    /// </summary>
    /// <param name="accessorList">The accessor list.</param>
    /// <returns><c>true</c> if the accessor list belongs to an auto-property; otherwise, <c>false</c>.</returns>
    private static bool IsAutoPropertyAccessorList(AccessorListSyntax accessorList)
    {
        if (accessorList.Parent is PropertyDeclarationSyntax == false)
        {
            return false;
        }

        if (accessorList.Accessors.Count == 0)
        {
            return false;
        }

        foreach (var accessor in accessorList.Accessors)
        {
            if (accessor.Body != null || accessor.ExpressionBody != null || accessor.SemicolonToken.IsKind(SyntaxKind.None))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Ensures that the token starts on a new line.
    /// </summary>
    /// <param name="token">The token to update.</param>
    /// <returns>The token with an end-of-line before it when required.</returns>
    private SyntaxToken EnsureLineBreakBeforeToken(SyntaxToken token)
    {
        var previousToken = token.GetPreviousToken();

        if (previousToken.IsKind(SyntaxKind.None))
        {
            return token;
        }

        if (ContainsEndOfLine(token.LeadingTrivia) || ContainsEndOfLine(previousToken.TrailingTrivia))
        {
            return token;
        }

        var leading = RemoveWhitespaceTrivia(token.LeadingTrivia);
        var result = new List<SyntaxTrivia>
                     {
                         SyntaxFactory.EndOfLine(Context.EndOfLine)
                     };

        result.AddRange(leading);

        return token.WithLeadingTrivia(SyntaxFactory.TriviaList(result));
    }

    /// <summary>
    /// Ensures that the token is followed by a new line.
    /// </summary>
    /// <param name="token">The token to update.</param>
    /// <returns>The token with an end-of-line after it when required.</returns>
    private SyntaxToken EnsureLineBreakAfterToken(SyntaxToken token)
    {
        var nextToken = token.GetNextToken();

        if (nextToken.IsKind(SyntaxKind.None))
        {
            return token;
        }

        if (ContainsEndOfLine(token.TrailingTrivia) || ContainsEndOfLine(nextToken.LeadingTrivia))
        {
            return token;
        }

        var trailing = RemoveWhitespaceTrivia(token.TrailingTrivia);
        var result = new List<SyntaxTrivia>
                     {
                         SyntaxFactory.EndOfLine(Context.EndOfLine)
                     };

        result.AddRange(trailing);

        return token.WithTrailingTrivia(SyntaxFactory.TriviaList(result));
    }

    /// <summary>
    /// Determines whether a close brace should force the next token to move to a new line.
    /// </summary>
    /// <param name="token">The close brace token.</param>
    /// <returns><c>true</c> when a line break should be inserted after the close brace; otherwise, <c>false</c>.</returns>
    private bool ShouldForceLineBreakAfterCloseBrace(SyntaxToken token)
    {
        var nextToken = token.GetNextToken();

        if (nextToken.IsKind(SyntaxKind.None))
        {
            return false;
        }

        return nextToken.IsKind(SyntaxKind.SemicolonToken) == false
               && nextToken.IsKind(SyntaxKind.CommaToken) == false
               && nextToken.IsKind(SyntaxKind.CloseParenToken) == false;
    }

    /// <summary>
    /// Determines whether a trivia list contains an end-of-line trivia.
    /// </summary>
    /// <param name="triviaList">The trivia list.</param>
    /// <returns><c>true</c> when an end-of-line is present; otherwise, <c>false</c>.</returns>
    private bool ContainsEndOfLine(SyntaxTriviaList triviaList)
    {
        foreach (var trivia in triviaList)
        {
            if (trivia.IsKind(SyntaxKind.EndOfLineTrivia))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Removes whitespace trivia entries from a trivia list.
    /// </summary>
    /// <param name="triviaList">The trivia list to clean.</param>
    /// <returns>A trivia list without whitespace trivia.</returns>
    private SyntaxTriviaList RemoveWhitespaceTrivia(SyntaxTriviaList triviaList)
    {
        var result = new List<SyntaxTrivia>(triviaList.Count);

        foreach (var trivia in triviaList)
        {
            if (trivia.IsKind(SyntaxKind.WhitespaceTrivia) == false)
            {
                result.Add(trivia);
            }
        }

        return SyntaxFactory.TriviaList(result);
    }

    /// <summary>
    /// Ensures that a token starts on a new line after an anchor token within the same node.
    /// </summary>
    /// <typeparam name="T">The node type.</typeparam>
    /// <param name="node">The node containing the tokens.</param>
    /// <param name="anchorToken">The preceding anchor token.</param>
    /// <param name="tokenToMove">The token that should start on a new line.</param>
    /// <returns>The updated node.</returns>
    private SyntaxNode EnsureTokenStartsOnNewLineAfterToken<T>(T node, SyntaxToken anchorToken, SyntaxToken tokenToMove)
        where T : SyntaxNode
    {
        if (ContainsEndOfLine(anchorToken.TrailingTrivia) || ContainsEndOfLine(tokenToMove.LeadingTrivia))
        {
            return node;
        }

        var newLeading = new List<SyntaxTrivia>
                         {
                             SyntaxFactory.EndOfLine(Context.EndOfLine)
                         };

        foreach (var trivia in tokenToMove.LeadingTrivia)
        {
            if (trivia.IsKind(SyntaxKind.WhitespaceTrivia) == false)
            {
                newLeading.Add(trivia);
            }
        }

        var updatedToken = tokenToMove.WithLeadingTrivia(SyntaxFactory.TriviaList(newLeading));

        return node.ReplaceToken(tokenToMove, updatedToken);
    }

    #endregion // Methods
}