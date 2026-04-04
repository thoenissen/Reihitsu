using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// Providing fixes for <see cref="RH0324MethodChainsShouldBeAlignedAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH0324MethodChainsShouldBeAlignedCodeFixProvider))]
public class RH0324MethodChainsShouldBeAlignedCodeFixProvider : CodeFixProvider
{
    #region Methods

    /// <summary>
    /// Applying code fix
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="diagnosticToken">Token with diagnostics</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private static async Task<Document> ApplyCodeFixAsync(Document document, SyntaxToken diagnosticToken, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken)
                                 .ConfigureAwait(false);

        if (root == null)
        {
            return document;
        }

        var chainRoot = FindChainRoot(diagnosticToken);

        if (chainRoot == null)
        {
            return document;
        }

        var referenceColumn = GetReferenceColumn(chainRoot);
        var previousToken = diagnosticToken.GetPreviousToken();
        var diagnosticLine = diagnosticToken.GetLocation().GetLineSpan().StartLinePosition.Line;
        var previousLine = previousToken.GetLocation().GetLineSpan().StartLinePosition.Line;
        var newLeadingTrivia = default(SyntaxTriviaList);

        if (diagnosticLine == previousLine)
        {
            newLeadingTrivia = newLeadingTrivia.Add(SyntaxFactory.EndOfLine(Environment.NewLine));
        }
        else
        {
            foreach (var trivia in diagnosticToken.LeadingTrivia.Where(trivia => trivia.IsKind(SyntaxKind.WhitespaceTrivia) == false))
            {
                newLeadingTrivia = newLeadingTrivia.Add(trivia);
            }
        }

        newLeadingTrivia = newLeadingTrivia.Add(SyntaxFactory.Whitespace(new string(' ', referenceColumn)));
        root = root.ReplaceToken(diagnosticToken, diagnosticToken.WithLeadingTrivia(newLeadingTrivia));

        return document.WithSyntaxRoot(root);
    }

    /// <summary>
    /// Finds the root expression of the chain containing the given token
    /// </summary>
    /// <param name="token">A token within the chain</param>
    /// <returns>The outermost chain node, or <c>null</c></returns>
    private static SyntaxNode FindChainRoot(SyntaxToken token)
    {
        var current = token.Parent;

        while (current != null)
        {
            var parent = current.Parent;

            if (parent is InvocationExpressionSyntax or ElementAccessExpressionSyntax or PostfixUnaryExpressionSyntax)
            {
                var grandParent = parent.Parent;

                while (grandParent is PostfixUnaryExpressionSyntax)
                {
                    grandParent = grandParent.Parent;
                }

                if (grandParent is MemberAccessExpressionSyntax or ConditionalAccessExpressionSyntax)
                {
                    current = grandParent;
                }
                else
                {
                    break;
                }
            }
            else if (parent is MemberAccessExpressionSyntax or ConditionalAccessExpressionSyntax)
            {
                current = parent;
            }
            else
            {
                break;
            }
        }

        return current;
    }

    /// <summary>
    /// Gets the reference column (the column of the first dot/question mark in the chain)
    /// </summary>
    /// <param name="chainRoot">The outermost node of the chain</param>
    /// <returns>The reference column</returns>
    private static int GetReferenceColumn(SyntaxNode chainRoot)
    {
        var links = CollectChainLinks(chainRoot);

        if (links.Count > 0)
        {
            return links[0].GetLocation().GetLineSpan().StartLinePosition.Character;
        }

        return 0;
    }

    /// <summary>
    /// Collects all chain link tokens from the outermost node down to the root expression
    /// </summary>
    /// <param name="node">The outermost node of the chain</param>
    /// <returns>List of alignment tokens in chain order (first link closest to root)</returns>
    private static List<SyntaxToken> CollectChainLinks(SyntaxNode node)
    {
        var links = new List<SyntaxToken>();
        var current = node;

        while (current != null)
        {
            if (current is InvocationExpressionSyntax invocation)
            {
                current = invocation.Expression;
            }
            else if (current is MemberAccessExpressionSyntax memberAccess)
            {
                if (memberAccess.Expression is PostfixUnaryExpressionSyntax postfixUnary)
                {
                    links.Add(postfixUnary.OperatorToken);
                    current = postfixUnary.Operand;
                }
                else
                {
                    links.Add(memberAccess.OperatorToken);
                    current = memberAccess.Expression;
                }
            }
            else if (current is ConditionalAccessExpressionSyntax conditionalAccess)
            {
                links.Add(conditionalAccess.OperatorToken);
                current = conditionalAccess.Expression;
            }
            else if (current is ElementAccessExpressionSyntax elementAccess)
            {
                current = elementAccess.Expression;
            }
            else if (current is PostfixUnaryExpressionSyntax postfix)
            {
                current = postfix.Operand;
            }
            else
            {
                break;
            }
        }

        links.Reverse();

        return links;
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH0324MethodChainsShouldBeAlignedAnalyzer.DiagnosticId];

    /// <inheritdoc/>
    public sealed override FixAllProvider GetFixAllProvider()
    {
        return WellKnownFixAllProviders.BatchFixer;
    }

    /// <inheritdoc/>
    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

        if (root != null)
        {
            foreach (var diagnostic in context.Diagnostics)
            {
                var diagnosticToken = root.FindToken(diagnostic.Location.SourceSpan.Start);

                context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH0324Title,
                                                          c => ApplyCodeFixAsync(context.Document, diagnosticToken, c),
                                                          nameof(RH0324MethodChainsShouldBeAlignedCodeFixProvider)),
                                        diagnostic);
            }
        }
    }

    #endregion // CodeFixProvider
}