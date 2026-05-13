using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// RH0396: Using declarations should not be used
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0396UsingDeclarationsShouldNotBeUsedAnalyzer : DiagnosticAnalyzerBase<RH0396UsingDeclarationsShouldNotBeUsedAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0396";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0396UsingDeclarationsShouldNotBeUsedAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Formatting, nameof(AnalyzerResources.RH0396Title), nameof(AnalyzerResources.RH0396MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyzes local declarations
    /// </summary>
    /// <param name="context">Context</param>
    private void OnLocalDeclarationStatement(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not LocalDeclarationStatementSyntax { IsConst: false, UsingKeyword.RawKind: not 0 } usingDeclaration)
        {
            return;
        }

        context.ReportDiagnostic(CreateDiagnostic(usingDeclaration.UsingKeyword.GetLocation()));
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnLocalDeclarationStatement, SyntaxKind.LocalDeclarationStatement);
    }

    #endregion // DiagnosticAnalyzer
}