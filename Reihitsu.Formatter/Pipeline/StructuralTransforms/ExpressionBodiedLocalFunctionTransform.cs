using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Formatter.Pipeline.StructuralTransforms;

/// <summary>
/// Converts expression-bodied local functions to block body.
/// Value-returning functions wrap the expression in a <see cref="ReturnStatementSyntax"/>.
/// Void and non-generic async task functions wrap it in an <see cref="ExpressionStatementSyntax"/>
/// </summary>
internal sealed class ExpressionBodiedLocalFunctionTransform : CSharpSyntaxRewriter
{
    #region Fields

    /// <summary>
    /// The cancellation token
    /// </summary>
    private readonly CancellationToken _cancellationToken;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    public ExpressionBodiedLocalFunctionTransform(CancellationToken cancellationToken)
    {
        _cancellationToken = cancellationToken;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Determines whether the given local function should use an expression statement when converting to a block body
    /// </summary>
    /// <param name="returnType">The return type syntax to check</param>
    /// <param name="modifiers">The local function modifiers</param>
    /// <returns><see langword="true"/> if the converted body should use an expression statement; otherwise, <see langword="false"/></returns>
    private static bool UsesExpressionStatement(TypeSyntax returnType, SyntaxTokenList modifiers)
    {
        if (returnType is PredefinedTypeSyntax predefined
            && predefined.Keyword.IsKind(SyntaxKind.VoidKeyword))
        {
            return true;
        }

        return HasAsyncModifier(modifiers) && IsNonGenericTaskReturnType(returnType);
    }

    /// <summary>
    /// Determines whether the provided modifiers include <see langword="async"/>
    /// </summary>
    /// <param name="modifiers">The modifiers to inspect</param>
    /// <returns><see langword="true"/> if an async modifier is present; otherwise, <see langword="false"/></returns>
    private static bool HasAsyncModifier(SyntaxTokenList modifiers)
    {
        foreach (var modifier in modifiers)
        {
            if (modifier.IsKind(SyntaxKind.AsyncKeyword))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Determines whether the given return type represents a non-generic task
    /// </summary>
    /// <param name="returnType">The return type syntax to check</param>
    /// <returns><see langword="true"/> if the return type is a non-generic task; otherwise, <see langword="false"/></returns>
    private static bool IsNonGenericTaskReturnType(TypeSyntax returnType)
    {
        return returnType switch
               {
                   IdentifierNameSyntax identifier => identifier.Identifier.ValueText == "Task",
                   QualifiedNameSyntax qualified => qualified.Right.Identifier.ValueText == "Task" && qualified.Right is GenericNameSyntax == false,
                   AliasQualifiedNameSyntax aliasQualified => aliasQualified.Name.Identifier.ValueText == "Task" && aliasQualified.Name is GenericNameSyntax == false,
                   _ => false,
               };
    }

    #endregion // Methods

    #region CSharpSyntaxRewriter

    /// <inheritdoc/>
    public override SyntaxNode VisitLocalFunctionStatement(LocalFunctionStatementSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (LocalFunctionStatementSyntax)base.VisitLocalFunctionStatement(node);

        if (node?.ExpressionBody == null)
        {
            return node;
        }

        var expression = node.ExpressionBody.Expression;
        var useExpressionStatement = UsesExpressionStatement(node.ReturnType, node.Modifiers);

        StatementSyntax statement;

        if (useExpressionStatement)
        {
            statement = SyntaxFactory.ExpressionStatement(expression);
        }
        else
        {
            statement = SyntaxFactory.ReturnStatement(SyntaxFactory.Token(SyntaxKind.ReturnKeyword),
                                                      expression,
                                                      SyntaxFactory.Token(SyntaxKind.SemicolonToken));
        }

        var openBraceTrivia = node.ExpressionBody.ArrowToken.LeadingTrivia;
        var closeBraceTrivia = node.SemicolonToken.TrailingTrivia;

        var block = SyntaxFactory.Block(SyntaxFactory.Token(SyntaxKind.OpenBraceToken).WithLeadingTrivia(openBraceTrivia),
                                        SyntaxFactory.SingletonList(statement),
                                        SyntaxFactory.Token(SyntaxKind.CloseBraceToken).WithTrailingTrivia(closeBraceTrivia));

        return node.WithBody(block)
                   .WithExpressionBody(null)
                   .WithSemicolonToken(default);
    }

    #endregion // CSharpSyntaxRewriter
}