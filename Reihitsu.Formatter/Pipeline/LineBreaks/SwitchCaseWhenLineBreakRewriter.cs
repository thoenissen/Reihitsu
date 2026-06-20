using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Formatter.Pipeline.LineBreaks;

/// <summary>
/// Applies line-break rules for <c>when</c> guard clauses on <c>case</c> labels in switch statements.
/// The guard stays on the same line as the <c>case</c> label when the pattern fits on a single line.
/// When the pattern spans multiple lines the guard is wrapped onto its own line, where the indentation
/// phase aligns it four spaces past the <c>case</c> keyword
/// </summary>
internal sealed class SwitchCaseWhenLineBreakRewriter : CSharpSyntaxRewriter
{
    #region Fields

    /// <summary>
    /// The formatting context
    /// </summary>
    private readonly FormattingContext _context;

    /// <summary>
    /// The cancellation token
    /// </summary>
    private readonly CancellationToken _cancellationToken;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="context">The formatting context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public SwitchCaseWhenLineBreakRewriter(FormattingContext context,
                                           CancellationToken cancellationToken)
    {
        _context = context;
        _cancellationToken = cancellationToken;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Determines whether the pattern itself spans multiple lines, ignoring the surrounding trivia
    /// such as the line break that precedes a wrapped <c>when</c> keyword
    /// </summary>
    /// <param name="pattern">The case pattern</param>
    /// <returns><see langword="true"/> if the pattern text spans multiple lines; otherwise, <see langword="false"/></returns>
    private static bool PatternSpansMultipleLines(PatternSyntax pattern)
    {
        return pattern.ToString().IndexOf('\n') >= 0;
    }

    /// <summary>
    /// Normalizes the placement of the <c>when</c> guard relative to the case pattern. A single-line
    /// pattern keeps the guard inline, while a multi-line pattern wraps the guard onto its own line
    /// </summary>
    /// <param name="node">The case pattern switch label</param>
    /// <returns>The updated case pattern switch label</returns>
    private CasePatternSwitchLabelSyntax NormalizeWhenClausePlacement(CasePatternSwitchLabelSyntax node)
    {
        if (node.WhenClause == null)
        {
            return node;
        }

        if (PatternSpansMultipleLines(node.Pattern))
        {
            return WrapWhenClauseOntoOwnLine(node);
        }

        return CollapseWhenClauseOntoPatternLine(node);
    }

    /// <summary>
    /// Collapses the <c>when</c> keyword onto the line that ends the case pattern, leaving a single
    /// space before it. The join is refused when a comment sits between the pattern and the guard so
    /// the comment is not absorbed
    /// </summary>
    /// <param name="node">The case pattern switch label</param>
    /// <returns>The updated case pattern switch label</returns>
    private CasePatternSwitchLabelSyntax CollapseWhenClauseOntoPatternLine(CasePatternSwitchLabelSyntax node)
    {
        var whenKeyword = node.WhenClause.WhenKeyword;

        if (LineBreakTriviaUtilities.HasLeadingEndOfLine(whenKeyword) == false)
        {
            return node;
        }

        var patternLastToken = node.Pattern.GetLastToken();

        if (LineBreakTriviaUtilities.WouldJoinIntoComment(patternLastToken, whenKeyword))
        {
            return node;
        }

        var newPatternLastToken = patternLastToken.WithTrailingTrivia(SyntaxFactory.TriviaList(SyntaxFactory.Space));
        var newWhenKeyword = LineBreakTriviaUtilities.RemoveLeadingEndOfLineAndWhitespace(whenKeyword);

        return node.ReplaceTokens([patternLastToken, whenKeyword],
                                  (original, _) => original == patternLastToken
                                                       ? newPatternLastToken
                                                       : newWhenKeyword);
    }

    /// <summary>
    /// Wraps the <c>when</c> keyword onto its own line when it currently shares the line with the case
    /// pattern. The wrap is skipped when a comment would be joined into by moving the previous token
    /// </summary>
    /// <param name="node">The case pattern switch label</param>
    /// <returns>The updated case pattern switch label</returns>
    private CasePatternSwitchLabelSyntax WrapWhenClauseOntoOwnLine(CasePatternSwitchLabelSyntax node)
    {
        var whenKeyword = node.WhenClause.WhenKeyword;

        if (LineBreakTriviaUtilities.HasLeadingEndOfLine(whenKeyword))
        {
            return node;
        }

        return LineBreakTriviaUtilities.MoveTokenToNewLine(node, whenKeyword, _context.EndOfLine);
    }

    #endregion // Methods

    #region CSharpSyntaxVisitor

    /// <inheritdoc/>
    public override SyntaxNode VisitCasePatternSwitchLabel(CasePatternSwitchLabelSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (CasePatternSwitchLabelSyntax)base.VisitCasePatternSwitchLabel(node);

        if (node == null)
        {
            return null;
        }

        return NormalizeWhenClausePlacement(node);
    }

    #endregion // CSharpSyntaxVisitor
}