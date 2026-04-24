using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// RH0355: Colons must be spaced correctly.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0355ColonsMustBeSpacedCorrectlyAnalyzer : DiagnosticAnalyzerBase<RH0355ColonsMustBeSpacedCorrectlyAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0355";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0355ColonsMustBeSpacedCorrectlyAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Formatting, nameof(AnalyzerResources.RH0355Title), nameof(AnalyzerResources.RH0355MessageFormat))
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

        foreach (var node in root.DescendantNodes().OfType<BaseListSyntax>())
        {
            var token = node.ColonToken;
            var hasLeadingSpace = token.SpanStart > 0 && sourceText[token.SpanStart - 1] == ' ';
            var hasTrailingSpace = token.Span.End < sourceText.Length && sourceText[token.Span.End] == ' ';

            if (hasLeadingSpace == false || hasTrailingSpace == false)
            {
                context.ReportDiagnostic(CreateDiagnostic(token.GetLocation()));
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