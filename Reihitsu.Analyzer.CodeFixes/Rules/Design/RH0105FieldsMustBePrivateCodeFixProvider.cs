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
/// Code fix provider for <see cref="RH0105FieldsMustBePrivateAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH0105FieldsMustBePrivateCodeFixProvider))]
public class RH0105FieldsMustBePrivateCodeFixProvider : CodeFixProvider
{
    #region Methods

    /// <summary>
    /// Applying the code fix
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="fieldDeclaration">Field declaration</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated <see cref="Document"/> with the code fix applied.</returns>
    private static async Task<Document> ApplyCodeFixAsync(Document document, FieldDeclarationSyntax fieldDeclaration, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (root == null)
        {
            return document;
        }

        var updatedFieldDeclaration = fieldDeclaration.WithModifiers(DeclarationModifierUtilities.AddAccessibilityModifier(fieldDeclaration.Modifiers, SyntaxKind.PrivateKeyword));
        var updatedRoot = root.ReplaceNode(fieldDeclaration, updatedFieldDeclaration);

        return document.WithSyntaxRoot(updatedRoot);
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH0105FieldsMustBePrivateAnalyzer.DiagnosticId];

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
                var fieldDeclaration = root.FindToken(diagnostic.Location.SourceSpan.Start).Parent?.AncestorsAndSelf()
                                           .OfType<FieldDeclarationSyntax>()
                                           .FirstOrDefault();

                if (fieldDeclaration != null)
                {
                    context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH0105Title,
                                                              token => ApplyCodeFixAsync(context.Document, fieldDeclaration, token),
                                                              nameof(RH0105FieldsMustBePrivateCodeFixProvider)),
                                            diagnostic);
                }
            }
        }
    }

    #endregion // CodeFixProvider
}