using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Analyzer.CodeFixes.Core;
using Reihitsu.Core;
using Reihitsu.Core.Enumerations;
using Reihitsu.Formatter.Utilities;

namespace Reihitsu.Analyzer.CodeFixes.Base;

/// <summary>
/// Base code-fix provider for target-based attribute placement rules
/// </summary>
public abstract class TargetAttributePlacementCodeFixProviderBase : CodeFixProvider
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
    /// Default placement mode
    /// </summary>
    protected abstract TargetAttributePlacementMode DefaultPlacementMode { get; }

    /// <summary>
    /// Code-fix title
    /// </summary>
    protected abstract string CodeFixTitle { get; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Resolves the placement mode for a specific attribute list
    /// </summary>
    /// <param name="attributeList">Attribute list</param>
    /// <returns>Placement mode</returns>
    protected virtual TargetAttributePlacementMode ResolvePlacementMode(AttributeListSyntax attributeList)
    {
        return DefaultPlacementMode;
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
    /// Applies the placement fix
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="attributeList">Attribute list</param>
    /// <param name="placementMode">Placement mode</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated document</returns>
    private static async Task<Document> ApplyCodeFixAsync(Document document, AttributeListSyntax attributeList, TargetAttributePlacementMode placementMode, CancellationToken cancellationToken)
    {
        if (AttributeTargetUtilities.TryGetTokenAfterAttributeList(attributeList, out var tokenAfter) == false)
        {
            return document;
        }

        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (root == null)
        {
            return document;
        }

        var closeBracket = attributeList.CloseBracketToken;
        var updatedCloseBracket = closeBracket;

        if (placementMode == TargetAttributePlacementMode.SeparateLine)
        {
            var endOfLine = ReihitsuFormatterHelpers.DetectEndOfLine(root);
            var trailingTrivia = SyntaxFactory.TriviaList(SyntaxFactory.EndOfLine(endOfLine));

            // The indentation of the inserted line is normally the declaration's nesting depth, which is
            // canonical: it self-corrects a stray extra space or tab on the attribute list's own line instead of
            // propagating it. That canonical value does not exist when an object initializer or anonymous object
            // sits between the declaration and its nearest brace scope, because neither is a level -
            // SyntaxIndentationUtilities' nesting-depth model has no way to turn "one more initializer" into the
            // anchor-derived column the formatter's own alignment contributors would place it at. In
            // that situation there is nothing to compute from, so the declaration's own first token - the
            // earliest attribute list already on it, or this list itself when it is the first one - is read
            // directly from the current source text instead, which also keeps a Fix All pass over several
            // attribute lists sharing one line correct, since every split member still aligns to the same first
            // token regardless of which list is being split
            var indentColumn = SyntaxIndentationUtilities.HasAnchorScopeAncestor(attributeList)
                                   ? SyntaxTokenPositionUtilities.GetColumn(attributeList.Parent.GetFirstToken())
                                   : SyntaxIndentationUtilities.ComputeBaseIndentLevel(attributeList) * SyntaxIndentationUtilities.IndentSize;

            if (indentColumn > 0)
            {
                trailingTrivia = trailingTrivia.Add(SyntaxFactory.Whitespace(new string(' ', indentColumn)));
            }

            updatedCloseBracket = closeBracket.WithTrailingTrivia(trailingTrivia);
        }
        else
        {
            updatedCloseBracket = closeBracket.WithTrailingTrivia(SyntaxFactory.Space);
        }

        var updatedTokenAfter = tokenAfter.WithLeadingTrivia(SyntaxFactory.TriviaList());
        var tokens = new List<SyntaxToken>
                     {
                         closeBracket,
                         tokenAfter
                     };
        var openBracket = attributeList.OpenBracketToken;
        var previousToken = openBracket.GetPreviousToken();

        // A list that starts a member behind another member on the same line gets a line of its own as well, in the
        // column of the declaration below it (SHARED-FIX-007, shared line)
        var breaksBefore = placementMode == TargetAttributePlacementMode.SeparateLine
                           && attributeList.Parent is MemberDeclarationSyntax member
                           && member.GetFirstToken() == openBracket
                           && LineLayoutCodeFixUtilities.SharesLineWithPreviousSiblingMember(member);

        if (breaksBefore)
        {
            tokens.Add(openBracket);
            tokens.Add(previousToken);
        }

        var updatedRoot = root.ReplaceTokens(tokens,
                                             (original, rewritten) =>
                                             {
                                                 if (original == closeBracket)
                                                 {
                                                     return updatedCloseBracket;
                                                 }

                                                 if (original == tokenAfter)
                                                 {
                                                     return updatedTokenAfter;
                                                 }

                                                 return original == openBracket
                                                            ? rewritten.WithLeadingTrivia(updatedCloseBracket.TrailingTrivia.Skip(1))
                                                            : rewritten.WithTrailingTrivia(LineLayoutCodeFixUtilities.EndWithLineBreak(rewritten.TrailingTrivia, updatedCloseBracket.TrailingTrivia[0]));
                                             });

        return document.WithSyntaxRoot(updatedRoot);
    }

    /// <summary>
    /// Determines whether an enclosing scope owns a brace the parser could not find. Such a scope silently drops
    /// out of <see cref="SyntaxIndentationUtilities.ComputeBaseIndentLevel"/>'s count instead of raising an error,
    /// which would understate the level-fallback indentation on exactly the transient, mid-edit documents where an
    /// IDE offers "Fix all in document" most often. This is unrelated to whether the level fallback is actually
    /// taken for a given attribute list, so the guard stays unconditional rather than trying to predict it. Its
    /// node-kind set is a private mirror of <see cref="SyntaxIndentationUtilities"/>'s internal
    /// <c>IsIndentingAncestor</c> brace kinds and must be kept in parity with it - this repository has already
    /// let such a mirror drift out of sync once, so this guard checks parity explicitly rather than assuming it
    /// </summary>
    /// <param name="attributeList">Attribute list</param>
    /// <returns><see langword="true"/> when an enclosing scope has a missing brace</returns>
    private static bool HasIncompleteEnclosingScope(AttributeListSyntax attributeList)
    {
        for (var ancestor = attributeList.Parent; ancestor != null; ancestor = ancestor.Parent)
        {
            var braces = ancestor switch
                         {
                             BlockSyntax block => (Open: block.OpenBraceToken, Close: block.CloseBraceToken),
                             TypeDeclarationSyntax typeDeclaration => (typeDeclaration.OpenBraceToken, typeDeclaration.CloseBraceToken),
                             NamespaceDeclarationSyntax namespaceDeclaration => (namespaceDeclaration.OpenBraceToken, namespaceDeclaration.CloseBraceToken),
                             EnumDeclarationSyntax enumDeclaration => (enumDeclaration.OpenBraceToken, enumDeclaration.CloseBraceToken),
                             SwitchStatementSyntax switchStatement => (switchStatement.OpenBraceToken, switchStatement.CloseBraceToken),
                             AccessorListSyntax accessorList => (accessorList.OpenBraceToken, accessorList.CloseBraceToken),
                             InitializerExpressionSyntax initializer => (initializer.OpenBraceToken, initializer.CloseBraceToken),
                             AnonymousObjectCreationExpressionSyntax anonymousObject => (anonymousObject.OpenBraceToken, anonymousObject.CloseBraceToken),
                             _ => default((SyntaxToken Open, SyntaxToken Close)?)
                         };

            if (braces is { Open.IsMissing: true } or { Close.IsMissing: true })
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Determines whether the innermost opening brace that encloses a list outside every initializer and anonymous object
    /// stands on a line that is not indented four spaces for each brace scope enclosing it, a tab counted as four spaces.
    /// The nesting depth that indents the declaration matches the formatter only where that brace sits at its own depth,
    /// so the separate-line fix is withheld otherwise (SHARED-ATTR-021)
    /// </summary>
    /// <param name="root">Root</param>
    /// <param name="attributeList">Attribute list</param>
    /// <returns><see langword="true"/> when the innermost enclosing brace is off its depth column</returns>
    private static bool IsInnermostEnclosingBraceOffItsDepthColumn(SyntaxNode root, AttributeListSyntax attributeList)
    {
        if (SyntaxIndentationUtilities.HasAnchorScopeAncestor(attributeList))
        {
            return false;
        }

        foreach (var ancestor in attributeList.Ancestors())
        {
            var (owner, openBrace) = ancestor switch
                                     {
                                         BlockSyntax block => (ancestor, block.OpenBraceToken),
                                         TypeDeclarationSyntax typeDeclaration => (ancestor, typeDeclaration.OpenBraceToken),
                                         NamespaceDeclarationSyntax namespaceDeclaration => (ancestor, namespaceDeclaration.OpenBraceToken),
                                         EnumDeclarationSyntax enumDeclaration => (ancestor, enumDeclaration.OpenBraceToken),
                                         SwitchStatementSyntax switchStatement => (ancestor, switchStatement.OpenBraceToken),
                                         AccessorListSyntax accessorList => (ancestor, accessorList.OpenBraceToken),
                                         _ => (null, default),
                                     };

            if (owner == null
                || openBrace.IsMissing
                || openBrace.SpanStart > attributeList.SpanStart)
            {
                continue;
            }

            var sourceText = root.SyntaxTree.GetText();
            var line = sourceText.Lines.GetLineFromPosition(openBrace.SpanStart);
            var indentation = 0;

            for (var position = line.Start; position < line.End && sourceText[position] is ' ' or '\t'; position++)
            {
                indentation += sourceText[position] == '\t' ? SyntaxIndentationUtilities.IndentSize : 1;
            }

            return indentation != SyntaxIndentationUtilities.ComputeBaseIndentLevel(owner) * SyntaxIndentationUtilities.IndentSize;
        }

        return false;
    }

    /// <summary>
    /// Tries to get a fixable attribute list from a diagnostic
    /// </summary>
    /// <param name="root">Root</param>
    /// <param name="diagnostic">Diagnostic</param>
    /// <param name="attributeList">Resolved attribute list</param>
    /// <param name="placementMode">Resolved placement mode</param>
    /// <returns><see langword="true"/> when fix can be offered</returns>
    private bool TryGetFixableAttributeList(SyntaxNode root, Diagnostic diagnostic, out AttributeListSyntax attributeList, out TargetAttributePlacementMode placementMode)
    {
        var diagnosticNode = root.FindToken(diagnostic.Location.SourceSpan.Start).Parent;

        placementMode = default;
        attributeList = diagnosticNode?.AncestorsAndSelf()
                                      .OfType<AttributeListSyntax>()
                                      .FirstOrDefault();

        if (attributeList == null
            || AttributeTargetUtilities.TryResolveLayoutTarget(attributeList, out var target) == false
            || IsAttributeListInScope(attributeList, target) == false
            || AttributeTargetUtilities.TryGetTokenAfterAttributeList(attributeList, out var tokenAfter) == false
            || SyntaxNodeUtilities.InteriorContainsCommentOrDirective(attributeList))
        {
            return false;
        }

        // Both placements rewrite the gap between the closing bracket and the member token: the fix overwrites the
        // bracket's trailing trivia and clears the member token's leading trivia, so trivia in that gap is destroyed
        // whichever placement is resolved. The interior guard above cannot see it, because it belongs to neither the
        // attribute list's own span nor the member's. The formatter preserves such a comment, so the fix stays
        // unregistered rather than reshaping the source destructively
        if (SyntaxTriviaUtilities.WouldJoinAcrossUnjoinableTrivia(attributeList.CloseBracketToken, tokenAfter))
        {
            return false;
        }

        placementMode = ResolvePlacementMode(attributeList);

        // The analyzer base never enforces a separate line on an inline auto-property accessor, because the formatter
        // keeps every attribute list there on the property's line. Such a list already sits where a single-line
        // placement would put it, so there is nothing to offer
        if (placementMode == TargetAttributePlacementMode.SeparateLine
            && (AttributeTargetUtilities.IsAttributeListOnInlineAutoPropertyAccessor(attributeList)
                || HasIncompleteEnclosingScope(attributeList)
                || IsInnermostEnclosingBraceOffItsDepthColumn(root, attributeList)))
        {
            return false;
        }

        return true;
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
            if (TryGetFixableAttributeList(root, diagnostic, out var attributeList, out var placementMode))
            {
                context.RegisterCodeFix(CodeAction.Create(CodeFixTitle,
                                                          token => ApplyCodeFixAsync(context.Document, attributeList, placementMode, token),
                                                          GetType().Name),
                                        diagnostic);
            }
        }
    }

    #endregion // CodeFixProvider
}