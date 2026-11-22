namespace Reihitsu.Formatter.Pipeline.Core.Enumerations;

/// <summary>
/// The gaps between two tokens that the line-break phase joins onto one line (FMT-BREAK-016, FMT-BREAK-017, FMT-BREAK-041) and
/// that the precheck refuses when they hold a comment, a directive, or disabled text (FMT-UNSUP-017)
/// </summary>
internal enum JoinedGap
{
    /// <summary>
    /// No such gap
    /// </summary>
    None,

    /// <summary>
    /// Between <c>return</c>, <c>yield return</c>, <c>throw</c>, <c>await</c>, or <c>new</c> and what follows it
    /// </summary>
    KeywordOperand,

    /// <summary>
    /// Between the <c>is</c> of a pattern match and its pattern
    /// </summary>
    IsPattern,

    /// <summary>
    /// Between the <c>is</c> of a type test and its type; joined by FMT-BREAK-041, refused by FMT-UNSUP-011
    /// </summary>
    TypeTest,

    /// <summary>
    /// Between <c>and</c>, <c>or</c>, or <c>not</c> and its pattern
    /// </summary>
    PatternCombinator,

    /// <summary>
    /// In front of the <c>when</c> of a switch-expression arm
    /// </summary>
    SwitchArmWhen,

    /// <summary>
    /// In front of the <c>=&gt;</c> of a switch-expression arm
    /// </summary>
    SwitchArmArrow,

    /// <summary>
    /// In front of the <c>:</c> of a base list
    /// </summary>
    BaseList,

    /// <summary>
    /// Behind a modifier of a declaration that is no parameter
    /// </summary>
    Modifier,

    /// <summary>
    /// Behind a modifier of a parameter or of a function-pointer parameter; joined by FMT-BREAK-015
    /// </summary>
    ParameterModifier,

    /// <summary>
    /// In front of the <c>when</c> of a catch filter clause
    /// </summary>
    CatchFilter,

    /// <summary>
    /// Behind a <c>;</c> of a <c>for</c> statement's header
    /// </summary>
    ForHeader,

    /// <summary>
    /// Behind the <c>in</c> of a <c>foreach</c> statement
    /// </summary>
    ForEachExpression,

    /// <summary>
    /// Between a cast and its operand
    /// </summary>
    Cast,

    /// <summary>
    /// Behind the <c>=&gt;</c> of a lambda with an expression body
    /// </summary>
    LambdaArrow,

    /// <summary>
    /// In front of <c>with</c>
    /// </summary>
    With,

    /// <summary>
    /// In front of or behind the <c>=</c> of an anonymous-object member declarator; joined by FMT-BREAK-016
    /// </summary>
    AnonymousMemberEquals,

    /// <summary>
    /// In front of or behind the <c>=</c> of a <c>let</c> clause; joined by FMT-BREAK-016
    /// </summary>
    LetClauseEquals
}