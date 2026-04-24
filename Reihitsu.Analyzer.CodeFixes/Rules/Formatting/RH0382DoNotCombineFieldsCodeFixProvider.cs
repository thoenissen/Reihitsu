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

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// Code fix provider for <see cref="RH0382DoNotCombineFieldsAnalyzer"/>.
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH0382DoNotCombineFieldsCodeFixProvider))]
public class RH0382DoNotCombineFieldsCodeFixProvider : CodeFixProvider
{
    #region Methods

    /// <summary>
    /// Applies the code fix.
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

        var syntaxRoot = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (syntaxRoot == null)
        {
            return document;
        }

        var replacementMembers = new MemberDeclarationSyntax[fieldDeclaration.Declaration.Variables.Count];

        for (var variableIndex = 0; variableIndex < fieldDeclaration.Declaration.Variables.Count; variableIndex++)
        {
            var variable = fieldDeclaration.Declaration.Variables[variableIndex];
            var updatedFieldDeclaration = fieldDeclaration.WithDeclaration(fieldDeclaration.Declaration.WithVariables(SyntaxFactory.SingletonSeparatedList(variable)));

            if (variableIndex == 0)
            {
                replacementMembers[variableIndex] = updatedFieldDeclaration.WithTrailingTrivia(SyntaxFactory.ElasticMarker);

                continue;
            }

            var trailingTrivia = variableIndex == fieldDeclaration.Declaration.Variables.Count - 1
                                     ? fieldDeclaration.GetTrailingTrivia()
                                     : new SyntaxTriviaList(SyntaxFactory.ElasticMarker);

            updatedFieldDeclaration = updatedFieldDeclaration.WithLeadingTrivia(SyntaxFactory.ElasticMarker);
            updatedFieldDeclaration = updatedFieldDeclaration.WithTrailingTrivia(trailingTrivia);
            replacementMembers[variableIndex] = updatedFieldDeclaration;
        }

        var formattingAnnotation = new SyntaxAnnotation();
        var memberIndex = typeDeclaration.Members.IndexOf(fieldDeclaration);
        var updatedMembers = typeDeclaration.Members.RemoveAt(memberIndex);
        updatedMembers = updatedMembers.InsertRange(memberIndex, replacementMembers);

        var updatedTypeDeclaration = typeDeclaration.WithMembers(updatedMembers).WithAdditionalAnnotations(formattingAnnotation);
        var updatedRoot = syntaxRoot.ReplaceNode(typeDeclaration, updatedTypeDeclaration);
        var updatedDocument = document.WithSyntaxRoot(updatedRoot);
        var formattedRoot = await updatedDocument.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        var formattedTypeDeclaration = formattedRoot?.GetAnnotatedNodes(formattingAnnotation).OfType<TypeDeclarationSyntax>().FirstOrDefault();

        return formattedTypeDeclaration == null
                   ? updatedDocument
                   : await ReihitsuFormatter.FormatNodeInDocumentAsync(updatedDocument, formattedTypeDeclaration, cancellationToken).ConfigureAwait(false);
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH0382DoNotCombineFieldsAnalyzer.DiagnosticId];

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
            var fieldDeclaration = syntaxRoot.FindToken(diagnostic.Location.SourceSpan.Start).Parent?.AncestorsAndSelf()
                                             .OfType<FieldDeclarationSyntax>()
                                             .FirstOrDefault();

            if (fieldDeclaration != null)
            {
                context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH0382Title,
                                                          token => ApplyCodeFixAsync(context.Document, fieldDeclaration, token),
                                                          nameof(RH0382DoNotCombineFieldsCodeFixProvider)),
                                        diagnostic);
            }
        }
    }

    #endregion // CodeFixProvider
}