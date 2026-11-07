using System.Composition;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.Rules.Clarity;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Clarity;

/// <summary>
/// Code fix provider for <see cref="RH3106UnnecessaryDelegateParenthesesShouldBeRemovedAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH3106UnnecessaryDelegateParenthesesShouldBeRemovedCodeFixProvider))]
public class RH3106UnnecessaryDelegateParenthesesShouldBeRemovedCodeFixProvider : EmptyParenthesesCodeFixProviderBase<AnonymousMethodExpressionSyntax>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH3106UnnecessaryDelegateParenthesesShouldBeRemovedCodeFixProvider()
        : base(RH3106UnnecessaryDelegateParenthesesShouldBeRemovedAnalyzer.DiagnosticId, CodeFixResources.RH3106Title, new RH3106UnnecessaryDelegateParenthesesShouldBeRemovedAnalyzer())
    {
    }

    #endregion // Constructor

    #region EmptyParenthesesCodeFixProviderBase

    /// <inheritdoc/>
    private protected override bool KeepsSpaceInFrontOfFollowingToken => true;

    /// <inheritdoc/>
    private protected override SyntaxNode GetList(AnonymousMethodExpressionSyntax node)
    {
        return node.ParameterList;
    }

    /// <inheritdoc/>
    private protected override AnonymousMethodExpressionSyntax RemoveList(AnonymousMethodExpressionSyntax node, SyntaxTriviaList separatingTrivia)
    {
        return node.WithParameterList(null)
                   .WithDelegateKeyword(node.DelegateKeyword.WithTrailingTrivia(separatingTrivia));
    }

    /// <inheritdoc/>
    private protected override async Task<bool> KeepsMeaningAsync(Document document, SyntaxNode root, AnonymousMethodExpressionSyntax node, CancellationToken cancellationToken)
    {
        // Without a parameter list, an anonymous method converts to a delegate of any parameter list and has no natural
        // type, so it can convert to another delegate type, another overload, user-defined operator, or collection
        // initializer Add method can be chosen for it, the choice can become ambiguous, or a type argument inferred from
        // it can no longer be inferred. The list stays unless the code around the anonymous method binds as before
        // without it and gains no error, and the anonymous method converts to the same type, wherever it stands
        var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

        return semanticModel != null
               && RewriteBindingUtilities.BindsAsBefore(semanticModel, node, BuildNodeWithoutList(node), out var convertedType, cancellationToken)
               && SymbolEqualityComparer.Default.Equals(convertedType, semanticModel.GetTypeInfo(node, cancellationToken).ConvertedType);
    }

    #endregion // EmptyParenthesesCodeFixProviderBase
}