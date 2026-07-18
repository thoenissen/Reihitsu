using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.Rules.Spacing;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Spacing;

/// <summary>
/// Code fix provider for <see cref="RH6021ColonsMustBeSpacedCorrectlyAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH6021ColonsMustBeSpacedCorrectlyCodeFixProvider))]
public class RH6021ColonsMustBeSpacedCorrectlyCodeFixProvider : CommentSafeSpanReplacementCodeFixProviderBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH6021ColonsMustBeSpacedCorrectlyCodeFixProvider()
        : base(RH6021ColonsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, CodeFixResources.RH6021Title)
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

        // Mirrors RH6021ColonsMustBeSpacedCorrectlyAnalyzer: a line-broken side already counts as spaced, so the
        // fix must never touch it - only the flagged same-line side(s) may be rewritten.
        var (hasLeadingSpace, hasTrailingSpace) = AdjacentTokenSpacingUtilities.DetermineLineBreakTolerantSpacing(token, sourceText);

        if (hasLeadingSpace && hasTrailingSpace)
        {
            guardSpan = replacementSpan = default;
            replacementText = string.Empty;

            return false;
        }

        if (hasLeadingSpace == false && hasTrailingSpace == false)
        {
            guardSpan = replacementSpan = TextSpan.FromBounds(previousToken.Span.End, nextToken.SpanStart);
            replacementText = " : ";
        }
        else if (hasLeadingSpace == false)
        {
            guardSpan = replacementSpan = TextSpan.FromBounds(previousToken.Span.End, token.SpanStart);
            replacementText = " ";
        }
        else
        {
            guardSpan = replacementSpan = TextSpan.FromBounds(token.Span.End, nextToken.SpanStart);
            replacementText = " ";
        }

        return true;
    }

    #endregion // CommentSafeSpanReplacementCodeFixProviderBase
}