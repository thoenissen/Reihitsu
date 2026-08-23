using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.Rules.Layout;

/// <summary>
/// RH5410: Final array initializer items must not have trailing commas
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH5410FinalArrayInitializerItemsMustNotHaveTrailingCommasAnalyzer : DiagnosticAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH5410";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5410FinalArrayInitializerItemsMustNotHaveTrailingCommasAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Layout, nameof(AnalyzerResources.RH5410Title), nameof(AnalyzerResources.RH5410MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyzes array initializer expressions
    /// </summary>
    /// <param name="context">Context</param>
    private void OnArrayInitializerExpression(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not InitializerExpressionSyntax initializer)
        {
            return;
        }

        var expressionsAndSeparators = initializer.Expressions.GetWithSeparators();

        if (expressionsAndSeparators.Count == 0)
        {
            return;
        }

        var lastItem = expressionsAndSeparators[expressionsAndSeparators.Count - 1];

        if (lastItem.IsToken == false || lastItem.AsToken().IsKind(SyntaxKind.CommaToken) == false)
        {
            return;
        }

        var lastSeparator = lastItem.AsToken();
        var gap = TextSpan.FromBounds(lastSeparator.Span.End, initializer.CloseBraceToken.SpanStart);

        if (SyntaxTriviaUtilities.ContainsConditionalCompilationBoundary(initializer, gap))
        {
            return;
        }

        context.ReportDiagnostic(CreateDiagnostic(lastSeparator.GetLocation()));
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnArrayInitializerExpression, SyntaxKind.ArrayInitializerExpression);
    }

    #endregion // DiagnosticAnalyzer
}