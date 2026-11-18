using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.CodeFixes.Core;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Formatter;
using Reihitsu.Formatter.Utilities;

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
    /// stand on lines of their own (SHARED-FIX-007); then the formatter formats the declaration, and the blank lines behind
    /// its attribute lists are kept as written (RH5401-003)
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="propertyDeclaration">Property declaration</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated document</returns>
    private static Task<Document> ApplyCodeFixAsync(Document document, PropertyDeclarationSyntax propertyDeclaration, CancellationToken cancellationToken)
    {
        return BlankLineCodeFixUtilities.FormatMemberOnLinesOfItsOwnAsync(document, propertyDeclaration, FormatKeepingAttributeBlankLinesAsync, cancellationToken);
    }

    /// <summary>
    /// Formats the property through the formatter and then gives every gap behind one of its attribute lists the blank lines
    /// that gap had as written (RH5401-003): the formatter inserts a blank line in front of a comment on its own line there
    /// (FMT-BLANK-011) and collapses several blank lines into one, and the fix does neither. Only blank lines are written
    /// back; every other line keeps the line and column the formatter gave it. A gap whose comments, directives, or disabled
    /// text the formatter laid out differently from the text as written keeps the formatter's blank lines
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="node">Property declaration, which belongs to <paramref name="document"/></param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated document</returns>
    private static async Task<Document> FormatKeepingAttributeBlankLinesAsync(Document document, SyntaxNode node, CancellationToken cancellationToken)
    {
        var formattedDocument = await BlankLineCodeFixUtilities.FormatNodeInDocumentAsync(document, node, cancellationToken).ConfigureAwait(false);

        if (node is not PropertyDeclarationSyntax propertyDeclaration
            || propertyDeclaration.AttributeLists.Count == 0
            || await formattedDocument.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false) is not { } formattedRoot)
        {
            return formattedDocument;
        }

        // The formatter writes back the property's leading trivia and changes nothing in front of it, so the property's first
        // token keeps its position
        var formattedProperty = formattedRoot.FindToken(propertyDeclaration.SpanStart)
                                             .Parent
                                             ?.FirstAncestorOrSelf<PropertyDeclarationSyntax>();

        if (formattedProperty == null
            || formattedProperty.SpanStart != propertyDeclaration.SpanStart)
        {
            return formattedDocument;
        }

        var originalAttributes = propertyDeclaration.AttributeLists.SelectMany(obj => obj.Attributes).ToList();
        var formattedAttributes = formattedProperty.AttributeLists.SelectMany(obj => obj.Attributes).ToList();

        if (originalAttributes.Count != formattedAttributes.Count)
        {
            return formattedDocument;
        }

        var endOfLine = SyntaxFactory.EndOfLine(ReihitsuFormatterHelpers.DetectEndOfLine(formattedRoot));
        var replacements = new Dictionary<SyntaxToken, SyntaxToken>();
        var attributeIndex = -1;

        // The formatter may split an attribute list into one list per attribute (FMT-STRUCT-028); the gap behind a list as
        // written is then the gap behind the list that holds its last attribute, and the gaps between the split lists are new
        foreach (var attributeList in propertyDeclaration.AttributeLists)
        {
            attributeIndex += attributeList.Attributes.Count;

            if (attributeList.Attributes.Count == 0
                || formattedAttributes[attributeIndex].Parent is not AttributeListSyntax formattedList
                || formattedList.Attributes.Last() != formattedAttributes[attributeIndex])
            {
                continue;
            }

            var originalCloseBracket = attributeList.CloseBracketToken;
            var formattedCloseBracket = formattedList.CloseBracketToken;
            var formattedNextToken = formattedCloseBracket.GetNextToken();

            if (TryKeepBlankLines(originalCloseBracket.TrailingTrivia,
                                  originalCloseBracket.GetNextToken().LeadingTrivia,
                                  formattedCloseBracket.TrailingTrivia,
                                  formattedNextToken.LeadingTrivia,
                                  endOfLine,
                                  out var leadingTrivia))
            {
                replacements[formattedNextToken] = formattedNextToken.WithLeadingTrivia(leadingTrivia);
            }
        }

        return replacements.Count == 0
                   ? formattedDocument
                   : formattedDocument.WithSyntaxRoot(formattedRoot.ReplaceTokens(replacements.Keys, (original, _) => replacements[original]));
    }

    /// <summary>
    /// Gives a formatted gap between two tokens the blank lines the same gap had as written. A gap is divided into slots by
    /// its comments, directives, and disabled text; each slot of the formatted gap gets the blank lines of the slot as
    /// written. Blank lines are only added or removed in the leading trivia of the token behind the gap; the line break that
    /// ends the line in front of a slot is never one of them
    /// </summary>
    /// <param name="originalTrailingTrivia">Trailing trivia of the token in front of the gap, as written</param>
    /// <param name="originalLeadingTrivia">Leading trivia of the token behind the gap, as written</param>
    /// <param name="formattedTrailingTrivia">Trailing trivia of the token in front of the gap, as formatted</param>
    /// <param name="formattedLeadingTrivia">Leading trivia of the token behind the gap, as formatted</param>
    /// <param name="endOfLine">Line break to insert</param>
    /// <param name="leadingTrivia">The leading trivia of the token behind the gap with the blank lines as written</param>
    /// <returns><see langword="true"/> when the blank lines differ and can be written back; otherwise, <see langword="false"/></returns>
    private static bool TryKeepBlankLines(SyntaxTriviaList originalTrailingTrivia,
                                          SyntaxTriviaList originalLeadingTrivia,
                                          SyntaxTriviaList formattedTrailingTrivia,
                                          SyntaxTriviaList formattedLeadingTrivia,
                                          SyntaxTrivia endOfLine,
                                          out SyntaxTriviaList leadingTrivia)
    {
        leadingTrivia = formattedLeadingTrivia;

        var originalSlots = GetSlots(originalTrailingTrivia.Concat(originalLeadingTrivia).ToList());
        var formattedGap = formattedTrailingTrivia.Concat(formattedLeadingTrivia).ToList();
        var formattedSlots = GetSlots(formattedGap);

        if (originalSlots.Count != formattedSlots.Count)
        {
            return false;
        }

        var hasChanged = false;

        // Slots are edited from the last to the first, so the indices of the slots in front stay valid
        for (var slotIndex = formattedSlots.Count - 1; slotIndex >= 0; slotIndex--)
        {
            var formattedSlot = formattedSlots[slotIndex];
            var difference = originalSlots[slotIndex].BlankLineCount - formattedSlot.BlankLineCount;

            if (difference == 0)
            {
                continue;
            }

            // Every line break of a slot that starts its line ends a blank line; in a slot behind code or a comment on its
            // line, the first line break ends that line
            var blankLineBreaks = formattedSlot.LineBreakIndices.Skip(formattedSlot.StartsLine ? 0 : 1).ToList();

            if (difference < 0)
            {
                foreach (var lineBreakIndex in blankLineBreaks.Skip(blankLineBreaks.Count + difference).Reverse())
                {
                    if (lineBreakIndex < formattedTrailingTrivia.Count)
                    {
                        return false;
                    }

                    formattedGap.RemoveAt(lineBreakIndex);

                    if (lineBreakIndex - 1 >= formattedTrailingTrivia.Count
                        && formattedGap[lineBreakIndex - 1].IsKind(SyntaxKind.WhitespaceTrivia))
                    {
                        formattedGap.RemoveAt(lineBreakIndex - 1);
                    }
                }
            }
            else
            {
                if (formattedSlot.StartsLine == false
                    && formattedSlot.LineBreakIndices.Count == 0)
                {
                    return false;
                }

                var insertIndex = formattedSlot.StartsLine
                                      ? formattedSlot.Start
                                      : formattedSlot.LineBreakIndices[0] + 1;

                if (insertIndex < formattedTrailingTrivia.Count)
                {
                    return false;
                }

                formattedGap.InsertRange(insertIndex, Enumerable.Repeat(endOfLine, difference));
            }

            hasChanged = true;
        }

        leadingTrivia = SyntaxFactory.TriviaList(formattedGap.Skip(formattedTrailingTrivia.Count));

        return hasChanged;
    }

    /// <summary>
    /// Divides a gap into the slots its comments, directives, and disabled text separate
    /// </summary>
    /// <param name="gap">Trivia of the gap, from the token in front of it to the token behind it</param>
    /// <returns>The slots, in order</returns>
    private static List<(int Start, bool StartsLine, List<int> LineBreakIndices, int BlankLineCount)> GetSlots(IReadOnlyList<SyntaxTrivia> gap)
    {
        var slots = new List<(int Start, bool StartsLine, List<int> LineBreakIndices, int BlankLineCount)>();
        var start = 0;
        var startsLine = false;
        var lineBreakIndices = new List<int>();

        for (var triviaIndex = 0; triviaIndex <= gap.Count; triviaIndex++)
        {
            if (triviaIndex < gap.Count
                && gap[triviaIndex].IsKind(SyntaxKind.WhitespaceTrivia))
            {
                continue;
            }

            if (triviaIndex < gap.Count
                && gap[triviaIndex].IsKind(SyntaxKind.EndOfLineTrivia))
            {
                lineBreakIndices.Add(triviaIndex);

                continue;
            }

            slots.Add((start, startsLine, lineBreakIndices, Math.Max(0, lineBreakIndices.Count - (startsLine ? 0 : 1))));

            if (triviaIndex < gap.Count)
            {
                // A directive, a single-line documentation comment, and disabled text end with the line break of their
                // last line, so the slot behind them starts its line
                start = triviaIndex + 1;
                startsLine = gap[triviaIndex].ToFullString().EndsWith("\n", StringComparison.Ordinal);
                lineBreakIndices = [];
            }
        }

        return slots;
    }

    /// <summary>
    /// Determines whether the formatter puts the property's own code on one line: from its first token behind its attribute
    /// lists through its <c>;</c>, without the trivia in front of that token or behind the <c>;</c> (RH5401-004). The
    /// attribute lists, and a comment between them and that first token, do not count
    /// </summary>
    /// <param name="propertyDeclaration">Property declaration</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns><see langword="true"/> when formatting puts the property's own code on one line; otherwise, <see langword="false"/></returns>
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
        return ownCode.IndexOf('\n') < 0;
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