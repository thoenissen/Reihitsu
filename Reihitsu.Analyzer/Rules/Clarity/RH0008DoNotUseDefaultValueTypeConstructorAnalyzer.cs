using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Clarity;

/// <summary>
/// RH0008: Do not use default value type constructor.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0008DoNotUseDefaultValueTypeConstructorAnalyzer : DiagnosticAnalyzerBase<RH0008DoNotUseDefaultValueTypeConstructorAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0008";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0008DoNotUseDefaultValueTypeConstructorAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Clarity, nameof(AnalyzerResources.RH0008Title), nameof(AnalyzerResources.RH0008MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Determine whether the creation expression should be reported.
    /// </summary>
    /// <param name="typeSymbol">Type symbol</param>
    /// <param name="argumentCount">Argument count</param>
    /// <param name="hasInitializer">Has initializer</param>
    /// <returns><see langword="true"/> if the expression should be reported</returns>
    private static bool ShouldReport(ITypeSymbol typeSymbol, int argumentCount, bool hasInitializer)
    {
        return typeSymbol is { IsValueType: true }
               && argumentCount == 0
               && hasInitializer == false;
    }

    /// <summary>
    /// Analyze implicit object creation expressions.
    /// </summary>
    /// <param name="context">Context</param>
    private void OnImplicitObjectCreationExpression(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not ImplicitObjectCreationExpressionSyntax implicitObjectCreationExpression)
        {
            return;
        }

        if (ShouldReport(context.SemanticModel.GetTypeInfo(implicitObjectCreationExpression, context.CancellationToken).Type,
                         implicitObjectCreationExpression.ArgumentList?.Arguments.Count ?? 0,
                         implicitObjectCreationExpression.Initializer != null))
        {
            context.ReportDiagnostic(CreateDiagnostic(implicitObjectCreationExpression.NewKeyword.GetLocation()));
        }
    }

    /// <summary>
    /// Analyze object creation expressions.
    /// </summary>
    /// <param name="context">Context</param>
    private void OnObjectCreationExpression(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not ObjectCreationExpressionSyntax objectCreationExpression)
        {
            return;
        }

        if (ShouldReport(context.SemanticModel.GetTypeInfo(objectCreationExpression, context.CancellationToken).Type,
                         objectCreationExpression.ArgumentList?.Arguments.Count ?? 0,
                         objectCreationExpression.Initializer != null))
        {
            context.ReportDiagnostic(CreateDiagnostic(objectCreationExpression.Type.GetLocation()));
        }
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnObjectCreationExpression, SyntaxKind.ObjectCreationExpression);
        context.RegisterSyntaxNodeAction(OnImplicitObjectCreationExpression, SyntaxKind.ImplicitObjectCreationExpression);
    }

    #endregion // DiagnosticAnalyzer
}