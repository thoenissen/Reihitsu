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

using Reihitsu.Analyzer.Rules.Design;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Design;

/// <summary>
/// Code fix provider for <see cref="RH2004AccessModifierMustBeDeclaredAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH2004AccessModifierMustBeDeclaredCodeFixProvider))]
public class RH2004AccessModifierMustBeDeclaredCodeFixProvider : CodeFixProvider
{
    #region Methods

    /// <summary>
    /// Applying the code fix
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="memberDeclaration">Declaration</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated <see cref="Document"/> with the code fix applied</returns>
    private static async Task<Document> ApplyCodeFixAsync(Document document, MemberDeclarationSyntax memberDeclaration, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (root == null)
        {
            return document;
        }

        var updatedDeclaration = await CreateUpdatedDeclarationAsync(document, memberDeclaration, cancellationToken).ConfigureAwait(false);
        var updatedRoot = root.ReplaceNode(memberDeclaration, updatedDeclaration);

        return document.WithSyntaxRoot(updatedRoot);
    }

    /// <summary>
    /// Creates the declaration with the missing accessibility modifier added
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="memberDeclaration">Declaration</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated declaration</returns>
    private static async Task<MemberDeclarationSyntax> CreateUpdatedDeclarationAsync(Document document, MemberDeclarationSyntax memberDeclaration, CancellationToken cancellationToken)
    {
        // A partial type may declare its accessibility on another part. Selecting the modifier syntactically would
        // insert a conflicting one next to that part (CS0262), so the already declared accessibility is read from
        // the merged declared symbol instead.
        if (memberDeclaration is TypeDeclarationSyntax typeDeclaration
            && typeDeclaration.Modifiers.Any(SyntaxKind.PartialKeyword))
        {
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

            if (semanticModel?.GetDeclaredSymbol(typeDeclaration, cancellationToken) is { } declaredSymbol)
            {
                return DeclarationModifierUtilities.AddAccessibilityModifiers(typeDeclaration, GetAccessibilityModifiers(declaredSymbol.DeclaredAccessibility));
            }
        }

        var defaultModifier = memberDeclaration.Parent is CompilationUnitSyntax
                                                       or BaseNamespaceDeclarationSyntax
                                  ? SyntaxKind.InternalKeyword
                                  : SyntaxKind.PrivateKeyword;

        return DeclarationModifierUtilities.AddAccessibilityModifier(memberDeclaration, defaultModifier);
    }

    /// <summary>
    /// Maps a declared accessibility to the modifier keywords that express it
    /// </summary>
    /// <param name="accessibility">Declared accessibility</param>
    /// <returns>The modifier keywords in declaration order</returns>
    private static IReadOnlyList<SyntaxKind> GetAccessibilityModifiers(Accessibility accessibility)
    {
        return accessibility switch
               {
                   Accessibility.Public => [SyntaxKind.PublicKeyword],
                   Accessibility.Internal => [SyntaxKind.InternalKeyword],
                   Accessibility.Protected => [SyntaxKind.ProtectedKeyword],
                   Accessibility.Private => [SyntaxKind.PrivateKeyword],
                   Accessibility.ProtectedOrInternal => [SyntaxKind.ProtectedKeyword, SyntaxKind.InternalKeyword],
                   Accessibility.ProtectedAndInternal => [SyntaxKind.PrivateKeyword, SyntaxKind.ProtectedKeyword],
                   _ => [SyntaxKind.InternalKeyword]
               };
    }

    /// <summary>
    /// Try to find the affected declaration
    /// </summary>
    /// <param name="root">Root</param>
    /// <param name="diagnostic">Diagnostic</param>
    /// <param name="memberDeclaration">Declaration</param>
    /// <returns><see langword="true"/> if the declaration was found</returns>
    private static bool TryGetMemberDeclaration(SyntaxNode root, Diagnostic diagnostic, out MemberDeclarationSyntax memberDeclaration)
    {
        memberDeclaration = root.FindToken(diagnostic.Location.SourceSpan.Start).Parent
                                ?.AncestorsAndSelf()
                                .OfType<MemberDeclarationSyntax>()
                                .FirstOrDefault(obj => obj is BaseTypeDeclarationSyntax
                                                           or DelegateDeclarationSyntax
                                                           or MethodDeclarationSyntax
                                                           or PropertyDeclarationSyntax
                                                           or FieldDeclarationSyntax
                                                           or EventDeclarationSyntax
                                                           or EventFieldDeclarationSyntax
                                                           or ConstructorDeclarationSyntax
                                                           or IndexerDeclarationSyntax
                                                           or OperatorDeclarationSyntax
                                                           or ConversionOperatorDeclarationSyntax);

        return memberDeclaration != null;
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH2004AccessModifierMustBeDeclaredAnalyzer.DiagnosticId];

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
                if (TryGetMemberDeclaration(root, diagnostic, out var memberDeclaration))
                {
                    context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH2004Title,
                                                              token => ApplyCodeFixAsync(context.Document, memberDeclaration, token),
                                                              nameof(RH2004AccessModifierMustBeDeclaredCodeFixProvider)),
                                            diagnostic);
                }
            }
        }
    }

    #endregion // CodeFixProvider
}