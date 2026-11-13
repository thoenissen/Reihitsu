using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Analyzer.Rules.Clarity;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Clarity;

/// <summary>
/// Code fix provider for <see cref="RH3102CodeMustNotContainEmptyStatementsAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH3102CodeMustNotContainEmptyStatementsCodeFixProvider))]
public class RH3102CodeMustNotContainEmptyStatementsCodeFixProvider : CodeFixProvider
{
    #region Methods

    /// <summary>
    /// Applying the code fix. An empty statement that ends a line behind other code takes its line break along only after
    /// handing it to the code in front of it, whose whitespace at the end of the line is removed, so that the next statement
    /// keeps its line (SHARED-FIX-007)
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="emptyStatement">Empty statement</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated <see cref="Document"/> with the code fix applied</returns>
    private static async Task<Document> ApplyCodeFixAsync(Document document, EmptyStatementSyntax emptyStatement, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (root == null)
        {
            return document;
        }

        var previousToken = emptyStatement.GetFirstToken().GetPreviousToken();
        var lineBreak = emptyStatement.GetTrailingTrivia().FirstOrDefault(static trivia => trivia.IsKind(SyntaxKind.EndOfLineTrivia));

        if (lineBreak.IsKind(SyntaxKind.EndOfLineTrivia)
            && previousToken.IsKind(SyntaxKind.None) == false
            && previousToken.TrailingTrivia.Any(SyntaxKind.EndOfLineTrivia) == false
            && previousToken.TrailingTrivia.All(static trivia => trivia.IsKind(SyntaxKind.WhitespaceTrivia)))
        {
            root = root.TrackNodes(emptyStatement);
            root = root.ReplaceToken(root.FindToken(previousToken.SpanStart), previousToken.WithTrailingTrivia(lineBreak));
            emptyStatement = root.GetCurrentNode(emptyStatement);
        }

        var updatedRoot = emptyStatement == null
                              ? null
                              : root.RemoveNode(emptyStatement, SyntaxRemoveOptions.KeepNoTrivia);

        return updatedRoot == null
                   ? document
                   : document.WithSyntaxRoot(updatedRoot);
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH3102CodeMustNotContainEmptyStatementsAnalyzer.DiagnosticId];

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
                var emptyStatement = root.FindToken(diagnostic.Location.SourceSpan.Start).Parent?.AncestorsAndSelf().OfType<EmptyStatementSyntax>().FirstOrDefault();

                if (emptyStatement != null
                    && emptyStatement.Parent is BlockSyntax or SwitchSectionSyntax
                    && SyntaxNodeUtilities.ContainsCommentOrDirective(emptyStatement) == false)
                {
                    context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH3102Title,
                                                              token => ApplyCodeFixAsync(context.Document, emptyStatement, token),
                                                              nameof(RH3102CodeMustNotContainEmptyStatementsCodeFixProvider)),
                                            diagnostic);
                }
            }
        }
    }

    #endregion // CodeFixProvider
}