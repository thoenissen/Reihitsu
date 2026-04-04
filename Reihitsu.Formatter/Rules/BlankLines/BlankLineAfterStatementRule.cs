using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Formatter.Rules.BlankLines;

/// <summary>
/// Ensures that break statements are followed by a blank line.
/// Covers rule RH0313.
/// </summary>
internal sealed class BlankLineAfterStatementRule : FormattingRuleBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="context">The formatting context.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public BlankLineAfterStatementRule(FormattingContext context, CancellationToken cancellationToken)
        : base(context, cancellationToken)
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Combines two trivia lists into a single enumerable sequence.
    /// </summary>
    /// <param name="trailing">The trailing trivia of the previous element.</param>
    /// <param name="leading">The leading trivia of the next element.</param>
    /// <returns>The combined trivia sequence.</returns>
    private static IEnumerable<SyntaxTrivia> CombineTrivia(SyntaxTriviaList trailing, SyntaxTriviaList leading)
    {
        foreach (var trivia in trailing)
        {
            yield return trivia;
        }

        foreach (var trivia in leading)
        {
            yield return trivia;
        }
    }

    /// <summary>
    /// Determines whether the combined trivia contains a blank line
    /// (two or more end-of-line trivia entries).
    /// </summary>
    /// <param name="trivia">The trivia sequence to examine.</param>
    /// <returns><c>true</c> if a blank line exists; otherwise, <c>false</c>.</returns>
    private static bool HasBlankLine(IEnumerable<SyntaxTrivia> trivia)
    {
        var endOfLineCount = 0;

        foreach (var triviaEntry in trivia)
        {
            if (triviaEntry.IsKind(SyntaxKind.EndOfLineTrivia))
            {
                endOfLineCount++;

                if (endOfLineCount >= 2)
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Inserts an end-of-line trivia at position 0 of the given node's leading trivia.
    /// </summary>
    /// <typeparam name="T">The type of syntax node.</typeparam>
    /// <param name="node">The node whose leading trivia should be modified.</param>
    /// <returns>The node with the inserted blank line.</returns>
    private T InsertBlankLineBeforeLeadingTrivia<T>(T node)
        where T : SyntaxNode
    {
        var leadingTrivia = node.GetLeadingTrivia();
        var newLeadingTrivia = leadingTrivia.Insert(0, SyntaxFactory.EndOfLine(Context.EndOfLine));

        return node.WithLeadingTrivia(newLeadingTrivia);
    }

    #endregion // Methods

    #region FormattingRuleBase

    /// <inheritdoc/>
    public override SyntaxNode VisitSwitchStatement(SwitchStatementSyntax node)
    {
        var visited = (SwitchStatementSyntax)base.VisitSwitchStatement(node);

        if (visited == null)
        {
            return null;
        }

        var sections = visited.Sections;
        var modified = false;
        var newSections = new List<SwitchSectionSyntax>(sections.Count);

        for (var index = 0; index < sections.Count; index++)
        {
            var section = sections[index];

            if (index > 0)
            {
                var previousSection = sections[index - 1];
                var previousStatements = previousSection.Statements;

                if (previousStatements.Count > 0 && previousStatements.Last() is BreakStatementSyntax)
                {
                    var firstLabel = section.Labels.First();
                    var combinedTrivia = CombineTrivia(previousSection.GetTrailingTrivia(), firstLabel.GetLeadingTrivia());

                    if (HasBlankLine(combinedTrivia) == false)
                    {
                        section = section.WithLabels(section.Labels.Replace(firstLabel, InsertBlankLineBeforeLeadingTrivia(firstLabel)));

                        modified = true;
                    }
                }
            }

            newSections.Add(section);
        }

        if (modified)
        {
            return visited.WithSections(SyntaxFactory.List(newSections));
        }

        return visited;
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitBlock(BlockSyntax node)
    {
        var visited = (BlockSyntax)base.VisitBlock(node);

        if (visited == null)
        {
            return null;
        }

        var statements = visited.Statements;
        var modified = false;
        var newStatements = new List<StatementSyntax>(statements.Count);

        for (var i = 0; i < statements.Count; i++)
        {
            var statement = statements[i];

            if (i > 0 && statements[i - 1] is BreakStatementSyntax breakStatement)
            {
                var combinedTrivia = CombineTrivia(breakStatement.GetTrailingTrivia(), statement.GetLeadingTrivia());

                if (HasBlankLine(combinedTrivia) == false)
                {
                    statement = InsertBlankLineBeforeLeadingTrivia(statement);

                    modified = true;
                }
            }

            newStatements.Add(statement);
        }

        if (modified)
        {
            return visited.WithStatements(SyntaxFactory.List(newStatements));
        }

        return visited;
    }

    #endregion // FormattingRuleBase

    #region IFormattingRule

    /// <inheritdoc/>
    public override FormattingPhase Phase => FormattingPhase.BlankLineManagement;

    #endregion // IFormattingRule
}