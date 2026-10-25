using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Unit.Pipeline;

/// <summary>
/// Tests that observe every phase of the production pipeline: no phase after the first receives or returns a tab in the
/// whitespace of the code or between directive tokens, and no phase after the indentation phase moves anything within its line
/// </summary>
[TestClass]
public class PhaseColumnOwnershipTests : FormatterTestsBase
{
    #region Constants

    /// <summary>
    /// Name of the indentation phase as the phase observer reports it
    /// </summary>
    private const string IndentationPhaseName = "IndentationPhase";

    #endregion // Constants

    #region Columns are final after the indentation phase

    /// <summary>
    /// Verifies the spec example: the indentation phase places <c>2</c> under <c>1</c> and moves the raw string under its
    /// opening quotes, and the phases after it only remove the final line break
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-015")]
    public void LinesOfTheSpecExampleAreFinalAfterTheIndentationPhase()
    {
        const string input = """"
                             class C
                             {
                             void M()
                             {
                             var s = """
                                 a
                                 """;

                                 Call(1,
                             2);
                             }
                             }
                             """";
        const string expected = """"
                                class C
                                {
                                    void M()
                                    {
                                        var s = """
                                                a
                                                """;

                                        Call(1,
                                             2);
                                    }
                                }
                                """";

        AssertColumnsAreFinalAfterTheIndentationPhase(input, expected);
    }

    /// <summary>
    /// Verifies the spec example: a tab in front of a trailing comment is already four spaces when the indentation phase runs,
    /// so the comment keeps the column the indentation phase left it in, and trailing spaces are removed without moving a token
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-015")]
    public void TrailingCommentBehindATabKeepsItsColumnAfterTheIndentationPhase()
    {
        const string input = "class C\n{\n    void M()\n    {\n        Call(1,\t// c\n2);   \n    }\n}";
        const string expected = "class C\n{\n    void M()\n    {\n        Call(1,    // c\n             2);\n    }\n}";

        AssertColumnsAreFinalAfterTheIndentationPhase(input, expected);
    }

    /// <summary>
    /// Verifies that the closing line of a raw string literal that moves, and the chain link anchored behind its closing marker,
    /// are final after the indentation phase
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-015")]
    public void AnchorBehindTheClosingMarkerOfAMovedRawStringIsFinalAfterTheIndentationPhase()
    {
        const string input = """"
                             class C
                             {
                                 void M()
                                 {
                                     var s = """
                                 a
                                 """.Trim()
                                 .ToUpper();
                                 }
                             }
                             """";
        const string expected = """"
                                class C
                                {
                                    void M()
                                    {
                                        var s = """
                                                a
                                                """.Trim()
                                                   .ToUpper();
                                    }
                                }
                                """";

        AssertColumnsAreFinalAfterTheIndentationPhase(input, expected);
    }

    /// <summary>
    /// Verifies that a tab-indented hole line of a multi-line interpolated raw string that moves is final after the
    /// indentation phase
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-015")]
    public void TabIndentedHoleLineOfAMovedRawStringIsFinalAfterTheIndentationPhase()
    {
        const string input = "class C\n{\n    void M()\n    {\n        var name = \"x\";\n        var a = $\"\"\"\n    Hello {\n\t    name\n    }\n    \"\"\";\n    }\n}";
        const string expected = "class C\n{\n    void M()\n    {\n        var name = \"x\";\n        var a = $\"\"\"\n                 Hello {\n                     name\n                 }\n                 \"\"\";\n    }\n}";

        AssertColumnsAreFinalAfterTheIndentationPhase(input, expected);
    }

    #endregion // Columns are final after the indentation phase

    #region No phase after the first sees a tab in whitespace

    /// <summary>
    /// Verifies the spec example: the first phase writes four spaces for every tab of the indentation, every later phase
    /// receives and returns that text without a tab, and the result is the formatted block
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-016")]
    public void FirstPhaseReplacesEveryIndentationTabAndNoLaterPhaseSeesOne()
    {
        const string input = "class C\n{\n\tvoid M()\n\t{\n\t\tCall(1,\n\t\t\t2);\n\t}\n}";
        const string expected = "class C\n{\n    void M()\n    {\n        Call(1,\n             2);\n    }\n}";

        foreach (var endOfLine in _lineEndings)
        {
            var normalizedInput = NormalizeLineEndings(input, endOfLine);
            var phases = ObservedPipeline.Run(normalizedInput, endOfLine);

            Assert.AreEqual("TabReplacementPhase", phases[0].Name, "The first phase of the pipeline should be the tab replacement.");
            Assert.AreEqual(normalizedInput.Replace("\t", "    "), phases[0].Output.ToFullString(), $"The first phase should write four spaces for every tab under {DescribeLineEnding(endOfLine)} line endings.");

            AssertNoPhaseAfterTheFirstSeesATab(phases, endOfLine);

            Assert.AreEqual(NormalizeLineEndings(expected, endOfLine), phases[^1].Output.ToFullString(), $"Formatter output mismatch under {DescribeLineEnding(endOfLine)} line endings.");
        }
    }

    /// <summary>
    /// Verifies that no phase after the first receives or returns a tab in indentation, in a token gap, in front of an
    /// end-of-line comment, on a hole line of a multi-line interpolated raw string, or between directive tokens, while the tab
    /// inside a documentation comment, which is outside the requirement, is kept through the pipeline
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-016")]
    public void NoPhaseAfterTheFirstSeesATabInTheWhitespaceOfTheCodeOrBetweenDirectiveTokens()
    {
        const string input = "class C\n{\n\tvoid M()\n\t{\n\t\tint\tx = 1;\t// e\n#pragma\twarning disable CS0168\n\t\tvar name = \"x\";\n\t\tvar a = $\"\"\"\n    Hello {\n\t    name\n    }\n    \"\"\";\n\t}\n\n\t/// <param\tname=\"x\">b</param>\n\tvoid N(int x)\n\t{\n\t}\n}";
        const string expected = "class C\n{\n    void M()\n    {\n        int x = 1;    // e\n#pragma    warning disable CS0168\n        var name = \"x\";\n        var a = $\"\"\"\n                 Hello {\n                     name\n                 }\n                 \"\"\";\n    }\n\n    /// <param\tname=\"x\">b</param>\n    void N(int x)\n    {\n    }\n}";

        foreach (var endOfLine in _lineEndings)
        {
            var phases = ObservedPipeline.Run(NormalizeLineEndings(input, endOfLine), endOfLine);

            Assert.AreEqual("TabReplacementPhase", phases[0].Name, "The first phase of the pipeline should be the tab replacement.");

            AssertNoPhaseAfterTheFirstSeesATab(phases, endOfLine);

            Assert.AreEqual(NormalizeLineEndings(expected, endOfLine), phases[^1].Output.ToFullString(), $"Formatter output mismatch under {DescribeLineEnding(endOfLine)} line endings.");
        }
    }

    /// <summary>
    /// Verifies that a tab in front of the <c>///</c> marker of a documentation comment's continuation line is left to the
    /// indentation phase, which rewrites that whitespace with spaces when it indents the marker
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-016")]
    public void TabInFrontOfDocumentationContinuationLineIsReplacedByTheIndentationPhase()
    {
        const string input = "class C\n{\n    /// <summary>\n\t/// a\n    /// </summary>\n    void M()\n    {\n    }\n}";
        const string expected = "class C\n{\n    /// <summary>\n    /// a\n    /// </summary>\n    void M()\n    {\n    }\n}";

        foreach (var endOfLine in _lineEndings)
        {
            var normalizedInput = NormalizeLineEndings(input, endOfLine);
            var phases = ObservedPipeline.Run(normalizedInput, endOfLine);
            var indentationPhase = phases.Single(phase => phase.Name == IndentationPhaseName);

            Assert.AreEqual("TabReplacementPhase", phases[0].Name, "The first phase of the pipeline should be the tab replacement.");
            Assert.AreEqual(normalizedInput, phases[0].Output.ToFullString(), "The first phase should keep the tab in front of the continuation line's marker.");
            Assert.AreEqual(NormalizeLineEndings(expected, endOfLine), indentationPhase.Output.ToFullString(), $"The indentation phase should rewrite the marker's whitespace with spaces under {DescribeLineEnding(endOfLine)} line endings.");
            Assert.AreEqual(NormalizeLineEndings(expected, endOfLine), phases[^1].Output.ToFullString(), $"Formatter output mismatch under {DescribeLineEnding(endOfLine)} line endings.");
        }
    }

    #endregion // No phase after the first sees a tab in whitespace

    #region Methods

    /// <summary>
    /// Asserts, under LF and CRLF line endings, that the pipeline produces the expected output, that the output of the
    /// indentation phase already has every line of that output, and that no phase after it changes a line other than by
    /// removing the whitespace that ends it or the line breaks at the end of the file
    /// </summary>
    /// <param name="input">The source text</param>
    /// <param name="expected">The expected formatted output</param>
    private static void AssertColumnsAreFinalAfterTheIndentationPhase(string input, string expected)
    {
        foreach (var endOfLine in _lineEndings)
        {
            var endingName = DescribeLineEnding(endOfLine);
            var phases = ObservedPipeline.Run(NormalizeLineEndings(input, endOfLine), endOfLine);
            var indentationIndex = phases.ToList().FindIndex(phase => phase.Name == IndentationPhaseName);

            Assert.IsGreaterThanOrEqualTo(0, indentationIndex, "The pipeline should run the indentation phase.");
            Assert.AreEqual(NormalizeLineEndings(expected, endOfLine), phases[^1].Output.ToFullString(), $"Formatter output mismatch under {endingName} line endings.");

            for (var index = indentationIndex + 1; index < phases.Count; index++)
            {
                var phase = phases[index];

                Assert.AreSequenceEqual(ObservedPipeline.GetLinesWithoutTrailingWhitespace(phase.Input.ToFullString()).ToList(),
                                        ObservedPipeline.GetLinesWithoutTrailingWhitespace(phase.Output.ToFullString()).ToList(),
                                        $"{phase.Name} runs after the indentation phase and must not move anything within its line under {endingName} line endings.");
            }

            Assert.AreSequenceEqual(ObservedPipeline.GetLinesWithoutTrailingWhitespace(expected).ToList(),
                                    ObservedPipeline.GetLinesWithoutTrailingWhitespace(phases[indentationIndex].Output.ToFullString()).ToList(),
                                    $"The output of the indentation phase should already have every line of the final output under {endingName} line endings.");
        }
    }

    /// <summary>
    /// Asserts that every phase after the first receives and returns the whitespace of the code and the whitespace between
    /// directive tokens without a tab
    /// </summary>
    /// <param name="phases">The observed phases</param>
    /// <param name="endOfLine">The end-of-line sequence of the run</param>
    private static void AssertNoPhaseAfterTheFirstSeesATab(IReadOnlyList<ObservedPhase> phases, string endOfLine)
    {
        foreach (var phase in phases.Skip(1))
        {
            var inputFindings = ObservedPipeline.FindTabsInCodeAndDirectiveWhitespace(phase.Input);
            var outputFindings = ObservedPipeline.FindTabsInCodeAndDirectiveWhitespace(phase.Output);

            Assert.IsEmpty(inputFindings, $"{phase.Name} received a tab under {DescribeLineEnding(endOfLine)} line endings: {string.Join("; ", inputFindings)}");
            Assert.IsEmpty(outputFindings, $"{phase.Name} returned a tab under {DescribeLineEnding(endOfLine)} line endings: {string.Join("; ", outputFindings)}");
        }
    }

    #endregion // Methods
}