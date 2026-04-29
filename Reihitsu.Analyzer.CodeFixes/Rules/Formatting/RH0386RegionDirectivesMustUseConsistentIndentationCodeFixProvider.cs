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
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.Core;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// Code fix provider for <see cref="RH0386RegionDirectivesMustUseConsistentIndentationAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH0386RegionDirectivesMustUseConsistentIndentationCodeFixProvider))]
public class RH0386RegionDirectivesMustUseConsistentIndentationCodeFixProvider : CodeFixProvider
{
    #region Methods

    /// <summary>
    /// Applies the code fix
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="directiveTrivia">Directive trivia</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated document</returns>
    private static async Task<Document> ApplyCodeFixAsync(Document document, SyntaxTrivia directiveTrivia, CancellationToken cancellationToken)
    {
        var syntaxRoot = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (syntaxRoot == null)
        {
            return document;
        }

        if (RegionDirectiveUtilities.TryFindMatchingDirective(syntaxRoot, directiveTrivia, out var matchingDirectiveTrivia) == false)
        {
            return document;
        }

        var regionTrivia = directiveTrivia.IsKind(SyntaxKind.RegionDirectiveTrivia)
                               ? directiveTrivia
                               : matchingDirectiveTrivia;
        var endRegionTrivia = directiveTrivia.IsKind(SyntaxKind.EndRegionDirectiveTrivia)
                                  ? directiveTrivia
                                  : matchingDirectiveTrivia;
        var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
        var expectedIndentation = RH0386RegionDirectivesMustUseConsistentIndentationAnalyzer.GetExpectedIndentation(syntaxRoot, sourceText, regionTrivia, endRegionTrivia);

        if (expectedIndentation == null)
        {
            return document;
        }

        var tokenUpdates = CreateTokenUpdates(regionTrivia, endRegionTrivia, expectedIndentation);
        var updatedRoot = syntaxRoot.ReplaceTokens(tokenUpdates.Keys,
                                                   (originalToken, rewrittenToken) => ApplyTokenUpdate(originalToken, rewrittenToken, tokenUpdates[originalToken]));

        return document.WithSyntaxRoot(updatedRoot);
    }

    /// <summary>
    /// Applies the configured indentation update to a token
    /// </summary>
    /// <param name="originalToken">Original token</param>
    /// <param name="rewrittenToken">Rewritten token</param>
    /// <param name="directiveIndentations">Directive indentations keyed by leading-trivia index</param>
    /// <returns>Updated token</returns>
    private static SyntaxToken ApplyTokenUpdate(SyntaxToken originalToken, SyntaxToken rewrittenToken, IReadOnlyDictionary<int, string> directiveIndentations)
    {
        return rewrittenToken.WithLeadingTrivia(RewriteLeadingTrivia(originalToken.LeadingTrivia, directiveIndentations));
    }

    /// <summary>
    /// Builds the indentation updates to apply to the affected tokens
    /// </summary>
    /// <param name="regionTrivia">Region trivia</param>
    /// <param name="endRegionTrivia">Endregion trivia</param>
    /// <param name="expectedIndentation">Expected indentation</param>
    /// <returns>Token updates</returns>
    private static Dictionary<SyntaxToken, IReadOnlyDictionary<int, string>> CreateTokenUpdates(SyntaxTrivia regionTrivia, SyntaxTrivia endRegionTrivia, string expectedIndentation)
    {
        var updates = new Dictionary<SyntaxToken, Dictionary<int, string>>();

        AddTokenUpdate(updates, regionTrivia.Token, regionTrivia, expectedIndentation);
        AddTokenUpdate(updates, endRegionTrivia.Token, endRegionTrivia, expectedIndentation);

        return updates.ToDictionary(pair => pair.Key, pair => (IReadOnlyDictionary<int, string>)pair.Value);
    }

    /// <summary>
    /// Adds an indentation update for a directive within a token
    /// </summary>
    /// <param name="updates">Updates map</param>
    /// <param name="token">Token</param>
    /// <param name="directiveTrivia">Directive trivia</param>
    /// <param name="expectedIndentation">Expected indentation</param>
    private static void AddTokenUpdate(Dictionary<SyntaxToken, Dictionary<int, string>> updates, SyntaxToken token, SyntaxTrivia directiveTrivia, string expectedIndentation)
    {
        if (TryGetDirectiveIndex(token.LeadingTrivia, directiveTrivia, out var directiveIndex) == false)
        {
            return;
        }

        if (updates.TryGetValue(token, out var tokenUpdates) == false)
        {
            tokenUpdates = [];
            updates[token] = tokenUpdates;
        }

        tokenUpdates[directiveIndex] = expectedIndentation;
    }

    /// <summary>
    /// Determines whether the given trivia marks the start of a new line
    /// </summary>
    /// <param name="trivia">Trivia</param>
    /// <returns><see langword="true"/> if line boundary</returns>
    private static bool IsLineBoundary(SyntaxTrivia trivia)
    {
        return trivia.IsKind(SyntaxKind.EndOfLineTrivia)
               || (trivia.HasStructure && trivia.GetStructure() is DirectiveTriviaSyntax);
    }

    /// <summary>
    /// Preserves a BOM prefix when present in removed whitespace
    /// </summary>
    /// <param name="rewrittenTrivia">Trivia being built</param>
    /// <param name="lineTrivia">Original line trivia</param>
    private static void PreserveBom(List<SyntaxTrivia> rewrittenTrivia, IEnumerable<SyntaxTrivia> lineTrivia)
    {
        foreach (var trivia in lineTrivia)
        {
            if (trivia.IsKind(SyntaxKind.WhitespaceTrivia) && trivia.ToFullString().Contains('\uFEFF'))
            {
                rewrittenTrivia.Add(SyntaxFactory.Whitespace("\uFEFF"));

                break;
            }
        }
    }

    /// <summary>
    /// Rewrites token leading trivia so the specified directives use the requested indentation
    /// </summary>
    /// <param name="leadingTrivia">Leading trivia</param>
    /// <param name="directiveIndentations">Directive indentations keyed by leading-trivia index</param>
    /// <returns>Updated leading trivia</returns>
    private static SyntaxTriviaList RewriteLeadingTrivia(SyntaxTriviaList leadingTrivia, IReadOnlyDictionary<int, string> directiveIndentations)
    {
        var rewrittenTrivia = new List<SyntaxTrivia>();
        var cursor = 0;

        foreach (var directiveIndex in directiveIndentations.Keys.OrderBy(index => index))
        {
            var lineStartIndex = directiveIndex;

            while (lineStartIndex > cursor && IsLineBoundary(leadingTrivia[lineStartIndex - 1]) == false)
            {
                lineStartIndex--;
            }

            for (var triviaIndex = cursor; triviaIndex < lineStartIndex; triviaIndex++)
            {
                rewrittenTrivia.Add(leadingTrivia[triviaIndex]);
            }

            var lineTrivia = Enumerable.Range(lineStartIndex, directiveIndex - lineStartIndex)
                                       .Select(index => leadingTrivia[index])
                                       .ToArray();

            PreserveBom(rewrittenTrivia, lineTrivia);

            if (string.IsNullOrEmpty(directiveIndentations[directiveIndex]) == false)
            {
                rewrittenTrivia.Add(SyntaxFactory.Whitespace(directiveIndentations[directiveIndex]));
            }

            foreach (var trivia in lineTrivia)
            {
                if (trivia.IsKind(SyntaxKind.WhitespaceTrivia) == false)
                {
                    rewrittenTrivia.Add(trivia);
                }
            }

            rewrittenTrivia.Add(leadingTrivia[directiveIndex]);
            cursor = directiveIndex + 1;
        }

        for (var triviaIndex = cursor; triviaIndex < leadingTrivia.Count; triviaIndex++)
        {
            rewrittenTrivia.Add(leadingTrivia[triviaIndex]);
        }

        return SyntaxFactory.TriviaList(rewrittenTrivia);
    }

    /// <summary>
    /// Tries to find the index of a directive trivia inside a token's leading trivia
    /// </summary>
    /// <param name="leadingTrivia">Leading trivia</param>
    /// <param name="directiveTrivia">Directive trivia</param>
    /// <param name="directiveIndex">Directive index</param>
    /// <returns><see langword="true"/> if found</returns>
    private static bool TryGetDirectiveIndex(SyntaxTriviaList leadingTrivia, SyntaxTrivia directiveTrivia, out int directiveIndex)
    {
        for (directiveIndex = 0; directiveIndex < leadingTrivia.Count; directiveIndex++)
        {
            if (leadingTrivia[directiveIndex] == directiveTrivia)
            {
                return true;
            }
        }

        directiveIndex = -1;

        return false;
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH0386RegionDirectivesMustUseConsistentIndentationAnalyzer.DiagnosticId];

    /// <inheritdoc/>
    public sealed override FixAllProvider GetFixAllProvider()
    {
        return WellKnownFixAllProviders.BatchFixer;
    }

    /// <inheritdoc/>
    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

        if (syntaxRoot == null)
        {
            return;
        }

        foreach (var diagnostic in context.Diagnostics)
        {
            var directiveTrivia = syntaxRoot.FindTrivia(diagnostic.Location.SourceSpan.Start);

            if (directiveTrivia.RawKind == (int)SyntaxKind.RegionDirectiveTrivia
                || directiveTrivia.RawKind == (int)SyntaxKind.EndRegionDirectiveTrivia)
            {
                context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH0386Title,
                                                          token => ApplyCodeFixAsync(context.Document, directiveTrivia, token),
                                                          nameof(RH0386RegionDirectivesMustUseConsistentIndentationCodeFixProvider)),
                                        diagnostic);
            }
        }
    }

    #endregion // CodeFixProvider
}