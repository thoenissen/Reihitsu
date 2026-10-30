using System.Collections.Generic;
using System.Reflection;
using System.Threading;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Pipeline;
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

    #region Fields

    /// <summary>
    /// A cause for the recorder tests whose phase is the indentation phase
    /// </summary>
    private static readonly PhaseReversalCause _indentationCause = new(nameof(IndentationPhase), "Test cause of the indentation phase.");

    /// <summary>
    /// A cause for the helper tests whose phase is <see cref="TextChangingPhase"/>
    /// </summary>
    private static readonly PhaseReversalCause _textChangingCause = new(nameof(TextChangingPhase), "Test cause of the text-changing phase.");

    #endregion // Fields

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
            Assert.AreEqual(PhaseStabilityCheck.ComputeFingerprint(FormattedSource), changes[0].OutputFingerprint);
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
        const string phaseOutput = """
                                   using System;
                                   /** doc */
                                   using System.Text;
                                   """;

        // Act & Assert
        AssertOnlyPhaseChanges(source, nameof(UsingDirectiveOrderingPhase), phaseOutput);
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
        const string phaseOutput = """
                                   using System;
                                   // comment
                                   using System.Text;
                                   """;

        // Act & Assert
        AssertOnlyPhaseChanges(source, nameof(UsingDirectiveOrderingPhase), phaseOutput);
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
        const string phaseOutput = """
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
        AssertOnlyPhaseChanges(source, nameof(BlankLinePhase), phaseOutput);
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
        const string phaseOutput = """
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
        AssertOnlyPhaseChanges(source, nameof(BlankLinePhase), phaseOutput);
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
        const string phaseOutput = """
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
        AssertOnlyPhaseChanges(source, nameof(LineBreakPhase), phaseOutput);
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
        const string phaseOutput = """
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
        AssertOnlyPhaseChanges(source, nameof(LineBreakPhase), phaseOutput);
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
    /// Verifies that a phase that changes a fixture without an entry fails with the fixture, the phase, the phase's output,
    /// and the line ending
    /// </summary>
    [TestMethod]
    public void UnlistedChangeFailsNamingFixturePhaseOutputAndLineEnding()
    {
        foreach (var endOfLine in _lineEndings)
        {
            // Arrange
            var recorder = new PhaseStabilityRecorder(RecorderTestName, []);
            var text = NormalizeLineEndings(MisindentedSource, endOfLine);

            // Act
            var failures = recorder.Check(text, endOfLine, null);

            // Assert
            Assert.HasCount(1, failures);
            Assert.Contains(nameof(IndentationPhase), failures[0]);
            Assert.Contains(RecorderTestName, failures[0]);
            Assert.Contains(PhaseStabilityCheck.ComputeFingerprint(text), failures[0]);
            Assert.Contains(PhaseStabilityCheck.ComputeFingerprint(FormattedSource), failures[0]);
            Assert.Contains($"under {DescribeLineEnding(endOfLine)} line endings", failures[0]);
        }
    }

    /// <summary>
    /// Verifies that a listed reversal passes under every line ending its entry covers, and that the entry is reached
    /// </summary>
    [TestMethod]
    public void ListedChangePasses()
    {
        // Arrange
        var entry = CreateEntry(MisindentedSource, _indentationCause, FormattedSource, PhaseReversalLineEndings.Both);
        var recorder = new PhaseStabilityRecorder(RecorderTestName, [entry]);

        foreach (var endOfLine in _lineEndings)
        {
            // Act
            var failures = recorder.Check(NormalizeLineEndings(MisindentedSource, endOfLine), endOfLine, null);

            // Assert
            Assert.IsEmpty(failures, $"Under {DescribeLineEnding(endOfLine)} line endings.");
        }

        Assert.IsEmpty(recorder.FindUnreachedEntries());
    }

    /// <summary>
    /// Verifies that an entry whose phase makes a different change than the one the entry records does not hide the
    /// change: the change fails as unlisted and the entry as stale. This is what a listed fixture shows when its phase
    /// gains a second change or makes another change instead
    /// </summary>
    [TestMethod]
    public void EntryForTheSamePhaseWithAnotherOutputDoesNotHideTheChange()
    {
        foreach (var endOfLine in _lineEndings)
        {
            // Arrange
            var entry = CreateEntry(MisindentedSource, _indentationCause, OtherMisindentedSource, PhaseReversalLineEndings.Both);
            var recorder = new PhaseStabilityRecorder(RecorderTestName, [entry]);

            // Act
            var failures = recorder.Check(NormalizeLineEndings(MisindentedSource, endOfLine), endOfLine, null);

            // Assert
            Assert.HasCount(2, failures, $"Under {DescribeLineEnding(endOfLine)} line endings.");
            Assert.Contains(failure => failure.StartsWith(nameof(IndentationPhase), StringComparison.Ordinal), failures, "The unlisted change should be reported.");
            Assert.Contains(failure => failure.StartsWith("Stale phase reversal entry", StringComparison.Ordinal), failures, "The stale entry should be reported.");
        }
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
            var entry = CreateEntry(FormattedSource, PhaseReversalCauses.BlankLineNextToBrace, MisindentedSource, PhaseReversalLineEndings.Both);
            var recorder = new PhaseStabilityRecorder(RecorderTestName, [entry]);

            // Act
            var failures = recorder.Check(NormalizeLineEndings(FormattedSource, endOfLine), endOfLine, null);

            // Assert
            Assert.HasCount(1, failures);
            Assert.Contains("Stale phase reversal entry", failures[0]);
            Assert.Contains(nameof(BlankLinePhase), failures[0]);
            Assert.Contains($"under {DescribeLineEnding(endOfLine)} line endings", failures[0]);
        }
    }

    /// <summary>
    /// Verifies that an entry of another phase does not hide the change: the change fails as unlisted and the entry as
    /// stale
    /// </summary>
    [TestMethod]
    public void EntryForAnotherPhaseDoesNotHideTheChange()
    {
        // Arrange
        var entry = CreateEntry(MisindentedSource, PhaseReversalCauses.BlankLineNextToBrace, FormattedSource, PhaseReversalLineEndings.Both);
        var recorder = new PhaseStabilityRecorder(RecorderTestName, [entry]);

        // Act
        var failures = recorder.Check(MisindentedSource.Replace("\r\n", "\n"), "\n", null);

        // Assert
        Assert.HasCount(2, failures);
        Assert.Contains(failure => failure.StartsWith(nameof(IndentationPhase), StringComparison.Ordinal), failures, "The unlisted change should be reported.");
        Assert.Contains(failure => failure.StartsWith("Stale phase reversal entry", StringComparison.Ordinal), failures, "The stale entry should be reported.");
    }

    /// <summary>
    /// Verifies that an entry covering LF only hides the change under LF and not under CRLF
    /// </summary>
    [TestMethod]
    public void EntryCoversOnlyTheLineEndingsItNames()
    {
        // Arrange
        var entry = CreateEntry(MisindentedSource, _indentationCause, FormattedSource, PhaseReversalLineEndings.Lf);
        var recorder = new PhaseStabilityRecorder(RecorderTestName, [entry]);

        // Act
        var lineFeedFailures = recorder.Check(NormalizeLineEndings(MisindentedSource, "\n"), "\n", null);
        var carriageReturnLineFeedFailures = recorder.Check(NormalizeLineEndings(MisindentedSource, "\r\n"), "\r\n", null);

        // Assert
        Assert.IsEmpty(lineFeedFailures);
        Assert.HasCount(1, carriageReturnLineFeedFailures);
        Assert.Contains("under CRLF line endings", carriageReturnLineFeedFailures[0]);
        Assert.IsEmpty(recorder.FindUnreachedEntries());
    }

    /// <summary>
    /// Verifies that an entry no fixture of its test reached fails as stale under every line ending it covers
    /// </summary>
    [TestMethod]
    public void EntryNoFixtureReachedFailsAsStale()
    {
        // Arrange
        var entry = CreateEntry(MisindentedSource, _indentationCause, FormattedSource, PhaseReversalLineEndings.Both);
        var recorder = new PhaseStabilityRecorder(RecorderTestName, [entry]);

        recorder.Check(NormalizeLineEndings(FormattedSource, "\n"), "\n", null);

        // Act
        var failures = recorder.FindUnreachedEntries();

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
        var entry = CreateEntry(MisindentedSource, _indentationCause, FormattedSource, PhaseReversalLineEndings.Both);
        var recorder = new PhaseStabilityRecorder(RecorderTestName, [entry]);

        recorder.Check(NormalizeLineEndings(MisindentedSource, "\n"), "\n", null);

        // Act
        var failures = recorder.FindUnreachedEntries();

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
        var entry = new PhaseReversalEntry("Sample.Tests.OtherFixture", PhaseStabilityCheck.ComputeFingerprint(MisindentedSource), _indentationCause, PhaseStabilityCheck.ComputeFingerprint(FormattedSource), PhaseReversalLineEndings.Both);
        var recorder = new PhaseStabilityRecorder(RecorderTestName, [entry]);

        // Act
        var failures = recorder.Check(NormalizeLineEndings(MisindentedSource, "\n"), "\n", null);

        // Assert
        Assert.HasCount(1, failures);
        Assert.Contains(nameof(IndentationPhase), failures[0]);
        Assert.IsEmpty(recorder.FindUnreachedEntries());
    }

    /// <summary>
    /// Verifies that the entry of one fixture of a test does not cover another fixture of the same test
    /// </summary>
    [TestMethod]
    public void EntryOfOneFixtureDoesNotCoverAnotherFixtureOfTheSameTest()
    {
        // Arrange
        var entry = CreateEntry(MisindentedSource, _indentationCause, FormattedSource, PhaseReversalLineEndings.Both);
        var recorder = new PhaseStabilityRecorder(RecorderTestName, [entry]);

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
    /// Verifies that the helper for already-formatted source fails when a phase changes the output without an entry,
    /// naming the phase, the fixture, and the line ending
    /// </summary>
    [TestMethod]
    public void AssertRuleResultWithoutExpectationFailsOnAnUnlistedChange()
    {
        // Arrange
        PhaseStabilityCheck.OverrideForCurrentTest([], CreateTextChangingPhases);

        // Act
        var failure = Assert.Throws<AssertFailedException>(() => AssertRuleResult(FormattedSource));

        // Assert
        AssertUnlistedChangeFailure(failure);
    }

    /// <summary>
    /// Verifies that the helper with an expectation fails when a phase changes the final output without an entry
    /// </summary>
    [TestMethod]
    public void AssertRuleResultWithExpectationFailsOnAnUnlistedChange()
    {
        // Arrange
        PhaseStabilityCheck.OverrideForCurrentTest([], CreateTextChangingPhases);

        // Act
        var failure = Assert.Throws<AssertFailedException>(() => AssertRuleResult(MisindentedSource, FormattedSource));

        // Assert
        AssertUnlistedChangeFailure(failure);
    }

    /// <summary>
    /// Verifies that the second-pass helper fails when a phase changes the final output without an entry
    /// </summary>
    [TestMethod]
    public void AssertSecondPassIsStableFailsOnAnUnlistedChange()
    {
        // Arrange
        PhaseStabilityCheck.OverrideForCurrentTest([], CreateTextChangingPhases);

        // Act
        var failure = Assert.Throws<AssertFailedException>(() => AssertSecondPassIsStable(MisindentedSource));

        // Assert
        AssertUnlistedChangeFailure(failure);
    }

    /// <summary>
    /// Verifies that the helpers pass, and the test cleanup finds nothing stale, when the entry of the test covers the
    /// change under both line endings
    /// </summary>
    [TestMethod]
    public void HelpersPassOnAListedChange()
    {
        // Arrange
        var entry = new PhaseReversalEntry(typeof(PhaseStabilityCheckTests),
                                           nameof(HelpersPassOnAListedChange),
                                           PhaseStabilityCheck.ComputeFingerprint(FormattedSource),
                                           _textChangingCause,
                                           PhaseStabilityCheck.ComputeFingerprint(TextChangingPhase.GetOutput(FormattedSource.Replace("\r\n", "\n"), "\n")),
                                           PhaseReversalLineEndings.Both);

        PhaseStabilityCheck.OverrideForCurrentTest([entry], CreateTextChangingPhases);

        // Act
        AssertRuleResult(FormattedSource);
        AssertSecondPassIsStable(MisindentedSource);

        // Assert
        PhaseStabilityCheck.EndTest();
    }

    /// <summary>
    /// Verifies that a helper fails when the entry of its fixture names a phase that no longer changes the final output.
    /// The entry covers LF only, the line ending the helper checks first, so the test cleanup finds it reached
    /// </summary>
    [TestMethod]
    public void HelperFailsOnAnEntryWhosePhaseNoLongerChangesTheFixture()
    {
        // Arrange
        var entry = new PhaseReversalEntry(typeof(PhaseStabilityCheckTests),
                                           nameof(HelperFailsOnAnEntryWhosePhaseNoLongerChangesTheFixture),
                                           PhaseStabilityCheck.ComputeFingerprint(FormattedSource),
                                           PhaseReversalCauses.BlankLineNextToBrace,
                                           PhaseStabilityCheck.ComputeFingerprint(MisindentedSource),
                                           PhaseReversalLineEndings.Lf);

        PhaseStabilityCheck.OverrideForCurrentTest([entry], PhaseStabilityCheck.CreateProductionPhases);

        // Act
        var failure = Assert.Throws<AssertFailedException>(() => AssertRuleResult(FormattedSource));

        // Assert
        Assert.Contains("Stale phase reversal entry", failure.Message);
        Assert.Contains(nameof(BlankLinePhase), failure.Message);
        Assert.Contains("under LF line endings", failure.Message);
    }

    /// <summary>
    /// Verifies that the test cleanup fails on an entry of the test that none of its fixtures reached, under every line
    /// ending the entry covers
    /// </summary>
    [TestMethod]
    public void TestCleanupFailsOnAnEntryNoFixtureReached()
    {
        // Arrange
        var entry = new PhaseReversalEntry(typeof(PhaseStabilityCheckTests),
                                           nameof(TestCleanupFailsOnAnEntryNoFixtureReached),
                                           PhaseStabilityCheck.ComputeFingerprint(MisindentedSource),
                                           PhaseReversalCauses.BlankLineNextToBrace,
                                           PhaseStabilityCheck.ComputeFingerprint(FormattedSource),
                                           PhaseReversalLineEndings.Both);

        PhaseStabilityCheck.OverrideForCurrentTest([entry], PhaseStabilityCheck.CreateProductionPhases);
        AssertRuleResult(FormattedSource);

        // Act
        var failure = Assert.Throws<AssertFailedException>(PhaseStabilityCheck.EndTest);

        // Assert
        Assert.Contains("Stale phase reversal entry", failure.Message);
        Assert.Contains(PhaseStabilityCheck.ComputeFingerprint(MisindentedSource), failure.Message);
        Assert.Contains("under LF line endings", failure.Message);
        Assert.Contains("under CRLF line endings", failure.Message);
    }

    /// <summary>
    /// Verifies that every cause names a production phase
    /// </summary>
    [TestMethod]
    public void EveryCauseNamesAProductionPhase()
    {
        // Arrange
        var phaseNames = PhaseStabilityCheck.CreateProductionPhases().Select(phase => phase.GetType().Name).ToList();

        foreach (var cause in GetKnownCauses())
        {
            // Act & Assert
            Assert.Contains(cause.Phase, phaseNames, $"The cause \"{cause.Description}\" names no production phase.");
        }
    }

    /// <summary>
    /// Verifies that a data-driven test keys its entries by data row: the entry of one row covers that row's change, and
    /// the other row, which formats the same fixture without the change, neither fails on it nor reports it as stale
    /// </summary>
    /// <param name="listedRow">Whether this row is the one the entry names</param>
    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void DataRowEntryCoversOnlyItsOwnRow(bool listedRow)
    {
        // Arrange
        var entry = new PhaseReversalEntry(typeof(PhaseStabilityCheckTests),
                                           nameof(DataRowEntryCoversOnlyItsOwnRow),
                                           PhaseStabilityCheck.ComputeFingerprint(FormattedSource),
                                           _textChangingCause,
                                           PhaseStabilityCheck.ComputeFingerprint(TextChangingPhase.GetOutput(FormattedSource.Replace("\r\n", "\n"), "\n")),
                                           PhaseReversalLineEndings.Both,
                                           "true");

        PhaseStabilityCheck.OverrideForCurrentTest([entry], listedRow ? CreateTextChangingPhases : PhaseStabilityCheck.CreateProductionPhases);

        // Act
        AssertRuleResult(FormattedSource);

        // Assert
        Assert.AreEqual(listedRow ? "true" : "false", DataRowKey.Format(TestContext.TestData));
        PhaseStabilityCheck.EndTest();
    }

    /// <summary>
    /// Verifies that data rows whose only argument is an array are keyed element by element: the entry of row
    /// <c>[1]</c> covers that row's change, and row <c>[2]</c>, which formats the same fixture without the change, neither
    /// fails on it nor reports it as stale
    /// </summary>
    /// <param name="values">The array argument of the row</param>
    [TestMethod]
    [DataRow(new[] { 1 })]
    [DataRow(new[] { 2 })]
    public void ArrayDataRowEntryCoversOnlyItsOwnRow(int[] values)
    {
        // Arrange
        var listedRow = values[0] == 1;
        var entry = new PhaseReversalEntry(typeof(PhaseStabilityCheckTests),
                                           nameof(ArrayDataRowEntryCoversOnlyItsOwnRow),
                                           PhaseStabilityCheck.ComputeFingerprint(FormattedSource),
                                           _textChangingCause,
                                           PhaseStabilityCheck.ComputeFingerprint(TextChangingPhase.GetOutput(FormattedSource.Replace("\r\n", "\n"), "\n")),
                                           PhaseReversalLineEndings.Both,
                                           "System.Int32[1]");

        PhaseStabilityCheck.OverrideForCurrentTest([entry], listedRow ? CreateTextChangingPhases : PhaseStabilityCheck.CreateProductionPhases);

        // Act
        AssertRuleResult(FormattedSource);

        // Assert
        Assert.AreEqual(listedRow ? "System.Int32[1]" : "System.Int32[2]", DataRowKey.Format(TestContext.TestData));
        PhaseStabilityCheck.EndTest();
    }

    /// <summary>
    /// Verifies that data rows of strings whose text holds a quoted separator are keyed apart: the two rows below gave the
    /// same key while strings were quoted without escaping
    /// </summary>
    /// <param name="first">The first argument</param>
    /// <param name="second">The second argument</param>
    [TestMethod]
    [DataRow("a\", \"b", "c")]
    [DataRow("a", "b\", \"c")]
    public void StringDataRowEntryCoversOnlyItsOwnRow(string first, string second)
    {
        // Arrange
        const string listedKey = "\"a\\\", \\\"b\", \"c\"";
        var listedRow = first.Length > 1;
        var entry = new PhaseReversalEntry(typeof(PhaseStabilityCheckTests),
                                           nameof(StringDataRowEntryCoversOnlyItsOwnRow),
                                           PhaseStabilityCheck.ComputeFingerprint(FormattedSource),
                                           _textChangingCause,
                                           PhaseStabilityCheck.ComputeFingerprint(TextChangingPhase.GetOutput(FormattedSource.Replace("\r\n", "\n"), "\n")),
                                           PhaseReversalLineEndings.Both,
                                           listedKey);

        PhaseStabilityCheck.OverrideForCurrentTest([entry], listedRow ? CreateTextChangingPhases : PhaseStabilityCheck.CreateProductionPhases);

        // Act
        AssertRuleResult(FormattedSource);

        // Assert
        Assert.AreEqual(listedRow, DataRowKey.Format(TestContext.TestData) == listedKey, $"Row ({first}) ({second}) has the key {DataRowKey.Format(TestContext.TestData)}.");
        PhaseStabilityCheck.EndTest();
    }

    /// <summary>
    /// Verifies the key format: the qualified test name, followed by the data row in parentheses for a data-driven test
    /// </summary>
    [TestMethod]
    public void TestKeyAppendsTheDataRowInParentheses()
    {
        // Act & Assert
        Assert.AreEqual("Sample.Tests.Fixture(1)", PhaseStabilityCheck.FormatTestKey(RecorderTestName, "1"));
        Assert.AreEqual(RecorderTestName, PhaseStabilityCheck.FormatTestKey(RecorderTestName, null));
        Assert.AreEqual(PhaseStabilityCheck.FormatTestName(typeof(PhaseStabilityCheckTests).FullName, "M"), PhaseStabilityCheck.FormatTestName(typeof(PhaseStabilityCheckTests), "M"));
    }

    /// <summary>
    /// Verifies that the runtime key of the running test equals the key of an entry naming it by type and method, so both
    /// sides use the one test-name format
    /// </summary>
    [TestMethod]
    public void RuntimeTestNameMatchesTheEntryTestName()
    {
        // Arrange
        var entry = new PhaseReversalEntry(typeof(PhaseStabilityCheckTests),
                                           nameof(RuntimeTestNameMatchesTheEntryTestName),
                                           PhaseStabilityCheck.ComputeFingerprint(FormattedSource),
                                           _textChangingCause,
                                           PhaseStabilityCheck.ComputeFingerprint(TextChangingPhase.GetOutput(FormattedSource.Replace("\r\n", "\n"), "\n")),
                                           PhaseReversalLineEndings.Both);

        PhaseStabilityCheck.OverrideForCurrentTest([entry], CreateTextChangingPhases);

        // Act
        AssertRuleResult(FormattedSource);

        // Assert
        Assert.AreEqual(PhaseStabilityCheck.FormatTestName(TestContext.FullyQualifiedTestClassName, TestContext.TestName), entry.TestKey);
        PhaseStabilityCheck.EndTest();
    }

    /// <summary>
    /// Verifies that the test cleanup fails on an entry of a test that never runs the check, so an entry outlives no
    /// helper call
    /// </summary>
    [TestMethod]
    public void TestCleanupFailsOnAnEntryOfATestThatNeverRunsTheCheck()
    {
        // Arrange
        var entry = new PhaseReversalEntry(typeof(PhaseStabilityCheckTests),
                                           nameof(TestCleanupFailsOnAnEntryOfATestThatNeverRunsTheCheck),
                                           PhaseStabilityCheck.ComputeFingerprint(MisindentedSource),
                                           PhaseReversalCauses.BlankLineNextToBrace,
                                           PhaseStabilityCheck.ComputeFingerprint(FormattedSource),
                                           PhaseReversalLineEndings.Both);

        PhaseStabilityCheck.OverrideForCurrentTest([entry], PhaseStabilityCheck.CreateProductionPhases);

        // Act
        var failure = Assert.Throws<AssertFailedException>(PhaseStabilityCheck.EndTest);

        // Assert
        Assert.Contains("Stale phase reversal entry", failure.Message);
        Assert.Contains("under LF line endings", failure.Message);
        Assert.Contains("under CRLF line endings", failure.Message);
    }

    /// <summary>
    /// Verifies that every entry names a test method of the formatter test suite and a known cause, that an entry of a
    /// data-driven method names one of its data rows and an entry of another method names none, and that no test
    /// execution, fixture, and phase is listed twice
    /// </summary>
    [TestMethod]
    public void EveryEntryNamesATestMethodItsDataRowAndAKnownCause()
    {
        // Arrange
        var causes = GetKnownCauses();

        foreach (var entry in PhaseReversalExceptionList.Entries)
        {
            // Act
            Assert.IsNotNull(entry.TestClass, $"{entry} names its test by name only; the exception list names it by type.");

            var method = entry.TestClass.GetMethod(entry.TestMethod, BindingFlags.Public | BindingFlags.Instance);

            Assert.IsNotNull(method, $"{entry} names no public test method.");

            var dataRows = method.GetCustomAttributes<DataRowAttribute>().Select(row => DataRowKey.Format(row.Data)).ToList();
            var hasDynamicData = method.IsDefined(typeof(DynamicDataAttribute), inherit: true);

            // Assert
            Assert.IsTrue(method.IsDefined(typeof(TestMethodAttribute), inherit: true), $"{entry} names a method that is not a test.");
            Assert.IsTrue(typeof(FormatterTestsBase).IsAssignableFrom(entry.TestClass), $"{entry} names a class whose tests do not run the check.");
            Assert.Contains(entry.Cause, causes, $"{entry} names no known cause.");

            if (dataRows.Count == 0
                && hasDynamicData == false)
            {
                Assert.IsNull(entry.DataRow, $"{entry} names a data row of a test that is not data-driven.");
            }
            else
            {
                Assert.IsNotNull(entry.DataRow, $"{entry} names no data row of its data-driven test.");

                if (hasDynamicData == false)
                {
                    Assert.Contains(entry.DataRow, dataRows, $"{entry} names a data row its test does not have.");
                }
            }
        }

        var keys = PhaseReversalExceptionList.Entries.Select(entry => (entry.TestKey, entry.Fingerprint, entry.Phase)).ToList();

        Assert.HasCount(keys.Count, keys.Distinct(), "A test execution, fixture, and phase is listed twice.");
    }

    /// <summary>
    /// Returns the causes <see cref="PhaseReversalCauses"/> declares
    /// </summary>
    /// <returns>The causes</returns>
    private static List<PhaseReversalCause> GetKnownCauses()
    {
        return typeof(PhaseReversalCauses).GetFields(BindingFlags.Public | BindingFlags.Static)
                                          .Select(field => (PhaseReversalCause)field.GetValue(null))
                                          .ToList();
    }

    /// <summary>
    /// Creates a recorder-test entry for the final output of a fixture
    /// </summary>
    /// <param name="finalOutput">The final output of the fixture</param>
    /// <param name="cause">The cause, which names the phase</param>
    /// <param name="phaseOutput">The text the phase returns</param>
    /// <param name="lineEndings">The line endings the entry covers</param>
    /// <returns>The entry</returns>
    private static PhaseReversalEntry CreateEntry(string finalOutput, PhaseReversalCause cause, string phaseOutput, PhaseReversalLineEndings lineEndings)
    {
        return new PhaseReversalEntry(RecorderTestName, PhaseStabilityCheck.ComputeFingerprint(finalOutput), cause, PhaseStabilityCheck.ComputeFingerprint(phaseOutput), lineEndings);
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
    /// Asserts that a helper failed on the unlisted change of <see cref="TextChangingPhase"/> to <see cref="FormattedSource"/>
    /// under LF line endings, the first line ending the helpers check
    /// </summary>
    /// <param name="failure">The failure</param>
    private static void AssertUnlistedChangeFailure(AssertFailedException failure)
    {
        Assert.Contains($"{nameof(TextChangingPhase)} changes the final output of fixture {PhaseStabilityCheck.ComputeFingerprint(FormattedSource)}", failure.Message);
        Assert.Contains(PhaseStabilityCheck.FormatTestName(typeof(PhaseStabilityCheckTests), string.Empty), failure.Message);
        Assert.Contains("under LF line endings", failure.Message);
    }

    /// <summary>
    /// Asserts that no phase changes the source when it runs alone, under LF and CRLF
    /// </summary>
    /// <param name="source">The source, a final output of the formatter</param>
    private static void AssertNoPhaseChanges(string source)
    {
        foreach (var endOfLine in _lineEndings)
        {
            var text = NormalizeLineEndings(source, endOfLine);

            AssertIsFinalOutput(text, endOfLine);

            var changes = PhaseStabilityCheck.FindChangingPhases(text, endOfLine, null);

            Assert.IsEmpty(changes, $"Under {DescribeLineEnding(endOfLine)} line endings: {string.Join(" ", changes.Select(change => $"{change.Phase}: {change.Difference}"))}");
        }
    }

    /// <summary>
    /// Asserts that exactly one phase changes the source when it runs alone, under LF and CRLF, and that it returns the
    /// given output
    /// </summary>
    /// <param name="source">The source, a final output of the formatter</param>
    /// <param name="phase">The phase expected to change it</param>
    /// <param name="phaseOutput">The text the phase is expected to return</param>
    private static void AssertOnlyPhaseChanges(string source, string phase, string phaseOutput)
    {
        foreach (var endOfLine in _lineEndings)
        {
            var text = NormalizeLineEndings(source, endOfLine);
            var endingName = DescribeLineEnding(endOfLine);

            AssertIsFinalOutput(text, endOfLine);

            var changes = PhaseStabilityCheck.FindChangingPhases(text, endOfLine, null);

            Assert.AreSequenceEqual(new[] { phase }, changes.Select(change => change.Phase).ToList(), $"Under {endingName} line endings: {string.Join(" ", changes.Select(change => change.Difference))}");
            Assert.AreEqual(NormalizeLineEndings(phaseOutput, endOfLine), changes[0].Output, $"Under {endingName} line endings.");
        }
    }

    /// <summary>
    /// Asserts that the source is a fixed point of the whole pipeline, so a phase that changes it alone is a reversal
    /// </summary>
    /// <param name="text">The source</param>
    /// <param name="endOfLine">The end-of-line sequence to format with</param>
    private static void AssertIsFinalOutput(string text, string endOfLine)
    {
        var (root, context) = SecondPassInput.Create(text, endOfLine, null);

        var formatted = FormattingPipeline.Execute(root, context, CancellationToken.None).ToFullString();

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