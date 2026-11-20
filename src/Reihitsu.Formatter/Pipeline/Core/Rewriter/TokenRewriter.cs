using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Reihitsu.Formatter.Pipeline.Core.Rewriter;

/// <summary>
/// Rewrites every token of a node in one walk of its tree. A replacement call such as <c>ReplaceTokens</c> costs time
/// that grows with the square of its replacement set, so a phase that edits tokens anywhere in the file walks the tree
/// with this rewriter instead, at a cost that grows with the number of tokens. Like <c>ReplaceTokens</c> over
/// <c>DescendantTokens()</c>, it visits the tokens of the code in text order, never the tokens inside structured trivia
/// such as documentation comments and directives, and hands each rewrite the token of the input tree, so its position
/// and its neighbours are those of the input
/// </summary>
internal sealed class TokenRewriter : CSharpSyntaxRewriter
{
    #region Fields

    /// <summary>
    /// The rewrite of one token
    /// </summary>
    private readonly Func<SyntaxToken, SyntaxToken> _rewrite;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="rewrite">The rewrite of one token</param>
    private TokenRewriter(Func<SyntaxToken, SyntaxToken> rewrite)
        : base(visitIntoStructuredTrivia: false)
    {
        _rewrite = rewrite;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Rewrites every token of a node, each from the token of the input tree. A node in which no token changes is
    /// returned as it is
    /// </summary>
    /// <param name="root">The node whose tokens are rewritten</param>
    /// <param name="rewrite">The rewrite of one token; it receives the token of the input tree and returns its replacement</param>
    /// <returns>The node with every token replaced by its rewrite</returns>
    public static SyntaxNode RewriteTokens(SyntaxNode root, Func<SyntaxToken, SyntaxToken> rewrite)
    {
        return new TokenRewriter(rewrite).Visit(root);
    }

    #endregion // Methods

    #region CSharpSyntaxRewriter

    /// <inheritdoc/>
    public override SyntaxToken VisitToken(SyntaxToken token)
    {
        return _rewrite(token);
    }

    #endregion // CSharpSyntaxRewriter
}