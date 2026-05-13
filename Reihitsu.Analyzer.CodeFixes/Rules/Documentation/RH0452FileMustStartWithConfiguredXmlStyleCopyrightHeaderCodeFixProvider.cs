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

namespace Reihitsu.Analyzer.Rules.Documentation;

/// <summary>
/// Code fix provider for <see cref="RH0452FileMustStartWithConfiguredXmlStyleCopyrightHeaderAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH0452FileMustStartWithConfiguredXmlStyleCopyrightHeaderCodeFixProvider))]
public class RH0452FileMustStartWithConfiguredXmlStyleCopyrightHeaderCodeFixProvider : CodeFixProvider
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

        var expectedHeader = CopyrightHeaderTemplateResolver.ResolveHeader(configuration, document.FilePath);

        if (string.IsNullOrWhiteSpace(expectedHeader))
        {
            return document;
        }

        var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
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
                       : Environment.NewLine;
        }

        return Environment.NewLine;
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

        foreach (var trivia in firstToken.LeadingTrivia)
        {
            if (trivia.IsKind(SyntaxKind.WhitespaceTrivia)
                || trivia.IsKind(SyntaxKind.EndOfLineTrivia)
                || trivia.IsKind(SyntaxKind.SingleLineCommentTrivia)
                || trivia.IsKind(SyntaxKind.MultiLineCommentTrivia))
            {
                endPosition = trivia.FullSpan.End;

                continue;
            }

            break;
        }

        return TextSpan.FromBounds(0, Math.Min(endPosition, sourceText.Length));
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH0452FileMustStartWithConfiguredXmlStyleCopyrightHeaderAnalyzer.DiagnosticId];

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
            context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH0452Title,
                                                      token => ApplyCodeFixAsync(context.Document, token),
                                                      nameof(RH0452FileMustStartWithConfiguredXmlStyleCopyrightHeaderCodeFixProvider)),
                                    diagnostic);
        }

        return Task.CompletedTask;
    }

    #endregion // CodeFixProvider
}