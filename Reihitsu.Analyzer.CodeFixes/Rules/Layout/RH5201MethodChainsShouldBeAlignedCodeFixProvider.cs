using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Formatter;
using Reihitsu.Formatter.Pipeline.LineBreaks;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Layout;

/// <summary>
/// Providing fixes for <see cref="RH5201MethodChainsShouldBeAlignedAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH5201MethodChainsShouldBeAlignedCodeFixProvider))]
public class RH5201MethodChainsShouldBeAlignedCodeFixProvider : CodeFixProvider
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
            newLeadingTrivia = newLeadingTrivia.Add(SyntaxFactory.EndOfLine(ReihitsuFormatterHelpers.DetectEndOfLine(root)));
        }
        else
        {
            foreach (var trivia in diagnosticToken.LeadingTrivia.Where(trivia => trivia.IsKind(SyntaxKind.WhitespaceTrivia) == false))
            {
                newLeadingTrivia = newLeadingTrivia.Add(trivia);
            }
        }

        newLeadingTrivia = newLeadingTrivia.Add(SyntaxFactory.Whitespace(new string(' ', referenceColumn)));

        var newDiagnosticToken = diagnosticToken.WithLeadingTrivia(newLeadingTrivia);

        if (diagnosticLine == previousLine)
        {
            var newPreviousToken = previousToken.WithTrailingTrivia(LineBreakTriviaUtilities.StripTrailingWhitespace(previousToken.TrailingTrivia));

            root = root.ReplaceTokens([previousToken, diagnosticToken],
                                      (originalToken, _) => originalToken == previousToken ? newPreviousToken : newDiagnosticToken);
        }
        else
        {
            root = root.ReplaceToken(diagnosticToken, newDiagnosticToken);
        }

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
        var links = FluentChainAnalysisHelper.CollectChainLinks(chainRoot);

        if (links.Count > 0)
        {
            return links[0].GetLocation().GetLineSpan().StartLinePosition.Character;
        }

        return 0;
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH5201MethodChainsShouldBeAlignedAnalyzer.DiagnosticId];

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

                context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH5201Title,
                                                          cancellationToken => ApplyCodeFixAsync(context.Document, diagnosticToken, cancellationToken),
                                                          nameof(RH5201MethodChainsShouldBeAlignedCodeFixProvider)),
                                        diagnostic);
            }
        }
    }

    #endregion // CodeFixProvider
}