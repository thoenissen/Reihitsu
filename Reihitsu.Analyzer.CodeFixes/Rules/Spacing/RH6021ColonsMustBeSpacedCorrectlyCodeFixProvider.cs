using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.Rules.Spacing;

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

        guardSpan = replacementSpan = TextSpan.FromBounds(token.GetPreviousToken().Span.End, token.GetNextToken().SpanStart);
        replacementText = " : ";

        return true;
    }

    #endregion // CommentSafeSpanReplacementCodeFixProviderBase
}