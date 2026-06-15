using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Core;
using Reihitsu.Formatter;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Layout;

/// <summary>
/// Code fix provider for <see cref="RH5402BracesForMultiLineStatementsMustNotShareLineAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH5402BracesForMultiLineStatementsMustNotShareLineCodeFixProvider))]
public class RH5402BracesForMultiLineStatementsMustNotShareLineCodeFixProvider : CommentSafeSpanReplacementCodeFixProviderBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5402BracesForMultiLineStatementsMustNotShareLineCodeFixProvider()
        : base(RH5402BracesForMultiLineStatementsMustNotShareLineAnalyzer.DiagnosticId, CodeFixResources.RH5402Title)
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Gets the leading whitespace for the specified line
    /// </summary>
    /// <param name="lineText">Line text</param>
    /// <returns>The leading whitespace</returns>
    private static string GetIndentation(string lineText)
    {
        var length = 0;

        while (length < lineText.Length
               && char.IsWhiteSpace(lineText[length]))
        {
            length++;
        }

        return lineText.Substring(0, length);
    }

    #endregion // Methods

    #region CommentSafeSpanReplacementCodeFixProviderBase

    /// <inheritdoc/>
    protected override bool TryGetReplacement(SyntaxNode root, SourceText sourceText, TextSpan diagnosticSpan, out TextSpan guardSpan, out TextSpan replacementSpan, out string replacementText)
    {
        var token = root.FindToken(diagnosticSpan.Start);
        var owner = token.Parent?.FirstAncestorOrSelf<BlockSyntax>()?.Parent ?? token.Parent;
        var line = sourceText.Lines.GetLineFromPosition(owner?.SpanStart ?? token.SpanStart);

        guardSpan = replacementSpan = TextSpan.FromBounds(token.GetPreviousToken().Span.End, token.SpanStart);
        replacementText = ReihitsuFormatterHelpers.DetectEndOfLine(root) + GetIndentation(FormattingTextAnalysisUtilities.GetLineText(sourceText, line));

        return true;
    }

    #endregion // CommentSafeSpanReplacementCodeFixProviderBase
}