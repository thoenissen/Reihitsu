using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Core;
using Reihitsu.Formatter.Utilities;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Documentation;

/// <summary>
/// Code fix provider for <see cref="RH8303ElementDocumentationHeaderMustBePrecededByBlankLineAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH8303ElementDocumentationHeaderMustBePrecededByBlankLineCodeFixProvider))]
public class RH8303ElementDocumentationHeaderMustBePrecededByBlankLineCodeFixProvider : CodeFixProvider
{
    #region Methods

    /// <summary>
    /// Applies the code fix: inserts an empty line above a documentation comment that starts its own line, and moves a
    /// comment written behind code onto a line of its own below an empty line
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
        var line = sourceText.Lines.GetLineFromPosition(diagnosticSpan.Start);
        var endOfLine = ReihitsuFormatterHelpers.DetectEndOfLine(root);
        var whiteSpaceStart = FormattingTextAnalysisUtilities.GetLeadingWhitespaceRunStart(sourceText, diagnosticSpan.Start, line.Start);

        if (whiteSpaceStart == line.Start)
        {
            return document.WithText(sourceText.Replace(TextSpan.FromBounds(line.Start, line.Start), endOfLine));
        }

        // A comment written behind code moves onto a line of its own, with one empty line above it, in the column of
        // the declaration it documents; the white space between the code and the comment goes away
        var declarationLine = sourceText.Lines.GetLineFromPosition(root.FindTrivia(diagnosticSpan.Start).Token.SpanStart);
        var indentation = FormattingTextAnalysisUtilities.GetLeadingWhitespace(FormattingTextAnalysisUtilities.GetLineText(sourceText, declarationLine));

        return document.WithText(sourceText.Replace(TextSpan.FromBounds(whiteSpaceStart, diagnosticSpan.Start), $"{endOfLine}{endOfLine}{indentation}"));
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH8303ElementDocumentationHeaderMustBePrecededByBlankLineAnalyzer.DiagnosticId];

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
            context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH8303Title,
                                                      token => ApplyCodeFixAsync(context.Document, diagnostic.Location.SourceSpan, token),
                                                      nameof(RH8303ElementDocumentationHeaderMustBePrecededByBlankLineCodeFixProvider)),
                                    diagnostic);
        }

        return Task.CompletedTask;
    }

    #endregion // CodeFixProvider
}