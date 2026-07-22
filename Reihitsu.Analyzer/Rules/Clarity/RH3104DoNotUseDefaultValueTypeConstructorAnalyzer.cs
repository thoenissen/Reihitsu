using System.Linq;

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
                         implicitObjectCreationExpression.Initializer != null))
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