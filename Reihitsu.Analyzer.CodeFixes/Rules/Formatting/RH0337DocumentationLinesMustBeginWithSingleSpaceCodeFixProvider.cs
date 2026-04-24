using System;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.Core;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// Code fix provider for <see cref="RH0337DocumentationLinesMustBeginWithSingleSpaceAnalyzer"/>.
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH0337DocumentationLinesMustBeginWithSingleSpaceCodeFixProvider))]
public class RH0337DocumentationLinesMustBeginWithSingleSpaceCodeFixProvider : CodeFixProvider
{
    #region Methods

    /// <summary>
    /// Applies the code fix.
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="diagnosticSpan">Diagnostic span</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated document</returns>
    private static async Task<Document> ApplyCodeFixAsync(Document document, TextSpan diagnosticSpan, CancellationToken cancellationToken)
    {
        var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
        var line = sourceText.Lines.GetLineFromPosition(diagnosticSpan.Start);
        var lineText = FormattingTextAnalysisUtilities.GetLineText(sourceText, line);
        var trimmed = lineText.TrimStart();
        var indentation = lineText.Substring(0, lineText.Length - trimmed.Length);
        var suffix = trimmed.StartsWith("///", StringComparison.Ordinal) ? trimmed.Substring(3).TrimStart() : trimmed;
        var replacement = indentation + "///" + (suffix.Length == 0 ? string.Empty : " " + suffix);

        return document.WithText(sourceText.Replace(TextSpan.FromBounds(line.Start, line.End), replacement));
    }

    /// <summary>
    /// Gets the leading whitespace for the specified line.
    /// </summary>
    /// <param name="lineText">Line text</param>
    /// <returns>The leading whitespace</returns>
    private static string GetIndentation(string lineText)
    {
        var length = 0;

        while (length < lineText.Length
               && char.IsWhiteSpace(lineText[length]))
        {
            length++;
        }

        return lineText.Substring(0, length);
    }

    /// <summary>
    /// Wraps the specified statement with braces.
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="diagnosticSpan">Diagnostic span</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated document</returns>
    private static async Task<Document> WrapStatementWithBracesAsync(Document document, TextSpan diagnosticSpan, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (root == null)
        {
            return document;
        }

        var statement = root.FindNode(diagnosticSpan).FirstAncestorOrSelf<StatementSyntax>();

        if (statement == null)
        {
            return document;
        }

        var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
        var statementLine = sourceText.Lines.GetLineFromPosition(statement.SpanStart);
        var statementIndentation = GetIndentation(FormattingTextAnalysisUtilities.GetLineText(sourceText, statementLine));
        var parentNode = statement.Parent switch
                         {
                             ElseClauseSyntax elseClause => elseClause,
                             StatementSyntax parentStatement => parentStatement,
                             _ => statement.Parent,
                         };
        var parentLine = sourceText.Lines.GetLineFromPosition(parentNode?.SpanStart ?? statement.SpanStart);
        var parentIndentation = GetIndentation(FormattingTextAnalysisUtilities.GetLineText(sourceText, parentLine));
        var statementText = sourceText.ToString(statement.Span);
        var replacement = "{" + Environment.NewLine + statementIndentation + statementText + Environment.NewLine + parentIndentation + "}";

        return document.WithText(sourceText.Replace(statement.Span, replacement));
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH0337DocumentationLinesMustBeginWithSingleSpaceAnalyzer.DiagnosticId];

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
            context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH0337Title,
                                                      token => ApplyCodeFixAsync(context.Document, diagnostic.Location.SourceSpan, token),
                                                      nameof(RH0337DocumentationLinesMustBeginWithSingleSpaceCodeFixProvider)),
                                    diagnostic);
        }

        return Task.CompletedTask;
    }

    #endregion // CodeFixProvider
}