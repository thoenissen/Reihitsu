using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Reihitsu.Analyzer.CodeFixes.Base;

/// <summary>
/// Base class for code fixes that delete a reported whitespace run between two tokens; the fix is
/// not offered when the surrounding token gap contains a comment, because deleting the whitespace
/// would otherwise either remove the comment or glue it to a neighbouring token
/// </summary>
public abstract class RemoveWhitespaceRunCodeFixProviderBase : CommentSafeSpanReplacementCodeFixProviderBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="diagnosticId">Diagnostic ID</param>
    /// <param name="title">Code fix title</param>
    protected RemoveWhitespaceRunCodeFixProviderBase(string diagnosticId, string title)
        : base(diagnosticId, title)
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Determines whether the fix can be offered for a specific diagnostic; derived classes can add
    /// additional safety checks beyond the shared comment guard
    /// </summary>
    /// <param name="root">Syntax root</param>
    /// <param name="diagnosticSpan">Diagnostic span</param>
    /// <returns><see langword="true"/> when the fix should be offered; otherwise <see langword="false"/></returns>
    protected virtual bool CanOfferFix(SyntaxNode root, TextSpan diagnosticSpan)
    {
        return true;
    }

    #endregion // Methods

    #region CommentSafeSpanReplacementCodeFixProviderBase

    /// <inheritdoc/>
    protected override bool TryGetReplacement(SyntaxNode root, SourceText sourceText, TextSpan diagnosticSpan, out TextSpan guardSpan, out TextSpan replacementSpan, out string replacementText)
    {
        var precedingToken = root.FindToken(diagnosticSpan.Start);

        guardSpan = TextSpan.FromBounds(precedingToken.Span.End, precedingToken.GetNextToken().SpanStart);
        replacementSpan = diagnosticSpan;
        replacementText = string.Empty;

        return CanOfferFix(root, diagnosticSpan);
    }

    #endregion // CommentSafeSpanReplacementCodeFixProviderBase
}