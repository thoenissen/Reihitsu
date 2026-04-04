using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Formatter.Rules.Structural;

/// <summary>
/// Converts expression-bodied constructors to block-bodied constructors.
/// </summary>
internal sealed class ExpressionBodiedConstructorRule : FormattingRuleBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="context">The formatting context.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public ExpressionBodiedConstructorRule(FormattingContext context, CancellationToken cancellationToken)
        : base(context, cancellationToken)
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Converts an expression-bodied constructor declaration to a block-bodied constructor declaration.
    /// </summary>
    /// <param name="constructorDeclaration">The expression-bodied constructor declaration to convert.</param>
    /// <returns>The equivalent block-bodied constructor declaration.</returns>
    private ConstructorDeclarationSyntax ConvertToBlockBody(ConstructorDeclarationSyntax constructorDeclaration)
    {
        var expression = constructorDeclaration.ExpressionBody!.Expression;
        var endOfLine = Context.EndOfLine;
        var semicolonTrailingTrivia = constructorDeclaration.SemicolonToken.TrailingTrivia;

        var statement = SyntaxFactory.ExpressionStatement(expression)
                                     .WithLeadingTrivia(SyntaxFactory.EndOfLine(endOfLine));

        var openBrace = SyntaxFactory.Token(SyntaxKind.OpenBraceToken)
                                     .WithLeadingTrivia(SyntaxFactory.EndOfLine(endOfLine))
                                     .WithTrailingTrivia(SyntaxTriviaList.Empty);

        var closeBrace = SyntaxFactory.Token(SyntaxKind.CloseBraceToken)
                                      .WithLeadingTrivia(SyntaxFactory.EndOfLine(endOfLine))
                                      .WithTrailingTrivia(semicolonTrailingTrivia);

        var block = SyntaxFactory.Block(openBrace, SyntaxFactory.SingletonList<StatementSyntax>(statement), closeBrace);

        return constructorDeclaration.WithExpressionBody(null)
                                     .WithSemicolonToken(default)
                                     .WithBody(block);
    }

    #endregion // Methods

    #region FormattingRuleBase

    /// <inheritdoc/>
    public override SyntaxNode VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
    {
        var visited = (ConstructorDeclarationSyntax)base.VisitConstructorDeclaration(node);

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