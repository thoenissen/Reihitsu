using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Layout;

/// <summary>
/// RH5105: Opening parenthesis must be on declaration line
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH5105OpeningParenthesisMustBeOnDeclarationLineAnalyzer : DiagnosticAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH5105";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5105OpeningParenthesisMustBeOnDeclarationLineAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Layout, nameof(AnalyzerResources.RH5105Title), nameof(AnalyzerResources.RH5105MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Reports a diagnostic when the opening parenthesis of a parameter list is not on the same line as the preceding declaration token
    /// </summary>
    /// <param name="context">Context</param>
    /// <param name="parameterList">Parameter list to inspect</param>
    private void CheckParameterList(SyntaxNodeAnalysisContext context, ParameterListSyntax parameterList)
    {
        var openParenToken = parameterList.OpenParenToken;
        var previousToken = openParenToken.GetPreviousToken();

        if (previousToken.GetLocation().GetLineSpan().StartLinePosition.Line != openParenToken.GetLocation().GetLineSpan().StartLinePosition.Line)
        {
            context.ReportDiagnostic(CreateDiagnostic(openParenToken.GetLocation()));
        }
    }

    /// <summary>
    /// Analyzes a method declaration node
    /// </summary>
    /// <param name="context">Context</param>
    private void OnMethodDeclaration(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is MethodDeclarationSyntax method)
        {
            CheckParameterList(context, method.ParameterList);
        }
    }

    /// <summary>
    /// Analyzes a constructor declaration node
    /// </summary>
    /// <param name="context">Context</param>
    private void OnConstructorDeclaration(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is ConstructorDeclarationSyntax constructor)
        {
            CheckParameterList(context, constructor.ParameterList);
        }
    }

    /// <summary>
    /// Analyzes a local function statement node
    /// </summary>
    /// <param name="context">Context</param>
    private void OnLocalFunctionStatement(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is LocalFunctionStatementSyntax localFunction)
        {
            CheckParameterList(context, localFunction.ParameterList);
        }
    }

    /// <summary>
    /// Analyzes an operator declaration node
    /// </summary>
    /// <param name="context">Context</param>
    private void OnOperatorDeclaration(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is OperatorDeclarationSyntax operatorDeclaration)
        {
            CheckParameterList(context, operatorDeclaration.ParameterList);
        }
    }

    /// <summary>
    /// Analyzes a conversion operator declaration node
    /// </summary>
    /// <param name="context">Context</param>
    private void OnConversionOperatorDeclaration(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is ConversionOperatorDeclarationSyntax conversionOperator)
        {
            CheckParameterList(context, conversionOperator.ParameterList);
        }
    }

    /// <summary>
    /// Analyzes a delegate declaration node
    /// </summary>
    /// <param name="context">Context</param>
    private void OnDelegateDeclaration(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is DelegateDeclarationSyntax delegateDeclaration)
        {
            CheckParameterList(context, delegateDeclaration.ParameterList);
        }
    }

    /// <summary>
    /// Analyzes a record declaration node
    /// </summary>
    /// <param name="context">Context</param>
    private void OnRecordDeclaration(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is RecordDeclarationSyntax { ParameterList: { } parameterList })
        {
            CheckParameterList(context, parameterList);
        }
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnMethodDeclaration, SyntaxKind.MethodDeclaration);
        context.RegisterSyntaxNodeAction(OnConstructorDeclaration, SyntaxKind.ConstructorDeclaration);
        context.RegisterSyntaxNodeAction(OnLocalFunctionStatement, SyntaxKind.LocalFunctionStatement);
        context.RegisterSyntaxNodeAction(OnOperatorDeclaration, SyntaxKind.OperatorDeclaration);
        context.RegisterSyntaxNodeAction(OnConversionOperatorDeclaration, SyntaxKind.ConversionOperatorDeclaration);
        context.RegisterSyntaxNodeAction(OnDelegateDeclaration, SyntaxKind.DelegateDeclaration);
        context.RegisterSyntaxNodeAction(OnRecordDeclaration, SyntaxKind.RecordDeclaration, SyntaxKind.RecordStructDeclaration);
    }

    #endregion // DiagnosticAnalyzer
}