using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Formatter.Pipeline.SwitchCaseBraces;

/// <summary>
/// Rewrites switch statements to add or remove braces from case sections
/// </summary>
internal sealed class SwitchCaseBraceRewriter : CSharpSyntaxRewriter
{
    #region Fields

    /// <summary>
    /// The formatting context
    /// </summary>
    private readonly FormattingContext _context;

    /// <summary>
    /// Cancellation token
    /// </summary>
    private readonly CancellationToken _cancellationToken;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="context">The formatting context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public SwitchCaseBraceRewriter(FormattingContext context, CancellationToken cancellationToken)
    {
        _context = context;
        _cancellationToken = cancellationToken;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Determines whether a switch section is a fall-through section (no statements, or only labels)
    /// </summary>
    /// <param name="section">The switch section to check</param>
    /// <returns><see langword="true"/> if the section is a fall-through; otherwise, <see langword="false"/></returns>
    private static bool IsFallThroughSection(SwitchSectionSyntax section)
    {
        return section.Statements.Count == 0;
    }

    /// <summary>
    /// Determines whether a switch section is multi-line.
    /// A section is multi-line if its label contains a multi-line delimited pattern, it has more
    /// than one non-terminal statement, a single non-terminal statement that spans multiple lines,
    /// or any statement containing a multi-line switch expression
    /// </summary>
    /// <param name="section">The switch section to check</param>
    /// <returns><see langword="true"/> if the section is multi-line; otherwise, <see langword="false"/></returns>
    private static bool IsMultiLineSection(SwitchSectionSyntax section)
    {
        if (LabelContainsMultiLinePattern(section))
        {
            return true;
        }

        var statements = GetNonTerminalStatements(section);

        if (statements.Count > 1)
        {
            return true;
        }

        if (statements.Count == 1)
        {
            return SpansMultipleLines(statements[0]);
        }

        return ContainsMultiLineSwitchExpression(section);
    }

    /// <summary>
    /// Determines whether a switch section's label contains a multi-line delimited pattern
    /// (recursive, list, or parenthesized). Combinator chains and guard clauses that merely wrap
    /// across lines do not count
    /// </summary>
    /// <param name="section">The switch section to check</param>
    /// <returns><see langword="true"/> if a label contains a multi-line delimited pattern; otherwise, <see langword="false"/></returns>
    private static bool LabelContainsMultiLinePattern(SwitchSectionSyntax section)
    {
        return section.Labels
                      .SelectMany(label => label.DescendantNodes())
                      .Any(node => node is RecursivePatternSyntax or ListPatternSyntax or ParenthesizedPatternSyntax
                                   && SpansMultipleLines(node));
    }

    /// <summary>
    /// Determines whether any statement in a switch section contains a switch expression that
    /// spans multiple lines, including switch expressions nested inside terminal statements
    /// such as <c>return</c> or <c>throw</c>
    /// </summary>
    /// <param name="section">The switch section to check</param>
    /// <returns><see langword="true"/> if the section contains a multi-line switch expression; otherwise, <see langword="false"/></returns>
    private static bool ContainsMultiLineSwitchExpression(SwitchSectionSyntax section)
    {
        return section.Statements
                      .SelectMany(statement => statement.DescendantNodes().OfType<SwitchExpressionSyntax>())
                      .Any(SpansMultipleLines);
    }

    /// <summary>
    /// Checks whether a syntax node spans multiple lines
    /// </summary>
    /// <param name="node">The syntax node to check</param>
    /// <returns><see langword="true"/> if the node spans multiple lines; otherwise, <see langword="false"/></returns>
    private static bool SpansMultipleLines(SyntaxNode node)
    {
        var lineSpan = node.GetLocation().GetLineSpan();

        return lineSpan.StartLinePosition.Line != lineSpan.EndLinePosition.Line;
    }

    /// <summary>
    /// Gets the non-terminal statements from a switch section (excludes break, return, and throw)
    /// </summary>
    /// <param name="section">The switch section</param>
    /// <returns>A list of non-terminal statements</returns>
    private static List<StatementSyntax> GetNonTerminalStatements(SwitchSectionSyntax section)
    {
        return section.Statements
                      .Where(statement => IsTerminalStatement(statement) == false)
                      .ToList();
    }

    /// <summary>
    /// Determines whether a statement is a terminal statement (break, return, or throw)
    /// </summary>
    /// <param name="statement">The statement to check</param>
    /// <returns><see langword="true"/> if the statement is terminal; otherwise, <see langword="false"/></returns>
    private static bool IsTerminalStatement(StatementSyntax statement)
    {
        return statement is BreakStatementSyntax
               || statement is ReturnStatementSyntax
               || statement is ThrowStatementSyntax;
    }

    /// <summary>
    /// Removes braces from a switch section, extracting the block's statements.
    /// If the section does not have braces, it is returned as-is.
    /// When the braces carry comment trivia that cannot be re-attached without loss, the
    /// braces are kept so the comments are not silently deleted
    /// </summary>
    /// <param name="section">The switch section</param>
    /// <returns>The section with braces removed</returns>
    private static SwitchSectionSyntax RemoveBraces(SwitchSectionSyntax section)
    {
        var statements = section.Statements;

        if (statements.Count == 0 || (statements[0] is BlockSyntax) == false)
        {
            return section;
        }

        var block = (BlockSyntax)statements[0];

        // Removing the braces discards the brace tokens, so any comments or directives attached to them
        // would be lost. Keep the braces in that case rather than deleting them.
        if (BraceTokensCarryCommentsOrDirectives(block))
        {
            return section;
        }

        var extractedStatements = new List<StatementSyntax>();

        foreach (var statement in block.Statements)
        {
            extractedStatements.Add(statement);
        }

        // Add remaining statements after the block (e.g., break after braces)
        for (var statementIndex = 1; statementIndex < statements.Count; statementIndex++)
        {
            extractedStatements.Add(statements[statementIndex]);
        }

        return section.WithStatements(SyntaxFactory.List(extractedStatements));
    }

    /// <summary>
    /// Builds leading trivia that preserves any comments from the original leading trivia while
    /// resetting the layout to a single leading end-of-line per comment
    /// </summary>
    /// <param name="original">The original leading trivia</param>
    /// <param name="eolTrivia">The end-of-line trivia to use</param>
    /// <returns>The rebuilt leading trivia</returns>
    private static SyntaxTriviaList BuildLeadingTrivia(SyntaxTriviaList original, SyntaxTrivia eolTrivia)
    {
        var result = new List<SyntaxTrivia>
                     {
                         eolTrivia
                     };

        foreach (var comment in CollectComments(original))
        {
            result.Add(comment);
            result.Add(eolTrivia);
        }

        return SyntaxFactory.TriviaList(result);
    }

    /// <summary>
    /// Builds trailing trivia that preserves a trailing comment (for example "DoWork(); // note")
    /// while terminating the line with the given end-of-line trivia
    /// </summary>
    /// <param name="original">The original trailing trivia</param>
    /// <param name="eolTrivia">The end-of-line trivia to use</param>
    /// <returns>The rebuilt trailing trivia</returns>
    private static SyntaxTriviaList BuildTrailingTrivia(SyntaxTriviaList original, SyntaxTrivia eolTrivia)
    {
        var comments = CollectComments(original);

        if (comments.Count == 0)
        {
            return SyntaxFactory.TriviaList(eolTrivia);
        }

        var result = new List<SyntaxTrivia>
                     {
                         SyntaxFactory.Space
                     };

        foreach (var comment in comments)
        {
            result.Add(comment);
        }

        result.Add(eolTrivia);

        return SyntaxFactory.TriviaList(result);
    }

    /// <summary>
    /// Builds the trailing trivia for the last label, keeping any trailing comment but dropping
    /// trailing whitespace so the open brace is not preceded by a blank line
    /// </summary>
    /// <param name="original">The original label trailing trivia</param>
    /// <returns>The rebuilt label trailing trivia</returns>
    private static SyntaxTriviaList BuildLabelTrailingTrivia(SyntaxTriviaList original)
    {
        var comments = CollectComments(original);

        if (comments.Count == 0)
        {
            return SyntaxFactory.TriviaList();
        }

        var result = new List<SyntaxTrivia>
                     {
                         SyntaxFactory.Space
                     };

        foreach (var comment in comments)
        {
            result.Add(comment);
        }

        return SyntaxFactory.TriviaList(result);
    }

    /// <summary>
    /// Determines whether the open or close brace tokens of a block carry comment trivia,
    /// a preprocessor directive, or disabled text
    /// </summary>
    /// <param name="block">The block to inspect</param>
    /// <returns><see langword="true"/> if any brace token carries a comment or directive; otherwise, <see langword="false"/></returns>
    private static bool BraceTokensCarryCommentsOrDirectives(BlockSyntax block)
    {
        return BraceTriviaCarriesCommentOrDirective(block.OpenBraceToken.LeadingTrivia)
               || BraceTriviaCarriesCommentOrDirective(block.OpenBraceToken.TrailingTrivia)
               || BraceTriviaCarriesCommentOrDirective(block.CloseBraceToken.LeadingTrivia)
               || BraceTriviaCarriesCommentOrDirective(block.CloseBraceToken.TrailingTrivia);
    }

    /// <summary>
    /// Determines whether a trivia list carries comment trivia, a preprocessor directive, or disabled text
    /// </summary>
    /// <param name="triviaList">The trivia list to inspect</param>
    /// <returns><see langword="true"/> if the list carries a comment or directive; otherwise, <see langword="false"/></returns>
    private static bool BraceTriviaCarriesCommentOrDirective(SyntaxTriviaList triviaList)
    {
        return triviaList.Any(static trivia => ReihitsuFormatterHelpers.IsCommentTrivia(trivia)
                                               || ReihitsuFormatterHelpers.IsDirectiveOrDisabledTextTrivia(trivia));
    }

    /// <summary>
    /// Collects the comment trivia from a trivia list, preserving their order
    /// </summary>
    /// <param name="triviaList">The trivia list to inspect</param>
    /// <returns>The comment trivia contained in the list</returns>
    private static List<SyntaxTrivia> CollectComments(SyntaxTriviaList triviaList)
    {
        return triviaList.Where(ReihitsuFormatterHelpers.IsCommentTrivia)
                         .ToList();
    }

    /// <summary>
    /// Determines whether a switch section carries a preprocessor directive or disabled text in the
    /// section-level trivia of its labels or statements — the trivia that brace insertion rebuilds
    /// from comments only and would therefore drop
    /// </summary>
    /// <param name="section">The switch section to inspect</param>
    /// <returns><see langword="true"/> if the section carries a directive or disabled text; otherwise, <see langword="false"/></returns>
    private static bool SectionCarriesDirectives(SwitchSectionSyntax section)
    {
        foreach (var label in section.Labels)
        {
            if (label.GetLeadingTrivia().Any(ReihitsuFormatterHelpers.IsDirectiveOrDisabledTextTrivia)
                || label.GetTrailingTrivia().Any(ReihitsuFormatterHelpers.IsDirectiveOrDisabledTextTrivia))
            {
                return true;
            }
        }

        foreach (var statement in section.Statements)
        {
            if (statement.GetLeadingTrivia().Any(ReihitsuFormatterHelpers.IsDirectiveOrDisabledTextTrivia)
                || statement.GetTrailingTrivia().Any(ReihitsuFormatterHelpers.IsDirectiveOrDisabledTextTrivia))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Adds braces around the non-terminal statements of a switch section.
    /// If braces already exist, the section is returned as-is
    /// </summary>
    /// <param name="section">The switch section</param>
    /// <returns>The section with braces added</returns>
    private SwitchSectionSyntax AddBraces(SwitchSectionSyntax section)
    {
        var statements = section.Statements;

        // Already has a block as the first statement — nothing to do
        if (statements.Count > 0 && statements[0] is BlockSyntax)
        {
            return section;
        }

        // Adding braces rebuilds the label, first-statement, and trailing-break trivia from comments
        // only. A preprocessor directive or disabled text in that section-level trivia would be dropped,
        // silently removing conditional compilation, so leave a directive-bearing section unbraced.
        if (SectionCarriesDirectives(section))
        {
            return section;
        }

        var eolTrivia = SyntaxFactory.EndOfLine(_context.EndOfLine);

        // Strip trailing whitespace from the last label to prevent a blank line before the open brace,
        // but keep any trailing comment (for example "case 1: // note") so it is not deleted
        var labels = section.Labels;
        var lastLabel = labels.Last();
        section = section.WithLabels(labels.Replace(lastLabel, lastLabel.WithTrailingTrivia(BuildLabelTrailingTrivia(lastLabel.GetTrailingTrivia()))));

        // Separate trailing break from the statements that go into the block
        StatementSyntax trailingBreak = null;

        if (statements.Count > 0 && statements[statements.Count - 1] is BreakStatementSyntax breakStatement)
        {
            trailingBreak = breakStatement;
        }

        var statementsForBlock = trailingBreak != null
                                     ? statements.Count - 1
                                     : statements.Count;

        var blockStatements = new List<StatementSyntax>(statementsForBlock);

        for (var statementIndex = 0; statementIndex < statementsForBlock; statementIndex++)
        {
            var stmt = statements[statementIndex];

            if (statementIndex == 0)
            {
                stmt = stmt.WithLeadingTrivia(BuildLeadingTrivia(stmt.GetLeadingTrivia(), eolTrivia));
            }

            if (statementIndex == statementsForBlock - 1)
            {
                var lastToken = stmt.GetLastToken();
                stmt = stmt.ReplaceToken(lastToken, lastToken.WithTrailingTrivia(BuildTrailingTrivia(lastToken.TrailingTrivia, eolTrivia)));
            }

            blockStatements.Add(stmt);
        }

        var openBrace = SyntaxFactory.Token(SyntaxKind.OpenBraceToken)
                                     .WithLeadingTrivia(eolTrivia)
                                     .WithTrailingTrivia(SyntaxFactory.TriviaList());

        var closeBraceTrailing = trailingBreak != null
                                     ? SyntaxFactory.TriviaList()
                                     : SyntaxFactory.TriviaList(eolTrivia);

        var closeBrace = SyntaxFactory.Token(SyntaxKind.CloseBraceToken)
                                      .WithLeadingTrivia(SyntaxFactory.TriviaList())
                                      .WithTrailingTrivia(closeBraceTrailing);

        var block = SyntaxFactory.Block(openBrace,
                                        SyntaxFactory.List(blockStatements),
                                        closeBrace);

        var sectionStatements = new List<StatementSyntax>
                                {
                                    block
                                };

        if (trailingBreak != null)
        {
            trailingBreak = trailingBreak.WithLeadingTrivia(BuildLeadingTrivia(trailingBreak.GetLeadingTrivia(), eolTrivia));

            var lastBreakToken = trailingBreak.GetLastToken();
            trailingBreak = trailingBreak.ReplaceToken(lastBreakToken, lastBreakToken.WithTrailingTrivia(BuildTrailingTrivia(lastBreakToken.TrailingTrivia, eolTrivia)));

            sectionStatements.Add(trailingBreak);
        }

        return section.WithStatements(SyntaxFactory.List(sectionStatements));
    }

    #endregion // Methods

    #region CSharpSyntaxVisitor

    /// <inheritdoc/>
    public override SyntaxNode VisitSwitchStatement(SwitchStatementSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (SwitchStatementSyntax)base.VisitSwitchStatement(node);

        if (node == null)
        {
            return null;
        }

        var sections = node.Sections;

        if (sections.Count == 0)
        {
            return node;
        }

        var anyMultiLine = false;

        foreach (var section in sections)
        {
            if (IsFallThroughSection(section))
            {
                continue;
            }

            if (IsMultiLineSection(section))
            {
                anyMultiLine = true;

                break;
            }
        }

        var newSections = new List<SwitchSectionSyntax>(sections.Count);

        foreach (var section in sections)
        {
            if (IsFallThroughSection(section))
            {
                newSections.Add(section);

                continue;
            }

            if (anyMultiLine)
            {
                newSections.Add(AddBraces(section));
            }
            else
            {
                newSections.Add(RemoveBraces(section));
            }
        }

        return node.WithSections(SyntaxFactory.List(newSections));
    }

    #endregion // CSharpSyntaxVisitor
}