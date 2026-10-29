﻿using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Pipeline;
using Reihitsu.Formatter.Test.Helpers.Enumerations;

namespace Reihitsu.Formatter.Test.Helpers;

/// <summary>
/// The phase-level stability check: every phase of the production pipeline, run alone on the final output of a fixture,
/// must leave that output unchanged. A final output that two phases change and change back in every pass is stable as
/// text and still fails this check
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
    /// Starts recording the phase-level stability check for a test, with the entries of <see cref="PhaseReversalExceptionList"/>
    /// and the production phases
    /// </summary>
    public static void BeginTest()
    {
        _currentSession.Value = new PhaseStabilitySession(PhaseReversalExceptionList.Entries, CreateProductionPhases);
    }

    /// <summary>
    /// Ends recording for the current test and fails when an entry of a test method that called the check was reached by
    /// none of its fixtures. A second call in the same test finds nothing left to report
    /// </summary>
    public static void EndTest()
    {
        var session = _currentSession.Value;

        _currentSession.Value = null;

        if (session == null)
        {
            return;
        }

        var failures = session.Recorders.Values.SelectMany(recorder => recorder.FindUnreachedEntries()).ToList();

        session.Recorders.Clear();

        if (failures.Count > 0)
        {
            Assert.Fail(string.Join(Environment.NewLine, failures));
        }
    }

    /// <summary>
    /// Runs the check on the final output of a fixture of the current test and fails on every change without an entry,
    /// and on every entry of the fixture whose change no longer happens
    /// </summary>
    /// <param name="finalOutput">The final output of the fixture</param>
    /// <param name="endOfLine">The end-of-line sequence the output was formatted with</param>
    /// <param name="parseOptions">The parse options the fixture was formatted with, or <see langword="null"/> for the defaults</param>
    public static void Verify(string finalOutput, string endOfLine, CSharpParseOptions parseOptions)
    {
        var session = GetCurrentSession();
        var testName = FindCallingTestName();

        if (session.Recorders.TryGetValue(testName, out var recorder) == false)
        {
            recorder = new PhaseStabilityRecorder(testName, session.Entries, session.CreatePhases);
            session.Recorders.Add(testName, recorder);
        }

        var failures = recorder.Check(finalOutput, endOfLine, parseOptions);

        if (failures.Count > 0)
        {
            Assert.Fail(string.Join(Environment.NewLine, failures));
        }
    }

    /// <summary>
    /// Returns the fixtures the check ran on for the calling test method so far
    /// </summary>
    /// <returns>The fingerprint, line ending, and parse options of every checked fixture, in the order the check ran</returns>
    public static IReadOnlyList<(string Fingerprint, PhaseReversalLineEndings LineEnding, CSharpParseOptions ParseOptions)> GetCheckedFixtures()
    {
        return GetCurrentSession().Recorders.TryGetValue(FindCallingTestName(), out var recorder)
                   ? recorder.CheckedFixtures
                   : [];
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
    /// Computes the fingerprint that identifies a text: the first 16 hexadecimal digits of the SHA-256 hash of the text
    /// with every line break written as LF, so one fingerprint names a fixture's output under both line endings
    /// </summary>
    /// <param name="text">The text</param>
    /// <returns>The fingerprint</returns>
    public static string ComputeFingerprint(string text)
    {
        var normalized = text.Replace("\r\n", "\n");
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(normalized));

        return Convert.ToHexString(hash, 0, 8).ToLowerInvariant();
    }

    /// <summary>
    /// Formats the name of a test method, qualified with the full name of its class
    /// </summary>
    /// <param name="testClass">The test class</param>
    /// <param name="testMethod">The name of the test method</param>
    /// <returns>The qualified name</returns>
    public static string FormatTestName(Type testClass, string testMethod)
    {
        return $"{testClass.FullName}.{testMethod}";
    }

    /// <summary>
    /// Creates the phases of the production pipeline in the order the pipeline runs them
    /// </summary>
    /// <returns>The production phases</returns>
    internal static IReadOnlyList<IFormattingPhase> CreateProductionPhases()
    {
        Assert.IsNotNull(_createPhases, "FormattingPipeline.CreatePhases was not found; the phase-level stability check must run the production phase list.");

        return (IReadOnlyList<IFormattingPhase>)_createPhases.Invoke(null, null);
    }

    /// <summary>
    /// Replaces, for the rest of the current test, the entries and the phases the check uses. The tests of the check use
    /// it to drive the helpers and the test cleanup into their failures without a real reversal
    /// </summary>
    /// <param name="entries">The entries to apply instead of <see cref="PhaseReversalExceptionList"/></param>
    /// <param name="createPhases">Creates the phases to run alone instead of the production phases</param>
    internal static void OverrideForCurrentTest(IReadOnlyList<PhaseReversalEntry> entries, Func<IReadOnlyList<IFormattingPhase>> createPhases)
    {
        var session = GetCurrentSession();

        Assert.IsEmpty(session.Recorders, "The check must be overridden before it first runs in the test.");

        session.Override(entries, createPhases);
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
            var output = FormattingPipeline.ExecutePhases(root, context, [phase], null, CancellationToken.None).ToFullString();

            if (output != text)
            {
                changes.Add(new PhaseChange(phase.GetType().Name, text, output));
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

    /// <summary>
    /// Finds the test method on the call stack: the innermost method that carries a test method attribute, or whose async
    /// state machine is on the stack
    /// </summary>
    /// <returns>The name of the test method, qualified with the full name of its class</returns>
    private static string FindCallingTestName()
    {
        foreach (var frame in new StackTrace().GetFrames())
        {
            var method = frame.GetMethod();

            if (method == null)
            {
                continue;
            }

            if (method.IsDefined(typeof(TestMethodAttribute), inherit: true))
            {
                return FormatTestName(method.DeclaringType, method.Name);
            }

            var stateMachineType = method.DeclaringType;
            var ownerType = stateMachineType?.DeclaringType;

            if (ownerType == null)
            {
                continue;
            }

            foreach (var candidate in ownerType.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
            {
                if (candidate.GetCustomAttribute<AsyncStateMachineAttribute>()?.StateMachineType == stateMachineType
                    && candidate.IsDefined(typeof(TestMethodAttribute), inherit: true))
                {
                    return FormatTestName(ownerType, candidate.Name);
                }
            }
        }

        Assert.Fail("The phase-level stability check found no test method on the call stack.");

        return null;
    }

    #endregion // Methods
}