using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Layout;

/// <summary>
/// RH5108: Parameter list must follow declaration
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH5108ParameterListMustFollowDeclarationAnalyzer : DiagnosticAnalyzerBase<RH5108ParameterListMustFollowDeclarationAnalyzer>
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

        if (parameterList.OpenParenToken.GetLocation().GetLineSpan().StartLinePosition.Line != firstParameterFirstToken.GetLocation().GetLineSpan().StartLinePosition.Line)
        {
            context.ReportDiagnostic(CreateDiagnostic(firstParameterFirstToken.GetLocation()));
        }
    }

    /// <summary>
    /// Analyzes method declarations
    /// </summary>
    /// <param name="context">Context</param>
    private void OnMethodDeclaration(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not MethodDeclarationSyntax method)
        {
            return;
        }

        CheckFirstParameterLine(context, method.ParameterList);
    }

    /// <summary>
    /// Analyzes constructor declarations
    /// </summary>
    /// <param name="context">Context</param>
    private void OnConstructorDeclaration(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not ConstructorDeclarationSyntax constructor)
        {
            return;
        }

        CheckFirstParameterLine(context, constructor.ParameterList);
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnMethodDeclaration, SyntaxKind.MethodDeclaration);
        context.RegisterSyntaxNodeAction(OnConstructorDeclaration, SyntaxKind.ConstructorDeclaration);
    }

    #endregion // DiagnosticAnalyzer
}