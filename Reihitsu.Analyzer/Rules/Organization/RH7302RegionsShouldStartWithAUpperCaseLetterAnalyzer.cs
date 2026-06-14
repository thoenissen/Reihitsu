using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Organization;

/// <summary>
/// RH7302: The description of a #region should start with an uppercase letter
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH7302RegionsShouldStartWithAUpperCaseLetterAnalyzer : DiagnosticAnalyzerBase<RH7302RegionsShouldStartWithAUpperCaseLetterAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH7302";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH7302RegionsShouldStartWithAUpperCaseLetterAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Organization, nameof(AnalyzerResources.RH7302Title), nameof(AnalyzerResources.RH7302MessageFormat))
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
        foreach (var character in text)
        {
            if (char.IsWhiteSpace(character))
            {
                continue;
            }

            // The description is only invalid when its first non-whitespace character is a letter that has a distinct
            // uppercase form (i.e. a lowercase letter). Digits, symbols and already-uppercase letters are accepted so
            // the diagnostic never flags a shape that the code fix cannot capitalize
            return char.ToUpperInvariant(character) == character;
        }

        return true;
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