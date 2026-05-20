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

    /// <summary>
    /// Determines whether the trivia list contains content that should be preserved when removing a separator
    /// </summary>
    /// <param name="triviaList">Trivia list</param>
    /// <returns><see langword="true"/> if the trivia contains comments, directives, or other non-formatting content</returns>
    private static bool ContainsNonFormattingTrivia(SyntaxTriviaList triviaList)
    {
        foreach (var trivia in triviaList)
        {
            if (trivia.IsKind(SyntaxKind.WhitespaceTrivia) == false
                && trivia.IsKind(SyntaxKind.EndOfLineTrivia) == false)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Gets the separator trivia that should be preserved after removing the trailing comma
    /// </summary>
    /// <param name="separator">Separator token</param>
    /// <returns>The trivia that should stay attached to the final enum member</returns>
    private static SyntaxTriviaList GetTriviaToPreserve(SyntaxToken separator)
    {
        var triviaToPreserve = SyntaxFactory.TriviaList();

        if (ContainsNonFormattingTrivia(separator.LeadingTrivia))
        {
            triviaToPreserve = triviaToPreserve.AddRange(separator.LeadingTrivia);
        }

        return triviaToPreserve.AddRange(separator.TrailingTrivia);
    }

    /// <summary>
    /// Adds preserved separator trivia to the final token of the enum member
    /// </summary>
    /// <param name="member">Enum member</param>
    /// <param name="triviaToPreserve">Trivia to preserve</param>
    /// <returns>The updated enum member</returns>
    private static EnumMemberDeclarationSyntax PreserveTrailingTrivia(EnumMemberDeclarationSyntax member, SyntaxTriviaList triviaToPreserve)
    {
        if (triviaToPreserve.Count == 0)
        {
            return member;
        }

        var lastToken = member.GetLastToken();
        var updatedLastToken = lastToken.WithTrailingTrivia(lastToken.TrailingTrivia.AddRange(triviaToPreserve));

        return member.ReplaceToken(lastToken, updatedLastToken);
    }

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
        var updatedLastMember = PreserveTrailingTrivia(lastMember, GetTriviaToPreserve(lastSeparator));
        var updatedMembers = node.Members.Replace(lastMember, updatedLastMember);
        var updatedMembersAndSeparators = updatedMembers.GetWithSeparators().RemoveAt(updatedMembers.GetWithSeparators().Count - 1);

        return node.WithMembers(SyntaxFactory.SeparatedList<EnumMemberDeclarationSyntax>(updatedMembersAndSeparators));
    }

    #endregion // CSharpSyntaxVisitor
}