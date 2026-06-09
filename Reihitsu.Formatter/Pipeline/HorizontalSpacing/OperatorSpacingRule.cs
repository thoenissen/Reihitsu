using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Formatter.Pipeline.HorizontalSpacing;

/// <summary>
/// Requires a single space when either the current or the next token is a binary or assignment
/// operator, for example <c>a + b</c> and <c>x = 1</c>
/// </summary>
internal sealed class OperatorSpacingRule : ISpacingRule
{
    #region ISpacingRule

    /// <inheritdoc/>
    public int? DesiredSpacesAfter(SyntaxToken left, SyntaxToken right)
    {
        if (IsBinaryOrAssignmentOperator(left) || IsBinaryOrAssignmentOperator(right))
        {
            return 1;
        }

        return null;
    }

    #endregion // ISpacingRule

    #region Methods

    /// <summary>
    /// Determines whether the specified token is a binary or assignment operator
    /// based on its parent syntax node
    /// </summary>
    /// <param name="token">The token to check</param>
    /// <returns><see langword="true"/> if the token is a binary or assignment operator; otherwise, <see langword="false"/></returns>
    private static bool IsBinaryOrAssignmentOperator(SyntaxToken token)
    {
        return token.Parent is BinaryExpressionSyntax
               || token.Parent is AssignmentExpressionSyntax
               || (token.IsKind(SyntaxKind.EqualsToken) && token.Parent is EqualsValueClauseSyntax)
               || (token.IsKind(SyntaxKind.EqualsToken) && token.Parent is NameEqualsSyntax);
    }

    #endregion // Methods
}