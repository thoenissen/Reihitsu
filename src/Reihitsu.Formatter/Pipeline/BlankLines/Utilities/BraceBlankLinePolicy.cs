using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Core;
using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Pipeline.LineBreaks.Rewriter;
using Reihitsu.Formatter.Pipeline.LineBreaks.Utilities;

namespace Reihitsu.Formatter.Pipeline.BlankLines.Utilities;

/// <summary>
/// The one owner of which braces, brackets, and parentheses the blank-line rules next to such a token reach: the opening
/// brace of a block statement that follows a statement (FMT-BLANK-017), the tokens a comment glued to them keeps no blank
/// line above (FMT-BLANK-018, FMT-BLANK-020), and the closing brackets and parentheses no blank line precedes
/// (FMT-BLANK-019). The blank-line subphases read these facts from here
/// </summary>
internal static class BraceBlankLinePolicy
{
    #region Methods

    /// <summary>
    /// Determines whether a token is the opening brace of a block statement that follows another statement of the same
    /// block or switch section (FMT-BLANK-017). A detached formatting root has no parent, so for the root block the
    /// position it has in its document decides (FMT-PIPE-024)
    /// </summary>
    /// <param name="token">The token to inspect, still part of the visited tree</param>
    /// <param name="context">The formatting context, which carries the position of the formatting root</param>
    /// <returns><see langword="true"/> if the token opens a block statement that follows a statement</returns>
    public static bool IsOpeningBraceOfBlockStatementAfterStatement(SyntaxToken token, FormattingContext context)
    {
        if (token.IsKind(SyntaxKind.OpenBraceToken) == false
            || token.Parent is not BlockSyntax block)
        {
            return false;
        }

        return block.Parent == null
                   ? context.RootListPosition.HasPreviousStatement
                   : BlankLineSpacingPolicy.IsBlockStatementAfterStatement(block);
    }

    /// <summary>
    /// Determines whether a comment that starts its line glued to the token gets no blank line above it (FMT-BLANK-018), and
    /// whether the blank lines between an own-line comment and the token are removed (FMT-BLANK-020): every opening and
    /// closing brace other than the braces of an array initializer, an extension block, or an interpolation, the opening
    /// brace of a property pattern, the opening brace of a complex element initializer that stays on one line, the closing
    /// brace of a collection initializer assigned to a member, and the opening brace of a block statement that follows a
    /// statement; the closing bracket of a collection expression or a list pattern; and the closing parenthesis of a
    /// parenthesized pattern
    /// </summary>
    /// <param name="token">The token to inspect, still part of the visited tree</param>
    /// <param name="context">The formatting context, which carries the position of the formatting root</param>
    /// <returns><see langword="true"/> if the token is one FMT-BLANK-018 and FMT-BLANK-020 name</returns>
    /// <remarks>
    /// The set is the one whose glued comment has had no blank line above it in the formatter's output. A complex element
    /// initializer that spans several lines and a collection initializer assigned to a member follow the line-break layout
    /// of their braces (<see cref="LineBreakInitializerRewriter.IsAssignedCollectionInitializer"/>): the opening brace of the
    /// former is laid out on its own line like an object initializer's, the closing brace of the latter is not
    /// </remarks>
    public static bool KeepsGluedCommentWithoutBlankLine(SyntaxToken token, FormattingContext context)
    {
        switch (token.Kind())
        {
            case SyntaxKind.OpenBraceToken:
                {
                    return token.Parent switch
                           {
                               InitializerExpressionSyntax initializer => initializer.Kind() switch
                                                                          {
                                                                              SyntaxKind.ArrayInitializerExpression => false,
                                                                              SyntaxKind.ComplexElementInitializerExpression => LineBreakTriviaUtilities.SpansMultipleLines(initializer.OpenBraceToken, initializer.CloseBraceToken),
                                                                              _ => true,
                                                                          },
                               PropertyPatternClauseSyntax or ExtensionBlockDeclarationSyntax or InterpolationSyntax => false,
                               _ => IsOpeningBraceOfBlockStatementAfterStatement(token, context) == false,
                           };
                }

            case SyntaxKind.CloseBraceToken:
                {
                    return token.Parent switch
                           {
                               InitializerExpressionSyntax initializer => initializer.IsKind(SyntaxKind.ArrayInitializerExpression) == false
                                                                          && LineBreakInitializerRewriter.IsAssignedCollectionInitializer(initializer, context.RootConstruct) == false,
                               ExtensionBlockDeclarationSyntax or InterpolationSyntax => false,
                               _ => true,
                           };
                }

            default:
                {
                    return IsClosingTokenOfPatternOrCollectionExpression(token);
                }
        }
    }

    /// <summary>
    /// Determines whether a token is the closing bracket of a collection expression or a list pattern, or the closing
    /// parenthesis of a parenthesized pattern, before which no blank line stands (FMT-BLANK-019)
    /// </summary>
    /// <param name="token">The token to inspect, still part of the visited tree</param>
    /// <returns><see langword="true"/> if the token is such a closing bracket or parenthesis</returns>
    public static bool IsClosingTokenOfPatternOrCollectionExpression(SyntaxToken token)
    {
        return token.Kind() switch
               {
                   SyntaxKind.CloseBracketToken => token.Parent is CollectionExpressionSyntax or ListPatternSyntax,
                   SyntaxKind.CloseParenToken => token.Parent is ParenthesizedPatternSyntax,
                   _ => false,
               };
    }

    #endregion // Methods
}