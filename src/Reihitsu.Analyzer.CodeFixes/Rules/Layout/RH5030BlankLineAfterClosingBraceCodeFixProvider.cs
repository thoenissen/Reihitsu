using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.CodeFixes.Core;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Layout;

/// <summary>
/// Code fix provider for <see cref="RH5030BlankLineAfterClosingBraceAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH5030BlankLineAfterClosingBraceCodeFixProvider))]
public class RH5030BlankLineAfterClosingBraceCodeFixProvider : CodeFixProvider
{
    #region Methods

    /// <summary>
    /// Applies the code fix by inserting a blank line after the closing brace
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="diagnosticSpan">Diagnostic span</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated document</returns>
    private static async Task<Document> ApplyCodeFixAsync(Document document, TextSpan diagnosticSpan, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (root == null)
        {
            return document;
        }

        var closeBraceToken = root.FindToken(diagnosticSpan.Start);

        if (closeBraceToken.IsKind(SyntaxKind.CloseBraceToken) == false)
        {
            return document;
        }

        var nextToken = closeBraceToken.GetNextToken();

        if (nextToken.IsKind(SyntaxKind.None))
        {
            return document;
        }

        return BlankLineCodeFixUtilities.TryInsertBlankLineBefore(root, nextToken, behindLeadingDirectives: true, out var updatedRoot)
                   ? document.WithSyntaxRoot(updatedRoot)
                   : document;
    }

    /// <summary>
    /// Determines whether the code fix can be offered. The blank line goes at the start of the next statement's leading
    /// trivia (SHARED-AFTERBRACE-001), which separates the statements only when a line break outside any comment already ends
    /// the line in front of the statement. When every line break between the closing brace and the next statement lies
    /// inside a comment, the statement starts behind the comment on its last line, and the fix is withheld (RH5030-003)
    /// </summary>
    /// <param name="root">Syntax root</param>
    /// <param name="diagnosticSpan">Diagnostic span</param>
    /// <returns><see langword="true"/> if the code fix can be offered</returns>
    private static bool CanRegisterCodeFix(SyntaxNode root, TextSpan diagnosticSpan)
    {
        var closeBraceToken = root.FindToken(diagnosticSpan.Start);

        if (closeBraceToken.IsKind(SyntaxKind.CloseBraceToken) == false)
        {
            return false;
        }

        var nextToken = closeBraceToken.GetNextToken();

        return nextToken.IsKind(SyntaxKind.None) == false
               && SyntaxTriviaUtilities.AreSeparatedByEndOfLine(closeBraceToken, nextToken);
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH5030BlankLineAfterClosingBraceAnalyzer.DiagnosticId];

    /// <inheritdoc/>
    public sealed override FixAllProvider GetFixAllProvider()
    {
        return WellKnownFixAllProviders.BatchFixer;
    }

    /// <inheritdoc/>
    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

        if (root == null)
        {
            return;
        }

        foreach (var diagnostic in context.Diagnostics)
        {
            if (CanRegisterCodeFix(root, diagnostic.Location.SourceSpan) == false)
            {
                continue;
            }

            context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH5030Title,
                                                      token => ApplyCodeFixAsync(context.Document, diagnostic.Location.SourceSpan, token),
                                                      nameof(RH5030BlankLineAfterClosingBraceCodeFixProvider)),
                                    diagnostic);
        }
    }

    #endregion // CodeFixProvider
}