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
public class RH5401ExpressionStyleGetOnlyPropertiesShouldBeSingleLinedAnalyzer : DiagnosticAnalyzerBase<RH5401ExpressionStyleGetOnlyPropertiesShouldBeSingleLinedAnalyzer>
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

        var lineSpan = propertyDeclaration.SyntaxTree.GetLineSpan(propertyDeclaration.Span);

        if (lineSpan.StartLinePosition.Line != lineSpan.EndLinePosition.Line)
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