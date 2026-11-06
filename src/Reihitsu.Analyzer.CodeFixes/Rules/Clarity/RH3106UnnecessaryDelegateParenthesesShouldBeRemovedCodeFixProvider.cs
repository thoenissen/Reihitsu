using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.Rules.Clarity;

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
        : base(RH3106UnnecessaryDelegateParenthesesShouldBeRemovedAnalyzer.DiagnosticId, CodeFixResources.RH3106Title)
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Determines whether the node binds a call or an operator whose target overload resolution or type inference
    /// chooses: an invocation, an object creation, a constructor initializer, a primary constructor base type, an element
    /// access, or a binary operator
    /// </summary>
    /// <param name="node">Node</param>
    /// <returns><see langword="true"/> if the node binds such a call or operator</returns>
    private static bool IsOverloadResolvedCall(SyntaxNode node)
    {
        return node is InvocationExpressionSyntax
                    or BaseObjectCreationExpressionSyntax
                    or ConstructorInitializerSyntax
                    or PrimaryConstructorBaseTypeSyntax
                    or ElementAccessExpressionSyntax
                    or BinaryExpressionSyntax;
    }

    /// <summary>
    /// Gets the display text of the symbol a node binds to
    /// </summary>
    /// <param name="semanticModel">Semantic model</param>
    /// <param name="node">Node</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The display text, or <see langword="null"/> when the node binds to no symbol</returns>
    private static string GetBoundSymbolText(SemanticModel semanticModel, SyntaxNode node, CancellationToken cancellationToken)
    {
        return semanticModel.GetSymbolInfo(node, cancellationToken).Symbol?.ToDisplayString();
    }

    #endregion // Methods

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
        // type, so another overload or user-defined operator can become applicable, the call or the operator can become
        // ambiguous, or a type argument inferred from the anonymous method can no longer be inferred. The list stays
        // where any call that takes the anonymous method as an argument, or any binary operator that takes it as an
        // operand, binds differently without it, or where the anonymous method converts to another type
        var enclosingNodes = node.Ancestors()
                                 .TakeWhile(static ancestor => ancestor is not (StatementSyntax or MemberDeclarationSyntax))
                                 .ToList();

        if (enclosingNodes.Any(static ancestor => ancestor is ArgumentSyntax or BinaryExpressionSyntax) == false)
        {
            return true;
        }

        var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

        if (semanticModel == null)
        {
            return false;
        }

        var calls = enclosingNodes.Where(IsOverloadResolvedCall).ToList();
        var trackedRoot = root.TrackNodes(calls.Append<SyntaxNode>(node));
        var trackedNode = trackedRoot.GetCurrentNode(node);
        var updatedRoot = trackedRoot.ReplaceNode(trackedNode, BuildNodeWithoutList(trackedNode));
        var updatedSemanticModel = await document.WithSyntaxRoot(updatedRoot).GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
        var updatedTree = updatedSemanticModel?.SyntaxTree;

        if (updatedTree == null)
        {
            return false;
        }

        var updatedTreeRoot = await updatedTree.GetRootAsync(cancellationToken).ConfigureAwait(false);

        if (updatedTreeRoot.GetCurrentNode(node) is not { } updatedNode
            || semanticModel.GetTypeInfo(node, cancellationToken).ConvertedType?.ToDisplayString() != updatedSemanticModel.GetTypeInfo(updatedNode, cancellationToken).ConvertedType?.ToDisplayString())
        {
            return false;
        }

        foreach (var call in calls)
        {
            var originalSymbolText = GetBoundSymbolText(semanticModel, call, cancellationToken);

            // A binary operator such as `??` or `as` binds to no operator method, so for it only an unchanged binding counts
            if ((originalSymbolText == null && call is not BinaryExpressionSyntax)
                || updatedTreeRoot.GetCurrentNode(call) is not { } updatedCall
                || GetBoundSymbolText(updatedSemanticModel, updatedCall, cancellationToken) != originalSymbolText)
            {
                return false;
            }
        }

        return true;
    }

    #endregion // EmptyParenthesesCodeFixProviderBase
}