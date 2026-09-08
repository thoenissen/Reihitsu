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
    /// <param name="target">Resolved target</param>
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

            // The indentation of the inserted line is computed from the attribute list's syntactic nesting depth
            // rather than read from source text or trivia. Text- and trivia-based derivations both have to special
            // case every shape that can precede the list on its line or share its line — another token, a
            // directive, a multi-line comment, a multi-line string literal, a multi-line attribute list, parameter
            // list, or initializer — and each such case is its own way to read the wrong thing as indentation.
            // Nesting depth answers the same question without reading any of that: it depends only on which
            // braced scopes contain the attribute list, matching Reihitsu.Formatter's own IndentationPhase
            var indentLevel = SyntaxIndentationUtilities.ComputeBaseIndentLevel(attributeList);

            if (indentLevel > 0)
            {
                trailingTrivia = trailingTrivia.Add(SyntaxFactory.Whitespace(new string(' ', indentLevel * SyntaxIndentationUtilities.IndentSize)));
            }

            updatedCloseBracket = closeBracket.WithTrailingTrivia(trailingTrivia);
        }
        else
        {
            updatedCloseBracket = closeBracket.WithTrailingTrivia(SyntaxFactory.Space);
        }

        var updatedTokenAfter = tokenAfter.WithLeadingTrivia(SyntaxFactory.TriviaList());

        var updatedRoot = root.ReplaceTokens([closeBracket, tokenAfter], (original, _) => original == closeBracket ? updatedCloseBracket : updatedTokenAfter);

        return document.WithSyntaxRoot(updatedRoot);
    }

    /// <summary>
    /// Determines whether an enclosing scope that <see cref="SyntaxIndentationUtilities.ComputeBaseIndentLevel"/>
    /// would count owns a brace the parser could not find. Such a scope silently drops out of the computed
    /// indentation level instead of raising an error, which would strip the member's indentation on exactly the
    /// transient, mid-edit documents where an IDE offers "Fix all in document" most often
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
            || AttributeTargetUtilities.TryResolveTarget(attributeList, out var target) == false
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

        // ComputeBaseIndentLevel counts an enclosing scope by its brace pair; a scope whose brace the parser
        // could not find silently drops out of that count instead of raising an error, understating the
        // indentation level. There is no way to recover the intended depth from a scope the parser never closed,
        // so the fix stays unregistered rather than guessing at a document that is still being edited
        if (placementMode == TargetAttributePlacementMode.SeparateLine
            && HasIncompleteEnclosingScope(attributeList))
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