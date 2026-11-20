using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.Base;

namespace Reihitsu.Analyzer.Rules.Design;

/// <summary>
/// RH2008: Methods should not exceed the line limit
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH2008MethodsShouldNotExceedTheLineLimitAnalyzer : LineCountAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH2008";

    /// <summary>
    /// Default limit of a member's body in production code
    /// </summary>
    private const int DefaultProductionLimit = 60;

    /// <summary>
    /// Default limit of a member's body in test code
    /// </summary>
    private const int DefaultTestLimit = 120;

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH2008MethodsShouldNotExceedTheLineLimitAnalyzer()
        : base(DiagnosticId,
               design => design.MaximumMethodLines,
               DefaultProductionLimit,
               design => design.MaximumTestMethodLines,
               DefaultTestLimit,
               SyntaxKind.MethodDeclaration,
               SyntaxKind.ConstructorDeclaration,
               SyntaxKind.DestructorDeclaration,
               SyntaxKind.OperatorDeclaration,
               SyntaxKind.ConversionOperatorDeclaration,
               SyntaxKind.LocalFunctionStatement,
               SyntaxKind.GetAccessorDeclaration,
               SyntaxKind.SetAccessorDeclaration,
               SyntaxKind.InitAccessorDeclaration,
               SyntaxKind.AddAccessorDeclaration,
               SyntaxKind.RemoveAccessorDeclaration,
               SyntaxKind.PropertyDeclaration,
               SyntaxKind.IndexerDeclaration)
    {
    }

    #endregion // Constructor

    #region LineCountAnalyzerBase

    /// <inheritdoc/>
    private protected override bool TryGetCountedSpan(SyntaxNode node, out TextSpan span, out Location location)
    {
        var (body, expressionBody, nameToken) = node switch
                                                {
                                                    MethodDeclarationSyntax method => (method.Body, method.ExpressionBody, method.Identifier),
                                                    ConstructorDeclarationSyntax constructor => (constructor.Body, constructor.ExpressionBody, constructor.Identifier),
                                                    DestructorDeclarationSyntax destructor => (destructor.Body, destructor.ExpressionBody, destructor.Identifier),
                                                    OperatorDeclarationSyntax operatorDeclaration => (operatorDeclaration.Body, operatorDeclaration.ExpressionBody, operatorDeclaration.OperatorToken),
                                                    ConversionOperatorDeclarationSyntax conversion => (conversion.Body, conversion.ExpressionBody, conversion.OperatorKeyword),
                                                    LocalFunctionStatementSyntax localFunction => (localFunction.Body, localFunction.ExpressionBody, localFunction.Identifier),
                                                    AccessorDeclarationSyntax accessor => (accessor.Body, accessor.ExpressionBody, accessor.Keyword),
                                                    PropertyDeclarationSyntax property => (null, property.ExpressionBody, property.Identifier),
                                                    IndexerDeclarationSyntax indexer => (null, indexer.ExpressionBody, indexer.ThisKeyword),
                                                    _ => ((BlockSyntax)null, (ArrowExpressionClauseSyntax)null, default(SyntaxToken))
                                                };

        if (TryGetBodySpan(body, expressionBody, out span))
        {
            location = nameToken.GetLocation();

            return true;
        }

        location = null;

        return false;
    }

    #endregion // LineCountAnalyzerBase
}