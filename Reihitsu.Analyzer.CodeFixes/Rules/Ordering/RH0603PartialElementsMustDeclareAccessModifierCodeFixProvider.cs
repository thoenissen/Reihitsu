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

using Reihitsu.Formatter;

namespace Reihitsu.Analyzer.Rules.Ordering;

/// <summary>
/// Code fix provider for <see cref="RH0603PartialElementsMustDeclareAccessModifierAnalyzer"/>.
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH0603PartialElementsMustDeclareAccessModifierCodeFixProvider))]
public class RH0603PartialElementsMustDeclareAccessModifierCodeFixProvider : CodeFixProvider
{
    #region Methods

    /// <summary>
    /// Applies the code fix.
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="typeDeclaration">Type declaration</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated document</returns>
    private static async Task<Document> ApplyCodeFixAsync(Document document, TypeDeclarationSyntax typeDeclaration, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

        if (root == null
            || semanticModel == null)
        {
            return document;
        }

        var declaredSymbol = semanticModel.GetDeclaredSymbol(typeDeclaration, cancellationToken);
        var accessibilityTokens = GetAccessibilityTokens(declaredSymbol?.DeclaredAccessibility ?? Accessibility.NotApplicable,
                                                         typeDeclaration.Parent is TypeDeclarationSyntax,
                                                         typeDeclaration.Modifiers[0].LeadingTrivia);
        var updatedModifiers = SyntaxFactory.TokenList(accessibilityTokens.Concat(typeDeclaration.Modifiers.Select((modifier, modifierIndex) => modifierIndex == 0 ? modifier.WithLeadingTrivia() : modifier)));
        var formattingAnnotation = new SyntaxAnnotation();
        var updatedTypeDeclaration = typeDeclaration.WithModifiers(updatedModifiers).WithAdditionalAnnotations(formattingAnnotation);
        var updatedRoot = root.ReplaceNode(typeDeclaration, updatedTypeDeclaration);
        var updatedDocument = document.WithSyntaxRoot(updatedRoot);
        var formattedRoot = await updatedDocument.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        var annotatedTypeDeclarations = formattedRoot?.GetAnnotatedNodes(formattingAnnotation).OfType<TypeDeclarationSyntax>();
        var formattedTypeDeclaration = annotatedTypeDeclarations?.FirstOrDefault();

        return formattedTypeDeclaration == null
                   ? updatedDocument
                   : await ReihitsuFormatter.FormatNodeInDocumentAsync(updatedDocument, formattedTypeDeclaration, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Creates the accessibility tokens.
    /// </summary>
    /// <param name="accessibility">Accessibility</param>
    /// <param name="isNestedType">Whether the declaration is nested in another type</param>
    /// <param name="leadingTrivia">Leading trivia for the first token</param>
    /// <returns>The accessibility tokens</returns>
    private static IReadOnlyList<SyntaxToken> GetAccessibilityTokens(Accessibility accessibility, bool isNestedType, SyntaxTriviaList leadingTrivia)
    {
        return accessibility switch
               {
                   Accessibility.Public => CreateAccessibilityTokens(leadingTrivia, SyntaxKind.PublicKeyword),
                   Accessibility.Internal => CreateAccessibilityTokens(leadingTrivia, SyntaxKind.InternalKeyword),
                   Accessibility.Protected => CreateAccessibilityTokens(leadingTrivia, SyntaxKind.ProtectedKeyword),
                   Accessibility.Private => CreateAccessibilityTokens(leadingTrivia, SyntaxKind.PrivateKeyword),
                   Accessibility.ProtectedOrInternal => CreateAccessibilityTokens(leadingTrivia, SyntaxKind.ProtectedKeyword, SyntaxKind.InternalKeyword),
                   Accessibility.ProtectedAndInternal => CreateAccessibilityTokens(leadingTrivia, SyntaxKind.PrivateKeyword, SyntaxKind.ProtectedKeyword),
                   _ when isNestedType => CreateAccessibilityTokens(leadingTrivia, SyntaxKind.PrivateKeyword),
                   _ => CreateAccessibilityTokens(leadingTrivia, SyntaxKind.InternalKeyword),
               };
    }

    /// <summary>
    /// Creates accessibility tokens with a trailing space.
    /// </summary>
    /// <param name="leadingTrivia">Leading trivia for the first token</param>
    /// <param name="syntaxKinds">Accessibility token kinds</param>
    /// <returns>The created tokens</returns>
    private static IReadOnlyList<SyntaxToken> CreateAccessibilityTokens(SyntaxTriviaList leadingTrivia, params SyntaxKind[] syntaxKinds)
    {
        var tokens = new List<SyntaxToken>(syntaxKinds.Length);

        for (var tokenIndex = 0; tokenIndex < syntaxKinds.Length; tokenIndex++)
        {
            tokens.Add(SyntaxFactory.Token(tokenIndex == 0 ? leadingTrivia : default, syntaxKinds[tokenIndex], [SyntaxFactory.Space]));
        }

        return tokens;
    }

    /// <summary>
    /// Tries to find the type declaration.
    /// </summary>
    /// <param name="root">Root</param>
    /// <param name="diagnostic">Diagnostic</param>
    /// <param name="typeDeclaration">Type declaration</param>
    /// <returns><see langword="true"/> if the declaration was found</returns>
    private static bool TryGetTypeDeclaration(SyntaxNode root, Diagnostic diagnostic, out TypeDeclarationSyntax typeDeclaration)
    {
        var diagnosticNode = root.FindToken(diagnostic.Location.SourceSpan.Start).Parent;

        typeDeclaration = diagnosticNode?.AncestorsAndSelf().OfType<TypeDeclarationSyntax>().FirstOrDefault();

        return typeDeclaration != null;
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH0603PartialElementsMustDeclareAccessModifierAnalyzer.DiagnosticId];

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
                if (TryGetTypeDeclaration(root, diagnostic, out var typeDeclaration))
                {
                    context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH0603Title,
                                                              token => ApplyCodeFixAsync(context.Document, typeDeclaration, token),
                                                              nameof(RH0603PartialElementsMustDeclareAccessModifierCodeFixProvider)),
                                            diagnostic);
                }
            }
        }
    }

    #endregion // CodeFixProvider
}