using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Reihitsu.Core;
using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Pipeline.BlankLines.Utilities;

namespace Reihitsu.Formatter.Pipeline.BlankLines.Rewriter;

/// <summary>
/// Syntax rewriter that collapses excessive consecutive blank lines
/// to a single blank line
/// </summary>
internal sealed class BlankLineCollapser : CSharpSyntaxRewriter
{
    #region Fields

    /// <summary>
    /// Formatting context of the current blank-line subphase
    /// </summary>
    private readonly FormattingContext _context;

    /// <summary>
    /// Cancellation token of the current blank-line subphase
    /// </summary>
    private readonly CancellationToken _cancellationToken;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="context">The formatting context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public BlankLineCollapser(FormattingContext context, CancellationToken cancellationToken)
    {
        _context = context;
        _cancellationToken = cancellationToken;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Collapses sequences of two or more consecutive blank lines in the trivia list to a single blank line
    /// </summary>
    /// <param name="trivia">The trivia list to process</param>
    /// <param name="firstLineHasContent">Whether the first trivia line continues content from the previous token</param>
    /// <returns>The trivia list with excessive blank lines collapsed</returns>
    private static SyntaxTriviaList CollapseBlankLinesInTrivia(SyntaxTriviaList trivia, bool firstLineHasContent)
    {
        // Parse trivia into lines (each line ends with EndOfLine, except possibly the last)
        var lines = new List<List<SyntaxTrivia>>();
        var currentLine = new List<SyntaxTrivia>();

        foreach (var triviaItem in trivia)
        {
            currentLine.Add(triviaItem);

            if (triviaItem.IsKind(SyntaxKind.EndOfLineTrivia) || BlankLineTriviaUtilities.EndsWithLineBreak(triviaItem))
            {
                lines.Add(currentLine);
                currentLine = [];
            }
        }

        if (currentLine.Count > 0)
        {
            lines.Add(currentLine);
        }

        // Process: collapse excessive blank-line runs to a single blank line.
        var result = new List<SyntaxTrivia>();
        var blankLineBuffer = new List<List<SyntaxTrivia>>();

        for (var lineIndex = 0; lineIndex < lines.Count; lineIndex++)
        {
            var line = lines[lineIndex];
            var isBlankLine = IsBlankLine(line)
                              && (lineIndex > 0 || firstLineHasContent == false);

            if (isBlankLine)
            {
                blankLineBuffer.Add(line);
            }
            else
            {
                FlushBlankLines(blankLineBuffer, result);
                blankLineBuffer.Clear();
                result.AddRange(line);
            }
        }

        FlushBlankLines(blankLineBuffer, result);

        return SyntaxFactory.TriviaList(result);
    }

    /// <summary>
    /// Flushes buffered blank lines into the result list, collapsing two or more
    /// source blank lines to a single preserved blank line
    /// </summary>
    /// <param name="buffer">The buffered blank lines</param>
    /// <param name="result">The result trivia list to append to</param>
    private static void FlushBlankLines(List<List<SyntaxTrivia>> buffer, List<SyntaxTrivia> result)
    {
        if (buffer.Count >= 2)
        {
            // Collapse to 1 blank line: keep the first blank line only.
            result.AddRange(buffer[0]);
        }
        else
        {
            foreach (var line in buffer)
            {
                result.AddRange(line);
            }
        }
    }

    /// <summary>
    /// Determines whether the specified list of trivia constitutes a blank line
    /// </summary>
    /// <param name="line">The list of trivia representing a single line</param>
    /// <returns><see langword="true"/> if the line contains only whitespace and an end-of-line</returns>
    private static bool IsBlankLine(List<SyntaxTrivia> line)
    {
        return TokenGapAnalysis.IsBlankLine(line);
    }

    #endregion // Methods

    #region CSharpSyntaxRewriter

    /// <inheritdoc/>
    /// <remarks>
    /// Whether the first trivia line continues the preceding token's line is read from the facts resolved through
    /// <see cref="PrecedingTokenFacts.Resolve"/>. The first token of a formatting root that an earlier subphase detached
    /// has no previous token in its tree, and reading the live previous token there would treat the line break that
    /// ends the preceding token's line as a blank line and collapse a blank line the boundary rewriters just inserted
    /// </remarks>
    public override SyntaxToken VisitToken(SyntaxToken token)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        token = base.VisitToken(token);

        var leading = token.LeadingTrivia;

        if (leading.Count < 2)
        {
            return token;
        }

        var previousTokenFacts = PrecedingTokenFacts.Resolve(token.GetPreviousToken(), _context);
        var firstLineHasContent = previousTokenFacts.Exists
                                  && TokenGapAnalysis.OfTriviaRange(previousTokenFacts.TrailingTrivia,
                                                                    0,
                                                                    previousTokenFacts.TrailingTrivia.Count).HasTerminalLineBreak == false;
        var collapsed = CollapseBlankLinesInTrivia(leading, firstLineHasContent);

        if (collapsed.Count != leading.Count)
        {
            return token.WithLeadingTrivia(collapsed);
        }

        return token;
    }

    #endregion // CSharpSyntaxRewriter
}