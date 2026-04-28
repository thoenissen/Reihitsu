using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Text;

namespace Reihitsu.Analyzer.Rules.Documentation;

/// <summary>
/// Code fix provider for <see cref="RH0447XmlDocumentationElementsMustBeOnSeparateLinesAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH0447XmlDocumentationElementsMustBeOnSeparateLinesCodeFixProvider))]
public class RH0447XmlDocumentationElementsMustBeOnSeparateLinesCodeFixProvider : CodeFixProvider
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
        var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
        var affectedLine = sourceText.Lines.GetLineFromPosition(diagnosticSpan.Start);
        var replacementStart = diagnosticSpan.Start;

        while (replacementStart > affectedLine.Start
               && IsHorizontalWhitespace(sourceText[replacementStart - 1]))
        {
            replacementStart--;
        }

        var replacementSpan = TextSpan.FromBounds(replacementStart, diagnosticSpan.Start);
        var insertionText = GetLineBreak(sourceText, affectedLine) + GetDocumentationPrefix(sourceText, affectedLine);

        return document.WithText(sourceText.Replace(replacementSpan, insertionText));
    }

    /// <summary>
    /// Gets the line break sequence for the affected line
    /// </summary>
    /// <param name="sourceText">Source text</param>
    /// <param name="line">Affected line</param>
    /// <returns>Line break sequence</returns>
    private static string GetLineBreak(SourceText sourceText, TextLine line)
    {
        return line.EndIncludingLineBreak > line.End
                   ? sourceText.ToString(TextSpan.FromBounds(line.End, line.EndIncludingLineBreak))
                   : Environment.NewLine;
    }

    /// <summary>
    /// Gets the documentation prefix for the specified line
    /// </summary>
    /// <param name="sourceText">Source text</param>
    /// <param name="line">Line</param>
    /// <returns>Documentation prefix</returns>
    private static string GetDocumentationPrefix(SourceText sourceText, TextLine line)
    {
        var lineText = sourceText.ToString(line.Span);
        var elementIndex = lineText.IndexOf('<');

        return elementIndex >= 0 ? lineText.Substring(0, elementIndex) : string.Empty;
    }

    /// <summary>
    /// Determines whether the specified character is horizontal whitespace
    /// </summary>
    /// <param name="value">Character to inspect</param>
    /// <returns><see langword="true"/> if the character is a space or tab</returns>
    private static bool IsHorizontalWhitespace(char value)
    {
        return value == ' ' || value == '\t';
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH0447XmlDocumentationElementsMustBeOnSeparateLinesAnalyzer.DiagnosticId];

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
            context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH0447Title,
                                                      token => ApplyCodeFixAsync(context.Document, diagnostic.Location.SourceSpan, token),
                                                      nameof(RH0447XmlDocumentationElementsMustBeOnSeparateLinesCodeFixProvider)),
                                    diagnostic);
        }

        return Task.CompletedTask;
    }

    #endregion // CodeFixProvider
}