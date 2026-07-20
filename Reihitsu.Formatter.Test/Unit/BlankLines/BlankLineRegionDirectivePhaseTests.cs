using System.Threading;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Pipeline.BlankLines;
using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Unit.BlankLines;

/// <summary>
/// Tests for <see cref="BlankLinePhase"/> — blank lines required before <c>#region</c> and <c>#endregion</c>
/// directives that directly follow a switch-label colon. Exercised at the phase level rather than through the
/// full pipeline because <see cref="Reihitsu.Formatter.Pipeline.RegionFormatting.NestedRegionRemovalStep"/>
/// unconditionally strips region directives nested inside a switch statement, which would otherwise remove the
/// directives under test before blank-line formatting ever runs
/// </summary>
[TestClass]
public class BlankLineRegionDirectivePhaseTests : FormatterTestsBase
{
    #region Properties

    /// <summary>
    /// Test context for the current test
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies that a blank line is inserted before a <c>#region</c> directive that directly follows a
    /// switch-label colon. RH5031's Core policy exempts only lines ending with an opening brace, unlike the
    /// general first-in-block exemption that also treats switch labels as exempt (issue #428)
    /// </summary>
    [TestMethod]
    public void InsertsBlankLineBeforeRegionAfterSwitchLabel()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int value)
                                 {
                                     switch (value)
                                     {
                                         case 1:
                                             #region R
                                             Bar();
                                             #endregion
                                             break;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int value)
                                    {
                                        switch (value)
                                        {
                                            case 1:

                                                #region R

                                                Bar();

                                                #endregion

                                                break;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertPhaseResult(input, expected);
    }

    /// <summary>
    /// Verifies that a blank line is inserted before an <c>#endregion</c> directive that directly follows a
    /// switch-label colon, the symmetric counterpart of the <c>#region</c> case (issue #428)
    /// </summary>
    [TestMethod]
    public void InsertsBlankLineBeforeEndRegionAfterSwitchLabel()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int value)
                                 {
                                     #region R
                                     switch (value)
                                     {
                                         case 1:
                                         #endregion
                                             Bar();
                                             break;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int value)
                                    {
                                        #region R

                                        switch (value)
                                        {
                                            case 1:

                                            #endregion

                                                Bar();
                                                break;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertPhaseResult(input, expected);
    }

    /// <summary>
    /// Applies <see cref="BlankLinePhase"/> under both LF and CRLF line endings and verifies the result matches
    /// the expected text and is idempotent on a second pass, reusing <see cref="FormatterTestsBase"/>'s shared
    /// line-ending helpers instead of duplicating them (issue #428 review)
    /// </summary>
    /// <param name="input">The input source text</param>
    /// <param name="expected">The expected rewritten source text</param>
    private static void AssertPhaseResult(string input, string expected)
    {
        foreach (var endOfLine in _lineEndings)
        {
            var normalizedInput = NormalizeLineEndings(input, endOfLine);
            var normalizedExpected = NormalizeLineEndings(expected, endOfLine);
            var endingName = DescribeLineEnding(endOfLine);

            var actual = ApplyRewriter(normalizedInput, endOfLine);

            Assert.AreEqual(normalizedExpected, actual, $"Phase output mismatch under {endingName} line endings.");
            AssertUsesLineEnding(actual, endOfLine);

            var actualSecondPass = ApplyRewriter(actual, endOfLine);

            Assert.AreEqual(normalizedExpected, actualSecondPass, $"Phase is not idempotent under {endingName} line endings.");
        }
    }

    /// <summary>
    /// Applies the <see cref="BlankLinePhase"/> to the given source text, threading the requested end-of-line
    /// sequence through the pipeline so the chosen ending is honored verbatim
    /// </summary>
    /// <param name="source">The source text to rewrite</param>
    /// <param name="endOfLine">The end-of-line sequence to format with</param>
    /// <returns>The rewritten source text</returns>
    private static string ApplyRewriter(string source, string endOfLine)
    {
        var tree = CSharpSyntaxTree.ParseText(source);
        var context = new FormattingContext(endOfLine);
        var result = new BlankLinePhase().Execute(tree.GetRoot(), context, CancellationToken.None);

        return result.ToFullString();
    }

    #endregion // Methods
}