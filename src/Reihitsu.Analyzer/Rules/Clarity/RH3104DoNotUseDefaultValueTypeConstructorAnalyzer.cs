using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Clarity;

/// <summary>
/// RH3104: Do not use default value type constructor
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH3104DoNotUseDefaultValueTypeConstructorAnalyzer : DiagnosticAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH3104";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH3104DoNotUseDefaultValueTypeConstructorAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Clarity, nameof(AnalyzerResources.RH3104Title), nameof(AnalyzerResources.RH3104MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Determine whether the creation expression should be reported
    /// </summary>
    /// <param name="typeSymbol">Type symbol</param>
    /// <param name="argumentCount">Argument count</param>
    /// <param name="hasInitializer">Has initializer</param>
    /// <returns><see langword="true"/> if the expression should be reported</returns>
    private static bool ShouldReport(ITypeSymbol typeSymbol, int argumentCount, bool hasInitializer)
    {
        // Type parameters are exempt: for "new T()" where T : struct the substituted struct may declare a C# 10
        // parameterless constructor, so "new T()" invokes it while "default(T)" does not. Whether the substitution
        // provides such a constructor is unknown at analysis time, so the creation must not be rewritten.
        return typeSymbol is { IsValueType: true } and not ITypeParameterSymbol
               && argumentCount == 0
               && hasInitializer == false
               && HasUserDefinedParameterlessConstructor(typeSymbol) == false;
    }

    /// <summary>
    /// Determine whether the type declares a user-defined parameterless instance constructor
    /// </summary>
    /// <param name="typeSymbol">Type symbol</param>
    /// <returns><see langword="true"/> if the type has an explicit parameterless constructor</returns>
    private static bool HasUserDefinedParameterlessConstructor(ITypeSymbol typeSymbol)
    {
        // C# 10 structs may declare an explicit parameterless constructor. For such a type "new S()" runs the
        // constructor while "default(S)" does not, so the creation must not be rewritten to "default(S)".
        return typeSymbol is INamedTypeSymbol namedType
               && namedType.InstanceConstructors.Any(constructor => constructor.Parameters.Length == 0
                                                                    && constructor.IsImplicitlyDeclared == false);
    }

    /// <summary>
    /// Determine whether the creation stands where only a statement expression is valid: as the expression of an
    /// expression statement, as an initializer or an iterator of a <c>for</c> statement, or as the expression body of a
    /// function whose body is a statement. <c>default(S)</c> is not a statement expression, so the change the rule asks
    /// for cannot be written there (CS0201)
    /// </summary>
    /// <param name="creationExpression">Creation expression</param>
    /// <param name="semanticModel">Semantic model</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns><see langword="true"/> if the creation stands as a statement</returns>
    private static bool StandsAsStatement(ExpressionSyntax creationExpression, SemanticModel semanticModel, CancellationToken cancellationToken)
    {
        switch (creationExpression.Parent)
        {
            case ExpressionStatementSyntax:
                return true;

            case ForStatementSyntax forStatement:
                return forStatement.Initializers.Contains(creationExpression)
                       || forStatement.Incrementors.Contains(creationExpression);

            case ArrowExpressionClauseSyntax arrowExpressionClause:
                return semanticModel.GetDeclaredSymbol(arrowExpressionClause.Parent, cancellationToken) is IMethodSymbol method
                       && HasStatementBody(method);

            case LambdaExpressionSyntax lambdaExpression when lambdaExpression.ExpressionBody == creationExpression:
                return semanticModel.GetSymbolInfo(lambdaExpression, cancellationToken).Symbol is IMethodSymbol lambda
                       && HasStatementBody(lambda);

            default:
                return false;
        }
    }

    /// <summary>
    /// Determine whether the body of the function is a statement rather than a value: the function returns
    /// <see langword="void"/>, or it is <see langword="async"/> and its return type, such as <c>Task</c> or
    /// <c>ValueTask</c>, carries no value. Constructors, finalizers, and <c>set</c>, <c>init</c>, <c>add</c>, and
    /// <c>remove</c> accessors return <see langword="void"/>
    /// </summary>
    /// <param name="method">Method symbol of the function</param>
    /// <returns><see langword="true"/> if the expression body of the function stands as a statement</returns>
    private static bool HasStatementBody(IMethodSymbol method)
    {
        return method.ReturnsVoid
               || (method.IsAsync
                   && method.ReturnType is INamedTypeSymbol { Arity: 0 });
    }

    /// <summary>
    /// Analyze implicit object creation expressions
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
                         implicitObjectCreationExpression.Initializer != null)
            && StandsAsStatement(implicitObjectCreationExpression, context.SemanticModel, context.CancellationToken) == false)
        {
            context.ReportDiagnostic(CreateDiagnostic(implicitObjectCreationExpression.NewKeyword.GetLocation()));
        }
    }

    /// <summary>
    /// Analyze object creation expressions
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
                         objectCreationExpression.Initializer != null)
            && StandsAsStatement(objectCreationExpression, context.SemanticModel, context.CancellationToken) == false)
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