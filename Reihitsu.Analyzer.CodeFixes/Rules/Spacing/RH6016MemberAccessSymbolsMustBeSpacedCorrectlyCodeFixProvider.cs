using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.CodeFixes.Base;
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
            guardSpan = replacementSpan = TextSpan.FromBounds(previousToken.Span.End, nextToken.SpanStart);
            replacementText = ".";
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