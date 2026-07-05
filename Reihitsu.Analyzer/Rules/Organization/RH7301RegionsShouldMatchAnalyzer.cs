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
    /// Checks if the region name is valid
    /// </summary>
    /// <param name="text">Text of the region</param>
    /// <returns>Is the name valid?</returns>
    private static bool IsRegionNameValid(string text)
    {
        return text.Length > 4
               && text.StartsWith(" // ");
    }

    /// <summary>
    /// Analyzing all <see cref="SyntaxKind.LogicalNotExpression"/> occurrences
    /// </summary>
    /// <param name="context">Context</param>
    private void OnEndRegion(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is EndRegionDirectiveTriviaSyntax node)
        {
            var endText = node.ParentTrivia.ToString().Substring(10);

            if (IsRegionNameValid(endText) == false)
            {
                context.ReportDiagnostic(CreateDiagnostic(node.GetLocation()));
            }
            else
            {
                CheckRegionPair(context, node, endText);
            }
        }
    }

    /// <summary>
    /// Check if the region pair has a matching text
    /// </summary>
    /// <param name="context">Context</param>
    /// <param name="node">#endregion node</param>
    /// <param name="text">Region text</param>
    private void CheckRegionPair(SyntaxNodeAnalysisContext context, EndRegionDirectiveTriviaSyntax node, string text)
    {
        var syntaxRoot = context.Node.SyntaxTree.GetRoot(context.CancellationToken);

        if (RegionDirectiveUtilities.TryFindMatchingDirective(syntaxRoot, node.ParentTrivia, out var regionTrivia))
        {
            var startText = regionTrivia.ToString();

            if (startText.Length >= 8)
            {
                startText = startText.Substring(8);

                if (text.Substring(4) != startText)
                {
                    context.ReportDiagnostic(CreateDiagnostic(node.GetLocation()));
                }
            }
        }
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnEndRegion, SyntaxKind.EndRegionDirectiveTrivia);
    }

    #endregion // DiagnosticAnalyzer
}