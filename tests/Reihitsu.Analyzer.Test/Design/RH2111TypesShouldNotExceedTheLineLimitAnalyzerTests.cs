using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Design;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Design;

/// <summary>
/// Test methods for <see cref="RH2111TypesShouldNotExceedTheLineLimitAnalyzer"/>
/// </summary>
[TestClass]
public class RH2111TypesShouldNotExceedTheLineLimitAnalyzerTests : AnalyzerTestsBase<RH2111TypesShouldNotExceedTheLineLimitAnalyzer>
{
    #region Constants

    /// <summary>
    /// Configuration with a production limit of one line of code
    /// </summary>
    private const string LimitOne = """{ "design": { "maximumTypeLines": 1 } }""";

    /// <summary>
    /// Configuration with a production limit of two lines of code
    /// </summary>
    private const string LimitTwo = """{ "design": { "maximumTypeLines": 2 } }""";

    /// <summary>
    /// Configuration with a production limit of two and a test limit of three lines of code
    /// </summary>
    private const string ProductionTwoTestThree = """{ "design": { "maximumTypeLines": 2, "maximumTestTypeLines": 3 } }""";

    /// <summary>
    /// A type whose body holds two lines of code, and one whose body holds three
    /// </summary>
    private const string TwoAndThreeLineTypes = """
                                                class C
                                                {
                                                    public int A;
                                                    public int B;
                                                }

                                                class {|#0:D|}
                                                {
                                                    public int A;
                                                    public int B;
                                                    public int C;
                                                }
                                                """;

    /// <summary>
    /// A type whose body holds three lines of code, the type case of the SHARED-LINES-003 example
    /// </summary>
    private const string ThreeLineType = """
                                         class {|#0:C|}
                                         {
                                             public int A;
                                             public int B;
                                             public int D;
                                         }
                                         """;

    #endregion // Constants

    #region Tests — limit and configuration (RH2111-001, SHARED-LINES-002)

    /// <summary>
    /// Verifies the default production limit of RH2111-001 without a configuration file: 1500 lines of code are not
    /// reported and 1501 are, with the count and the limit in the message
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2111-001")]
    [Requirement("SHARED-LINES-002")]
    [Requirement("SHARED-LIMIT-002")]
    public async Task DefaultProductionLimitIsFifteenHundred()
    {
        await VerifyLines(CreateTypesAround(1500), null, LineLimit(0, 1501, 1500));
    }

    /// <summary>
    /// Verifies the default test limit of RH2111-001 without a configuration file: 3000 lines of code are not reported
    /// and 3001 are, with the count and the limit in the message
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2111-001")]
    [Requirement("SHARED-LINES-002")]
    [Requirement("SHARED-LIMIT-002")]
    public async Task DefaultTestLimitIsThreeThousand()
    {
        await VerifyLines(CreateTypesAround(3000), null, "build_property.IsTestProject = true", LineLimit(0, 3001, 3000));
    }

    /// <summary>
    /// Verifies the example of RH2111-001 that a configured production setting alone leaves the test limit at its
    /// default: a test class with 700 lines of code is not reported, one with 3001 is
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2111-001")]
    [Requirement("SHARED-LINES-002")]
    public async Task ConfiguredProductionSettingLeavesTestDefault()
    {
        var source = $"{CreateType("T700", 700, false)}\n{CreateType("T3001", 3001, true)}";

        await VerifyLines(source, """{ "design": { "maximumTypeLines": 400 } }""", "build_property.IsTestingPlatformApplication = true", LineLimit(0, 3001, 3000));
    }

    /// <summary>
    /// Verifies that a configured test setting alone leaves the production limit at its default of 1500
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2111-001")]
    [Requirement("SHARED-LINES-002")]
    public async Task ConfiguredTestSettingLeavesProductionDefault()
    {
        await VerifyLines(CreateTypesAround(1500), """{ "design": { "maximumTestTypeLines": 300 } }""", LineLimit(0, 1501, 1500));
    }

    /// <summary>
    /// Verifies the example of RH2111-001 that a setting RH0001 reports keeps the default: at
    /// <c>"maximumTypeLines": 0</c>, a production type with 1500 lines of code is not reported and one with 1501 is
    /// </summary>
    /// <param name="configuration">Text of <c>reihitsu.json</c></param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2111-001")]
    [Requirement("SHARED-LINES-002")]
    [Requirement("SHARED-LIMIT-001")]
    [DataRow("""{ "design": { "maximumTypeLines": 0 } }""")]
    [DataRow("""{ "design": { "maximumTypeLines": "400" } }""")]
    [DataRow("""{ "design": { "maximumTypeLines": 400.0 } }""")]
    [DataRow("""{ "design": { "maximumTypeLines": 4e2 } }""")]
    [DataRow("""{ "design": { "maximumTypeLines": null } }""")]
    [DataRow("")]
    [DataRow("{ } junk")]
    public async Task InvalidSettingKeepsDefaultProductionLimit(string configuration)
    {
        await VerifyLines(CreateTypesAround(1500), configuration, LineLimit(0, 1501, 1500));
    }

    /// <summary>
    /// Verifies that an invalid test setting keeps the default test limit of 3000
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2111-001")]
    [Requirement("SHARED-LINES-002")]
    [Requirement("SHARED-LIMIT-001")]
    public async Task InvalidTestSettingKeepsDefaultTestLimit()
    {
        await VerifyLines(CreateTypesAround(3000), """{ "design": { "maximumTestTypeLines": 0 } }""", "build_property.IsTestProject = true", LineLimit(0, 3001, 3000));
    }

    /// <summary>
    /// Verifies the configured production limit: a body at the limit is not reported and one above it is
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2111-001")]
    [Requirement("SHARED-LINES-002")]
    [Requirement("SHARED-LIMIT-002")]
    public async Task ConfiguredLimitReportsOnlyTheBodyAboveIt()
    {
        await VerifyLines(TwoAndThreeLineTypes, LimitTwo, LineLimit(0, 3, 2));
    }

    /// <summary>
    /// Verifies that the production setting does not apply to test code and the test setting does
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2111-001")]
    [Requirement("SHARED-LINES-002")]
    public async Task TestSettingAndNotProductionSettingAppliesToTestCode()
    {
        await VerifyLines(TwoAndThreeLineTypes.Replace("{|#0:D|}", "D"), LimitTwo, "build_property.IsTestProject = true");
        await VerifyLines(TwoAndThreeLineTypes, """{ "design": { "maximumTestTypeLines": 2 } }""", "build_property.IsTestProject = true", LineLimit(0, 3, 2));
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
    [DataRow("""{ "design": { "maximumTypeLines": 2 }, "formatting": 1 }""")]
    [DataRow("""{ "design": { "maximumTypeLines": 2, "maximumMethodLines": "60" } }""")]
    [DataRow("""{ "design": { "maximumTypeLines": 2, "maximumFileLines": 900 } }""")]
    public async Task ProblemElsewhereDoesNotKeepValidLimitFromTakingEffect(string configuration)
    {
        await VerifyLines(TwoAndThreeLineTypes, configuration, LineLimit(0, 3, 2));
    }

    /// <summary>
    /// Verifies the last example of RH2111-001: without a configuration file, the types of RH2111-002 lie below both
    /// defaults and are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2111-001")]
    public async Task ExampleTypesAreNotReportedAtTheDefault()
    {
        await VerifyLines(RegionExample.Replace("{|#0:C|}", "C"), null);
    }

    #endregion // Tests — limit and configuration (RH2111-001, SHARED-LINES-002)

    #region Tests — test code (SHARED-LINES-003)

    /// <summary>
    /// Verifies that a type in a project for which one of the two MSBuild properties is <c>true</c> is test code,
    /// judged against the test limit of three and not reported
    /// </summary>
    /// <param name="globalConfiguration">Global analyzer options of the project</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-LINES-003")]
    [DataRow("build_property.IsTestProject = true")]
    [DataRow("build_property.IsTestingPlatformApplication = true")]
    [DataRow("build_property.IsTestProject = true\nbuild_property.IsTestingPlatformApplication = false")]
    [DataRow("build_property.IsTestProject = false\nbuild_property.IsTestingPlatformApplication = true")]
    [DataRow("build_property.IsTestProject = TRUE")]
    [DataRow("build_property.IsTestingPlatformApplication = True")]
    public async Task TypeInTestProjectIsJudgedAgainstTestLimit(string globalConfiguration)
    {
        await VerifyLines(ThreeLineType.Replace("{|#0:C|}", "C"), ProductionTwoTestThree, globalConfiguration);
    }

    /// <summary>
    /// Verifies that a type in a project for which neither MSBuild property is <c>true</c> is production code, judged
    /// against the production limit of two and reported
    /// </summary>
    /// <param name="globalConfiguration">Global analyzer options of the project</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-LINES-003")]
    [DataRow("")]
    [DataRow("build_property.IsTestProject = false\nbuild_property.IsTestingPlatformApplication = false")]
    [DataRow("build_property.IsTestProject = \nbuild_property.IsTestingPlatformApplication = ")]
    [DataRow("build_property.IsTestProject = yes")]
    [DataRow("build_property.IsTestingPlatformApplication = yes")]
    public async Task TypeOutsideTestProjectIsJudgedAgainstProductionLimit(string globalConfiguration)
    {
        await VerifyLines(ThreeLineType, ProductionTwoTestThree, globalConfiguration, LineLimit(0, 3, 2));
    }

    /// <summary>
    /// Verifies that a type is production code when the project passes no global analyzer options at all
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-LINES-003")]
    public async Task TypeWithoutGlobalOptionsIsProductionCode()
    {
        await VerifyLines(ThreeLineType, ProductionTwoTestThree, LineLimit(0, 3, 2));
    }

    #endregion // Tests — test code (SHARED-LINES-003)

    #region Tests — judged types and counted span (RH2111-002)

    /// <summary>
    /// Verifies the example of RH2111-002: regions, documentation comments, and blank lines are not counted; the body
    /// of <c>C</c> holds six lines and is reported, the body of <c>S</c> four and is not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2111-002")]
    [Requirement("RH2111-006")]
    [Requirement("SHARED-LINES-001")]
    public async Task TypeIsJudgedByTheLinesBetweenItsBraces()
    {
        await VerifyLines(RegionExample, """{ "design": { "maximumTypeLines": 4 } }""", LineLimit(0, 6, 4));
    }

    /// <summary>
    /// Verifies that the example of RH2111-002 gives the same counts for every line ending
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
        var source = RegionExample.Replace("\r\n", "\n").Replace("\n", endOfLine);

        await VerifyLines(source, """{ "design": { "maximumTypeLines": 4 } }""", LineLimit(0, 6, 4));
    }

    /// <summary>
    /// Verifies that the header of a type — attributes, modifiers, type parameters, base list, and constraint clauses
    /// over several lines — is not counted, and the diagnostic stands on the identifier without its type parameters
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2111-002")]
    [Requirement("RH2111-006")]
    public async Task HeaderIsNotCounted()
    {
        const string source = """
                              [System.Serializable]
                              [System.Obsolete]
                              public sealed class {|#0:C|}<T>
                                  : System.Collections.Generic.List<T>,
                                    System.IDisposable
                                  where T : class
                              {
                                  public void Dispose()
                                  {
                                  }
                              }
                              """;

        await VerifyLines(source, LimitOne, LineLimit(0, 3, 1));
    }

    /// <summary>
    /// Verifies that the parameter list of a positional record with a body is not counted
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2111-002")]
    public async Task ParameterListOfPositionalRecordIsNotCounted()
    {
        const string source = """
                              record {|#0:R|}(
                                  int X,
                                  int Y)
                              {
                                  public int A => X;
                                  public int B => Y;
                              }
                              """;

        await VerifyLines(source, LimitOne, LineLimit(0, 2, 1));
    }

    /// <summary>
    /// Verifies that a struct, an interface, a record, a record class, and a record struct are judged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2111-002")]
    public async Task EveryTypeKindWithBraceBodyIsJudged()
    {
        const string source = """
                              struct {|#0:S|}
                              {
                                  public int A;
                                  public int B;
                              }

                              interface {|#1:I|}
                              {
                                  void A();
                                  void B();
                              }

                              record {|#2:R|}
                              {
                                  public int A { get; init; }
                                  public int B { get; init; }
                              }

                              record class {|#3:Q|}
                              {
                                  public int A { get; init; }
                                  public int B { get; init; }
                              }

                              record struct {|#4:P|}
                              {
                                  public int A { get; init; }
                                  public int B { get; init; }
                              }
                              """;

        await VerifyLines(source, LimitOne, LineLimit(0, 2, 1), LineLimit(1, 2, 1), LineLimit(2, 2, 1), LineLimit(3, 2, 1), LineLimit(4, 2, 1));
    }

    /// <summary>
    /// Verifies the union and the enum of the change doc's example: a union with a brace body is judged, an enum is not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2111-002")]
    public async Task UnionIsJudgedAndEnumIsNot()
    {
        const string source = """
                              union {|#0:U|}(int, string)
                              {
                                  public int A => 1;
                                  public int B => 2;
                                  public int C => 3;
                              }

                              enum E
                              {
                                  A,
                                  B,
                                  C,
                              }
                              """;

        var unionSupport = await UnionSupport.GetReferenceAsync();

        await Verify(source,
                     test =>
                     {
                         test.TestState.AdditionalReferences.Add(unionSupport);
                         test.TestState.AdditionalFiles.Add(("reihitsu.json", LimitTwo));
                     },
                     LineLimit(0, 3, 2));
    }

    /// <summary>
    /// Verifies that a <c>file</c> type and a static class are judged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2111-002")]
    public async Task FileTypeAndStaticClassAreJudged()
    {
        const string source = """
                              file class {|#0:F|}
                              {
                                  public int A;
                                  public int B;
                              }

                              static class {|#1:S|}
                              {
                                  public static int A;
                                  public static int B;
                              }
                              """;

        await VerifyLines(source, LimitOne, LineLimit(0, 2, 1), LineLimit(1, 2, 1));
    }

    /// <summary>
    /// Verifies that a member inside a conditional group that the analyzed build excludes is not counted, and is
    /// counted once its symbol is defined; <c>#pragma</c> and delimited documentation comment lines are not counted
    /// </summary>
    /// <param name="isFeatureDefined">Whether the symbol <c>FEATURE</c> is defined</param>
    /// <param name="count">Expected lines of code</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-LINES-001")]
    [DataRow(false, 2)]
    [DataRow(true, 3)]
    public async Task ExcludedMemberIsNotCounted(bool isFeatureDefined, int count)
    {
        const string source = """
                              class {|#0:C|}
                              {
                              #if FEATURE
                                  public int A;
                              #endif
                              #pragma warning disable CS0169
                                  private int _b;
                              #pragma warning restore CS0169
                                  /** <summary>Doc</summary> */
                                  public int D;
                              }
                              """;

        await Verify(source,
                     test =>
                     {
                         test.TestState.AdditionalFiles.Add(("reihitsu.json", LimitOne));

                         if (isFeatureDefined)
                         {
                             test.SolutionTransforms.Add((solution, projectId) => solution.GetProject(projectId)?.ParseOptions is CSharpParseOptions parseOptions
                                                                                      ? solution.WithProjectParseOptions(projectId, parseOptions.WithPreprocessorSymbols("FEATURE"))
                                                                                      : solution);
                         }
                     },
                     LineLimit(0, count, 1));
    }

    /// <summary>
    /// Verifies that every line of a multi-line raw string literal in a field initializer counts toward the type
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-LINES-001")]
    public async Task EveryLineOfRawStringInFieldCountsTowardTheType()
    {
        const string source = """"
                              class {|#0:C|}
                              {
                                  private const string Text = """
                                      first

                                      second
                                      """;
                              }
                              """";

        await VerifyLines(source, LimitOne, LineLimit(0, 5, 1));
    }

    /// <summary>
    /// Verifies that an attribute list on a line of its own counts, since its tokens lie in the body
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2111-002")]
    [Requirement("SHARED-LINES-001")]
    public async Task MemberAttributeLineIsCounted()
    {
        const string source = """
                              class {|#0:C|}
                              {
                                  [System.Obsolete]
                                  public void M()
                                  {
                                  }
                              }
                              """;

        await VerifyLines(source, LimitOne, LineLimit(0, 4, 1));
    }

    /// <summary>
    /// Verifies that a lambda in a field initializer counts toward the type, as RH2008-003 states
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2111-002")]
    public async Task LambdaInFieldInitializerCountsTowardTheType()
    {
        const string source = """
                              using System;

                              class {|#0:C|}
                              {
                                  private static readonly Func<int, int> Compute = value =>
                                  {
                                      var result = value;
                                      result += 1;
                                      return result;
                                  };
                              }
                              """;

        await VerifyLines(source, LimitTwo, LineLimit(0, 6, 2));
    }

    /// <summary>
    /// Verifies that an extension block counts toward the type that holds it and is not judged itself
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2111-002")]
    public async Task ExtensionBlockCountsTowardItsTypeAndIsNotJudged()
    {
        const string source = """
                              static class {|#0:TextExtensions|}
                              {
                                  extension(string text)
                                  {
                                      public int Twice()
                                          => text.Length * 2;
                                  }
                              }
                              """;

        await VerifyLines(source, LimitOne, LineLimit(0, 5, 1));
    }

    /// <summary>
    /// Verifies that a body written on one line holds one line of code, and an empty body none
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2111-002")]
    [Requirement("SHARED-LINES-001")]
    public async Task OneLineBodyAndEmptyBodyAreAtOrBelowLimitOfOne()
    {
        const string source = """
                              class C { public int A; public int B; }

                              class D
                              {
                              }

                              class E { }
                              """;

        await VerifyLines(source, LimitOne);
    }

    /// <summary>
    /// Verifies that a type in a generated file is not reported, while a part of the same type in an ordinary file is
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2111-004")]
    public async Task PartInGeneratedFileIsNotReported()
    {
        const string source = """
                              partial class {|#0:C|}
                              {
                                  public int A;
                                  public int B;
                              }
                              """;

        const string generated = """
                                 partial class C
                                 {
                                     public int D;
                                     public int E;
                                 }
                                 """;

        await Verify(source,
                     test =>
                     {
                         test.TestState.Sources.Add(("Generated.g.cs", generated));
                         test.TestState.AdditionalFiles.Add(("reihitsu.json", LimitOne));
                     },
                     LineLimit(0, 2, 1));
    }

    #endregion // Tests — judged types and counted span (RH2111-002)

    #region Tests — nested and partial types (RH2111-003, RH2111-004)

    /// <summary>
    /// Verifies the example of RH2111-003: the nested type counts toward the outer type and is judged on its own
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2111-003")]
    public async Task NestedTypeCountsTowardTheOuterType()
    {
        const string source = """
                              class {|#0:Outer|}
                              {
                                  class Inner
                                  {
                                      public int A;
                                      public int B;
                                  }
                              }
                              """;

        await VerifyLines(source, """{ "design": { "maximumTypeLines": 3 } }""", LineLimit(0, 5, 3));
    }

    /// <summary>
    /// Verifies that a type nested two levels deep counts toward both containers and each level is judged on its own
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2111-003")]
    public async Task EveryLevelOfNestingIsJudged()
    {
        const string source = """
                              class {|#0:Outer|}
                              {
                                  class {|#1:Middle|}
                                  {
                                      struct {|#2:Inner|}
                                      {
                                          public int A;
                                          public int B;
                                      }
                                  }
                              }
                              """;

        await VerifyLines(source, LimitOne, LineLimit(0, 8, 1), LineLimit(1, 5, 1), LineLimit(2, 2, 1));
    }

    /// <summary>
    /// Verifies the example of RH2111-004: each part of a partial type is judged by its own body
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2111-004")]
    public async Task EachPartIsJudgedOnItsOwn()
    {
        const string source = """
                              partial class {|#0:C|}
                              {
                                  public int A;
                                  public int B;
                                  public int C1;
                              }

                              partial class C
                              {
                                  public int D;
                                  public int E;
                              }
                              """;

        await VerifyLines(source, LimitTwo, LineLimit(0, 3, 2));
    }

    /// <summary>
    /// Verifies that two parts in two files that are each over the limit are both reported, each on its own part, and
    /// that a nested type inside a part counts toward that part only
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2111-004")]
    [Requirement("RH2111-003")]
    public async Task EveryPartOverTheLimitIsReportedOnThatPart()
    {
        const string source = """
                              partial class {|#0:C|}
                              {
                                  public int A;
                                  public int B;
                                  public int D;
                              }
                              """;

        const string secondPart = """
                                  partial class {|#1:C|}
                                  {
                                      class Inner
                                      {
                                      }
                                  }
                                  """;

        await Verify(source,
                     test =>
                     {
                         test.TestState.Sources.Add(("Second.cs", secondPart));
                         test.TestState.AdditionalFiles.Add(("reihitsu.json", LimitTwo));
                     },
                     LineLimit(0, 3, 2),
                     LineLimit(1, 3, 2));
    }

    #endregion // Tests — nested and partial types (RH2111-003, RH2111-004)

    #region Tests — types without a body and location (RH2111-005, RH2111-006)

    /// <summary>
    /// Verifies the example of RH2111-005 and the other type declarations that end with <c>;</c>: none is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2111-005")]
    public async Task TypeWithoutBraceBodyIsNotReported()
    {
        const string source = """
                              record R(int X, int Y, int Z);

                              record struct P(
                                  int X,
                                  int Y);

                              class C;

                              struct S;

                              delegate void D(
                                  int first,
                                  int second);
                              """;

        await VerifyLines(source, LimitOne);
    }

    /// <summary>
    /// Verifies that a union without a brace body is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2111-005")]
    public async Task UnionWithoutBraceBodyIsNotReported()
    {
        const string source = """
                              union U(
                                  int,
                                  string);
                              """;

        var unionSupport = await UnionSupport.GetReferenceAsync();

        await Verify(source,
                     test =>
                     {
                         test.TestState.AdditionalReferences.Add(unionSupport);
                         test.TestState.AdditionalFiles.Add(("reihitsu.json", LimitOne));
                     });
    }

    /// <summary>
    /// Verifies that a verbatim identifier locates the diagnostic on the whole identifier token
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2111-006")]
    public async Task VerbatimIdentifierIsReportedOnTheWholeToken()
    {
        const string source = """
                              class {|#0:@class|}
                              {
                                  public int A;
                                  public int B;
                              }
                              """;

        await VerifyLines(source, LimitOne, LineLimit(0, 2, 1));
    }

    #endregion // Tests — types without a body and location (RH2111-005, RH2111-006)

    #region Properties

    /// <summary>
    /// Example of RH2111-002: the body of <c>C</c> holds six lines of code and the body of <c>S</c> four
    /// </summary>
    private static string RegionExample => """
                                           class {|#0:C|}
                                           {
                                               #region Fields

                                               /// <summary>
                                               /// Value
                                               /// </summary>
                                               private int _value;

                                               #endregion // Fields

                                               #region Methods

                                               public int Get()
                                               {
                                                   return _value;
                                               }

                                               public void Set(int value) => _value = value;

                                               #endregion // Methods
                                           }

                                           struct S
                                           {
                                               public int X;
                                               public int Y;
                                               public int Z;
                                               public int W;
                                           }
                                           """;

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Creates the expected diagnostic at a markup location with the message that states the count and the limit
    /// </summary>
    /// <param name="markupKey">Markup key of the location</param>
    /// <param name="count">Lines of code of the type's body</param>
    /// <param name="limit">Limit that applies</param>
    /// <returns>Expected diagnostic</returns>
    private static DiagnosticResult LineLimit(int markupKey, int count, int limit)
    {
        return Diagnostic(RH2111TypesShouldNotExceedTheLineLimitAnalyzer.DiagnosticId).WithLocation(markupKey, DiagnosticLocationOptions.InterpretAsMarkupKey)
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
                             test.TestState.AnalyzerConfigFiles.Add(("/.globalconfig", globalConfiguration.Length == 0 ? "is_global = true\n" : $"is_global = true\n{globalConfiguration}\n"));
                         }
                     },
                     expected);
    }

    /// <summary>
    /// Creates a type of exactly <paramref name="limit"/> lines of code, not marked, and one of <paramref name="limit"/>
    /// + 1 lines, marked with the markup key 0
    /// </summary>
    /// <param name="limit">Limit the two types lie at and above</param>
    /// <returns>Source with markup</returns>
    private static string CreateTypesAround(int limit)
    {
        return $"{CreateType($"T{limit}", limit, false)}\n{CreateType($"T{limit + 1}", limit + 1, true)}";
    }

    /// <summary>
    /// Creates a class whose body holds the given number of lines of code, one field per line
    /// </summary>
    /// <param name="name">Name of the class</param>
    /// <param name="lines">Lines of code of the body</param>
    /// <param name="isMarked">Whether the name is marked with the markup key 0</param>
    /// <returns>Source of the class</returns>
    private static string CreateType(string name, int lines, bool isMarked)
    {
        var builder = new StringBuilder();

        builder.Append("class ")
               .Append(isMarked ? $"{{|#0:{name}|}}" : name)
               .Append("\n{\n");

        foreach (var index in Enumerable.Range(0, lines))
        {
            builder.Append("    public int F").Append(index).Append(";\n");
        }

        builder.Append("}\n");

        return builder.ToString();
    }

    #endregion // Methods
}