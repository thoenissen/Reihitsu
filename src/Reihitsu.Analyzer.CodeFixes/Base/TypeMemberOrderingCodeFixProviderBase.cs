using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.CodeFixes.Core;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.CodeFixes.Base;

/// <summary>
/// Base class for type member reordering code fixes
/// </summary>
public abstract class TypeMemberOrderingCodeFixProviderBase : ReihitsuCodeFixProviderBase
{
    #region Fields

    /// <summary>
    /// Analyzer that reports the diagnostics the code fix corrects
    /// </summary>
    private readonly DiagnosticAnalyzer _analyzer;

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
    /// <param name="analyzer">Analyzer that reports the diagnostics the code fix corrects</param>
    /// <param name="diagnosticId">Diagnostic ID</param>
    /// <param name="title">Code fix title</param>
    protected TypeMemberOrderingCodeFixProviderBase(DiagnosticAnalyzer analyzer, string diagnosticId, string title)
    {
        _analyzer = analyzer;
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
    /// Determines whether moving the member before the target member preserves the meaning of the code, in every build
    /// configuration. The move is refused when it would relocate a preprocessor directive away from the code it
    /// governs, while a region pair contained completely in the moved or the crossed half, or a <c>#pragma warning</c>
    /// or <c>#nullable</c> pair that ends its own effect inside one member, travels intact and is allowed
    /// (SHARED-ORDER-003 to SHARED-ORDER-006, SHARED-ORDER-011). It is refused as well when it would put two members of
    /// one context that each have a runtime initializer, or two instance fields of a type whose field order sets its
    /// layout, into the other order (SHARED-ORDER-007), and when the moved or the crossed span holds a conditional
    /// directive (SHARED-ORDER-008). Every member reordering fix applies both guards, because every one of them moves the
    /// member together with its leading trivia
    /// </summary>
    /// <param name="semanticModel">Semantic model of the document</param>
    /// <param name="typeDeclaration">Containing type declaration</param>
    /// <param name="memberDeclaration">Member declaration to move</param>
    /// <param name="targetMember">Target member</param>
    /// <returns><see langword="true"/> if the move is safe to offer</returns>
    private static bool IsMoveSafe(SemanticModel semanticModel, TypeDeclarationSyntax typeDeclaration, MemberDeclarationSyntax memberDeclaration, MemberDeclarationSyntax targetMember)
    {
        return OrderingDeclarationUtilities.MoveRangeContainsDirectives(typeDeclaration, memberDeclaration, targetMember) == false
               && semanticModel?.GetDeclaredSymbol(typeDeclaration) is INamedTypeSymbol typeSymbol
               && OrderingDeclarationUtilities.ChangesMemberOrder(typeDeclaration, typeSymbol, memberDeclaration, targetMember) == false;
    }

    /// <summary>
    /// Applies the code fix: the member moves in front of the target member as a move of whole lines, and nothing else of
    /// the type changes (SHARED-FIX-006, SHARED-FIX-007, SHARED-ORDER-009)
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="typeDeclaration">Containing type declaration</param>
    /// <param name="memberDeclaration">Member declaration to move</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated document</returns>
    private async Task<Document> ApplyCodeFixAsync(Document document, TypeDeclarationSyntax typeDeclaration, MemberDeclarationSyntax memberDeclaration, CancellationToken cancellationToken)
    {
        if (TryGetTargetMember(typeDeclaration, memberDeclaration, out var targetMember) == false
            || OrderingMoveSafety.TryCreateWholeLineMove(typeDeclaration.Members, memberDeclaration, targetMember, out var change) == false)
        {
            return document;
        }

        var text = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);

        return document.WithText(text.WithChanges(change));
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [_diagnosticId];

    /// <inheritdoc/>
    /// <remarks>
    /// The moves of one type are applied one after another, each registered on the code the moves in front of it produced, so
    /// that one application corrects every diagnostic whose fix is offered (SHARED-FIX-002)
    /// </remarks>
    public sealed override FixAllProvider GetFixAllProvider()
    {
        return DocumentOrderFixAllProvider.Create(_analyzer);
    }

    #endregion // CodeFixProvider

    #region ReihitsuCodeFixProviderBase

    /// <inheritdoc/>
    protected sealed override async Task RegisterCodeFixesCoreAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);

        if (root != null)
        {
            foreach (var diagnostic in context.Diagnostics)
            {
                if (OrderingDeclarationUtilities.TryGetContainingTypeAndMember(root, diagnostic, out var typeDeclaration, out var memberDeclaration)
                    && TryGetTargetMember(typeDeclaration, memberDeclaration, out var targetMember)
                    && IsMoveSafe(semanticModel, typeDeclaration, memberDeclaration, targetMember)
                    && OrderingMoveSafety.TryCreateWholeLineMove(typeDeclaration.Members, memberDeclaration, targetMember, out _))
                {
                    context.RegisterCodeFix(CodeAction.Create(_title,
                                                              token => ApplyCodeFixAsync(context.Document, typeDeclaration, memberDeclaration, token),
                                                              GetType().Name),
                                            diagnostic);
                }
            }
        }
    }

    #endregion // ReihitsuCodeFixProviderBase
}