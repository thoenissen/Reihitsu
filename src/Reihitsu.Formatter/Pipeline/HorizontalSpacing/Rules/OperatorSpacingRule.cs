using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Formatter.Pipeline.HorizontalSpacing.Rules;

/// <summary>
/// Requires a single space when either the current or the next token is a binary or assignment
/// operator, for example <c>a + b</c> and <c>x = 1</c>
/// </summary>
internal sealed class OperatorSpacingRule : ISpacingRule
{
    #region Methods

    /// <summary>
    /// Determines whether the specified token is a binary or assignment operator
    /// based on its parent syntax node. The combinator of a binary pattern, the <c>is</c> of a pattern match, and the <c>=</c>
    /// of a <c>let</c> clause are written like a binary operator and the <c>=</c> of a name-equals clause, so a join that
    /// places them next to their operand gets the same space (FMT-BREAK-016, FMT-BREAK-017, FMT-BREAK-041)
    /// </summary>
    /// <param name="token">The token to check</param>
    /// <returns><see langword="true"/> if the token is a binary or assignment operator; otherwise, <see langword="false"/></returns>
    private static bool IsBinaryOrAssignmentOperator(SyntaxToken token)
    {
        return token.Parent is BinaryExpressionSyntax
               || token.Parent is AssignmentExpressionSyntax
               || (token.IsKind(SyntaxKind.EqualsToken) && token.Parent is EqualsValueClauseSyntax)
               || (token.IsKind(SyntaxKind.EqualsToken) && token.Parent is NameEqualsSyntax)
               || (token.IsKind(SyntaxKind.EqualsToken) && token.Parent is LetClauseSyntax)
               || token.Parent is BinaryPatternSyntax
               || (token.IsKind(SyntaxKind.IsKeyword) && token.Parent is IsPatternExpressionSyntax);
    }

    #endregion // Methods

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
}