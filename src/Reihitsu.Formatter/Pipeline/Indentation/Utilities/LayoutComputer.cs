using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Core;
using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Pipeline.Indentation.Contributors;

namespace Reihitsu.Formatter.Pipeline.Indentation.Utilities;

/// <summary>
/// Computes the indentation layout model for a syntax tree.
/// Applies block indentation, then repeats an alignment sweep until the columns settle, then
/// aligns comments to the code they precede. A line that a token starts inside an interpolation hole
/// of a multi-line interpolated raw string keeps its column unless a construct inside the hole places it.
/// Every column is measured where the indentation phase puts the token: behind a comment that leads its line,
/// and after the phase moved the lines of multi-line raw string literals
/// </summary>
internal static class LayoutComputer
{
    #region Constants

    /// <summary>
    /// Upper bound on the alignment sweeps in pass 2. The corrective sweeps settle by the second —
    /// the first resolves every anchor whose own line is already final, the second the anchors that
    /// depended on those — and a further sweep then observes no change and stops the loop, so the
    /// shapes this bound exists for run three times. Measured maxima: repository sources two sweeps,
    /// the repository's own test fixtures three, generated adversarial nesting four. The bound only
    /// guards against a cycle between two contributors that would otherwise spin; reaching it leaves
    /// the last computed state, which is still deterministic
    /// </summary>
    private const int MaxAlignmentPasses = 8;

    #endregion // Constants

    #region Methods

    /// <summary>
    /// Computes the layout model for all first-on-line tokens in the syntax tree
    /// </summary>
    /// <param name="root">The root syntax node</param>
    /// <param name="context">The formatting context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A layout model mapping line numbers to desired indentation</returns>
    public static LayoutModel Compute(SyntaxNode root, FormattingContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var model = new LayoutModel();
        var baseColumn = context.BaseIndentLevel * FormattingContext.IndentSize;

        model.UseRawStringLineShifts(new RawStringLineShifts(root, root.SyntaxTree.GetText(cancellationToken), cancellationToken),
                                     token => GetOpeningQuotesColumnBeforeRawStringShift(token, model));

        RegisterRawStringHoleLines(root, model);

        // Pass 1: Block indentation — recursive descent over the tree
        ComputeBlockIndentation(root, 0, model, baseColumn, cancellationToken);

        // A node formatted inside its document keeps its first token in its document column (FMT-PIPE-017)
        ShiftToRootFirstTokenColumn(root, model, context);

        // Pass 2: Alignment contributors — override block indentation for specific constructs
        var contributors = CreateContributors();

        RunAlignmentSweeps(root, model, contributors, context, cancellationToken);

        // Pass 3: Comment alignment — align comments to the code they precede
        var commentContributor = new CommentIndentationContributor(cancellationToken);

        commentContributor.Contribute(root, model, context);

        // The hole lines no construct inside their hole placed keep their whitespace and move only with the literal
        model.ReleaseUnplacedRawStringHoleLines();

        return model;
    }

    /// <summary>
    /// Pass 1: Walks the syntax tree top-down and sets block indentation
    /// for all first-on-line tokens based on nesting depth
    /// </summary>
    /// <param name="node">The current syntax node</param>
    /// <param name="indentLevel">The current block indentation level</param>
    /// <param name="model">The layout model to write to</param>
    /// <param name="baseColumn">The base column offset</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <remarks>
    /// Internal, rather than private, so a test can observe this pass's own cancellation check directly —
    /// calling it through <see cref="Compute"/> would always short-circuit on that method's entry check first
    /// </remarks>
    internal static void ComputeBlockIndentation(SyntaxNode node, int indentLevel, LayoutModel model, int baseColumn, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        foreach (var child in node.ChildNodesAndTokens())
        {
            var childIndent = SyntaxIndentationUtilities.GetChildIndentLevel(node, child, indentLevel);

            if (child.IsToken)
            {
                var token = child.AsToken();

                SetDirectiveIndentation(token, node, indentLevel, model, baseColumn);
                SetTokenIndentation(token, childIndent, model, baseColumn);
            }
            else
            {
                ComputeBlockIndentation(child.AsNode(), childIndent, model, baseColumn, cancellationToken);
            }
        }
    }

    /// <summary>
    /// Pass 2: Repeats the alignment contributor sweep until the columns settle, because a single
    /// ordered sweep cannot satisfy the dependencies in both directions. A contributor resolves its
    /// anchor through <see cref="GetAdjustedColumn"/>, which is only correct once that anchor's line is
    /// final, and the dependencies run both ways across the node order: a chain's anchor sits on a line
    /// owned by a construct nested inside the chain root (a multi-line initializer closed by "2 }"),
    /// which the pre-order walk reaches after the chain, while an argument list or lambda nested under a
    /// continuation line depends on the chain's own column and is reached before it is final. Sweeping
    /// once leaves whichever side runs last measuring against a stale column. The model is told which
    /// node's contributors are writing, so a line of an interpolation hole takes only the columns of
    /// constructs inside that hole
    /// </summary>
    /// <param name="root">The root syntax node</param>
    /// <param name="model">The layout model to update</param>
    /// <param name="contributors">The alignment contributors to run each sweep, in priority order</param>
    /// <param name="context">The formatting context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <remarks>
    /// Internal, rather than private, so a test can observe this pass's own cancellation checks directly —
    /// calling it through <see cref="Compute"/> would always short-circuit on that method's entry check first
    /// </remarks>
    internal static void RunAlignmentSweeps(SyntaxNode root, LayoutModel model, ILayoutContributor[] contributors, FormattingContext context, CancellationToken cancellationToken)
    {
        for (var pass = 0; pass < MaxAlignmentPasses; pass++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var columns = model.CaptureColumns();

            foreach (var node in root.DescendantNodesAndSelf())
            {
                cancellationToken.ThrowIfCancellationRequested();

                model.Contributor = node;

                foreach (var contributor in contributors)
                {
                    model.AlignmentContributor = contributor;

                    contributor.Contribute(node, model, context);
                }
            }

            model.Contributor = null;
            model.AlignmentContributor = null;

            if (model.MatchesColumns(columns))
            {
                break;
            }
        }
    }

    #endregion // Methods

    #region Private methods

    /// <summary>
    /// Determines whether a token is the first token on its line. The rule itself lives in
    /// <see cref="SyntaxTokenPositionUtilities.IsFirstOnLine"/>, shared with the analyzers so every engine agrees on which
    /// token owns a line (SHARED-BLANK-008); this forwarder keeps the formatter's own call sites unchanged
    /// </summary>
    /// <param name="token">The token to check</param>
    /// <returns><see langword="true"/> if the token is first on its line; otherwise, <see langword="false"/></returns>
    internal static bool IsFirstOnLine(SyntaxToken token)
    {
        return SyntaxTokenPositionUtilities.IsFirstOnLine(token);
    }

    /// <summary>
    /// Gets the 0-based line number of a token
    /// </summary>
    /// <param name="token">The token</param>
    /// <returns>The line number</returns>
    internal static int GetLine(SyntaxToken token)
    {
        return SyntaxTokenPositionUtilities.GetLine(token);
    }

    /// <summary>
    /// Gets the 0-based column of a token
    /// </summary>
    /// <param name="token">The token</param>
    /// <returns>The column</returns>
    internal static int GetColumn(SyntaxToken token)
    {
        return SyntaxTokenPositionUtilities.GetColumn(token);
    }

    /// <summary>
    /// Gets the column of a token in the output of the indentation phase: after its line is placed, and after the
    /// phase moved the lines of multi-line raw string literals, so a token inside such a literal's interpolation hole,
    /// or behind its closing marker, is measured where the move puts it
    /// </summary>
    /// <param name="token">The token to get the adjusted column for</param>
    /// <param name="model">The layout model from Pass 1</param>
    /// <returns>The adjusted column position</returns>
    internal static int GetAdjustedColumn(SyntaxToken token, LayoutModel model)
    {
        return GetColumnBeforeRawStringShift(token, model) + model.GetRawStringShift(GetLine(token));
    }

    /// <summary>
    /// Gets the column of a token once its line is placed, before any line of a multi-line raw string literal moves.
    /// The indentation phase writes a line's column in front of the text that starts the line — the line's first
    /// token, or a comment that stands in front of that token on its line — and the rest of the line keeps its gaps,
    /// so a token is measured at the line's column plus its distance from the start of that text (FMT-INDENT-030).
    /// A line that starts inside text that began on an earlier line, such as the last line of a multi-line comment or
    /// the closing line of a raw string literal, is not placed, and its tokens keep their columns; so does a line of
    /// an interpolation hole that no construct inside the hole placed
    /// </summary>
    /// <param name="token">The token to get the column for</param>
    /// <param name="model">The layout model</param>
    /// <returns>The column after its line is placed</returns>
    internal static int GetColumnBeforeRawStringShift(SyntaxToken token, LayoutModel model)
    {
        var originalColumn = GetColumn(token);

        if (model.TryGetPlacedColumnBeforeRawStringShift(GetLine(token), out var lineColumn) == false
            || TryGetLineTextColumn(token, out var lineTextColumn) == false)
        {
            return originalColumn;
        }

        return lineColumn + (originalColumn - lineTextColumn);
    }

    /// <summary>
    /// Sets layout for a token if it is first on its line
    /// </summary>
    /// <param name="token">The token</param>
    /// <param name="column">The desired column</param>
    /// <param name="source">Debug label for the contributor</param>
    /// <param name="model">The layout model</param>
    internal static void SetIfFirstOnLine(SyntaxToken token, int column, string source, LayoutModel model)
    {
        if (IsFirstOnLine(token))
        {
            model.Set(GetLine(token), new TokenLayout(column, source));
        }
    }

    /// <summary>
    /// Creates the ordered list of alignment contributors.
    /// Later contributors override earlier ones for the same line
    /// </summary>
    /// <returns>An array of layout contributors in priority order</returns>
    private static ILayoutContributor[] CreateContributors()
    {
        return [
                   new ArgumentAlignmentContributor(),
                   new MethodChainAlignmentContributor(),
                   new ObjectInitializerContributor(),
                   new CollectionExpressionContributor(),
                   new ListPatternContributor(),
                   new BinaryExpressionContributor(),
                   new ConditionalExpressionContributor(),
                   new SwitchExpressionContributor(),
                   new CaseWhenClauseContributor(),
                   new ParenthesizedPatternContributor(),
                   new RecursivePatternContributor(),
                   new ConstructorInitializerContributor(),
                   new GenericConstraintContributor(),
                   new BaseTypeListContributor(),
                   new AngleBracketListAlignmentContributor(),
                   new AnonymousObjectContributor(),
                   new LambdaAlignmentContributor()
               ];
    }

    /// <summary>
    /// Gets the column the first token of a multi-line raw string literal has once its line is placed, as the move of the
    /// literal's lines measures it. That is <see cref="GetColumnBeforeRawStringShift"/>, except on a placed line that
    /// starts with the byte order mark of the text: the rewriter keeps the mark in front of the line's column, and the
    /// literal's lines, which hold their indentation in the literal's text, count it as a column (FMT-RAW-005)
    /// </summary>
    /// <param name="token">The first token of the literal</param>
    /// <param name="model">The layout model</param>
    /// <returns>The column the move of the literal's lines measures the token at</returns>
    private static int GetOpeningQuotesColumnBeforeRawStringShift(SyntaxToken token, LayoutModel model)
    {
        var column = GetColumnBeforeRawStringShift(token, model);
        var lineStart = token.SpanStart - GetColumn(token);

        if (lineStart == 0
            && token.SyntaxTree?.GetText() is { Length: > 0 } text
            && text[0] == '\uFEFF'
            && model.TryGetPlacedColumnBeforeRawStringShift(GetLine(token), out _)
            && TryGetLineTextColumn(token, out _))
        {
            column++;
        }

        return column;
    }

    /// <summary>
    /// Shifts the block indentation of every line by the root's offset when the caller supplies the column of the root's
    /// first token in its document (FMT-PIPE-017): that column less the column block indentation gives the token. The
    /// first token then keeps its column, and a line placed by its depth keeps its place relative to it. The alignments
    /// run after the shift, so a line aligned to an anchor follows the anchor where it stands, also on a line this phase
    /// does not place. When the first token's line is not placed, because the token stands behind the last line of a
    /// multi-line comment, the token keeps its column and the offset is zero
    /// </summary>
    /// <param name="root">The root syntax node</param>
    /// <param name="model">The layout model after block indentation</param>
    /// <param name="context">The formatting context</param>
    private static void ShiftToRootFirstTokenColumn(SyntaxNode root, LayoutModel model, FormattingContext context)
    {
        if (context.RootFirstTokenColumn is not { } rootFirstTokenColumn)
        {
            return;
        }

        var firstToken = root.GetFirstToken();

        if (firstToken.IsKind(SyntaxKind.None)
            || model.TryGetPlacedColumnBeforeRawStringShift(GetLine(firstToken), out _) == false
            || TryGetLineTextColumn(firstToken, out _) == false)
        {
            return;
        }

        model.ShiftRange(0, int.MaxValue, rootFirstTokenColumn - GetColumnBeforeRawStringShift(firstToken, model), "RootFirstTokenColumn");
    }

    /// <summary>
    /// Registers every line that a token starts inside an interpolation hole of a multi-line interpolated raw
    /// string with the innermost such hole. The column the indentation requirements would give such a line from the
    /// scope depth, or from a construct outside the hole, does not depend on where the literal sits, while the
    /// move of the literal's lines carries the line with the literal; giving it both would move it twice (FMT-RAW-007).
    /// The line keeps its whitespace, which holds no tab once the first phase ran, so the column registered for it is
    /// the column at which its text starts
    /// </summary>
    /// <param name="root">The root syntax node</param>
    /// <param name="model">The layout model to register the lines with</param>
    private static void RegisterRawStringHoleLines(SyntaxNode root, LayoutModel model)
    {
        foreach (var interpolatedString in root.DescendantNodesAndSelf().OfType<InterpolatedStringExpressionSyntax>())
        {
            if (interpolatedString.StringStartToken.IsKind(SyntaxKind.InterpolatedMultiLineRawStringStartToken) == false)
            {
                continue;
            }

            foreach (var hole in interpolatedString.Contents.OfType<InterpolationSyntax>())
            {
                foreach (var token in hole.DescendantTokens().Where(IsFirstOnLine))
                {
                    var column = TryGetLineTextColumn(token, out var lineTextColumn)
                                     ? lineTextColumn
                                     : GetColumn(token);

                    model.RegisterRawStringHoleLine(GetLine(token), hole, column);
                }
            }
        }
    }

    /// <summary>
    /// Applies indentation entries for region-related directive trivia. A directive inside a branch the
    /// compiler skipped is left where the author wrote it: the code around it is untouched disabled
    /// text, so re-indenting the directive alone would half-format a region nobody compiles
    /// </summary>
    /// <param name="token">The token whose leading trivia is inspected</param>
    /// <param name="parent">The syntax node that owns the token</param>
    /// <param name="indentLevel">The current indentation level</param>
    /// <param name="model">The layout model to update</param>
    /// <param name="baseColumn">The base indentation column</param>
    private static void SetDirectiveIndentation(SyntaxToken token, SyntaxNode parent, int indentLevel, LayoutModel model, int baseColumn)
    {
        foreach (var directiveTrivia in token.LeadingTrivia.Where(static trivia => SyntaxTriviaUtilities.IsRegionDirective(trivia)
                                                                                   && SyntaxTriviaUtilities.IsInactiveDirective(trivia) == false))
        {
            var directiveIndent = SyntaxIndentationUtilities.GetTriviaIndentLevel(parent, directiveTrivia, indentLevel);

            var directiveLine = directiveTrivia.GetLocation().GetLineSpan().StartLinePosition.Line;

            model.Set(directiveLine, new TokenLayout(directiveIndent * FormattingContext.IndentSize + baseColumn, "Directive"));
        }
    }

    /// <summary>
    /// Applies block indentation for first-on-line tokens, and for the line the end of the file stands on when no token
    /// with text stands on that line. The end of the file has no text and starts no line (SHARED-BLANK-008), but a comment
    /// or directive in front of it on its line, and the comments above it, take the column of the file's top level
    /// </summary>
    /// <param name="token">The token to evaluate</param>
    /// <param name="childIndent">The computed child indentation level</param>
    /// <param name="model">The layout model to update</param>
    /// <param name="baseColumn">The base indentation column</param>
    private static void SetTokenIndentation(SyntaxToken token, int childIndent, LayoutModel model, int baseColumn)
    {
        if (IsFirstOnLine(token) == false
            && IsAloneOnEndOfFileLine(token) == false)
        {
            return;
        }

        model.Set(GetLine(token), new TokenLayout(childIndent * FormattingContext.IndentSize + baseColumn, "Block"));
    }

    /// <summary>
    /// Determines whether a token is the end of the file and no token with text stands on its line
    /// </summary>
    /// <param name="token">The token to check</param>
    /// <returns><see langword="true"/> if the token is the end of the file alone on its line; otherwise, <see langword="false"/></returns>
    private static bool IsAloneOnEndOfFileLine(SyntaxToken token)
    {
        if (token.IsKind(SyntaxKind.EndOfFileToken) == false)
        {
            return false;
        }

        var previousToken = token.GetPreviousToken();

        return previousToken.IsKind(SyntaxKind.None)
               || SyntaxTriviaUtilities.AreSeparatedByEndOfLine(previousToken, token);
    }

    /// <summary>
    /// Gets the column at which the text of a token's line starts: the comment or directive that starts the line, or the
    /// token that starts it (<see cref="LineStartIndex"/>). The indentation phase writes the line's column there. The text
    /// does not start on the line when the line starts inside the previous token, inside its trailing trivia, or inside a
    /// trivia that began on an earlier line, such as a multi-line comment; the phase then leaves the line's start as it is
    /// </summary>
    /// <param name="token">A token on the line</param>
    /// <param name="column">The column at which the line's text starts</param>
    /// <returns><see langword="true"/> if the line's text starts on the line; otherwise, <see langword="false"/></returns>
    private static bool TryGetLineTextColumn(SyntaxToken token, out int column)
    {
        return LineStartIndex.For(token.SyntaxTree).TryGetLineStartColumn(GetLine(token), out column);
    }

    #endregion // Private methods
}