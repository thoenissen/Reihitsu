using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Formatter;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Layout;

/// <summary>
/// Code fix provider for <see cref="RH5401ExpressionStyleGetOnlyPropertiesShouldBeSingleLinedAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH5401ExpressionStyleGetOnlyPropertiesShouldBeSingleLinedCodeFixProvider))]
public class RH5401ExpressionStyleGetOnlyPropertiesShouldBeSingleLinedCodeFixProvider : CodeFixProvider
{
    #region Methods

    /// <summary>
    /// Applies the code fix
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="propertyDeclaration">Property declaration</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated document</returns>
    private static async Task<Document> ApplyCodeFixAsync(Document document, PropertyDeclarationSyntax propertyDeclaration, CancellationToken cancellationToken)
    {
        return await ReihitsuFormatter.FormatNodeInDocumentAsync(document, propertyDeclaration, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Determines whether the formatter is able to collapse the property to a single line
    /// </summary>
    /// <param name="propertyDeclaration">Property declaration</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns><see langword="true"/> when formatting produces a single-lined property; otherwise, <see langword="false"/></returns>
    private static bool CanCollapse(PropertyDeclarationSyntax propertyDeclaration, CancellationToken cancellationToken)
    {
        var formatted = ReihitsuFormatter.FormatNode(propertyDeclaration, cancellationToken: cancellationToken);

        // For shapes the formatter keeps multi-lined (for example multi-line collection or switch expressions) the
        // result still spans several lines, so single-lining is impossible and the action must not be offered
        return formatted.ToString().IndexOf('\n') < 0;
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH5401ExpressionStyleGetOnlyPropertiesShouldBeSingleLinedAnalyzer.DiagnosticId];

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
            // Only offer the action when the formatter is actually able to collapse the property. For shapes the
            // formatter keeps multi-lined the formatting is a no-op, so registering would leave the diagnostic unresolved
            if (root.FindNode(diagnostic.Location.SourceSpan) is PropertyDeclarationSyntax propertyDeclaration
                && CanCollapse(propertyDeclaration, context.CancellationToken))
            {
                context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH5401Title,
                                                          token => ApplyCodeFixAsync(context.Document, propertyDeclaration, token),
                                                          nameof(RH5401ExpressionStyleGetOnlyPropertiesShouldBeSingleLinedCodeFixProvider)),
                                        diagnostic);
            }
        }
    }

    #endregion // CodeFixProvider
}