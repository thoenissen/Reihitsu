using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Unit;

/// <summary>
/// Tests for whole-document formatting of a file whose string literal or delimited comment another build compiles as code
/// (FMT-PIPE-033): every kind of hiding carrier and every line kind that counts, both sides of each condition of the definition,
/// the decided over-refusal and gap, its order against the other reasons to leave a file alone, and the node-level entry points,
/// which do not check it
/// </summary>
[TestClass]
public class ReihitsuFormatterHiddenCodeTests : FormatterTestsBase
{
    #region Constants

    /// <summary>
    /// A method whose <c>return a+b;</c> the formatter would correct; <c>CARRIER</c> stands for the lines inside the <c>#if !X</c>
    /// group, which is active in the build being formatted
    /// </summary>
    private const string GroupTemplate = """
                                         class C
                                         {
                                             int M(int a, int b)
                                             {
                                         #if !X
                                         CARRIER
                                         #endif
                                                 return a+b;
                                             }
                                         }
                                         """;

    #endregion // Constants

    #region Methods

    /// <summary>
    /// Verifies the specification example: the comment starts inside the <c>#if !A</c> group and holds the line <c>#else</c>, so
    /// the document is returned unchanged, also the block's indentation the formatter would correct elsewhere, and is not reported
    /// as unsupported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-033")]
    public async Task SpecExampleCommentThatIsCodeInAnotherBuildLeavesTheDocumentUnchanged()
    {
        const string input = """
                             using System;

                             class C
                             {
                                 void M()
                                 {
                             #if !A
                                     /*
                             #else
                                     var s = @"
                                     */
                                     Action a = delegate ()
                                         {
                                             Console.WriteLine(1);
                                         }; //";
                             #endif
                                 }
                             }
                             """;

        await AssertLeftAlone(input);
    }

    /// <summary>
    /// Verifies the specification example of the other side: a comment that holds <c>#if</c> and <c>#endif</c> lines but starts
    /// in no conditional group, and a raw string whose <c>#</c> line is a Markdown heading, are no hiding carriers, and the
    /// document is formatted
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-033")]
    public void SpecExampleCommentOutsideEveryGroupAndMarkdownHeadingAreFormatted()
    {
        const string input = """"
                             class C
                             {
                                 string M(string s)
                                 {
                                     /*
                             #if DEBUG
                                     Log();
                             #endif
                                     */
                                     var t = """
                                             # Title
                                             """;

                                     return s+t;
                                 }
                             }
                             """";
        const string expected = """"
                                class C
                                {
                                    string M(string s)
                                    {
                                        /*
                                #if DEBUG
                                        Log();
                                #endif
                                        */
                                        var t = """
                                                # Title
                                                """;

                                        return s + t;
                                    }
                                }
                                """";

        AssertFormattedThroughEntryPoint(input, expected);
    }

    /// <summary>
    /// Verifies the specification example of the decided gap: a verbatim string that only the excluded branch opens is disabled
    /// text in the build being formatted, and the code behind it is formatted
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-033")]
    public void SpecExampleStringOpenedOnlyByAnExcludedBranchIsADecidedGap()
    {
        const string input = """
                             class C
                             {
                                 int M(int a, int b)
                                 {
                             #if B
                                     var s = @"
                             #endif
                                         var x = a+b;
                             #if B
                             ";
                             #endif
                                     return 0;
                                 }
                             }
                             """;

        AssertFormattedThroughEntryPointContains(input, "        var x = a + b;");
    }

    /// <summary>
    /// Verifies that every kind of string literal and delimited comment that spans lines is a hiding carrier when it starts inside
    /// a conditional group and holds an <c>#else</c> line: the document is returned unchanged
    /// </summary>
    /// <param name="carrier">The lines of the carrier inside the group</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-033")]
    [DataRow("        var s = @\"\n#else\n        \";", DisplayName = "Verbatim string")]
    [DataRow("        var s = \"\"\"\n            #else\n            \"\"\";", DisplayName = "Raw string")]
    [DataRow("        var s = $@\"{a}\n#else\n        \";", DisplayName = "Interpolated verbatim string, line behind a hole")]
    [DataRow("        var s = $\"\"\"\n            {a}\n            #else\n            \"\"\";", DisplayName = "Interpolated raw string, line behind a hole")]
    [DataRow("        var s = @\"\n#else\n        \"u8;", DisplayName = "Verbatim UTF-8 string")]
    [DataRow("        var s = \"\"\"\n            #else\n            \"\"\"u8;", DisplayName = "Raw UTF-8 string")]
    [DataRow("        /*\n#else\n        */", DisplayName = "Delimited comment")]
    [DataRow("        /**\n#else\n        */", DisplayName = "Delimited documentation comment")]
    public async Task EveryKindOfCarrierLeavesTheDocumentUnchanged(string carrier)
    {
        await AssertLeftAlone(GroupTemplate.Replace("CARRIER", carrier));
    }

    /// <summary>
    /// Verifies that the lines <c>#elif</c> and <c>#endif</c>, and <c>#else</c> with whitespace in front of it and between the
    /// <c>#</c> and the word, count as lines another build reads as a directive: the document is returned unchanged
    /// </summary>
    /// <param name="line">The line inside the carrier</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-033")]
    [DataRow("#elif Y", DisplayName = "#elif")]
    [DataRow("#endif", DisplayName = "#endif")]
    [DataRow("#  else", DisplayName = "Whitespace between # and else")]
    [DataRow("        #else", DisplayName = "Whitespace in front of #else")]
    public async Task EveryLineKindThatEndsTheGroupCounts(string line)
    {
        await AssertLeftAlone(GroupTemplate.Replace("CARRIER", $"        /*\n{line}\n        */"));
    }

    /// <summary>
    /// Verifies that a carrier inside a conditional group whose <c>#</c> lines another build does not read as <c>#elif</c>,
    /// <c>#else</c>, or <c>#endif</c> is no hiding carrier, and the document is formatted
    /// </summary>
    /// <param name="carrier">The lines of the carrier inside the group</param>
    [TestMethod]
    [Requirement("FMT-PIPE-033")]
    [DataRow("        var s = \"\"\"\n            # Title\n            \"\"\";", DisplayName = "Markdown heading")]
    [DataRow("        /*\n#if Y\n        */", DisplayName = "#if")]
    [DataRow("        /*\n#region R\n        */", DisplayName = "#region")]
    [DataRow("        /*\n#nullable enable\n        */", DisplayName = "#nullable")]
    [DataRow("        /*\n         * #else\n         */", DisplayName = "Line that starts with an asterisk")]
    [DataRow("        var s = @\"#else\n        \";", DisplayName = "#else on the carrier's first line")]
    [DataRow("        /// #else\n        /// #endif\n        var s = 1;", DisplayName = "Single-line documentation comment")]
    [DataRow("        // #else\n        var s = 1;", DisplayName = "Single-line comment")]
    public void CarrierWithoutALineThatEndsTheGroupIsFormatted(string carrier)
    {
        AssertFormattedThroughEntryPointContains(GroupTemplate.Replace("CARRIER", carrier), "        return a + b;");
    }

    /// <summary>
    /// Verifies that a carrier that holds an <c>#else</c> line but stands outside every conditional group, also behind a group
    /// that has already ended, is no hiding carrier, and the document is formatted
    /// </summary>
    /// <param name="input">The source text</param>
    [TestMethod]
    [Requirement("FMT-PIPE-033")]
    [DataRow("class C\n{\n    int M(int a, int b)\n    {\n        /*\n#else\n        */\n        return a+b;\n    }\n}", DisplayName = "No group")]
    [DataRow("class C\n{\n    int M(int a, int b)\n    {\n#if X\n        Log();\n#endif\n        var s = @\"\n#else\n\";\n        return a+b;\n    }\n}", DisplayName = "Behind an ended group")]
    public void CarrierOutsideEveryGroupIsFormatted(string input)
    {
        AssertFormattedThroughEntryPointContains(input, "        return a + b;");
    }

    /// <summary>
    /// Verifies that a carrier that starts behind an <c>#else</c> directive starts inside a conditional group as well: the
    /// document is returned unchanged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-033")]
    public async Task CarrierBehindAnElseDirectiveLeavesTheDocumentUnchanged()
    {
        const string input = """
                             class C
                             {
                                 int M(int a, int b)
                                 {
                             #if X
                                     Log();
                             #else
                                     /*
                             #endif
                                     */
                             #endif
                                     return a+b;
                                 }
                             }
                             """;

        await AssertLeftAlone(input);
    }

    /// <summary>
    /// Verifies that the group is judged in the build being formatted: the issue's input, formatted with <c>A</c> defined, holds a
    /// comment in the active <c>#if A</c> branch, and the document is returned unchanged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-033")]
    public async Task CarrierInABranchThatADefinedSymbolActivatesLeavesTheDocumentUnchanged()
    {
        const string input = """
                             class C
                             {
                                 int M(int a, int b)
                                 {
                             #if A
                                     /*
                             #else
                                     var s = @"
                                     */
                                     int c = 1; //";
                             #endif
                                     return a+b;
                                 }
                             }
                             """;

        await AssertLeftAlone(input, CSharpParseOptions.Default.WithPreprocessorSymbols("A"));
    }

    /// <summary>
    /// Verifies the decided over-refusal: a carrier inside a conditional group whose <c>#if</c> and <c>#endif</c> lines close only
    /// a group the carrier opens itself hides nothing, but counts, and the document is returned unchanged — a commented-out
    /// block and a generator template
    /// </summary>
    /// <param name="input">The source text</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-033")]
    [DataRow("class C\n{\n    int M(int a, int b)\n    {\n#if DEBUG\n        /*\n#if TRACE\n        Log();\n#endif\n        */\n#endif\n        return a+b;\n    }\n}", DisplayName = "Commented-out block")]
    [DataRow("class C\n{\n#if NET\n    const string T = \"\"\"\n        #if NULLABLE\n        #nullable enable\n        #endif\n        \"\"\";\n#endif\n    int M(int a, int b)\n    {\n        return a+b;\n    }\n}", DisplayName = "Generator template")]
    public async Task CarrierWhoseDirectiveLinesCloseOnlyItsOwnGroupsCounts(string input)
    {
        await AssertLeftAlone(input, CSharpParseOptions.Default.WithPreprocessorSymbols("DEBUG", "NET"));
    }

    /// <summary>
    /// Verifies that a document that holds a hiding carrier and a shape the formatter refuses as unsupported is returned unchanged
    /// and is not reported as unsupported: the hiding carrier decides first
    /// </summary>
    /// <param name="shape">The unsupported statement in front of <c>return</c></param>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-033")]
    [DataRow("        int /* c */ d = 1;", DisplayName = "Comment inside a variable declaration")]
    [DataRow("        var e = a > b ? // c\n            1\n            : 2;", DisplayName = "Comment behind a conditional operator")]
    public async Task CarrierBesideAnUnsupportedShapeIsNotReportedAsUnsupported(string shape)
    {
        var input = GroupTemplate.Replace("CARRIER", "        /*\n#else\n        */")
                                 .Replace("        return a+b;", $"{shape}\n        return a+b;");

        await AssertLeftAlone(input);
    }

    /// <summary>
    /// Verifies that a document with a syntax error that also holds a hiding carrier is returned unchanged and is not reported as
    /// unsupported
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-033")]
    public void CarrierBesideASyntaxErrorIsReturnedUnchanged()
    {
        var input = GroupTemplate.Replace("CARRIER", "        /*\n#else\n        */")
                                 .Replace("return a+b;", "return a+b");
        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);

        var result = ReihitsuFormatter.FormatSyntaxTree(tree, TestContext.CancellationToken);

        Assert.AreEqual(input, result.Output.ToString());
        Assert.IsNull(result.Unsupported);
    }

    /// <summary>
    /// Verifies that the node-level entry points do not check for a hiding carrier: the method that holds one is formatted when it
    /// is the target, by <see cref="ReihitsuFormatter.FormatNodeInDocumentAsync"/> and by <see cref="ReihitsuFormatter.FormatNode"/>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-033")]
    public async Task NodeLevelEntryPointsDoNotCheckForACarrier()
    {
        var input = GroupTemplate.Replace("CARRIER", "        /*\n#else\n        */");

        using (var workspace = new AdhocWorkspace())
        {
            var document = workspace.AddProject("TestProject", LanguageNames.CSharp).AddDocument("Test.cs", SourceText.From(input));
            var root = await document.GetSyntaxRootAsync(TestContext.CancellationToken);
            var method = root.DescendantNodes().OfType<MethodDeclarationSyntax>().Single();

            var documentResult = await ReihitsuFormatter.FormatNodeInDocumentAsync(document, method, TestContext.CancellationToken);
            var nodeResult = ReihitsuFormatter.FormatNode(method, cancellationToken: TestContext.CancellationToken);

            Assert.Contains("return a + b;", (await documentResult.Output.GetTextAsync(TestContext.CancellationToken)).ToString());
            Assert.Contains("return a + b;", nodeResult.Output.ToFullString());
        }
    }

    /// <summary>
    /// Verifies under LF and CRLF line endings that <see cref="ReihitsuFormatter.FormatSyntaxTree"/> and
    /// <see cref="ReihitsuFormatter.FormatDocumentAsync"/> return the input unchanged and do not report it as unsupported
    /// </summary>
    /// <param name="input">The source text</param>
    /// <param name="parseOptions">The parse options, or <see langword="null"/> for the defaults</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private async Task AssertLeftAlone(string input, CSharpParseOptions parseOptions = null)
    {
        foreach (var endOfLine in _lineEndings)
        {
            var endingName = DescribeLineEnding(endOfLine);
            var text = NormalizeLineEndings(input, endOfLine);
            var tree = CSharpSyntaxTree.ParseText(text, parseOptions, cancellationToken: TestContext.CancellationToken);

            Assert.DoesNotContain(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error, tree.GetDiagnostics(TestContext.CancellationToken), "The fixture must parse without an error.");

            var treeResult = ReihitsuFormatter.FormatSyntaxTree(tree, TestContext.CancellationToken);

            Assert.AreEqual(text, (await treeResult.Output.GetTextAsync(TestContext.CancellationToken)).ToString(), $"FormatSyntaxTree must return the document unchanged under {endingName} line endings.");
            Assert.IsNull(treeResult.Unsupported, $"FormatSyntaxTree must not report the document as unsupported under {endingName} line endings.");

            using (var workspace = new AdhocWorkspace())
            {
                var project = workspace.AddProject("TestProject", LanguageNames.CSharp);

                if (parseOptions != null)
                {
                    project = project.WithParseOptions(parseOptions);
                }

                var document = project.AddDocument("Test.cs", SourceText.From(text));

                var documentResult = await ReihitsuFormatter.FormatDocumentAsync(document, TestContext.CancellationToken);

                Assert.AreEqual(text, (await documentResult.Output.GetTextAsync(TestContext.CancellationToken)).ToString(), $"FormatDocumentAsync must return the document unchanged under {endingName} line endings.");
                Assert.IsNull(documentResult.Unsupported, $"FormatDocumentAsync must not report the document as unsupported under {endingName} line endings.");
            }
        }
    }

    /// <summary>
    /// Verifies under LF and CRLF line endings that <see cref="ReihitsuFormatter.FormatSyntaxTree"/> formats the input to the
    /// expected text, and that the expected text is a fixed point of the formatter
    /// </summary>
    /// <param name="input">The source text</param>
    /// <param name="expected">The expected output</param>
    private void AssertFormattedThroughEntryPoint(string input, string expected)
    {
        foreach (var endOfLine in _lineEndings)
        {
            var tree = CSharpSyntaxTree.ParseText(NormalizeLineEndings(input, endOfLine), cancellationToken: TestContext.CancellationToken);

            var result = ReihitsuFormatter.FormatSyntaxTree(tree, TestContext.CancellationToken);

            Assert.IsNull(result.Unsupported);
            Assert.AreEqual(NormalizeLineEndings(expected, endOfLine), result.Output.ToString(), $"Formatter output mismatch under {DescribeLineEnding(endOfLine)} line endings.");
        }

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies under LF and CRLF line endings that <see cref="ReihitsuFormatter.FormatSyntaxTree"/> formats the input: its output
    /// holds the given line, which the input does not hold
    /// </summary>
    /// <param name="input">The source text</param>
    /// <param name="formattedLine">A line the formatter writes for the input</param>
    private void AssertFormattedThroughEntryPointContains(string input, string formattedLine)
    {
        Assert.DoesNotContain(formattedLine, input, "The fixture must need formatting.");

        foreach (var endOfLine in _lineEndings)
        {
            var tree = CSharpSyntaxTree.ParseText(NormalizeLineEndings(input, endOfLine), cancellationToken: TestContext.CancellationToken);

            var result = ReihitsuFormatter.FormatSyntaxTree(tree, TestContext.CancellationToken);
            var output = result.Output.ToString();

            Assert.IsNull(result.Unsupported);
            Assert.Contains(formattedLine + endOfLine, output, $"The document must be formatted under {DescribeLineEnding(endOfLine)} line endings:{endOfLine}{output}");
        }
    }

    #endregion // Methods
}