using System;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.Rules.Organization;

/// <summary>
/// RH7301: The description of the #region and #endregion should match
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH7301RegionsShouldMatchAnalyzer : DiagnosticAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH7301";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH7301RegionsShouldMatchAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Organization, nameof(AnalyzerResources.RH7301Title), nameof(AnalyzerResources.RH7301MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Reports a diagnostic when the normalized descriptions of a matched region pair differ
    /// </summary>
    /// <param name="context">Context</param>
    /// <param name="regionTrivia">Opening region trivia</param>
    /// <param name="endRegionTrivia">Closing endregion trivia</param>
    private void AnalyzeRegionPair(SyntaxTreeAnalysisContext context, SyntaxTrivia regionTrivia, SyntaxTrivia endRegionTrivia)
    {
        if (regionTrivia.GetStructure() is RegionDirectiveTriviaSyntax regionDirective
            && endRegionTrivia.GetStructure() is EndRegionDirectiveTriviaSyntax endRegionDirective
            && string.Equals(RegionDirectiveUtilities.GetRegionDescription(regionDirective),
                             RegionDirectiveUtilities.GetEndRegionDescription(endRegionDirective),
                             StringComparison.Ordinal) == false)
        {
            context.ReportDiagnostic(CreateDiagnostic(endRegionTrivia.GetLocation()));
        }
    }

    /// <summary>
    /// Analyzes matched region and endregion directive pairs in closing-directive traversal order
    /// </summary>
    /// <param name="context">Context</param>
    private void OnSyntaxTree(SyntaxTreeAnalysisContext context)
    {
        var syntaxRoot = context.Tree.GetRoot(context.CancellationToken);

        foreach (var (regionTrivia, endRegionTrivia) in RegionDirectiveUtilities.GetRegionPairs(syntaxRoot, includeDirective: null))
        {
            AnalyzeRegionPair(context, regionTrivia, endRegionTrivia);
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