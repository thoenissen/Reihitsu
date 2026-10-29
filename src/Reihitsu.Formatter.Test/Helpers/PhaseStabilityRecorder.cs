using System.Collections.Generic;

using Microsoft.CodeAnalysis.CSharp;

using Reihitsu.Formatter.Test.Helpers.Enumerations;

namespace Reihitsu.Formatter.Test.Helpers;

/// <summary>
/// Runs the phase-level stability check for the fixtures of one test and keeps track of which phase reversal exceptions
/// the test's fixtures matched, so an exception whose reversal no longer happens fails the test that carries it
/// </summary>
public sealed class PhaseStabilityRecorder
{
    #region Fields

    /// <summary>
    /// The exceptions that belong to the test
    /// </summary>
    private readonly List<PhaseReversalException> _exceptions;

    /// <summary>
    /// The exceptions and line endings that a fixture of the test reached, whether or not the phase still changes it
    /// </summary>
    private readonly HashSet<(PhaseReversalException Exception, PhaseReversalLineEndings LineEnding)> _reached = [];

    /// <summary>
    /// The fixtures the check ran on, in the order it ran on them
    /// </summary>
    private readonly List<(string Fingerprint, PhaseReversalLineEndings LineEnding, CSharpParseOptions ParseOptions)> _checkedFixtures = [];

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="testClass">The test class that declares the test method</param>
    /// <param name="testMethod">The name of the test method</param>
    /// <param name="exceptions">The phase reversal exceptions to apply; only those of the given test are kept</param>
    public PhaseStabilityRecorder(Type testClass, string testMethod, IEnumerable<PhaseReversalException> exceptions)
        : this(PhaseStabilityCheck.FormatTestName(testClass, testMethod), exceptions)
    {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="testName">The name of the test method, qualified with the full name of its class</param>
    /// <param name="exceptions">The phase reversal exceptions to apply; only those of the given test are kept</param>
    public PhaseStabilityRecorder(string testName, IEnumerable<PhaseReversalException> exceptions)
    {
        TestName = testName;
        _exceptions = exceptions.Where(exception => exception.TestName == testName).ToList();
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
    /// Runs every production phase alone on the final output of a fixture and returns one failure per phase that changes
    /// it without an exception of this test covering it, and one per exception of this fixture whose phase no longer
    /// changes it under the given line ending
    /// </summary>
    /// <param name="finalOutput">The final output of the fixture</param>
    /// <param name="endOfLine">The end-of-line sequence the output was formatted with</param>
    /// <param name="parseOptions">The parse options the fixture was formatted with, or <see langword="null"/> for the defaults</param>
    /// <returns>The failures; empty when the fixture passes the check</returns>
    public IReadOnlyList<string> Check(string finalOutput, string endOfLine, CSharpParseOptions parseOptions)
    {
        var lineEnding = endOfLine == "\n" ? PhaseReversalLineEndings.Lf : PhaseReversalLineEndings.Crlf;
        var endingName = endOfLine == "\n" ? "LF" : "CRLF";
        var fingerprint = PhaseStabilityCheck.ComputeFingerprint(finalOutput);
        var changes = PhaseStabilityCheck.FindChangingPhases(finalOutput, endOfLine, parseOptions);
        var fixtureExceptions = _exceptions.Where(exception => exception.Fingerprint == fingerprint
                                                               && exception.Covers(lineEnding))
                                           .ToList();
        var failures = new List<string>();

        _checkedFixtures.Add((fingerprint, lineEnding, parseOptions));

        foreach (var exception in fixtureExceptions)
        {
            _reached.Add((exception, lineEnding));
        }

        foreach (var change in changes)
        {
            if (fixtureExceptions.Any(exception => exception.Phase == change.Phase) == false)
            {
                failures.Add($"{change.Phase} changes the final output of fixture {fingerprint} of {TestName} under {endingName} line endings when it runs alone. {change.Difference} A reversal with a known cause is listed in {nameof(PhaseReversalExceptions)} with fingerprint \"{fingerprint}\" and phase {change.Phase}.");
            }
        }

        foreach (var exception in fixtureExceptions)
        {
            if (changes.Any(change => change.Phase == exception.Phase) == false)
            {
                failures.Add($"Stale phase reversal exception ({exception}): {exception.Phase} no longer changes the final output of fixture {fingerprint} under {endingName} line endings. Delete the entry, or narrow its line endings.");
            }
        }

        return failures;
    }

    /// <summary>
    /// Returns one failure per exception of this test and line ending it covers that no fixture of the test reached
    /// </summary>
    /// <returns>The failures; empty when every exception of the test was reached</returns>
    public IReadOnlyList<string> FindUnreachedExceptions()
    {
        var failures = new List<string>();

        foreach (var exception in _exceptions)
        {
            foreach (var lineEnding in new[] { PhaseReversalLineEndings.Lf, PhaseReversalLineEndings.Crlf })
            {
                if (exception.Covers(lineEnding)
                    && _reached.Contains((exception, lineEnding)) == false)
                {
                    failures.Add($"Stale phase reversal exception ({exception}): no fixture of the test has the final output {exception.Fingerprint} under {(lineEnding == PhaseReversalLineEndings.Lf ? "LF" : "CRLF")} line endings. Delete the entry, or update its fingerprint.");
                }
            }
        }

        return failures;
    }

    #endregion // Methods
}