using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Pipeline;
using Reihitsu.Formatter.Test.Helpers.Enumerations;
using Reihitsu.Formatter.Utilities;

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
    /// The recorders of the test that is currently running on this logical call context, one per test method that called
    /// the check. The test initialization creates the dictionary, so the check and the test cleanup share it
    /// </summary>
    private static readonly AsyncLocal<Dictionary<string, PhaseStabilityRecorder>> _currentRecorders = new();

    /// <summary>
    /// The private factory of the production phase list, read through reflection so the check runs exactly the phases
    /// the pipeline runs, in its order, and no restated copy of the list can drift from it
    /// </summary>
    private static readonly MethodInfo _createPhases = typeof(FormattingPipeline).GetMethod("CreatePhases", BindingFlags.NonPublic | BindingFlags.Static);

    #endregion // Fields

    #region Methods

    /// <summary>
    /// Starts recording the phase-level stability check for a test
    /// </summary>
    public static void BeginTest()
    {
        _currentRecorders.Value = [];
    }

    /// <summary>
    /// Ends recording for the current test and fails when an exception of a test method that called the check was
    /// reached by none of its fixtures
    /// </summary>
    public static void EndTest()
    {
        var recorders = _currentRecorders.Value;

        _currentRecorders.Value = null;

        if (recorders == null)
        {
            return;
        }

        var failures = recorders.Values.SelectMany(recorder => recorder.FindUnreachedExceptions()).ToList();

        if (failures.Count > 0)
        {
            Assert.Fail(string.Join(Environment.NewLine, failures));
        }
    }

    /// <summary>
    /// Runs the check on the final output of a fixture of the current test and fails on every phase that changes it
    /// without an exception, and on every exception of the fixture that no longer applies
    /// </summary>
    /// <param name="finalOutput">The final output of the fixture</param>
    /// <param name="endOfLine">The end-of-line sequence the output was formatted with</param>
    /// <param name="parseOptions">The parse options the fixture was formatted with, or <see langword="null"/> for the defaults</param>
    public static void Verify(string finalOutput, string endOfLine, CSharpParseOptions parseOptions)
    {
        var recorders = _currentRecorders.Value;

        Assert.IsNotNull(recorders, "The phase-level stability check runs only inside a test of a class derived from FormatterTestsBase.");

        var testName = FindCallingTestName();

        if (recorders.TryGetValue(testName, out var recorder) == false)
        {
            recorder = new PhaseStabilityRecorder(testName, PhaseReversalExceptions.Entries);
            recorders.Add(testName, recorder);
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
        var recorders = _currentRecorders.Value;

        Assert.IsNotNull(recorders, "The phase-level stability check runs only inside a test of a class derived from FormatterTestsBase.");

        return recorders.TryGetValue(FindCallingTestName(), out var recorder)
                   ? recorder.CheckedFixtures
                   : [];
    }

    /// <summary>
    /// Runs every production phase alone on the given text, parsed as a second pass would parse it, and returns the
    /// phases that change it
    /// </summary>
    /// <param name="text">The text to check</param>
    /// <param name="endOfLine">The end-of-line sequence of the formatting context</param>
    /// <param name="parseOptions">The parse options, or <see langword="null"/> for the defaults</param>
    /// <returns>One entry per phase that changes the text, in pipeline order</returns>
    public static IReadOnlyList<(string Phase, string Difference)> FindChangingPhases(string text, string endOfLine, CSharpParseOptions parseOptions)
    {
        var changes = new List<(string Phase, string Difference)>();

        foreach (var phase in CreateProductionPhases())
        {
            var tree = CSharpSyntaxTree.ParseText(text, parseOptions);
            var context = new FormattingContext(endOfLine, languageVersion: LanguageVersionResolver.Resolve(tree.Options));
            var output = FormattingPipeline.ExecutePhases(tree.GetRoot(), context, [phase], null, CancellationToken.None).ToFullString();

            if (output != text)
            {
                changes.Add((phase.GetType().Name, DescribeFirstDifference(text, output)));
            }
        }

        return changes;
    }

    /// <summary>
    /// Computes the fingerprint that identifies a fixture by its final output: the first 16 hexadecimal digits of the
    /// SHA-256 hash of the output with every line break written as LF, so one fingerprint names the fixture under both
    /// line endings
    /// </summary>
    /// <param name="finalOutput">The final output of the fixture</param>
    /// <returns>The fingerprint</returns>
    public static string ComputeFingerprint(string finalOutput)
    {
        var normalized = finalOutput.Replace("\r\n", "\n");
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

    /// <summary>
    /// Describes the first line in which two texts differ
    /// </summary>
    /// <param name="expected">The text the phase received</param>
    /// <param name="actual">The text the phase returned</param>
    /// <returns>The description</returns>
    private static string DescribeFirstDifference(string expected, string actual)
    {
        var expectedLines = expected.Split('\n');
        var actualLines = actual.Split('\n');
        var lineCount = Math.Max(expectedLines.Length, actualLines.Length);

        for (var index = 0; index < lineCount; index++)
        {
            var expectedLine = index < expectedLines.Length ? expectedLines[index] : null;
            var actualLine = index < actualLines.Length ? actualLines[index] : null;

            if (expectedLine != actualLine)
            {
                return $"First difference at line {index + 1}: expected {Quote(expectedLine)}, phase returned {Quote(actualLine)}.";
            }
        }

        return "The texts differ only in their line breaks.";
    }

    /// <summary>
    /// Quotes a line for a failure message, making a carriage return visible
    /// </summary>
    /// <param name="line">The line, or <see langword="null"/> when the text has no such line</param>
    /// <returns>The quoted line</returns>
    private static string Quote(string line)
    {
        return line == null ? "<no line>" : $"\"{line.Replace("\r", "\\r")}\"";
    }

    #endregion // Methods
}