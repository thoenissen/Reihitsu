using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.Rules.Spacing;

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

        guardSpan = replacementSpan = TextSpan.FromBounds(token.GetPreviousToken().Span.End, token.GetNextToken().SpanStart);
        replacementText = ".";

        return true;
    }

    #endregion // CommentSafeSpanReplacementCodeFixProviderBase
}