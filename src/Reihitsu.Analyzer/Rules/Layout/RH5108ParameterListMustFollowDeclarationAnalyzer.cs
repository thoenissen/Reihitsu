using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.Rules.Layout;

/// <summary>
/// RH5108: Parameter list must follow declaration
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH5108ParameterListMustFollowDeclarationAnalyzer : DiagnosticAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH5108";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5108ParameterListMustFollowDeclarationAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Layout, nameof(AnalyzerResources.RH5108Title), nameof(AnalyzerResources.RH5108MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Reports a diagnostic when the first parameter of a parameter list is not on the same line as the opening parenthesis
    /// </summary>
    /// <param name="context">Context</param>
    /// <param name="parameterList">Parameter list to inspect</param>
    private void CheckFirstParameterLine(SyntaxNodeAnalysisContext context, ParameterListSyntax parameterList)
    {
        if (parameterList.Parameters.Count == 0)
        {
            return;
        }

        var firstParameterFirstToken = parameterList.Parameters[0].GetFirstToken();

        if (parameterList.OpenParenToken.GetLocation().GetLineSpan().StartLinePosition.Line == firstParameterFirstToken.GetLocation().GetLineSpan().StartLinePosition.Line)
        {
            return;
        }

        // The formatter refuses to pull the first parameter onto the opening parenthesis line when a comment or
        // directive sits in the gap, so flagging that shape would leave a permanent diagnostic.
        if (SyntaxTriviaUtilities.WouldJoinAcrossUnjoinableTrivia(parameterList.OpenParenToken, firstParameterFirstToken))
        {
            return;
        }

        context.ReportDiagnostic(CreateDiagnostic(firstParameterFirstToken.GetLocation()));
    }

    /// <summary>
    /// Analyzes method declarations
    /// </summary>
    /// <param name="context">Context</param>
    private void OnMethodDeclaration(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is MethodDeclarationSyntax method)
        {
            CheckFirstParameterLine(context, method.ParameterList);
        }
    }

    /// <summary>
    /// Analyzes constructor declarations
    /// </summary>
    /// <param name="context">Context</param>
    private void OnConstructorDeclaration(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is ConstructorDeclarationSyntax constructor)
        {
            CheckFirstParameterLine(context, constructor.ParameterList);
        }
    }

    /// <summary>
    /// Analyzes local function statements
    /// </summary>
    /// <param name="context">Context</param>
    private void OnLocalFunctionStatement(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is LocalFunctionStatementSyntax localFunction)
        {
            CheckFirstParameterLine(context, localFunction.ParameterList);
        }
    }

    /// <summary>
    /// Analyzes operator declarations
    /// </summary>
    /// <param name="context">Context</param>
    private void OnOperatorDeclaration(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is OperatorDeclarationSyntax operatorDeclaration)
        {
            CheckFirstParameterLine(context, operatorDeclaration.ParameterList);
        }
    }

    /// <summary>
    /// Analyzes conversion operator declarations
    /// </summary>
    /// <param name="context">Context</param>
    private void OnConversionOperatorDeclaration(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is ConversionOperatorDeclarationSyntax conversionOperator)
        {
            CheckFirstParameterLine(context, conversionOperator.ParameterList);
        }
    }

    /// <summary>
    /// Analyzes delegate declarations
    /// </summary>
    /// <param name="context">Context</param>
    private void OnDelegateDeclaration(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is DelegateDeclarationSyntax delegateDeclaration)
        {
            CheckFirstParameterLine(context, delegateDeclaration.ParameterList);
        }
    }

    /// <summary>
    /// Analyzes record declarations
    /// </summary>
    /// <param name="context">Context</param>
    private void OnRecordDeclaration(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is RecordDeclarationSyntax { ParameterList: { } parameterList })
        {
            CheckFirstParameterLine(context, parameterList);
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