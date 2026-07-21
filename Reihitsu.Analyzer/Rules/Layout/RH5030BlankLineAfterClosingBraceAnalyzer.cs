using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Layout;

/// <summary>
/// RH5030: Require a blank line after a closing brace before the next statement
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH5030BlankLineAfterClosingBraceAnalyzer : DiagnosticAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH5030";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5030BlankLineAfterClosingBraceAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Layout, nameof(AnalyzerResources.RH5030Title), nameof(AnalyzerResources.RH5030MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Checks whether a blank line already exists in the combined trivia between two tokens. A blank line
    /// requires two consecutive line breaks with nothing but whitespace between them, so a comment line between
    /// the tokens resets the run instead of counting toward it. Directive trivia such as <c>#endregion</c> carries
    /// its own trailing line break as part of its text rather than as a separate end-of-line trivia, so its text
    /// is scanned too (issue #440)
    /// </summary>
    /// <param name="trailingTrivia">Trailing trivia of the preceding token</param>
    /// <param name="leadingTrivia">Leading trivia of the following token</param>
    /// <returns><see langword="true"/> if a blank line is present; otherwise, <see langword="false"/></returns>
    private static bool HasBlankLineBetween(SyntaxTriviaList trailingTrivia, SyntaxTriviaList leadingTrivia)
    {
        var sawLineBreak = false;
        var lineHasContent = false;

        foreach (var trivia in trailingTrivia.Concat(leadingTrivia))
        {
            if (trivia.IsKind(SyntaxKind.WhitespaceTrivia))
            {
                continue;
            }

            if (trivia.IsKind(SyntaxKind.EndOfLineTrivia) == false)
            {
                lineHasContent = true;

                if (HasEmbeddedBlankLine(trivia.ToFullString(), ref sawLineBreak, ref lineHasContent))
                {
                    return true;
                }

                continue;
            }

            if (sawLineBreak && lineHasContent == false)
            {
                return true;
            }

            sawLineBreak = true;
            lineHasContent = false;
        }

        return false;
    }

    /// <summary>
    /// Scans the full text of a single trivia entry, such as a directive, for embedded line breaks and updates
    /// the running blank-line state accordingly
    /// </summary>
    /// <param name="text">Full text of the trivia entry</param>
    /// <param name="sawLineBreak">Tracks whether a line break has already been encountered</param>
    /// <param name="lineHasContent">Tracks whether the current logical line contains non-whitespace content</param>
    /// <returns><see langword="true"/> if a blank line is found within the trivia's own text</returns>
    private static bool HasEmbeddedBlankLine(string text, ref bool sawLineBreak, ref bool lineHasContent)
    {
        var index = 0;

        while (index < text.Length)
        {
            var lineBreakLength = GetLineBreakLength(text, index);

            if (lineBreakLength == 0)
            {
                index++;

                continue;
            }

            if (sawLineBreak && lineHasContent == false)
            {
                return true;
            }

            sawLineBreak = true;
            lineHasContent = index + lineBreakLength < text.Length;
            index += lineBreakLength;
        }

        return false;
    }

    /// <summary>
    /// Gets the length of the line-break sequence that starts at the specified index
    /// </summary>
    /// <param name="text">The text to inspect</param>
    /// <param name="index">The index to inspect</param>
    /// <returns>The length of the line-break sequence; otherwise, <c>0</c></returns>
    private static int GetLineBreakLength(string text, int index)
    {
        if (text[index] == '\r')
        {
            return index + 1 < text.Length && text[index + 1] == '\n' ? 2 : 1;
        }

        return text[index] == '\n' ? 1 : 0;
    }

    /// <summary>
    /// Analyzes a list of statements and reports a diagnostic when a statement that follows a closing
    /// brace is not preceded by a blank line
    /// </summary>
    /// <param name="context">Analysis context</param>
    /// <param name="statements">Statements to analyze</param>
    /// <param name="inSwitchSection">Whether the statements belong to a switch section</param>
    private void AnalyzeStatements(SyntaxNodeAnalysisContext context, SyntaxList<StatementSyntax> statements, bool inSwitchSection = false)
    {
        for (var statementIndex = 0; statementIndex < statements.Count - 1; statementIndex++)
        {
            var current = statements[statementIndex];
            var next = statements[statementIndex + 1];

            var lastToken = current.GetLastToken();

            if (lastToken.IsKind(SyntaxKind.CloseBraceToken) == false)
            {
                continue;
            }

            // Inside a switch section, no blank line is required before a break statement
            if (inSwitchSection && next is BreakStatementSyntax)
            {
                continue;
            }

            var nextFirstToken = next.GetFirstToken();

            if (HasBlankLineBetween(lastToken.TrailingTrivia, nextFirstToken.LeadingTrivia))
            {
                continue;
            }

            var lastLine = lastToken.GetLocation().GetLineSpan().EndLinePosition.Line;
            var nextLine = nextFirstToken.GetLocation().GetLineSpan().StartLinePosition.Line;

            // Skip pairs that are on the same line (e.g. inline blocks like if (x) { } Consume();)
            if (lastLine >= nextLine)
            {
                continue;
            }

            context.ReportDiagnostic(CreateDiagnostic(lastToken.GetLocation()));
        }
    }

    /// <summary>
    /// Analyzes a block
    /// </summary>
    /// <param name="context">Context</param>
    private void OnBlock(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not BlockSyntax block)
        {
            return;
        }

        AnalyzeStatements(context, block.Statements, inSwitchSection: block.Parent is SwitchSectionSyntax);
    }

    /// <summary>
    /// Analyzes a switch section
    /// </summary>
    /// <param name="context">Context</param>
    private void OnSwitchSection(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not SwitchSectionSyntax switchSection)
        {
            return;
        }

        AnalyzeStatements(context, switchSection.Statements, inSwitchSection: true);
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnBlock, SyntaxKind.Block);
        context.RegisterSyntaxNodeAction(OnSwitchSection, SyntaxKind.SwitchSection);
    }

    #endregion // DiagnosticAnalyzer
}