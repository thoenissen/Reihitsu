using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.Rules.Organization;

/// <summary>
/// RH7303: Regions must not be placed within elements
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH7303DoNotPlaceRegionsWithinElementsAnalyzer : DiagnosticAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH7303";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH7303DoNotPlaceRegionsWithinElementsAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Organization, nameof(AnalyzerResources.RH7303Title), nameof(AnalyzerResources.RH7303MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyzes region directives
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