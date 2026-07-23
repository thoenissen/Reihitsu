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

        var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
        var token = root.FindToken(diagnosticSpan.Start);
        var (leadingWhitespaceSpan, trailingWhitespaceSpan, hasTrailingLineBreak) = RH6002CommasMustBeSpacedCorrectlyAnalyzer.AnalyzeSpacing(token, sourceText);

        var textChanges = ImmutableArray.CreateBuilder<TextChange>(2);

        if (leadingWhitespaceSpan.IsEmpty == false)
        {
            textChanges.Add(new TextChange(leadingWhitespaceSpan, string.Empty));
        }

        if (hasTrailingLineBreak == false
            && RH6002CommasMustBeSpacedCorrectlyAnalyzer.HasExactlyOneTrailingSpace(trailingWhitespaceSpan, sourceText) == false)
        {
            textChanges.Add(new TextChange(trailingWhitespaceSpan, " "));
        }

        if (textChanges.Count == 0)
        {
            return document;
        }

        return document.WithText(sourceText.WithChanges(textChanges));
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