using System.Collections.Immutable;
using System.Composition;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Analyzer.Rules.Naming;

/// <summary>
/// Providing fixes for <see cref="RH0201TypeNameShouldMatchFileNameAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH0201TypeNameShouldMatchFileNameCodeFixProvider))]
public class RH0201TypeNameShouldMatchFileNameCodeFixProvider : CodeFixProvider
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
        var newFileName = GetExpectedFileName(typeDeclaration) + ".cs";
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
    /// Gets the identifier token from a type declaration
    /// </summary>
    /// <param name="typeDeclaration">Type declaration</param>
    /// <returns>The identifier token</returns>
    private static SyntaxToken GetIdentifier(MemberDeclarationSyntax typeDeclaration)
    {
        return typeDeclaration switch
               {
                   BaseTypeDeclarationSyntax baseType => baseType.Identifier,
                   DelegateDeclarationSyntax delegateType => delegateType.Identifier,
                   _ => default
               };
    }

    /// <summary>
    /// Gets the expected filename for the given type declaration, formatting generic type parameters with curly braces
    /// </summary>
    /// <param name="typeDeclaration">Type declaration</param>
    /// <returns>Expected filename without extension</returns>
    private static string GetExpectedFileName(MemberDeclarationSyntax typeDeclaration)
    {
        var typeName = GetIdentifier(typeDeclaration).Text;

        var typeParameterList = typeDeclaration switch
                                {
                                    TypeDeclarationSyntax typeSyntax => typeSyntax.TypeParameterList,
                                    DelegateDeclarationSyntax delegateSyntax => delegateSyntax.TypeParameterList,
                                    _ => null
                                };

        if (typeParameterList is { Parameters.Count: > 0 })
        {
            typeName += "{" + string.Join(",", typeParameterList.Parameters.Select(p => p.Identifier.Text)) + "}";
        }

        return typeName;
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH0201TypeNameShouldMatchFileNameAnalyzer.DiagnosticId];

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

                if (typeDeclaration != null)
                {
                    context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH0201Title,
                                                              ct => ApplyCodeFixAsync(context.Document, typeDeclaration, ct),
                                                              nameof(RH0201TypeNameShouldMatchFileNameCodeFixProvider)),
                                            diagnostic);
                }
            }
        }
    }

    #endregion // CodeFixProvider
}