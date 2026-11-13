using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.Rules.Layout;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Layout;

/// <summary>
/// Code fix provider for <see cref="RH5204IndentationMustUseFourSpacesPerScopeLevelAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH5204IndentationMustUseFourSpacesPerScopeLevelCodeFixProvider))]
public class RH5204IndentationMustUseFourSpacesPerScopeLevelCodeFixProvider : CodeFixProvider
{
    #region Methods

    /// <summary>
    /// Applies the code fix: the whitespace in front of the reported line's first token, comment, or directive is replaced
    /// with the indentation the analyzer expects for that line, and nothing else changes (RH5204-006). The analyzer owns
    /// the single indentation policy, so reusing its expected-indentation map guarantees the fix writes exactly the column
    /// the analyzer expects. A run of <c>///</c> lines is one documentation comment with one diagnostic, so every line of
    /// the run gets the indentation of its first line; the later lines of a delimited comment keep their columns
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="diagnostic">Diagnostic</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated document</returns>
    private static async Task<Document> ApplyCodeFixAsync(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (root == null)
        {
            return document;
        }

        var diagnosticLine = diagnostic.Location.GetLineSpan().StartLinePosition.Line;
        var expectedIndentationByLine = RH5204IndentationMustUseFourSpacesPerScopeLevelAnalyzer.BuildExpectedIndentationMap(root);

        if (expectedIndentationByLine.TryGetValue(diagnosticLine, out var expectation) == false)
        {
            return document;
        }

        var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
        var lastLine = diagnosticLine;
        var trivia = root.FindTrivia(diagnostic.Location.SourceSpan.Start);

        if (trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia))
        {
            lastLine = sourceText.Lines.GetLineFromPosition(trivia.FullSpan.End - 1).LineNumber;
        }

        var changes = new List<TextChange>();

        for (var lineNumber = diagnosticLine; lineNumber <= lastLine; lineNumber++)
        {
            changes.Add(CreateIndentationChange(sourceText.Lines[lineNumber], sourceText, expectation.Indentation));
        }

        return document.WithText(sourceText.WithChanges(changes));
    }

    /// <summary>
    /// Creates the change that replaces the whitespace at the start of a line with the given number of spaces
    /// </summary>
    /// <param name="line">Line</param>
    /// <param name="sourceText">Source text</param>
    /// <param name="indentation">Number of spaces</param>
    /// <returns>The text change</returns>
    private static TextChange CreateIndentationChange(TextLine line, SourceText sourceText, int indentation)
    {
        var start = line.Start;

        while (start < line.End && sourceText[start] == '\uFEFF')
        {
            start++;
        }

        var end = start;

        while (end < line.End && char.IsWhiteSpace(sourceText[end]))
        {
            end++;
        }

        return new TextChange(TextSpan.FromBounds(start, end), new string(' ', indentation));
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH5204IndentationMustUseFourSpacesPerScopeLevelAnalyzer.DiagnosticId];

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
            context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH5204Title,
                                                      token => ApplyCodeFixAsync(context.Document, diagnostic, token),
                                                      nameof(RH5204IndentationMustUseFourSpacesPerScopeLevelCodeFixProvider)),
                                    diagnostic);
        }

        return Task.CompletedTask;
    }

    #endregion // CodeFixProvider
}