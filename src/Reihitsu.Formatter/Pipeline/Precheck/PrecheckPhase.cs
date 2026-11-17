using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Core;
using Reihitsu.Formatter.Data;

namespace Reihitsu.Formatter.Pipeline.Precheck;

/// <summary>
/// The first phase: refuses an input that holds a shape <c>sdd/specs/formatter/unsupported-syntax.md</c> declares unsupported,
/// decided on exactly the node the phases receive and from that node alone, and changes nothing (FMT-UNSUP-001 to
/// FMT-UNSUP-003). <see cref="FindUnsupportedSyntax"/> is the one statement of the unsupported shapes; the phase and
/// <see cref="ReihitsuFormatter.FindUnsupportedSyntax"/> both ask it (FMT-UNSUP-007)
/// </summary>
internal sealed class PrecheckPhase : IFormattingPhase
{
    #region Constants

    /// <summary>
    /// The reason given for a comment inside a variable declaration (FMT-UNSUP-004)
    /// </summary>
    internal const string VariableDeclarationCommentReason = "a comment between a variable declaration's own tokens";

    /// <summary>
    /// The reason given for a comment, a directive, or disabled text between a conditional operator and its branch (FMT-UNSUP-008)
    /// </summary>
    internal const string ConditionalOperatorCommentReason = "a comment or directive between a conditional operator and its branch";

    /// <summary>
    /// The reason given for a comment, a directive, or disabled text between a list element and the comma behind it, with the
    /// comma on a later line (FMT-UNSUP-009)
    /// </summary>
    internal const string ListSeparatorCommentReason = "a comment or directive between a list element and the comma behind it";

    /// <summary>
    /// The reason given for a directive or disabled text inside a field declaration with several declarators (FMT-UNSUP-010)
    /// </summary>
    internal const string FieldDeclarationDirectiveReason = "a directive inside a field declaration with several declarators";

    /// <summary>
    /// The reason given for a comment, a directive, or disabled text between a binary operator and its right operand (FMT-UNSUP-011)
    /// </summary>
    internal const string BinaryOperatorCommentReason = "a comment or directive between a binary operator and its right operand";

    /// <summary>
    /// The reason given for a comment, a directive, or disabled text between the arrow of an expression body and its expression
    /// (FMT-UNSUP-012)
    /// </summary>
    internal const string ExpressionBodyArrowCommentReason = "a comment or directive between the arrow of an expression body and its expression";

    #endregion // Constants

    #region Methods

    /// <summary>
    /// Finds the unsupported shape that starts first in the text of a node, deciding from the tokens and trivia inside the node
    /// alone (FMT-UNSUP-001, FMT-UNSUP-003)
    /// </summary>
    /// <param name="node">The node</param>
    /// <returns>The shape with its location in the node's syntax tree, or <see langword="null"/> when the node holds none</returns>
    internal static UnsupportedSyntax FindUnsupportedSyntax(SyntaxNode node)
    {
        SyntaxTrivia? firstComment = null;
        SyntaxTrivia? firstOperatorTrivia = null;
        SyntaxTrivia? firstFieldDirective = null;
        SyntaxTrivia? firstBinaryOperatorTrivia = null;
        SyntaxTrivia? firstArrowTrivia = null;

        foreach (var descendant in node.DescendantNodesAndSelf())
        {
            switch (descendant)
            {
                case VariableDeclarationSyntax declaration:
                    {
                        firstComment = SelectFirst(firstComment, FindCommentInDeclaration(declaration, declaration == node));
                    }
                    break;

                case ConditionalExpressionSyntax conditional:
                    {
                        firstOperatorTrivia = SelectFirst(firstOperatorTrivia, FindTriviaBehindConditionalOperator(conditional));
                    }
                    break;

                case FieldDeclarationSyntax fieldDeclaration:
                    {
                        firstFieldDirective = SelectFirst(firstFieldDirective, FindDirectiveInFieldDeclaration(fieldDeclaration));
                    }
                    break;

                case BinaryExpressionSyntax binary:
                    {
                        firstBinaryOperatorTrivia = SelectFirst(firstBinaryOperatorTrivia, FindTriviaBehindBinaryOperator(binary));
                    }
                    break;

                case ArrowExpressionClauseSyntax arrowExpressionClause:
                    {
                        firstArrowTrivia = SelectFirst(firstArrowTrivia, FindTriviaBehindExpressionBodyArrow(arrowExpressionClause));
                    }
                    break;
            }
        }

        var (firstSeparatorTrivia, isDeclaratorComma) = FindTriviaInFrontOfComma(node);

        // Of two shapes that start at the same trivia, the one listed first wins: FMT-UNSUP-004 keeps its reason for a comment,
        // and FMT-UNSUP-009 for a directive, in front of a comma between two declarators; FMT-UNSUP-009 gives its own for a
        // comment in front of every other comma inside a variable declaration, and FMT-UNSUP-010 gives its own for every other
        // directive inside a field declaration with several declarators. FMT-UNSUP-011 and FMT-UNSUP-012 are listed last, so
        // FMT-UNSUP-004 and FMT-UNSUP-010 keep their reason for a comment or directive behind a binary operator, and FMT-UNSUP-010
        // keeps its reason for a directive behind the arrow of an expression body, that they refuse as well
        var candidates = isDeclaratorComma
                             ? new List<(SyntaxTrivia? Trivia, string Reason)>
                               {
                                   (firstComment, VariableDeclarationCommentReason),
                                   (firstSeparatorTrivia, ListSeparatorCommentReason),
                                   (firstFieldDirective, FieldDeclarationDirectiveReason),
                                   (firstOperatorTrivia, ConditionalOperatorCommentReason),
                                   (firstBinaryOperatorTrivia, BinaryOperatorCommentReason),
                                   (firstArrowTrivia, ExpressionBodyArrowCommentReason)
                               }
                             : new List<(SyntaxTrivia? Trivia, string Reason)>
                               {
                                   (firstFieldDirective, FieldDeclarationDirectiveReason),
                                   (firstSeparatorTrivia, ListSeparatorCommentReason),
                                   (firstComment, VariableDeclarationCommentReason),
                                   (firstOperatorTrivia, ConditionalOperatorCommentReason),
                                   (firstBinaryOperatorTrivia, BinaryOperatorCommentReason),
                                   (firstArrowTrivia, ExpressionBodyArrowCommentReason)
                               };

        (SyntaxTrivia? Trivia, string Reason) first = (null, null);

        foreach (var candidate in candidates)
        {
            if (candidate.Trivia != null
                && (first.Trivia == null || candidate.Trivia.Value.FullSpan.Start < first.Trivia.Value.FullSpan.Start))
            {
                first = candidate;
            }
        }

        return first.Trivia == null
                   ? null
                   : new UnsupportedSyntax(first.Reason, Location.Create(node.SyntaxTree, first.Trivia.Value.FullSpan));
    }

    /// <summary>
    /// Finds the first comment, directive, or disabled text between a comma token of a node and the token in front of it, where
    /// the comma stands on a later line of the text than the line that token ends on (FMT-UNSUP-009). The gap starts behind the
    /// last token with text in front of the comma, so an omitted element without text belongs to it, and a line break counts
    /// wherever it stands in the gap's text, also inside a multi-line comment
    /// </summary>
    /// <param name="node">The node</param>
    /// <returns>The first such trivia, or <see langword="null"/>, and whether its comma separates two declarators of a variable declaration</returns>
    private static (SyntaxTrivia? Trivia, bool IsDeclaratorComma) FindTriviaInFrontOfComma(SyntaxNode node)
    {
        foreach (var comma in node.DescendantTokens())
        {
            if (comma.IsKind(SyntaxKind.CommaToken) == false)
            {
                continue;
            }

            var previousToken = comma.GetPreviousToken();

            if (previousToken == default
                || previousToken.SpanStart < node.FullSpan.Start)
            {
                continue;
            }

            SyntaxTrivia? firstTrivia = null;
            var hasLineBreak = false;

            foreach (var trivia in EnumerateGapTrivia(previousToken, comma))
            {
                var text = trivia.ToFullString();

                hasLineBreak |= text.Contains('\n') || text.Contains('\r');

                if (firstTrivia == null
                    && (SyntaxTriviaUtilities.IsCommentTrivia(trivia) || SyntaxTriviaUtilities.IsDirectiveOrDisabledTextTrivia(trivia)))
                {
                    firstTrivia = trivia;
                }
            }

            if (firstTrivia != null
                && hasLineBreak)
            {
                return (firstTrivia, comma.Parent is VariableDeclarationSyntax);
            }
        }

        return (null, false);
    }

    /// <summary>
    /// Gets the trivia between two tokens in text order: the trailing trivia of the first, the trivia of every token without
    /// text between them, and the leading trivia of the second
    /// </summary>
    /// <param name="previousToken">The token in front of the gap</param>
    /// <param name="nextToken">The token behind the gap</param>
    /// <returns>The trivia of the gap</returns>
    private static IEnumerable<SyntaxTrivia> EnumerateGapTrivia(SyntaxToken previousToken, SyntaxToken nextToken)
    {
        foreach (var trivia in previousToken.TrailingTrivia)
        {
            yield return trivia;
        }

        for (var token = previousToken.GetNextToken(includeZeroWidth: true); token != nextToken && token != default; token = token.GetNextToken(includeZeroWidth: true))
        {
            foreach (var trivia in token.LeadingTrivia)
            {
                yield return trivia;
            }

            foreach (var trivia in token.TrailingTrivia)
            {
                yield return trivia;
            }
        }

        foreach (var trivia in nextToken.LeadingTrivia)
        {
            yield return trivia;
        }
    }

    /// <summary>
    /// Gets the one of two trivia that starts first in the text
    /// </summary>
    /// <param name="current">The trivia found so far, or <see langword="null"/></param>
    /// <param name="candidate">The trivia found next, or <see langword="null"/></param>
    /// <returns>The trivia that starts first, or <see langword="null"/> when there is none</returns>
    private static SyntaxTrivia? SelectFirst(SyntaxTrivia? current, SyntaxTrivia? candidate)
    {
        return candidate != null
               && (current == null || candidate.Value.FullSpan.Start < current.Value.FullSpan.Start)
                   ? candidate
                   : current;
    }

    /// <summary>
    /// Finds the first comment, directive, or disabled text between an operator of a conditional expression and the branch behind
    /// it: in the trailing trivia of the <c>?</c> or the <c>:</c>, or in the leading trivia of the branch's first token
    /// (FMT-UNSUP-008)
    /// </summary>
    /// <param name="conditional">The conditional expression</param>
    /// <returns>The first such trivia, or <see langword="null"/></returns>
    private static SyntaxTrivia? FindTriviaBehindConditionalOperator(ConditionalExpressionSyntax conditional)
    {
        return FindFirstUnsupportedOperatorTrivia(conditional.QuestionToken.TrailingTrivia)
                   ?? FindFirstUnsupportedOperatorTrivia(conditional.WhenTrue.GetFirstToken().LeadingTrivia)
                   ?? FindFirstUnsupportedOperatorTrivia(conditional.ColonToken.TrailingTrivia)
                   ?? FindFirstUnsupportedOperatorTrivia(conditional.WhenFalse.GetFirstToken().LeadingTrivia);
    }

    /// <summary>
    /// Finds the first comment, directive, or disabled text between the operator of a binary expression and its right operand: in
    /// the trailing trivia of the operator, or in the leading trivia of the right operand's first token (FMT-UNSUP-011). The
    /// syntax tree decides what a binary expression is, so an <c>is</c> the parser reads as a type test counts, and a pattern
    /// match, an assignment, or a range does not
    /// </summary>
    /// <param name="binary">The binary expression</param>
    /// <returns>The first such trivia, or <see langword="null"/></returns>
    private static SyntaxTrivia? FindTriviaBehindBinaryOperator(BinaryExpressionSyntax binary)
    {
        return FindFirstUnsupportedOperatorTrivia(binary.OperatorToken.TrailingTrivia)
                   ?? FindFirstUnsupportedOperatorTrivia(binary.Right.GetFirstToken().LeadingTrivia);
    }

    /// <summary>
    /// Finds the first comment, directive, or disabled text between the arrow of an expression body and its expression: in the
    /// trailing trivia of the <c>=&gt;</c>, or in the leading trivia of the expression's first token (FMT-UNSUP-012). An arrow
    /// clause is the expression body of a member, an accessor, or a local function; the arrow of a lambda or of a
    /// switch-expression arm is no arrow clause
    /// </summary>
    /// <param name="arrowExpressionClause">The arrow clause</param>
    /// <returns>The first such trivia, or <see langword="null"/></returns>
    private static SyntaxTrivia? FindTriviaBehindExpressionBodyArrow(ArrowExpressionClauseSyntax arrowExpressionClause)
    {
        return FindFirstUnsupportedOperatorTrivia(arrowExpressionClause.ArrowToken.TrailingTrivia)
                   ?? FindFirstUnsupportedOperatorTrivia(arrowExpressionClause.Expression.GetFirstToken().LeadingTrivia);
    }

    /// <summary>
    /// Gets the first comment, directive, or disabled text of a trivia list
    /// </summary>
    /// <param name="trivia">The trivia list</param>
    /// <returns>The first such trivia, or <see langword="null"/> when the list holds none</returns>
    private static SyntaxTrivia? FindFirstUnsupportedOperatorTrivia(SyntaxTriviaList trivia)
    {
        var found = trivia.FirstOrDefault(static item => SyntaxTriviaUtilities.IsCommentTrivia(item)
                                                         || SyntaxTriviaUtilities.IsDirectiveOrDisabledTextTrivia(item));

        return found == default ? null : found;
    }

    /// <summary>
    /// Finds the first comment in the span of a variable declaration outside the places FMT-UNSUP-004 excepts: the leading
    /// trivia of the span's first token, the trailing trivia of the <c>;</c> or <c>)</c> that ends it, and the inside of an
    /// initializer value. The first and last token of each initializer value are taken once per declaration, because finding them
    /// walks the value's outermost spine. A declaration inside a value is a declaration of its own and is not looked at here
    /// </summary>
    /// <param name="declaration">The variable declaration</param>
    /// <param name="isInputRoot">Whether the declaration is the input itself, so that the statement or member around it is not part of the input</param>
    /// <returns>The first such comment, or <see langword="null"/></returns>
    private static SyntaxTrivia? FindCommentInDeclaration(VariableDeclarationSyntax declaration, bool isInputRoot)
    {
        var (tokens, terminator) = GetSpanTokens(declaration, isInputRoot);

        var values = declaration.Variables
                                .Select(static variable => variable.Initializer?.Value)
                                .Where(static value => value != null)
                                .Select(static value => (value.Span, FirstToken: value.GetFirstToken(), LastToken: value.GetLastToken()))
                                .ToList();
        var isFirst = true;

        foreach (var token in tokens)
        {
            var valueIndex = values.FindIndex(candidate => candidate.Span.Contains(token.Span));
            var checksLeadingTrivia = isFirst == false
                                      && (valueIndex < 0 || token == values[valueIndex].FirstToken);
            var checksTrailingTrivia = token != terminator
                                       && (valueIndex < 0 || token == values[valueIndex].LastToken);

            isFirst = false;

            var comment = (checksLeadingTrivia ? FindFirstComment(token.LeadingTrivia) : null)
                              ?? (checksTrailingTrivia ? FindFirstComment(token.TrailingTrivia) : null);

            if (comment != null)
            {
                return comment;
            }
        }

        return null;
    }

    /// <summary>
    /// Finds the first directive or disabled text in a field declaration with more than one declarator, from its first token
    /// through its <c>;</c>, initializer values included, outside the leading trivia of its first token (FMT-UNSUP-010). The
    /// declarators are those of the build being parsed: a declarator in disabled text does not count
    /// </summary>
    /// <param name="fieldDeclaration">The field declaration</param>
    /// <returns>The first such trivia, or <see langword="null"/></returns>
    private static SyntaxTrivia? FindDirectiveInFieldDeclaration(FieldDeclarationSyntax fieldDeclaration)
    {
        if (fieldDeclaration.Declaration.Variables.Count < 2)
        {
            return null;
        }

        var isFirst = true;

        foreach (var token in fieldDeclaration.DescendantTokens())
        {
            var directive = (isFirst ? null : FindFirstDirectiveOrDisabledText(token.LeadingTrivia))
                                ?? FindFirstDirectiveOrDisabledText(token.TrailingTrivia);

            isFirst = false;

            if (directive != null)
            {
                return directive;
            }
        }

        return null;
    }

    /// <summary>
    /// Gets the first directive or disabled text of a trivia list
    /// </summary>
    /// <param name="trivia">The trivia list</param>
    /// <returns>The first directive or disabled text, or <see langword="null"/> when the list holds none</returns>
    private static SyntaxTrivia? FindFirstDirectiveOrDisabledText(SyntaxTriviaList trivia)
    {
        var found = trivia.FirstOrDefault(SyntaxTriviaUtilities.IsDirectiveOrDisabledTextTrivia);

        return found == default ? null : found;
    }

    /// <summary>
    /// Gets the tokens of a variable declaration's span, from its first token through its terminator (FMT-UNSUP-004)
    /// </summary>
    /// <param name="declaration">The variable declaration</param>
    /// <param name="isInputRoot">Whether the declaration is the input itself</param>
    /// <returns>The tokens in text order, and the terminator, or the default token when the span has none in the input</returns>
    private static (List<SyntaxToken> Tokens, SyntaxToken Terminator) GetSpanTokens(VariableDeclarationSyntax declaration, bool isInputRoot)
    {
        var tokens = new List<SyntaxToken>();
        SyntaxToken terminator = default;

        switch (isInputRoot ? null : declaration.Parent)
        {
            case BaseFieldDeclarationSyntax:
            case LocalDeclarationStatementSyntax:
                {
                    tokens.AddRange(declaration.Parent.DescendantTokens());
                    terminator = tokens[tokens.Count - 1];
                }
                break;

            case ForStatementSyntax forStatement:
                {
                    terminator = forStatement.FirstSemicolonToken;
                }
                break;

            case UsingStatementSyntax usingStatement:
                {
                    terminator = usingStatement.CloseParenToken;
                }
                break;

            case FixedStatementSyntax fixedStatement:
                {
                    terminator = fixedStatement.CloseParenToken;
                }
                break;
        }

        if (tokens.Count == 0)
        {
            tokens.AddRange(declaration.DescendantTokens());

            if (terminator != default)
            {
                tokens.Add(terminator);
            }
        }

        return (tokens, terminator);
    }

    /// <summary>
    /// Gets the first comment of a trivia list
    /// </summary>
    /// <param name="trivia">The trivia list</param>
    /// <returns>The first comment, or <see langword="null"/> when the list holds none</returns>
    private static SyntaxTrivia? FindFirstComment(SyntaxTriviaList trivia)
    {
        var comment = trivia.FirstOrDefault(SyntaxTriviaUtilities.IsCommentTrivia);

        return comment == default ? null : comment;
    }

    #endregion // Methods

    #region IFormattingPhase

    /// <inheritdoc/>
    /// <remarks>
    /// Returns its input unchanged, or throws <see cref="SyntaxNotSupportedException"/> for the first unsupported shape in it,
    /// which the pipeline turns into the unsupported outcome before any other phase runs (FMT-UNSUP-001)
    /// </remarks>
    public SyntaxNode Execute(SyntaxNode root, FormattingContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var unsupported = FindUnsupportedSyntax(root);

        if (unsupported != null)
        {
            throw new SyntaxNotSupportedException(unsupported);
        }

        return root;
    }

    #endregion // IFormattingPhase
}