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
using Reihitsu.Formatter;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// Code fix provider for <see cref="RH0381ParametersMustBeOnSameLineOrSeparateLinesAnalyzer"/>.
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH0381ParametersMustBeOnSameLineOrSeparateLinesCodeFixProvider))]
public class RH0381ParametersMustBeOnSameLineOrSeparateLinesCodeFixProvider : CodeFixProvider
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
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (root == null)
        {
            return document;
        }

        var parameterList = root.FindToken(diagnosticSpan.Start).Parent?.FirstAncestorOrSelf<ParameterListSyntax>();

        if (parameterList == null)
        {
            return document;
        }

        var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
        var parameters = parameterList.Parameters.Select(parameter => sourceText.ToString(parameter.Span));
        var firstParameter = parameterList.Parameters[0];
        var firstParameterLine = sourceText.Lines.GetLineFromPosition(firstParameter.SpanStart);
        var firstParameterColumn = firstParameter.SpanStart - firstParameterLine.Start;
        var alignment = new string(' ', firstParameterColumn);
        var replacement = "(" + parameters.First() + "," + Environment.NewLine + alignment + string.Join("," + Environment.NewLine + alignment, parameters.Skip(1)) + ")";
        var updatedDocument = document.WithText(sourceText.Replace(parameterList.Span, replacement));
        var updatedRoot = await updatedDocument.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        var updatedParameterList = updatedRoot?.FindToken(parameterList.SpanStart).Parent?.FirstAncestorOrSelf<ParameterListSyntax>();
        var formattedScope = updatedParameterList?.Parent ?? updatedParameterList;

        return formattedScope == null
                   ? updatedDocument
                   : await ReihitsuFormatter.FormatNodeInDocumentAsync(updatedDocument, formattedScope, cancellationToken).ConfigureAwait(false);
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
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH0381ParametersMustBeOnSameLineOrSeparateLinesAnalyzer.DiagnosticId];

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
            context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH0381Title,
                                                      token => ApplyCodeFixAsync(context.Document, diagnostic.Location.SourceSpan, token),
                                                      nameof(RH0381ParametersMustBeOnSameLineOrSeparateLinesCodeFixProvider)),
                                    diagnostic);
        }

        return Task.CompletedTask;
    }

    #endregion // CodeFixProvider
}