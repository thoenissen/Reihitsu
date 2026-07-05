using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.Rules.Clarity;

/// <summary>
/// RH3204: Interpolated strings without interpolation should not use the dollar marker
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH3204InterpolatedStringsWithoutInterpolationShouldNotUseDollarAnalyzer : DiagnosticAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH3204";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH3204InterpolatedStringsWithoutInterpolationShouldNotUseDollarAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Clarity, nameof(AnalyzerResources.RH3204Title), nameof(AnalyzerResources.RH3204MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyzes interpolated string expressions for unnecessary interpolation markers
    /// </summary>
    /// <param name="context">Context</param>
    private void OnInterpolatedStringExpression(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not InterpolatedStringExpressionSyntax interpolatedString)
        {
            return;
        }

        if (StringInterpolationUtilities.HasInterpolations(interpolatedString) == false)
        {
            context.ReportDiagnostic(CreateDiagnostic(interpolatedString.GetLocation()));
        }
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnInterpolatedStringExpression, SyntaxKind.InterpolatedStringExpression);
    }

    #endregion // DiagnosticAnalyzer
}