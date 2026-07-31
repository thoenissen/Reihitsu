using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Naming;

/// <summary>
/// Analyzer for single-letter local variable names
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH4125SingleLetterLocalVariableNamesAnalyzer : DiagnosticAnalyzerBase
{
    #region Fields

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH4125";

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH4125SingleLetterLocalVariableNamesAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Naming, nameof(AnalyzerResources.RH4125Title), nameof(AnalyzerResources.RH4125MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyzes a single variable designation
    /// </summary>
    /// <param name="context">Context</param>
    private void OnSingleVariableDesignation(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is SingleVariableDesignationSyntax { Identifier.ValueText: var identifierName } designation
            && context.SemanticModel.GetDeclaredSymbol(designation, context.CancellationToken) is ILocalSymbol
            && SingleLetterIdentifierUtilities.IsForEachIterationVariable(designation) == false
            && SingleLetterIdentifierUtilities.HasSingleLetterName(identifierName))
        {
            context.ReportDiagnostic(CreateDiagnostic(designation.Identifier.GetLocation()));
        }
    }

    /// <summary>
    /// Analyzes a variable declarator
    /// </summary>
    /// <param name="context">Context</param>
    private void OnVariableDeclarator(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is VariableDeclaratorSyntax { Identifier.ValueText: var identifierName } variableDeclarator
            && context.SemanticModel.GetDeclaredSymbol(variableDeclarator, context.CancellationToken) is ILocalSymbol
            && SingleLetterIdentifierUtilities.HasSingleLetterName(identifierName))
        {
            context.ReportDiagnostic(CreateDiagnostic(variableDeclarator.Identifier.GetLocation()));
        }
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnSingleVariableDesignation, SyntaxKind.SingleVariableDesignation);
        context.RegisterSyntaxNodeAction(OnVariableDeclarator, SyntaxKind.VariableDeclarator);
    }

    #endregion // DiagnosticAnalyzer
}