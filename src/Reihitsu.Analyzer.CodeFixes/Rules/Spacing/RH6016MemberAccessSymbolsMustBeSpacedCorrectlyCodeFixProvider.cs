using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.CodeFixes.Core;
using Reihitsu.Analyzer.Rules.Spacing;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Spacing;

/// <summary>
/// Code fix provider for <see cref="RH6016MemberAccessSymbolsMustBeSpacedCorrectlyAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH6016MemberAccessSymbolsMustBeSpacedCorrectlyCodeFixProvider))]
public class RH6016MemberAccessSymbolsMustBeSpacedCorrectlyCodeFixProvider : CommentSafeSpanReplacementCodeFixProviderBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH6016MemberAccessSymbolsMustBeSpacedCorrectlyCodeFixProvider()
        : base(RH6016MemberAccessSymbolsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, CodeFixResources.RH6016Title)
    {
    }

    #endregion // Constructor

    #region CodeFixProvider

    /// <inheritdoc/>
    /// <remarks>
    /// The replacements of a conditional access's <c>?</c> and the <c>.</c> behind it share the gap between them, so the
    /// fixes are applied one after another, each registered on the code the fixes in front of it produced (SHARED-FIX-002)
    /// </remarks>
    public sealed override FixAllProvider GetFixAllProvider()
    {
        return DocumentOrderFixAllProvider.Create(new RH6016MemberAccessSymbolsMustBeSpacedCorrectlyAnalyzer());
    }

    #endregion // CodeFixProvider

    #region CommentSafeSpanReplacementCodeFixProviderBase

    /// <inheritdoc/>
    protected override bool TryGetReplacement(SyntaxNode root, SourceText sourceText, TextSpan diagnosticSpan, out TextSpan guardSpan, out TextSpan replacementSpan, out string replacementText)
    {
        var token = root.FindToken(diagnosticSpan.Start);
        var previousToken = token.GetPreviousToken();
        var nextToken = token.GetNextToken();

        // Mirrors RH6016MemberAccessSymbolsMustBeSpacedCorrectlyAnalyzer: only a same-line side can carry the
        // flagged unwanted space, so the fix must never reach across a line break into the other side.
        var (hasLeadingSpace, hasTrailingSpace) = AdjacentTokenSpacingUtilities.DetermineSameLineAdjacentSpacing(token, sourceText);

        if (hasLeadingSpace == false && hasTrailingSpace == false)
        {
            guardSpan = replacementSpan = default;
            replacementText = string.Empty;

            return false;
        }

        if (hasLeadingSpace && hasTrailingSpace)
        {
            // The flagged symbol is a dot, the question mark of a conditional access or a pointer arrow, so
            // the replacement has to carry the token's own text rather than a hard-coded "."
            guardSpan = replacementSpan = TextSpan.FromBounds(previousToken.Span.End, nextToken.SpanStart);
            replacementText = token.Text;
        }
        else if (hasLeadingSpace)
        {
            guardSpan = replacementSpan = TextSpan.FromBounds(previousToken.Span.End, token.SpanStart);
            replacementText = string.Empty;
        }
        else
        {
            guardSpan = replacementSpan = TextSpan.FromBounds(token.Span.End, nextToken.SpanStart);
            replacementText = string.Empty;
        }

        return true;
    }

    #endregion // CommentSafeSpanReplacementCodeFixProviderBase
}