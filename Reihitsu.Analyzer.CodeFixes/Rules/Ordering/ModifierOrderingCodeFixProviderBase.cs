using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Analyzer.Core;
using Reihitsu.Formatter;

namespace Reihitsu.Analyzer.Rules.Ordering;

/// <summary>
/// Base class for modifier ordering code fixes
/// </summary>
[Shared]
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
    /// Normalizes modifier spacing after reordering
    /// </summary>
    /// <param name="updatedModifiers">Updated modifiers</param>
    /// <param name="originalModifiers">Original modifiers</param>
    /// <returns>The normalized modifiers</returns>
    private static SyntaxTokenList NormalizeModifierTrivia(SyntaxTokenList updatedModifiers, SyntaxTokenList originalModifiers)
    {
        if (updatedModifiers.Count == 0)
        {
            return updatedModifiers;
        }

        var firstLeadingTrivia = originalModifiers[0].LeadingTrivia;
        var normalizedModifiers = new List<SyntaxToken>(updatedModifiers.Count);

        for (var modifierIndex = 0; modifierIndex < updatedModifiers.Count; modifierIndex++)
        {
            normalizedModifiers.Add(SyntaxFactory.Token(modifierIndex == 0 ? firstLeadingTrivia : default, (SyntaxKind)updatedModifiers[modifierIndex].RawKind, [SyntaxFactory.Space]));
        }

        return SyntaxFactory.TokenList(normalizedModifiers);
    }

    /// <summary>
    /// Applies the code fix
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

        var originalModifiers = DeclarationModifierUtilities.GetModifiers(memberDeclaration);
        var formattingAnnotation = new SyntaxAnnotation();
        var updatedDeclaration = DeclarationModifierUtilities.WithModifiers(memberDeclaration, NormalizeModifierTrivia(GetUpdatedModifiers(originalModifiers), originalModifiers))
                                                             .WithAdditionalAnnotations(formattingAnnotation);
        var updatedRoot = root.ReplaceNode(memberDeclaration, updatedDeclaration);
        var updatedDocument = document.WithSyntaxRoot(updatedRoot);
        var formattedRoot = await updatedDocument.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        var formattedMemberDeclaration = formattedRoot?.GetAnnotatedNodes(formattingAnnotation).OfType<MemberDeclarationSyntax>().FirstOrDefault();

        return formattedMemberDeclaration == null
                   ? updatedDocument
                   : await ReihitsuFormatter.FormatNodeInDocumentAsync(updatedDocument, formattedMemberDeclaration, cancellationToken).ConfigureAwait(false);
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
                if (OrderingDeclarationUtilities.TryGetMemberDeclaration(root, diagnostic, out var memberDeclaration))
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