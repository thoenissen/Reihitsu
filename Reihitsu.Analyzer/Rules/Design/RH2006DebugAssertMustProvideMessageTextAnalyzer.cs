using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Design;

/// <summary>
/// RH2006: Debug.Assert must provide message text
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH2006DebugAssertMustProvideMessageTextAnalyzer : DiagnosticAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH2006";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH2006DebugAssertMustProvideMessageTextAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Design, nameof(AnalyzerResources.RH2006Title), nameof(AnalyzerResources.RH2006MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Determines whether the invocation syntactically calls a method with the specified name
    /// </summary>
    /// <param name="invocationExpression">Invocation expression</param>
    /// <param name="methodName">Method name</param>
    /// <returns><see langword="true"/> if the called method has the specified simple name</returns>
    private static bool IsCalledMethodNamed(InvocationExpressionSyntax invocationExpression, string methodName)
    {
        return invocationExpression.Expression switch
               {
                   MemberAccessExpressionSyntax memberAccess => memberAccess.Name.Identifier.ValueText == methodName,
                   MemberBindingExpressionSyntax memberBinding => memberBinding.Name.Identifier.ValueText == methodName,
                   IdentifierNameSyntax identifierName => identifierName.Identifier.ValueText == methodName,
                   _ => false
               };
    }

    /// <summary>
    /// Analyzing all <see cref="SyntaxKind.InvocationExpression"/> nodes
    /// </summary>
    /// <param name="context">Context</param>
    private void OnInvocationExpression(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not InvocationExpressionSyntax invocationExpression)
        {
            return;
        }

        // Cheap syntactic pre-filter before the semantic binding below
        if (IsCalledMethodNamed(invocationExpression, "Assert") == false)
        {
            return;
        }

        var symbolInfo = context.SemanticModel.GetSymbolInfo(invocationExpression, context.CancellationToken);
        var methodSymbol = symbolInfo.Symbol as IMethodSymbol ?? symbolInfo.CandidateSymbols.OfType<IMethodSymbol>().FirstOrDefault();

        if (methodSymbol == null)
        {
            return;
        }

        if (methodSymbol.Name != "Assert")
        {
            return;
        }

        if (methodSymbol.ContainingType.ToDisplayString() != "System.Diagnostics.Debug")
        {
            return;
        }

        if (invocationExpression.ArgumentList.Arguments.Count >= 2)
        {
            return;
        }

        context.ReportDiagnostic(CreateDiagnostic(invocationExpression.GetLocation()));
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnInvocationExpression, SyntaxKind.InvocationExpression);
    }

    #endregion // DiagnosticAnalyzer
}