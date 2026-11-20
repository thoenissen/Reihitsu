using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.CodeFixes.Core;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Core;
using Reihitsu.Formatter;
using Reihitsu.Formatter.Pipeline.LineBreaks.Utilities;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Layout;

/// <summary>
/// Code fix provider for <see cref="RH5419ExpressionStyleGetOnlyIndexersShouldBeSingleLinedAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH5419ExpressionStyleGetOnlyIndexersShouldBeSingleLinedCodeFixProvider))]
public class RH5419ExpressionStyleGetOnlyIndexersShouldBeSingleLinedCodeFixProvider : ReihitsuCodeFixProviderBase
{
    #region Methods

    /// <summary>
    /// Applies the code fix (RH5419-003). The indexer's parameter list is joined onto one line; a declaration that shares its
    /// line with another member is then separated from it, so that both stand on lines of their own (SHARED-FIX-007); then
    /// the formatter formats the declaration
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="indexerDeclaration">Indexer declaration</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated document</returns>
    private static async Task<Document> ApplyCodeFixAsync(Document document, IndexerDeclarationSyntax indexerDeclaration, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (root == null)
        {
            return document;
        }

        var annotation = new SyntaxAnnotation();
        var newDocument = document.WithSyntaxRoot(root.ReplaceNode(indexerDeclaration, JoinParameterList(indexerDeclaration).WithAdditionalAnnotations(annotation)));
        var newRoot = await newDocument.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (newRoot?.GetAnnotatedNodes(annotation).FirstOrDefault() is not IndexerDeclarationSyntax joinedIndexer)
        {
            return document;
        }

        return await BlankLineCodeFixUtilities.FormatMemberOnLinesOfItsOwnAsync(newDocument, joinedIndexer, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Puts the parameter list of an indexer on one line (RH5419-003): every gap between two tokens from its <c>[</c> through
    /// its <c>]</c> that holds a line break loses its line breaks together with the whitespace around them, and keeps only the
    /// minimal separator a join of the formatter writes (FMT-BREAK-031). Such a gap holds nothing but whitespace and
    /// line breaks, because the fix is withheld for a parameter list on several lines that holds a comment, a directive, or
    /// disabled text (<see cref="HoldsCommentDirectiveOrDisabledTextInWrappedParameterList"/>). The formatter spaces the
    /// joined tokens afterwards
    /// </summary>
    /// <param name="indexerDeclaration">Indexer declaration</param>
    /// <returns>The indexer declaration with its parameter list on one line</returns>
    private static IndexerDeclarationSyntax JoinParameterList(IndexerDeclarationSyntax indexerDeclaration)
    {
        var openBracket = indexerDeclaration.ParameterList.OpenBracketToken;
        var closeBracket = indexerDeclaration.ParameterList.CloseBracketToken;
        var joinedGaps = new HashSet<SyntaxToken>();

        // Each joined gap is recorded by the token in front of it
        for (var token = openBracket; token != closeBracket; token = token.GetNextToken())
        {
            var nextToken = token.GetNextToken();

            if (token.TrailingTrivia.Concat(nextToken.LeadingTrivia).Any(obj => obj.IsKind(SyntaxKind.EndOfLineTrivia)))
            {
                joinedGaps.Add(token);
            }
        }

        if (joinedGaps.Count == 0)
        {
            return indexerDeclaration;
        }

        var tokens = joinedGaps.Concat(joinedGaps.Select(obj => obj.GetNextToken())).Distinct();

        return indexerDeclaration.ReplaceTokens(tokens,
                                                (original, rewritten) =>
                                                {
                                                    if (joinedGaps.Contains(original))
                                                    {
                                                        rewritten = rewritten.WithTrailingTrivia();
                                                    }

                                                    var previousToken = original.GetPreviousToken();

                                                    return joinedGaps.Contains(previousToken)
                                                               ? rewritten.WithLeadingTrivia(LineBreakTriviaUtilities.CreateJoinSeparator(previousToken, original))
                                                               : rewritten;
                                                });
    }

    /// <summary>
    /// Determines whether the parameter list of an indexer spans several lines and holds a comment, a directive, or disabled
    /// text between its <c>[</c> and its <c>]</c> (RH5419-004): in the trivia of a token from the <c>[</c> through the
    /// <c>]</c>, other than the leading trivia of the <c>[</c> and the trailing trivia of the <c>]</c>. A parameter list on
    /// one line is joined nowhere, so a comment inside it does not count
    /// </summary>
    /// <param name="parameterList">Parameter list as written</param>
    /// <returns><see langword="true"/> when the parameter list spans several lines and holds such trivia; otherwise, <see langword="false"/></returns>
    private static bool HoldsCommentDirectiveOrDisabledTextInWrappedParameterList(BracketedParameterListSyntax parameterList)
    {
        var openBracket = parameterList.OpenBracketToken;
        var closeBracket = parameterList.CloseBracketToken;

        if (openBracket.GetLocation().GetLineSpan().StartLinePosition.Line == closeBracket.GetLocation().GetLineSpan().StartLinePosition.Line)
        {
            return false;
        }

        for (var token = openBracket; token != closeBracket; token = token.GetNextToken())
        {
            if (SyntaxTriviaUtilities.WouldJoinAcrossUnjoinableTrivia(token, token.GetNextToken()))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Determines whether formatting the indexer with its parameter list joined completes the fix (RH5419-003, RH5419-004):
    /// the formatter puts the indexer's code from the <c>[</c> of its parameter list through its <c>;</c> on one line, and it
    /// neither inserts nor removes a blank line in the gap behind each attribute list as written. The code in front of the
    /// <c>[</c>, such as a comment between <c>this</c> and the <c>[</c>, does not count
    /// </summary>
    /// <param name="root">Syntax root of the document that contains the indexer</param>
    /// <param name="indexerDeclaration">Indexer declaration</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns><see langword="true"/> when formatting puts that code on one line and keeps the blank lines behind the attribute lists; otherwise, <see langword="false"/></returns>
    private static bool FormatsOnOneLineKeepingBlankLines(SyntaxNode root, IndexerDeclarationSyntax indexerDeclaration, CancellationToken cancellationToken)
    {
        var annotation = new SyntaxAnnotation();
        var joinedRoot = root.ReplaceNode(indexerDeclaration, JoinParameterList(indexerDeclaration).WithAdditionalAnnotations(annotation));

        if (joinedRoot.GetAnnotatedNodes(annotation).FirstOrDefault() is not IndexerDeclarationSyntax joinedIndexer
            || ReihitsuFormatter.FormatNode(joinedIndexer, cancellationToken: cancellationToken).Output is not IndexerDeclarationSyntax formatted)
        {
            return false;
        }

        var text = formatted.ToFullString();
        var start = formatted.ParameterList.OpenBracketToken.SpanStart - formatted.FullSpan.Start;
        var end = formatted.SemicolonToken.Span.End - formatted.FullSpan.Start;

        // A literal that spans lines inside the parameter list, or an expression the formatter keeps on several lines, leaves
        // the indexer on several lines, so the fix cannot put it on one line and is not offered
        if (text.IndexOf('\n', start, end - start) >= 0)
        {
            return false;
        }

        return AttributeListCodeFixUtilities.KeepsBlankLinesBehindAttributeLists(indexerDeclaration, formatted);
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH5419ExpressionStyleGetOnlyIndexersShouldBeSingleLinedAnalyzer.DiagnosticId];

    /// <inheritdoc/>
    public sealed override FixAllProvider GetFixAllProvider()
    {
        return WellKnownFixAllProviders.BatchFixer;
    }

    #endregion // CodeFixProvider

    #region ReihitsuCodeFixProviderBase

    /// <inheritdoc/>
    protected override bool FormatsThroughFormatter => true;

    /// <inheritdoc/>
    protected sealed override async Task RegisterCodeFixesCoreAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

        if (root == null)
        {
            return;
        }

        foreach (var diagnostic in context.Diagnostics)
        {
            // Only offer the action when the formatter accepts the indexer, no comment, directive, or disabled text stands
            // inside a parameter list on several lines, no attribute list holds a blank line, a directive, disabled text, or
            // an own-line comment between its brackets, and formatting the joined indexer puts it on one line and keeps the
            // blank lines behind its attribute lists (RH5419-004); otherwise the fix would move user trivia, leave the
            // diagnostic unresolved, or change blank lines it does not own
            if (root.FindNode(diagnostic.Location.SourceSpan) is IndexerDeclarationSyntax { ExpressionBody: not null } indexerDeclaration
                && ReihitsuFormatter.FindUnsupportedSyntax(indexerDeclaration) == null
                && HoldsCommentDirectiveOrDisabledTextInWrappedParameterList(indexerDeclaration.ParameterList) == false
                && AttributeListCodeFixUtilities.AnyHoldsBlankLineDirectiveDisabledTextOrOwnLineCommentBetweenBrackets(indexerDeclaration) == false
                && FormatsOnOneLineKeepingBlankLines(root, indexerDeclaration, context.CancellationToken))
            {
                context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH5419Title,
                                                          token => ApplyCodeFixAsync(context.Document, indexerDeclaration, token),
                                                          nameof(RH5419ExpressionStyleGetOnlyIndexersShouldBeSingleLinedCodeFixProvider)),
                                        diagnostic);
            }
        }
    }

    #endregion // ReihitsuCodeFixProviderBase
}