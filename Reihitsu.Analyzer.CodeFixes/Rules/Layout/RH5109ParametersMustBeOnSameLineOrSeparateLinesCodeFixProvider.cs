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
/// Code fix provider for <see cref="RH5109ParametersMustBeOnSameLineOrSeparateLinesAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH5109ParametersMustBeOnSameLineOrSeparateLinesCodeFixProvider))]
public class RH5109ParametersMustBeOnSameLineOrSeparateLinesCodeFixProvider : CodeFixProvider
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
        var endOfLine = ReihitsuFormatterHelpers.DetectEndOfLine(root);
        var replacement = $"({parameters.First()},{endOfLine}{alignment}{string.Join($",{endOfLine}{alignment}", parameters.Skip(1))})";

        // The rebuilt text already places every parameter on its own line, aligned under the first parameter,
        // so it is the final layout for the guarded scope (the registration rejects parameter lists that carry
        // comments or directives). Return it directly instead of running the formatter over the owning member:
        // that oversized scope reformatted the unrelated member body and inherited the body-scope formatter
        // defects, while formatting the isolated parameter list mis-aligns its continuation lines.
        return document.WithText(sourceText.Replace(parameterList.Span, replacement));
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH5109ParametersMustBeOnSameLineOrSeparateLinesAnalyzer.DiagnosticId];

    /// <inheritdoc/>
    public sealed override FixAllProvider GetFixAllProvider()
    {
        return WellKnownFixAllProviders.BatchFixer;
    }

    /// <inheritdoc/>
    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

        if (root == null)
        {
            return;
        }

        foreach (var diagnostic in context.Diagnostics)
        {
            var parameterList = root.FindToken(diagnostic.Location.SourceSpan.Start).Parent?.FirstAncestorOrSelf<ParameterListSyntax>();

            if (parameterList == null
                || SyntaxNodeUtilities.HasCommentsOrDirectives(parameterList))
            {
                continue;
            }

            context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH5109Title,
                                                      token => ApplyCodeFixAsync(context.Document, diagnostic.Location.SourceSpan, token),
                                                      nameof(RH5109ParametersMustBeOnSameLineOrSeparateLinesCodeFixProvider)),
                                    diagnostic);
        }
    }

    #endregion // CodeFixProvider
}