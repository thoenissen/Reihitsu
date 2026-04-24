using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// RH0346: Closing generic brackets must be spaced correctly.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0346ClosingGenericBracketsMustBeSpacedCorrectlyAnalyzer : DiagnosticAnalyzerBase<RH0346ClosingGenericBracketsMustBeSpacedCorrectlyAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0346";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0346ClosingGenericBracketsMustBeSpacedCorrectlyAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Formatting, nameof(AnalyzerResources.RH0346Title), nameof(AnalyzerResources.RH0346MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyzes the syntax tree.
    /// </summary>
    /// <param name="context">Context</param>
    private void OnSyntaxTree(SyntaxTreeAnalysisContext context)
    {
        var root = context.Tree.GetRoot(context.CancellationToken);
        var sourceText = context.Tree.GetText(context.CancellationToken);

        foreach (var node in root.DescendantNodes().OfType<TypeArgumentListSyntax>())
        {
            var token = node.GreaterThanToken;
            var start = token.SpanStart;

            while (start > 0
                   && (sourceText[start - 1] == ' ' || sourceText[start - 1] == '\t'))
            {
                start--;
            }

            if (start < token.SpanStart)
            {
                context.ReportDiagnostic(CreateDiagnostic(Location.Create(context.Tree, TextSpan.FromBounds(start, token.SpanStart))));
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