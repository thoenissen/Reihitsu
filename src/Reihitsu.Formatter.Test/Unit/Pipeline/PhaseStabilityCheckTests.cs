using System.Collections.Generic;
using System.Threading;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Pipeline;
using Reihitsu.Formatter.Pipeline.Indentation;
using Reihitsu.Formatter.Test.Helpers;
using Reihitsu.Formatter.Utilities;

namespace Reihitsu.Formatter.Test.Unit.Pipeline;

/// <summary>
/// Tests for FMT-PIPE-027 and for the phase-level stability check of the formatter test helpers: every production phase,
/// run alone on the final output of a fixture, must leave it unchanged
/// </summary>
[TestClass]
public class PhaseStabilityCheckTests : FormatterTestsBase
{
    #region Constants

    /// <summary>
    /// Formatted source that no phase changes
    /// </summary>
    private const string FormattedSource = """
                                           class C
                                           {
                                               int _value;
                                           }
                                           """;

    /// <summary>
    /// Source whose only defect is the indentation of one member, so only the indentation phase changes it
    /// </summary>
    private const string MisindentedSource = """
                                             class C
                                             {
                                                     int _value;
                                             }
                                             """;

    #endregion // Constants

    #region Methods

    /// <summary>
    /// Verifies that the check runs the phases of the production pipeline, in the order the pipeline runs them and under the
    /// names the pipeline observer reports, the sub-phases of the preparation phase included, so a phase added to or removed
    /// from the pipeline is checked without a second list to update
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-027")]
    public void CheckRunsTheProductionPhasesInPipelineOrder()
    {
        // Arrange
        var observedNames = new[] { ObservedPipeline.PrecheckPhaseName }.Concat(ObservedPipeline.Run(FormattedSource, "\n").Select(phase => phase.Name)).ToList();

        // Act
        var checkedNames = PhaseStabilityCheck.CreateProductionPhases().Select(PhaseStabilityCheck.GetReportedName).ToList();

        // Assert
        Assert.AreSequenceEqual(observedNames, checkedNames);
        Assert.HasCount(checkedNames.Count, checkedNames.Distinct(), "Every production phase should be checked once.");
    }

    /// <summary>
    /// Verifies that no phase changes formatted source
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-027")]
    public void CheckFindsNoChangeInFormattedSource()
    {
        // Act & Assert
        AssertNoPhaseChanges(FormattedSource);
    }

    /// <summary>
    /// Verifies that the check finds the phase that changes source when that phase runs alone, and what it returns
    /// </summary>
    [TestMethod]
    public void CheckFindsThePhaseThatChangesTheSource()
    {
        foreach (var endOfLine in _lineEndings)
        {
            // Act
            var changes = PhaseStabilityCheck.FindChangingPhases(NormalizeLineEndings(MisindentedSource, endOfLine), endOfLine, null);

            // Assert
            Assert.HasCount(1, changes, $"Under {DescribeLineEnding(endOfLine)} line endings.");
            Assert.AreEqual(nameof(IndentationPhase), changes[0].Phase);
            Assert.AreEqual(NormalizeLineEndings(FormattedSource, endOfLine), changes[0].Output);
        }
    }

    /// <summary>
    /// Verifies that the check parses the text with the fixture's parse options, so code inside a conditional branch
    /// that the fixture's symbols activate is checked and not treated as disabled text
    /// </summary>
    [TestMethod]
    public void CheckParsesTheTextWithTheFixtureParseOptions()
    {
        // Arrange
        const string source = """
                              #if FEATURE
                              class C
                              {
                                      int _value;
                              }
                              #endif
                              """;
        var parseOptions = CSharpParseOptions.Default.WithPreprocessorSymbols("FEATURE");

        foreach (var endOfLine in _lineEndings)
        {
            var text = NormalizeLineEndings(source, endOfLine);

            // Act
            var withSymbol = PhaseStabilityCheck.FindChangingPhases(text, endOfLine, parseOptions);
            var withoutSymbol = PhaseStabilityCheck.FindChangingPhases(text, endOfLine, null);

            // Assert
            Assert.Contains(nameof(IndentationPhase), withSymbol.Select(change => change.Phase).ToList(), $"Under {DescribeLineEnding(endOfLine)} line endings.");
            Assert.IsEmpty(withoutSymbol, $"Under {DescribeLineEnding(endOfLine)} line endings.");
        }
    }

    /// <summary>
    /// Verifies that a delimited documentation comment that leads a using directive of the same group, with a blank line
    /// above it, passes the check: the using phase keeps that blank line, so no phase run alone changes the output
    /// </summary>
    [TestMethod]
    [Requirement("FMT-USING-010")]
    public void CheckFindsNoChangeAboveDelimitedDocumentationLedUsingDirectiveOfTheSameGroup()
    {
        // Arrange
        const string source = """
                              using System;

                              /** doc */
                              using System.Text;
                              """;

        // Act & Assert
        AssertNoPhaseChanges(source);
    }

    /// <summary>
    /// Verifies that a single-line comment that leads a using directive of the same group, with a blank line above it,
    /// passes the check: the using phase keeps that blank line, so no phase run alone changes the output. The source is
    /// the first example of FMT-PIPE-027
    /// </summary>
    [TestMethod]
    [Requirement("FMT-USING-010")]
    [Requirement("FMT-PIPE-027")]
    public void CheckFindsNoChangeAboveCommentLedUsingDirectiveOfTheSameGroup()
    {
        // Arrange
        const string source = """
                              using System;

                              // About Linq
                              using System.Linq;

                              class C;
                              """;

        // Act & Assert
        AssertNoPhaseChanges(source);
    }

    /// <summary>
    /// Verifies that an argument the line-break phase keeps on a line of its own and the indentation phase keeps in the
    /// column of the first argument is formatter output that no phase run alone changes. The source is the second example
    /// of FMT-PIPE-027
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-027")]
    public void CheckFindsNoChangeInArgumentAlignedUnderTheFirstArgument()
    {
        // Arrange
        const string source = """
                              class C
                              {
                                  void M()
                                  {
                                      Call(1,
                                           2);
                                  }
                              }
                              """;

        // Act & Assert
        AssertNoPhaseChanges(source);
    }

    /// <summary>
    /// Verifies that the output of an input the formatter formats, not only of an input it returns unchanged, is changed
    /// by no phase run alone, with the line ending the formatter detects in that output
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-027")]
    public void CheckFindsNoChangeInTheOutputOfAnInputTheFormatterFormats()
    {
        // Arrange
        const string input = "class C{void M(){int x=1;}}";

        // Act
        var output = AssertNoPhaseChangesTheOutputOf(input);

        // Assert
        Assert.AreNotEqual(input, output, "The formatter should format the input.");

        foreach (var endOfLine in _lineEndings)
        {
            // Arrange
            var inputWithLineBreak = $"class C{{{endOfLine}void M(){{int x=1;}}}}";

            // Act
            var outputWithLineBreak = AssertNoPhaseChangesTheOutputOf(inputWithLineBreak);

            // Assert
            Assert.AreNotEqual(inputWithLineBreak, outputWithLineBreak, $"The formatter should format the input under {DescribeLineEnding(endOfLine)} line endings.");
            AssertUsesLineEnding(outputWithLineBreak, endOfLine);
        }
    }

    /// <summary>
    /// Verifies that a comment-led using directive of another group, which no phase reverses, passes the check
    /// </summary>
    [TestMethod]
    public void CheckFindsNoChangeAboveCommentLedUsingDirectiveOfAnotherGroup()
    {
        // Arrange
        const string source = """
                              using System;

                              // comment
                              using Xunit;
                              """;

        // Act & Assert
        AssertNoPhaseChanges(source);
    }

    /// <summary>
    /// Verifies that the check catches a phase whose change a later phase writes back: the source is a fixed point of
    /// both phases together, yet the earlier phase, run alone, changes it
    /// </summary>
    [TestMethod]
    public void CheckCatchesAPhaseWhoseChangeALaterPhaseWritesBack()
    {
        // Arrange
        IReadOnlyList<IFormattingPhase> phases = [new IndentationStrippingPhase(), new IndentationPhase()];

        foreach (var endOfLine in _lineEndings)
        {
            var text = NormalizeLineEndings(FormattedSource, endOfLine);
            var endingName = DescribeLineEnding(endOfLine);
            var (root, context) = SecondPassInput.Create(text, endOfLine, null);

            Assert.AreEqual(text, FormattingPipeline.ExecutePhases(root, context, phases, null, CancellationToken.None).Output.ToFullString(), $"The source should be a fixed point of both phases under {endingName} line endings.");

            // Act
            var changes = PhaseStabilityCheck.FindChangingPhases(text, endOfLine, null, phases);

            // Assert
            Assert.AreSequenceEqual(new[] { nameof(IndentationStrippingPhase) }, changes.Select(change => change.Phase).ToList(), $"Under {endingName} line endings.");
            Assert.AreEqual(IndentationStrippingPhase.GetOutput(text, endOfLine), changes[0].Output, $"Under {endingName} line endings.");
        }
    }

    /// <summary>
    /// Verifies that the helper for already-formatted source runs the check on its output under both line endings
    /// </summary>
    [TestMethod]
    public void AssertRuleResultWithoutExpectationRunsTheCheck()
    {
        // Act
        AssertRuleResult(FormattedSource);

        // Assert
        AssertCheckedFixtures(FormattedSource, null);
    }

    /// <summary>
    /// Verifies that the helper with an expectation runs the check on the final output under both line endings
    /// </summary>
    [TestMethod]
    public void AssertRuleResultWithExpectationRunsTheCheck()
    {
        // Act
        AssertRuleResult(MisindentedSource, FormattedSource);

        // Assert
        AssertCheckedFixtures(FormattedSource, null);
    }

    /// <summary>
    /// Verifies that the second-pass helper runs the check on the final output under both line endings
    /// </summary>
    [TestMethod]
    public void AssertSecondPassIsStableRunsTheCheck()
    {
        // Act
        AssertSecondPassIsStable(MisindentedSource);

        // Assert
        AssertCheckedFixtures(FormattedSource, null);
    }

    /// <summary>
    /// Verifies that the helpers hand the fixture's parse options to the check
    /// </summary>
    [TestMethod]
    public void HelpersRunTheCheckWithTheFixtureParseOptions()
    {
        // Arrange
        const string source = """
                              #if FEATURE
                              class C
                              {
                                  int _value;
                              }
                              #endif
                              """;
        var parseOptions = CSharpParseOptions.Default.WithPreprocessorSymbols("FEATURE");

        // Act
        AssertRuleResult(source, parseOptions: parseOptions);
        AssertSecondPassIsStable(source, parseOptions);

        // Assert
        var checkedFixtures = PhaseStabilityCheck.GetCheckedFixtures();

        Assert.HasCount(4, checkedFixtures);
        Assert.IsTrue(checkedFixtures.All(fixture => ReferenceEquals(fixture.ParseOptions, parseOptions)), "Every check should parse with the fixture's parse options.");
    }

    /// <summary>
    /// Verifies that the helper for already-formatted source fails when a phase changes the output, naming the phase, the
    /// line ending, and the first changed line
    /// </summary>
    [TestMethod]
    public void AssertRuleResultWithoutExpectationFailsOnAPhaseThatChangesTheOutput()
    {
        // Arrange
        PhaseStabilityCheck.OverrideForCurrentTest(CreateTextChangingPhases);

        // Act
        var failure = Assert.Throws<AssertFailedException>(() => AssertRuleResult(FormattedSource));

        // Assert
        AssertChangeFailure(failure);
    }

    /// <summary>
    /// Verifies that the helper with an expectation fails when a phase changes the final output
    /// </summary>
    [TestMethod]
    public void AssertRuleResultWithExpectationFailsOnAPhaseThatChangesTheOutput()
    {
        // Arrange
        PhaseStabilityCheck.OverrideForCurrentTest(CreateTextChangingPhases);

        // Act
        var failure = Assert.Throws<AssertFailedException>(() => AssertRuleResult(MisindentedSource, FormattedSource));

        // Assert
        AssertChangeFailure(failure);
    }

    /// <summary>
    /// Verifies that the second-pass helper fails when a phase changes the final output
    /// </summary>
    [TestMethod]
    public void AssertSecondPassIsStableFailsOnAPhaseThatChangesTheOutput()
    {
        // Arrange
        PhaseStabilityCheck.OverrideForCurrentTest(CreateTextChangingPhases);

        // Act
        var failure = Assert.Throws<AssertFailedException>(() => AssertSecondPassIsStable(MisindentedSource));

        // Assert
        AssertChangeFailure(failure);
    }

    /// <summary>
    /// Creates a phase list with only <see cref="TextChangingPhase"/>
    /// </summary>
    /// <returns>The phases</returns>
    private static IReadOnlyList<IFormattingPhase> CreateTextChangingPhases()
    {
        return [new TextChangingPhase()];
    }

    /// <summary>
    /// Asserts that a helper failed on the change of <see cref="TextChangingPhase"/> to <see cref="FormattedSource"/> under
    /// LF line endings, the first line ending the helpers check
    /// </summary>
    /// <param name="failure">The failure</param>
    private static void AssertChangeFailure(AssertFailedException failure)
    {
        Assert.Contains($"{nameof(TextChangingPhase)} changes the final output under LF line endings when it runs alone", failure.Message);
        Assert.Contains($"phase returned \"{TextChangingPhase.AppendedComment}\"", failure.Message);
    }

    /// <summary>
    /// Asserts, under LF and CRLF, that the source is a fixed point of <see cref="ReihitsuFormatter.FormatSyntaxTree"/> and
    /// that no phase changes it when it runs alone
    /// </summary>
    /// <param name="source">The source, a final output of the formatter</param>
    private static void AssertNoPhaseChanges(string source)
    {
        foreach (var endOfLine in _lineEndings)
        {
            var text = NormalizeLineEndings(source, endOfLine);

            var output = AssertNoPhaseChangesTheOutputOf(text);

            Assert.AreEqual(text, output, $"The source should be a final output of the formatter under {DescribeLineEnding(endOfLine)} line endings.");
        }
    }

    /// <summary>
    /// Formats the input with <see cref="ReihitsuFormatter.FormatSyntaxTree"/> and asserts that no phase changes the output
    /// when it runs alone on it, parsed with the parse options of the input's tree and formatted with the line ending
    /// detected in the output
    /// </summary>
    /// <param name="input">The input</param>
    /// <returns>The output of the formatter</returns>
    private static string AssertNoPhaseChangesTheOutputOf(string input)
    {
        var tree = CSharpSyntaxTree.ParseText(input);
        var outputRoot = ReihitsuFormatter.FormatSyntaxTree(tree).Output.GetRoot();
        var output = outputRoot.ToFullString();
        var endOfLine = ReihitsuFormatterHelpers.DetectEndOfLine(outputRoot);

        var changes = PhaseStabilityCheck.FindChangingPhases(output, endOfLine, (CSharpParseOptions)tree.Options);

        Assert.IsEmpty(changes, $"Under {DescribeLineEnding(endOfLine)} line endings: {string.Join(" ", changes.Select(change => $"{change.Phase}: {change.Difference}"))}");

        return output;
    }

    /// <summary>
    /// Asserts that the check ran on the given final output once under LF and once under CRLF, in that order
    /// </summary>
    /// <param name="finalOutput">The final output</param>
    /// <param name="parseOptions">The parse options the check should have used</param>
    private static void AssertCheckedFixtures(string finalOutput, CSharpParseOptions parseOptions)
    {
        var checkedFixtures = PhaseStabilityCheck.GetCheckedFixtures();

        Assert.HasCount(2, checkedFixtures);
        Assert.AreEqual((NormalizeLineEndings(finalOutput, "\n"), "\n", parseOptions), checkedFixtures[0]);
        Assert.AreEqual((NormalizeLineEndings(finalOutput, "\r\n"), "\r\n", parseOptions), checkedFixtures[1]);
    }

    #endregion // Methods
}