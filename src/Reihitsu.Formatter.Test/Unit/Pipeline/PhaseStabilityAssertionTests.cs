using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Pipeline.Indentation;
using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Unit.Pipeline;

/// <summary>
/// Tests for the session-free assertion of the phase-level stability check (FMT-PIPE-027), which tests outside
/// <see cref="FormatterTestsBase"/> run on the final output of a full formatter run. The class does not derive from
/// <see cref="FormatterTestsBase"/>, so no test here has the session that base starts
/// </summary>
[TestClass]
public class PhaseStabilityAssertionTests
{
    #region Constants

    /// <summary>
    /// Formatted source that no phase changes
    /// </summary>
    private const string FormattedSource = "class C\n{\n    int _value;\n}";

    /// <summary>
    /// Source whose only defect is the indentation of one member, so only the indentation phase changes it
    /// </summary>
    private const string MisindentedSource = "class C\n{\n        int _value;\n}";

    /// <summary>
    /// Source whose misindented member is code only when <c>FEATURE</c> is defined, and disabled text otherwise
    /// </summary>
    private const string ConditionalMisindentedSource = "#if FEATURE\nclass C\n{\n        int _value;\n}\n#endif";

    #endregion // Constants

    #region Fields

    /// <summary>
    /// The line endings every test runs under
    /// </summary>
    private static readonly string[] _lineEndings = ["\n", "\r\n"];

    #endregion // Fields

    #region Properties

    /// <summary>
    /// Test context for the current test
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies that the assertion passes on formatted source outside a test of <see cref="FormatterTestsBase"/>
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-027")]
    public void AssertionPassesOnFormattedSourceWithoutASession()
    {
        foreach (var endOfLine in _lineEndings)
        {
            PhaseStabilityCheck.AssertNoPhaseChanges(FormattedSource.Replace("\n", endOfLine), endOfLine, null);
        }
    }

    /// <summary>
    /// Verifies that the assertion fails on a phase that changes the output, with the text the session's check fails with:
    /// the phase, the line ending, the requirement, and the first changed line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-027")]
    public void AssertionFailsNamingThePhaseThatChangesTheOutput()
    {
        foreach (var (endOfLine, endingName) in new[] { ("\n", "LF"), ("\r\n", "CRLF") })
        {
            // Act
            var failure = Assert.Throws<AssertFailedException>(() => PhaseStabilityCheck.AssertNoPhaseChanges(MisindentedSource.Replace("\n", endOfLine), endOfLine, null));

            // Assert
            Assert.Contains($"{nameof(IndentationPhase)} changes the final output under {endingName} line endings when it runs alone (FMT-PIPE-027).", failure.Message);
            Assert.Contains("First difference at line 3: expected \"        int _value;", failure.Message);
            Assert.Contains("phase returned \"    int _value;", failure.Message);
        }
    }

    /// <summary>
    /// Verifies that the assertion parses the output with the given parse options, so code the input's symbols activate is
    /// checked and not treated as disabled text
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-027")]
    public void AssertionParsesTheOutputWithTheGivenParseOptions()
    {
        var parseOptions = CSharpParseOptions.Default.WithPreprocessorSymbols("FEATURE");

        foreach (var endOfLine in _lineEndings)
        {
            var text = ConditionalMisindentedSource.Replace("\n", endOfLine);

            // Act
            var failure = Assert.Throws<AssertFailedException>(() => PhaseStabilityCheck.AssertNoPhaseChanges(text, endOfLine, parseOptions));

            // Assert
            Assert.Contains(nameof(IndentationPhase), failure.Message);

            PhaseStabilityCheck.AssertNoPhaseChanges(text, endOfLine, null);
        }
    }

    /// <summary>
    /// Verifies that the assertion on an output tree checks the tree's text with the line ending detected in it and with the
    /// tree's parse options, as FMT-PIPE-027 states the input of the check
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-027")]
    public void AssertionOnAnOutputTreeUsesTheDetectedLineEndingAndTheTreeParseOptions()
    {
        var parseOptions = CSharpParseOptions.Default.WithPreprocessorSymbols("FEATURE");

        foreach (var (endOfLine, endingName) in new[] { ("\n", "LF"), ("\r\n", "CRLF") })
        {
            var text = ConditionalMisindentedSource.Replace("\n", endOfLine);

            // Act
            var failure = Assert.Throws<AssertFailedException>(() => PhaseStabilityCheck.AssertNoPhaseChanges(CSharpSyntaxTree.ParseText(text, parseOptions, cancellationToken: TestContext.CancellationToken)));

            // Assert
            Assert.Contains($"{nameof(IndentationPhase)} changes the final output under {endingName} line endings when it runs alone (FMT-PIPE-027).", failure.Message);

            PhaseStabilityCheck.AssertNoPhaseChanges(CSharpSyntaxTree.ParseText(text, cancellationToken: TestContext.CancellationToken));
        }
    }

    /// <summary>
    /// Verifies that the assertion on the output tree of <see cref="ReihitsuFormatter.FormatSyntaxTree"/> passes, with
    /// the parse options of the input carried to the output
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-027")]
    public void AssertionPassesOnTheOutputTreeOfTheFormatter()
    {
        var parseOptions = CSharpParseOptions.Default.WithPreprocessorSymbols("FEATURE");

        foreach (var endOfLine in _lineEndings)
        {
            var output = ReihitsuFormatter.FormatSyntaxTree(CSharpSyntaxTree.ParseText(ConditionalMisindentedSource.Replace("\n", endOfLine), parseOptions, cancellationToken: TestContext.CancellationToken), TestContext.CancellationToken).Output;

            Assert.Contains($"{endOfLine}    int _value;", output.ToString(), "The formatter should indent the active member.");

            PhaseStabilityCheck.AssertNoPhaseChanges(output);
        }
    }

    #endregion // Methods
}