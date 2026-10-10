using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Core;
using Reihitsu.Formatter;

namespace Reihitsu.Analyzer.CodeFixes.Base;

/// <summary>
/// Base class for code-fix providers that convert empty type declarations to semicolon declarations
/// </summary>
public abstract class EmptyTypeDeclarationShouldUseSemicolonCodeFixProviderBase : CodeFixProvider
{
    #region Properties

    /// <summary>
    /// Diagnostic ID handled by this provider
    /// </summary>
    protected abstract string DiagnosticId { get; }

    /// <summary>
    /// Title shown for the registered code action
    /// </summary>
    protected abstract string CodeFixTitle { get; }

    /// <summary>
    /// Expected declaration kind
    /// </summary>
    protected abstract SyntaxKind DeclarationKind { get; }

    /// <summary>
    /// Minimum required language version
    /// </summary>
    protected abstract LanguageVersion MinimumLanguageVersion { get; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Applies the code fix
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="typeDeclaration">Type declaration</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated document</returns>
    private static async Task<Document> ApplyCodeFixAsync(Document document, TypeDeclarationSyntax typeDeclaration, CancellationToken cancellationToken)
    {
        return await ReihitsuFormatter.FormatNodeInDocumentAsync(document, typeDeclaration, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Tries to resolve the diagnostic target type declaration
    /// </summary>
    /// <param name="root">Syntax root</param>
    /// <param name="diagnostic">Diagnostic</param>
    /// <param name="declarationKind">Expected declaration kind</param>
    /// <param name="typeDeclaration">Resolved type declaration</param>
    /// <returns><see langword="true"/> if the type declaration was found; otherwise, <see langword="false"/></returns>
    private static bool TryGetTypeDeclaration(SyntaxNode root, Diagnostic diagnostic, SyntaxKind declarationKind, out TypeDeclarationSyntax typeDeclaration)
    {
        var diagnosticNode = root.FindToken(diagnostic.Location.SourceSpan.Start).Parent;
        typeDeclaration = diagnosticNode?.AncestorsAndSelf()
                                        .OfType<TypeDeclarationSyntax>()
                                        .FirstOrDefault(candidate => candidate.IsKind(declarationKind));

        return typeDeclaration != null;
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [DiagnosticId];

    /// <inheritdoc/>
    public sealed override FixAllProvider GetFixAllProvider()
    {
        return WellKnownFixAllProviders.BatchFixer;
    }

    /// <inheritdoc/>
    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

        if (root == null)
        {
            return;
        }

        foreach (var diagnostic in context.Diagnostics)
        {
            if (TryGetTypeDeclaration(root, diagnostic, DeclarationKind, out var typeDeclaration)
                && EmptyTypeDeclarationSemicolonAnalysisUtilities.CanConvertSafely(typeDeclaration, DeclarationKind, MinimumLanguageVersion))
            {
                context.RegisterCodeFix(CodeAction.Create(CodeFixTitle,
                                                          token => ApplyCodeFixAsync(context.Document, typeDeclaration, token),
                                                          GetType().Name),
                                        diagnostic);
            }
        }
    }

    #endregion // CodeFixProvider
}