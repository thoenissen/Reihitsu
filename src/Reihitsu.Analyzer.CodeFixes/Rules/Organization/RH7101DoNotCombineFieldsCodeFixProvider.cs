using System.Collections.Generic;
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
    /// Applies the code fix: the declaration is formatted with the type that declares it as context, so the structural phase
    /// splits it as document-level formatting splits it, the later phases lay out the fields, and only the fields the split
    /// makes of the declaration are written back (FMT-PIPE-029)
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="fieldDeclaration">Field declaration</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated document</returns>
    private static Task<Document> ApplyCodeFixAsync(Document document, FieldDeclarationSyntax fieldDeclaration, CancellationToken cancellationToken)
    {
        return ReihitsuFormatter.FormatNodeInDocumentWithContextAsync(document, fieldDeclaration, fieldDeclaration.Parent, cancellationToken);
    }

    /// <summary>
    /// Applies the code fix to every fixable diagnostic of one document, one declaration after the other. Two fixes of
    /// declarations that share a line both write the line break between them; merging their independent text changes
    /// would write it twice, so each fix runs on the document the previous fix produced
    /// </summary>
    /// <param name="context">Fix All context</param>
    /// <param name="document">Document</param>
    /// <param name="diagnostics">Diagnostics of the document</param>
    /// <returns>The updated <see cref="Document"/> with every code fix applied</returns>
    private static async Task<Document> ApplyFixAllAsync(FixAllContext context, Document document, ImmutableArray<Diagnostic> diagnostics)
    {
        var root = await document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

        if (root == null)
        {
            return document;
        }

        var fieldDeclarations = new List<FieldDeclarationSyntax>();

        foreach (var diagnostic in diagnostics)
        {
            if (TryGetFixableFieldDeclaration(root, diagnostic, out var fieldDeclaration)
                && fieldDeclarations.Contains(fieldDeclaration) == false)
            {
                fieldDeclarations.Add(fieldDeclaration);
            }
        }

        if (fieldDeclarations.Count == 0)
        {
            return document;
        }

        var updatedDocument = document.WithSyntaxRoot(root.TrackNodes(fieldDeclarations));

        foreach (var fieldDeclaration in fieldDeclarations)
        {
            var updatedRoot = await updatedDocument.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var currentFieldDeclaration = updatedRoot?.GetCurrentNode(fieldDeclaration);

            if (currentFieldDeclaration != null)
            {
                updatedDocument = await ApplyCodeFixAsync(updatedDocument, currentFieldDeclaration, context.CancellationToken).ConfigureAwait(false);
            }
        }

        return updatedDocument;
    }

    /// <summary>
    /// Gets the field declaration of a diagnostic when the fix is offered for it: exactly where the formatter splits the
    /// declaration, because the fix splits it only by formatting it; elsewhere, such as for a declaration with a directive,
    /// it would change nothing (RH7101-005)
    /// </summary>
    /// <param name="root">Syntax root</param>
    /// <param name="diagnostic">Diagnostic</param>
    /// <param name="fieldDeclaration">The field declaration of the diagnostic</param>
    /// <returns><see langword="true"/> if the fix is offered for the diagnostic; otherwise, <see langword="false"/></returns>
    private static bool TryGetFixableFieldDeclaration(SyntaxNode root, Diagnostic diagnostic, out FieldDeclarationSyntax fieldDeclaration)
    {
        fieldDeclaration = root.FindToken(diagnostic.Location.SourceSpan.Start)
                               .Parent
                               ?.AncestorsAndSelf()
                               .OfType<FieldDeclarationSyntax>()
                               .FirstOrDefault();

        return fieldDeclaration != null
               && ReihitsuFormatter.SplitsFieldDeclaration(fieldDeclaration);
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH7101DoNotCombineFieldsAnalyzer.DiagnosticId];

    /// <inheritdoc/>
    public sealed override FixAllProvider GetFixAllProvider()
    {
        return FixAllProvider.Create(ApplyFixAllAsync);
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
            if (TryGetFixableFieldDeclaration(syntaxRoot, diagnostic, out var fieldDeclaration))
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