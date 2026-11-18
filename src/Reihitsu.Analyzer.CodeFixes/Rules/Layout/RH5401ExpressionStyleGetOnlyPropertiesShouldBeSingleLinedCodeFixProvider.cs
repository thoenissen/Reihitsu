using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.CodeFixes.Core;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Core;
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
    /// Determines whether the formatted property holds, in the gap behind each of its attribute lists as written, as many
    /// blank lines as that gap as written (RH5401-004). The gap runs from the <c>]</c> closing the list to the next token,
    /// with every comment, directive, and disabled text in between. The formatter may split a list into one list per
    /// attribute (FMT-STRUCT-028); the gap behind a list as written is then the gap behind the formatted list that holds its
    /// last attribute, and the gaps between the split lists have no counterpart as written
    /// </summary>
    /// <param name="propertyDeclaration">Property declaration as written</param>
    /// <param name="formatted">Property declaration as formatted</param>
    /// <returns><see langword="true"/> when every gap keeps its blank lines; otherwise, <see langword="false"/></returns>
    private static bool KeepsBlankLinesBehindAttributeLists(PropertyDeclarationSyntax propertyDeclaration, PropertyDeclarationSyntax formatted)
    {
        var formattedAttributes = formatted.AttributeLists.SelectMany(obj => obj.Attributes).ToList();

        if (formattedAttributes.Count != propertyDeclaration.AttributeLists.Sum(obj => obj.Attributes.Count))
        {
            return false;
        }

        var attributeIndex = -1;

        foreach (var attributeList in propertyDeclaration.AttributeLists)
        {
            if (attributeList.Attributes.Count == 0)
            {
                return false;
            }

            attributeIndex += attributeList.Attributes.Count;

            if (formattedAttributes[attributeIndex].Parent is not AttributeListSyntax formattedList)
            {
                return false;
            }

            var originalCloseBracket = attributeList.CloseBracketToken;
            var formattedCloseBracket = formattedList.CloseBracketToken;

            if (TokenGapAnalysis.Between(originalCloseBracket, originalCloseBracket.GetNextToken()).BlankLineCount
                != TokenGapAnalysis.Between(formattedCloseBracket, formattedCloseBracket.GetNextToken()).BlankLineCount)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Determines whether the formatter puts the property's own code on one line: from its first token behind its attribute
    /// lists through its <c>;</c>, without the trivia in front of that token or behind the <c>;</c> (RH5401-004). The
    /// attribute lists, and a comment between them and that first token, do not count. The formatted property must also keep
    /// the blank lines behind each attribute list as written, because the fix neither inserts nor removes one there
    /// (RH5401-004)
    /// </summary>
    /// <param name="propertyDeclaration">Property declaration</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns><see langword="true"/> when formatting puts the property's own code on one line and keeps the blank lines behind its attribute lists; otherwise, <see langword="false"/></returns>
    private static bool CanCollapse(PropertyDeclarationSyntax propertyDeclaration, CancellationToken cancellationToken)
    {
        if (ReihitsuFormatter.FormatNode(propertyDeclaration, cancellationToken: cancellationToken).Output is not PropertyDeclarationSyntax formatted)
        {
            return false;
        }

        var firstToken = formatted.AttributeLists.Count > 0
                             ? formatted.AttributeLists.Last().GetLastToken().GetNextToken()
                             : formatted.GetFirstToken();
        var text = formatted.ToFullString();
        var ownCode = text.Substring(firstToken.SpanStart - formatted.FullSpan.Start, formatted.Span.End - firstToken.SpanStart);

        // For shapes the formatter keeps multi-lined (for example multi-line collection or switch expressions) the
        // result still spans several lines, so single-lining is impossible and the action must not be offered
        return ownCode.IndexOf('\n') < 0
               && KeepsBlankLinesBehindAttributeLists(propertyDeclaration, formatted);
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
            // Only offer the action when the formatter is actually able to collapse the property. For shapes the
            // formatter keeps multi-lined the formatting is a no-op, so registering would leave the diagnostic unresolved
            if (root.FindNode(diagnostic.Location.SourceSpan) is PropertyDeclarationSyntax propertyDeclaration
                && ReihitsuFormatter.FindUnsupportedSyntax(propertyDeclaration) == null
                && CanCollapse(propertyDeclaration, context.CancellationToken))
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