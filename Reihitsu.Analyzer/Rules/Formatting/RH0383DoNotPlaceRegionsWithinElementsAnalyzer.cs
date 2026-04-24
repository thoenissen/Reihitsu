using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// RH0383: Regions must not be placed within elements.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0383DoNotPlaceRegionsWithinElementsAnalyzer : DiagnosticAnalyzerBase<RH0383DoNotPlaceRegionsWithinElementsAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0383";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0383DoNotPlaceRegionsWithinElementsAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Formatting, nameof(AnalyzerResources.RH0383Title), nameof(AnalyzerResources.RH0383MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyzes region directives.
    /// </summary>
    /// <param name="context">Context</param>
    private void OnSyntaxTree(SyntaxTreeAnalysisContext context)
    {
        var syntaxRoot = context.Tree.GetRoot(context.CancellationToken);

        foreach (var directiveTrivia in syntaxRoot.DescendantTrivia(descendIntoTrivia: true))
        {
            if ((directiveTrivia.IsKind(SyntaxKind.RegionDirectiveTrivia) || directiveTrivia.IsKind(SyntaxKind.EndRegionDirectiveTrivia))
                && RegionDirectiveUtilities.IsWithinElementBody(directiveTrivia))
            {
                context.ReportDiagnostic(CreateDiagnostic(directiveTrivia.GetLocation()));
            }
        }
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxTreeAction(OnSyntaxTree);
    }

    #endregion // DiagnosticAnalyzer
}