using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Clarity;

/// <summary>
/// RH3106: Unnecessary delegate parentheses should be removed
/// </summary>
[Microsoft.CodeAnalysis.Diagnostics.DiagnosticAnalyzer(Microsoft.CodeAnalysis.LanguageNames.CSharp)]
public class RH3106UnnecessaryDelegateParenthesesShouldBeRemovedAnalyzer : EmptyParenthesesAnalyzerBase<AnonymousMethodExpressionSyntax>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH3106";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH3106UnnecessaryDelegateParenthesesShouldBeRemovedAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Clarity, nameof(AnalyzerResources.RH3106Title), nameof(AnalyzerResources.RH3106MessageFormat), SyntaxKind.AnonymousMethodExpression)
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Determines whether the anonymous method takes its natural type: no delegate type is its target, and no overload
    /// resolution or type inference chooses one. Without a parameter list, an anonymous method has no natural type, so
    /// the list cannot be removed there. That is the case for the initializer of an implicitly typed local, for a cast to
    /// a type that is not a delegate type, for a conversion to a type that is not a delegate type, such as
    /// <see langword="object"/> or <see cref="System.Delegate"/>, and for an argument of such a parameter type when no
    /// other overload is a candidate for the call and no type argument is inferred
    /// </summary>
    /// <param name="node">Anonymous method</param>
    /// <param name="semanticModel">Semantic model</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns><see langword="true"/> if the anonymous method takes its natural type</returns>
    private static bool TakesNaturalType(AnonymousMethodExpressionSyntax node, SemanticModel semanticModel, CancellationToken cancellationToken)
    {
        switch (node.Parent)
        {
            case EqualsValueClauseSyntax { Parent: VariableDeclaratorSyntax { Parent: VariableDeclarationSyntax { Type.IsVar: true } } }:
                return true;

            case CastExpressionSyntax castExpression:
                return IsDelegateType(semanticModel.GetTypeInfo(castExpression.Type, cancellationToken).Type) == false;
        }

        if (IsDelegateType(semanticModel.GetTypeInfo(node, cancellationToken).ConvertedType))
        {
            return false;
        }

        return node.Parent is not ArgumentSyntax { Parent: BaseArgumentListSyntax { Parent: { } call } }
               || HasCompetingCandidates(call, semanticModel, cancellationToken) == false;
    }

    /// <summary>
    /// Determines whether the type is a delegate type
    /// </summary>
    /// <param name="type">Type</param>
    /// <returns><see langword="true"/> for a delegate type</returns>
    private static bool IsDelegateType(ITypeSymbol type)
    {
        return type?.TypeKind == TypeKind.Delegate;
    }

    /// <summary>
    /// Determines whether overload resolution or type inference takes part in choosing the target of the call: another
    /// overload is a candidate for it, or the bound method is generic. A call that binds to nothing counts as such
    /// </summary>
    /// <param name="call">Invocation, object creation, constructor initializer, or element access</param>
    /// <param name="semanticModel">Semantic model</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns><see langword="true"/> if the binding of the call can depend on the argument</returns>
    private static bool HasCompetingCandidates(SyntaxNode call, SemanticModel semanticModel, CancellationToken cancellationToken)
    {
        var symbol = semanticModel.GetSymbolInfo(call, cancellationToken).Symbol;

        switch (symbol)
        {
            case IMethodSymbol { IsGenericMethod: true }:
                return true;

            case IMethodSymbol { MethodKind: MethodKind.Constructor } constructor:
                return constructor.ContainingType.InstanceConstructors.Length > 1;

            case IMethodSymbol when call is InvocationExpressionSyntax invocation:
                return semanticModel.GetMemberGroup(invocation.Expression, cancellationToken).Length > 1;

            case IPropertySymbol { IsIndexer: true } indexer:
                return indexer.ContainingType.GetMembers(indexer.Name).Length > 1;

            default:
                return true;
        }
    }

    #endregion // Methods

    #region EmptyParenthesesAnalyzerBase

    /// <inheritdoc/>
    protected override bool HasUnnecessaryParentheses(AnonymousMethodExpressionSyntax node, SemanticModel semanticModel, CancellationToken cancellationToken)
    {
        return node.ParameterList is { Parameters.Count: 0 }
               && TakesNaturalType(node, semanticModel, cancellationToken) == false;
    }

    /// <inheritdoc/>
    protected override Location GetDiagnosticLocation(AnonymousMethodExpressionSyntax node)
    {
        return node.ParameterList?.GetLocation() ?? node.GetLocation();
    }

    #endregion // EmptyParenthesesAnalyzerBase
}