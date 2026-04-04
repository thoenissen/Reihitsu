using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Formatter.Rules.Structural;

/// <summary>
/// Converts expression-bodied methods to block-bodied methods.
/// </summary>
internal sealed class ExpressionBodiedMethodRule : FormattingRuleBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="context">The formatting context.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public ExpressionBodiedMethodRule(FormattingContext context, CancellationToken cancellationToken)
        : base(context, cancellationToken)
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Converts an expression-bodied method declaration to a block-bodied method declaration.
    /// </summary>
    /// <param name="methodDeclaration">The expression-bodied method declaration to convert.</param>
    /// <returns>The equivalent block-bodied method declaration.</returns>
    private MethodDeclarationSyntax ConvertToBlockBody(MethodDeclarationSyntax methodDeclaration)
    {
        var expression = methodDeclaration.ExpressionBody!.Expression;
        var endOfLine = Context.EndOfLine;
        var semicolonTrailingTrivia = methodDeclaration.SemicolonToken.TrailingTrivia;

        var isVoid = methodDeclaration.ReturnType is PredefinedTypeSyntax predefined
                     && predefined.Keyword.IsKind(SyntaxKind.VoidKeyword);

        StatementSyntax statement;

        if (isVoid)
        {
            statement = SyntaxFactory.ExpressionStatement(expression);
        }
        else
        {
            statement = SyntaxFactory.ReturnStatement(expression);
        }

        statement = statement.WithLeadingTrivia(SyntaxFactory.EndOfLine(endOfLine));

        var openBrace = SyntaxFactory.Token(SyntaxKind.OpenBraceToken)
                                     .WithLeadingTrivia(SyntaxFactory.EndOfLine(endOfLine))
                                     .WithTrailingTrivia(SyntaxTriviaList.Empty);

        var closeBrace = SyntaxFactory.Token(SyntaxKind.CloseBraceToken)
                                      .WithLeadingTrivia(SyntaxFactory.EndOfLine(endOfLine))
                                      .WithTrailingTrivia(semicolonTrailingTrivia);

        var block = SyntaxFactory.Block(openBrace, SyntaxFactory.SingletonList(statement), closeBrace);

        return methodDeclaration.WithExpressionBody(null)
                                .WithSemicolonToken(default)
                                .WithBody(block);
    }

    #endregion // Methods

    #region FormattingRuleBase

    /// <inheritdoc/>
    public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
        var visited = (MethodDeclarationSyntax)base.VisitMethodDeclaration(node);

        if (visited?.ExpressionBody == null)
        {
            return visited;
        }

        return ConvertToBlockBody(visited);
    }

    #endregion // FormattingRuleBase

    #region IFormattingRule

    /// <inheritdoc/>
    public override FormattingPhase Phase => FormattingPhase.StructuralTransform;

    #endregion // IFormattingRule
}