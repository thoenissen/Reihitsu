using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Analyzer.Rules.Clarity;

/// <summary>
/// Code fix provider for <see cref="RH0007UseShorthandForNullableTypesAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH0007UseShorthandForNullableTypesCodeFixProvider))]
public class RH0007UseShorthandForNullableTypesCodeFixProvider : CodeFixProvider
{
    #region Methods

    /// <summary>
    /// Applying the code fix
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="typeSyntax">Type syntax</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated <see cref="Document"/> with the code fix applied.</returns>
    private static async Task<Document> ApplyCodeFixAsync(Document document, TypeSyntax typeSyntax, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (root == null)
        {
            return document;
        }

        var genericName = typeSyntax switch
                          {
                              GenericNameSyntax matchingGenericName => matchingGenericName,
                              QualifiedNameSyntax { Right: GenericNameSyntax matchingGenericName } => matchingGenericName,
                              _ => null
                          };

        if (genericName == null)
        {
            return document;
        }

        var replacementType = SyntaxFactory.NullableType(genericName.TypeArgumentList.Arguments[0].WithoutTrivia()).WithTriviaFrom(typeSyntax);
        var updatedRoot = root.ReplaceNode(typeSyntax, replacementType);

        return document.WithSyntaxRoot(updatedRoot);
    }

    /// <summary>
    /// Try to get the target type syntax.
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
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH0007UseShorthandForNullableTypesAnalyzer.DiagnosticId];

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

                if (typeSyntax != null)
                {
                    context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH0007Title,
                                                              token => ApplyCodeFixAsync(context.Document, typeSyntax, token),
                                                              nameof(RH0007UseShorthandForNullableTypesCodeFixProvider)),
                                            diagnostic);
                }
            }
        }
    }

    #endregion // CodeFixProvider
}