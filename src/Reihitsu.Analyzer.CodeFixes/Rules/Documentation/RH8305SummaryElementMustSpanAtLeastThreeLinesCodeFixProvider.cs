using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;

using Reihitsu.Analyzer.CodeFixes.Core;
using Reihitsu.Analyzer.Rules.Documentation;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Documentation;

/// <summary>
/// Code fix provider for <see cref="RH8305SummaryElementMustSpanAtLeastThreeLinesAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH8305SummaryElementMustSpanAtLeastThreeLinesCodeFixProvider))]
public class RH8305SummaryElementMustSpanAtLeastThreeLinesCodeFixProvider : CodeFixProvider
{
    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH8305SummaryElementMustSpanAtLeastThreeLinesAnalyzer.DiagnosticId];

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

        var sourceText = await context.Document.GetTextAsync(context.CancellationToken).ConfigureAwait(false);

        foreach (var diagnostic in context.Diagnostics.Where(obj => DocumentationCommentCodeFixUtilities.CanLayOutElement(root, sourceText, obj.Location.SourceSpan)))
        {
            context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH8305Title,
                                                      token => DocumentationCommentCodeFixUtilities.LayOutElementAsync(context.Document, diagnostic.Location.SourceSpan, token),
                                                      nameof(RH8305SummaryElementMustSpanAtLeastThreeLinesCodeFixProvider)),
                                    diagnostic);
        }
    }

    #endregion // CodeFixProvider
}