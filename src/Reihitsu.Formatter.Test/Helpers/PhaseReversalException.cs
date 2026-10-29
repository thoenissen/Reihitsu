using Reihitsu.Formatter.Test.Helpers.Enumerations;

namespace Reihitsu.Formatter.Test.Helpers;

/// <summary>
/// One known case in which a single formatting phase, run alone on the final output of a fixture, changes it, because a
/// later phase of the pipeline undoes that change in every pass. The entry identifies the fixture by the test method
/// that formats it and by the fingerprint of its final output, names the phase that changes the output, the line endings
/// under which it does, and the cause
/// </summary>
public sealed class PhaseReversalException
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="testClass">The test class that declares the test method</param>
    /// <param name="testMethod">The name of the test method that formats the fixture</param>
    /// <param name="fingerprint">The fingerprint of the fixture's final output, as <see cref="PhaseStabilityCheck.ComputeFingerprint"/> computes it</param>
    /// <param name="phase">The type name of the phase that changes the final output when it runs alone</param>
    /// <param name="lineEndings">The line endings under which the phase changes the final output</param>
    /// <param name="cause">Why the phase changes the final output, one of the texts of <see cref="PhaseReversalCauses"/></param>
    public PhaseReversalException(Type testClass, string testMethod, string fingerprint, string phase, PhaseReversalLineEndings lineEndings, string cause)
    {
        TestClass = testClass;
        TestMethod = testMethod;
        TestName = PhaseStabilityCheck.FormatTestName(testClass, testMethod);
        Fingerprint = fingerprint;
        Phase = phase;
        LineEndings = lineEndings;
        Cause = cause;
    }

    /// <summary>
    /// Constructor for an exception that names its test by its qualified name only, as the tests of the check itself do
    /// </summary>
    /// <param name="testName">The name of the test method, qualified with the full name of its class</param>
    /// <param name="fingerprint">The fingerprint of the fixture's final output, as <see cref="PhaseStabilityCheck.ComputeFingerprint"/> computes it</param>
    /// <param name="phase">The type name of the phase that changes the final output when it runs alone</param>
    /// <param name="lineEndings">The line endings under which the phase changes the final output</param>
    /// <param name="cause">Why the phase changes the final output</param>
    public PhaseReversalException(string testName, string fingerprint, string phase, PhaseReversalLineEndings lineEndings, string cause)
    {
        TestName = testName;
        Fingerprint = fingerprint;
        Phase = phase;
        LineEndings = lineEndings;
        Cause = cause;
    }

    #endregion // Constructor

    #region Properties

    /// <summary>
    /// The test class that declares the test method, or <see langword="null"/> when the exception names its test by its
    /// qualified name only
    /// </summary>
    public Type TestClass { get; }

    /// <summary>
    /// The name of the test method that formats the fixture
    /// </summary>
    public string TestMethod { get; }

    /// <summary>
    /// The fingerprint of the fixture's final output
    /// </summary>
    public string Fingerprint { get; }

    /// <summary>
    /// The type name of the phase that changes the final output when it runs alone
    /// </summary>
    public string Phase { get; }

    /// <summary>
    /// The line endings under which the phase changes the final output
    /// </summary>
    public PhaseReversalLineEndings LineEndings { get; }

    /// <summary>
    /// Why the phase changes the final output
    /// </summary>
    public string Cause { get; }

    /// <summary>
    /// The name of the test method, qualified with the full name of its class
    /// </summary>
    public string TestName { get; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Determines whether the entry covers the given line ending
    /// </summary>
    /// <param name="lineEnding">The line ending</param>
    /// <returns><see langword="true"/> when the entry covers the line ending</returns>
    public bool Covers(PhaseReversalLineEndings lineEnding)
    {
        return (LineEndings & lineEnding) == lineEnding;
    }

    #endregion // Methods

    #region Object

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"{TestName}, fixture {Fingerprint}, {Phase}, {LineEndings}";
    }

    #endregion // Object
}