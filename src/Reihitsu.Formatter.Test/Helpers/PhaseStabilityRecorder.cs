using System.Collections.Generic;

using Microsoft.CodeAnalysis.CSharp;

using Reihitsu.Formatter.Pipeline;
using Reihitsu.Formatter.Test.Helpers.Enumerations;

namespace Reihitsu.Formatter.Test.Helpers;

/// <summary>
/// Runs the phase-level stability check for the fixtures of one test and keeps track of which phase reversal entries
/// the test's fixtures reached, so an entry whose reversal no longer happens fails the test that carries it
/// </summary>
public sealed class PhaseStabilityRecorder
{
    #region Fields

    /// <summary>
    /// The entries that belong to the test
    /// </summary>
    private readonly List<PhaseReversalEntry> _entries;

    /// <summary>
    /// Creates the phases the check runs alone
    /// </summary>
    private readonly Func<IReadOnlyList<IFormattingPhase>> _createPhases;

    /// <summary>
    /// The entries and line endings that a fixture of the test reached, whether or not the phase still makes the entry's
    /// change
    /// </summary>
    private readonly HashSet<(PhaseReversalEntry Entry, PhaseReversalLineEndings LineEnding)> _reached = [];

    /// <summary>
    /// The fixtures the check ran on, in the order it ran on them
    /// </summary>
    private readonly List<(string Fingerprint, PhaseReversalLineEndings LineEnding, CSharpParseOptions ParseOptions)> _checkedFixtures = [];

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor for a recorder that runs the production phases
    /// </summary>
    /// <param name="testName">The name of the test method, qualified with the full name of its class</param>
    /// <param name="entries">The phase reversal entries to apply; only those of the given test are kept</param>
    public PhaseStabilityRecorder(string testName, IEnumerable<PhaseReversalEntry> entries)
        : this(testName, entries, PhaseStabilityCheck.CreateProductionPhases)
    {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="testName">The name of the test method, qualified with the full name of its class</param>
    /// <param name="entries">The phase reversal entries to apply; only those of the given test are kept</param>
    /// <param name="createPhases">Creates the phases the check runs alone</param>
    internal PhaseStabilityRecorder(string testName, IEnumerable<PhaseReversalEntry> entries, Func<IReadOnlyList<IFormattingPhase>> createPhases)
    {
        TestName = testName;
        _entries = entries.Where(entry => entry.TestName == testName).ToList();
        _createPhases = createPhases;
    }

    #endregion // Constructor

    #region Properties

    /// <summary>
    /// The name of the test method, qualified with the full name of its class
    /// </summary>
    public string TestName { get; }

    /// <summary>
    /// The fixtures the check ran on, in the order it ran on them: the fingerprint of each final output, its line ending,
    /// and the parse options it was parsed with
    /// </summary>
    public IReadOnlyList<(string Fingerprint, PhaseReversalLineEndings LineEnding, CSharpParseOptions ParseOptions)> CheckedFixtures => _checkedFixtures;

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Runs every phase alone on the final output of a fixture and returns one failure per change that no entry of this
    /// fixture covers, and one per entry of this fixture whose change the phase no longer makes under the given line
    /// ending. An entry covers a change only when its phase and the fingerprint of the phase's output both match
    /// </summary>
    /// <param name="finalOutput">The final output of the fixture</param>
    /// <param name="endOfLine">The end-of-line sequence the output was formatted with</param>
    /// <param name="parseOptions">The parse options the fixture was formatted with, or <see langword="null"/> for the defaults</param>
    /// <returns>The failures; empty when the fixture passes the check</returns>
    public IReadOnlyList<string> Check(string finalOutput, string endOfLine, CSharpParseOptions parseOptions)
    {
        var lineEnding = ToLineEnding(endOfLine);
        var endingName = Describe(lineEnding);
        var fingerprint = PhaseStabilityCheck.ComputeFingerprint(finalOutput);
        var changes = PhaseStabilityCheck.FindChangingPhases(finalOutput, endOfLine, parseOptions, _createPhases());
        var fixtureEntries = _entries.Where(entry => entry.Fingerprint == fingerprint
                                                     && entry.Covers(lineEnding))
                                     .ToList();
        var failures = new List<string>();

        _checkedFixtures.Add((fingerprint, lineEnding, parseOptions));

        foreach (var entry in fixtureEntries)
        {
            _reached.Add((entry, lineEnding));
        }

        foreach (var change in changes)
        {
            if (fixtureEntries.Any(entry => IsCoveredBy(change, entry)) == false)
            {
                failures.Add($"{change.Phase} changes the final output of fixture {fingerprint} of {TestName} under {endingName} line endings when it runs alone, to the output {change.OutputFingerprint}. {change.Difference} A reversal with a known cause is listed in {nameof(PhaseReversalExceptionList)} with fixture \"{fingerprint}\" and phase output \"{change.OutputFingerprint}\".");
            }
        }

        foreach (var entry in fixtureEntries)
        {
            if (changes.Any(change => IsCoveredBy(change, entry)) == false)
            {
                failures.Add($"Stale phase reversal entry ({entry}): {entry.Phase} no longer changes the final output of fixture {fingerprint} to the output {entry.PhaseOutputFingerprint} under {endingName} line endings. Delete the entry, or narrow its line endings.");
            }
        }

        return failures;
    }

    /// <summary>
    /// Returns one failure per entry of this test and line ending it covers that no fixture of the test reached
    /// </summary>
    /// <returns>The failures; empty when every entry of the test was reached</returns>
    public IReadOnlyList<string> FindUnreachedEntries()
    {
        var failures = new List<string>();

        foreach (var entry in _entries)
        {
            foreach (var lineEnding in new[] { PhaseReversalLineEndings.Lf, PhaseReversalLineEndings.Crlf })
            {
                if (entry.Covers(lineEnding)
                    && _reached.Contains((entry, lineEnding)) == false)
                {
                    failures.Add($"Stale phase reversal entry ({entry}): no fixture of the test has the final output {entry.Fingerprint} under {Describe(lineEnding)} line endings. Delete the entry, or update its fingerprint.");
                }
            }
        }

        return failures;
    }

    /// <summary>
    /// Determines whether an entry covers a change: the same phase, returning the same output
    /// </summary>
    /// <param name="change">The change</param>
    /// <param name="entry">The entry</param>
    /// <returns><see langword="true"/> when the entry covers the change</returns>
    private static bool IsCoveredBy(PhaseChange change, PhaseReversalEntry entry)
    {
        return change.Phase == entry.Phase
               && change.OutputFingerprint == entry.PhaseOutputFingerprint;
    }

    /// <summary>
    /// Maps an end-of-line sequence to its line ending
    /// </summary>
    /// <param name="endOfLine">The end-of-line sequence</param>
    /// <returns>The line ending</returns>
    private static PhaseReversalLineEndings ToLineEnding(string endOfLine)
    {
        return endOfLine == "\n" ? PhaseReversalLineEndings.Lf : PhaseReversalLineEndings.Crlf;
    }

    /// <summary>
    /// Names a line ending for a failure message
    /// </summary>
    /// <param name="lineEnding">The line ending</param>
    /// <returns><c>LF</c> or <c>CRLF</c></returns>
    private static string Describe(PhaseReversalLineEndings lineEnding)
    {
        return lineEnding == PhaseReversalLineEndings.Lf ? "LF" : "CRLF";
    }

    #endregion // Methods
}