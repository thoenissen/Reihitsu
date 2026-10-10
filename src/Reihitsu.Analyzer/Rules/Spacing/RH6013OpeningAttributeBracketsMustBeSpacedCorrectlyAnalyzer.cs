using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Spacing;

/// <summary>
/// RH6013: Opening attribute brackets must be spaced correctly
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH6013OpeningAttributeBracketsMustBeSpacedCorrectlyAnalyzer : DiagnosticAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH6013";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH6013OpeningAttributeBracketsMustBeSpacedCorrectlyAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Spacing, nameof(AnalyzerResources.RH6013Title), nameof(AnalyzerResources.RH6013MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyzes an attribute list
    /// </summary>
    /// <param name="context">Context</param>
    private void OnSyntaxNode(SyntaxNodeAnalysisContext context)
    {
        var token = ((AttributeListSyntax)context.Node).OpenBracketToken;
        var sourceText = context.Node.SyntaxTree.GetText(context.CancellationToken);
        var start = token.Span.End;
        var end = start;

        while (end < sourceText.Length
               && (sourceText[end] == ' ' || sourceText[end] == '\t'))
        {
            end++;
        }

        if (end > start)
        {
            context.ReportDiagnostic(CreateDiagnostic(Location.Create(context.Node.SyntaxTree, TextSpan.FromBounds(start, end))));
        }
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnSyntaxNode, SyntaxKind.AttributeList);
    }

    #endregion // DiagnosticAnalyzer
}