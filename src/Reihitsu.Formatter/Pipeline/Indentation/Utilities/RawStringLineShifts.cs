using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Core;

namespace Reihitsu.Formatter.Pipeline.Indentation.Utilities;

/// <summary>
/// Determines how many columns the indentation phase moves each line of a multi-line raw string literal: plain, UTF-8,
/// or interpolated, and applies those moves. A literal moves every line after its opening line by the distance between
/// its opening quotes and its closing marker; a literal nested in an interpolation hole adds its own distance to the
/// lines inside it, measured after the literals around it moved its opening quotes and its closing marker; and a line
/// that starts inside the text of a nested literal other than a raw string, or inside a nested <c>/* */</c> or
/// <c>/** */</c> comment, never moves. The indentation phase measures every anchor after these moves and applies them
/// itself, so a line that is aligned to a token or a comment that takes a token's column ends up where that token
/// ends up, and the move is computed once
/// </summary>
internal sealed class RawStringLineShifts
{
    #region Fields

    /// <summary>
    /// The multi-line raw string literals of the tree, outermost and earliest first
    /// </summary>
    private readonly List<RawStringLiteral> _literals = [];

    /// <summary>
    /// Maps each line that a literal moves to the indices of the literals that move it, outermost first
    /// </summary>
    private readonly Dictionary<int, List<int>> _movingLiterals = [];

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="root">The syntax root</param>
    /// <param name="text">The text of the syntax tree</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public RawStringLineShifts(SyntaxNode root, SourceText text, CancellationToken cancellationToken)
    {
        foreach (var node in root.DescendantNodesAndSelf())
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (IsMultiLineRawStringLiteral(node) == false)
            {
                continue;
            }

            var openingLine = text.Lines.GetLineFromPosition(node.SpanStart);
            var closingLine = text.Lines.GetLineFromPosition(node.Span.End);

            if (openingLine.LineNumber == closingLine.LineNumber)
            {
                continue;
            }

            var literalIndex = _literals.Count;

            _literals.Add(new RawStringLiteral(node,
                                               openingLine.LineNumber,
                                               closingLine.LineNumber,
                                               GetLeadingSpaceCount(text, closingLine),
                                               GetQuoteOffset(node)));

            for (var lineNumber = openingLine.LineNumber + 1; lineNumber <= closingLine.LineNumber; lineNumber++)
            {
                if (StartsInsideNestedText(root, text.Lines[lineNumber].Start))
                {
                    continue;
                }

                if (_movingLiterals.TryGetValue(lineNumber, out var literalIndices) == false)
                {
                    literalIndices = [];
                    _movingLiterals[lineNumber] = literalIndices;
                }

                literalIndices.Add(literalIndex);
            }
        }
    }

    #endregion // Constructor

    #region Properties

    /// <summary>
    /// Whether the tree holds no multi-line raw string literal, so no line moves
    /// </summary>
    public bool IsEmpty => _literals.Count == 0;

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Determines whether the node is a multi-line raw string literal: plain, UTF-8, or interpolated
    /// </summary>
    /// <param name="node">The node to inspect</param>
    /// <returns><see langword="true"/> if the node is a multi-line raw string literal</returns>
    public static bool IsMultiLineRawStringLiteral(SyntaxNode node)
    {
        return node switch
               {
                   LiteralExpressionSyntax literal => literal.Token.IsKind(SyntaxKind.MultiLineRawStringLiteralToken)
                                                      || literal.Token.IsKind(SyntaxKind.Utf8MultiLineRawStringLiteralToken),
                   InterpolatedStringExpressionSyntax interpolated => interpolated.StringStartToken.IsKind(SyntaxKind.InterpolatedMultiLineRawStringStartToken),
                   _ => false
               };
    }

    /// <summary>
    /// Counts the number of leading space characters of a line
    /// </summary>
    /// <param name="text">The text of the syntax tree</param>
    /// <param name="line">The line to examine</param>
    /// <returns>The number of leading space characters</returns>
    public static int GetLeadingSpaceCount(SourceText text, TextLine line)
    {
        var count = 0;

        while (line.Start + count < line.End
               && text[line.Start + count] == ' ')
        {
            count++;
        }

        return count;
    }

    /// <summary>
    /// Moves the lines of every multi-line raw string literal of an indented tree after its opening line by the number
    /// of columns <paramref name="getShift"/> gives each line. Only literals that are not nested in an interpolation
    /// hole of another multi-line interpolated raw string are walked; a nested literal's lines are lines of the outermost
    /// literal around it, so both move together. The move edits the tokens and the whitespace trivia that hold the moved
    /// indentation and keeps every other node and token, so the annotations of the literal and of its holes survive it
    /// (FMT-PIPE-022). A content line that starts with a hole's opening brace has its moved indentation written as text of
    /// the literal, as the parser reads it (FMT-PIPE-021): appended to the text token in front of the hole, or, on the
    /// first content line, where only the start token stands in front of it, as a new text element in front of the hole.
    /// Placing lines changes no line break, so the line numbers of the indented tree are those the shifts were computed for
    /// </summary>
    /// <param name="root">The indented syntax root</param>
    /// <param name="getShift">Gets the number of columns a line moves</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The syntax root with the lines of its raw string literals moved</returns>
    public static SyntaxNode MoveLines(SyntaxNode root, Func<int, int> getShift, CancellationToken cancellationToken)
    {
        var edits = new Dictionary<SyntaxToken, List<(int LineStart, int OldLength, int NewLength)>>();
        var leadingTexts = new Dictionary<InterpolatedStringExpressionSyntax, int>();
        SourceText text = null;

        foreach (var node in root.DescendantNodesAndSelf())
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (IsMultiLineRawStringLiteral(node) == false
                || IsNestedInRawStringHole(node))
            {
                continue;
            }

            text ??= root.SyntaxTree.GetText(cancellationToken);

            CollectEdits(root, node, text, getShift, edits, leadingTexts);
        }

        if (edits.Count == 0
            && leadingTexts.Count == 0)
        {
            return root;
        }

        return root.ReplaceSyntax(leadingTexts.Keys,
                                  (originalNode, rewrittenNode) => InsertLeadingText((InterpolatedStringExpressionSyntax)rewrittenNode, leadingTexts[(InterpolatedStringExpressionSyntax)originalNode]),
                                  edits.Keys,
                                  (originalToken, _) => ApplyEdits(originalToken, edits[originalToken]),
                                  [],
                                  static (_, rewrittenTrivia) => rewrittenTrivia);
    }

    /// <summary>
    /// Gets the number of columns the indentation phase moves the given line
    /// </summary>
    /// <param name="lineNumber">The 0-based line number</param>
    /// <param name="getColumnBeforeShift">Gets the column a literal's first token has once its line is placed, before any raw string literal's line moves</param>
    /// <returns>The number of columns; positive moves the line to the right, and zero leaves it in place</returns>
    public int GetShift(int lineNumber, Func<SyntaxToken, int> getColumnBeforeShift)
    {
        if (_movingLiterals.TryGetValue(lineNumber, out var literalIndices) == false)
        {
            return 0;
        }

        var shift = 0;

        foreach (var literalIndex in literalIndices)
        {
            shift += GetDistance(literalIndex, getColumnBeforeShift);
        }

        return shift;
    }

    /// <summary>
    /// Collects the edits that move each line of a multi-line raw string literal after the first by the number of columns
    /// <paramref name="getShift"/> gives it. Each edit belongs to the token that holds the start of the line: in its text,
    /// on a content or closing line, or in its leading trivia, on a line of an interpolation hole. A content line that
    /// starts with a hole's opening brace has no text in front of the brace, and the parser reads indentation there as
    /// text of the literal: its edit belongs to the text token that ends at the line's start, or, when the start token
    /// stands in front of the hole, it becomes a new text element of the literal
    /// </summary>
    /// <param name="root">The indented syntax root</param>
    /// <param name="node">The literal expression or interpolated string expression</param>
    /// <param name="text">The text of the indented syntax tree</param>
    /// <param name="getShift">Gets the number of columns a line moves</param>
    /// <param name="edits">Receives the edits, by the token they change</param>
    /// <param name="leadingTexts">Receives the number of spaces of the text element to insert in front of the first hole, by the literal that receives it</param>
    private static void CollectEdits(SyntaxNode root, SyntaxNode node, SourceText text, Func<int, int> getShift, Dictionary<SyntaxToken, List<(int LineStart, int OldLength, int NewLength)>> edits, Dictionary<InterpolatedStringExpressionSyntax, int> leadingTexts)
    {
        var openingLineNumber = text.Lines.GetLineFromPosition(node.SpanStart).LineNumber;
        var closingLineNumber = text.Lines.GetLineFromPosition(node.Span.End).LineNumber;

        for (var lineNumber = openingLineNumber + 1; lineNumber <= closingLineNumber; lineNumber++)
        {
            var shift = getShift(lineNumber);
            var line = text.Lines[lineNumber];

            if (shift == 0
                || line.Span.Length == 0)
            {
                continue;
            }

            var indentation = GetIndentationLength(root, text, line);
            var newSpaces = Math.Max(0, indentation + shift);

            if (newSpaces == indentation)
            {
                continue;
            }

            var token = root.FindToken(line.Start);

            if (IsHoleOpeningLine(token, line.Start, out var interpolation))
            {
                var previousToken = token.GetPreviousToken();

                if (previousToken.IsKind(SyntaxKind.InterpolatedStringTextToken) == false)
                {
                    leadingTexts[(InterpolatedStringExpressionSyntax)interpolation.Parent] = newSpaces;

                    continue;
                }

                token = previousToken;
            }

            if (edits.TryGetValue(token, out var tokenEdits) == false)
            {
                tokenEdits = [];
                edits[token] = tokenEdits;
            }

            tokenEdits.Add((line.Start, indentation, newSpaces));
        }
    }

    /// <summary>
    /// Determines whether a content line of an interpolated raw string literal starts with the opening brace of one of its
    /// holes, so no text of the literal stands in front of the brace on that line
    /// </summary>
    /// <param name="token">The token that holds the line's start</param>
    /// <param name="lineStart">The position at which the line starts</param>
    /// <param name="interpolation">The hole whose opening brace starts the line</param>
    /// <returns><see langword="true"/> if the line starts with a hole's opening brace</returns>
    private static bool IsHoleOpeningLine(SyntaxToken token, int lineStart, out InterpolationSyntax interpolation)
    {
        interpolation = token.Parent as InterpolationSyntax;

        return interpolation != null
               && interpolation.OpenBraceToken == token
               && token.SpanStart == lineStart
               && interpolation.Parent is InterpolatedStringExpressionSyntax;
    }

    /// <summary>
    /// Inserts the moved indentation of a first content line that starts with a hole as a text element in front of that
    /// hole, where the parser reads it. Indentation is not part of the literal's value, so the element's value is empty
    /// </summary>
    /// <param name="node">The interpolated string expression, with its tokens already moved</param>
    /// <param name="spaces">The number of spaces of the indentation</param>
    /// <returns>The interpolated string expression with the text element in front of its first hole</returns>
    private static InterpolatedStringExpressionSyntax InsertLeadingText(InterpolatedStringExpressionSyntax node, int spaces)
    {
        var textToken = SyntaxFactory.Token(default, SyntaxKind.InterpolatedStringTextToken, new string(' ', spaces), string.Empty, default);

        return node.WithContents(node.Contents.Insert(0, SyntaxFactory.InterpolatedStringText(textToken)));
    }

    /// <summary>
    /// Applies the indentation edits of one token. An edit at a position inside the token's text, or at its start when
    /// the token holds the text of a raw string literal, replaces the leading spaces of that line of the text. An edit in
    /// the token's leading trivia replaces the whitespace trivia that starts the line, or inserts whitespace in front of the
    /// trivia that does, or behind the trivia when the token itself starts the line; a line that starts inside a trivia,
    /// such as a later line of a <c>///</c> comment, is moved inside that trivia's text. The token keeps its kind, its
    /// value, and its annotations
    /// </summary>
    /// <param name="token">The token of the indented tree</param>
    /// <param name="tokenEdits">The edits of the token</param>
    /// <returns>The token with its lines moved</returns>
    private static SyntaxToken ApplyEdits(SyntaxToken token, List<(int LineStart, int OldLength, int NewLength)> tokenEdits)
    {
        var tokenText = token.Text;
        var leadingTrivia = token.LeadingTrivia.ToList();
        var triviaStarts = token.LeadingTrivia.Select(static trivia => trivia.FullSpan.Start).ToList();
        var textChanged = false;

        // Later positions first, so the positions and indices in front of each edit stay valid
        foreach (var edit in tokenEdits.OrderByDescending(static edit => edit.LineStart))
        {
            if (edit.LineStart > token.SpanStart
                || (edit.LineStart == token.SpanStart && HoldsRawStringText(token)))
            {
                tokenText = ReplaceIndentation(tokenText, edit.LineStart - token.SpanStart, edit);
                textChanged = true;

                continue;
            }

            var triviaIndex = triviaStarts.FindIndex(triviaStart => triviaStart >= edit.LineStart);

            if (triviaIndex < 0)
            {
                triviaIndex = triviaStarts.Count;
            }

            if (triviaIndex > 0
                && edit.LineStart < token.LeadingTrivia[triviaIndex - 1].FullSpan.End)
            {
                leadingTrivia[triviaIndex - 1] = MoveLineInsideTrivia(leadingTrivia[triviaIndex - 1], edit.LineStart - triviaStarts[triviaIndex - 1], edit);

                continue;
            }

            if (triviaIndex < leadingTrivia.Count
                && leadingTrivia[triviaIndex].IsKind(SyntaxKind.WhitespaceTrivia))
            {
                leadingTrivia.RemoveAt(triviaIndex);
            }

            if (edit.NewLength > 0)
            {
                leadingTrivia.Insert(triviaIndex, SyntaxFactory.Whitespace(new string(' ', edit.NewLength)));
            }
        }

        var movedToken = textChanged
                             ? CreateMovedTextToken(token, tokenText).WithLeadingTrivia(leadingTrivia)
                             : token.WithLeadingTrivia(leadingTrivia);

        return token.CopyAnnotationsTo(movedToken);
    }

    /// <summary>
    /// Creates the token that holds the moved text of a raw string literal token. The end token of an interpolated literal
    /// stands for its own text, so its value is the moved text. Every other token keeps the value it had: the value of a
    /// literal's content is its text without the closing line's indentation, and every line of one literal, the closing
    /// line included, moves by the same number of columns, so the value does not change
    /// </summary>
    /// <param name="token">The token of the indented tree</param>
    /// <param name="movedText">The moved text of the token</param>
    /// <returns>The token with the moved text and the original trailing trivia, without leading trivia</returns>
    private static SyntaxToken CreateMovedTextToken(SyntaxToken token, string movedText)
    {
        var valueText = token.IsKind(SyntaxKind.InterpolatedRawStringEndToken)
                            ? movedText
                            : token.ValueText;

        return SyntaxFactory.Token(default, token.Kind(), movedText, valueText, token.TrailingTrivia);
    }

    /// <summary>
    /// Moves a line that starts inside the text of a trivia, such as a later line of a <c>///</c> comment or a line of
    /// disabled text. The trivia is built again from its moved text; a trivia whose moved text does not read back as one
    /// trivia of its kind is kept as it is
    /// </summary>
    /// <param name="trivia">The trivia</param>
    /// <param name="offset">The offset of the line's start in the trivia's text</param>
    /// <param name="edit">The edit: the position the line starts at, the number of characters of indentation it replaces, and the number of spaces that replace them</param>
    /// <returns>The trivia with the line moved</returns>
    private static SyntaxTrivia MoveLineInsideTrivia(SyntaxTrivia trivia, int offset, (int LineStart, int OldLength, int NewLength) edit)
    {
        var movedText = ReplaceIndentation(trivia.ToFullString(), offset, edit);

        if (trivia.IsKind(SyntaxKind.DisabledTextTrivia))
        {
            return SyntaxFactory.DisabledText(movedText);
        }

        var parsedTrivia = SyntaxFactory.ParseLeadingTrivia(movedText);

        return parsedTrivia.Count == 1
               && parsedTrivia[0].IsKind(trivia.Kind())
                   ? parsedTrivia[0]
                   : trivia;
    }

    /// <summary>
    /// Replaces the indentation of a line inside a text by the number of spaces an edit gives it
    /// </summary>
    /// <param name="text">The text</param>
    /// <param name="offset">The offset of the line's start in the text</param>
    /// <param name="edit">The edit: the position the line starts at, the number of characters of indentation it replaces, and the number of spaces that replace them</param>
    /// <returns>The text with the line's indentation replaced</returns>
    private static string ReplaceIndentation(string text, int offset, (int LineStart, int OldLength, int NewLength) edit)
    {
        return $"{text.Substring(0, offset)}{new string(' ', edit.NewLength)}{text.Substring(offset + edit.OldLength)}";
    }

    /// <summary>
    /// Determines whether the token holds text of a raw string literal that a line can start in: the token of a plain or
    /// UTF-8 literal, a text token of an interpolated literal, or the end token of an interpolated literal, whose text
    /// starts with the line break in front of the closing marker
    /// </summary>
    /// <param name="token">The token to inspect</param>
    /// <returns><see langword="true"/> if the token holds such text</returns>
    private static bool HoldsRawStringText(SyntaxToken token)
    {
        return token.Kind() is SyntaxKind.MultiLineRawStringLiteralToken
                            or SyntaxKind.Utf8MultiLineRawStringLiteralToken
                            or SyntaxKind.InterpolatedStringTextToken
                            or SyntaxKind.InterpolatedRawStringEndToken;
    }

    /// <summary>
    /// Gets the length of the indentation that moving a line rewrites. On a line of an interpolation hole, the
    /// indentation is whitespace trivia, which holds no tab once the first phase ran; it is rewritten as a whole, so
    /// the line moves by the full shift. On a content or closing line, the indentation is the literal's text, and only
    /// its leading spaces are rewritten
    /// </summary>
    /// <param name="root">The indented syntax root</param>
    /// <param name="text">The text of the indented syntax tree</param>
    /// <param name="line">The line to examine</param>
    /// <returns>The number of characters the indentation takes, each of them one column</returns>
    private static int GetIndentationLength(SyntaxNode root, SourceText text, TextLine line)
    {
        var trivia = root.FindTrivia(line.Start);

        if (trivia.IsKind(SyntaxKind.WhitespaceTrivia)
            && trivia.SpanStart == line.Start)
        {
            return Math.Min(trivia.Span.End, line.End) - line.Start;
        }

        return GetLeadingSpaceCount(text, line);
    }

    /// <summary>
    /// Determines whether the node lies inside an interpolation hole of a multi-line interpolated raw string
    /// </summary>
    /// <param name="node">The node to inspect</param>
    /// <returns><see langword="true"/> if a multi-line interpolated raw string encloses the node</returns>
    private static bool IsNestedInRawStringHole(SyntaxNode node)
    {
        return node.Ancestors()
                   .Any(IsMultiLineRawStringLiteral);
    }

    /// <summary>
    /// Gets the offset of the first opening quote within the literal, so an interpolated literal is measured under its
    /// quotes rather than under its <c>$</c> characters
    /// </summary>
    /// <param name="node">The literal expression or interpolated string expression</param>
    /// <returns>The offset of the first opening quote</returns>
    private static int GetQuoteOffset(SyntaxNode node)
    {
        return node is InterpolatedStringExpressionSyntax interpolated
                   ? RawStringLiteralUtilities.GetQuoteOffset(interpolated.StringStartToken.Text)
                   : 0;
    }

    /// <summary>
    /// Determines whether a line that starts at the given position starts inside the text of a literal other than a
    /// raw string, or inside a <c>/* */</c> or <c>/** */</c> comment, that began on an earlier line. Moving the start
    /// of such a line would change the literal's value or the comment's text. The following lines of a <c>///</c>
    /// comment start with a marker that is laid out like the first one, so they move
    /// </summary>
    /// <param name="root">The syntax root</param>
    /// <param name="lineStart">The position at which the line starts</param>
    /// <returns><see langword="true"/> if the line starts inside such text</returns>
    private static bool StartsInsideNestedText(SyntaxNode root, int lineStart)
    {
        var trivia = root.FindTrivia(lineStart);

        if (trivia.Span.Start < lineStart
            && trivia.Span.End > lineStart
            && (trivia.IsKind(SyntaxKind.MultiLineCommentTrivia)
                || trivia.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia)))
        {
            return true;
        }

        var token = root.FindToken(lineStart);

        if (token.SpanStart > lineStart)
        {
            return false;
        }

        if (token.SpanStart < lineStart)
        {
            return IsNonRawStringText(token);
        }

        var previousToken = token.GetPreviousToken();

        return token.LeadingTrivia.Count == 0
               && previousToken.Span.End == lineStart
               && IsNonRawStringText(previousToken);
    }

    /// <summary>
    /// Determines whether the token holds the text of a string literal other than a raw string: a regular or verbatim
    /// string, or the text of an interpolated string that is not raw
    /// </summary>
    /// <param name="token">The token to inspect</param>
    /// <returns><see langword="true"/> if the token holds such text</returns>
    private static bool IsNonRawStringText(SyntaxToken token)
    {
        if (token.IsKind(SyntaxKind.StringLiteralToken)
            || token.IsKind(SyntaxKind.Utf8StringLiteralToken))
        {
            return true;
        }

        return token.IsKind(SyntaxKind.InterpolatedStringTextToken)
               && token.Parent?.Parent is InterpolatedStringExpressionSyntax interpolated
               && interpolated.StringStartToken.IsKind(SyntaxKind.InterpolatedMultiLineRawStringStartToken) == false
               && interpolated.StringStartToken.IsKind(SyntaxKind.InterpolatedSingleLineRawStringStartToken) == false;
    }

    /// <summary>
    /// Gets the distance from a literal's closing marker to its opening quotes, both measured after the literals
    /// before and around it moved them. The literal moves its lines by that distance
    /// </summary>
    /// <param name="literalIndex">The index of the literal</param>
    /// <param name="getColumnBeforeShift">Gets the column a literal's first token has once its line is placed, before any raw string literal's line moves</param>
    /// <returns>The distance in columns</returns>
    private int GetDistance(int literalIndex, Func<SyntaxToken, int> getColumnBeforeShift)
    {
        var literal = _literals[literalIndex];
        var openingColumn = getColumnBeforeShift(literal.Node.GetFirstToken())
                            + literal.QuoteOffset
                            + GetShift(literal.OpeningLine, getColumnBeforeShift);
        var closingColumn = literal.ClosingColumn;

        // The closing line is moved by the literal itself and by the literals around it, which come first
        foreach (var outerIndex in _movingLiterals[literal.ClosingLine])
        {
            if (outerIndex == literalIndex)
            {
                break;
            }

            closingColumn += GetDistance(outerIndex, getColumnBeforeShift);
        }

        return openingColumn - closingColumn;
    }

    #endregion // Methods
}