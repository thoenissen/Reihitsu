using System.Collections.Generic;

using Microsoft.CodeAnalysis.CSharp;

using Reihitsu.Formatter.Pipeline;

namespace Reihitsu.Formatter.Test.Helpers;

/// <summary>
/// The check's state for one test execution: the phases it runs alone and the fixtures it ran on
/// </summary>
internal sealed class PhaseStabilitySession
{
    #region Fields

    /// <summary>
    /// The fixtures the check ran on, in the order it ran on them
    /// </summary>
    private readonly List<(string FinalOutput, string EndOfLine, CSharpParseOptions ParseOptions)> _checkedFixtures = [];

    /// <summary>
    /// Creates the phases the check runs alone
    /// </summary>
    private Func<IReadOnlyList<IFormattingPhase>> _createPhases;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="createPhases">Creates the phases to run alone</param>
    public PhaseStabilitySession(Func<IReadOnlyList<IFormattingPhase>> createPhases)
    {
        _createPhases = createPhases;
    }

    #endregion // Constructor

    #region Properties

    /// <summary>
    /// The fixtures the check ran on, in the order it ran on them: the final output of each, its end-of-line sequence,
    /// and the parse options it was parsed with
    /// </summary>
    public IReadOnlyList<(string FinalOutput, string EndOfLine, CSharpParseOptions ParseOptions)> CheckedFixtures => _checkedFixtures;

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Replaces the phases the check runs alone, before the check first runs
    /// </summary>
    /// <param name="createPhases">Creates the phases to run alone</param>
    public void Override(Func<IReadOnlyList<IFormattingPhase>> createPhases)
    {
        _createPhases = createPhases;
    }

    /// <summary>
    /// Runs every phase alone on the final output of a fixture and returns one failure per phase that changes it
    /// </summary>
    /// <param name="finalOutput">The final output of the fixture</param>
    /// <param name="endOfLine">The end-of-line sequence the output was formatted with</param>
    /// <param name="parseOptions">The parse options the fixture was formatted with, or <see langword="null"/> for the defaults</param>
    /// <returns>The failures; empty when no phase changes the output</returns>
    public IReadOnlyList<string> Check(string finalOutput, string endOfLine, CSharpParseOptions parseOptions)
    {
        var endingName = endOfLine == "\n" ? "LF" : "CRLF";
        var changes = PhaseStabilityCheck.FindChangingPhases(finalOutput, endOfLine, parseOptions, _createPhases());

        _checkedFixtures.Add((finalOutput, endOfLine, parseOptions));

        return changes.Select(change => $"{change.Phase} changes the final output under {endingName} line endings when it runs alone (FMT-PIPE-027). {change.Difference}")
                      .ToList();
    }

    #endregion // Methods
}