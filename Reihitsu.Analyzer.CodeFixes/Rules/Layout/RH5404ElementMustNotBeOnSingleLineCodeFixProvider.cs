using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Core;
using Reihitsu.Formatter;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Layout;

/// <summary>
/// Code fix provider for <see cref="RH5404ElementMustNotBeOnSingleLineAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH5404ElementMustNotBeOnSingleLineCodeFixProvider))]
public class RH5404ElementMustNotBeOnSingleLineCodeFixProvider : CodeFixProvider
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

        var declaration = root.FindToken(diagnosticSpan.Start).Parent?.FirstAncestorOrSelf<BaseTypeDeclarationSyntax>();

        if (declaration == null)
        {
            return document;
        }

        var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
        var line = sourceText.Lines.GetLineFromPosition(declaration.SpanStart);
        var indentation = GetIndentation(FormattingTextAnalysisUtilities.GetLineText(sourceText, line));
        var content = sourceText.ToString(TextSpan.FromBounds(declaration.OpenBraceToken.Span.End, declaration.CloseBraceToken.SpanStart)).Trim();
        var memberIndentation = indentation + "    ";
        var endOfLine = ReihitsuFormatterHelpers.DetectEndOfLine(root);
        var replacement = content.Length == 0
                              ? $"{endOfLine}{indentation}{{{endOfLine}{indentation}}}"
                              : $"{endOfLine}{indentation}{{{endOfLine}{memberIndentation}{content}{endOfLine}{indentation}}}";
        var replacementStart = declaration.OpenBraceToken.SpanStart;

        while (replacementStart > declaration.SpanStart
               && char.IsWhiteSpace(sourceText[replacementStart - 1])
               && sourceText[replacementStart - 1] != '\r'
               && sourceText[replacementStart - 1] != '\n')
        {
            replacementStart--;
        }

        var replacementSpan = TextSpan.FromBounds(replacementStart, declaration.CloseBraceToken.Span.End);

        return document.WithText(sourceText.Replace(replacementSpan, replacement));
    }

    /// <summary>
    /// Gets the leading whitespace for the specified line
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

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH5404ElementMustNotBeOnSingleLineAnalyzer.DiagnosticId];

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
            context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH5404Title,
                                                      token => ApplyCodeFixAsync(context.Document, diagnostic.Location.SourceSpan, token),
                                                      nameof(RH5404ElementMustNotBeOnSingleLineCodeFixProvider)),
                                    diagnostic);
        }

        return Task.CompletedTask;
    }

    #endregion // CodeFixProvider
}