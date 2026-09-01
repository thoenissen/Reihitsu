using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Reihitsu.Core;
using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Pipeline.Indentation.Contributors;

namespace Reihitsu.Formatter.Pipeline.Indentation.Utilities;

/// <summary>
/// Computes the indentation layout model for a syntax tree.
/// Applies block indentation, then repeats an alignment sweep until the columns settle, then
/// aligns comments to the code they precede
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
    /// <returns>A layout model mapping line numbers to desired indentation</returns>
    public static LayoutModel Compute(SyntaxNode root, FormattingContext context)
    {
        var model = new LayoutModel();
        var baseColumn = context.BaseIndentLevel * FormattingContext.IndentSize;

        // Pass 1: Block indentation — recursive descent over the tree
        ComputeBlockIndentation(root, 0, model, baseColumn);

        // Pass 2: Alignment contributors — override block indentation for specific constructs
        var contributors = CreateContributors();

        // The sweep repeats until no column changes, because a single ordered sweep cannot satisfy
        // the dependencies in both directions. A contributor resolves its anchor through
        // GetAdjustedColumn, which is only correct once that anchor's line is final, and the
        // dependencies run both ways across the node order: a chain's anchor sits on a line owned by
        // a construct nested inside the chain root (a multi-line initializer closed by "2 }"), which
        // the pre-order walk reaches after the chain, while an argument list or lambda nested under a
        // continuation line depends on the chain's own column and is reached before it is final.
        // Sweeping once leaves whichever side runs last measuring against a stale column
        for (var pass = 0; pass < MaxAlignmentPasses; pass++)
        {
            var columns = model.CaptureColumns();

            foreach (var node in root.DescendantNodesAndSelf())
            {
                foreach (var contributor in contributors)
                {
                    contributor.Contribute(node, model, context);
                }
            }

            if (model.MatchesColumns(columns))
            {
                break;
            }
        }

        // Pass 3: Comment alignment — align comments to the code they precede
        var commentContributor = new CommentIndentationContributor();

        commentContributor.Contribute(root, model, context);

        return model;
    }

    #endregion // Methods

    #region Private methods

    /// <summary>
    /// Determines whether a token is the first token on its line. The rule itself lives in
    /// <see cref="SyntaxTokenPositionUtilities.IsFirstOnLine"/>, shared with the RH5204 analyzer so both engines
    /// agree on which token owns a line; this forwarder keeps the formatter's own call sites unchanged
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
    /// Gets the column of a token as it will be after indentation is applied.
    /// For tokens that are first on their line, this returns the layout model's column.
    /// For tokens that are not first on their line, this computes the offset from the
    /// first token's adjusted position
    /// </summary>
    /// <param name="token">The token to get the adjusted column for</param>
    /// <param name="model">The layout model from Pass 1</param>
    /// <returns>The adjusted column position</returns>
    internal static int GetAdjustedColumn(SyntaxToken token, LayoutModel model)
    {
        var originalColumn = GetColumn(token);
        var line = GetLine(token);

        if (model.TryGetLayout(line, out var layout) == false)
        {
            return originalColumn;
        }

        var firstTokenOnLine = FindFirstTokenOnLine(token);
        var originalLineStart = GetColumn(firstTokenOnLine);

        return layout.Column + (originalColumn - originalLineStart);
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
    /// Pass 1: Walks the syntax tree top-down and sets block indentation
    /// for all first-on-line tokens based on nesting depth
    /// </summary>
    /// <param name="node">The current syntax node</param>
    /// <param name="indentLevel">The current block indentation level</param>
    /// <param name="model">The layout model to write to</param>
    /// <param name="baseColumn">The base column offset</param>
    private static void ComputeBlockIndentation(SyntaxNode node, int indentLevel, LayoutModel model, int baseColumn)
    {
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
                ComputeBlockIndentation(child.AsNode(), childIndent, model, baseColumn);
            }
        }
    }

    /// <summary>
    /// Applies indentation entries for region-related directive trivia. A directive inside a branch the
    /// compiler skipped is left where the author wrote it: the code around it is untouched disabled
    /// text, so re-indenting the directive alone would half-format a region nobody compiles (issue #434)
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
    /// Applies block indentation for first-on-line tokens
    /// </summary>
    /// <param name="token">The token to evaluate</param>
    /// <param name="childIndent">The computed child indentation level</param>
    /// <param name="model">The layout model to update</param>
    /// <param name="baseColumn">The base indentation column</param>
    private static void SetTokenIndentation(SyntaxToken token, int childIndent, LayoutModel model, int baseColumn)
    {
        if (IsFirstOnLine(token) == false)
        {
            return;
        }

        model.Set(GetLine(token), new TokenLayout(childIndent * FormattingContext.IndentSize + baseColumn, "Block"));
    }

    /// <summary>
    /// Finds the first token on the same line as the given token
    /// </summary>
    /// <param name="token">The token to find the first token on the same line for</param>
    /// <returns>The first token on the same line</returns>
    private static SyntaxToken FindFirstTokenOnLine(SyntaxToken token)
    {
        var targetLine = GetLine(token);
        var current = token;

        while (true)
        {
            var prev = current.GetPreviousToken();

            if (prev == default || prev.IsKind(SyntaxKind.None) || GetLine(prev) != targetLine)
            {
                return current;
            }

            current = prev;
        }
    }

    #endregion // Private methods
}