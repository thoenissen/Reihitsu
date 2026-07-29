using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Naming;

/// <summary>
/// Analyzer for single-letter catch exception variable names
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH4127SingleLetterCatchExceptionVariableNamesAnalyzer : DiagnosticAnalyzerBase
{
    #region Fields

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH4127";

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH4127SingleLetterCatchExceptionVariableNamesAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Naming, nameof(AnalyzerResources.RH4127Title), nameof(AnalyzerResources.RH4127MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyzes a catch declaration
    /// </summary>
    /// <param name="context">Context</param>
    private void OnCatchDeclaration(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is CatchDeclarationSyntax { Identifier.ValueText: var identifierName, Identifier.RawKind: not 0 } catchDeclaration
            && SingleLetterIdentifierUtilities.HasSingleLetterName(identifierName))
        {
            context.ReportDiagnostic(CreateDiagnostic(catchDeclaration.Identifier.GetLocation()));
        }
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnCatchDeclaration, SyntaxKind.CatchDeclaration);
    }

    #endregion // DiagnosticAnalyzer
}