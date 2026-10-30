using Reihitsu.Formatter.Test.Helpers.Enumerations;

namespace Reihitsu.Formatter.Test.Helpers;

/// <summary>
/// One known case in which a single formatting phase, run alone on the final output of a fixture, changes it, because a
/// later phase of the pipeline undoes that change in every pass. The entry identifies the fixture by the test execution
/// that formats it (its method and, for a data-driven test, its data row) and by the fingerprint of its final output,
/// identifies the change by the fingerprint of the phase's output, and names the cause, which names the phase, and the
/// line endings under which the change happens. Any other change of the same phase to the same fixture is not covered
/// </summary>
public sealed class PhaseReversalEntry
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="testClass">The test class that declares the test method</param>
    /// <param name="testMethod">The name of the test method that formats the fixture</param>
    /// <param name="fingerprint">The fingerprint of the fixture's final output, as <see cref="PhaseStabilityCheck.ComputeFingerprint"/> computes it</param>
    /// <param name="cause">The cause of the reversal, one of <see cref="PhaseReversalCauses"/>; it names the phase</param>
    /// <param name="phaseOutputFingerprint">The fingerprint of the text the phase returns when it runs alone on the final output</param>
    /// <param name="lineEndings">The line endings under which the phase changes the final output</param>
    /// <param name="dataRow">For a data-driven test, the data row of the execution that formats the fixture, as <see cref="DataRowKey.Format"/> formats it; otherwise <see langword="null"/></param>
    public PhaseReversalEntry(Type testClass, string testMethod, string fingerprint, PhaseReversalCause cause, string phaseOutputFingerprint, PhaseReversalLineEndings lineEndings, string dataRow = null)
        : this(PhaseStabilityCheck.FormatTestName(testClass, testMethod), fingerprint, cause, phaseOutputFingerprint, lineEndings, dataRow)
    {
        TestClass = testClass;
        TestMethod = testMethod;
    }

    /// <summary>
    /// Constructor for an entry that names its test by its qualified name only, as the tests of the recorder do. Such an
    /// entry has no <see cref="TestClass"/> and no <see cref="TestMethod"/>, and <see cref="PhaseReversalExceptionList"/>
    /// does not accept it
    /// </summary>
    /// <param name="testName">The name of the test method, qualified with the full name of its class</param>
    /// <param name="fingerprint">The fingerprint of the fixture's final output, as <see cref="PhaseStabilityCheck.ComputeFingerprint"/> computes it</param>
    /// <param name="cause">The cause of the reversal; it names the phase</param>
    /// <param name="phaseOutputFingerprint">The fingerprint of the text the phase returns when it runs alone on the final output</param>
    /// <param name="lineEndings">The line endings under which the phase changes the final output</param>
    /// <param name="dataRow">For a data-driven test, the data row of the execution that formats the fixture; otherwise <see langword="null"/></param>
    public PhaseReversalEntry(string testName, string fingerprint, PhaseReversalCause cause, string phaseOutputFingerprint, PhaseReversalLineEndings lineEndings, string dataRow = null)
    {
        TestName = testName;
        DataRow = dataRow;
        TestKey = PhaseStabilityCheck.FormatTestKey(testName, dataRow);
        Fingerprint = fingerprint;
        Cause = cause;
        PhaseOutputFingerprint = phaseOutputFingerprint;
        LineEndings = lineEndings;
    }

    #endregion // Constructor

    #region Properties

    /// <summary>
    /// The test class that declares the test method, or <see langword="null"/> when the entry names its test by its
    /// qualified name only
    /// </summary>
    public Type TestClass { get; }

    /// <summary>
    /// The name of the test method that formats the fixture, or <see langword="null"/> when the entry names its test by
    /// its qualified name only
    /// </summary>
    public string TestMethod { get; }

    /// <summary>
    /// The name of the test method, qualified with the full name of its class
    /// </summary>
    public string TestName { get; }

    /// <summary>
    /// For a data-driven test, the data row of the execution that formats the fixture; otherwise <see langword="null"/>
    /// </summary>
    public string DataRow { get; }

    /// <summary>
    /// The key of the test execution that formats the fixture: the qualified test name, with the data row of a
    /// data-driven test
    /// </summary>
    public string TestKey { get; }

    /// <summary>
    /// The fingerprint of the fixture's final output
    /// </summary>
    public string Fingerprint { get; }

    /// <summary>
    /// The cause of the reversal
    /// </summary>
    public PhaseReversalCause Cause { get; }

    /// <summary>
    /// The type name of the phase that changes the final output when it runs alone, as the cause names it
    /// </summary>
    public string Phase => Cause.Phase;

    /// <summary>
    /// The fingerprint of the text the phase returns when it runs alone on the final output
    /// </summary>
    public string PhaseOutputFingerprint { get; }

    /// <summary>
    /// The line endings under which the phase changes the final output
    /// </summary>
    public PhaseReversalLineEndings LineEndings { get; }

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
        return $"{TestKey}, fixture {Fingerprint}, {Phase} output {PhaseOutputFingerprint}, {LineEndings}";
    }

    #endregion // Object
}