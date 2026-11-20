using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.CodeFixes.Core;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Core;
using Reihitsu.Formatter;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Layout;

/// <summary>
/// Providing fixes for <see cref="RH5307IndexerBracketedArgumentsShouldBeSingleLinedAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH5307IndexerBracketedArgumentsShouldBeSingleLinedCodeFixProvider))]
public class RH5307IndexerBracketedArgumentsShouldBeSingleLinedCodeFixProvider : ReihitsuCodeFixProviderBase
{
    #region Methods

    /// <summary>
    /// Applying code fix
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="formatTarget">Node that should be formatted</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private static async Task<Document> ApplyCodeFixAsync(Document document, SyntaxNode formatTarget, CancellationToken cancellationToken)
    {
        return (await ReihitsuFormatter.FormatNodeInDocumentAsync(document, formatTarget, cancellationToken).ConfigureAwait(false)).Output;
    }

    /// <summary>
    /// Gets the node the fix formats: the bracketed argument list the diagnostic is located on (RH5307-003), whose first token
    /// keeps its line and column, so the code around it stays as written (RH5307-004, SHARED-FIX-006). The diagnostic covers
    /// the whole element access, or, for an element binding or an implicit element access, the list itself; the innermost node with exactly that
    /// span is that element access or list, also where an argument or another node around it has the same span. Neither the
    /// receiver of the element access nor the code around it decides which list is fixed
    /// </summary>
    /// <param name="root">Root</param>
    /// <param name="diagnostic">Diagnostic</param>
    /// <returns>The bracketed argument list, or <see langword="null"/> when there is none or the analyzer would not report it</returns>
    private static SyntaxNode TryGetFormatTarget(SyntaxNode root, Diagnostic diagnostic)
    {
        var span = diagnostic.Location.SourceSpan;

        if (root.FullSpan.Contains(span) == false)
        {
            return null;
        }

        var bracketedArgumentList = root.FindNode(span, getInnermostNodeForTie: true) switch
                                    {
                                        ElementAccessExpressionSyntax elementAccess when elementAccess.Span == span => elementAccess.ArgumentList,
                                        BracketedArgumentListSyntax list when list.Span == span => list,
                                        _ => null
                                    };

        return bracketedArgumentList == null
               || SyntaxNodeUtilities.IsIndexArgumentList(bracketedArgumentList) == false
               || RH5307IndexerBracketedArgumentsShouldBeSingleLinedAnalyzer.CanSafelyCollapseToSingleLine(bracketedArgumentList) == false
                   ? null
                   : bracketedArgumentList;
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH5307IndexerBracketedArgumentsShouldBeSingleLinedAnalyzer.DiagnosticId];

    /// <inheritdoc/>
    public sealed override FixAllProvider GetFixAllProvider()
    {
        // The fix changes only the reported construct and lays it out from the column its first token has, so two constructs
        // on one line are fixed one after another, each from the column the fix in front of it left (SHARED-FIX-002)
        return DocumentOrderFixAllProvider.Create(new RH5307IndexerBracketedArgumentsShouldBeSingleLinedAnalyzer());
    }

    #endregion // CodeFixProvider

    #region ReihitsuCodeFixProviderBase

    /// <inheritdoc/>
    protected override bool FormatsThroughFormatter => true;

    /// <inheritdoc/>
    protected sealed override async Task RegisterCodeFixesCoreAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

        if (root != null)
        {
            foreach (var diagnostic in context.Diagnostics)
            {
                var formatTarget = TryGetFormatTarget(root, diagnostic);

                if (formatTarget != null
                    && ReihitsuFormatter.FindUnsupportedSyntax(formatTarget) == null)
                {
                    context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH5307Title,
                                                              cancellationToken => ApplyCodeFixAsync(context.Document, formatTarget, cancellationToken),
                                                              nameof(RH5307IndexerBracketedArgumentsShouldBeSingleLinedCodeFixProvider)),
                                            diagnostic);
                }
            }
        }
    }

    #endregion // ReihitsuCodeFixProviderBase
}