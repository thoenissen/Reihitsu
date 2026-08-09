using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Core;
using Reihitsu.Formatter;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Layout;

/// <summary>
/// Providing fixes for <see cref="RH5102ArgumentsShouldBeOnSingleOrSeparateLinesAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH5102ArgumentsShouldBeOnSingleOrSeparateLinesCodeFixProvider))]
public class RH5102ArgumentsShouldBeOnSingleOrSeparateLinesCodeFixProvider : CodeFixProvider
{
    #region Methods

    /// <summary>
    /// Applying code fix by delegating the layout to the shared formatter
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="argumentList">Argument list with diagnostics</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private static async Task<Document> ApplyCodeFixAsync(Document document, ArgumentListSyntax argumentList, CancellationToken cancellationToken)
    {
        return await ReihitsuFormatter.FormatNodeInDocumentAsync(document, argumentList, cancellationToken).ConfigureAwait(false);
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH5102ArgumentsShouldBeOnSingleOrSeparateLinesAnalyzer.DiagnosticId];

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
                var node = root.FindNode(diagnostic.Location.SourceSpan);
                var argumentList = node as ArgumentListSyntax ?? node.FirstAncestorOrSelf<ArgumentListSyntax>();

                // The trailing side is released because the shared formatter restores the last token's trailing
                // trivia unconditionally, so a comment behind the closing parenthesis is never crossed. The leading
                // side stays guarded because that same entry point restores the first token's leading trivia only
                // when the token does not start a line.
                if (argumentList != null && SyntaxNodeUtilities.LeadingAndInteriorContainsCommentOrDirective(argumentList) == false)
                {
                    context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH5102Title,
                                                              cancellationToken => ApplyCodeFixAsync(context.Document, argumentList, cancellationToken),
                                                              nameof(RH5102ArgumentsShouldBeOnSingleOrSeparateLinesCodeFixProvider)),
                                            diagnostic);
                }
            }
        }
    }

    #endregion // CodeFixProvider
}