using Reihitsu.Formatter.Pipeline.BlankLines;
using Reihitsu.Formatter.Pipeline.LineBreaks;
using Reihitsu.Formatter.Pipeline.UsingDirectives;

namespace Reihitsu.Formatter.Test.Helpers;

/// <summary>
/// The known causes of a phase reversal
/// </summary>
public static class PhaseReversalCauses
{
    #region Fields

    /// <summary>
    /// The using phase removes the blank line above a comment that leads a using directive of the same group, and the
    /// blank-line phase inserts it again. Run alone, the using phase changes the final output
    /// </summary>
    public static readonly PhaseReversalCause BlankLineAboveCommentLedUsingDirective = new(nameof(UsingDirectiveOrderingPhase), "The using phase removes the blank line above a comment that leads a using directive of the same group; the blank-line phase inserts it again.");

    /// <summary>
    /// The blank-line phase inserts a blank line next to <c>{</c> or <c>}</c>, and the line-break phase removes it again.
    /// Run alone, the blank-line phase changes the final output
    /// </summary>
    public static readonly PhaseReversalCause BlankLineNextToBrace = new(nameof(BlankLinePhase), "The blank-line phase inserts a blank line next to '{' or '}'; the line-break phase removes it again.");

    /// <summary>
    /// The line-break phase strips the indentation of a line, and the indentation phase writes it back. Run alone, the
    /// line-break phase changes the final output
    /// </summary>
    public static readonly PhaseReversalCause LineStartIndentation = new(nameof(LineBreakPhase), "The line-break phase strips the indentation of a line; the indentation phase writes it back.");

    #endregion // Fields
}