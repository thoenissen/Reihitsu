using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Core;
using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Pipeline.LineBreaks.Utilities;

namespace Reihitsu.Formatter.Pipeline.LineBreaks.Rewriter;

/// <summary>
/// Applies line-break rules for generic <c>where</c> constraint clauses: every clause of a declaration that FMT-BREAK-023
/// covers starts its own line, and the constraints of every clause, whichever declaration owns it, are on one line or
/// one constraint per line (FMT-BREAK-037)
/// </summary>
/// <remarks>
/// Every decision is taken on line-start facts (SHARED-BLANK-008): a line break is an end-of-line trivia or the line
/// break that ends a single-line comment, a <c>///</c> comment, or a directive
/// (<see cref="SyntaxTriviaUtilities.AreSeparatedByEndOfLine"/>), and a line break inside the text of a multi-line comment
/// separates no tokens. The clause is laid out in two steps. First the wraps that structure nothing are joined: the gap
/// between the type parameter and the colon, and every gap inside one constraint except the gaps inside a type argument
/// list, which the angle-bracket collapse (FMT-BREAK-014) and the list element interior join (FMT-BREAK-015) own and
/// which have already run. Then the clause is judged as these joins leave it: when a line break separates its colon from
/// its type parameter or from its first constraint, or separates two tokens of its constraints, the first constraint
/// joins the colon's line, every comma at the start of a line joins the line of the constraint in front of it, and every
/// later constraint is separated from the token in front of it by a line break. No join crosses a comment, a directive,
/// or disabled text (FMT-BREAK-001); the line break in such a gap stays, and the indentation phase aligns the line it
/// starts (FMT-INDENT-032)
/// </remarks>
internal sealed class GenericConstraintLineBreakRewriter : CSharpSyntaxRewriter
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
    public GenericConstraintLineBreakRewriter(FormattingContext context,
                                              CancellationToken cancellationToken)
    {
        _context = context;
        _cancellationToken = cancellationToken;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Adapts the constraint clauses of a declaration that owns them, exposing both the current
    /// clauses and a factory that writes updated clauses back onto the declaration
    /// </summary>
    /// <param name="node">The syntax node to inspect</param>
    /// <returns>The clauses and a write-back factory, or <see langword="null"/> when the node does not own constraint clauses</returns>
    private static (SyntaxList<TypeParameterConstraintClauseSyntax> Clauses, Func<SyntaxList<TypeParameterConstraintClauseSyntax>, SyntaxNode> WithClauses)? GetConstraintClauseAdapter(SyntaxNode node)
    {
        return node switch
               {
                   ClassDeclarationSyntax classDeclaration => (classDeclaration.ConstraintClauses, clauses => classDeclaration.WithConstraintClauses(clauses)),
                   StructDeclarationSyntax structDeclaration => (structDeclaration.ConstraintClauses, clauses => structDeclaration.WithConstraintClauses(clauses)),
                   InterfaceDeclarationSyntax interfaceDeclaration => (interfaceDeclaration.ConstraintClauses, clauses => interfaceDeclaration.WithConstraintClauses(clauses)),
                   RecordDeclarationSyntax recordDeclaration => (recordDeclaration.ConstraintClauses, clauses => recordDeclaration.WithConstraintClauses(clauses)),
                   MethodDeclarationSyntax methodDeclaration => (methodDeclaration.ConstraintClauses, clauses => methodDeclaration.WithConstraintClauses(clauses)),
                   DelegateDeclarationSyntax delegateDeclaration => (delegateDeclaration.ConstraintClauses, clauses => delegateDeclaration.WithConstraintClauses(clauses)),
                   LocalFunctionStatementSyntax localFunction => (localFunction.ConstraintClauses, clauses => localFunction.WithConstraintClauses(clauses)),
                   _ => null,
               };
    }

    /// <summary>
    /// Determines whether a gap inside a constraint lies inside a type argument list of that constraint, from its opening
    /// to its closing angle bracket. Such a gap belongs to the angle-bracket collapse (FMT-BREAK-014) or to the join of
    /// wraps inside a list element (FMT-BREAK-015), and is left to them
    /// </summary>
    /// <param name="constraint">The constraint containing both tokens</param>
    /// <param name="previousToken">The token in front of the gap</param>
    /// <param name="token">The token behind the gap</param>
    /// <returns><see langword="true"/> if a type argument list inside the constraint contains both tokens; otherwise, <see langword="false"/></returns>
    private static bool IsInsideTypeArgumentList(TypeParameterConstraintSyntax constraint, SyntaxToken previousToken, SyntaxToken token)
    {
        for (var node = token.Parent; node != null && node != constraint; node = node.Parent)
        {
            if (node is TypeArgumentListSyntax typeArgumentList
                && typeArgumentList.Span.Contains(previousToken.Span))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Joins the wraps of a clause that structure nothing (FMT-BREAK-037): the colon onto the line of the type parameter,
    /// and every line break inside one constraint outside its type argument lists. A gap that no line break separates is
    /// left as it is, also where a multi-line comment puts its tokens on different lines of the text, and a gap that
    /// holds a comment, a directive, or disabled text is not joined. The joins go through the shared batch join
    /// (<see cref="LineBreakTriviaUtilities.JoinGaps"/>)
    /// </summary>
    /// <param name="clause">The constraint clause</param>
    /// <returns>The clause with the joins applied</returns>
    private static TypeParameterConstraintClauseSyntax JoinUnstructuredWraps(TypeParameterConstraintClauseSyntax clause)
    {
        var gaps = new List<(SyntaxToken PreviousToken, SyntaxToken Token)>
                   {
                       (clause.Name.GetLastToken(), clause.ColonToken)
                   };

        foreach (var constraint in clause.Constraints)
        {
            var firstToken = constraint.GetFirstToken();

            foreach (var token in constraint.DescendantTokens())
            {
                if (token == firstToken || token.Span.IsEmpty)
                {
                    continue;
                }

                var previousToken = token.GetPreviousToken();

                if (IsInsideTypeArgumentList(constraint, previousToken, token) == false)
                {
                    gaps.Add((previousToken, token));
                }
            }
        }

        return LineBreakTriviaUtilities.JoinGaps(clause,
                                                 gaps.Where(gap => SyntaxTriviaUtilities.AreSeparatedByEndOfLine(gap.PreviousToken, gap.Token)
                                                                   && LineBreakTriviaUtilities.WouldJoinAcrossUnjoinableTrivia(gap.PreviousToken, gap.Token) == false));
    }

    /// <summary>
    /// Determines whether a clause is wrapped (FMT-BREAK-037): a line break separates its colon from its type parameter
    /// or from its first constraint, or separates two tokens of its constraints, from the first token of the first
    /// constraint to the last token of the last one. A line break inside the text of a multi-line comment separates no
    /// tokens (<see cref="SyntaxTriviaUtilities.AreSeparatedByEndOfLine"/>). After the joins of
    /// <see cref="JoinUnstructuredWraps"/>, a line break still separates the colon from the type parameter only where a
    /// comment, a directive, or disabled text keeps it
    /// </summary>
    /// <param name="clause">The constraint clause, after the joins of <see cref="JoinUnstructuredWraps"/></param>
    /// <returns><see langword="true"/> if the clause is wrapped; otherwise, <see langword="false"/></returns>
    private static bool IsWrapped(TypeParameterConstraintClauseSyntax clause)
    {
        var firstToken = clause.Constraints[0].GetFirstToken();

        if (SyntaxTriviaUtilities.AreSeparatedByEndOfLine(clause.Name.GetLastToken(), clause.ColonToken)
            || SyntaxTriviaUtilities.AreSeparatedByEndOfLine(clause.ColonToken, firstToken))
        {
            return true;
        }

        var previousToken = firstToken;

        // Every token of the clause behind the first constraint's first token belongs to a constraint or is a comma
        // between two of them
        foreach (var token in clause.DescendantTokens().Where(token => token.SpanStart > firstToken.SpanStart))
        {
            if (SyntaxTriviaUtilities.AreSeparatedByEndOfLine(previousToken, token))
            {
                return true;
            }

            previousToken = token;
        }

        return false;
    }

    /// <summary>
    /// Joins the token behind a gap that a line break separates (<see cref="SyntaxTriviaUtilities.AreSeparatedByEndOfLine"/>)
    /// onto the line of the token in front of it through the shared collapse (<see cref="LineBreakTriviaUtilities.CollapseTokenToSameLine"/>), which refuses a gap that holds a comment,
    /// a directive, or disabled text
    /// </summary>
    /// <param name="clause">The constraint clause containing both tokens</param>
    /// <param name="previousToken">The token in front of the gap</param>
    /// <param name="token">The token behind the gap</param>
    /// <returns>The clause with the gap joined, or the clause unchanged</returns>
    private static TypeParameterConstraintClauseSyntax JoinGap(TypeParameterConstraintClauseSyntax clause, SyntaxToken previousToken, SyntaxToken token)
    {
        return SyntaxTriviaUtilities.AreSeparatedByEndOfLine(previousToken, token)
                   ? LineBreakTriviaUtilities.CollapseTokenToSameLine(clause, token)
                   : clause;
    }

    /// <summary>
    /// Lays out a wrapped clause one constraint per line (FMT-BREAK-037): the first constraint joins the colon's line,
    /// every comma joins the line of the constraint in front of it, and every later constraint that no line break
    /// separates from the comma in front of it gets a line break appended to the comma's trailing trivia, behind any
    /// comment there, so a multi-line comment behind a comma stays behind it. A line break inside the text of a multi-line
    /// comment separates no tokens (<see cref="SyntaxTriviaUtilities.AreSeparatedByEndOfLine"/>). A comma that a comment, a
    /// directive, or disabled text keeps at the start of its line stays there, and the constraint behind it shares its
    /// line with no other constraint
    /// </summary>
    /// <param name="clause">The wrapped constraint clause</param>
    /// <param name="endOfLine">The end-of-line sequence to insert</param>
    /// <returns>The clause laid out one constraint per line</returns>
    private static TypeParameterConstraintClauseSyntax PlaceOneConstraintPerLine(TypeParameterConstraintClauseSyntax clause, string endOfLine)
    {
        clause = JoinGap(clause, clause.ColonToken, clause.Constraints[0].GetFirstToken());

        for (var separatorIndex = 0; separatorIndex < clause.Constraints.SeparatorCount; separatorIndex++)
        {
            clause = JoinGap(clause, clause.Constraints[separatorIndex].GetLastToken(), clause.Constraints.GetSeparator(separatorIndex));
        }

        var replacements = new Dictionary<SyntaxToken, SyntaxToken>();

        for (var separatorIndex = 0; separatorIndex < clause.Constraints.SeparatorCount; separatorIndex++)
        {
            var separator = clause.Constraints.GetSeparator(separatorIndex);

            if (SyntaxTriviaUtilities.AreSeparatedByEndOfLine(separator, clause.Constraints[separatorIndex + 1].GetFirstToken())
                || SyntaxTriviaUtilities.AreSeparatedByEndOfLine(clause.Constraints[separatorIndex].GetLastToken(), separator))
            {
                continue;
            }

            replacements[separator] = separator.WithTrailingTrivia(LineBreakTriviaUtilities.AppendEndOfLine(LineBreakTriviaUtilities.RemoveTrailingWhitespace(separator.TrailingTrivia), endOfLine));
        }

        if (replacements.Count == 0)
        {
            return clause;
        }

        return clause.ReplaceTokens(replacements.Keys, (original, _) => replacements[original]);
    }

    /// <summary>
    /// Lays out the constraints of a clause on one line or one constraint per line (FMT-BREAK-037)
    /// </summary>
    /// <param name="clause">The constraint clause</param>
    /// <returns>The clause laid out</returns>
    private TypeParameterConstraintClauseSyntax LayOutConstraints(TypeParameterConstraintClauseSyntax clause)
    {
        if (clause.Constraints.Count == 0)
        {
            return clause;
        }

        clause = JoinUnstructuredWraps(clause);

        return IsWrapped(clause)
                   ? PlaceOneConstraintPerLine(clause, _context.EndOfLine)
                   : clause;
    }

    /// <summary>
    /// Ensures all <c>where</c> constraint clauses on a declaration start on new lines
    /// </summary>
    /// <param name="node">The declaration node</param>
    /// <returns>The node with <c>where</c> clauses on new lines</returns>
    private SyntaxNode EnsureGenericConstraintsOnNewLines(SyntaxNode node)
    {
        var adapter = GetConstraintClauseAdapter(node);

        if (adapter == null || adapter.Value.Clauses.Count == 0)
        {
            return node;
        }

        var modified = false;
        var newClauses = new List<TypeParameterConstraintClauseSyntax>();

        foreach (var clause in adapter.Value.Clauses)
        {
            var whereKeyword = clause.WhereKeyword;

            if (LineBreakTriviaUtilities.HasLeadingEndOfLine(whereKeyword) == false)
            {
                var newWhereKeyword = LineBreakTriviaUtilities.PrependEndOfLine(whereKeyword, _context.EndOfLine);

                newClauses.Add(clause.WithWhereKeyword(newWhereKeyword));
                modified = true;
            }
            else
            {
                newClauses.Add(clause);
            }
        }

        if (modified == false)
        {
            return node;
        }

        return adapter.Value.WithClauses(SyntaxFactory.List(newClauses));
    }

    #endregion // Methods

    #region CSharpSyntaxVisitor

    /// <inheritdoc/>
    public override SyntaxNode Visit(SyntaxNode node)
    {
        if (node == null)
        {
            return null;
        }

        _cancellationToken.ThrowIfCancellationRequested();

        var visited = base.Visit(node);

        if (visited == null)
        {
            return null;
        }

        if (visited is TypeParameterConstraintClauseSyntax clause)
        {
            return LayOutConstraints(clause);
        }

        return EnsureGenericConstraintsOnNewLines(visited);
    }

    #endregion // CSharpSyntaxVisitor
}