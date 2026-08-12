using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Design;

/// <summary>
/// RH2003: NotImplementedException should not be used
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH2003NotImplementedExceptionShouldNotBeUsedAnalyzer : DiagnosticAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH2003";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH2003NotImplementedExceptionShouldNotBeUsedAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Design, nameof(AnalyzerResources.RH2003Title), nameof(AnalyzerResources.RH2003MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Determine whether the expression creates <see cref="System.NotImplementedException"/>
    /// </summary>
    /// <param name="expression">Creation expression</param>
    /// <param name="context">Context</param>
    /// <returns><see langword="true"/> if the created type is <see cref="System.NotImplementedException"/></returns>
    private static bool IsNotImplementedException(ExpressionSyntax expression, SyntaxNodeAnalysisContext context)
    {
        return context.SemanticModel.GetTypeInfo(expression, context.CancellationToken).Type is INamedTypeSymbol typeSymbol
               && typeSymbol.ToDisplayString() == "System.NotImplementedException";
    }

    /// <summary>
    /// Gets the simple (rightmost) type name of a type syntax
    /// </summary>
    /// <param name="type">Type syntax</param>
    /// <returns>The simple type name, or <see langword="null"/> if it cannot be determined cheaply</returns>
    private static string GetSimpleTypeName(TypeSyntax type)
    {
        return type switch
               {
                   IdentifierNameSyntax identifierName => identifierName.Identifier.ValueText,
                   QualifiedNameSyntax qualifiedName => qualifiedName.Right.Identifier.ValueText,
                   AliasQualifiedNameSyntax aliasQualifiedName => aliasQualifiedName.Name.Identifier.ValueText,
                   _ => null
               };
    }

    /// <summary>
    /// Analyzing all <see cref="SyntaxKind.ObjectCreationExpression"/> nodes
    /// </summary>
    /// <param name="context">Context</param>
    private void OnObjectCreationExpression(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not ObjectCreationExpressionSyntax objectCreation)
        {
            return;
        }

        // Cheap syntactic pre-filter before the semantic binding below
        if (GetSimpleTypeName(objectCreation.Type) != "NotImplementedException")
        {
            return;
        }

        if (IsNotImplementedException(objectCreation, context))
        {
            context.ReportDiagnostic(CreateDiagnostic(objectCreation.Type.GetLocation()));
        }
    }

    /// <summary>
    /// Analyzing all <see cref="SyntaxKind.ImplicitObjectCreationExpression"/> nodes
    /// </summary>
    /// <param name="context">Context</param>
    private void OnImplicitObjectCreationExpression(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is ImplicitObjectCreationExpressionSyntax implicitObjectCreation
            && IsNotImplementedException(implicitObjectCreation, context))
        {
            context.ReportDiagnostic(CreateDiagnostic(implicitObjectCreation.NewKeyword.GetLocation()));
        }
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnObjectCreationExpression, SyntaxKind.ObjectCreationExpression);
        context.RegisterSyntaxNodeAction(OnImplicitObjectCreationExpression, SyntaxKind.ImplicitObjectCreationExpression);
    }

    #endregion // DiagnosticAnalyzer
}