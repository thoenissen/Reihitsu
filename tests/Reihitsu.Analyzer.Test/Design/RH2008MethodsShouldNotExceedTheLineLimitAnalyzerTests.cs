using System;
using System.Text;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Design;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Design;

/// <summary>
/// Test methods for <see cref="RH2008MethodsShouldNotExceedTheLineLimitAnalyzer"/>
/// </summary>
[TestClass]
public class RH2008MethodsShouldNotExceedTheLineLimitAnalyzerTests : AnalyzerTestsBase<RH2008MethodsShouldNotExceedTheLineLimitAnalyzer>
{
    #region Constants

    /// <summary>
    /// Configuration with a production limit of one line of code
    /// </summary>
    private const string LimitOne = """{ "design": { "maximumMethodLines": 1 } }""";

    /// <summary>
    /// Configuration with a production limit of two lines of code
    /// </summary>
    private const string LimitTwo = """{ "design": { "maximumMethodLines": 2 } }""";

    /// <summary>
    /// Configuration with a production limit of three lines of code
    /// </summary>
    private const string LimitThree = """{ "design": { "maximumMethodLines": 3 } }""";

    /// <summary>
    /// Configuration with a production limit of two and a test limit of three lines of code, from the example of
    /// SHARED-LINES-003
    /// </summary>
    private const string ProductionTwoTestThree = """{ "design": { "maximumMethodLines": 2, "maximumTestMethodLines": 3 } }""";

    /// <summary>
    /// Example of SHARED-LINES-002: <c>M</c> holds two lines of code and <c>N</c> three
    /// </summary>
    private const string TwoAndThreeLineMethods = """
                                                  class C
                                                  {
                                                      void M()
                                                      {
                                                          M();
                                                          M();
                                                      }

                                                      void {|#0:N|}()
                                                      {
                                                          N();
                                                          N();
                                                          N();
                                                      }
                                                  }
                                                  """;

    /// <summary>
    /// Example of SHARED-LINES-003: the body of <c>M</c> holds three lines of code
    /// </summary>
    private const string ThreeLineMethod = """
                                           class C
                                           {
                                               void {|#0:M|}()
                                               {
                                                   M();
                                                   M();
                                                   M();
                                               }
                                           }
                                           """;

    /// <summary>
    /// First example of SHARED-LINES-001: the body of <c>M</c> holds four lines of code while <c>FEATURE</c> is not
    /// defined
    /// </summary>
    private const string TriviaExample = """
                                         class C
                                         {
                                             void {|#0:M|}()
                                             {
                                                 // Prepare.
                                                 A();

                                         #if FEATURE
                                                 A();
                                         #endif
                                                 B(@"first
                                         second"); // Trailing comment.
                                                 A();
                                             }

                                             void A()
                                             {
                                             }

                                             void B(string text)
                                             {
                                             }
                                         }
                                         """;

    #endregion // Constants

    #region Tests — counting (SHARED-LINES-001)

    /// <summary>
    /// Verifies the first example of SHARED-LINES-001: comment, blank, directive, and excluded lines are not counted,
    /// both lines of a verbatim string are, and so is a line that holds a token and a comment
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-LINES-001")]
    public async Task TriviaLinesAreNotCountedAndVerbatimStringLinesAre()
    {
        await VerifyLines(TriviaExample, LimitThree, LineLimit(0, 4, 3));
    }

    /// <summary>
    /// Verifies the second example of SHARED-LINES-001: without the line <c>A();</c> in front of the blank line the
    /// body holds three lines of code and is not reported at a limit of three
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-LINES-001")]
    public async Task TriviaExampleAtTheLimitIsNotReported()
    {
        const string source = """
                              class C
                              {
                                  void M()
                                  {
                                      // Prepare.

                              #if FEATURE
                                      A();
                              #endif
                                      B(@"first
                              second"); // Trailing comment.
                                      A();
                                  }

                                  void A()
                                  {
                                  }

                                  void B(string text)
                                  {
                                  }
                              }
                              """;

        await VerifyLines(source, LimitThree);
    }

    /// <summary>
    /// Verifies that the line a conditional directive excludes in the analyzed build is not counted, while the same
    /// line counts when the symbol is defined
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-LINES-001")]
    public async Task LineIncludedByDefinedSymbolIsCounted()
    {
        await Verify(TriviaExample,
                     test =>
                     {
                         test.TestState.AdditionalFiles.Add(("reihitsu.json", LimitThree));
                         test.SolutionTransforms.Add(DefineSymbols("FEATURE"));
                     },
                     LineLimit(0, 5, 3));
    }

    /// <summary>
    /// Verifies that the same code gives the same count whether its lines end with CRLF, LF, or CR alone
    /// </summary>
    /// <param name="endOfLine">Line ending of the source</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-LINES-001")]
    [DataRow("\r\n")]
    [DataRow("\n")]
    [DataRow("\r")]
    public async Task CountIsTheSameForEveryLineEnding(string endOfLine)
    {
        await VerifyLines(WithEndOfLine(TriviaExample, endOfLine), LimitThree, LineLimit(0, 4, 3));
    }

    /// <summary>
    /// Verifies the third example of SHARED-LINES-001: every line of a raw string literal counts, the blank line
    /// inside it and the lines of its quotes included
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-LINES-001")]
    public async Task EveryLineOfRawStringLiteralIsCounted()
    {
        const string source = """"
                              class C
                              {
                                  string {|#0:M|}()
                                  {
                                      return """
                                          first

                                          second
                                          """;
                                  }
                              }
                              """";

        await VerifyLines(source, """{ "design": { "maximumMethodLines": 4 } }""", LineLimit(0, 5, 4));
    }

    /// <summary>
    /// Verifies that the raw string example of SHARED-LINES-001 is not reported when the limit equals its count of five
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-LINES-001")]
    [Requirement("SHARED-LINES-002")]
    public async Task RawStringLiteralAtTheLimitIsNotReported()
    {
        const string source = """"
                              class C
                              {
                                  string M()
                                  {
                                      return """
                                          first

                                          second
                                          """;
                                  }
                              }
                              """";

        await VerifyLines(source, """{ "design": { "maximumMethodLines": 5 } }""");
    }

    /// <summary>
    /// Verifies that a blank line inside a verbatim string literal is counted
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-LINES-001")]
    public async Task BlankLineInsideVerbatimStringIsCounted()
    {
        const string source = """
                              class C
                              {
                                  string {|#0:M|}()
                                  {
                                      return @"first

                              third";
                                  }
                              }
                              """;

        await VerifyLines(source, LimitOne, LineLimit(0, 3, 1));
    }

    /// <summary>
    /// Verifies that every line of an interpolated raw string literal counts, also the lines over which a hole runs
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-LINES-001")]
    public async Task EveryLineOfInterpolatedRawStringWithMultiLineHoleIsCounted()
    {
        const string source = """"
                              class C
                              {
                                  string {|#0:M|}()
                                  {
                                      return $"""
                                          {A(
                                              1)}
                                          """;
                                  }

                                  int A(int value)
                                  {
                                      return value;
                                  }
                              }
                              """";

        await VerifyLines(source, LimitOne, LineLimit(0, 4, 1));
    }

    /// <summary>
    /// Verifies that text inside a string literal that looks like a comment or a directive is code and is counted
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-LINES-001")]
    public async Task CommentAndDirectiveLookingTextInsideLiteralIsCounted()
    {
        const string source = """
                              class C
                              {
                                  string {|#0:M|}()
                                  {
                                      var comment = "// x";
                                      return comment + @"
                              #if X
                              /* y */
                              ";
                                  }
                              }
                              """;

        await VerifyLines(source, LimitOne, LineLimit(0, 5, 1));
    }

    /// <summary>
    /// Verifies that the lines inside a multi-line block comment that hold no token are not counted, while the lines on
    /// which the comment starts and ends next to tokens are
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-LINES-001")]
    public async Task MultiLineBlockCommentBetweenTokensCountsOnlyItsLinesWithTokens()
    {
        const string source = """
                              class C
                              {
                                  void {|#0:M|}()
                                  {
                                      A(1, /* first
                                             second
                                             third */ 2);
                                      /*
                                       * Standalone.
                                       */
                                  }

                                  void A(int first, int second)
                                  {
                                  }
                              }
                              """;

        await VerifyLines(source, LimitOne, LineLimit(0, 2, 1));
    }

    /// <summary>
    /// Verifies that <c>#region</c>, <c>#endregion</c>, and <c>#pragma</c> lines inside a body are not counted
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-LINES-001")]
    public async Task RegionAndPragmaLinesAreNotCounted()
    {
        const string source = """
                              class C
                              {
                                  void {|#0:M|}()
                                  {
                                      #region Work
                              #pragma warning disable CS0168
                                      int unused;
                              #pragma warning restore CS0168
                                      M();
                                      #endregion // Work
                                  }
                              }
                              """;

        await VerifyLines(source, LimitOne, LineLimit(0, 2, 1));
    }

    /// <summary>
    /// Verifies that an <c>#if false</c> branch is not counted and the active <c>#else</c> branch is
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-LINES-001")]
    public async Task InactiveIfFalseBranchIsNotCountedAndActiveElseBranchIs()
    {
        const string source = """
                              class C
                              {
                                  void {|#0:M|}()
                                  {
                              #if false
                                      M();
                                      M();
                                      M();
                              #else
                                      M();
                                      M();
                              #endif
                                  }
                              }
                              """;

        await VerifyLines(source, LimitOne, LineLimit(0, 2, 1));
    }

    /// <summary>
    /// Verifies that a documentation comment inside a body is not counted, whichever documentation mode parses it
    /// </summary>
    /// <param name="documentationMode">Documentation mode of the test project</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-LINES-001")]
    [DataRow(DocumentationMode.None)]
    [DataRow(DocumentationMode.Parse)]
    [DataRow(DocumentationMode.Diagnose)]
    public async Task DocumentationCommentInsideBodyIsNotCountedInEveryDocumentationMode(DocumentationMode documentationMode)
    {
        const string source = """
                              class C
                              {
                                  void {|#1:M|}()
                                  {
                                      /// <summary>
                                      /// Local
                                      /// </summary>
                                      void {|#0:Local|}()
                                      {
                                          /** <summary>
                                              Inner
                                              </summary> */
                                          M();
                                          M();
                                      }
                                  }
                              }
                              """;

        await Verify(source,
                     test =>
                     {
                         test.TestState.AdditionalFiles.Add(("reihitsu.json", LimitOne));
                         test.SolutionTransforms.Add((solution, projectId) => solution.GetProject(projectId)?.ParseOptions is CSharpParseOptions parseOptions
                                                                                  ? solution.WithProjectParseOptions(projectId, parseOptions.WithDocumentationMode(documentationMode))
                                                                                  : solution);
                     },
                     LineLimit(0, 2, 1),
                     LineLimit(1, 5, 1));
    }

    #endregion // Tests — counting (SHARED-LINES-001)

    #region Tests — limit and configuration (SHARED-LINES-002, RH2008-001)

    /// <summary>
    /// Verifies the example of SHARED-LINES-002: at a production limit of two, a body of two lines is not reported
    /// and a body of three is
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-LINES-002")]
    [Requirement("RH2008-001")]
    [Requirement("SHARED-LIMIT-001")]
    [Requirement("SHARED-LIMIT-002")]
    public async Task ConfiguredLimitReportsOnlyTheBodyAboveIt()
    {
        await VerifyLines(TwoAndThreeLineMethods, LimitTwo, LineLimit(0, 3, 2));
    }

    /// <summary>
    /// Verifies that a problem RH0001 reports elsewhere in the configuration does not keep a valid limit from taking
    /// effect
    /// </summary>
    /// <param name="configuration">Configuration with a valid limit of two and a problem elsewhere</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-LINES-002")]
    [Requirement("SHARED-LIMIT-001")]
    [DataRow("""{ "design": { "maximumMethodLines": 2 }, "formatting": 1 }""")]
    [DataRow("""{ "design": { "maximumMethodLines": 2, "maximumTypeLines": "500" } }""")]
    [DataRow("""{ "design": { "maximumMethodLines": 2, "maximumTestMethodLines": 0 } }""")]
    [DataRow("""{ "design": { "maximumMethodLines": 2, "maximumFileLines": 900 } }""")]
    [DataRow("""{ "copyright": "x", "design": { "maximumMethodLines": 2 } }""")]
    public async Task ProblemElsewhereDoesNotKeepValidLimitFromTakingEffect(string configuration)
    {
        await VerifyLines(TwoAndThreeLineMethods, configuration, LineLimit(0, 3, 2));
    }

    /// <summary>
    /// Verifies the configurations of SHARED-LINES-002 under which the default production limit of 60 applies, so
    /// neither method of the example is reported
    /// </summary>
    /// <param name="configuration">Text of <c>reihitsu.json</c>, or <see langword="null"/> for no file</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-LINES-002")]
    [Requirement("RH2008-001")]
    [Requirement("SHARED-LIMIT-001")]
    [DataRow("""{ }""")]
    [DataRow("""{ "design": { } }""")]
    [DataRow("""{ "design": { "maximumMethodLines": 0 } }""")]
    [DataRow("""{ "design": { "maximumMethodLines": "2" } }""")]
    [DataRow("")]
    [DataRow("{ } junk")]
    [DataRow("""{ "design": { "maximumTestMethodLines": 2 } }""")]
    [DataRow("""{ "design": [ ] }""")]
    public async Task DefaultProductionLimitAppliesWithoutValidSetting(string configuration)
    {
        await VerifyLines(TwoAndThreeLineMethods.Replace("{|#0:N|}", "N"), configuration);
    }

    /// <summary>
    /// Verifies that without a configuration file the default production limit of 60 applies, so neither method of the
    /// example of SHARED-LINES-002 is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-LINES-002")]
    [Requirement("RH2008-001")]
    [Requirement("SHARED-LIMIT-001")]
    public async Task DefaultProductionLimitAppliesWithoutConfigurationFile()
    {
        await VerifyLines(TwoAndThreeLineMethods.Replace("{|#0:N|}", "N"), null);
    }

    /// <summary>
    /// Verifies the test-project part of the example of SHARED-LINES-002: a production setting does not apply to test
    /// code, whose default of 120 applies
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-LINES-002")]
    [Requirement("RH2008-001")]
    public async Task ProductionSettingDoesNotApplyToTestCode()
    {
        await VerifyLines(TwoAndThreeLineMethods.Replace("{|#0:N|}", "N"), LimitTwo, "build_property.IsTestProject = true");
    }

    /// <summary>
    /// Verifies the test-project part of the example of SHARED-LINES-002: the test setting applies to test code
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-LINES-002")]
    [Requirement("RH2008-001")]
    public async Task TestSettingAppliesToTestCode()
    {
        await VerifyLines(TwoAndThreeLineMethods, """{ "design": { "maximumTestMethodLines": 2 } }""", "build_property.IsTestProject = true", LineLimit(0, 3, 2));
    }

    /// <summary>
    /// Verifies the default production limit of RH2008-001 without a configuration file: 60 lines of code are not
    /// reported and 61 are, with the count and the limit in the message
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2008-001")]
    [Requirement("SHARED-LINES-002")]
    [Requirement("SHARED-LIMIT-002")]
    public async Task DefaultProductionLimitIsSixty()
    {
        await VerifyLines(CreateMethodsAround(60), null, LineLimit(0, 61, 60));
    }

    /// <summary>
    /// Verifies the default test limit of RH2008-001 without a configuration file: 120 lines of code are not reported
    /// and 121 are, with the count and the limit in the message
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2008-001")]
    [Requirement("SHARED-LINES-002")]
    [Requirement("SHARED-LIMIT-002")]
    public async Task DefaultTestLimitIsOneHundredTwenty()
    {
        await VerifyLines(CreateMethodsAround(120), null, "build_property.IsTestProject = true", LineLimit(0, 121, 120));
    }

    /// <summary>
    /// Verifies the example of RH2008-001 that a configured production setting alone leaves the test limit at its
    /// default: a test method with 70 lines of code is not reported, one with 121 is
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2008-001")]
    [Requirement("SHARED-LINES-002")]
    public async Task ConfiguredProductionSettingLeavesTestDefault()
    {
        var source = $"class C\n{{\n{CreateMethod("M70", 70, false)}\n{CreateMethod("M121", 121, true)}}}\n";

        await VerifyLines(source, """{ "design": { "maximumMethodLines": 40 } }""", "build_property.IsTestProject = true", LineLimit(0, 121, 120));
    }

    /// <summary>
    /// Verifies that a configured test setting alone leaves the production limit at its default of 60
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-LINES-002")]
    [Requirement("RH2008-001")]
    public async Task ConfiguredTestSettingLeavesProductionDefault()
    {
        await VerifyLines(CreateMethodsAround(60), """{ "design": { "maximumTestMethodLines": 30 } }""", LineLimit(0, 61, 60));
    }

    /// <summary>
    /// Verifies the example of RH2008-001 that a production setting RH0001 reports keeps the default: a production
    /// method with 70 lines of code is reported against 60
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2008-001")]
    [Requirement("SHARED-LINES-002")]
    [Requirement("SHARED-LIMIT-001")]
    public async Task ProductionSettingReportedByRH0001KeepsDefault()
    {
        var source = $"class C\n{{\n{CreateMethod("M70", 70, true)}}}\n";

        await VerifyLines(source, """{ "design": { "maximumMethodLines": "40" } }""", LineLimit(0, 70, 60));
    }

    /// <summary>
    /// Verifies that every setting without a valid value keeps the default production limit: 60 lines of code are
    /// not reported and 61 are
    /// </summary>
    /// <param name="configuration">Text of <c>reihitsu.json</c></param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-LINES-002")]
    [Requirement("RH2008-001")]
    [Requirement("SHARED-LIMIT-001")]
    [DataRow("""{ "design": { "maximumMethodLines": "40" } }""")]
    [DataRow("""{ "design": { "maximumMethodLines": 0 } }""")]
    [DataRow("""{ "design": { "maximumMethodLines": -5 } }""")]
    [DataRow("""{ "design": { "maximumMethodLines": 50.0 } }""")]
    [DataRow("""{ "design": { "maximumMethodLines": 40.5 } }""")]
    [DataRow("""{ "design": { "maximumMethodLines": 5e1 } }""")]
    [DataRow("""{ "design": { "maximumMethodLines": null } }""")]
    [DataRow("""{ "design": { "maximumMethodLines": true } }""")]
    [DataRow("""{ "design": { "maximumMethodLines": 2147483648 } }""")]
    [DataRow("")]
    [DataRow("   ")]
    [DataRow("{ } junk")]
    [DataRow("""{ "design": { "maximumMethodLines": 40 """)]
    public async Task InvalidSettingKeepsDefaultProductionLimit(string configuration)
    {
        await VerifyLines(CreateMethodsAround(60), configuration, LineLimit(0, 61, 60));
    }

    /// <summary>
    /// Verifies that a test setting without a valid value keeps the default test limit: 120 lines of code are not
    /// reported and 121 are
    /// </summary>
    /// <param name="configuration">Text of <c>reihitsu.json</c></param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-LINES-002")]
    [Requirement("RH2008-001")]
    [Requirement("SHARED-LIMIT-001")]
    [DataRow("""{ "design": { "maximumTestMethodLines": 0 } }""")]
    [DataRow("""{ "design": { "maximumTestMethodLines": "40" } }""")]
    [DataRow("{ } junk")]
    public async Task InvalidTestSettingKeepsDefaultTestLimit(string configuration)
    {
        await VerifyLines(CreateMethodsAround(120), configuration, "build_property.IsTestProject = true", LineLimit(0, 121, 120));
    }

    /// <summary>
    /// Verifies the largest valid limit: a body far below it is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-LINES-002")]
    [Requirement("SHARED-LIMIT-001")]
    public async Task LargestValidLimitTakesEffect()
    {
        await VerifyLines(CreateMethodsAround(60).Replace("{|#0:M61|}", "M61"), """{ "design": { "maximumMethodLines": 2147483647 } }""");
    }

    #endregion // Tests — limit and configuration (SHARED-LINES-002, RH2008-001)

    #region Tests — test code (SHARED-LINES-003)

    /// <summary>
    /// Verifies that the method of the SHARED-LINES-003 example is test code, judged against the test limit of three
    /// and not reported, when one of the two MSBuild properties is <c>true</c> in any letter case
    /// </summary>
    /// <param name="globalConfiguration">Global analyzer options of the project</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-LINES-003")]
    [DataRow("build_property.IsTestProject = true")]
    [DataRow("build_property.IsTestingPlatformApplication = true")]
    [DataRow("build_property.IsTestProject = true\nbuild_property.IsTestingPlatformApplication = false")]
    [DataRow("build_property.IsTestProject = false\nbuild_property.IsTestingPlatformApplication = true")]
    [DataRow("build_property.IsTestProject = \nbuild_property.IsTestingPlatformApplication = true")]
    [DataRow("build_property.IsTestProject = true\nbuild_property.IsTestingPlatformApplication = ")]
    [DataRow("build_property.IsTestProject = true\nbuild_property.IsTestingPlatformApplication = true")]
    [DataRow("build_property.IsTestProject = True")]
    [DataRow("build_property.IsTestProject = TRUE")]
    [DataRow("build_property.IsTestingPlatformApplication = True")]
    [DataRow("build_property.IsTestingPlatformApplication = TRUE")]
    public async Task MethodInTestProjectIsJudgedAgainstTestLimit(string globalConfiguration)
    {
        await VerifyLines(ThreeLineMethod.Replace("{|#0:M|}", "M"), ProductionTwoTestThree, globalConfiguration);
    }

    /// <summary>
    /// Verifies that the method of the SHARED-LINES-003 example is production code, judged against the production limit
    /// of two and reported, when neither MSBuild property is <c>true</c>
    /// </summary>
    /// <param name="globalConfiguration">Global analyzer options of the project, or <see langword="null"/> for none</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-LINES-003")]
    [DataRow("")]
    [DataRow("build_property.IsTestProject = false\nbuild_property.IsTestingPlatformApplication = false")]
    [DataRow("build_property.IsTestProject = \nbuild_property.IsTestingPlatformApplication = ")]
    [DataRow("build_property.IsTestProject = false")]
    [DataRow("build_property.IsTestingPlatformApplication = false")]
    [DataRow("build_property.IsTestProject = ")]
    [DataRow("build_property.IsTestingPlatformApplication = ")]
    [DataRow("build_property.IsTestProject = yes")]
    [DataRow("build_property.IsTestingPlatformApplication = yes")]
    [DataRow("build_property.IsTestProject = 1")]
    [DataRow("build_property.IsTestingPlatformApplication = 1")]
    [DataRow("build_property.IsTestApplication = true")]
    [DataRow("build_property.TestProject = true")]
    public async Task MethodOutsideTestProjectIsJudgedAgainstProductionLimit(string globalConfiguration)
    {
        await VerifyLines(ThreeLineMethod, ProductionTwoTestThree, globalConfiguration, LineLimit(0, 3, 2));
    }

    /// <summary>
    /// Verifies that the method of the SHARED-LINES-003 example is production code when the project passes no global
    /// analyzer options at all, as a project that loads the analyzer without the package's build files
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-LINES-003")]
    public async Task MethodWithoutGlobalOptionsIsProductionCode()
    {
        await VerifyLines(ThreeLineMethod, ProductionTwoTestThree, LineLimit(0, 3, 2));
    }

    /// <summary>
    /// Verifies that the test-code decision covers every declaration of the compilation, also in a second file and in a
    /// type that holds no test
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-LINES-003")]
    public async Task EveryFileOfTestProjectIsTestCode()
    {
        const string helper = """
                              static class OrderBuilder
                              {
                                  public static int Create()
                                  {
                                      var order = 1;
                                      order += 1;
                                      return order;
                                  }
                              }
                              """;

        await Verify(ThreeLineMethod.Replace("{|#0:M|}", "M"),
                     test =>
                     {
                         test.TestState.Sources.Add(("OrderBuilder.cs", helper));
                         test.TestState.AdditionalFiles.Add(("reihitsu.json", ProductionTwoTestThree));
                         test.TestState.AnalyzerConfigFiles.Add(("/.globalconfig", CreateGlobalConfiguration("build_property.IsTestingPlatformApplication = true")));
                     });
    }

    #endregion // Tests — test code (SHARED-LINES-003)

    #region Tests — judged members and counted span (RH2008-002)

    /// <summary>
    /// Verifies the first example of RH2008-002: a constructor, a property with an expression body, a <c>get</c>
    /// accessor, and an operator with three lines of code are reported, and a property with two lines is not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2008-002")]
    [Requirement("RH2008-005")]
    public async Task MembersWithBodyAreJudgedByTheirBody()
    {
        const string source = """
                              class C
                              {
                                  private int _value;

                                  public {|#0:C|}()
                                  {
                                      _value = 1;
                                      _value = 2;
                                      _value = 3;
                                  }

                                  public int {|#1:Sum|} => _value
                                                       + _value
                                                       + _value;

                                  public int Short
                                      => _value
                                         + _value;

                                  public int Value
                                  {
                                      {|#2:get|}
                                      {
                                          var result = _value;
                                          result += _value;
                                          return result;
                                      }
                                  }

                                  public static C operator {|#3:+|}(C left, C right)
                                  {
                                      left._value += right._value;
                                      left._value += right._value;
                                      return left;
                                  }
                              }
                              """;

        await VerifyLines(source, LimitTwo, LineLimit(0, 3, 2), LineLimit(1, 3, 2), LineLimit(2, 3, 2), LineLimit(3, 3, 2));
    }

    /// <summary>
    /// Verifies the second example of RH2008-002: attributes, a parameter list over three lines, and the braces are not
    /// counted
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2008-002")]
    public async Task SignatureAndBracesAreNotCounted()
    {
        const string source = """
                              class C
                              {
                                  [System.Obsolete]
                                  void M(
                                      int first,
                                      int second)
                                  {
                                      M(first,
                                        second);
                                  }
                              }
                              """;

        await VerifyLines(source, LimitTwo);
    }

    /// <summary>
    /// Verifies that the second example of RH2008-002 is reported once the limit is below its count of two, so the
    /// method is judged at all
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2008-002")]
    public async Task SignatureExampleIsReportedBelowItsCount()
    {
        const string source = """
                              class C
                              {
                                  [System.Obsolete]
                                  void {|#0:M|}<T>(
                                      int first,
                                      int second)
                                      where T : class
                                  {
                                      M<T>(first,
                                           second);
                                  }
                              }
                              """;

        await VerifyLines(source, LimitOne, LineLimit(0, 2, 1));
    }

    /// <summary>
    /// Verifies that the constructor initializer is not counted
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2008-002")]
    public async Task ConstructorInitializerIsNotCounted()
    {
        const string source = """
                              class B
                              {
                                  public B(int first, int second)
                                  {
                                  }
                              }

                              class C : B
                              {
                                  public C()
                                      : base(1,
                                             2)
                                  {
                                      System.Console.WriteLine();
                                  }
                              }
                              """;

        await VerifyLines(source, LimitOne);
    }

    /// <summary>
    /// Verifies that a body whose tokens stand on the lines of its braces counts those lines
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2008-002")]
    [Requirement("SHARED-LINES-001")]
    public async Task TokensOnTheLinesOfTheBracesAreCounted()
    {
        const string source = """
                              class C
                              {
                                  void {|#0:M|}() { M();
                                      M(); }
                              }
                              """;

        await VerifyLines(source, LimitOne, LineLimit(0, 2, 1));
    }

    /// <summary>
    /// Verifies that a body written on one line holds one line of code however many statements it holds, and an empty
    /// body holds none
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2008-002")]
    [Requirement("SHARED-LINES-001")]
    public async Task OneLineBodyAndEmptyBodyAreAtOrBelowLimitOfOne()
    {
        const string source = """
                              class C
                              {
                                  void M() { M(); M(); M(); }

                                  void N()
                                  {
                                  }

                                  void O() { }
                              }
                              """;

        await VerifyLines(source, LimitOne);
    }

    /// <summary>
    /// Verifies that a method with a block body is judged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2008-002")]
    public async Task MethodWithBlockBodyIsJudged()
    {
        const string source = """
                              class C
                              {
                                  void {|#0:M|}()
                                  {
                                      M();
                                      M();
                                  }
                              }
                              """;

        await VerifyLines(source, LimitOne, LineLimit(0, 2, 1));
    }

    /// <summary>
    /// Verifies that an async method returning a task is judged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2008-002")]
    public async Task AsyncMethodIsJudged()
    {
        const string source = """
                              using System.Threading.Tasks;

                              class C
                              {
                                  async Task<int> {|#0:M|}()
                                  {
                                      await Task.Yield();
                                      return 1;
                                  }

                                  async ValueTask<int> {|#1:N|}() => await M()
                                                                    + 1;
                              }
                              """;

        await VerifyLines(source, LimitOne, LineLimit(0, 2, 1), LineLimit(1, 2, 1));
    }

    /// <summary>
    /// Verifies that a finalizer is judged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2008-002")]
    public async Task FinalizerIsJudged()
    {
        const string source = """
                              class C
                              {
                                  private int _value;

                                  ~{|#0:C|}()
                                  {
                                      _value = 0;
                                      _value = 1;
                                  }
                              }
                              """;

        await VerifyLines(source, LimitOne, LineLimit(0, 2, 1));
    }

    /// <summary>
    /// Verifies that a conversion operator is judged, implicit and explicit
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2008-002")]
    public async Task ConversionOperatorIsJudged()
    {
        const string source = """
                              class C
                              {
                                  private int _value;

                                  public static implicit {|#0:operator|} int(C value)
                                  {
                                      var result = value._value;
                                      return result;
                                  }

                                  public static explicit {|#1:operator|} long(C value)
                                  {
                                      var result = value._value;
                                      return result;
                                  }
                              }
                              """;

        await VerifyLines(source, LimitOne, LineLimit(0, 2, 1), LineLimit(1, 2, 1));
    }

    /// <summary>
    /// Verifies that the <c>set</c> and <c>init</c> accessors are judged, each on its own
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2008-002")]
    [Requirement("RH2008-005")]
    public async Task SetAndInitAccessorsAreJudged()
    {
        const string source = """
                              class C
                              {
                                  private int _value;

                                  public int First
                                  {
                                      get => _value;
                                      {|#0:set|}
                                      {
                                          var input = value;
                                          _value = input;
                                      }
                                  }

                                  public int Second
                                  {
                                      get => _value;
                                      {|#1:init|}
                                      {
                                          var input = value;
                                          _value = input;
                                      }
                                  }
                              }
                              """;

        await VerifyLines(source, LimitOne, LineLimit(0, 2, 1), LineLimit(1, 2, 1));
    }

    /// <summary>
    /// Verifies that the <c>add</c> and <c>remove</c> accessors of an event are judged, each on its own
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2008-002")]
    [Requirement("RH2008-005")]
    public async Task EventAccessorsAreJudged()
    {
        const string source = """
                              using System;

                              class C
                              {
                                  private Action _handler;

                                  public event Action Changed
                                  {
                                      {|#0:add|}
                                      {
                                          var handler = _handler;
                                          _handler = handler + value;
                                      }
                                      {|#1:remove|}
                                      {
                                          var handler = _handler;
                                          _handler = handler - value;
                                      }
                                  }
                              }
                              """;

        await VerifyLines(source, LimitOne, LineLimit(0, 2, 1), LineLimit(1, 2, 1));
    }

    /// <summary>
    /// Verifies that a method, a constructor, a finalizer, an operator, a conversion operator, and an accessor with an
    /// expression body are judged by the <c>=&gt;</c> and the lines of their expression
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2008-002")]
    public async Task ExpressionBodiedMembersAreJudged()
    {
        const string source = """
                              class C
                              {
                                  private int _value;

                                  public {|#0:C|}() => _value = A(
                                      1);

                                  ~{|#1:C|}() => _value = A(
                                      0);

                                  int {|#2:M|}() => A(
                                      1);

                                  public static C operator {|#3:-|}(C value) => Create(
                                      value);

                                  public static implicit {|#4:operator|} int(C value) => A(
                                      value._value);

                                  public int Value
                                  {
                                      {|#5:get|} => A(
                                          _value);
                                      {|#6:set|} => _value = A(
                                          value);
                                  }

                                  static int A(int value) => value;

                                  static C Create(C value) => value;
                              }
                              """;

        await VerifyLines(source,
                          LimitOne,
                          LineLimit(0, 2, 1),
                          LineLimit(1, 2, 1),
                          LineLimit(2, 2, 1),
                          LineLimit(3, 2, 1),
                          LineLimit(4, 2, 1),
                          LineLimit(5, 2, 1),
                          LineLimit(6, 2, 1));
    }

    /// <summary>
    /// Verifies that an indexer with an expression body is judged by its body
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2008-002")]
    public async Task ExpressionBodiedIndexerIsJudged()
    {
        const string source = """
                              class C
                              {
                                  public int {|#0:this|}[int index] => index
                                                                + 1;
                              }
                              """;

        await VerifyLines(source, LimitOne, LineLimit(0, 2, 1));
    }

    /// <summary>
    /// Verifies that the line of the <c>=&gt;</c> counts and the line of a <c>;</c> standing alone behind an
    /// expression body does not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2008-002")]
    public async Task SemicolonAloneBehindExpressionBodyIsNotCounted()
    {
        const string source = """
                              class C
                              {
                                  public int {|#0:P|}
                                      => A(
                                          1)
                                      ;

                                  int {|#1:M|}()
                                      =>
                                      A(1)
                                      ;

                                  static int A(int value) => value;
                              }
                              """;

        await VerifyLines(source, LimitOne, LineLimit(0, 2, 1), LineLimit(1, 2, 1));
    }

    /// <summary>
    /// Verifies that an expression body on one line holds one line of code, whether the <c>=&gt;</c> stands on the line
    /// of the signature or on a line of its own
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2008-002")]
    public async Task OneLineExpressionBodyIsAtTheLimitOfOne()
    {
        const string source = """
                              class C
                              {
                                  int M() => A(1) + A(2);

                                  int N()
                                      => A(1) + A(2);

                                  int P => A(1);

                                  static int A(int value) => value;
                              }
                              """;

        await VerifyLines(source, LimitOne);
    }

    /// <summary>
    /// Verifies that a member of an extension block is judged like any other member
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2008-002")]
    public async Task MemberOfExtensionBlockIsJudged()
    {
        const string source = """
                              static class TextExtensions
                              {
                                  extension(string text)
                                  {
                                      public int {|#0:Twice|}()
                                      {
                                          var length = text.Length;
                                          return length * 2;
                                      }
                                  }
                              }
                              """;

        await VerifyLines(source, LimitOne, LineLimit(0, 2, 1));
    }

    /// <summary>
    /// Verifies that an explicit interface implementation of a method and of an accessor is judged, on the method's
    /// identifier and on the accessor keyword
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2008-002")]
    [Requirement("RH2008-005")]
    public async Task ExplicitInterfaceImplementationIsJudged()
    {
        const string source = """
                              interface I
                              {
                                  void M();

                                  int P { get; }
                              }

                              class C : I
                              {
                                  void I.{|#0:M|}()
                                  {
                                      System.Console.WriteLine();
                                      System.Console.WriteLine();
                                  }

                                  int I.P
                                  {
                                      {|#1:get|}
                                      {
                                          var result = 1;
                                          return result;
                                      }
                                  }
                              }
                              """;

        await VerifyLines(source, LimitOne, LineLimit(0, 2, 1), LineLimit(1, 2, 1));
    }

    /// <summary>
    /// Verifies that an interface member with a default implementation is judged, and one without a body is not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2008-002")]
    [Requirement("RH2008-004")]
    public async Task InterfaceMemberWithDefaultImplementationIsJudged()
    {
        const string source = """
                              interface I
                              {
                                  void N();

                                  static abstract void S();

                                  void {|#0:M|}()
                                  {
                                      N();
                                      N();
                                  }
                              }
                              """;

        await VerifyLines(source, LimitOne, LineLimit(0, 2, 1));
    }

    /// <summary>
    /// Verifies that the members of a generic type, a struct, a record, and a <c>file</c> type are judged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2008-002")]
    public async Task MembersOfEveryTypeKindAreJudged()
    {
        const string source = """
                              class G<T>
                              {
                                  void {|#0:M|}()
                                  {
                                      M();
                                      M();
                                  }
                              }

                              struct S
                              {
                                  void {|#1:M|}()
                                  {
                                      M();
                                      M();
                                  }
                              }

                              record R
                              {
                                  void {|#2:M|}()
                                  {
                                      M();
                                      M();
                                  }
                              }

                              file class F
                              {
                                  void {|#3:M|}()
                                  {
                                      M();
                                      M();
                                  }
                              }
                              """;

        await VerifyLines(source, LimitOne, LineLimit(0, 2, 1), LineLimit(1, 2, 1), LineLimit(2, 2, 1), LineLimit(3, 2, 1));
    }

    #endregion // Tests — judged members and counted span (RH2008-002)

    #region Tests — nested functions (RH2008-003)

    /// <summary>
    /// Verifies the first example of RH2008-003: the lambda and the local function count toward the member, the local
    /// function is judged on its own and holds three lines, and the lambda is not judged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2008-003")]
    public async Task NestedFunctionsCountTowardTheMember()
    {
        const string source = """
                              class C
                              {
                                  void {|#0:M|}()
                                  {
                                      System.Action action = () =>
                                      {
                                          Local();
                                      };

                                      void Local()
                                      {
                                          M();
                                          M();
                                          M();
                                      }
                                  }
                              }
                              """;

        await VerifyLines(source, LimitThree, LineLimit(0, 10, 3));
    }

    /// <summary>
    /// Verifies the first example of RH2008-003 at a limit of two: the local function is reported as well, and the
    /// member that holds it is reported on its own identifier
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2008-003")]
    [Requirement("RH2008-005")]
    public async Task LocalFunctionIsJudgedOnItsOwn()
    {
        const string source = """
                              class C
                              {
                                  void {|#0:M|}()
                                  {
                                      System.Action action = () =>
                                      {
                                          Local();
                                      };

                                      void {|#1:Local|}()
                                      {
                                          M();
                                          M();
                                          M();
                                      }
                                  }
                              }
                              """;

        await VerifyLines(source, LimitTwo, LineLimit(0, 10, 2), LineLimit(1, 3, 2));
    }

    /// <summary>
    /// Verifies the second example of RH2008-003: a lambda in a field initializer is not judged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2008-003")]
    public async Task LambdaInFieldInitializerIsNotJudged()
    {
        const string source = """
                              using System;

                              class C
                              {
                                  private static readonly Func<int, int> Compute = value =>
                                  {
                                      var result = value;
                                      result += 1;
                                      return result;
                                  };
                              }
                              """;

        await VerifyLines(source, LimitTwo);
    }

    /// <summary>
    /// Verifies that a lambda and an anonymous method in a property initializer, a field initializer, and an argument of
    /// a constructor initializer are not judged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2008-003")]
    public async Task LambdaOutsideEveryBodyIsNotJudged()
    {
        const string source = """
                              using System;

                              class B
                              {
                                  public B(Func<int> factory)
                                  {
                                  }
                              }

                              class C : B
                              {
                                  public Func<int> Factory { get; } = () =>
                                  {
                                      var result = 1;
                                      result += 1;
                                      return result;
                                  };

                                  private readonly Func<int> _other = delegate
                                  {
                                      var result = 1;
                                      result += 1;
                                      return result;
                                  };

                                  public C()
                                      : base(() =>
                                             {
                                                 var result = 1;
                                                 result += 1;
                                                 return result;
                                             })
                                  {
                                  }
                              }
                              """;

        await VerifyLines(source, LimitOne);
    }

    /// <summary>
    /// Verifies that a local function inside a local function counts toward both containers and each is judged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2008-003")]
    public async Task LocalFunctionInLocalFunctionCountsTowardBoth()
    {
        const string source = """
                              class C
                              {
                                  void {|#0:M|}()
                                  {
                                      void {|#1:Outer|}()
                                      {
                                          void {|#2:Inner|}()
                                          {
                                              M();
                                              M();
                                          }
                                      }
                                  }
                              }
                              """;

        await VerifyLines(source, LimitOne, LineLimit(0, 8, 1), LineLimit(1, 5, 1), LineLimit(2, 2, 1));
    }

    /// <summary>
    /// Verifies that a lambda in a lambda, an anonymous method, and a local function inside a lambda count toward the
    /// member, and that only the local function is judged on its own
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2008-003")]
    public async Task FunctionsInsideLambdasCountTowardTheMember()
    {
        const string source = """
                              using System;

                              class C
                              {
                                  void {|#0:M|}()
                                  {
                                      Action outer = () =>
                                      {
                                          Action inner = () =>
                                          {
                                              M();
                                              M();
                                          };
                                          void {|#1:Local|}()
                                          {
                                              M();
                                              M();
                                          }
                                      };
                                      Action anonymous = delegate
                                      {
                                          M();
                                          M();
                                      };
                                  }
                              }
                              """;

        await VerifyLines(source, LimitOne, LineLimit(0, 18, 1), LineLimit(1, 2, 1));
    }

    /// <summary>
    /// Verifies that a local function inside an accessor counts toward the accessor and is judged on its own
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2008-003")]
    public async Task LocalFunctionInAccessorCountsTowardTheAccessor()
    {
        const string source = """
                              class C
                              {
                                  public int Value
                                  {
                                      {|#0:get|}
                                      {
                                          return Local();

                                          int {|#1:Local|}()
                                          {
                                              var result = 1;
                                              return result;
                                          }
                                      }
                                  }
                              }
                              """;

        await VerifyLines(source, LimitOne, LineLimit(0, 6, 1), LineLimit(1, 2, 1));
    }

    /// <summary>
    /// Verifies that a local function in top-level statements is judged on its own, while the top-level statements are
    /// not judged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2008-002")]
    [Requirement("RH2008-003")]
    public async Task LocalFunctionInTopLevelStatementsIsJudged()
    {
        const string source = """
                              System.Console.WriteLine(1);
                              System.Console.WriteLine(2);
                              System.Console.WriteLine(3);

                              static void {|#0:Local|}()
                              {
                                  System.Console.WriteLine(1);
                                  System.Console.WriteLine(2);
                              }
                              """;

        await Verify(source,
                     test =>
                     {
                         test.TestState.AdditionalFiles.Add(("reihitsu.json", LimitOne));
                         test.TestState.OutputKind = OutputKind.ConsoleApplication;
                     },
                     LineLimit(0, 2, 1));
    }

    #endregion // Tests — nested functions (RH2008-003)

    #region Tests — members without a body (RH2008-004)

    /// <summary>
    /// Verifies the first example of RH2008-004: an abstract method, an auto-property, and the defining part of a
    /// partial method are not reported, and the implementing part is
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2008-004")]
    public async Task MembersWithoutBodyAreNotReported()
    {
        const string source = """
                              abstract partial class C
                              {
                                  public abstract void A();

                                  public int P { get; set; }

                                  public partial void M();

                                  public partial void {|#0:M|}()
                                  {
                                      A();
                                      A();
                                  }
                              }
                              """;

        await VerifyLines(source, LimitOne, LineLimit(0, 2, 1));
    }

    /// <summary>
    /// Verifies the second example of RH2008-004: each accessor of a property is judged on its own and the property is
    /// not judged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2008-004")]
    public async Task AccessorsAreNotSummed()
    {
        const string source = """
                              class C
                              {
                                  private int _value;

                                  public int Value
                                  {
                                      get
                                      {
                                          var result = _value;
                                          result += 1;
                                          return result;
                                      }
                                      set
                                      {
                                          var input = value;
                                          input -= 1;
                                          _value = input;
                                      }
                                  }
                              }
                              """;

        await VerifyLines(source, """{ "design": { "maximumMethodLines": 4 } }""");
    }

    /// <summary>
    /// Verifies that the accessors of a property are reported one by one, on their keywords, and the property is not
    /// reported on its name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2008-004")]
    [Requirement("RH2008-005")]
    public async Task AccessorsOverTheLimitAreReportedOnTheirKeywords()
    {
        const string source = """
                              class C
                              {
                                  private int _value;

                                  public int Value
                                  {
                                      {|#0:get|}
                                      {
                                          var result = _value;
                                          result += 1;
                                          return result;
                                      }
                                      {|#1:set|}
                                      {
                                          var input = value;
                                          input -= 1;
                                          _value = input;
                                      }
                                  }
                              }
                              """;

        await VerifyLines(source, LimitTwo, LineLimit(0, 3, 2), LineLimit(1, 3, 2));
    }

    /// <summary>
    /// Verifies that an <c>extern</c> method, an abstract property, an interface indexer, an abstract event, and
    /// auto-accessors are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2008-004")]
    public async Task ExternAbstractAndInterfaceMembersAreNotReported()
    {
        const string source = """
                              using System;
                              using System.Runtime.InteropServices;

                              interface I
                              {
                                  int this[int index] { get; set; }
                              }

                              abstract class C
                              {
                                  [DllImport("native")]
                                  public static extern int Native(
                                      int first,
                                      int second);

                                  public abstract int P
                                  {
                                      get;
                                      set;
                                  }

                                  public abstract event Action E;

                                  public int Q
                                  {
                                      get;
                                      init;
                                  }
                              }
                              """;

        await VerifyLines(source, LimitOne);
    }

    /// <summary>
    /// Verifies that a partial property, a partial constructor, and a partial event are judged by the part that holds
    /// the body, and the defining part is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2008-004")]
    public async Task PartialMembersAreJudgedByTheImplementingPart()
    {
        const string source = """
                              using System;

                              partial class C
                              {
                                  private Action _handler;

                                  public partial int P { get; }

                                  public partial int P
                                  {
                                      {|#0:get|}
                                      {
                                          var result = 1;
                                          return result;
                                      }
                                  }

                                  public partial C();

                                  public partial {|#1:C|}()
                                  {
                                      _handler = null;
                                      _handler = null;
                                  }

                                  public partial event Action E;

                                  public partial event Action E
                                  {
                                      {|#2:add|}
                                      {
                                          var handler = _handler;
                                          _handler = handler + value;
                                      }
                                      remove
                                      {
                                          _handler -= value;
                                      }
                                  }
                              }
                              """;

        await VerifyLines(source, LimitOne, LineLimit(0, 2, 1), LineLimit(1, 2, 1), LineLimit(2, 2, 1));
    }

    #endregion // Tests — members without a body (RH2008-004)

    #region Tests — location (RH2008-005)

    /// <summary>
    /// Verifies the example of RH2008-005: a finalizer on its identifier, an indexer on <c>this</c>, and a conversion
    /// operator on the <c>operator</c> keyword
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2008-005")]
    public async Task MembersWithoutIdentifierAreReportedOnTheirNamingToken()
    {
        const string source = """
                              class C
                              {
                                  private int _value;

                                  ~{|#0:C|}()
                                  {
                                      _value = 0;
                                      _value = 1;
                                  }

                                  public int {|#1:this|}[int index] => index
                                                                  + _value;

                                  public static explicit {|#2:operator|} int(C value)
                                  {
                                      var result = value._value;
                                      return result;
                                  }
                              }
                              """;

        await VerifyLines(source, LimitOne, LineLimit(0, 2, 1), LineLimit(1, 2, 1), LineLimit(2, 2, 1));
    }

    /// <summary>
    /// Verifies that an equality operator is reported on its operator token
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2008-005")]
    public async Task EqualityOperatorIsReportedOnItsToken()
    {
        const string source = """
                              class C
                              {
                                  public static bool operator {|#0:==|}(C left, C right)
                                  {
                                      var same = ReferenceEquals(left, right);
                                      return same;
                                  }

                                  public static bool operator !=(C left, C right) => !(left == right);

                                  public override bool Equals(object obj) => ReferenceEquals(this, obj);

                                  public override int GetHashCode() => 0;
                              }
                              """;

        await VerifyLines(source, LimitOne, LineLimit(0, 2, 1));
    }

    /// <summary>
    /// Verifies that a verbatim identifier locates the diagnostic on the whole identifier token
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2008-005")]
    public async Task VerbatimIdentifierIsReportedOnTheWholeToken()
    {
        const string source = """
                              class C
                              {
                                  void {|#0:@class|}()
                                  {
                                      @class();
                                      @class();
                                  }
                              }
                              """;

        await VerifyLines(source, LimitOne, LineLimit(0, 2, 1));
    }

    #endregion // Tests — location (RH2008-005)

    #region Methods

    /// <summary>
    /// Creates the expected diagnostic at a markup location with the message that states the count and the limit
    /// </summary>
    /// <param name="markupKey">Markup key of the location</param>
    /// <param name="count">Lines of code of the member's body</param>
    /// <param name="limit">Limit that applies</param>
    /// <returns>Expected diagnostic</returns>
    private static DiagnosticResult LineLimit(int markupKey, int count, int limit)
    {
        return Diagnostic(RH2008MethodsShouldNotExceedTheLineLimitAnalyzer.DiagnosticId).WithLocation(markupKey, DiagnosticLocationOptions.InterpretAsMarkupKey)
                                                                                        .WithMessage(string.Format(MessageFormat, count, limit));
    }

    /// <summary>
    /// Verifies the analyzer in a production project with the given configuration file
    /// </summary>
    /// <param name="source">Source with markup</param>
    /// <param name="configuration">Text of <c>reihitsu.json</c>, or <see langword="null"/> for no file</param>
    /// <param name="expected">Expected diagnostics</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private static async Task VerifyLines(string source, string configuration, params DiagnosticResult[] expected)
    {
        await VerifyLines(source, configuration, null, expected);
    }

    /// <summary>
    /// Verifies the analyzer with the given configuration file and global analyzer options
    /// </summary>
    /// <param name="source">Source with markup</param>
    /// <param name="configuration">Text of <c>reihitsu.json</c>, or <see langword="null"/> for no file</param>
    /// <param name="globalConfiguration">Lines of the global analyzer config after <c>is_global = true</c>, or <see langword="null"/> for none</param>
    /// <param name="expected">Expected diagnostics</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private static async Task VerifyLines(string source, string configuration, string globalConfiguration, params DiagnosticResult[] expected)
    {
        await Verify(source,
                     test =>
                     {
                         if (configuration != null)
                         {
                             test.TestState.AdditionalFiles.Add(("reihitsu.json", configuration));
                         }

                         if (globalConfiguration != null)
                         {
                             test.TestState.AnalyzerConfigFiles.Add(("/.globalconfig", CreateGlobalConfiguration(globalConfiguration)));
                         }
                     },
                     expected);
    }

    /// <summary>
    /// Creates the text of a global analyzer config, as the build writes the MSBuild properties it makes visible
    /// </summary>
    /// <param name="lines">Lines after <c>is_global = true</c></param>
    /// <returns>Text of the global analyzer config</returns>
    private static string CreateGlobalConfiguration(string lines)
    {
        return lines.Length == 0
                   ? "is_global = true\n"
                   : $"is_global = true\n{lines}\n";
    }

    /// <summary>
    /// Creates a type with a method of exactly <paramref name="limit"/> lines of code, not marked, and one of
    /// <paramref name="limit"/> + 1 lines, marked with the markup key 0
    /// </summary>
    /// <param name="limit">Limit the two methods lie at and above</param>
    /// <returns>Source with markup</returns>
    private static string CreateMethodsAround(int limit)
    {
        return $"class C\n{{\n{CreateMethod($"M{limit}", limit, false)}\n{CreateMethod($"M{limit + 1}", limit + 1, true)}}}\n";
    }

    /// <summary>
    /// Creates a method whose block body holds the given number of lines of code, one statement per line
    /// </summary>
    /// <param name="name">Name of the method</param>
    /// <param name="lines">Lines of code of the body</param>
    /// <param name="isMarked">Whether the name is marked with the markup key 0</param>
    /// <returns>Source of the method</returns>
    private static string CreateMethod(string name, int lines, bool isMarked)
    {
        var builder = new StringBuilder();

        builder.Append("    void ")
               .Append(isMarked ? $"{{|#0:{name}|}}" : name)
               .Append("()\n    {\n");

        for (var line = 0; line < lines; line++)
        {
            builder.Append("        ").Append(name).Append("();\n");
        }

        builder.Append("    }\n");

        return builder.ToString();
    }

    /// <summary>
    /// Writes every line break of the text as the given line ending
    /// </summary>
    /// <param name="text">Text</param>
    /// <param name="endOfLine">Line ending</param>
    /// <returns>Text with the line ending</returns>
    private static string WithEndOfLine(string text, string endOfLine)
    {
        return text.Replace("\r\n", "\n").Replace("\n", endOfLine);
    }

    /// <summary>
    /// Creates a transform that defines the given preprocessor symbols for the test project
    /// </summary>
    /// <param name="symbols">Preprocessor symbols</param>
    /// <returns>Transform to add to the test's solution transforms</returns>
    private static Func<Solution, ProjectId, Solution> DefineSymbols(params string[] symbols)
    {
        return (solution, projectId) => solution.GetProject(projectId)?.ParseOptions is CSharpParseOptions parseOptions
                                            ? solution.WithProjectParseOptions(projectId, parseOptions.WithPreprocessorSymbols(symbols))
                                            : solution;
    }

    #endregion // Methods
}