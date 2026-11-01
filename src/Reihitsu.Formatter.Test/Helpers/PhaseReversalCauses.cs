using Reihitsu.Formatter.Pipeline.LineBreaks;

namespace Reihitsu.Formatter.Test.Helpers;

/// <summary>
/// The known causes of a phase reversal
/// </summary>
public static class PhaseReversalCauses
{
    #region Fields

    /// <summary>
    /// The line-break phase strips the indentation of a line, and the indentation phase writes it back. Run alone, the
    /// line-break phase changes the final output
    /// </summary>
    public static readonly PhaseReversalCause LineStartIndentation = new(nameof(LineBreakPhase), "The line-break phase strips the indentation of a line; the indentation phase writes it back.");

    #endregion // Fields
}