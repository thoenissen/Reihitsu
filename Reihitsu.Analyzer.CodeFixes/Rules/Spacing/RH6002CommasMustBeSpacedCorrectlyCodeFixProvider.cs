using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.Rules.Spacing;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Spacing;

/// <summary>
/// Code fix provider for <see cref="RH6002CommasMustBeSpacedCorrectlyAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH6002CommasMustBeSpacedCorrectlyCodeFixProvider))]
public class RH6002CommasMustBeSpacedCorrectlyCodeFixProvider : CodeFixProvider
{
    #region Methods

    /// <summary>
    /// Applies the code fix
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

        var token = root.FindToken(diagnosticSpan.Start);
        var (previousToken, normalizedPreviousToken, normalizedCommaToken) = RH6002CommasMustBeSpacedCorrectlyAnalyzer.AnalyzeSpacing(token);

        var tokensToReplace = ImmutableArray.CreateBuilder<SyntaxToken>(2);

        if (previousToken != normalizedPreviousToken)
        {
            tokensToReplace.Add(previousToken);
        }

        if (token != normalizedCommaToken)
        {
            tokensToReplace.Add(token);
        }

        if (tokensToReplace.Count == 0)
        {
            return document;
        }

        var updatedRoot = root.ReplaceTokens(tokensToReplace,
                                             (originalToken, _) => originalToken == previousToken
                                                                       ? normalizedPreviousToken
                                                                       : normalizedCommaToken);

        return document.WithSyntaxRoot(updatedRoot);
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH6002CommasMustBeSpacedCorrectlyAnalyzer.DiagnosticId];

    /// <inheritdoc/>
    public sealed override FixAllProvider GetFixAllProvider()
    {
        return WellKnownFixAllProviders.BatchFixer;
    }

    /// <inheritdoc/>
    public sealed override Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        foreach (var diagnostic in context.Diagnostics)
        {
            context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH6002Title,
                                                      token => ApplyCodeFixAsync(context.Document, diagnostic.Location.SourceSpan, token),
                                                      nameof(RH6002CommasMustBeSpacedCorrectlyCodeFixProvider)),
                                    diagnostic);
        }

        return Task.CompletedTask;
    }

    #endregion // CodeFixProvider
}