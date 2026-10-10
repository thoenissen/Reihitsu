using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Clarity;

/// <summary>
/// RH3102: Code must not contain empty statements
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH3102CodeMustNotContainEmptyStatementsAnalyzer : DiagnosticAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH3102";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH3102CodeMustNotContainEmptyStatementsAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Clarity, nameof(AnalyzerResources.RH3102Title), nameof(AnalyzerResources.RH3102MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyzing all <see cref="SyntaxKind.EmptyStatement"/> occurrences
    /// </summary>
    /// <param name="context">Context</param>
    private void OnEmptyStatement(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is EmptyStatementSyntax emptyStatement)
        {
            context.ReportDiagnostic(CreateDiagnostic(emptyStatement.SemicolonToken.GetLocation()));
        }
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnEmptyStatement, SyntaxKind.EmptyStatement);
    }

    #endregion // DiagnosticAnalyzer
}