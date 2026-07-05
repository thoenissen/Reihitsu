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

        if (context.SemanticModel.GetTypeInfo(objectCreation).Type is not INamedTypeSymbol typeSymbol)
        {
            return;
        }

        if (typeSymbol.ToDisplayString() == "System.NotImplementedException")
        {
            context.ReportDiagnostic(CreateDiagnostic(objectCreation.Type.GetLocation()));
        }
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnObjectCreationExpression, SyntaxKind.ObjectCreationExpression);
    }

    #endregion // DiagnosticAnalyzer
}