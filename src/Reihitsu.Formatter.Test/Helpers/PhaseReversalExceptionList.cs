using System.Collections.Generic;

namespace Reihitsu.Formatter.Test.Helpers;

/// <summary>
/// The exception list of the phase-level stability check: the fixtures whose final output a single phase changes today,
/// when it runs alone, because a later phase undoes the change in every pass. The list can only shrink: an entry whose
/// reversal no longer happens fails the test that carries it, so the change that removes a cause also deletes its entries
/// </summary>
public static class PhaseReversalExceptionList
{
    #region Fields

    /// <summary>
    /// The known phase reversals, one entry per fixture and phase, each with the output the phase returns
    /// </summary>
    public static readonly IReadOnlyList<PhaseReversalEntry> Entries = [];

    #endregion // Fields
}