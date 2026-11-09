using System.Collections.Generic;
using System.Reflection;
using System.Threading;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Pipeline;
using Reihitsu.Formatter.Pipeline.Preparation;

namespace Reihitsu.Formatter.Test.Helpers;

/// <summary>
/// The phase-level stability check (FMT-PIPE-027): every phase of the production pipeline, run alone on the final output
/// of a fixture, must leave that output unchanged. A final output that two phases change and change back in every pass is
/// stable as text and still fails this check. The helpers of <see cref="FormatterTestsBase"/> also run it on the output of
/// an input with syntax errors, which the requirement does not cover; that is stricter than the requirement, so a failure
/// there is a reversal between phases, not a violation of FMT-PIPE-027
/// </summary>
public static class PhaseStabilityCheck
{
    #region Fields

    /// <summary>
    /// The check's state for the test that is currently running on this logical call context. The test initialization
    /// creates it, so the check and the test cleanup share it
    /// </summary>
    private static readonly AsyncLocal<PhaseStabilitySession> _currentSession = new();

    /// <summary>
    /// The private factory of the production phase list, read through reflection so the check runs exactly the phases
    /// the pipeline runs, in its order, and no restated copy of the list can drift from it
    /// </summary>
    private static readonly MethodInfo _createPhases = typeof(FormattingPipeline).GetMethod("CreatePhases", BindingFlags.NonPublic | BindingFlags.Static);

    #endregion // Fields

    #region Methods

    /// <summary>
    /// Starts the check for a test, with the production phases
    /// </summary>
    public static void BeginTest()
    {
        _currentSession.Value = new PhaseStabilitySession(CreateProductionPhases);
    }

    /// <summary>
    /// Ends the check for the current test
    /// </summary>
    public static void EndTest()
    {
        _currentSession.Value = null;
    }

    /// <summary>
    /// Runs the check on the final output of a fixture of the current test and fails on every phase that changes it
    /// </summary>
    /// <param name="finalOutput">The final output of the fixture</param>
    /// <param name="endOfLine">The end-of-line sequence the output was formatted with</param>
    /// <param name="parseOptions">The parse options the fixture was formatted with, or <see langword="null"/> for the defaults</param>
    public static void Verify(string finalOutput, string endOfLine, CSharpParseOptions parseOptions)
    {
        var failures = GetCurrentSession().Check(finalOutput, endOfLine, parseOptions);

        if (failures.Count > 0)
        {
            Assert.Fail(string.Join(Environment.NewLine, failures));
        }
    }

    /// <summary>
    /// Returns the fixtures the check ran on in the current test so far
    /// </summary>
    /// <returns>The final output, end-of-line sequence, and parse options of every checked fixture, in the order the check ran</returns>
    public static IReadOnlyList<(string FinalOutput, string EndOfLine, CSharpParseOptions ParseOptions)> GetCheckedFixtures()
    {
        return GetCurrentSession().CheckedFixtures;
    }

    /// <summary>
    /// Runs every production phase alone on the given text, parsed as a second pass would parse it, and returns the
    /// changes of the phases that change it
    /// </summary>
    /// <param name="text">The text to check</param>
    /// <param name="endOfLine">The end-of-line sequence of the formatting context</param>
    /// <param name="parseOptions">The parse options, or <see langword="null"/> for the defaults</param>
    /// <returns>One change per phase that changes the text, in pipeline order</returns>
    public static IReadOnlyList<PhaseChange> FindChangingPhases(string text, string endOfLine, CSharpParseOptions parseOptions)
    {
        return FindChangingPhases(text, endOfLine, parseOptions, CreateProductionPhases());
    }

    /// <summary>
    /// Creates the phases of the production pipeline in the order the pipeline runs them, with the preparation phase replaced
    /// by its sub-phases, which the pipeline runs and reports as phases of their own
    /// </summary>
    /// <returns>The production phases</returns>
    internal static IReadOnlyList<IFormattingPhase> CreateProductionPhases()
    {
        Assert.IsNotNull(_createPhases, "FormattingPipeline.CreatePhases was not found; the phase-level stability check must run the production phase list.");

        var phases = new List<IFormattingPhase>();

        foreach (var phase in (IReadOnlyList<IFormattingPhase>)_createPhases.Invoke(null, null))
        {
            if (phase is PreparationPhase preparationPhase)
            {
                phases.AddRange(preparationPhase.SubPhases);
            }
            else
            {
                phases.Add(phase);
            }
        }

        return phases;
    }

    /// <summary>
    /// Returns the name under which the pipeline observer reports the phase when it runs alone, so the check names a phase as
    /// the observer and the trace do, a sub-phase of the preparation phase included
    /// </summary>
    /// <param name="phase">The phase</param>
    /// <returns>The reported name</returns>
    internal static string GetReportedName(IFormattingPhase phase)
    {
        var names = new List<string>();

        FormattingPipeline.ExecutePhases(SyntaxFactory.ParseCompilationUnit(string.Empty),
                                         new FormattingContext("\n"),
                                         [phase],
                                         (name, _, _) => names.Add(name),
                                         CancellationToken.None);

        Assert.HasCount(1, names, "A phase that runs alone should be reported as exactly one step.");

        return names[0];
    }

    /// <summary>
    /// Replaces, for the rest of the current test, the phases the check runs alone. The tests of the check use it to drive
    /// the helpers into their failures without a real reversal
    /// </summary>
    /// <param name="createPhases">Creates the phases to run alone instead of the production phases</param>
    internal static void OverrideForCurrentTest(Func<IReadOnlyList<IFormattingPhase>> createPhases)
    {
        var session = GetCurrentSession();

        Assert.IsEmpty(session.CheckedFixtures, "The check must be overridden before it first runs in the test.");

        session.Override(createPhases);
    }

    /// <summary>
    /// Runs the given phases alone on the given text, parsed as a second pass would parse it, and returns the changes of
    /// the phases that change it
    /// </summary>
    /// <param name="text">The text to check</param>
    /// <param name="endOfLine">The end-of-line sequence of the formatting context</param>
    /// <param name="parseOptions">The parse options, or <see langword="null"/> for the defaults</param>
    /// <param name="phases">The phases to run alone</param>
    /// <returns>One change per phase that changes the text, in the given order</returns>
    internal static IReadOnlyList<PhaseChange> FindChangingPhases(string text, string endOfLine, CSharpParseOptions parseOptions, IReadOnlyList<IFormattingPhase> phases)
    {
        var (root, context) = SecondPassInput.Create(text, endOfLine, parseOptions);

        var changes = new List<PhaseChange>();

        foreach (var phase in phases)
        {
            var output = FormattingPipeline.ExecutePhases(root, context, [phase], null, CancellationToken.None).Output.ToFullString();

            if (output != text)
            {
                changes.Add(new PhaseChange(GetReportedName(phase), text, output));
            }
        }

        return changes;
    }

    /// <summary>
    /// Returns the check's state for the current test
    /// </summary>
    /// <returns>The state</returns>
    private static PhaseStabilitySession GetCurrentSession()
    {
        var session = _currentSession.Value;

        Assert.IsNotNull(session, "The phase-level stability check runs only inside a test of a class derived from FormatterTestsBase.");

        return session;
    }

    #endregion // Methods
}