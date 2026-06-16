using System;
using System.Collections.Immutable;
using System.Composition;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Analyzer.Rules.Naming;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Naming;

/// <summary>
/// Providing fixes for <see cref="RH4001TypeNameShouldMatchFileNameAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH4001TypeNameShouldMatchFileNameCodeFixProvider))]
public class RH4001TypeNameShouldMatchFileNameCodeFixProvider : CodeFixProvider
{
    #region Methods

    /// <summary>
    /// Applying code fix by renaming the document
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="typeDeclaration">Type declaration node</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private static async Task<Solution> ApplyCodeFixAsync(Document document, MemberDeclarationSyntax typeDeclaration, CancellationToken cancellationToken)
    {
        var newFileName = GetNewFileName(document, typeDeclaration);
        var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);

        string newFilePath = null;

        if (document.FilePath != null)
        {
            var directory = Path.GetDirectoryName(document.FilePath);

            newFilePath = Path.Combine(directory ?? string.Empty, newFileName);
        }

        var solution = document.Project.Solution;
        var projectId = document.Project.Id;
        var folders = document.Folders;

        solution = solution.RemoveDocument(document.Id);

        var newDocumentId = DocumentId.CreateNewId(projectId);

        solution = solution.AddDocument(newDocumentId, newFileName, sourceText, folders, newFilePath);

        return solution;
    }

    /// <summary>
    /// Determines the file name the document would be renamed to
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="typeDeclaration">Type declaration node</param>
    /// <returns>The target file name including the extension</returns>
    private static string GetNewFileName(Document document, MemberDeclarationSyntax typeDeclaration)
    {
        return document.FilePath is null
                   ? RH4001TypeNameShouldMatchFileNameHelper.GetExpectedFileNameStem(typeDeclaration) + ".cs"
                   : RH4001TypeNameShouldMatchFileNameHelper.GetRenamedFileName(document.FilePath, typeDeclaration);
    }

    /// <summary>
    /// Determines whether the project already contains another document with the target file name in the same folders
    /// </summary>
    /// <param name="document">Document that would be renamed</param>
    /// <param name="newFileName">Target file name</param>
    /// <returns><see langword="true"/> when a colliding document already exists; otherwise <see langword="false"/></returns>
    private static bool TargetDocumentAlreadyExists(Document document, string newFileName)
    {
        foreach (var existingDocument in document.Project.Documents)
        {
            if (existingDocument.Id == document.Id)
            {
                continue;
            }

            if (string.Equals(existingDocument.Name, newFileName, StringComparison.OrdinalIgnoreCase)
                && existingDocument.Folders.SequenceEqual(document.Folders))
            {
                return true;
            }
        }

        return false;
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH4001TypeNameShouldMatchFileNameAnalyzer.DiagnosticId];

    /// <inheritdoc/>
    public sealed override FixAllProvider GetFixAllProvider()
    {
        return null;
    }

    /// <inheritdoc/>
    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

        if (root != null)
        {
            foreach (var diagnostic in context.Diagnostics)
            {
                var node = root.FindNode(diagnostic.Location.SourceSpan);

                var typeDeclaration = (MemberDeclarationSyntax)node.FirstAncestorOrSelf<BaseTypeDeclarationSyntax>() ?? node.FirstAncestorOrSelf<DelegateDeclarationSyntax>();

                // Renaming would overwrite an existing document when the project already contains a file with the
                // target name, so the fix is skipped to avoid silently replacing unrelated source on save
                if (typeDeclaration != null
                    && TargetDocumentAlreadyExists(context.Document, GetNewFileName(context.Document, typeDeclaration)) == false)
                {
                    context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH4001Title,
                                                              ct => ApplyCodeFixAsync(context.Document, typeDeclaration, ct),
                                                              nameof(RH4001TypeNameShouldMatchFileNameCodeFixProvider)),
                                            diagnostic);
                }
            }
        }
    }

    #endregion // CodeFixProvider
}