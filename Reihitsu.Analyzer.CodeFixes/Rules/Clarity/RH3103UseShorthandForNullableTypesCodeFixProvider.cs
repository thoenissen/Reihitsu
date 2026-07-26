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
using Reihitsu.Core;

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

        var typeArgument = NormalizeTypeArgumentTrivia(typeSyntax, genericName);
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
               && typeSyntax.DescendantTrivia(descendIntoTrivia: true)
                            .Any(SyntaxTriviaUtilities.IsDirectiveOrDisabledTextTrivia) == false
               && ContainsUnsupportedComment(typeSyntax, genericName) == false;
    }

    /// <summary>
    /// Determine whether comments would be lost or cannot be safely repositioned by the code fix
    /// </summary>
    /// <param name="typeSyntax">Complete type syntax being replaced</param>
    /// <param name="genericName">Nullable generic name</param>
    /// <returns><see langword="true"/> if a comment prevents the code fix</returns>
    private static bool ContainsUnsupportedComment(TypeSyntax typeSyntax, GenericNameSyntax genericName)
    {
        var typeArgumentListSpan = genericName.TypeArgumentList.FullSpan;

        return typeSyntax.DescendantTrivia(descendIntoTrivia: true)
                         .Any(trivia => SyntaxTriviaUtilities.IsCommentTrivia(trivia)
                                        && ((trivia.SpanStart >= typeSyntax.SpanStart
                                             && trivia.Span.End <= typeSyntax.Span.End
                                             && (trivia.SpanStart < typeArgumentListSpan.Start
                                                 || trivia.Span.End > typeArgumentListSpan.End))
                                            || trivia.IsKind(SyntaxKind.MultiLineCommentTrivia) == false));
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
                   AliasQualifiedNameSyntax { Name: GenericNameSyntax matchingGenericName } => matchingGenericName,
                   _ => null
               };
    }

    /// <summary>
    /// Normalize trivia that belonged to the removed nullable wrapper while retaining block comments
    /// </summary>
    /// <param name="typeSyntax">Complete type syntax being replaced</param>
    /// <param name="genericName">Nullable generic name</param>
    /// <returns>The type argument with normalized boundary trivia</returns>
    private static TypeSyntax NormalizeTypeArgumentTrivia(TypeSyntax typeSyntax, GenericNameSyntax genericName)
    {
        var typeArgumentList = genericName.TypeArgumentList;
        var typeArgument = typeArgumentList.Arguments[0];
        var leadingTrivia = typeSyntax.GetLeadingTrivia();

        foreach (var trivia in typeArgumentList.DescendantTrivia(descendIntoTrivia: true)
                                               .Where(trivia => trivia.IsKind(SyntaxKind.MultiLineCommentTrivia)
                                                                && trivia.Span.End <= typeArgument.SpanStart))
        {
            leadingTrivia = leadingTrivia.Add(trivia).Add(SyntaxFactory.Space);
        }

        var trailingTrivia = default(SyntaxTriviaList);

        foreach (var trivia in typeArgumentList.DescendantTrivia(descendIntoTrivia: true)
                                               .Where(trivia => trivia.IsKind(SyntaxKind.MultiLineCommentTrivia)
                                                                && trivia.SpanStart >= typeArgument.Span.End))
        {
            trailingTrivia = trailingTrivia.Add(SyntaxFactory.Space).Add(trivia);
        }

        return typeArgument.WithLeadingTrivia(leadingTrivia)
                           .WithTrailingTrivia(trailingTrivia);
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
                   AliasQualifiedNameSyntax aliasQualifiedName => aliasQualifiedName,
                   GenericNameSyntax { Parent: AliasQualifiedNameSyntax aliasQualifiedName } genericName when aliasQualifiedName.Name == genericName => aliasQualifiedName,
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