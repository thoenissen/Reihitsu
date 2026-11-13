using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Analyzer.CodeFixes.Core;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Formatter;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Layout;

/// <summary>
/// Code fix provider for <see cref="RH5403StatementMustNotBeOnSingleLineAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH5403StatementMustNotBeOnSingleLineCodeFixProvider))]
public class RH5403StatementMustNotBeOnSingleLineCodeFixProvider : CodeFixProvider
{
    #region Methods

    /// <summary>
    /// Applies the code fix: the reported block is laid out as the formatter lays it out, with its braces on their own
    /// lines in the column of the line on which its owner starts; the header, the other blocks of the statement, and the
    /// blank lines in front of it stay as written (RH5403-003)
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="block">Single-line block</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated document</returns>
    private static Task<Document> ApplyCodeFixAsync(Document document, BlockSyntax block, CancellationToken cancellationToken)
    {
        return LineLayoutCodeFixUtilities.ReplaceWithBlockOnOwnLinesAsync(document, block, block, cancellationToken);
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH5403StatementMustNotBeOnSingleLineAnalyzer.DiagnosticId];

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
                if (root.FindToken(diagnostic.Location.SourceSpan.Start).Parent?.FirstAncestorOrSelf<BlockSyntax>() is { Parent: not null } block
                    && ReihitsuFormatter.FindUnsupportedSyntax(block) == null)
                {
                    context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH5403Title,
                                                              token => ApplyCodeFixAsync(context.Document, block, token),
                                                              nameof(RH5403StatementMustNotBeOnSingleLineCodeFixProvider)),
                                            diagnostic);
                }
            }
        }
    }

    #endregion // CodeFixProvider
}