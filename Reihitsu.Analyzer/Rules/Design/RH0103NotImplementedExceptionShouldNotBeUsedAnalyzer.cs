using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Design;

/// <summary>
/// RH0103: NotImplementedException should not be used
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0103NotImplementedExceptionShouldNotBeUsedAnalyzer : DiagnosticAnalyzerBase<RH0103NotImplementedExceptionShouldNotBeUsedAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0103";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0103NotImplementedExceptionShouldNotBeUsedAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Design, nameof(AnalyzerResources.RH0103Title), nameof(AnalyzerResources.RH0103MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

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