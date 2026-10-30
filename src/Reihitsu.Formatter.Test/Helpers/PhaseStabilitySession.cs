using System.Collections.Generic;

using Reihitsu.Formatter.Pipeline;

namespace Reihitsu.Formatter.Test.Helpers;

/// <summary>
/// The check's state for one test execution: its key, the entries and phases it applies, and the recorder of its fixtures
/// </summary>
internal sealed class PhaseStabilitySession
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="testKey">The key of the test execution, as <see cref="PhaseStabilityCheck.FormatTestKey"/> formats it</param>
    /// <param name="entries">The entries to apply</param>
    /// <param name="createPhases">Creates the phases to run alone</param>
    public PhaseStabilitySession(string testKey, IReadOnlyList<PhaseReversalEntry> entries, Func<IReadOnlyList<IFormattingPhase>> createPhases)
    {
        TestKey = testKey;
        Recorder = new PhaseStabilityRecorder(testKey, entries, createPhases);
    }

    #endregion // Constructor

    #region Properties

    /// <summary>
    /// The key of the test execution
    /// </summary>
    public string TestKey { get; }

    /// <summary>
    /// The recorder of the test execution's fixtures
    /// </summary>
    public PhaseStabilityRecorder Recorder { get; private set; }

    /// <summary>
    /// Whether the test's recording has ended
    /// </summary>
    public bool IsEnded { get; private set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Replaces the entries and the phases the check applies, before the check first runs
    /// </summary>
    /// <param name="entries">The entries to apply</param>
    /// <param name="createPhases">Creates the phases to run alone</param>
    public void Override(IReadOnlyList<PhaseReversalEntry> entries, Func<IReadOnlyList<IFormattingPhase>> createPhases)
    {
        Recorder = new PhaseStabilityRecorder(TestKey, entries, createPhases);
    }

    /// <summary>
    /// Marks the test's recording as ended, so a later end finds nothing left to report
    /// </summary>
    public void End()
    {
        IsEnded = true;
    }

    #endregion // Methods
}