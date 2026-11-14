using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Core;
using Reihitsu.Core.Enumerations;
using Reihitsu.Formatter.Utilities;

namespace Reihitsu.Analyzer.CodeFixes.Base;

/// <summary>
/// Base code-fix provider for target-based attribute list-shape rules
/// </summary>
public abstract class TargetAttributeListShapeCodeFixProviderBase : CodeFixProvider
{
    #region Properties

    /// <summary>
    /// Diagnostic ID handled by this provider
    /// </summary>
    protected abstract string DiagnosticId { get; }

    /// <summary>
    /// Target handled by this provider
    /// </summary>
    protected abstract AttributeTargets Target { get; }

    /// <summary>
    /// List-shape mode
    /// </summary>
    protected abstract TargetAttributeListShapeMode ListShapeMode { get; }

    /// <summary>
    /// Code-fix title
    /// </summary>
    protected abstract string CodeFixTitle { get; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Resolves the effective list-shape mode for an attribute list
    /// </summary>
    /// <param name="attributeList">Attribute list</param>
    /// <returns>Effective list-shape mode</returns>
    protected virtual TargetAttributeListShapeMode ResolveListShapeMode(AttributeListSyntax attributeList)
    {
        return ListShapeMode;
    }

    /// <summary>
    /// Determines whether the attribute list is in scope for this code-fix provider
    /// </summary>
    /// <param name="attributeList">Attribute list</param>
    /// <param name="target">Resolved layout target (see <see cref="AttributeTargetUtilities.TryResolveLayoutTarget"/>)</param>
    /// <returns><see langword="true"/> when the attribute list should be processed</returns>
    protected virtual bool IsAttributeListInScope(AttributeListSyntax attributeList, AttributeTargets target)
    {
        return target == Target;
    }

    /// <summary>
    /// Applies split-list fix for a single attribute list. The reported list is replaced, in the text, by one list per
    /// attribute, and nothing else changes (SHARED-ATTR-017): the first list stays where the reported list starts, and each
    /// further list starts a new line, behind a copy of the whitespace at the start of the line on which the reported list
    /// starts. The line breaks between the lists are the document's line ending (SHARED-FIX-008)
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="attributeList">Attribute list</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated document</returns>
    private static async Task<Document> ApplySplitCodeFixAsync(Document document, AttributeListSyntax attributeList, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (root == null
            || attributeList.Attributes.Count <= 1
            || SyntaxNodeUtilities.InteriorContainsCommentOrDirective(attributeList))
        {
            return document;
        }

        var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
        var line = sourceText.Lines.GetLineFromPosition(attributeList.SpanStart);
        var lineIndentation = FormattingTextAnalysisUtilities.GetLeadingWhitespace(FormattingTextAnalysisUtilities.GetLineText(sourceText, line));
        var separator = ReihitsuFormatterHelpers.DetectEndOfLine(root) + lineIndentation;
        var target = attributeList.Target?.ToFullString() ?? string.Empty;
        var replacement = string.Join(separator, attributeList.Attributes.Select(attribute => $"[{target}{attribute}]"));

        return document.WithText(sourceText.Replace(attributeList.Span, replacement));
    }

    /// <summary>
    /// Determines whether a <c>{</c> token stands in front of the attribute list on the line on which the list starts and
    /// is still open there, because its <c>}</c> does not stand in front of the list on that line (SHARED-ATTR-022). The
    /// whitespace at the start of such a line is the indentation of the code that owns that <c>{</c> or of code further
    /// out, so a further list started behind it would stand at the wrong column, in front of the rest of that body. The
    /// scan walks the tokens in front of the list on its line backwards and counts the <c>}</c> tokens still waiting for
    /// their <c>{</c>; a <c>{</c> found while none is waiting is open at the list. A brace inside a literal, a comment, or
    /// disabled text is no token, and a missing brace has no width, so <see cref="SyntaxToken.GetPreviousToken"/> skips it;
    /// neither is seen
    /// </summary>
    /// <param name="attributeList">Attribute list</param>
    /// <returns><see langword="true"/> when the split fix is withheld for the list</returns>
    private static bool StartsBehindAnOpenBraceOnItsLine(AttributeListSyntax attributeList)
    {
        var tree = attributeList.SyntaxTree;
        var listLine = tree.GetLineSpan(attributeList.OpenBracketToken.Span).StartLinePosition.Line;
        var waitingCloseBraces = 0;

        var token = attributeList.OpenBracketToken.GetPreviousToken();

        while (token.IsKind(SyntaxKind.None) == false
               && tree.GetLineSpan(token.Span).StartLinePosition.Line == listLine)
        {
            if (token.IsKind(SyntaxKind.CloseBraceToken))
            {
                waitingCloseBraces++;
            }
            else if (token.IsKind(SyntaxKind.OpenBraceToken))
            {
                if (waitingCloseBraces == 0)
                {
                    return true;
                }

                waitingCloseBraces--;
            }

            token = token.GetPreviousToken();
        }

        return false;
    }

    /// <summary>
    /// Determines whether the split fix would copy or keep a line break it must not (SHARED-ATTR-023): a list with a target
    /// specifier whose first attribute starts on a later line than its <c>[</c>, or a list that holds a line break other
    /// than the document's line ending (SHARED-FIX-008)
    /// </summary>
    /// <param name="root">Syntax root</param>
    /// <param name="attributeList">Attribute list</param>
    /// <returns><see langword="true"/> when the split fix is withheld for the list</returns>
    private static bool HoldsLineBreakTheSplitCannotWrite(SyntaxNode root, AttributeListSyntax attributeList)
    {
        var tree = attributeList.SyntaxTree;

        if (attributeList.Target != null
            && tree.GetLineSpan(attributeList.OpenBracketToken.Span).StartLinePosition.Line != tree.GetLineSpan(attributeList.Attributes[0].Span).StartLinePosition.Line)
        {
            return true;
        }

        return LineEndingUtilities.HoldsOtherLineBreak(tree.GetText(), attributeList.Span, ReihitsuFormatterHelpers.DetectEndOfLine(root));
    }

    /// <summary>
    /// Applies merged-list fix for all matching sibling lists
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="attributeList">Attribute list from diagnostic span</param>
    /// <param name="listShapeMode">List-shape mode to merge</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated document</returns>
    private async Task<Document> ApplyMergeCodeFixAsync(Document document, AttributeListSyntax attributeList, TargetAttributeListShapeMode listShapeMode, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        var owner = attributeList.Parent;

        if (root == null || owner == null)
        {
            return document;
        }

        var lists = AttributeTargetUtilities.GetAttributeLists(owner);
        var matchingLists = lists.Where(list => AttributeTargetUtilities.TryResolveLayoutTarget(list, out var target)
                                                && IsAttributeListInScope(list, target)
                                                && AttributeTargetUtilities.HaveSameTarget(attributeList, list)
                                                && ResolveListShapeMode(list) == listShapeMode)
                                 .ToArray();

        if (AttributeTargetUtilities.CanMergeAttributeListGroup(matchingLists) == false)
        {
            return document;
        }

        var updatedOwner = AttributeTargetUtilities.MergeAttributeListGroup(owner, matchingLists);
        var updatedRoot = root.ReplaceNode(owner, updatedOwner);

        return document.WithSyntaxRoot(updatedRoot);
    }

    /// <summary>
    /// Tries to get a fixable attribute list from a diagnostic
    /// </summary>
    /// <param name="root">Root</param>
    /// <param name="diagnostic">Diagnostic</param>
    /// <param name="attributeList">Resolved attribute list</param>
    /// <param name="listShapeMode">Resolved list-shape mode</param>
    /// <returns><see langword="true"/> when fix can be offered</returns>
    private bool TryGetFixableAttributeList(SyntaxNode root, Diagnostic diagnostic, out AttributeListSyntax attributeList, out TargetAttributeListShapeMode listShapeMode)
    {
        var diagnosticNode = root.FindToken(diagnostic.Location.SourceSpan.Start).Parent;
        listShapeMode = default;

        attributeList = diagnosticNode?.AncestorsAndSelf()
                                      .OfType<AttributeListSyntax>()
                                      .FirstOrDefault();

        if (attributeList == null
            || AttributeTargetUtilities.TryResolveLayoutTarget(attributeList, out var target) == false
            || IsAttributeListInScope(attributeList, target) == false)
        {
            return false;
        }

        listShapeMode = ResolveListShapeMode(attributeList);

        if (listShapeMode == TargetAttributeListShapeMode.SplitLists)
        {
            // Mirrors the analyzer base: the formatter keeps the lists of an inline auto-property accessor merged, so
            // splitting one would only be undone again. A list with a comment or directive inside is withheld
            // (SHARED-ATTR-019), and so are a list behind a brace on its line that is still open there
            // (SHARED-ATTR-022) and a list with a line break the split would copy or keep in another line ending
            // (SHARED-ATTR-023)
            return attributeList.Attributes.Count > 1
                   && SyntaxNodeUtilities.InteriorContainsCommentOrDirective(attributeList) == false
                   && AttributeTargetUtilities.IsAttributeListOnInlineAutoPropertyAccessor(attributeList) == false
                   && StartsBehindAnOpenBraceOnItsLine(attributeList) == false
                   && HoldsLineBreakTheSplitCannotWrite(root, attributeList) == false;
        }

        var owner = attributeList.Parent;

        if (owner == null)
        {
            return false;
        }

        var expectedListShapeMode = listShapeMode;
        var diagnosedList = attributeList;
        var matchingLists = AttributeTargetUtilities.GetAttributeLists(owner)
                                                    .Where(list => AttributeTargetUtilities.TryResolveLayoutTarget(list, out var siblingTarget)
                                                                   && IsAttributeListInScope(list, siblingTarget)
                                                                   && AttributeTargetUtilities.HaveSameTarget(diagnosedList, list)
                                                                   && ResolveListShapeMode(list) == expectedListShapeMode)
                                                    .ToArray();

        return AttributeTargetUtilities.CanMergeAttributeListGroup(matchingLists);
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [DiagnosticId];

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
            if (TryGetFixableAttributeList(root, diagnostic, out var attributeList, out var listShapeMode))
            {
                context.RegisterCodeFix(CodeAction.Create(CodeFixTitle,
                                                          token => listShapeMode == TargetAttributeListShapeMode.SplitLists
                                                                       ? ApplySplitCodeFixAsync(context.Document, attributeList, token)
                                                                       : ApplyMergeCodeFixAsync(context.Document, attributeList, listShapeMode, token),
                                                          GetType().Name),
                                        diagnostic);
            }
        }
    }

    #endregion // CodeFixProvider
}