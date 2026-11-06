using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Reihitsu.Core;

/// <summary>
/// Decides whether the code around an expression still binds as before when the expression is replaced. Where the
/// language chooses among candidates for an expression, or takes a type from it, a replacement can make the code around
/// it bind to another overload, user-defined operator, <c>Add</c> method, or inferred type argument, or not compile at
/// all, wherever the expression stands. Analyzers and code fixes that must not report or write such code share this one
/// answer, which the C# binder gives on the replaced code, instead of a list of the positions where it can happen
/// </summary>
public static class RewriteBindingUtilities
{
    #region Methods

    /// <summary>
    /// Determines whether the code around an expression binds as before when the expression is replaced. The outermost
    /// statement, initializer, expression body, or constructor initializer around the expression, below the member or
    /// accessor it belongs to, is bound again with the replacement in place of the expression. The
    /// code binds as before when the replacement converts to a type in its position, the replaced code has no
    /// error the original code did not have, and every method, constructor, indexer, operator, and collection initializer
    /// <c>Add</c> method that the expression and the nodes around it bind to, type arguments included, stays the same
    /// </summary>
    /// <param name="semanticModel">Semantic model of the expression's document</param>
    /// <param name="expression">Expression</param>
    /// <param name="replacement">Replacement of the expression</param>
    /// <param name="replacementConvertedType">The type the replacement converts to, when the code binds as before</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns><see langword="true"/> if the code binds as before; <see langword="false"/> when it does not, or when the code around the expression cannot be bound again</returns>
    public static bool BindsAsBefore(SemanticModel semanticModel, ExpressionSyntax expression, ExpressionSyntax replacement, out ITypeSymbol replacementConvertedType, CancellationToken cancellationToken)
    {
        replacementConvertedType = null;

        var bindingRoot = GetBindingRoot(expression);

        if (bindingRoot == null)
        {
            return false;
        }

        var annotation = new SyntaxAnnotation();
        var replacedRoot = bindingRoot.ReplaceNode(expression, replacement.WithAdditionalAnnotations(annotation));

        if (TryGetSpeculativeSemanticModel(semanticModel, bindingRoot, replacedRoot, out var replacedModel) == false
            || replacedRoot.GetAnnotatedNodes(annotation).FirstOrDefault() is not ExpressionSyntax replacedExpression)
        {
            return false;
        }

        if (replacedModel.GetConversion(replacedExpression, cancellationToken).Exists == false
            || CountErrors(replacedModel.GetOperation(replacedRoot, cancellationToken)) > CountErrors(semanticModel.GetOperation(bindingRoot, cancellationToken)))
        {
            return false;
        }

        // The replacement leaves the structure around it unchanged, so the nodes around the expression and around the
        // replacement correspond one to one
        var originalNodes = GetBoundNodes(expression, bindingRoot);
        var replacedNodes = GetBoundNodes(replacedExpression, replacedRoot);

        for (var index = 0; index < originalNodes.Count; index++)
        {
            if (GetBoundSymbolText(semanticModel, originalNodes[index], cancellationToken) != GetBoundSymbolText(replacedModel, replacedNodes[index], cancellationToken))
            {
                return false;
            }
        }

        replacementConvertedType = replacedModel.GetTypeInfo(replacedExpression, cancellationToken).ConvertedType;

        return true;
    }

    /// <summary>
    /// Gets the node that is bound again: the outermost statement other than a block, variable or property initializer,
    /// expression body, constructor initializer, or primary constructor base type around the expression, below the
    /// member or accessor the expression belongs to. A statement inside an anonymous function belongs to the code around
    /// that function, whose types it can decide, so the search goes on past it, and so it does past a local function,
    /// which is bound as the statement it is
    /// </summary>
    /// <param name="expression">Expression</param>
    /// <returns>The node, or <see langword="null"/> when there is none</returns>
    private static SyntaxNode GetBindingRoot(ExpressionSyntax expression)
    {
        SyntaxNode bindingRoot = null;

        foreach (var ancestor in expression.Ancestors())
        {
            if (ancestor is MemberDeclarationSyntax or AccessorDeclarationSyntax)
            {
                break;
            }

            if ((ancestor is StatementSyntax and not BlockSyntax)
                || ancestor is EqualsValueClauseSyntax
                || ancestor is ArrowExpressionClauseSyntax
                || ancestor is ConstructorInitializerSyntax
                || ancestor is PrimaryConstructorBaseTypeSyntax)
            {
                bindingRoot = ancestor;
            }
        }

        return bindingRoot;
    }

    /// <summary>
    /// Binds the replaced node in place of the original one
    /// </summary>
    /// <param name="semanticModel">Semantic model of the original node</param>
    /// <param name="bindingRoot">Original node</param>
    /// <param name="replacedRoot">Replaced node</param>
    /// <param name="replacedModel">Semantic model of the replaced node</param>
    /// <returns><see langword="true"/> if the replaced node could be bound</returns>
    private static bool TryGetSpeculativeSemanticModel(SemanticModel semanticModel, SyntaxNode bindingRoot, SyntaxNode replacedRoot, out SemanticModel replacedModel)
    {
        var position = bindingRoot.SpanStart;

        replacedModel = null;

        return replacedRoot switch
               {
                   StatementSyntax statement => semanticModel.TryGetSpeculativeSemanticModel(position, statement, out replacedModel),
                   EqualsValueClauseSyntax initializer => semanticModel.TryGetSpeculativeSemanticModel(position, initializer, out replacedModel),
                   ArrowExpressionClauseSyntax expressionBody => semanticModel.TryGetSpeculativeSemanticModel(position, expressionBody, out replacedModel),
                   ConstructorInitializerSyntax constructorInitializer => semanticModel.TryGetSpeculativeSemanticModel(position, constructorInitializer, out replacedModel),
                   PrimaryConstructorBaseTypeSyntax baseType => semanticModel.TryGetSpeculativeSemanticModel(position, baseType, out replacedModel),
                   _ => false,
               };
    }

    /// <summary>
    /// Counts the errors the binder records in a bound operation tree: invalid operations, conversions that do not
    /// exist, and operations of an error type
    /// </summary>
    /// <param name="operation">Operation</param>
    /// <returns>The number of errors</returns>
    private static int CountErrors(IOperation operation)
    {
        if (operation == null)
        {
            return 0;
        }

        return operation.DescendantsAndSelf()
                        .Count(static descendant => descendant is IInvalidOperation
                                                    || descendant is IConversionOperation { Conversion.Exists: false }
                                                    || descendant.Type is IErrorTypeSymbol);
    }

    /// <summary>
    /// Gets the expression and the nodes around it up to the bound node, innermost first
    /// </summary>
    /// <param name="expression">Expression</param>
    /// <param name="bindingRoot">Bound node</param>
    /// <returns>The nodes</returns>
    private static List<SyntaxNode> GetBoundNodes(ExpressionSyntax expression, SyntaxNode bindingRoot)
    {
        var nodes = new List<SyntaxNode>
                    {
                        expression
                    };

        foreach (var ancestor in expression.Ancestors())
        {
            nodes.Add(ancestor);

            if (ancestor == bindingRoot)
            {
                break;
            }
        }

        return nodes;
    }

    /// <summary>
    /// Gets the display text of the symbols a node binds to: the symbol of the node, or the candidates when the binding
    /// failed, and, for an element of a collection initializer, the <c>Add</c> method it calls. An anonymous function
    /// is no choice among candidates, and its own signature may follow the replacement, so it is left out
    /// </summary>
    /// <param name="semanticModel">Semantic model</param>
    /// <param name="node">Node</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The display text</returns>
    private static string GetBoundSymbolText(SemanticModel semanticModel, SyntaxNode node, CancellationToken cancellationToken)
    {
        var symbolInfo = semanticModel.GetSymbolInfo(node, cancellationToken);
        var text = symbolInfo.Symbol is IMethodSymbol { MethodKind: MethodKind.AnonymousFunction }
                       ? string.Empty
                       : GetDisplayText(symbolInfo);

        if (node is ExpressionSyntax { Parent: InitializerExpressionSyntax initializer } element
            && initializer.IsKind(SyntaxKind.CollectionInitializerExpression))
        {
            text = $"{text}|{GetDisplayText(semanticModel.GetCollectionInitializerSymbolInfo(element, cancellationToken))}";
        }

        return text;
    }

    /// <summary>
    /// Gets the display text of a symbol info: its symbol, or its candidates and the reason none was chosen
    /// </summary>
    /// <param name="symbolInfo">Symbol info</param>
    /// <returns>The display text</returns>
    private static string GetDisplayText(SymbolInfo symbolInfo)
    {
        return symbolInfo.Symbol != null
                   ? symbolInfo.Symbol.ToDisplayString()
                   : $"{symbolInfo.CandidateReason}:{string.Join(";", symbolInfo.CandidateSymbols.Select(static candidate => candidate.ToDisplayString()))}";
    }

    #endregion // Methods
}