using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Clarity;

/// <summary>
/// RH0003: Code must not contain empty statements
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0003CodeMustNotContainEmptyStatementsAnalyzer : DiagnosticAnalyzerBase<RH0003CodeMustNotContainEmptyStatementsAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0003";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0003CodeMustNotContainEmptyStatementsAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Clarity, nameof(AnalyzerResources.RH0003Title), nameof(AnalyzerResources.RH0003MessageFormat))
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