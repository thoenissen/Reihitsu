using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Formatter.Pipeline.StructuralTransforms;

/// <summary>
/// Removes trailing commas from final enum members while preserving attached trivia
/// </summary>
internal sealed class EnumTrailingCommaRemovalTransform : CSharpSyntaxRewriter
{
    #region Fields

    /// <summary>
    /// Cancellation token
    /// </summary>
    private readonly CancellationToken _cancellationToken;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    public EnumTrailingCommaRemovalTransform(CancellationToken cancellationToken)
    {
        _cancellationToken = cancellationToken;
    }

    #endregion // Constructor

    #region Methods

    #endregion // Methods

    #region CSharpSyntaxVisitor

    /// <inheritdoc/>
    public override SyntaxNode VisitEnumDeclaration(EnumDeclarationSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (EnumDeclarationSyntax)base.VisitEnumDeclaration(node);

        if (node == null || node.Members.Count == 0 || node.Members.SeparatorCount != node.Members.Count)
        {
            return node;
        }

        var lastMember = node.Members[node.Members.Count - 1];
        var lastSeparator = node.Members.GetSeparator(node.Members.SeparatorCount - 1);
        var updatedLastMember = TrailingCommaRemovalUtilities.PreserveTrailingTrivia(lastMember, TrailingCommaRemovalUtilities.GetTriviaToPreserve(lastSeparator));
        var updatedMembers = node.Members.Replace(lastMember, updatedLastMember);
        var updatedMembersAndSeparators = updatedMembers.GetWithSeparators().RemoveAt(updatedMembers.GetWithSeparators().Count - 1);

        return node.WithMembers(SyntaxFactory.SeparatedList<EnumMemberDeclarationSyntax>(updatedMembersAndSeparators));
    }

    #endregion // CSharpSyntaxVisitor
}