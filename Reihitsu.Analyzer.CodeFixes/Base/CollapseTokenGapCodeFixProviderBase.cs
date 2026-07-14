using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Reihitsu.Analyzer.CodeFixes.Base;

/// <summary>
/// Base class for code fixes that collapse the whitespace gap between a token and its previous token,
/// the fix is not offered when the gap contains a comment or a preprocessor directive so user comments
/// and directives are never deleted
/// </summary>
public abstract class CollapseTokenGapCodeFixProviderBase : CommentSafeSpanReplacementCodeFixProviderBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="diagnosticId">Diagnostic ID</param>
    /// <param name="title">Code fix title</param>
    protected CollapseTokenGapCodeFixProviderBase(string diagnosticId, string title)
        : base(diagnosticId, title)
    {
    }

    #endregion // Constructor

    #region CommentSafeSpanReplacementCodeFixProviderBase

    /// <inheritdoc/>
    protected override bool TryGetReplacement(SyntaxNode root, SourceText sourceText, TextSpan diagnosticSpan, out TextSpan guardSpan, out TextSpan replacementSpan, out string replacementText)
    {
        var token = root.FindToken(diagnosticSpan.Start);

        guardSpan = replacementSpan = TextSpan.FromBounds(token.GetPreviousToken().Span.End, token.SpanStart);
        replacementText = string.Empty;

        return true;
    }

    #endregion // CommentSafeSpanReplacementCodeFixProviderBase
}