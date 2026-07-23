using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Analyzer.Rules.Organization;
using Reihitsu.Formatter;
using Reihitsu.Formatter.Pipeline.StructuralTransforms;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Organization;

/// <summary>
/// Code fix provider for <see cref="RH7101DoNotCombineFieldsAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH7101DoNotCombineFieldsCodeFixProvider))]
public class RH7101DoNotCombineFieldsCodeFixProvider : CodeFixProvider
{
    #region Methods

    /// <summary>
    /// Applies the code fix
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="fieldDeclaration">Field declaration</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated document</returns>
    private static async Task<Document> ApplyCodeFixAsync(Document document, FieldDeclarationSyntax fieldDeclaration, CancellationToken cancellationToken)
    {
        if (fieldDeclaration.Parent is not TypeDeclarationSyntax typeDeclaration
            || fieldDeclaration.Declaration.Variables.Count <= 1)
        {
            return document;
        }

        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (root == null)
        {
            return document;
        }

        // The 1→N replacement the split needs forces a type-level scope, but running the full formatting pipeline
        // over the type reformats members unrelated to the diagnostic. Reuse the formatter's field-split structural
        // transform on its own instead: it splits the combined fields and preserves the comments attached to
        // declarators and their separators, without touching the rest of the type.
        var context = new FormattingContext(ReihitsuFormatterHelpers.DetectEndOfLine(root));
        var splitTypeDeclaration = new FieldDeclarationSplitTransform(context, cancellationToken).Visit(typeDeclaration);

        return splitTypeDeclaration == null
                   ? document
                   : document.WithSyntaxRoot(root.ReplaceNode(typeDeclaration, splitTypeDeclaration));
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH7101DoNotCombineFieldsAnalyzer.DiagnosticId];

    /// <inheritdoc/>
    public sealed override FixAllProvider GetFixAllProvider()
    {
        return WellKnownFixAllProviders.BatchFixer;
    }

    /// <inheritdoc/>
    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

        if (syntaxRoot == null)
        {
            return;
        }

        foreach (var diagnostic in context.Diagnostics)
        {
            var fieldDeclaration = syntaxRoot.FindToken(diagnostic.Location.SourceSpan.Start).Parent
                                             ?.AncestorsAndSelf()
                                             .OfType<FieldDeclarationSyntax>()
                                             .FirstOrDefault();

            if (fieldDeclaration != null)
            {
                context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH7101Title,
                                                          token => ApplyCodeFixAsync(context.Document, fieldDeclaration, token),
                                                          nameof(RH7101DoNotCombineFieldsCodeFixProvider)),
                                        diagnostic);
            }
        }
    }

    #endregion // CodeFixProvider
}