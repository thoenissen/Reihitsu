using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// RH0813: Final array initializer items must not have trailing commas
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0813FinalArrayInitializerItemsMustNotHaveTrailingCommasAnalyzer : DiagnosticAnalyzerBase<RH0813FinalArrayInitializerItemsMustNotHaveTrailingCommasAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0813";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0813FinalArrayInitializerItemsMustNotHaveTrailingCommasAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Formatting, nameof(AnalyzerResources.RH0813Title), nameof(AnalyzerResources.RH0813MessageFormat))
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

        if (lastItem.IsToken && lastItem.AsToken().IsKind(SyntaxKind.CommaToken))
        {
            context.ReportDiagnostic(CreateDiagnostic(lastItem.AsToken().GetLocation()));
        }
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