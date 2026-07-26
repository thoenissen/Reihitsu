using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Analyzer.Rules.Clarity;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Clarity;

/// <summary>
/// Code fix provider for <see cref="RH3103UseShorthandForNullableTypesAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH3103UseShorthandForNullableTypesCodeFixProvider))]
public class RH3103UseShorthandForNullableTypesCodeFixProvider : CodeFixProvider
{
    #region Methods

    /// <summary>
    /// Applying the code fix
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="typeSyntax">Type syntax</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated <see cref="Document"/> with the code fix applied</returns>
    private static async Task<Document> ApplyCodeFixAsync(Document document, TypeSyntax typeSyntax, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (root == null)
        {
            return document;
        }

        var genericName = GetGenericName(typeSyntax);

        if (genericName == null)
        {
            return document;
        }

        var typeArgumentList = genericName.TypeArgumentList;
        var typeArgument = typeArgumentList.Arguments[0].WithLeadingTrivia(typeSyntax.GetLeadingTrivia()
                                                                                     .AddRange(genericName.Identifier.TrailingTrivia)
                                                                                     .AddRange(typeArgumentList.LessThanToken.LeadingTrivia)
                                                                                     .AddRange(typeArgumentList.LessThanToken.TrailingTrivia)
                                                                                     .AddRange(typeArgumentList.Arguments[0].GetLeadingTrivia()))
                                                        .WithTrailingTrivia(typeArgumentList.Arguments[0].GetTrailingTrivia()
                                                                                                         .AddRange(typeArgumentList.GreaterThanToken.LeadingTrivia));
        var replacementType = SyntaxFactory.NullableType(typeArgument)
                                           .WithTrailingTrivia(typeSyntax.GetTrailingTrivia());

        var updatedRoot = root.ReplaceNode(typeSyntax, replacementType);

        return document.WithSyntaxRoot(updatedRoot);
    }

    /// <summary>
    /// Determine whether the code fix can preserve the complete type argument
    /// </summary>
    /// <param name="typeSyntax">Type syntax</param>
    /// <returns><see langword="true"/> if the code fix can be applied safely</returns>
    private static bool CanApplyCodeFix(TypeSyntax typeSyntax)
    {
        var genericName = GetGenericName(typeSyntax);

        return genericName != null
               && genericName.TypeArgumentList.DescendantTrivia(descendIntoTrivia: true)
                                              .Any(trivia => trivia.IsDirective || trivia.IsKind(SyntaxKind.DisabledTextTrivia)) == false;
    }

    /// <summary>
    /// Get the nullable generic name represented by a target type syntax
    /// </summary>
    /// <param name="typeSyntax">Type syntax</param>
    /// <returns>The generic name, or <see langword="null"/> when the target is unsupported</returns>
    private static GenericNameSyntax GetGenericName(TypeSyntax typeSyntax)
    {
        return typeSyntax switch
               {
                   GenericNameSyntax matchingGenericName => matchingGenericName,
                   QualifiedNameSyntax { Right: GenericNameSyntax matchingGenericName } => matchingGenericName,
                   _ => null
               };
    }

    /// <summary>
    /// Try to get the target type syntax
    /// </summary>
    /// <param name="root">Root</param>
    /// <param name="diagnostic">Diagnostic</param>
    /// <returns>Target type syntax</returns>
    private static TypeSyntax TryGetTypeSyntax(SyntaxNode root, Diagnostic diagnostic)
    {
        var node = root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);

        return node switch
               {
                   QualifiedNameSyntax qualifiedName => qualifiedName,
                   GenericNameSyntax { Parent: QualifiedNameSyntax qualifiedName } genericName when qualifiedName.Right == genericName => qualifiedName,
                   GenericNameSyntax genericName => genericName,
                   _ => node.AncestorsAndSelf().OfType<TypeSyntax>().FirstOrDefault()
               };
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH3103UseShorthandForNullableTypesAnalyzer.DiagnosticId];

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
                var typeSyntax = TryGetTypeSyntax(root, diagnostic);

                if (typeSyntax != null && CanApplyCodeFix(typeSyntax))
                {
                    context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH3103Title,
                                                              token => ApplyCodeFixAsync(context.Document, typeSyntax, token),
                                                              nameof(RH3103UseShorthandForNullableTypesCodeFixProvider)),
                                            diagnostic);
                }
            }
        }
    }

    #endregion // CodeFixProvider
}