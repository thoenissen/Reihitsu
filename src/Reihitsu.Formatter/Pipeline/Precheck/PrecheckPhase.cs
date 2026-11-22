using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Core;
using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Pipeline.Core.Enumerations;
using Reihitsu.Formatter.Pipeline.Core.Utilities;

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

    /// <summary>
    /// The reason given for a comment, a directive, or disabled text between the arrow of an expression body and the token in
    /// front of it (FMT-UNSUP-013)
    /// </summary>
    internal const string ExpressionBodyArrowLeadingCommentReason = "a comment or directive between the arrow of an expression body and the code in front of it";

    /// <summary>
    /// The reason given for a comment, a directive, or disabled text in front of an argument list, an element access, a
    /// conditional element access, or a null-forgiving operator, or between the <c>?</c> and the <c>[</c> of a conditional
    /// element access (FMT-UNSUP-014)
    /// </summary>
    internal const string AttachedPartCommentReason = "a comment or directive in front of an argument list, an element access, or a null-forgiving operator";

    /// <summary>
    /// The reason given for a comment, a directive, or disabled text between two tokens of a link operator <c>?.</c>,
    /// <c>!.</c>, or <c>!?.</c> (FMT-UNSUP-015)
    /// </summary>
    internal const string LinkOperatorCommentReason = "a comment or directive inside a ?. or !. operator";

    /// <summary>
    /// The reason given for a comment, a directive, or disabled text between the <c>.</c> of a simple member access or a member
    /// binding and its member name (FMT-UNSUP-016)
    /// </summary>
    internal const string LinkDotNameCommentReason = "a comment or directive between a . and its member name";

    /// <summary>
    /// The reason given for a comment, a directive, or disabled text between <c>return</c>, <c>throw</c>, <c>await</c>, or
    /// <c>new</c> and what follows it (FMT-UNSUP-017)
    /// </summary>
    internal const string KeywordOperandCommentReason = "a comment or directive between return, throw, await, or new and what follows it";

    /// <summary>
    /// The reason given for a comment, a directive, or disabled text between the <c>is</c> of a pattern match and its pattern
    /// (FMT-UNSUP-017)
    /// </summary>
    internal const string IsPatternCommentReason = "a comment or directive between is and its pattern";

    /// <summary>
    /// The reason given for a comment, a directive, or disabled text between <c>and</c>, <c>or</c>, or <c>not</c> and its
    /// pattern (FMT-UNSUP-017)
    /// </summary>
    internal const string PatternCombinatorCommentReason = "a comment or directive between and, or, or not and its pattern";

    /// <summary>
    /// The reason given for a comment, a directive, or disabled text in front of the <c>when</c> of a switch arm (FMT-UNSUP-017)
    /// </summary>
    internal const string SwitchArmWhenCommentReason = "a comment or directive in front of the when of a switch arm";

    /// <summary>
    /// The reason given for a comment, a directive, or disabled text in front of the <c>=&gt;</c> of a switch arm (FMT-UNSUP-017)
    /// </summary>
    internal const string SwitchArmArrowCommentReason = "a comment or directive in front of the => of a switch arm";

    /// <summary>
    /// The reason given for a comment, a directive, or disabled text in front of the <c>:</c> of a base list (FMT-UNSUP-017)
    /// </summary>
    internal const string BaseListCommentReason = "a comment or directive in front of the : of a base list";

    /// <summary>
    /// The reason given for a comment, a directive, or disabled text behind a modifier (FMT-UNSUP-017)
    /// </summary>
    internal const string ModifierCommentReason = "a comment or directive behind a modifier";

    /// <summary>
    /// The reason given for a comment, a directive, or disabled text in front of the <c>when</c> of a catch clause (FMT-UNSUP-017)
    /// </summary>
    internal const string CatchFilterCommentReason = "a comment or directive in front of the when of a catch clause";

    /// <summary>
    /// The reason given for a comment, a directive, or disabled text behind a <c>;</c> of a <c>for</c> header (FMT-UNSUP-017)
    /// </summary>
    internal const string ForHeaderCommentReason = "a comment or directive behind a ; of a for header";

    /// <summary>
    /// The reason given for a comment, a directive, or disabled text behind the <c>in</c> of a <c>foreach</c> statement
    /// (FMT-UNSUP-017)
    /// </summary>
    internal const string ForEachExpressionCommentReason = "a comment or directive behind the in of a foreach statement";

    /// <summary>
    /// The reason given for a comment, a directive, or disabled text between a cast and its operand (FMT-UNSUP-017)
    /// </summary>
    internal const string CastCommentReason = "a comment or directive between a cast and its operand";

    /// <summary>
    /// The reason given for a comment, a directive, or disabled text behind the <c>=&gt;</c> of a lambda (FMT-UNSUP-017)
    /// </summary>
    internal const string LambdaArrowCommentReason = "a comment or directive behind the => of a lambda";

    /// <summary>
    /// The reason given for a comment, a directive, or disabled text in front of <c>with</c> (FMT-UNSUP-017)
    /// </summary>
    internal const string WithCommentReason = "a comment or directive in front of with";

    /// <summary>
    /// The reason given for a comment, a directive, or disabled text in front of or behind the <c>=</c> of an anonymous-object
    /// member (FMT-UNSUP-017)
    /// </summary>
    internal const string AnonymousMemberEqualsCommentReason = "a comment or directive in front of or behind the = of an anonymous-object member";

    /// <summary>
    /// The reason given for a comment, a directive, or disabled text in front of or behind the <c>=</c> of a <c>let</c> clause
    /// (FMT-UNSUP-017)
    /// </summary>
    internal const string LetClauseEqualsCommentReason = "a comment or directive in front of or behind the = of a let clause";

    /// <summary>
    /// The reason given for a comment, a directive, or disabled text inside a list of omitted type arguments (FMT-UNSUP-018)
    /// </summary>
    internal const string OmittedTypeArgumentCommentReason = "a comment or directive inside a list of omitted type arguments";

    /// <summary>
    /// The reason given for a comment, a directive, or disabled text behind an assignment operator inside a switch arm or an
    /// anonymous-object member (FMT-UNSUP-019)
    /// </summary>
    internal const string AssignmentInArmCommentReason = "a comment or directive behind an assignment operator inside a switch arm or an anonymous-object member";

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
        SyntaxTrivia? firstArrowLeadingTrivia = null;
        SyntaxTrivia? firstLinkOperatorTrivia = null;
        SyntaxTrivia? firstLinkDotNameTrivia = null;

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
                        firstArrowLeadingTrivia = SelectFirst(firstArrowLeadingTrivia, FindTriviaInFrontOfExpressionBodyArrow(node, arrowExpressionClause));
                    }
                    break;

                case ExpressionSyntax link when link is MemberAccessExpressionSyntax or MemberBindingExpressionSyntax:
                    {
                        firstLinkOperatorTrivia = SelectFirst(firstLinkOperatorTrivia, FindTriviaInsideLinkOperator(node, link));
                        firstLinkDotNameTrivia = SelectFirst(firstLinkDotNameTrivia, FindTriviaBetweenLinkDotAndName(link));
                    }
                    break;
            }
        }

        var (firstSeparatorTrivia, isDeclaratorComma) = FindTriviaInFrontOfComma(node);

        var firstAttachedPartTrivia = FindTriviaInAttachedPartGap(node);

        // Of two shapes that start at the same trivia, the one listed first wins: FMT-UNSUP-004 keeps its reason for a comment,
        // and FMT-UNSUP-009 for a directive, in front of a comma between two declarators; FMT-UNSUP-009 gives its own for a
        // comment in front of every other comma inside a variable declaration, and FMT-UNSUP-010 gives its own for every other
        // directive inside a field declaration with several declarators. FMT-UNSUP-011, FMT-UNSUP-012, and FMT-UNSUP-013 are listed
        // last, so FMT-UNSUP-004 and FMT-UNSUP-010 keep their reason for a comment or directive behind a binary operator, and
        // FMT-UNSUP-010 keeps its reason for a directive on either side of the arrow of an expression body, that they refuse as well.
        // FMT-UNSUP-014 is listed after them, so FMT-UNSUP-004 and FMT-UNSUP-010 keep their reason for a comment or directive in
        // the gap of an attached part that they refuse as well. FMT-UNSUP-015 and FMT-UNSUP-016 are listed last, so FMT-UNSUP-004
        // and FMT-UNSUP-010 keep their reason for a comment or directive inside a link operator or between a link's dot and its
        // member name that they refuse as well. FMT-UNSUP-017, FMT-UNSUP-018, and FMT-UNSUP-019 are listed after every other
        // shape, so FMT-UNSUP-004, FMT-UNSUP-009, and FMT-UNSUP-010 keep their reason for a comment or directive they refuse as well
        var candidates = CreateDeclarationCandidates(isDeclaratorComma, firstComment, firstSeparatorTrivia, firstFieldDirective);

        candidates.AddRange([
                                (firstOperatorTrivia, ConditionalOperatorCommentReason),
                                (firstBinaryOperatorTrivia, BinaryOperatorCommentReason),
                                (firstArrowTrivia, ExpressionBodyArrowCommentReason),
                                (firstArrowLeadingTrivia, ExpressionBodyArrowLeadingCommentReason),
                                (firstAttachedPartTrivia, AttachedPartCommentReason),
                                (firstLinkOperatorTrivia, LinkOperatorCommentReason),
                                (firstLinkDotNameTrivia, LinkDotNameCommentReason)
                            ]);

        AddLastCandidates(node, candidates);

        return CreateFirstUnsupportedSyntax(node, candidates);
    }

    /// <summary>
    /// Creates the candidates of the shapes that can start at the same trivia inside a variable or field declaration, in the
    /// order that decides which reason wins: FMT-UNSUP-004 before FMT-UNSUP-009 before FMT-UNSUP-010 when the trivia stands in
    /// front of a comma between two declarators, and the reverse order otherwise
    /// </summary>
    /// <param name="isDeclaratorComma">Whether the first trivia in front of a comma stands in front of a comma between two declarators</param>
    /// <param name="firstComment">The first comment inside a variable declaration (FMT-UNSUP-004)</param>
    /// <param name="firstSeparatorTrivia">The first trivia between a list element and a comma on a later line (FMT-UNSUP-009)</param>
    /// <param name="firstFieldDirective">The first directive inside a field declaration with several declarators (FMT-UNSUP-010)</param>
    /// <returns>The candidates in their order of precedence</returns>
    private static List<(SyntaxTrivia? Trivia, string Reason)> CreateDeclarationCandidates(bool isDeclaratorComma, SyntaxTrivia? firstComment, SyntaxTrivia? firstSeparatorTrivia, SyntaxTrivia? firstFieldDirective)
    {
        return isDeclaratorComma
                   ? [
                         (firstComment, VariableDeclarationCommentReason),
                         (firstSeparatorTrivia, ListSeparatorCommentReason),
                         (firstFieldDirective, FieldDeclarationDirectiveReason)
                     ]
                   : [
                         (firstFieldDirective, FieldDeclarationDirectiveReason),
                         (firstSeparatorTrivia, ListSeparatorCommentReason),
                         (firstComment, VariableDeclarationCommentReason)
                     ];
    }

    /// <summary>
    /// Creates the unsupported shape of the candidate whose trivia starts first; of two candidates that start at the same
    /// trivia, the one listed first wins
    /// </summary>
    /// <param name="node">The node the candidates were found in</param>
    /// <param name="candidates">The first trivia of every shape, or <see langword="null"/> for a shape the node does not hold, with the shape's reason</param>
    /// <returns>The shape with its location in the node's syntax tree, or <see langword="null"/> when the node holds none</returns>
    private static UnsupportedSyntax CreateFirstUnsupportedSyntax(SyntaxNode node, List<(SyntaxTrivia? Trivia, string Reason)> candidates)
    {
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
    /// wherever it stands in the gap's text, also inside a multi-line comment. The node's tokens are walked once in text order,
    /// carrying the last token with text and the tokens without text behind it, so the cost grows linearly with the number of
    /// tokens. A comma with no token with text in front of it inside the node is skipped, because the node alone does not decide
    /// its gap
    /// </summary>
    /// <param name="node">The node</param>
    /// <returns>The first such trivia, or <see langword="null"/>, and whether its comma separates two declarators of a variable declaration</returns>
    private static (SyntaxTrivia? Trivia, bool IsDeclaratorComma) FindTriviaInFrontOfComma(SyntaxNode node)
    {
        SyntaxToken? previousToken = null;
        var zeroWidthTokens = new List<SyntaxToken>();

        foreach (var token in node.DescendantTokens())
        {
            if (token.Span.IsEmpty)
            {
                zeroWidthTokens.Add(token);

                continue;
            }

            if (token.IsKind(SyntaxKind.CommaToken)
                && previousToken != null)
            {
                var firstTrivia = FindTriviaInGap(previousToken.Value, zeroWidthTokens, token);

                if (firstTrivia != null)
                {
                    return (firstTrivia, token.Parent is VariableDeclarationSyntax);
                }
            }

            previousToken = token;
            zeroWidthTokens.Clear();
        }

        return (null, false);
    }

    /// <summary>
    /// Finds the first comment, directive, or disabled text in the gap between two tokens, when the gap's text holds a line break.
    /// The gap is the trailing trivia of the first token, the trivia of every token without text between them, and the leading
    /// trivia of the second, in text order
    /// </summary>
    /// <param name="previousToken">The token in front of the gap</param>
    /// <param name="zeroWidthTokens">The tokens without text between the two tokens, in text order</param>
    /// <param name="nextToken">The token behind the gap</param>
    /// <returns>The first such trivia, or <see langword="null"/> when there is none or the gap holds no line break</returns>
    private static SyntaxTrivia? FindTriviaInGap(SyntaxToken previousToken, List<SyntaxToken> zeroWidthTokens, SyntaxToken nextToken)
    {
        SyntaxTrivia? firstTrivia = null;
        var hasLineBreak = false;

        VisitGapTrivia(previousToken.TrailingTrivia, ref firstTrivia, ref hasLineBreak);

        foreach (var token in zeroWidthTokens)
        {
            VisitGapTrivia(token.LeadingTrivia, ref firstTrivia, ref hasLineBreak);
            VisitGapTrivia(token.TrailingTrivia, ref firstTrivia, ref hasLineBreak);
        }

        VisitGapTrivia(nextToken.LeadingTrivia, ref firstTrivia, ref hasLineBreak);

        return hasLineBreak
                   ? firstTrivia
                   : null;
    }

    /// <summary>
    /// Visits one trivia list of a gap between two tokens: records whether its text holds a line break, and the first comment,
    /// directive, or disabled text when none has been found yet
    /// </summary>
    /// <param name="triviaList">The trivia list</param>
    /// <param name="firstTrivia">The first comment, directive, or disabled text of the gap found so far, or <see langword="null"/></param>
    /// <param name="hasLineBreak">Whether the gap's text visited so far holds a line break</param>
    private static void VisitGapTrivia(SyntaxTriviaList triviaList, ref SyntaxTrivia? firstTrivia, ref bool hasLineBreak)
    {
        foreach (var trivia in triviaList)
        {
            var text = trivia.ToFullString();

            hasLineBreak |= text.Contains('\n') || text.Contains('\r');

            if (firstTrivia == null
                && (SyntaxTriviaUtilities.IsCommentTrivia(trivia) || SyntaxTriviaUtilities.IsDirectiveOrDisabledTextTrivia(trivia)))
            {
                firstTrivia = trivia;
            }
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
    /// Finds the first comment, directive, or disabled text between the arrow of an expression body and the token in front of it:
    /// in the trailing trivia of that token, or in the leading trivia of the <c>=&gt;</c> (FMT-UNSUP-013). The gap lies between
    /// two tokens, so a node that holds the arrow but not the token in front of it, such as the arrow clause itself, does not
    /// hold it (FMT-UNSUP-003)
    /// </summary>
    /// <param name="node">The node the precheck decides on</param>
    /// <param name="arrowExpressionClause">The arrow clause</param>
    /// <returns>The first such trivia, or <see langword="null"/></returns>
    private static SyntaxTrivia? FindTriviaInFrontOfExpressionBodyArrow(SyntaxNode node, ArrowExpressionClauseSyntax arrowExpressionClause)
    {
        var arrowToken = arrowExpressionClause.ArrowToken;
        var previousToken = SyntaxTokenNavigationUtilities.GetPreviousToken(arrowToken);

        if (previousToken == default
            || node.Span.Contains(previousToken.Span) == false)
        {
            return null;
        }

        return FindFirstUnsupportedOperatorTrivia(previousToken.TrailingTrivia)
                   ?? FindFirstUnsupportedOperatorTrivia(arrowToken.LeadingTrivia);
    }

    /// <summary>
    /// Finds the first comment, directive, or disabled text in a gap of an attached part (FMT-UNSUP-014): in front of the
    /// first token of an argument list, an element access, a conditional element access, or a null-forgiving operator, or
    /// between the <c>?</c> and the <c>[</c> of a conditional element access
    /// (<see cref="AttachedPartGapUtilities.IsAttachedPartGap"/>), in the trailing trivia of the token in front of the gap or
    /// in the leading trivia of the token behind it. The gap lies between two tokens, so a gap whose token in front lies
    /// outside the node is not looked at (FMT-UNSUP-003). The node's tokens are walked once in text order
    /// </summary>
    /// <param name="node">The node the precheck decides on</param>
    /// <returns>The first such trivia, or <see langword="null"/></returns>
    private static SyntaxTrivia? FindTriviaInAttachedPartGap(SyntaxNode node)
    {
        foreach (var token in node.DescendantTokens())
        {
            if (AttachedPartGapUtilities.IsAttachedPartGap(token) == false)
            {
                continue;
            }

            var previousToken = SyntaxTokenNavigationUtilities.GetPreviousToken(token);

            if (previousToken == default
                || node.Span.Contains(previousToken.Span) == false)
            {
                continue;
            }

            var trivia = FindFirstUnsupportedOperatorTrivia(previousToken.TrailingTrivia)
                             ?? FindFirstUnsupportedOperatorTrivia(token.LeadingTrivia);

            if (trivia != null)
            {
                return trivia;
            }
        }

        return null;
    }

    /// <summary>
    /// Finds the first comment, directive, or disabled text between two tokens of the link operator of a member access or a
    /// member binding (FMT-UNSUP-015): in the trailing trivia of an operator token other than its <c>.</c>, or in the leading
    /// trivia of an operator token other than its first. The chain model's <see cref="FluentChain.GetLinkOperatorTokens"/>
    /// is the one definition of which tokens form the operator, so every gap the line-break phase can join inside a link
    /// operator is refused; the precheck looks at every member access and member binding of its input, which is more than
    /// the links of the chains the phase normalizes. A plain <c>.</c> is one token and has no such gap. The operator's first token lies in front of the
    /// node of a member binding, so an operator whose first token lies outside the input is not looked at (FMT-UNSUP-003)
    /// </summary>
    /// <param name="node">The node the precheck decides on</param>
    /// <param name="link">The member access or member binding</param>
    /// <returns>The first such trivia, or <see langword="null"/></returns>
    private static SyntaxTrivia? FindTriviaInsideLinkOperator(SyntaxNode node, ExpressionSyntax link)
    {
        var tokens = FluentChain.GetLinkOperatorTokens(link);

        if (tokens.Count < 2
            || node.Span.Contains(tokens[0].Span) == false)
        {
            return null;
        }

        for (var tokenIndex = 1; tokenIndex < tokens.Count; tokenIndex++)
        {
            var trivia = FindFirstUnsupportedOperatorTrivia(tokens[tokenIndex - 1].TrailingTrivia)
                             ?? FindFirstUnsupportedOperatorTrivia(tokens[tokenIndex].LeadingTrivia);

            if (trivia != null)
            {
                return trivia;
            }
        }

        return null;
    }

    /// <summary>
    /// Finds the first comment, directive, or disabled text between the <c>.</c> of a simple member access or a member binding
    /// and its member name (FMT-UNSUP-016): in the trailing trivia of the <c>.</c>, or in the leading trivia of the name's first
    /// token. The <c>.</c> and the name belong to the link's own node, so an input that holds the link holds the whole gap, and
    /// one that holds only the name holds none of it (FMT-UNSUP-003). The <c>-&gt;</c> of a pointer member access is no link
    /// </summary>
    /// <param name="link">The member access or member binding</param>
    /// <returns>The first such trivia, or <see langword="null"/></returns>
    private static SyntaxTrivia? FindTriviaBetweenLinkDotAndName(ExpressionSyntax link)
    {
        var (dotToken, name) = link switch
                               {
                                   MemberAccessExpressionSyntax memberAccess when memberAccess.IsKind(SyntaxKind.SimpleMemberAccessExpression) => (memberAccess.OperatorToken, memberAccess.Name),
                                   MemberBindingExpressionSyntax memberBinding => (memberBinding.OperatorToken, memberBinding.Name),
                                   _ => (default, null)
                               };

        if (name == null)
        {
            return null;
        }

        return FindFirstUnsupportedOperatorTrivia(dotToken.TrailingTrivia)
                   ?? FindFirstUnsupportedOperatorTrivia(name.GetFirstToken().LeadingTrivia);
    }

    /// <summary>
    /// Finds the first comment, directive, or disabled text in a gap the line-break phase joins (FMT-UNSUP-017,
    /// <see cref="JoinedGapUtilities.GetGap"/>): in the trailing trivia of the token in front of the gap or in the leading trivia
    /// of the token behind it. The gap behind the <c>is</c> of a type test is FMT-UNSUP-011's and is not looked at here. The gap
    /// lies between two tokens, so a gap whose token in front lies outside the node is not looked at (FMT-UNSUP-003). The node's
    /// tokens are walked once in text order
    /// </summary>
    /// <param name="node">The node the precheck decides on</param>
    /// <returns>The first such trivia and the reason of its gap, or <see langword="null"/></returns>
    private static (SyntaxTrivia? Trivia, string Reason) FindTriviaInJoinedGap(SyntaxNode node)
    {
        SyntaxToken previousToken = default;

        foreach (var token in node.DescendantTokens())
        {
            if (token.Span.IsEmpty)
            {
                continue;
            }

            if (previousToken != default)
            {
                var reason = GetJoinedGapReason(JoinedGapUtilities.GetGap(previousToken, token));

                if (reason != null)
                {
                    var trivia = FindFirstUnsupportedOperatorTrivia(previousToken.TrailingTrivia)
                                     ?? FindFirstUnsupportedOperatorTrivia(token.LeadingTrivia);

                    if (trivia != null)
                    {
                        return (trivia, reason);
                    }
                }
            }

            previousToken = token;
        }

        return (null, null);
    }

    /// <summary>
    /// Gets the reason the precheck gives for a joined gap (FMT-UNSUP-017)
    /// </summary>
    /// <param name="gap">The gap</param>
    /// <returns>The reason, or <see langword="null"/> for a gap FMT-UNSUP-017 does not refuse</returns>
    private static string GetJoinedGapReason(JoinedGap gap)
    {
        return gap switch
               {
                   JoinedGap.KeywordOperand => KeywordOperandCommentReason,
                   JoinedGap.IsPattern => IsPatternCommentReason,
                   JoinedGap.PatternCombinator => PatternCombinatorCommentReason,
                   JoinedGap.SwitchArmWhen => SwitchArmWhenCommentReason,
                   JoinedGap.SwitchArmArrow => SwitchArmArrowCommentReason,
                   JoinedGap.BaseList => BaseListCommentReason,
                   JoinedGap.Modifier or JoinedGap.ParameterModifier => ModifierCommentReason,
                   JoinedGap.CatchFilter => CatchFilterCommentReason,
                   JoinedGap.ForHeader => ForHeaderCommentReason,
                   JoinedGap.ForEachExpression => ForEachExpressionCommentReason,
                   JoinedGap.Cast => CastCommentReason,
                   JoinedGap.LambdaArrow => LambdaArrowCommentReason,
                   JoinedGap.With => WithCommentReason,
                   JoinedGap.AnonymousMemberEquals => AnonymousMemberEqualsCommentReason,
                   JoinedGap.LetClauseEquals => LetClauseEqualsCommentReason,
                   _ => null
               };
    }

    /// <summary>
    /// Adds the candidates of the shapes listed after every other shape: a gap the line-break phase joins (FMT-UNSUP-017), a
    /// list of omitted type arguments (FMT-UNSUP-018), and an assignment inside a switch arm or an anonymous-object member
    /// (FMT-UNSUP-019)
    /// </summary>
    /// <param name="node">The node the precheck decides on</param>
    /// <param name="candidates">The candidates in their order of precedence</param>
    private static void AddLastCandidates(SyntaxNode node, List<(SyntaxTrivia? Trivia, string Reason)> candidates)
    {
        var (firstJoinedGapTrivia, joinedGapReason) = FindTriviaInJoinedGap(node);
        var (firstOmittedTypeArgumentTrivia, firstAssignmentInArmTrivia) = FindTriviaInOmittedTypeArgumentListOrBehindAssignmentInArm(node);

        candidates.AddRange([
                                (firstJoinedGapTrivia, joinedGapReason),
                                (firstOmittedTypeArgumentTrivia, OmittedTypeArgumentCommentReason),
                                (firstAssignmentInArmTrivia, AssignmentInArmCommentReason)
                            ]);
    }

    /// <summary>
    /// Finds the first comment, directive, or disabled text inside a list of omitted type arguments (FMT-UNSUP-018), and the
    /// first one behind an assignment operator inside a switch arm or an anonymous-object member (FMT-UNSUP-019), in one walk
    /// over the node
    /// </summary>
    /// <param name="node">The node the precheck decides on</param>
    /// <returns>The first trivia of each shape, or <see langword="null"/> for a shape the node does not hold</returns>
    private static (SyntaxTrivia? OmittedTypeArgumentTrivia, SyntaxTrivia? AssignmentInArmTrivia) FindTriviaInOmittedTypeArgumentListOrBehindAssignmentInArm(SyntaxNode node)
    {
        SyntaxTrivia? firstOmittedTypeArgumentTrivia = null;
        SyntaxTrivia? firstAssignmentInArmTrivia = null;

        foreach (var descendant in node.DescendantNodesAndSelf())
        {
            switch (descendant)
            {
                case TypeArgumentListSyntax typeArgumentList:
                    {
                        firstOmittedTypeArgumentTrivia = SelectFirst(firstOmittedTypeArgumentTrivia, FindTriviaInOmittedTypeArgumentList(typeArgumentList));
                    }
                    break;

                case AssignmentExpressionSyntax assignment:
                    {
                        firstAssignmentInArmTrivia = SelectFirst(firstAssignmentInArmTrivia, FindTriviaBehindAssignmentInArm(node, assignment));
                    }
                    break;
            }
        }

        return (firstOmittedTypeArgumentTrivia, firstAssignmentInArmTrivia);
    }

    /// <summary>
    /// Finds the first comment, directive, or disabled text inside a type argument list whose every argument is omitted
    /// (FMT-UNSUP-018): in the trailing trivia of its <c>&lt;</c> or of a comma, or in the leading trivia of a comma or of its
    /// <c>&gt;</c>
    /// </summary>
    /// <param name="typeArgumentList">The type argument list</param>
    /// <returns>The first such trivia, or <see langword="null"/></returns>
    private static SyntaxTrivia? FindTriviaInOmittedTypeArgumentList(TypeArgumentListSyntax typeArgumentList)
    {
        if (typeArgumentList.Arguments.All(static argument => argument is OmittedTypeArgumentSyntax) == false)
        {
            return null;
        }

        foreach (var token in typeArgumentList.DescendantTokens())
        {
            var trivia = (token == typeArgumentList.LessThanToken ? null : FindFirstUnsupportedOperatorTrivia(token.LeadingTrivia))
                             ?? (token == typeArgumentList.GreaterThanToken ? null : FindFirstUnsupportedOperatorTrivia(token.TrailingTrivia));

            if (trivia != null)
            {
                return trivia;
            }
        }

        return null;
    }

    /// <summary>
    /// Finds the first comment, directive, or disabled text between the operator of an assignment expression and its right
    /// operand when the assignment stands inside a switch-expression arm or an anonymous-object member declarator (FMT-UNSUP-019):
    /// the nearest arm, member declarator, statement, or member declaration around the assignment is an arm or a member
    /// declarator, and it lies inside the node (FMT-UNSUP-003)
    /// </summary>
    /// <param name="node">The node the precheck decides on</param>
    /// <param name="assignment">The assignment expression</param>
    /// <returns>The first such trivia, or <see langword="null"/></returns>
    private static SyntaxTrivia? FindTriviaBehindAssignmentInArm(SyntaxNode node, AssignmentExpressionSyntax assignment)
    {
        var trivia = FindFirstUnsupportedOperatorTrivia(assignment.OperatorToken.TrailingTrivia)
                         ?? FindFirstUnsupportedOperatorTrivia(assignment.Right.GetFirstToken().LeadingTrivia);

        if (trivia == null)
        {
            return null;
        }

        for (SyntaxNode current = assignment; current != node && current != null;)
        {
            current = current.Parent;

            switch (current)
            {
                case SwitchExpressionArmSyntax:
                case AnonymousObjectMemberDeclaratorSyntax:
                    return trivia;

                case StatementSyntax:
                case MemberDeclarationSyntax:
                    return null;
            }
        }

        return null;
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