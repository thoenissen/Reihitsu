using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// RH0344: Closing braces must be spaced correctly.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0344ClosingBracesMustBeSpacedCorrectlyAnalyzer : DiagnosticAnalyzerBase<RH0344ClosingBracesMustBeSpacedCorrectlyAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0344";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0344ClosingBracesMustBeSpacedCorrectlyAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Formatting, nameof(AnalyzerResources.RH0344Title), nameof(AnalyzerResources.RH0344MessageFormat))
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

        foreach (var token in root.DescendantTokens().Where(currentToken => currentToken.IsKind(SyntaxKind.CloseBraceToken)))
        {
            if (token.SpanStart == 0
                || char.IsWhiteSpace(sourceText[token.SpanStart - 1]))
            {
                continue;
            }

            context.ReportDiagnostic(CreateDiagnostic(token.GetLocation()));
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