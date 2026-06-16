using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Core;
using Reihitsu.Formatter;

namespace Reihitsu.Analyzer.CodeFixes.Base;

/// <summary>
/// Base class for type member reordering code fixes
/// </summary>
public abstract class TypeMemberOrderingCodeFixProviderBase : CodeFixProvider
{
    #region Fields

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    private readonly string _diagnosticId;

    /// <summary>
    /// Code fix title
    /// </summary>
    private readonly string _title;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="diagnosticId">Diagnostic ID</param>
    /// <param name="title">Code fix title</param>
    protected TypeMemberOrderingCodeFixProviderBase(string diagnosticId, string title)
    {
        _diagnosticId = diagnosticId;
        _title = title;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Tries to find the target member that should precede the current member
    /// </summary>
    /// <param name="typeDeclaration">Containing type declaration</param>
    /// <param name="memberDeclaration">Member declaration to move</param>
    /// <param name="targetMember">Target member</param>
    /// <returns><see langword="true"/> if a target member was found</returns>
    protected abstract bool TryGetTargetMember(TypeDeclarationSyntax typeDeclaration, MemberDeclarationSyntax memberDeclaration, out MemberDeclarationSyntax targetMember);

    /// <summary>
    /// Determines whether moving the member before the target member preserves the meaning of the code.
    /// The base implementation refuses moves that would scramble preprocessor directives sitting in the
    /// affected leading trivia; derived classes can add further restrictions
    /// </summary>
    /// <param name="typeDeclaration">Containing type declaration</param>
    /// <param name="memberDeclaration">Member declaration to move</param>
    /// <param name="targetMember">Target member</param>
    /// <returns><see langword="true"/> if the move is safe to offer</returns>
    protected virtual bool IsMoveSafe(TypeDeclarationSyntax typeDeclaration, MemberDeclarationSyntax memberDeclaration, MemberDeclarationSyntax targetMember)
    {
        return OrderingDeclarationUtilities.MoveRangeContainsDirectives(typeDeclaration, memberDeclaration, targetMember) == false;
    }

    /// <summary>
    /// Applies the code fix
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="typeDeclaration">Containing type declaration</param>
    /// <param name="memberDeclaration">Member declaration to move</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated document</returns>
    private async Task<Document> ApplyCodeFixAsync(Document document, TypeDeclarationSyntax typeDeclaration, MemberDeclarationSyntax memberDeclaration, CancellationToken cancellationToken)
    {
        if (TryGetTargetMember(typeDeclaration, memberDeclaration, out var targetMember) == false)
        {
            return document;
        }

        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (root == null)
        {
            return document;
        }

        var formattingAnnotation = new SyntaxAnnotation();
        var updatedTypeDeclaration = OrderingDeclarationUtilities.MoveMemberBefore(typeDeclaration, memberDeclaration, targetMember)
                                                                 .WithAdditionalAnnotations(formattingAnnotation);
        var updatedRoot = root.ReplaceNode(typeDeclaration, updatedTypeDeclaration);
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
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [_diagnosticId];

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
                if (OrderingDeclarationUtilities.TryGetContainingTypeAndMember(root, diagnostic, out var typeDeclaration, out var memberDeclaration)
                    && TryGetTargetMember(typeDeclaration, memberDeclaration, out var targetMember)
                    && IsMoveSafe(typeDeclaration, memberDeclaration, targetMember))
                {
                    context.RegisterCodeFix(CodeAction.Create(_title,
                                                              token => ApplyCodeFixAsync(context.Document, typeDeclaration, memberDeclaration, token),
                                                              GetType().Name),
                                            diagnostic);
                }
            }
        }
    }

    #endregion // CodeFixProvider
}