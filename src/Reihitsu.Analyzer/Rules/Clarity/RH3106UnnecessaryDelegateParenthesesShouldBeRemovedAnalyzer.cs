using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;
using Reihitsu.Core;

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

    /// <summary>
    /// Name of the parameter that gives the anonymous method another natural type when its position is decided
    /// </summary>
    private const string ProbeParameterName = "reihitsuNaturalTypeProbe";

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
    /// Determines whether the anonymous method takes its natural type: no delegate type is its target, and no overload,
    /// user-defined operator, collection initializer <c>Add</c> method, or type inference chooses among candidates for
    /// it. Without a parameter list, an anonymous method has no natural type, so the list cannot be removed there. The
    /// decision is made on the outcome, wherever the anonymous method stands: the anonymous method is bound again with a
    /// parameter list that gives it another natural type, and it takes its natural type exactly when the code around it
    /// still binds as before with that type, and the anonymous method then converts to that other type, where it
    /// converted to a delegate type, or still to the same type that is not a delegate type. A delegate type as its target
    /// rejects the other type or keeps it from following, and a choice among candidates picks another one or none
    /// </summary>
    /// <param name="node">Anonymous method</param>
    /// <param name="semanticModel">Semantic model</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns><see langword="true"/> if the anonymous method takes its natural type</returns>
    private static bool TakesNaturalType(AnonymousMethodExpressionSyntax node, SemanticModel semanticModel, CancellationToken cancellationToken)
    {
        var probe = node.WithParameterList(SyntaxFactory.ParameterList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Parameter(SyntaxFactory.Identifier(ProbeParameterName))
                                                                                                                         .WithType(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword))))));

        if (RewriteBindingUtilities.BindsAsBefore(semanticModel, node, probe, out var probeConvertedType, cancellationToken) == false)
        {
            return false;
        }

        var convertedType = semanticModel.GetTypeInfo(node, cancellationToken).ConvertedType;

        // A delegate type the anonymous method converts to came from the anonymous method itself only when it follows
        // the other natural type; a type that is not a delegate type has to stay the one it converts to
        return IsDelegateType(convertedType)
                   ? IsDelegateType(probeConvertedType) && SymbolEqualityComparer.Default.Equals(probeConvertedType, convertedType) == false
                   : SymbolEqualityComparer.Default.Equals(probeConvertedType, convertedType);
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