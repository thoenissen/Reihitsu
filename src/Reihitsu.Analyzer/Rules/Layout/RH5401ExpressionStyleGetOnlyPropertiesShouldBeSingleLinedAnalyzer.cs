using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Layout;

/// <summary>
/// RH5401: Expression style get-only properties should be single lined
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH5401ExpressionStyleGetOnlyPropertiesShouldBeSingleLinedAnalyzer : DiagnosticAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH5401";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5401ExpressionStyleGetOnlyPropertiesShouldBeSingleLinedAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Layout, nameof(AnalyzerResources.RH5401Title), nameof(AnalyzerResources.RH5401MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyzing all <see cref="SyntaxKind.PropertyDeclaration"/> occurrences
    /// </summary>
    /// <param name="context">Context</param>
    private void OnPropertyDeclaration(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not PropertyDeclarationSyntax propertyDeclaration)
        {
            return;
        }

        if (propertyDeclaration.ExpressionBody is null)
        {
            return;
        }

        if (propertyDeclaration.AccessorList is not null)
        {
            return;
        }

        // The formatter only pulls the arrow and the first expression token onto the signature line; the rest
        // of the expression body (a switch-expression body, a wrapped fluent chain, a multi-line collection
        // expression) is allowed to wrap. Flag only when the expression does not start on the signature line.
        var signatureLine = propertyDeclaration.Identifier.GetLocation().GetLineSpan().StartLinePosition.Line;
        var arrowLine = propertyDeclaration.ExpressionBody.ArrowToken.GetLocation().GetLineSpan().StartLinePosition.Line;
        var expressionStartLine = propertyDeclaration.ExpressionBody.Expression.GetFirstToken().GetLocation().GetLineSpan().StartLinePosition.Line;

        if (arrowLine != signatureLine
            || expressionStartLine != signatureLine)
        {
            context.ReportDiagnostic(CreateDiagnostic(propertyDeclaration.GetLocation()));
        }
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnPropertyDeclaration, SyntaxKind.PropertyDeclaration);
    }

    #endregion // DiagnosticAnalyzer
}