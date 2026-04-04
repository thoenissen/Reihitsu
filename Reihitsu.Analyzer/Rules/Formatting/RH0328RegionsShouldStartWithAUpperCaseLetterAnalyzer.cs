using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// RH0328: The description of a #region should start with an uppercase letter.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0328RegionsShouldStartWithAUpperCaseLetterAnalyzer : DiagnosticAnalyzerBase<RH0328RegionsShouldStartWithAUpperCaseLetterAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0328";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0328RegionsShouldStartWithAUpperCaseLetterAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Formatting, nameof(AnalyzerResources.RH0328Title), nameof(AnalyzerResources.RH0328MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Checks if the region name is valid
    /// </summary>
    /// <param name="text">Text of the region</param>
    /// <returns>Is the name valid?</returns>
    private static bool IsRegionNameValid(ReadOnlySpan<char> text)
    {
        return text.Length <= 1
               || char.IsUpper(text[1]);
    }

    /// <summary>
    /// Analyzing all <see cref="SyntaxKind.LogicalNotExpression"/> occurrences
    /// </summary>
    /// <param name="context">Context</param>
    private void OnStartRegion(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is RegionDirectiveTriviaSyntax node)
        {
            var endText = node.ParentTrivia.ToString().AsSpan().Slice(7);

            if (IsRegionNameValid(endText) == false)
            {
                context.ReportDiagnostic(CreateDiagnostic(node.GetLocation()));
            }
        }
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnStartRegion, SyntaxKind.RegionDirectiveTrivia);
    }

    #endregion // DiagnosticAnalyzer
}