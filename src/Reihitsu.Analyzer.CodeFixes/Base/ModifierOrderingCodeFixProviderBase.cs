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

namespace Reihitsu.Analyzer.CodeFixes.Base;

/// <summary>
/// Base class for modifier ordering code fixes
/// </summary>
public abstract class ModifierOrderingCodeFixProviderBase : CodeFixProvider
{
    #region Fields

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    private readonly string _diagnosticId;

    /// <summary>
    /// Code fix title
    /// </summary>
    private readonly string _title;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="diagnosticId">Diagnostic ID</param>
    /// <param name="title">Code fix title</param>
    protected ModifierOrderingCodeFixProviderBase(string diagnosticId, string title)
    {
        _diagnosticId = diagnosticId;
        _title = title;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Gets the updated modifiers
    /// </summary>
    /// <param name="modifiers">Modifiers</param>
    /// <returns>The updated modifiers</returns>
    protected abstract SyntaxTokenList GetUpdatedModifiers(SyntaxTokenList modifiers);

    /// <summary>
    /// Determines whether reordering the modifiers would drop or relocate a comment or a preprocessor directive that
    /// sits between them or behind the last of them; only the leading trivia of the first modifier is preserved when
    /// the modifiers are rebuilt, so a comment or directive anywhere else among the modifiers would be silently
    /// deleted, leaving the code non-compiling in the case of an orphaned directive. Each rebuilt modifier ends with a
    /// single space, so a comment, a directive, or disabled text in front of the token after the last modifier would
    /// be pulled onto the line of the modifiers
    /// </summary>
    /// <param name="modifiers">Modifiers</param>
    /// <returns><see langword="true"/> when a comment, a directive, or disabled text would be dropped or relocated; otherwise <see langword="false"/></returns>
    private static bool ReorderingWouldDropCommentOrDirective(SyntaxTokenList modifiers)
    {
        for (var modifierIndex = 0; modifierIndex < modifiers.Count; modifierIndex++)
        {
            if (modifiers[modifierIndex].TrailingTrivia.Any(SyntaxNodeUtilities.IsCommentOrDirective)
                || (modifierIndex > 0 && modifiers[modifierIndex].LeadingTrivia.Any(SyntaxNodeUtilities.IsCommentOrDirective)))
            {
                return true;
            }
        }

        return modifiers.Count > 0
               && modifiers[modifiers.Count - 1].GetNextToken().LeadingTrivia.Any(trivia => SyntaxNodeUtilities.IsCommentOrDirective(trivia)
                                                                                            || trivia.IsKind(SyntaxKind.DisabledTextTrivia));
    }

    /// <summary>
    /// Creates the member declaration the fix writes: the declaration with its modifiers in order. The guard of
    /// RegisterCodeFixesAsync leaves no comment or directive between the modifiers or behind the last of them, so the
    /// rewrite writes each modifier with a single space behind it and leaves the token behind the modifiers as it is
    /// (SHARED-MODORDER-002)
    /// </summary>
    /// <param name="memberDeclaration">Member declaration</param>
    /// <param name="updatedDeclaration">The updated member declaration</param>
    /// <returns><see langword="true"/> if the modifiers can be written in order; otherwise, <see langword="false"/></returns>
    private bool TryCreateUpdatedDeclaration(MemberDeclarationSyntax memberDeclaration, out MemberDeclarationSyntax updatedDeclaration)
    {
        var originalModifiers = DeclarationModifierUtilities.GetModifiers(memberDeclaration);

        if (originalModifiers.Count == 0
            || ModifierOrderingUtilities.TryWriteModifiersInOrder(originalModifiers, originalModifiers[originalModifiers.Count - 1].GetNextToken(), GetUpdatedModifiers(originalModifiers), out var updatedModifiers, out _) == false)
        {
            updatedDeclaration = null;

            return false;
        }

        updatedDeclaration = DeclarationModifierUtilities.WithModifiers(memberDeclaration, updatedModifiers);

        return true;
    }

    /// <summary>
    /// Applies the code fix: only the modifiers are rewritten, and the rest of the declaration stays as written
    /// (SHARED-FIX-006)
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="memberDeclaration">Declaration</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated document</returns>
    private async Task<Document> ApplyCodeFixAsync(Document document, MemberDeclarationSyntax memberDeclaration, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (root == null)
        {
            return document;
        }

        if (TryCreateUpdatedDeclaration(memberDeclaration, out var updatedDeclaration) == false)
        {
            return document;
        }

        return document.WithSyntaxRoot(root.ReplaceNode(memberDeclaration, updatedDeclaration));
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [_diagnosticId];

    /// <inheritdoc/>
    public sealed override FixAllProvider GetFixAllProvider()
    {
        return WellKnownFixAllProviders.BatchFixer;
    }

    /// <inheritdoc/>
    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

        if (root != null)
        {
            foreach (var diagnostic in context.Diagnostics)
            {
                if (OrderingDeclarationUtilities.TryGetMemberDeclaration(root, diagnostic, out var memberDeclaration)
                    && ReorderingWouldDropCommentOrDirective(DeclarationModifierUtilities.GetModifiers(memberDeclaration)) == false)
                {
                    context.RegisterCodeFix(CodeAction.Create(_title,
                                                              token => ApplyCodeFixAsync(context.Document, memberDeclaration, token),
                                                              GetType().Name),
                                            diagnostic);
                }
            }
        }
    }

    #endregion // CodeFixProvider
}