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
using Microsoft.CodeAnalysis.Text;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// Code fix provider for <see cref="RH0395BreakStatementsShouldNotBeInsideExplicitSwitchCaseBlocksAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH0395BreakStatementsShouldNotBeInsideExplicitSwitchCaseBlocksCodeFixProvider))]
public class RH0395BreakStatementsShouldNotBeInsideExplicitSwitchCaseBlocksCodeFixProvider : CodeFixProvider
{
    #region Methods

    /// <summary>
    /// Applies the code fix
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="breakStatement">Break statement</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated document</returns>
    private static async Task<Document> ApplyCodeFixAsync(Document document, BreakStatementSyntax breakStatement, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);

        if (root == null
            || TryGetFixableNodes(breakStatement, out var block, out var switchSection) == false)
        {
            return document;
        }

        var updatedBreakStatement = breakStatement.WithLeadingTrivia(UpdateLeadingTriviaIndentation(breakStatement.GetLeadingTrivia(), GetIndentationTrivia(sourceText, block.CloseBraceToken)));
        var updatedBlock = block.WithStatements(block.Statements.Remove(breakStatement));
        var updatedSwitchSection = switchSection.WithStatements(switchSection.Statements.Replace(block, updatedBlock)
                                                                                        .Add(updatedBreakStatement));

        return document.WithSyntaxRoot(root.ReplaceNode(switchSection, updatedSwitchSection));
    }

    /// <summary>
    /// Gets the indentation trivia from a token's current line
    /// </summary>
    /// <param name="sourceText">Source text</param>
    /// <param name="token">Token</param>
    /// <returns>The line indentation trivia</returns>
    private static SyntaxTriviaList GetIndentationTrivia(SourceText sourceText, SyntaxToken token)
    {
        var line = sourceText.Lines.GetLineFromPosition(token.SpanStart);
        var indentation = sourceText.ToString(TextSpan.FromBounds(line.Start, token.SpanStart));

        return string.IsNullOrEmpty(indentation)
                   ? []
                   : SyntaxFactory.TriviaList(SyntaxFactory.Whitespace(indentation));
    }

    /// <summary>
    /// Tries to get the nodes required for a safe code fix
    /// </summary>
    /// <param name="breakStatement">Break statement</param>
    /// <param name="block">Containing block</param>
    /// <param name="switchSection">Containing switch section</param>
    /// <returns><see langword="true"/> if the common fixable pattern was found; otherwise, <see langword="false"/></returns>
    private static bool TryGetFixableNodes(BreakStatementSyntax breakStatement, out BlockSyntax block, out SwitchSectionSyntax switchSection)
    {
        block = null;
        switchSection = null;

        if (breakStatement.Parent is not BlockSyntax breakBlock
            || breakBlock.Parent is not SwitchSectionSyntax containingSwitchSection
            || breakBlock.Statements.Count == 0
            || breakBlock.Statements[breakBlock.Statements.Count - 1] != breakStatement
            || containingSwitchSection.Statements.Count == 0
            || containingSwitchSection.Statements[containingSwitchSection.Statements.Count - 1] != breakBlock)
        {
            return false;
        }

        block = breakBlock;
        switchSection = containingSwitchSection;

        return true;
    }

    /// <summary>
    /// Updates indentation in leading trivia to match the target indentation
    /// </summary>
    /// <param name="leadingTrivia">Leading trivia</param>
    /// <param name="indentationTrivia">Target indentation</param>
    /// <returns>The updated leading trivia</returns>
    private static SyntaxTriviaList UpdateLeadingTriviaIndentation(SyntaxTriviaList leadingTrivia, SyntaxTriviaList indentationTrivia)
    {
        List<SyntaxTrivia> updatedLeadingTrivia = [];
        var isAtLineStart = true;

        if (leadingTrivia.Count == 0)
        {
            updatedLeadingTrivia.AddRange(indentationTrivia);

            return SyntaxFactory.TriviaList(updatedLeadingTrivia);
        }

        foreach (var trivia in leadingTrivia)
        {
            if (isAtLineStart)
            {
                if (trivia.IsKind(SyntaxKind.WhitespaceTrivia))
                {
                    updatedLeadingTrivia.AddRange(indentationTrivia);
                    isAtLineStart = false;

                    continue;
                }

                if (trivia.IsKind(SyntaxKind.EndOfLineTrivia) == false)
                {
                    updatedLeadingTrivia.AddRange(indentationTrivia);
                    isAtLineStart = false;
                }
            }

            updatedLeadingTrivia.Add(trivia);
            isAtLineStart = trivia.IsKind(SyntaxKind.EndOfLineTrivia);
        }

        return SyntaxFactory.TriviaList(updatedLeadingTrivia);
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH0395BreakStatementsShouldNotBeInsideExplicitSwitchCaseBlocksAnalyzer.DiagnosticId];

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
                var breakStatement = root.FindToken(diagnostic.Location.SourceSpan.Start).Parent?.FirstAncestorOrSelf<BreakStatementSyntax>();

                if (breakStatement != null
                    && TryGetFixableNodes(breakStatement, out _, out _))
                {
                    context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH0395Title,
                                                              token => ApplyCodeFixAsync(context.Document, breakStatement, token),
                                                              nameof(RH0395BreakStatementsShouldNotBeInsideExplicitSwitchCaseBlocksCodeFixProvider)),
                                            diagnostic);
                }
            }
        }
    }

    #endregion // CodeFixProvider
}