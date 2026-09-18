using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Core;

namespace Reihitsu.Analyzer.CodeFixes.Base;

/// <summary>
/// Base class for code fixes that delete a reported whitespace run between two tokens; the fix is
/// not offered when the surrounding token gap contains a comment, because deleting the whitespace
/// would otherwise either remove the comment or glue it to a neighbouring token, and not offered when
/// the reported span itself is not whitespace-only, so a future analyzer defect degrades to no fix
/// being offered instead of deleting non-whitespace text
/// <para>
/// This guard inspects the whole token gap around the deleted span, not only the span itself, so it can
/// withhold a fix for input the deleted span alone would never touch. Choose
/// <see cref="WhitespaceSpanRemovalCodeFixProviderBase"/> instead when the fix must stay offered for every
/// input the analyzer can report and no comment/directive check beyond the deleted span itself is required
/// </para>
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
        replacementSpan = diagnosticSpan;
        replacementText = string.Empty;

        if (FormattingTextAnalysisUtilities.IsWhitespaceOnly(sourceText, diagnosticSpan) == false)
        {
            guardSpan = default;

            return false;
        }

        var token = root.FindToken(diagnosticSpan.Start);

        // FindToken attributes leading trivia to the token it precedes, so a diagnostic span inside a token's
        // leading trivia resolves to the token *after* the edited gap, not the one before it. Deciding which
        // neighbouring token brackets the edited gap from diagnosticSpan.Start versus token.SpanStart, rather
        // than always treating the resolved token as the preceding one, keeps the guard aligned with the edit
        // on both sides of that boundary.
        if (diagnosticSpan.Start < token.SpanStart)
        {
            var previousToken = token.GetPreviousToken();

            // No token precedes the document's first token, so there is no gap for two tokens to bracket here.
            // Withholding is a refusal to guess an edited gap that does not exist, not a guard against a throw;
            // the bounds this branch would otherwise compute do not throw.
            if (previousToken.IsKind(SyntaxKind.None))
            {
                guardSpan = default;

                return false;
            }

            guardSpan = TextSpan.FromBounds(previousToken.Span.End, token.SpanStart);
        }
        else
        {
            var nextToken = token.GetNextToken();

            // Unlike the branch above, this guards a real throw: a diagnostic span at the end of a truncated
            // document resolves to the last non-zero-width token, whose next token does not exist, and computing
            // bounds from that missing token's start would throw because it defaults to position zero.
            if (nextToken.IsKind(SyntaxKind.None))
            {
                guardSpan = default;

                return false;
            }

            guardSpan = TextSpan.FromBounds(token.Span.End, nextToken.SpanStart);
        }

        return CanOfferFix(root, diagnosticSpan);
    }

    #endregion // CommentSafeSpanReplacementCodeFixProviderBase
}