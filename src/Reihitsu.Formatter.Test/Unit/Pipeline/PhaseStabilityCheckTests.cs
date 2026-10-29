using System.Collections.Generic;
using System.Reflection;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Pipeline.BlankLines;
using Reihitsu.Formatter.Pipeline.Indentation;
using Reihitsu.Formatter.Pipeline.LineBreaks;
using Reihitsu.Formatter.Pipeline.UsingDirectives;
using Reihitsu.Formatter.Test.Helpers;
using Reihitsu.Formatter.Test.Helpers.Enumerations;

namespace Reihitsu.Formatter.Test.Unit.Pipeline;

/// <summary>
/// Tests for the phase-level stability check of the formatter test helpers: every production phase, run alone on the
/// final output of a fixture, must leave it unchanged, apart from the reversals the exception list names
/// </summary>
[TestClass]
public class PhaseStabilityCheckTests : FormatterTestsBase
{
    #region Constants

    /// <summary>
    /// The name of the test the recorder tests attribute their fixtures to
    /// </summary>
    private const string RecorderTestName = "Sample.Tests.Fixture";

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

    /// <summary>
    /// A second source whose only defect is the indentation of one member
    /// </summary>
    private const string OtherMisindentedSource = """
                                                  class D
                                                  {
                                                          int _other;
                                                  }
                                                  """;

    #endregion // Constants

    #region Methods

    /// <summary>
    /// Verifies that the check runs the phases of the production pipeline, in the order the pipeline runs them, so a phase
    /// added to or removed from the pipeline is checked without a second list to update
    /// </summary>
    [TestMethod]
    public void CheckRunsTheProductionPhasesInPipelineOrder()
    {
        // Arrange
        var observedNames = ObservedPipeline.Run(FormattedSource, "\n").Select(phase => phase.Name).ToList();

        // Act
        var checkedNames = PhaseStabilityCheck.CreateProductionPhases().Select(phase => phase.GetType().Name).ToList();

        // Assert
        Assert.AreSequenceEqual(observedNames, checkedNames);
        Assert.HasCount(checkedNames.Count, checkedNames.Distinct(), "Every production phase should be checked once.");
    }

    /// <summary>
    /// Verifies that no phase changes formatted source
    /// </summary>
    [TestMethod]
    public void CheckFindsNoChangeInFormattedSource()
    {
        foreach (var endOfLine in _lineEndings)
        {
            // Act
            var changes = PhaseStabilityCheck.FindChangingPhases(NormalizeLineEndings(FormattedSource, endOfLine), endOfLine, null);

            // Assert
            Assert.IsEmpty(changes, $"Under {DescribeLineEnding(endOfLine)} line endings.");
        }
    }

    /// <summary>
    /// Verifies that the check finds the phase that changes source when that phase runs alone
    /// </summary>
    [TestMethod]
    public void CheckFindsThePhaseThatChangesTheSource()
    {
        foreach (var endOfLine in _lineEndings)
        {
            // Act
            var changes = PhaseStabilityCheck.FindChangingPhases(NormalizeLineEndings(MisindentedSource, endOfLine), endOfLine, null);

            // Assert
            Assert.AreSequenceEqual(new[] { nameof(IndentationPhase) }, changes.Select(change => change.Phase).ToList(), $"Under {DescribeLineEnding(endOfLine)} line endings.");
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
    /// Verifies that the check catches the using phase removing the blank line above a delimited documentation comment
    /// that leads a using directive of the same group, which the blank-line phase inserts again
    /// </summary>
    [TestMethod]
    public void CheckCatchesUsingPhaseRemovingBlankLineAboveDelimitedDocumentationLedUsingDirective()
    {
        // Arrange
        const string source = """
                              using System;

                              /** doc */
                              using System.Text;
                              """;

        // Act & Assert
        AssertOnlyPhaseChanges(source, nameof(UsingDirectiveOrderingPhase));
    }

    /// <summary>
    /// Verifies that the check catches the using phase removing the blank line above a single-line comment that leads a
    /// using directive of the same group, which the blank-line phase inserts again
    /// </summary>
    [TestMethod]
    public void CheckCatchesUsingPhaseRemovingBlankLineAboveCommentLedUsingDirective()
    {
        // Arrange
        const string source = """
                              using System;

                              // comment
                              using System.Text;
                              """;

        // Act & Assert
        AssertOnlyPhaseChanges(source, nameof(UsingDirectiveOrderingPhase));
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
        AssertOnlyPhaseChanges(source);
    }

    /// <summary>
    /// Verifies that the check catches the blank-line phase inserting a blank line between a block statement and the
    /// <c>}</c> of the statement before it, which the line-break phase removes again
    /// </summary>
    [TestMethod]
    public void CheckCatchesBlankLinePhaseInsertingBlankLineBeforeBlockAfterClosingBrace()
    {
        // Arrange
        const string source = """
                              class C
                              {
                                  void M(bool b)
                                  {
                                      if (b)
                                      {
                                      }
                                      {
                                      }
                                  }
                              }
                              """;

        // Act & Assert
        AssertOnlyPhaseChanges(source, nameof(BlankLinePhase));
    }

    /// <summary>
    /// Verifies that the check catches the blank-line phase inserting a blank line above an own-line comment glued to the
    /// <c>{</c> behind it, which the line-break phase removes again
    /// </summary>
    [TestMethod]
    public void CheckCatchesBlankLinePhaseInsertingBlankLineAboveCommentGluedToOpeningBrace()
    {
        // Arrange
        const string source = """
                              class C
                              {
                                  void M()
                                  {
                                      var p = new P
                                              /* o */ {
                                                  X = 2
                                              };
                                  }
                              }
                              """;

        // Act & Assert
        AssertOnlyPhaseChanges(source, nameof(BlankLinePhase));
    }

    /// <summary>
    /// Verifies that the check catches the line-break phase stripping the indentation of the <c>&gt;</c> behind an
    /// omitted type argument, which the indentation phase writes back
    /// </summary>
    [TestMethod]
    public void CheckCatchesLineBreakPhaseStrippingIndentationOfClosingAngleBracketBehindOmittedTypeArgument()
    {
        // Arrange
        const string source = """
                              using System.Collections.Generic;

                              public class Sample
                              {
                                  public void Foo()
                                  {
                                      var type = typeof(Dictionary<,
                                              >);
                                  }
                              }
                              """;

        // Act & Assert
        AssertOnlyPhaseChanges(source, nameof(LineBreakPhase));
    }

    /// <summary>
    /// Verifies that the check catches the line-break phase stripping the indentation of a parenthesized lambda argument
    /// on its own line, which the indentation phase writes back
    /// </summary>
    [TestMethod]
    public void CheckCatchesLineBreakPhaseStrippingIndentationOfParenthesizedLambdaArgument()
    {
        // Arrange
        const string source = """
                              class C
                              {
                                  void M()
                                  {
                                      Run(1,
                                          (x) => x);
                                  }
                              }
                              """;

        // Act & Assert
        AssertOnlyPhaseChanges(source, nameof(LineBreakPhase));
    }

    /// <summary>
    /// Verifies that one fingerprint names a fixture under both line endings and that different outputs get different
    /// fingerprints
    /// </summary>
    [TestMethod]
    public void FingerprintIgnoresLineEndingsAndTellsFixturesApart()
    {
        // Act
        var lineFeed = PhaseStabilityCheck.ComputeFingerprint(FormattedSource.Replace("\r\n", "\n"));
        var carriageReturnLineFeed = PhaseStabilityCheck.ComputeFingerprint(NormalizeLineEndings(FormattedSource, "\r\n"));
        var other = PhaseStabilityCheck.ComputeFingerprint(MisindentedSource);

        // Assert
        Assert.AreEqual(lineFeed, carriageReturnLineFeed);
        Assert.AreNotEqual(lineFeed, other);
        Assert.AreEqual(16, lineFeed.Length);
    }

    /// <summary>
    /// Verifies that a phase that changes a fixture without an exception fails with the fixture, the phase, and the line
    /// ending
    /// </summary>
    [TestMethod]
    public void UnlistedChangeFailsNamingFixturePhaseAndLineEnding()
    {
        foreach (var endOfLine in _lineEndings)
        {
            // Arrange
            var recorder = new PhaseStabilityRecorder(RecorderTestName, []);
            var text = NormalizeLineEndings(MisindentedSource, endOfLine);
            var endingName = DescribeLineEnding(endOfLine);

            // Act
            var failures = recorder.Check(text, endOfLine, null);

            // Assert
            Assert.HasCount(1, failures);
            Assert.Contains(nameof(IndentationPhase), failures[0]);
            Assert.Contains(RecorderTestName, failures[0]);
            Assert.Contains(PhaseStabilityCheck.ComputeFingerprint(text), failures[0]);
            Assert.Contains($"under {endingName} line endings", failures[0]);
        }
    }

    /// <summary>
    /// Verifies that a listed reversal passes under every line ending its entry covers, and that the entry is reached
    /// </summary>
    [TestMethod]
    public void ListedChangePasses()
    {
        // Arrange
        var exception = CreateException(RecorderTestName, MisindentedSource, nameof(IndentationPhase), PhaseReversalLineEndings.Both);
        var recorder = new PhaseStabilityRecorder(RecorderTestName, [exception]);

        foreach (var endOfLine in _lineEndings)
        {
            // Act
            var failures = recorder.Check(NormalizeLineEndings(MisindentedSource, endOfLine), endOfLine, null);

            // Assert
            Assert.IsEmpty(failures, $"Under {DescribeLineEnding(endOfLine)} line endings.");
        }

        Assert.IsEmpty(recorder.FindUnreachedExceptions());
    }

    /// <summary>
    /// Verifies that an entry whose phase no longer changes its fixture fails as stale, naming the line ending
    /// </summary>
    [TestMethod]
    public void EntryWhosePhaseNoLongerChangesTheFixtureFailsAsStale()
    {
        foreach (var endOfLine in _lineEndings)
        {
            // Arrange
            var exception = CreateException(RecorderTestName, FormattedSource, nameof(BlankLinePhase), PhaseReversalLineEndings.Both);
            var recorder = new PhaseStabilityRecorder(RecorderTestName, [exception]);

            // Act
            var failures = recorder.Check(NormalizeLineEndings(FormattedSource, endOfLine), endOfLine, null);

            // Assert
            Assert.HasCount(1, failures);
            Assert.Contains("Stale phase reversal exception", failures[0]);
            Assert.Contains(nameof(BlankLinePhase), failures[0]);
            Assert.Contains($"under {DescribeLineEnding(endOfLine)} line endings", failures[0]);
        }
    }

    /// <summary>
    /// Verifies that an entry listing a different phase than the one that changes the fixture fails twice: once for the
    /// unlisted change and once as stale
    /// </summary>
    [TestMethod]
    public void EntryForAnotherPhaseDoesNotHideTheChange()
    {
        // Arrange
        var exception = CreateException(RecorderTestName, MisindentedSource, nameof(BlankLinePhase), PhaseReversalLineEndings.Both);
        var recorder = new PhaseStabilityRecorder(RecorderTestName, [exception]);

        // Act
        var failures = recorder.Check(MisindentedSource.Replace("\r\n", "\n"), "\n", null);

        // Assert
        Assert.HasCount(2, failures);
        Assert.Contains(failure => failure.StartsWith(nameof(IndentationPhase), StringComparison.Ordinal), failures, "The unlisted change should be reported.");
        Assert.Contains(failure => failure.StartsWith("Stale phase reversal exception", StringComparison.Ordinal), failures, "The stale entry should be reported.");
    }

    /// <summary>
    /// Verifies that an entry covering LF only hides the change under LF and not under CRLF
    /// </summary>
    [TestMethod]
    public void EntryCoversOnlyTheLineEndingsItNames()
    {
        // Arrange
        var exception = CreateException(RecorderTestName, MisindentedSource, nameof(IndentationPhase), PhaseReversalLineEndings.Lf);
        var recorder = new PhaseStabilityRecorder(RecorderTestName, [exception]);

        // Act
        var lineFeedFailures = recorder.Check(NormalizeLineEndings(MisindentedSource, "\n"), "\n", null);
        var carriageReturnLineFeedFailures = recorder.Check(NormalizeLineEndings(MisindentedSource, "\r\n"), "\r\n", null);

        // Assert
        Assert.IsEmpty(lineFeedFailures);
        Assert.HasCount(1, carriageReturnLineFeedFailures);
        Assert.Contains("under CRLF line endings", carriageReturnLineFeedFailures[0]);
        Assert.IsEmpty(recorder.FindUnreachedExceptions());
    }

    /// <summary>
    /// Verifies that an entry no fixture of its test reached fails as stale under every line ending it covers
    /// </summary>
    [TestMethod]
    public void EntryNoFixtureReachedFailsAsStale()
    {
        // Arrange
        var exception = CreateException(RecorderTestName, MisindentedSource, nameof(IndentationPhase), PhaseReversalLineEndings.Both);
        var recorder = new PhaseStabilityRecorder(RecorderTestName, [exception]);

        recorder.Check(NormalizeLineEndings(FormattedSource, "\n"), "\n", null);

        // Act
        var failures = recorder.FindUnreachedExceptions();

        // Assert
        Assert.HasCount(2, failures);
        Assert.Contains("under LF line endings", failures[0]);
        Assert.Contains("under CRLF line endings", failures[1]);
    }

    /// <summary>
    /// Verifies that an entry reached under one of the line endings it covers still fails as stale for the other
    /// </summary>
    [TestMethod]
    public void EntryReachedUnderOneLineEndingFailsAsStaleForTheOther()
    {
        // Arrange
        var exception = CreateException(RecorderTestName, MisindentedSource, nameof(IndentationPhase), PhaseReversalLineEndings.Both);
        var recorder = new PhaseStabilityRecorder(RecorderTestName, [exception]);

        recorder.Check(NormalizeLineEndings(MisindentedSource, "\n"), "\n", null);

        // Act
        var failures = recorder.FindUnreachedExceptions();

        // Assert
        Assert.HasCount(1, failures);
        Assert.Contains("under CRLF line endings", failures[0]);
    }

    /// <summary>
    /// Verifies that an entry of another test does not cover the fixtures of this test and is not stale for it
    /// </summary>
    [TestMethod]
    public void EntryOfAnotherTestIsIgnored()
    {
        // Arrange
        var exception = CreateException("Sample.Tests.OtherFixture", MisindentedSource, nameof(IndentationPhase), PhaseReversalLineEndings.Both);
        var recorder = new PhaseStabilityRecorder(RecorderTestName, [exception]);

        // Act
        var failures = recorder.Check(NormalizeLineEndings(MisindentedSource, "\n"), "\n", null);

        // Assert
        Assert.HasCount(1, failures);
        Assert.Contains(nameof(IndentationPhase), failures[0]);
        Assert.IsEmpty(recorder.FindUnreachedExceptions());
    }

    /// <summary>
    /// Verifies that the entry of one fixture of a test does not cover another fixture of the same test
    /// </summary>
    [TestMethod]
    public void EntryOfOneFixtureDoesNotCoverAnotherFixtureOfTheSameTest()
    {
        // Arrange
        var exception = CreateException(RecorderTestName, MisindentedSource, nameof(IndentationPhase), PhaseReversalLineEndings.Both);
        var recorder = new PhaseStabilityRecorder(RecorderTestName, [exception]);

        // Act
        var listedFailures = recorder.Check(NormalizeLineEndings(MisindentedSource, "\n"), "\n", null);
        var otherFailures = recorder.Check(NormalizeLineEndings(OtherMisindentedSource, "\n"), "\n", null);

        // Assert
        Assert.IsEmpty(listedFailures);
        Assert.HasCount(1, otherFailures);
        Assert.Contains(PhaseStabilityCheck.ComputeFingerprint(OtherMisindentedSource), otherFailures[0]);
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
    /// Verifies that every exception names a test method of the formatter test suite and a known cause, and that no
    /// fixture and phase is listed twice
    /// </summary>
    [TestMethod]
    public void EveryExceptionNamesATestMethodAndAKnownCause()
    {
        // Arrange
        var causes = typeof(PhaseReversalCauses).GetFields(BindingFlags.Public | BindingFlags.Static)
                                                .Select(field => (string)field.GetRawConstantValue())
                                                .ToList();
        var phaseNames = PhaseStabilityCheck.CreateProductionPhases().Select(phase => phase.GetType().Name).ToList();

        foreach (var exception in PhaseReversalExceptions.Entries)
        {
            // Act
            var method = exception.TestClass.GetMethod(exception.TestMethod, BindingFlags.Public | BindingFlags.Instance);

            // Assert
            Assert.IsNotNull(method, $"{exception} names no public test method.");
            Assert.IsTrue(method.IsDefined(typeof(TestMethodAttribute), inherit: true), $"{exception} names a method that is not a test.");
            Assert.IsTrue(typeof(FormatterTestsBase).IsAssignableFrom(exception.TestClass), $"{exception} names a class whose tests do not run the check.");
            Assert.Contains(exception.Phase, phaseNames, $"{exception} names no production phase.");
            Assert.Contains(exception.Cause, causes, $"{exception} names no known cause.");
        }

        var keys = PhaseReversalExceptions.Entries.Select(exception => (exception.TestName, exception.Fingerprint, exception.Phase)).ToList();

        Assert.HasCount(keys.Count, keys.Distinct(), "A fixture and phase is listed twice.");
    }

    /// <summary>
    /// Creates an exception for the final output of a fixture
    /// </summary>
    /// <param name="testName">The qualified name of the test that formats the fixture</param>
    /// <param name="finalOutput">The final output of the fixture</param>
    /// <param name="phase">The phase that changes it</param>
    /// <param name="lineEndings">The line endings the exception covers</param>
    /// <returns>The exception</returns>
    private static PhaseReversalException CreateException(string testName, string finalOutput, string phase, PhaseReversalLineEndings lineEndings)
    {
        return new PhaseReversalException(testName, PhaseStabilityCheck.ComputeFingerprint(finalOutput), phase, lineEndings, PhaseReversalCauses.LineStartIndentation);
    }

    /// <summary>
    /// Asserts that the given phases, and no others, change the source when they run alone, under LF and CRLF
    /// </summary>
    /// <param name="source">The source, a final output of the formatter</param>
    /// <param name="phases">The phases expected to change it</param>
    private static void AssertOnlyPhaseChanges(string source, params string[] phases)
    {
        foreach (var endOfLine in _lineEndings)
        {
            var text = NormalizeLineEndings(source, endOfLine);
            var endingName = DescribeLineEnding(endOfLine);

            AssertSecondPassIsStableWithoutCheck(text, endOfLine);

            var changes = PhaseStabilityCheck.FindChangingPhases(text, endOfLine, null);

            Assert.AreSequenceEqual(phases, changes.Select(change => change.Phase).ToList(), $"Under {endingName} line endings: {string.Join(" ", changes.Select(change => change.Difference))}");
        }
    }

    /// <summary>
    /// Asserts that the source is a fixed point of the whole pipeline, so a phase that changes it alone is a reversal
    /// </summary>
    /// <param name="text">The source</param>
    /// <param name="endOfLine">The end-of-line sequence to format with</param>
    private static void AssertSecondPassIsStableWithoutCheck(string text, string endOfLine)
    {
        var tree = CSharpSyntaxTree.ParseText(text);
        var context = new Data.FormattingContext(endOfLine, languageVersion: Utilities.LanguageVersionResolver.Resolve(tree.Options));
        var formatted = Formatter.Pipeline.FormattingPipeline.Execute(tree.GetRoot(), context, System.Threading.CancellationToken.None).ToFullString();

        Assert.AreEqual(text, formatted, $"The source should be a final output of the formatter under {DescribeLineEnding(endOfLine)} line endings.");
    }

    /// <summary>
    /// Asserts that the check ran on the given final output once under LF and once under CRLF, in that order
    /// </summary>
    /// <param name="finalOutput">The final output</param>
    /// <param name="parseOptions">The parse options the check should have used</param>
    private static void AssertCheckedFixtures(string finalOutput, CSharpParseOptions parseOptions)
    {
        var checkedFixtures = PhaseStabilityCheck.GetCheckedFixtures();
        var fingerprint = PhaseStabilityCheck.ComputeFingerprint(finalOutput);

        Assert.HasCount(2, checkedFixtures);
        Assert.AreEqual((fingerprint, PhaseReversalLineEndings.Lf, parseOptions), checkedFixtures[0]);
        Assert.AreEqual((fingerprint, PhaseReversalLineEndings.Crlf, parseOptions), checkedFixtures[1]);
    }

    #endregion // Methods
}