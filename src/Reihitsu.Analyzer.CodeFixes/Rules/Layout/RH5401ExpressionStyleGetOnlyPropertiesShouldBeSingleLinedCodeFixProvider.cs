using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.CodeFixes.Core;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Formatter;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Layout;

/// <summary>
/// Code fix provider for <see cref="RH5401ExpressionStyleGetOnlyPropertiesShouldBeSingleLinedAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH5401ExpressionStyleGetOnlyPropertiesShouldBeSingleLinedCodeFixProvider))]
public class RH5401ExpressionStyleGetOnlyPropertiesShouldBeSingleLinedCodeFixProvider : ReihitsuCodeFixProviderBase
{
    #region Methods

    /// <summary>
    /// Applies the code fix. A declaration that shares its line with another member is first separated from it, so that both
    /// stand on lines of their own (SHARED-FIX-007); then the formatter formats the declaration
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="propertyDeclaration">Property declaration</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated document</returns>
    private static Task<Document> ApplyCodeFixAsync(Document document, PropertyDeclarationSyntax propertyDeclaration, CancellationToken cancellationToken)
    {
        return BlankLineCodeFixUtilities.FormatMemberOnLinesOfItsOwnAsync(document, propertyDeclaration, cancellationToken);
    }

    /// <summary>
    /// Determines whether formatting the property completes the fix (RH5401-003, RH5401-004): the formatter puts the
    /// property's own code on one line — from its first token behind its attribute lists through its <c>;</c>, without the
    /// trivia in front of that token or behind the <c>;</c>; the attribute lists, and a comment between them and that first
    /// token, do not count — and it neither inserts nor removes a blank line in the gap behind each attribute list as written
    /// (<see cref="AttributeListCodeFixUtilities.StandsOnOneLineKeepingBlankLines"/>). For shapes the formatter keeps
    /// multi-lined, such as multi-line collection or switch expressions, single-lining is impossible
    /// </summary>
    /// <param name="propertyDeclaration">Property declaration</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns><see langword="true"/> when formatting puts the property's own code on one line and keeps the blank lines behind its attribute lists; otherwise, <see langword="false"/></returns>
    private static bool FormatsOnOneLineKeepingBlankLines(PropertyDeclarationSyntax propertyDeclaration, CancellationToken cancellationToken)
    {
        return ReihitsuFormatter.FormatNode(propertyDeclaration, cancellationToken: cancellationToken).Output is PropertyDeclarationSyntax formatted
               && AttributeListCodeFixUtilities.StandsOnOneLineKeepingBlankLines(propertyDeclaration, formatted);
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

    #endregion // CodeFixProvider

    #region ReihitsuCodeFixProviderBase

    /// <inheritdoc/>
    protected override bool FormatsThroughFormatter => true;

    /// <inheritdoc/>
    protected sealed override async Task RegisterCodeFixesCoreAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

        if (root == null)
        {
            return;
        }

        foreach (var diagnostic in context.Diagnostics)
        {
            // Only offer the action when no attribute list holds a blank line, a directive, disabled text, or an own-line
            // comment between its brackets, and when formatting puts the property's own code on one line and keeps the
            // blank lines behind its attribute lists; otherwise the fix would leave the diagnostic unresolved or change
            // blank lines it does not own
            if (root.FindNode(diagnostic.Location.SourceSpan) is PropertyDeclarationSyntax propertyDeclaration
                && ReihitsuFormatter.FindUnsupportedSyntax(propertyDeclaration) == null
                && AttributeListCodeFixUtilities.AnyHoldsBlankLineDirectiveDisabledTextOrOwnLineCommentBetweenBrackets(propertyDeclaration) == false
                && FormatsOnOneLineKeepingBlankLines(propertyDeclaration, context.CancellationToken))
            {
                context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH5401Title,
                                                          token => ApplyCodeFixAsync(context.Document, propertyDeclaration, token),
                                                          nameof(RH5401ExpressionStyleGetOnlyPropertiesShouldBeSingleLinedCodeFixProvider)),
                                        diagnostic);
            }
        }
    }

    #endregion // ReihitsuCodeFixProviderBase
}