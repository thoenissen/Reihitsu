using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Formatter.Pipeline.Core.Enumerations;

namespace Reihitsu.Formatter.Pipeline.Core.Utilities;

/// <summary>
/// The one statement of the gaps the line-break phase joins behind a keyword, <c>is</c>, a pattern combinator, a modifier, a
/// cast, an arm, a base list, a filter, a loop header, a lambda, or <c>with</c>, and around the <c>=</c> of an anonymous-object
/// member and of a <c>let</c> clause (FMT-BREAK-016, FMT-BREAK-017, FMT-BREAK-041), which the precheck refuses when they hold a
/// comment, a directive, or disabled text (FMT-UNSUP-017). Each gap lies between two neighbouring tokens and is decided by the
/// syntax tree alone, from the roles of those two tokens
/// </summary>
internal static class JoinedGapUtilities
{
    #region Methods

    /// <summary>
    /// Determines the joined gap between two neighbouring tokens
    /// </summary>
    /// <param name="previousToken">The token in front of the gap</param>
    /// <param name="token">The token behind the gap</param>
    /// <returns>The gap, or <see cref="JoinedGap.None"/> when the two tokens hold no joined gap between them</returns>
    public static JoinedGap GetGap(SyntaxToken previousToken, SyntaxToken token)
    {
        var gap = GetGapBehind(previousToken);

        return gap != JoinedGap.None
                   ? gap
                   : GetGapInFrontOf(token);
    }

    /// <summary>
    /// Determines whether a token is a modifier of a declaration, a parameter, a function-pointer parameter, an accessor, a
    /// lambda, or an anonymous method
    /// </summary>
    /// <param name="token">The token</param>
    /// <returns><see langword="true"/> if the token is such a modifier; otherwise, <see langword="false"/></returns>
    public static bool IsModifier(SyntaxToken token)
    {
        var modifiers = token.Parent switch
                        {
                            MemberDeclarationSyntax member => member.Modifiers,
                            LocalDeclarationStatementSyntax localDeclaration => localDeclaration.Modifiers,
                            LocalFunctionStatementSyntax localFunction => localFunction.Modifiers,
                            BaseParameterSyntax parameter => parameter.Modifiers,
                            AccessorDeclarationSyntax accessor => accessor.Modifiers,
                            AnonymousFunctionExpressionSyntax anonymousFunction => anonymousFunction.Modifiers,
                            _ => default
                        };

        return modifiers.IndexOf(token) >= 0;
    }

    /// <summary>
    /// Determines the joined gap behind a token, decided by the role of the token in front of the gap
    /// </summary>
    /// <param name="previousToken">The token in front of the gap</param>
    /// <returns>The gap, or <see cref="JoinedGap.None"/></returns>
    private static JoinedGap GetGapBehind(SyntaxToken previousToken)
    {
        var gap = GetKeywordGapBehind(previousToken);

        if (gap == JoinedGap.None)
        {
            gap = GetPunctuationGapBehind(previousToken);
        }

        return gap == JoinedGap.None
                   ? GetModifierGap(previousToken)
                   : gap;
    }

    /// <summary>
    /// Determines the joined gap behind a keyword that takes an operand: <c>return</c>, <c>throw</c>, <c>await</c>,
    /// <c>new</c>, <c>is</c>, <c>and</c>, <c>or</c>, <c>not</c>, and the <c>in</c> of a <c>foreach</c> statement
    /// </summary>
    /// <param name="previousToken">The token in front of the gap</param>
    /// <returns>The gap, or <see cref="JoinedGap.None"/></returns>
    private static JoinedGap GetKeywordGapBehind(SyntaxToken previousToken)
    {
        return (previousToken.Kind(), previousToken.Parent) switch
               {
                   (SyntaxKind.ReturnKeyword, ReturnStatementSyntax { Expression: not null } or YieldStatementSyntax { Expression: not null }) => JoinedGap.KeywordOperand,
                   (SyntaxKind.ThrowKeyword, ThrowStatementSyntax { Expression: not null } or ThrowExpressionSyntax) => JoinedGap.KeywordOperand,
                   (SyntaxKind.AwaitKeyword, AwaitExpressionSyntax or CommonForEachStatementSyntax or UsingStatementSyntax or LocalDeclarationStatementSyntax) => JoinedGap.KeywordOperand,
                   (SyntaxKind.NewKeyword, ObjectCreationExpressionSyntax or ArrayCreationExpressionSyntax or ImplicitArrayCreationExpressionSyntax) => JoinedGap.KeywordOperand,
                   (SyntaxKind.IsKeyword, IsPatternExpressionSyntax) => JoinedGap.IsPattern,
                   (SyntaxKind.IsKeyword, BinaryExpressionSyntax) => JoinedGap.TypeTest,
                   (SyntaxKind.AndKeyword or SyntaxKind.OrKeyword, BinaryPatternSyntax) => JoinedGap.PatternCombinator,
                   (SyntaxKind.NotKeyword, UnaryPatternSyntax) => JoinedGap.PatternCombinator,
                   (SyntaxKind.InKeyword, CommonForEachStatementSyntax) => JoinedGap.ForEachExpression,
                   _ => JoinedGap.None
               };
    }

    /// <summary>
    /// Determines the joined gap behind a punctuation token: a <c>;</c> of a <c>for</c> header, the <c>)</c> of a cast, the
    /// <c>=&gt;</c> of a lambda with an expression body, and the <c>=</c> of an anonymous-object member or a <c>let</c> clause
    /// </summary>
    /// <param name="previousToken">The token in front of the gap</param>
    /// <returns>The gap, or <see cref="JoinedGap.None"/></returns>
    private static JoinedGap GetPunctuationGapBehind(SyntaxToken previousToken)
    {
        return (previousToken.Kind(), previousToken.Parent) switch
               {
                   (SyntaxKind.SemicolonToken, ForStatementSyntax) => JoinedGap.ForHeader,
                   (SyntaxKind.CloseParenToken, CastExpressionSyntax) => JoinedGap.Cast,
                   (SyntaxKind.EqualsGreaterThanToken, LambdaExpressionSyntax { ExpressionBody: not null }) => JoinedGap.LambdaArrow,
                   (SyntaxKind.EqualsToken, _) => GetEqualsGap(previousToken),
                   _ => JoinedGap.None
               };
    }

    /// <summary>
    /// Determines the joined gap in front of a token, decided by the role of the token behind the gap
    /// </summary>
    /// <param name="token">The token behind the gap</param>
    /// <returns>The gap, or <see cref="JoinedGap.None"/></returns>
    private static JoinedGap GetGapInFrontOf(SyntaxToken token)
    {
        var parent = token.Parent;

        return token.Kind() switch
               {
                   SyntaxKind.WhenKeyword => parent switch
                                             {
                                                 WhenClauseSyntax { Parent: SwitchExpressionArmSyntax } => JoinedGap.SwitchArmWhen,
                                                 CatchFilterClauseSyntax => JoinedGap.CatchFilter,
                                                 _ => JoinedGap.None
                                             },
                   SyntaxKind.EqualsGreaterThanToken => parent is SwitchExpressionArmSyntax
                                                            ? JoinedGap.SwitchArmArrow
                                                            : JoinedGap.None,
                   SyntaxKind.ColonToken => parent is BaseListSyntax
                                                ? JoinedGap.BaseList
                                                : JoinedGap.None,
                   SyntaxKind.WithKeyword => parent is WithExpressionSyntax
                                                 ? JoinedGap.With
                                                 : JoinedGap.None,
                   SyntaxKind.EqualsToken => GetEqualsGap(token),
                   _ => JoinedGap.None
               };
    }

    /// <summary>
    /// Determines the joined gap next to the <c>=</c> of an anonymous-object member declarator or of a <c>let</c> clause
    /// </summary>
    /// <param name="equalsToken">The <c>=</c></param>
    /// <returns>The gap, or <see cref="JoinedGap.None"/> for any other <c>=</c></returns>
    private static JoinedGap GetEqualsGap(SyntaxToken equalsToken)
    {
        return equalsToken.Parent switch
               {
                   NameEqualsSyntax { Parent: AnonymousObjectMemberDeclaratorSyntax } => JoinedGap.AnonymousMemberEquals,
                   LetClauseSyntax => JoinedGap.LetClauseEquals,
                   _ => JoinedGap.None
               };
    }

    /// <summary>
    /// Determines the joined gap behind a modifier
    /// </summary>
    /// <param name="token">The token in front of the gap</param>
    /// <returns><see cref="JoinedGap.ParameterModifier"/> for a modifier of a parameter or a function-pointer parameter, <see cref="JoinedGap.Modifier"/> for every other modifier, or <see cref="JoinedGap.None"/></returns>
    private static JoinedGap GetModifierGap(SyntaxToken token)
    {
        if (IsModifier(token) == false)
        {
            return JoinedGap.None;
        }

        return token.Parent is BaseParameterSyntax
                   ? JoinedGap.ParameterModifier
                   : JoinedGap.Modifier;
    }

    #endregion // Methods
}