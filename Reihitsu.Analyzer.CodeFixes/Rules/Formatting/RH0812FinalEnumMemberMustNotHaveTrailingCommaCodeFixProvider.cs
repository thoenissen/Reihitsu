using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// Code fix provider for <see cref="RH0812FinalEnumMemberMustNotHaveTrailingCommaAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH0812FinalEnumMemberMustNotHaveTrailingCommaCodeFixProvider))]
public class RH0812FinalEnumMemberMustNotHaveTrailingCommaCodeFixProvider : CodeFixProvider
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
        var commaToken = root.FindToken(diagnosticSpan.Start);

        if (commaToken.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.CommaToken) == false)
        {
            return document;
        }

        return document.WithText(RemoveTrailingComma(sourceText, commaToken));
    }

    /// <summary>
    /// Removes the trailing comma while preserving comments attached to the final enum member
    /// </summary>
    /// <param name="sourceText">Source text</param>
    /// <param name="commaToken">Comma token</param>
    /// <returns>The updated source text</returns>
    private static SourceText RemoveTrailingComma(SourceText sourceText, SyntaxToken commaToken)
    {
        var commaLine = sourceText.Lines.GetLineFromPosition(commaToken.SpanStart);

        if (commaLine.ToString().Trim() == ",")
        {
            return sourceText.Replace(TextSpan.FromBounds(commaLine.Start, commaLine.EndIncludingLineBreak), string.Empty);
        }

        return sourceText.Replace(commaToken.Span, string.Empty);
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH0812FinalEnumMemberMustNotHaveTrailingCommaAnalyzer.DiagnosticId];

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
            context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH0812Title,
                                                      token => ApplyCodeFixAsync(context.Document, diagnostic.Location.SourceSpan, token),
                                                      nameof(RH0812FinalEnumMemberMustNotHaveTrailingCommaCodeFixProvider)),
                                    diagnostic);
        }

        return Task.CompletedTask;
    }

    #endregion // CodeFixProvider
}