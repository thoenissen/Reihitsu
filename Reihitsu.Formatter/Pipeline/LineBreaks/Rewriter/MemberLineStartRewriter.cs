using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Formatter.Pipeline.Core.Utilities;
using Reihitsu.Formatter.Pipeline.LineBreaks.Utilities;

namespace Reihitsu.Formatter.Pipeline.LineBreaks.Rewriter;

/// <summary>
/// Ensures every member of a member declaration list (compilation unit, namespace, file-scoped namespace, and type
/// bodies) starts its own line instead of sharing a line with the token that precedes it
/// </summary>
internal sealed class MemberLineStartRewriter : CSharpSyntaxRewriter
{
    #region Fields

    /// <summary>
    /// The cancellation token
    /// </summary>
    private readonly CancellationToken _cancellationToken;

    /// <summary>
    /// The token gap normalizer
    /// </summary>
    private readonly TokenGapNormalizer _gapNormalizer;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="gapNormalizer">The token gap normalizer</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public MemberLineStartRewriter(TokenGapNormalizer gapNormalizer,
                                   CancellationToken cancellationToken)
    {
        _cancellationToken = cancellationToken;
        _gapNormalizer = gapNormalizer;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Determines whether a member is an empty top-level statement (a stray <c>;</c>)
    /// </summary>
    /// <param name="member">The member to inspect</param>
    /// <returns><see langword="true"/> if the member is an empty global statement; otherwise, <see langword="false"/></returns>
    private static bool IsEmptyGlobalStatement(MemberDeclarationSyntax member)
    {
        return member is GlobalStatementSyntax { Statement: EmptyStatementSyntax };
    }

    /// <summary>
    /// Moves every member whose first token shares a line with its preceding token onto a new line
    /// </summary>
    /// <typeparam name="TNode">The syntax node type owning the member list</typeparam>
    /// <param name="node">The node owning the member list</param>
    /// <param name="getMembers">Function that reads the member list from the owner</param>
    /// <param name="withMembers">Function that replaces the member list on the owner</param>
    /// <returns>The updated node</returns>
    /// <remarks>
    /// The preceding token is the member's actual token predecessor, so the first member is also covered when it
    /// follows a using directive, an extern alias, or a file-scoped namespace header. In namespace and type bodies
    /// whose braces have a brace-placement owner, the first member already starts a line after the opening brace;
    /// extension blocks have no such owner, so this rewriter also moves their first member. The inserted break
    /// carries no blank line; any comment in the gap stays on the preceding line, and a gap that already contains a
    /// line break — including one inside a multi-line comment — is left untouched. An empty top-level statement and
    /// the member that directly follows one are left in place, matching the statement-list rule in
    /// <see cref="LineBreakBlockRewriter"/>
    /// </remarks>
    private TNode EnsureMembersStartOnSeparateLines<TNode>(TNode node,
                                                           Func<TNode, SyntaxList<MemberDeclarationSyntax>> getMembers,
                                                           Func<TNode, SyntaxList<MemberDeclarationSyntax>, TNode> withMembers)
        where TNode : SyntaxNode
    {
        var members = getMembers(node).ToArray();
        var modified = false;

        for (var memberIndex = 0; memberIndex < members.Length; memberIndex++)
        {
            if (IsEmptyGlobalStatement(members[memberIndex])
                || (memberIndex > 0 && IsEmptyGlobalStatement(members[memberIndex - 1])))
            {
                continue;
            }

            var currentToken = members[memberIndex].GetFirstToken();
            var previousToken = currentToken.GetPreviousToken();

            if (previousToken.IsKind(SyntaxKind.None)
                || TokenGapUtilities.HasLineBreakBetween(previousToken, currentToken))
            {
                continue;
            }

            members[memberIndex] = _gapNormalizer.NormalizeGapBeforeToken(members[memberIndex], currentToken, blankLineCount: 0);
            modified = true;
        }

        return modified
                   ? withMembers(node, SyntaxFactory.List(members))
                   : node;
    }

    #endregion // Methods

    #region CSharpSyntaxVisitor

    /// <inheritdoc/>
    public override SyntaxNode VisitCompilationUnit(CompilationUnitSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (CompilationUnitSyntax)base.VisitCompilationUnit(node);

        return EnsureMembersStartOnSeparateLines(node, static owner => owner.Members, static (owner, members) => owner.WithMembers(members));
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitNamespaceDeclaration(NamespaceDeclarationSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (NamespaceDeclarationSyntax)base.VisitNamespaceDeclaration(node);

        return EnsureMembersStartOnSeparateLines(node, static owner => owner.Members, static (owner, members) => owner.WithMembers(members));
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitFileScopedNamespaceDeclaration(FileScopedNamespaceDeclarationSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (FileScopedNamespaceDeclarationSyntax)base.VisitFileScopedNamespaceDeclaration(node);

        return EnsureMembersStartOnSeparateLines(node, static owner => owner.Members, static (owner, members) => owner.WithMembers(members));
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (ClassDeclarationSyntax)base.VisitClassDeclaration(node);

        return EnsureMembersStartOnSeparateLines(node, static owner => owner.Members, static (owner, members) => owner.WithMembers(members));
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitStructDeclaration(StructDeclarationSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (StructDeclarationSyntax)base.VisitStructDeclaration(node);

        return EnsureMembersStartOnSeparateLines(node, static owner => owner.Members, static (owner, members) => owner.WithMembers(members));
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitInterfaceDeclaration(InterfaceDeclarationSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (InterfaceDeclarationSyntax)base.VisitInterfaceDeclaration(node);

        return EnsureMembersStartOnSeparateLines(node, static owner => owner.Members, static (owner, members) => owner.WithMembers(members));
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitRecordDeclaration(RecordDeclarationSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (RecordDeclarationSyntax)base.VisitRecordDeclaration(node);

        return EnsureMembersStartOnSeparateLines(node, static owner => owner.Members, static (owner, members) => owner.WithMembers(members));
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitExtensionBlockDeclaration(ExtensionBlockDeclarationSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (ExtensionBlockDeclarationSyntax)base.VisitExtensionBlockDeclaration(node);

        return EnsureMembersStartOnSeparateLines(node, static owner => owner.Members, static (owner, members) => owner.WithMembers(members));
    }

    #endregion // CSharpSyntaxVisitor
}