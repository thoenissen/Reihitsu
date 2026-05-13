using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Naming;

/// <summary>
/// Analyzer for single-letter identifiers
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0228SingleLetterIdentifiersShouldNotBeUsedAnalyzer : DiagnosticAnalyzerBase<RH0228SingleLetterIdentifiersShouldNotBeUsedAnalyzer>
{
    #region Fields

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0228";

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0228SingleLetterIdentifiersShouldNotBeUsedAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Naming, nameof(AnalyzerResources.RH0228Title), nameof(AnalyzerResources.RH0228MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Checks whether a parameter symbol should be analyzed
    /// </summary>
    /// <param name="parameterSymbol">Parameter symbol</param>
    /// <returns>True if the symbol should be analyzed, otherwise false</returns>
    private static bool ShouldAnalyze(IParameterSymbol parameterSymbol)
    {
        if (HasSingleLetterName(parameterSymbol.Name) == false)
        {
            return false;
        }

        if (IsFrameworkMandatedSignature(parameterSymbol))
        {
            return false;
        }

        return IsRecordPrimaryConstructorParameter(parameterSymbol) == false;
    }

    /// <summary>
    /// Checks whether a local symbol should be analyzed
    /// </summary>
    /// <param name="localSymbol">Local symbol</param>
    /// <returns>True if the symbol should be analyzed, otherwise false</returns>
    private static bool ShouldAnalyze(ILocalSymbol localSymbol)
    {
        return HasSingleLetterName(localSymbol.Name);
    }

    /// <summary>
    /// Checks whether an identifier uses a single-letter name
    /// </summary>
    /// <param name="name">Identifier name</param>
    /// <returns>True if the identifier uses a single-letter name, otherwise false</returns>
    private static bool HasSingleLetterName(string name)
    {
        return name.Length == 1
               && name != "_";
    }

    /// <summary>
    /// Checks whether the parameter belongs to a signature that is not practical to rename
    /// </summary>
    /// <param name="parameterSymbol">Parameter symbol</param>
    /// <returns>True if the signature should be excluded, otherwise false</returns>
    private static bool IsFrameworkMandatedSignature(IParameterSymbol parameterSymbol)
    {
        if (parameterSymbol.ContainingSymbol is IMethodSymbol methodSymbol)
        {
            return methodSymbol.MethodKind == MethodKind.AnonymousFunction
                   || methodSymbol.IsOverride
                   || methodSymbol.ExplicitInterfaceImplementations.Length > 0;
        }

        if (parameterSymbol.ContainingSymbol is IPropertySymbol propertySymbol)
        {
            return propertySymbol.IsOverride
                   || propertySymbol.ExplicitInterfaceImplementations.Length > 0;
        }

        return false;
    }

    /// <summary>
    /// Checks whether a parameter belongs to a record primary constructor
    /// </summary>
    /// <param name="parameterSymbol">Parameter symbol</param>
    /// <returns>True if the parameter belongs to a record primary constructor, otherwise false</returns>
    private static bool IsRecordPrimaryConstructorParameter(IParameterSymbol parameterSymbol)
    {
        foreach (var syntaxReference in parameterSymbol.DeclaringSyntaxReferences)
        {
            if (syntaxReference.GetSyntax() is ParameterSyntax { Parent: ParameterListSyntax { Parent: RecordDeclarationSyntax } })
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Analyzes a catch declaration
    /// </summary>
    /// <param name="context">Context</param>
    private void OnCatchDeclaration(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is CatchDeclarationSyntax { Identifier.ValueText: var identifierName, Identifier.RawKind: not 0 } catchDeclaration
            && HasSingleLetterName(identifierName))
        {
            context.ReportDiagnostic(CreateDiagnostic(catchDeclaration.Identifier.GetLocation()));
        }
    }

    /// <summary>
    /// Analyzes a foreach statement
    /// </summary>
    /// <param name="context">Context</param>
    private void OnForEachStatement(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is ForEachStatementSyntax { Identifier.ValueText: var identifierName } forEachStatement
            && HasSingleLetterName(identifierName))
        {
            context.ReportDiagnostic(CreateDiagnostic(forEachStatement.Identifier.GetLocation()));
        }
    }

    /// <summary>
    /// Analyzes a parameter
    /// </summary>
    /// <param name="context">Context</param>
    private void OnParameter(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is ParameterSyntax parameter
            && context.SemanticModel.GetDeclaredSymbol(parameter, context.CancellationToken) is IParameterSymbol parameterSymbol
            && ShouldAnalyze(parameterSymbol))
        {
            context.ReportDiagnostic(CreateDiagnostic(parameter.Identifier.GetLocation()));
        }
    }

    /// <summary>
    /// Analyzes a single variable designation
    /// </summary>
    /// <param name="context">Context</param>
    private void OnSingleVariableDesignation(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is SingleVariableDesignationSyntax { Identifier.ValueText: var identifierName } designation
            && context.SemanticModel.GetDeclaredSymbol(designation, context.CancellationToken) is ILocalSymbol
            && HasSingleLetterName(identifierName))
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
        if (context.Node is VariableDeclaratorSyntax variableDeclarator
            && context.SemanticModel.GetDeclaredSymbol(variableDeclarator, context.CancellationToken) is ILocalSymbol localSymbol
            && ShouldAnalyze(localSymbol))
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

        context.RegisterSyntaxNodeAction(OnCatchDeclaration, SyntaxKind.CatchDeclaration);
        context.RegisterSyntaxNodeAction(OnForEachStatement, SyntaxKind.ForEachStatement);
        context.RegisterSyntaxNodeAction(OnParameter, SyntaxKind.Parameter);
        context.RegisterSyntaxNodeAction(OnSingleVariableDesignation, SyntaxKind.SingleVariableDesignation);
        context.RegisterSyntaxNodeAction(OnVariableDeclarator, SyntaxKind.VariableDeclarator);
    }

    #endregion // DiagnosticAnalyzer
}