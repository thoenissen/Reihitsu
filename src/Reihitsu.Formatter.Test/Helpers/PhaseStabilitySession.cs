using System.Collections.Generic;

using Reihitsu.Formatter.Pipeline;

namespace Reihitsu.Formatter.Test.Helpers;

/// <summary>
/// The check's state for one test: the entries and phases it applies, and one recorder per test method that called it
/// </summary>
internal sealed class PhaseStabilitySession
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="entries">The entries to apply</param>
    /// <param name="createPhases">Creates the phases to run alone</param>
    public PhaseStabilitySession(IReadOnlyList<PhaseReversalEntry> entries, Func<IReadOnlyList<IFormattingPhase>> createPhases)
    {
        Entries = entries;
        CreatePhases = createPhases;
    }

    #endregion // Constructor

    #region Properties

    /// <summary>
    /// The entries to apply
    /// </summary>
    public IReadOnlyList<PhaseReversalEntry> Entries { get; private set; }

    /// <summary>
    /// Creates the phases to run alone
    /// </summary>
    public Func<IReadOnlyList<IFormattingPhase>> CreatePhases { get; private set; }

    /// <summary>
    /// One recorder per test method that called the check
    /// </summary>
    public Dictionary<string, PhaseStabilityRecorder> Recorders { get; } = [];

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Replaces the entries and the phases the check applies
    /// </summary>
    /// <param name="entries">The entries to apply</param>
    /// <param name="createPhases">Creates the phases to run alone</param>
    public void Override(IReadOnlyList<PhaseReversalEntry> entries, Func<IReadOnlyList<IFormattingPhase>> createPhases)
    {
        Entries = entries;
        CreatePhases = createPhases;
    }

    #endregion // Methods
}