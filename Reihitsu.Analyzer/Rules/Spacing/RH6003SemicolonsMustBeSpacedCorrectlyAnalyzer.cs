using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Spacing;

/// <summary>
/// RH6003: Semicolons must be spaced correctly
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH6003SemicolonsMustBeSpacedCorrectlyAnalyzer : DiagnosticAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH6003";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH6003SemicolonsMustBeSpacedCorrectlyAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Spacing, nameof(AnalyzerResources.RH6003Title), nameof(AnalyzerResources.RH6003MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyzes the syntax tree
    /// </summary>
    /// <param name="context">Context</param>
    private void OnSyntaxTree(SyntaxTreeAnalysisContext context)
    {
        var root = context.Tree.GetRoot(context.CancellationToken);
        var sourceText = context.Tree.GetText(context.CancellationToken);

        foreach (var tokenSpanStart in root.DescendantTokens()
                                           .Where(currentToken => currentToken.IsKind(SyntaxKind.SemicolonToken))
                                           .Select(token => token.SpanStart))
        {
            if (SameLinePrecedingWhitespaceAnalysis.GetSpan(sourceText, tokenSpanStart) is { } whitespaceSpan)
            {
                context.ReportDiagnostic(CreateDiagnostic(Location.Create(context.Tree, whitespaceSpan)));
            }
        }
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        // A semicolon token terminates dozens of distinct statement, declaration and directive node kinds, and
        // Roslyn offers no syntax-token action; there is no closed node-kind set to register against that would
        // narrow this scan rather than merely rename it.
        context.RegisterSyntaxTreeAction(OnSyntaxTree);
    }

    #endregion // DiagnosticAnalyzer
}