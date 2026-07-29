using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Naming;

/// <summary>
/// Analyzer for single-letter foreach iteration variable names
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH4126SingleLetterForeachIterationVariableNamesAnalyzer : DiagnosticAnalyzerBase
{
    #region Fields

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH4126";

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH4126SingleLetterForeachIterationVariableNamesAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Naming, nameof(AnalyzerResources.RH4126Title), nameof(AnalyzerResources.RH4126MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyzes a variable designation in a deconstructed foreach iteration variable
    /// </summary>
    /// <param name="context">Context</param>
    private void OnSingleVariableDesignation(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is SingleVariableDesignationSyntax { Identifier.ValueText: var identifierName } designation
            && context.SemanticModel.GetDeclaredSymbol(designation, context.CancellationToken) is ILocalSymbol
            && SingleLetterIdentifierUtilities.IsForEachIterationVariable(designation)
            && SingleLetterIdentifierUtilities.HasSingleLetterName(identifierName))
        {
            context.ReportDiagnostic(CreateDiagnostic(designation.Identifier.GetLocation()));
        }
    }

    /// <summary>
    /// Analyzes a foreach statement
    /// </summary>
    /// <param name="context">Context</param>
    private void OnForEachStatement(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is ForEachStatementSyntax { Identifier.ValueText: var identifierName } forEachStatement
            && SingleLetterIdentifierUtilities.HasSingleLetterName(identifierName))
        {
            context.ReportDiagnostic(CreateDiagnostic(forEachStatement.Identifier.GetLocation()));
        }
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnForEachStatement, SyntaxKind.ForEachStatement);
        context.RegisterSyntaxNodeAction(OnSingleVariableDesignation, SyntaxKind.SingleVariableDesignation);
    }

    #endregion // DiagnosticAnalyzer
}