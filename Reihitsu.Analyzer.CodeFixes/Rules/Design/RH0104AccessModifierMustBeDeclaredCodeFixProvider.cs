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

namespace Reihitsu.Analyzer.Rules.Design;

/// <summary>
/// Code fix provider for <see cref="RH0104AccessModifierMustBeDeclaredAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH0104AccessModifierMustBeDeclaredCodeFixProvider))]
public class RH0104AccessModifierMustBeDeclaredCodeFixProvider : CodeFixProvider
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

        var defaultModifier = memberDeclaration.Parent is CompilationUnitSyntax
                                                       or BaseNamespaceDeclarationSyntax
                                  ? SyntaxKind.InternalKeyword
                                  : SyntaxKind.PrivateKeyword;
        var modifiers = DeclarationModifierUtilities.GetModifiers(memberDeclaration);
        var updatedDeclaration = DeclarationModifierUtilities.WithModifiers(memberDeclaration, DeclarationModifierUtilities.AddAccessibilityModifier(modifiers, defaultModifier));
        var updatedRoot = root.ReplaceNode(memberDeclaration, updatedDeclaration);

        return document.WithSyntaxRoot(updatedRoot);
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
        memberDeclaration = root.FindToken(diagnostic.Location.SourceSpan.Start).Parent?.AncestorsAndSelf()
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
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH0104AccessModifierMustBeDeclaredAnalyzer.DiagnosticId];

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
                    context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH0104Title,
                                                              token => ApplyCodeFixAsync(context.Document, memberDeclaration, token),
                                                              nameof(RH0104AccessModifierMustBeDeclaredCodeFixProvider)),
                                            diagnostic);
                }
            }
        }
    }

    #endregion // CodeFixProvider
}