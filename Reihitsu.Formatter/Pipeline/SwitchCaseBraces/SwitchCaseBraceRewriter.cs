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

    #region CSharpSyntaxRewriter

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

    #endregion // CSharpSyntaxRewriter

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
    /// A section is multi-line if it has more than one non-terminal statement,
    /// or a single non-terminal statement that spans multiple lines
    /// </summary>
    /// <param name="section">The switch section to check</param>
    /// <returns><see langword="true"/> if the section is multi-line; otherwise, <see langword="false"/></returns>
    private static bool IsMultiLineSection(SwitchSectionSyntax section)
    {
        var statements = GetNonTerminalStatements(section);

        if (statements.Count > 1)
        {
            return true;
        }

        if (statements.Count == 1)
        {
            return SpansMultipleLines(statements[0]);
        }

        return false;
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
    /// If the section does not have braces, it is returned as-is
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

        var eolTrivia = SyntaxFactory.EndOfLine(_context.EndOfLine);

        // Strip trailing whitespace from last label to prevent blank line before open brace
        var labels = section.Labels;
        var lastLabel = labels.Last();
        section = section.WithLabels(labels.Replace(lastLabel, lastLabel.WithTrailingTrivia(SyntaxFactory.TriviaList())));

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
                stmt = stmt.WithLeadingTrivia(eolTrivia);
            }

            if (statementIndex == statementsForBlock - 1)
            {
                var lastToken = stmt.GetLastToken();
                stmt = stmt.ReplaceToken(lastToken, lastToken.WithTrailingTrivia(eolTrivia));
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
            trailingBreak = trailingBreak.WithLeadingTrivia(eolTrivia);

            var lastBreakToken = trailingBreak.GetLastToken();
            trailingBreak = trailingBreak.ReplaceToken(lastBreakToken, lastBreakToken.WithTrailingTrivia(eolTrivia));

            sectionStatements.Add(trailingBreak);
        }

        return section.WithStatements(SyntaxFactory.List(sectionStatements));
    }

    #endregion // Methods
}