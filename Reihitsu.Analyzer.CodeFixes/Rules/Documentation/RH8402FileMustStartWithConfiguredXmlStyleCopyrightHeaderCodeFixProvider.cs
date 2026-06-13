using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.Data;
using Reihitsu.Analyzer.Rules.Documentation;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Documentation;

/// <summary>
/// Code fix provider for <see cref="RH8402FileMustStartWithConfiguredXmlStyleCopyrightHeaderAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH8402FileMustStartWithConfiguredXmlStyleCopyrightHeaderCodeFixProvider))]
public class RH8402FileMustStartWithConfiguredXmlStyleCopyrightHeaderCodeFixProvider : CodeFixProvider
{
    #region Methods

    /// <summary>
    /// Applies the code fix
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated document</returns>
    private static async Task<Document> ApplyCodeFixAsync(Document document, CancellationToken cancellationToken)
    {
        var configuration = ConfigurationManager.GetConfiguration(document.Project.AnalyzerOptions.AdditionalFiles).Configuration?.Copyright;

        if (configuration == null)
        {
            return document;
        }

        var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
        var expectedHeader = CopyrightHeaderTemplateResolver.ResolveHeader(configuration, document.FilePath);

        if (string.IsNullOrWhiteSpace(expectedHeader))
        {
            return document;
        }

        expectedHeader = CopyrightHeaderTemplateResolver.NormalizeLineEndings(expectedHeader, sourceText);

        var syntaxRoot = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        var replaceSpan = GetLeadingHeaderSpan(sourceText, syntaxRoot);
        var replacementText = EnsureHeaderIsSeparatedFromCode(expectedHeader, sourceText, replaceSpan.End);

        return document.WithText(sourceText.WithChanges(new TextChange(replaceSpan, replacementText)));
    }

    /// <summary>
    /// Ensure header is separated from code
    /// </summary>
    /// <param name="header">Header</param>
    /// <param name="sourceText">Source text</param>
    /// <param name="contentStart">Content start position</param>
    /// <returns>Replacement text</returns>
    private static string EnsureHeaderIsSeparatedFromCode(string header, SourceText sourceText, int contentStart)
    {
        if (contentStart >= sourceText.Length
            || header.EndsWith("\n", StringComparison.Ordinal)
            || header.EndsWith("\r", StringComparison.Ordinal))
        {
            return header;
        }

        var contentStartsWithLineBreak = sourceText[contentStart] == '\r'
                                         || sourceText[contentStart] == '\n';

        if (contentStartsWithLineBreak)
        {
            return header;
        }

        return header + GetPreferredLineEnding(sourceText);
    }

    /// <summary>
    /// Get preferred line ending
    /// </summary>
    /// <param name="sourceText">Source text</param>
    /// <returns>Line ending</returns>
    private static string GetPreferredLineEnding(SourceText sourceText)
    {
        if (sourceText.Lines.Count > 1)
        {
            var firstLine = sourceText.Lines[0];
            var lineBreakLength = firstLine.SpanIncludingLineBreak.Length - firstLine.Span.Length;

            return lineBreakLength > 0
                       ? sourceText.GetSubText(new TextSpan(firstLine.End, lineBreakLength)).ToString()
                       : "\n";
        }

        return "\n";
    }

    /// <summary>
    /// Get leading header span
    /// </summary>
    /// <param name="sourceText">Source text</param>
    /// <param name="syntaxRoot">Syntax root</param>
    /// <returns>Leading header span</returns>
    private static TextSpan GetLeadingHeaderSpan(SourceText sourceText, SyntaxNode syntaxRoot)
    {
        if (syntaxRoot == null)
        {
            return new TextSpan(0, 0);
        }

        var firstToken = syntaxRoot.GetFirstToken(includeZeroWidth: true);

        if (firstToken.RawKind == 0)
        {
            return new TextSpan(0, 0);
        }

        var endPosition = 0;
        var hasComment = false;
        var hasCopyrightComment = false;

        foreach (var trivia in firstToken.LeadingTrivia)
        {
            if (trivia.IsKind(SyntaxKind.WhitespaceTrivia)
                || trivia.IsKind(SyntaxKind.EndOfLineTrivia))
            {
                endPosition = trivia.FullSpan.End;

                continue;
            }

            if (trivia.IsKind(SyntaxKind.SingleLineCommentTrivia)
                || trivia.IsKind(SyntaxKind.MultiLineCommentTrivia))
            {
                hasComment = true;
                hasCopyrightComment |= trivia.ToString().IndexOf("copyright", StringComparison.OrdinalIgnoreCase) >= 0;
                endPosition = trivia.FullSpan.End;

                continue;
            }

            break;
        }

        // Only replace the leading comment block when it is an existing copyright header. A leading
        // comment that is not a copyright header (for example a file note) must be preserved, so the
        // configured header is inserted before it instead of overwriting it.
        if (hasComment
            && hasCopyrightComment == false)
        {
            return new TextSpan(0, 0);
        }

        return TextSpan.FromBounds(0, Math.Min(endPosition, sourceText.Length));
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH8402FileMustStartWithConfiguredXmlStyleCopyrightHeaderAnalyzer.DiagnosticId];

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
            context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH8402Title,
                                                      token => ApplyCodeFixAsync(context.Document, token),
                                                      nameof(RH8402FileMustStartWithConfiguredXmlStyleCopyrightHeaderCodeFixProvider)),
                                    diagnostic);
        }

        return Task.CompletedTask;
    }

    #endregion // CodeFixProvider
}