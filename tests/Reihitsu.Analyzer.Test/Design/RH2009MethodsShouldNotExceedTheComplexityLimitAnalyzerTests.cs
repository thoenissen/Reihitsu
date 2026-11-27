using System;
using System.Text;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Design;
using Reihitsu.Analyzer.Test.Base;
using Reihitsu.Analyzer.Test.Verifiers;

namespace Reihitsu.Analyzer.Test.Design;

/// <summary>
/// Test methods for <see cref="RH2009MethodsShouldNotExceedTheComplexityLimitAnalyzer"/>
/// </summary>
[TestClass]
public class RH2009MethodsShouldNotExceedTheComplexityLimitAnalyzerTests : AnalyzerTestsBase<RH2009MethodsShouldNotExceedTheComplexityLimitAnalyzer>
{
    #region Constants

    /// <summary>
    /// Example of RH2009-001: <c>Score</c> has a cognitive complexity of 16 — <c>foreach</c> 1, the outer <c>if</c> 2,
    /// the inner <c>if</c> 3, <c>for</c> 4, the innermost <c>if</c> 5, <c>else if</c> 1
    /// </summary>
    private const string ScoreExample = """
                                        class C
                                        {
                                            public int {|#0:Score|}(int[] values)
                                            {
                                                int total = 0;

                                                foreach (var value in values)
                                                {
                                                    if (value > 0)
                                                    {
                                                        if (value % 2 == 0)
                                                        {
                                                            for (int i = 0; i < value; i++)
                                                            {
                                                                if (i % 3 == 0)
                                                                {
                                                                    total += i;
                                                                }
                                                                else if (i % 5 == 0)
                                                                {
                                                                    total -= i;
                                                                }
                                                            }
                                                        }
                                                    }
                                                }

                                                return total;
                                            }
                                        }
                                        """;

    /// <summary>
    /// A method with a cognitive complexity of exactly 15 that nests: <c>foreach</c> 1 and four nested <c>if</c>
    /// statements 2, 3, 4, and 5
    /// </summary>
    private const string FifteenExample = """
                                          class C
                                          {
                                              public int Score(int[] values)
                                              {
                                                  int total = 0;

                                                  foreach (var value in values)
                                                  {
                                                      if (value > 0)
                                                      {
                                                          if (value > 1)
                                                          {
                                                              if (value > 2)
                                                              {
                                                                  if (value > 3)
                                                                  {
                                                                      total++;
                                                                  }
                                                              }
                                                          }
                                                      }
                                                  }

                                                  return total;
                                              }
                                          }
                                          """;

    /// <summary>
    /// Template whose method <c>M</c> holds one <c>if</c> (1) with a <c>while</c> inside it (2), and the placeholder
    /// <c>EXPRESSION</c> as the condition of the <c>while</c>
    /// </summary>
    private const string LogicalOperatorTemplate = """
                                                   class C
                                                   {
                                                       bool _a, _b, _c, _d, _e, _f;
                                                       bool? _n;
                                                       int _x;
                                                       Flags _flags;

                                                       void {|#0:M|}()
                                                       {
                                                           if (_a)
                                                           {
                                                               while (EXPRESSION)
                                                               {
                                                               }
                                                           }
                                                       }

                                                       bool F(bool value)
                                                       {
                                                           return value;
                                                       }
                                                   }

                                                   enum Flags
                                                   {
                                                       None = 0,
                                                       A = 1,
                                                       B = 2,
                                                   }
                                                   """;

    /// <summary>
    /// Template whose method <c>M</c> holds the placeholder <c>STATEMENT</c> inside the block of an <c>if</c> (1)
    /// </summary>
    private const string NestedStatementTemplate = """
                                                   class C
                                                   {
                                                       private readonly object _gate = new object();

                                                       unsafe void {|#0:M|}(bool a, bool b, int x, int[] xs, int[] ys)
                                                       {
                                                           if (a)
                                                           {
                                                               STATEMENT
                                                           }
                                                       }
                                                   }
                                                   """;

    /// <summary>
    /// Template whose method <c>M</c> holds the placeholder <c>STATEMENT</c> at the top of its body
    /// </summary>
    private const string FlatStatementTemplate = """
                                                 class C
                                                 {
                                                     void M(bool a, bool b, int x, int[] xs, int[] ys)
                                                     {
                                                         STATEMENT
                                                     }
                                                 }
                                                 """;

    /// <summary>
    /// Template whose method <c>M</c> holds two nested <c>if</c> statements (1 and 2) and the placeholder
    /// <c>CALL</c> inside the inner one
    /// </summary>
    private const string RecursionTemplate = """
                                             class C
                                             {
                                                 int {|#0:M|}(int n)
                                                 {
                                                     if (n > 0)
                                                     {
                                                         if (n > 1)
                                                         {
                                                             CALL
                                                         }
                                                     }

                                                     return n;
                                                 }

                                                 int M(string text)
                                                 {
                                                     return text.Length;
                                                 }

                                                 static int S(int n)
                                                 {
                                                     return n;
                                                 }

                                                 int G<T>(int n)
                                                 {
                                                     return n;
                                                 }
                                             }
                                             """;

    #endregion // Constants

    #region Tests — limit (RH2009-001, SHARED-LIMIT)

    /// <summary>
    /// Verifies the example of RH2009-001: without a configuration file, <c>Score</c> has a cognitive complexity of 16
    /// and is reported against the default of 15
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2009-001")]
    [Requirement("RH2009-006")]
    [Requirement("SHARED-LIMIT-001")]
    [Requirement("SHARED-LIMIT-002")]
    public async Task ScoreExampleIsReportedAgainstTheDefaultLimit()
    {
        await VerifyComplexity(ScoreExample, null, Complexity(0, 16, 15));
    }

    /// <summary>
    /// Verifies that without a configuration file a nested body with a cognitive complexity of exactly 15 is not
    /// reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2009-001")]
    [Requirement("SHARED-LIMIT-002")]
    public async Task NestedBodyAtTheDefaultLimitIsNotReported()
    {
        await VerifyComplexity(FifteenExample, null);
    }

    /// <summary>
    /// Verifies the example of RH2009-001 with a configured limit equal to the complexity: <c>Score</c> is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2009-001")]
    [Requirement("SHARED-LIMIT-002")]
    public async Task ConfiguredLimitEqualToTheComplexityIsNotReported()
    {
        await VerifyComplexity(Unmarked(ScoreExample), Limit(16));
    }

    /// <summary>
    /// Verifies the example of RH2009-001 with a configured limit of 10: <c>Score</c> is reported against 10
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2009-001")]
    [Requirement("SHARED-LIMIT-001")]
    [Requirement("SHARED-LIMIT-002")]
    public async Task ConfiguredLimitBelowTheComplexityIsReported()
    {
        await VerifyComplexity(ScoreExample, Limit(10), Complexity(0, 16, 10));
    }

    /// <summary>
    /// Verifies the example of RH2009-001 in a test project: the configured limit applies to test code as to
    /// production code
    /// </summary>
    /// <param name="globalConfiguration">Lines of the global analyzer config that make the project a test project</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2009-001")]
    [DataRow("build_property.IsTestProject = true")]
    [DataRow("build_property.IsTestingPlatformApplication = true")]
    public async Task ConfiguredLimitAppliesToTestCode(string globalConfiguration)
    {
        await VerifyComplexity(ScoreExample, Limit(10), test => AddGlobalConfiguration(test, globalConfiguration), Complexity(0, 16, 10));
    }

    /// <summary>
    /// Verifies that without a configuration file the default of 15 applies to test code as to production code
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2009-001")]
    public async Task DefaultLimitAppliesToTestCode()
    {
        await VerifyComplexity(ScoreExample, null, test => AddGlobalConfiguration(test, "build_property.IsTestProject = true"), Complexity(0, 16, 15));
    }

    /// <summary>
    /// Verifies that there is no separate limit for test code: <c>maximumTestMethodComplexity</c>, which RH0001
    /// reports as unknown, does not change the limit of a test project
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2009-001")]
    public async Task UnknownTestSettingDoesNotChangeTheLimitOfTestCode()
    {
        await VerifyComplexity(ScoreExample,
                               """{ "design": { "maximumTestMethodComplexity": 30 } }""",
                               test => AddGlobalConfiguration(test, "build_property.IsTestProject = true"),
                               Complexity(0, 16, 15));
    }

    /// <summary>
    /// Verifies the example of RH2009-001 that a setting without a valid value keeps the default of 15, including the
    /// configurations SHARED-LIMIT-001 names
    /// </summary>
    /// <param name="configuration">Text of <c>reihitsu.json</c></param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2009-001")]
    [Requirement("SHARED-LIMIT-001")]
    [DataRow("""{ "design": { "maximumMethodComplexity": 0 } }""")]
    [DataRow("""{ "design": { "maximumMethodComplexity": "10" } }""")]
    [DataRow("""{ "design": { "maximumMethodComplexity": -1 } }""")]
    [DataRow("""{ "design": { "maximumMethodComplexity": 2.5 } }""")]
    [DataRow("""{ "design": { "maximumMethodComplexity": 10.0 } }""")]
    [DataRow("""{ "design": { "maximumMethodComplexity": 1e1 } }""")]
    [DataRow("""{ "design": { "maximumMethodComplexity": null } }""")]
    [DataRow("""{ "design": { "maximumMethodComplexity": true } }""")]
    [DataRow("""{ "design": { "maximumMethodComplexity": 2147483648 } }""")]
    [DataRow("""{ "design": { } }""")]
    [DataRow("""{ "design": [ ] }""")]
    [DataRow("""{ }""")]
    [DataRow("")]
    [DataRow("   ")]
    [DataRow("{ } junk")]
    [DataRow("""{ "design": { "maximumMethodComplexity": 10 """)]
    public async Task SettingWithoutValidValueKeepsTheDefaultLimit(string configuration)
    {
        await VerifyComplexity(ScoreExample, configuration, Complexity(0, 16, 15));
    }

    /// <summary>
    /// Verifies the example of RH2009-001 that a problem RH0001 reports elsewhere in the configuration does not keep a
    /// valid limit from taking effect
    /// </summary>
    /// <param name="configuration">Configuration with a valid limit of 10 and a problem elsewhere</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2009-001")]
    [Requirement("SHARED-LIMIT-001")]
    [DataRow("""{ "design": { "maximumMethodComplexity": 10 }, "formatting": 1 }""")]
    [DataRow("""{ "design": { "maximumMethodComplexity": 10, "maximumMethodLines": "60" } }""")]
    [DataRow("""{ "design": { "maximumMethodComplexity": 10, "maximumTestMethodComplexity": 30 } }""")]
    [DataRow("""{ "copyright": "x", "design": { "maximumMethodComplexity": 10 } }""")]
    public async Task ProblemElsewhereDoesNotKeepValidLimitFromTakingEffect(string configuration)
    {
        await VerifyComplexity(ScoreExample, configuration, Complexity(0, 16, 10));
    }

    /// <summary>
    /// Verifies the example of RH2009-001 that the line limits do not change the complexity limit
    /// </summary>
    /// <param name="configuration">Configuration that sets only line limits</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2009-001")]
    [DataRow("""{ "design": { "maximumMethodLines": 10 } }""")]
    [DataRow("""{ "design": { "maximumTestMethodLines": 10, "maximumTypeLines": 10 } }""")]
    public async Task LineLimitsDoNotChangeTheComplexityLimit(string configuration)
    {
        await VerifyComplexity(ScoreExample, configuration, Complexity(0, 16, 15));
    }

    /// <summary>
    /// Verifies the largest valid limit: a nested body far below it is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-LIMIT-001")]
    public async Task LargestValidLimitTakesEffect()
    {
        await VerifyComplexity(Unmarked(ScoreExample), Limit(2147483647));
    }

    /// <summary>
    /// Verifies that a body whose 30 <c>if</c> statements stand one after another is not reported without a
    /// configuration file, although its cognitive complexity of 30 exceeds the default
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2009-001")]
    [Requirement("RH2009-004")]
    public async Task FlatBodyFarAboveTheDefaultLimitIsNotReported()
    {
        await VerifyComplexity(CreateIfSequence(30, false), null);
    }

    /// <summary>
    /// Verifies that the same body with one <c>if</c> nested in its last <c>if</c> is reported by its whole
    /// complexity: 29 <c>if</c> statements 1 each, the last <c>if</c> 1, and the nested <c>if</c> 2
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2009-001")]
    [Requirement("RH2009-004")]
    public async Task SameBodyWithOneNestedIfIsReportedByItsWholeComplexity()
    {
        await VerifyComplexity(CreateIfSequence(30, true), null, Complexity(0, 32, 15));
    }

    #endregion // Tests — limit (RH2009-001, SHARED-LIMIT)

    #region Tests — judged members (RH2009-002)

    /// <summary>
    /// Verifies the first example of RH2009-002: a <c>get</c> accessor, an expression-bodied property, and an operator
    /// are judged by their body, and <c>Flat</c>, with 2, is not reported at a limit of 2
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2009-002")]
    [Requirement("RH2009-006")]
    public async Task AccessorPropertyAndOperatorAreJudgedByTheirBody()
    {
        const string source = """
                              class C
                              {
                                  private int _value;

                                  public int Value
                                  {
                                      {|#0:get|}
                                      {
                                          if (_value > 0)
                                          {
                                              if (_value > 1)
                                              {
                                                  return 2;
                                              }
                                          }

                                          return 0;
                                      }
                                  }

                                  public int {|#1:Sign|} => _value > 0 ? (_value > 1 ? 2 : 1) : 0;

                                  public static C operator {|#2:+|}(C left, C right)
                                  {
                                      foreach (var digit in right._value.ToString())
                                      {
                                          if (digit == '1')
                                          {
                                              left._value++;
                                          }
                                      }

                                      return left;
                                  }

                                  public int Flat()
                                  {
                                      while (_value > 10)
                                      {
                                          _value--;
                                      }

                                      return _value > 5 ? 1 : 0;
                                  }
                              }
                              """;

        await VerifyComplexity(source, Limit(2), Complexity(0, 3, 2), Complexity(1, 3, 2), Complexity(2, 3, 2));
    }

    /// <summary>
    /// Verifies the second example of RH2009-002: members without a body are not judged, and a partial method is
    /// judged once, by its implementing part
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2009-002")]
    public async Task PartialMethodIsJudgedByItsImplementingPart()
    {
        const string source = """
                              abstract partial class C
                              {
                                  public abstract void A(int value);

                                  public int P { get; set; }

                                  public partial void M(int value);

                                  public partial void {|#0:M|}(int value)
                                  {
                                      if (value > 0)
                                      {
                                          if (value > 1)
                                          {
                                              A(value);
                                          }
                                      }
                                  }
                              }
                              """;

        await VerifyComplexity(source, Limit(1), Complexity(0, 3, 1));
    }

    /// <summary>
    /// Verifies that a partial property is judged once, by the accessor of its implementing part
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2009-002")]
    public async Task PartialPropertyIsJudgedByItsImplementingPart()
    {
        const string source = """
                              partial class C
                              {
                                  private int _value;

                                  public partial int P { get; }

                                  public partial int P
                                  {
                                      {|#0:get|}
                                      {
                                          if (_value > 0)
                                          {
                                              if (_value > 1)
                                              {
                                                  return 2;
                                              }
                                          }

                                          return 0;
                                      }
                                  }
                              }
                              """;

        await VerifyComplexity(source, Limit(1), Complexity(0, 3, 1));
    }

    /// <summary>
    /// Verifies that every kind of expression-bodied member — method, property, indexer, constructor, finalizer,
    /// operator, and local function — is judged by the expression of its expression body, here a conditional
    /// expression nested in the true branch of another, 1 and 2
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2009-002")]
    [Requirement("RH2009-006")]
    public async Task ExpressionBodiedMembersAreJudgedByTheirExpression()
    {
        const string source = """
                              class C
                              {
                                  private int _value;

                                  public {|#0:C|}(int a) => _value = a > 0 ? (a > 1 ? 2 : 1) : 0;

                                  ~{|#1:C|}() => _value = _value > 0 ? (_value > 1 ? 2 : 1) : 0;

                                  public int {|#2:M|}(int a) => a > 0 ? (a > 1 ? 2 : 1) : 0;

                                  public int {|#3:P|} => _value > 0 ? (_value > 1 ? 2 : 1) : 0;

                                  public int {|#4:this|}[int a] => a > 0 ? (a > 1 ? 2 : 1) : 0;

                                  public static int operator {|#5:-|}(C c) => c._value > 0 ? (c._value > 1 ? 2 : 1) : 0;

                                  public static implicit {|#6:operator|} int(C c) => c._value > 0 ? (c._value > 1 ? 2 : 1) : 0;

                                  public int N()
                                  {
                                      return L(_value);

                                      int {|#7:L|}(int a) => a > 0 ? (a > 1 ? 2 : 1) : 0;
                                  }
                              }
                              """;

        await VerifyComplexity(source,
                               Limit(1),
                               Complexity(0, 3, 1),
                               Complexity(1, 3, 1),
                               Complexity(2, 3, 1),
                               Complexity(3, 3, 1),
                               Complexity(4, 3, 1),
                               Complexity(5, 3, 1),
                               Complexity(6, 3, 1),
                               Complexity(7, 3, 1));
    }

    /// <summary>
    /// Verifies that the <c>get</c>, <c>set</c>, <c>init</c>, <c>add</c>, and <c>remove</c> accessors are judged, each
    /// by its block body or its expression body, and reported on its keyword
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2009-002")]
    [Requirement("RH2009-006")]
    public async Task EveryAccessorKindIsJudged()
    {
        const string source = """
                              class C
                              {
                                  private int _value;

                                  public int this[int a]
                                  {
                                      {|#0:get|} => a > 0 ? (a > 1 ? 2 : 1) : 0;
                                      {|#1:set|} => _value = value > 0 ? (value > 1 ? 2 : 1) : 0;
                                  }

                                  public int Q
                                  {
                                      get => _value;
                                      {|#2:init|} => _value = value > 0 ? (value > 1 ? 2 : 1) : 0;
                                  }

                                  public event System.Action E
                                  {
                                      {|#3:add|}
                                      {
                                          if (value != null)
                                          {
                                              if (_value > 0)
                                              {
                                                  _value--;
                                              }
                                          }
                                      }

                                      {|#4:remove|} => _value = _value > 0 ? (_value > 1 ? 2 : 1) : 0;
                                  }
                              }
                              """;

        await VerifyComplexity(source,
                               Limit(1),
                               Complexity(0, 3, 1),
                               Complexity(1, 3, 1),
                               Complexity(2, 3, 1),
                               Complexity(3, 3, 1),
                               Complexity(4, 3, 1));
    }

    /// <summary>
    /// Verifies that the accessors of one property are judged on their own and their complexities are not summed: at a
    /// limit of 3, neither accessor with 3 is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2009-002")]
    public async Task AccessorsAtTheLimitAreNotSummed()
    {
        await VerifyComplexity(CreatePropertyWithTwoAccessors(false), Limit(3));
    }

    /// <summary>
    /// Verifies that both accessors of one property are reported with their own complexity of 3 when it exceeds the
    /// limit, not with their sum
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2009-002")]
    [Requirement("RH2009-006")]
    public async Task AccessorsAboveTheLimitAreReportedEachWithItsOwnComplexity()
    {
        await VerifyComplexity(CreatePropertyWithTwoAccessors(true), Limit(2), Complexity(0, 3, 2), Complexity(1, 3, 2));
    }

    /// <summary>
    /// Verifies that a static constructor, an async method, and a conversion operator with a block body are judged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2009-002")]
    [Requirement("RH2009-006")]
    public async Task StaticConstructorAsyncMethodAndConversionOperatorAreJudged()
    {
        const string source = """
                              using System.Threading.Tasks;

                              class C
                              {
                                  private static int _shared;

                                  private int _value;

                                  static {|#0:C|}()
                                  {
                                      if (_shared > 0)
                                      {
                                          if (_shared > 1)
                                          {
                                              _shared--;
                                          }
                                      }
                                  }

                                  public async Task<int> {|#1:RunAsync|}(int a)
                                  {
                                      if (a > 0)
                                      {
                                          if (a > 1)
                                          {
                                              await Task.Yield();
                                          }
                                      }

                                      return a;
                                  }

                                  public static explicit {|#2:operator|} int(C c)
                                  {
                                      if (c._value > 0)
                                      {
                                          if (c._value > 1)
                                          {
                                              return 2;
                                          }
                                      }

                                      return 0;
                                  }
                              }
                              """;

        await VerifyComplexity(source, Limit(1), Complexity(0, 3, 1), Complexity(1, 3, 1), Complexity(2, 3, 1));
    }

    /// <summary>
    /// Verifies that a compound-assignment operator is judged and reported on its operator token
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2009-002")]
    [Requirement("RH2009-006")]
    public async Task CompoundAssignmentOperatorIsJudged()
    {
        const string source = """
                              class C
                              {
                                  private int _value;

                                  public void operator {|#0:+=|}(int a)
                                  {
                                      if (a > 0)
                                      {
                                          if (a > 1)
                                          {
                                              _value += a;
                                          }
                                      }
                                  }
                              }
                              """;

        await VerifyComplexity(source, Limit(1), Complexity(0, 3, 1));
    }

    /// <summary>
    /// Verifies that a member of an extension block is judged like any other member
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2009-002")]
    public async Task MemberOfExtensionBlockIsJudged()
    {
        const string source = """
                              static class TextExtensions
                              {
                                  extension(string text)
                                  {
                                      public int {|#0:Twice|}()
                                      {
                                          if (text.Length > 0)
                                          {
                                              if (text.Length > 1)
                                              {
                                                  return 2;
                                              }
                                          }

                                          return 0;
                                      }
                                  }
                              }
                              """;

        await VerifyComplexity(source, Limit(1), Complexity(0, 3, 1));
    }

    /// <summary>
    /// Verifies that an explicit interface implementation of a method and of an accessor is judged, on the method's
    /// identifier and on the accessor keyword
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2009-002")]
    [Requirement("RH2009-006")]
    public async Task ExplicitInterfaceImplementationIsJudged()
    {
        const string source = """
                              interface I
                              {
                                  void M(int a);

                                  int P { get; }
                              }

                              class C : I
                              {
                                  private int _value;

                                  void I.{|#0:M|}(int a)
                                  {
                                      if (a > 0)
                                      {
                                          if (a > 1)
                                          {
                                              _value = a;
                                          }
                                      }
                                  }

                                  int I.P
                                  {
                                      {|#1:get|}
                                      {
                                          if (_value > 0)
                                          {
                                              if (_value > 1)
                                              {
                                                  return 2;
                                              }
                                          }

                                          return 0;
                                      }
                                  }
                              }
                              """;

        await VerifyComplexity(source, Limit(1), Complexity(0, 3, 1), Complexity(1, 3, 1));
    }

    /// <summary>
    /// Verifies that an interface member with a default implementation is judged, and one without a body is not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2009-002")]
    public async Task InterfaceMemberWithDefaultImplementationIsJudged()
    {
        const string source = """
                              interface I
                              {
                                  void N();

                                  void {|#0:M|}(int a)
                                  {
                                      if (a > 0)
                                      {
                                          if (a > 1)
                                          {
                                              N();
                                          }
                                      }
                                  }
                              }
                              """;

        await VerifyComplexity(source, Limit(1), Complexity(0, 3, 1));
    }

    /// <summary>
    /// Verifies that the members of a struct and of a record are judged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2009-002")]
    public async Task MembersOfStructAndRecordAreJudged()
    {
        const string source = """
                              struct S
                              {
                                  int {|#0:M|}(int a) => a > 0 ? (a > 1 ? 2 : 1) : 0;
                              }

                              record R
                              {
                                  int {|#1:M|}(int a) => a > 0 ? (a > 1 ? 2 : 1) : 0;
                              }
                              """;

        await VerifyComplexity(source, Limit(1), Complexity(0, 3, 1), Complexity(1, 3, 1));
    }

    /// <summary>
    /// Verifies that a constructor initializer, parameter default values, and attribute arguments are not part of the
    /// body: their conditional expressions do not count, and the members with empty bodies are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2009-002")]
    public async Task ConstructorInitializerParameterDefaultsAndAttributeArgumentsAreNotPartOfTheBody()
    {
        const string source = """
                              using System;

                              class B
                              {
                                  public B(int value)
                                  {
                                  }
                              }

                              class C : B
                              {
                                  public C(int a)
                                      : base(a > 0 ? (a > 1 ? 2 : 1) : 0)
                                  {
                                  }

                                  [Obsolete(true ? (false ? "a" : "b") : "c")]
                                  public void M(int a = true ? (false ? 1 : 2) : 3)
                                  {
                                  }
                              }
                              """;

        await VerifyComplexity(source, Limit(1));
    }

    #endregion // Tests — judged members (RH2009-002)

    #region Tests — cognitive complexity (RH2009-003)

    /// <summary>
    /// Verifies the first example of RH2009-003 at a limit of 5: three nested <c>if</c> statements score 6 and are
    /// reported, three in a row score 3
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2009-003")]
    public async Task DeeperNestingCostsMore()
    {
        await VerifyComplexity(NestedAndSequentialIfs(), Limit(5), Complexity(0, 6, 5));
    }

    /// <summary>
    /// Verifies the first example of RH2009-003 at a limit of 2: the nested <c>if</c> statements are still reported,
    /// and the three in a row are not, since they hold no nesting increment
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2009-003")]
    [Requirement("RH2009-004")]
    public async Task SequentialIfsAboveTheLimitAreNotReported()
    {
        await VerifyComplexity(NestedAndSequentialIfs(), Limit(2), Complexity(0, 6, 2));
    }

    /// <summary>
    /// Verifies the second example of RH2009-003: loops, <c>if</c> chains, logical operators, pattern combinators,
    /// conditional expressions, and <c>catch</c> add up to 11, while <c>try</c>, <c>checked</c>, <c>?.</c>, and
    /// <c>??</c> add nothing
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2009-003")]
    public async Task IncrementsOfTheSecondExampleAddUpToEleven()
    {
        await VerifyComplexity(ElevenExample(), Limit(10), Complexity(0, 11, 10));
    }

    /// <summary>
    /// Verifies the second example of RH2009-003 at a limit of 11: it is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2009-003")]
    public async Task SecondExampleAtItsComplexityIsNotReported()
    {
        await VerifyComplexity(Unmarked(ElevenExample()), Limit(11));
    }

    /// <summary>
    /// Verifies the example of RH2009-003 on sequences of logical operators and pattern combinators at a limit of 4
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2009-003")]
    public async Task SequencesOfLogicalOperatorsAndPatternCombinatorsExample()
    {
        const string source = """
                              class C
                              {
                                  bool _a, _b, _c;

                                  void A() { if (_a) { while (_a && _b && _c) { } } }
                                  void {|#0:B|}() { if (_a) { while (_a && _b || _c) { } } }
                                  void {|#1:D|}() { if (_a) { while (_a && (_b || _c)) { } } }
                                  void {|#2:E|}() { if (_a) { while (!(_a && _b) && _c) { } } }
                                  void F(int x) { if (_a) { while (x is 1 or 2 or 3) { } } }
                                  void {|#3:G|}(int x) { if (_a) { while (x is (1 or 2) and not 3) { } } }
                                  void {|#4:H|}() { if (_a) { while (_a & _b | _c) { } } }
                                  void I(int x) { if (_a) { while ((x & 4) != 0 && _b) { } } }
                                  void {|#5:J|}(int x) { if (_a) { while (_a && x is 1 or 2) { } } }
                                  void K(bool? x) { if (_a) { while ((x & _b) == true) { } } }
                              }
                              """;

        await VerifyComplexity(source,
                               Limit(4),
                               Complexity(0, 5, 4),
                               Complexity(1, 5, 4),
                               Complexity(2, 5, 4),
                               Complexity(3, 5, 4),
                               Complexity(4, 5, 4),
                               Complexity(5, 5, 4));
    }

    /// <summary>
    /// Verifies how the trees and sequences of binary logical operators and pattern combinators are counted, in the
    /// condition of a <c>while</c> (2) inside an <c>if</c> (1)
    /// </summary>
    /// <param name="expression">Condition of the <c>while</c></param>
    /// <param name="complexity">Cognitive complexity of <c>M</c></param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2009-003")]
    [DataRow("_a && _b && _c || _d || _e && _f", 6)]
    [DataRow("_a && !(_b && _c)", 5)]
    [DataRow("_a && (_b && _c)", 4)]
    [DataRow("!_a && !_b", 4)]
    [DataRow("_a && F(_b && _c)", 5)]
    [DataRow("(_a && _b) == (_c && _d)", 5)]
    [DataRow("(_n ?? _a && _b) && _c", 5)]
    [DataRow("_a && (_b ? _c && _d : _e)", 7)]
    [DataRow("_a ^ _b ^ _c", 4)]
    [DataRow("_a | _b & _c", 5)]
    [DataRow("(_x & 4) != 0 && _b", 4)]
    [DataRow("(_x | 1) > 0 || (_x ^ 2) > 0", 4)]
    [DataRow("(_flags & Flags.B) == Flags.B && _a", 4)]
    [DataRow("(_n & _b) == true", 4)]
    [DataRow("(_n | _b) == true", 4)]
    [DataRow("_x is 1 or 2 or 3", 4)]
    [DataRow("_x is (1 or 2) or 3", 4)]
    [DataRow("_x is > 0 and < 10 or 20", 5)]
    [DataRow("_x is not (> 0 and < 10) and not 20", 5)]
    [DataRow("_a && _x is 1 or 2", 5)]
    [DataRow("_x is 1 or 2 && _a", 5)]
    public async Task LogicalOperatorSequencesAreCountedPerTree(string expression, int complexity)
    {
        await VerifyComplexity(LogicalOperatorTemplate.Replace("EXPRESSION", expression), Limit(1), Complexity(0, complexity, 1));
    }

    /// <summary>
    /// Verifies that compound assignments with <c>&amp;=</c>, <c>|=</c>, and <c>^=</c> on <c>bool</c> score nothing: two
    /// nested <c>if</c> statements give 3
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2009-003")]
    public async Task CompoundLogicalAssignmentsScoreNothing()
    {
        const string source = """
                              class C
                              {
                                  bool {|#0:M|}(bool a, bool b)
                                  {
                                      var ok = true;

                                      if (a)
                                      {
                                          if (b)
                                          {
                                              ok &= a;
                                              ok |= b;
                                              ok ^= a;
                                          }
                                      }

                                      return ok;
                                  }
                              }
                              """;

        await VerifyComplexity(source, Limit(1), Complexity(0, 3, 1));
    }

    /// <summary>
    /// Verifies the example of RH2009-003 on chained conditional expressions: a chain, also with a parenthesized last
    /// link, scores like an <c>else if</c> chain, while a conditional expression in a true branch nests
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2009-003")]
    [Requirement("RH2009-004")]
    public async Task ChainedConditionalExpressionScoresLikeElseIfChain()
    {
        const string source = """
                              class C
                              {
                                  string Name(int code) => code == 1 ? "one" : code == 2 ? "two" : (code == 3 ? "three" : "other");

                                  string {|#0:Pick|}(int code, bool upper) => code == 1 ? (upper ? "ONE" : "one") : code == 2 ? "two" : "other";
                              }
                              """;

        await VerifyComplexity(source, Limit(3), Complexity(0, 4, 3));
    }

    /// <summary>
    /// Verifies that a conditional expression that is the false branch of another, directly or inside one or more
    /// pairs of parentheses, continues the chain and carries no nesting increment, however long the chain is
    /// </summary>
    /// <param name="expression">Expression body of <c>M</c></param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2009-003")]
    [Requirement("RH2009-004")]
    [DataRow("""code == 1 ? "one" : (code == 2 ? "two" : "other")""")]
    [DataRow("""code == 1 ? "one" : ((code == 2 ? "two" : "other"))""")]
    [DataRow("""code == 1 ? "one" : code == 2 ? "two" : code == 3 ? "three" : code == 4 ? "four" : code == 5 ? "five" : code == 6 ? "six" : "other" """)]
    public async Task ConditionalChainIsNotNested(string expression)
    {
        var source = $$"""
                       class C
                       {
                           string M(int code) => {{expression}};
                       }
                       """;

        await VerifyComplexity(source, Limit(1));
    }

    /// <summary>
    /// Verifies that a conditional expression inside the false branch of another, but behind another operator, does
    /// not continue the chain and nests: 1 and 2
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2009-003")]
    public async Task ConditionalBehindAnotherOperatorInFalseBranchNests()
    {
        const string source = """
                              class C
                              {
                                  string {|#0:M|}(int code) => code == 1 ? "one" : "x" + (code == 2 ? "two" : "other");
                              }
                              """;

        await VerifyComplexity(source, Limit(1), Complexity(0, 3, 1));
    }

    /// <summary>
    /// Verifies that an <c>else if</c>, an <c>else</c>, a chained conditional expression, and a logical operator score
    /// 1 also inside a nested part, without a nesting increment
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2009-003")]
    [Requirement("RH2009-004")]
    public async Task IncrementsWithoutNestingStayOneInsideNestedPart()
    {
        const string source = """
                              class C
                              {
                                  int {|#0:Chain|}(bool a, bool b, bool c)
                                  {
                                      var x = 0;

                                      if (a)
                                      {
                                          x = b ? 1 : c ? 2 : 3;
                                      }

                                      return x;
                                  }

                                  void {|#1:ElseChain|}(bool a, bool b, bool c)
                                  {
                                      if (a)
                                      {
                                          if (b)
                                          {
                                              a = false;
                                          }
                                          else if (c)
                                          {
                                              b = false;
                                          }
                                          else
                                          {
                                              c = false;
                                          }
                                      }
                                  }

                                  void {|#2:Logical|}(bool a, bool b, bool c)
                                  {
                                      if (a)
                                      {
                                          if (b && c)
                                          {
                                              a = false;
                                          }
                                      }
                                  }
                              }
                              """;

        await VerifyComplexity(source, Limit(1), Complexity(0, 4, 1), Complexity(1, 5, 1), Complexity(2, 4, 1));
    }

    /// <summary>
    /// Verifies the example of RH2009-003 on switches: a switch scores once, its nested part nests, the
    /// <c>&amp;&amp;</c> of a <c>when</c> clause and <c>goto case</c> score 1, and the combinators of a case label and
    /// an arm score nothing
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2009-003")]
    public async Task SwitchScoresOnceAndItsNestedPartNests()
    {
        const string source = """
                              class C
                              {
                                  int {|#0:M|}(int value, char letter)
                                  {
                                      switch (value)
                                      {
                                          case 1 or 2:
                                              return letter switch
                                              {
                                                  'a' or 'e' => 1,
                                                  _ when value > 0 && letter > 'x' => 2,
                                                  _ => 3,
                                              };

                                          case 3:
                                              return 0;

                                          default:
                                              goto case 3;
                                      }
                                  }
                              }
                              """;

        await VerifyComplexity(source, Limit(4), Complexity(0, 5, 4));
    }

    /// <summary>
    /// Verifies that a switch statement and a switch expression score once, however many sections or arms they have
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2009-003")]
    public async Task SwitchScoresOnceForAllItsSectionsAndArms()
    {
        const string source = """
                              class C
                              {
                                  int {|#0:Statement|}(bool a, int x)
                                  {
                                      if (a)
                                      {
                                          switch (x)
                                          {
                                              case 1:
                                                  return 10;

                                              case 2:
                                                  return 20;

                                              case 3:
                                                  return 30;

                                              default:
                                                  return 0;
                                          }
                                      }

                                      return x;
                                  }

                                  int {|#1:Expression|}(bool a, int x)
                                  {
                                      if (a)
                                      {
                                          return x switch
                                          {
                                              1 => 10,
                                              2 => 20,
                                              3 => 30,
                                              _ => 0,
                                          };
                                      }

                                      return x;
                                  }
                              }
                              """;

        await VerifyComplexity(source, Limit(1), Complexity(0, 3, 1), Complexity(1, 3, 1));
    }

    /// <summary>
    /// Verifies that the combinators of a case label and of a switch expression arm score nothing, while those of an
    /// <c>is</c> expression in a <c>when</c> clause count: <c>if</c> 1, <c>switch</c> 2, the <c>or</c> in the
    /// <c>when</c> clause 1, and the switch expression 1
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2009-003")]
    public async Task CombinatorsOfCaseLabelsAndArmsScoreNothing()
    {
        const string source = """
                              class C
                              {
                                  int {|#0:M|}(int value, bool flag)
                                  {
                                      if (flag)
                                      {
                                          switch (value)
                                          {
                                              case 1 or 2:
                                                  return 1;

                                              case > 2 when value is 3 or 4:
                                                  return 2;

                                              default:
                                                  return 0;
                                          }
                                      }

                                      return value switch
                                      {
                                          5 or 6 => 1,
                                          _ => 0,
                                      };
                                  }
                              }
                              """;

        await VerifyComplexity(source, Limit(1), Complexity(0, 5, 1));
    }

    /// <summary>
    /// Verifies the example of RH2009-003 on <c>when</c> clauses: a conditional expression in a <c>when</c> clause lies
    /// in the nested part of its switch and scores 2
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2009-003")]
    [Requirement("RH2009-006")]
    public async Task ConditionalInWhenClauseIsNestedInTheSwitch()
    {
        const string source = """
                              class C
                              {
                                  int {|#0:M|}(int value, bool strict)
                                  {
                                      switch (value)
                                      {
                                          case > 0 when (strict ? value > 10 : value > 1):
                                              return 1;

                                          default:
                                              return 0;
                                      }
                                  }
                              }
                              """;

        await VerifyComplexity(source, Limit(2), Complexity(0, 3, 2));
    }

    /// <summary>
    /// Verifies that a switch expression in a <c>when</c> clause lies in the nested part of the switch statement: 1
    /// and 2
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2009-003")]
    [Requirement("RH2009-004")]
    public async Task SwitchExpressionInWhenClauseIsNestedInTheSwitch()
    {
        const string source = """
                              class C
                              {
                                  int {|#0:M|}(int value, char letter)
                                  {
                                      switch (value)
                                      {
                                          case > 0 when (letter switch { 'a' => true, _ => false }):
                                              return 1;

                                          default:
                                              return 0;
                                      }
                                  }
                              }
                              """;

        await VerifyComplexity(source, Limit(1), Complexity(0, 3, 1));
    }

    /// <summary>
    /// Verifies that each form of <c>goto</c> scores 1: <c>if</c> 1, <c>if</c> 2, <c>goto</c> 1, <c>switch</c> 1,
    /// <c>goto case</c> 1, <c>goto default</c> 1
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2009-003")]
    public async Task EveryFormOfGotoScoresOne()
    {
        const string source = """
                              class C
                              {
                                  int {|#0:M|}(int value)
                                  {
                                      if (value > 0)
                                      {
                                          if (value > 1)
                                          {
                                              goto done;
                                          }
                                      }

                                      switch (value)
                                      {
                                          case 1:
                                              goto case 2;

                                          case 2:
                                              goto default;

                                          default:
                                              break;
                                      }

                                  done:
                                      return value;
                                  }
                              }
                              """;

        await VerifyComplexity(source, Limit(1), Complexity(0, 7, 1));
    }

    /// <summary>
    /// Verifies that each <c>catch</c> clause scores, that the <c>&amp;&amp;</c> of an exception filter counts, and that
    /// <c>try</c> and <c>finally</c> score nothing: <c>catch</c> 1, <c>if</c> 2, <c>catch</c> 1, <c>&amp;&amp;</c> 1
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2009-003")]
    public async Task EveryCatchClauseScoresAndTryAndFinallyDoNot()
    {
        const string source = """
                              class C
                              {
                                  void {|#0:M|}(int value)
                                  {
                                      try
                                      {
                                          Log(value);
                                      }
                                      catch (System.ArgumentException)
                                      {
                                          if (value > 0)
                                          {
                                              Log(1);
                                          }
                                      }
                                      catch (System.InvalidOperationException) when (value > 1 && value < 5)
                                      {
                                          Log(2);
                                      }
                                      finally
                                      {
                                          Log(3);
                                      }
                                  }

                                  void Log(int value)
                                  {
                                  }
                              }
                              """;

        await VerifyComplexity(source, Limit(1), Complexity(0, 5, 1));
    }

    /// <summary>
    /// Verifies the example of RH2009-003 on recursion: a method that calls itself scores 1 once, however many calls it
    /// holds, and a member that calls it back scores nothing for recursion
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2009-003")]
    public async Task RecursionScoresOnceAndOnlyForDirectSelfCalls()
    {
        const string source = """
                              class C
                              {
                                  int {|#0:Fib|}(int n)
                                  {
                                      if (n > 1)
                                      {
                                          if (n > 2)
                                          {
                                              return Fib(n - 1) + Fib(n - 2);
                                          }
                                      }

                                      return n;
                                  }

                                  bool IsEven(int n)
                                  {
                                      if (n > 0)
                                      {
                                          if (n > 1)
                                          {
                                              return IsOdd(n - 1);
                                          }
                                      }

                                      return n == 0;
                                  }

                                  bool IsOdd(int n) => n != 0 && IsEven(n - 1);
                              }
                              """;

        await VerifyComplexity(source, Limit(3), Complexity(0, 4, 3));
    }

    /// <summary>
    /// Verifies which uses of the method count as recursion, in a method with two nested <c>if</c> statements (3): an
    /// invocation of the method itself, also through <c>this</c>, on another instance, and inside a lambda, adds 1 once,
    /// also when a call on another instance and a direct call stand together; a call of another overload, also on
    /// another instance, or of another method, a method group, and a self-call inside a local function add nothing
    /// </summary>
    /// <param name="call">Statements inside the inner <c>if</c></param>
    /// <param name="complexity">Cognitive complexity of <c>M</c></param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2009-003")]
    [DataRow("M(n - 1);", 4)]
    [DataRow("this.M(n - 1);", 4)]
    [DataRow("M(n - 1); M(n - 2);", 4)]
    [DataRow("new C().M(n - 1);", 4)]
    [DataRow("new C().M(n - 1); M(n - 2);", 4)]
    [DataRow("new C().M(\"text\");", 3)]
    [DataRow("System.Func<int> f = () => M(n - 1);", 4)]
    [DataRow("M(\"text\");", 3)]
    [DataRow("S(n);", 3)]
    [DataRow("G<int>(n);", 3)]
    [DataRow("System.Func<int, int> f = M;", 3)]
    [DataRow("Local(n); int Local(int value) { return M(value); }", 3)]
    public async Task OnlyAnInvocationOfTheMemberItselfIsRecursion(string call, int complexity)
    {
        await VerifyComplexity(RecursionTemplate.Replace("CALL", call), Limit(1), Complexity(0, complexity, 1));
    }

    /// <summary>
    /// Verifies that a static method called through its type name and a generic method called with type arguments
    /// score 1 for recursion: 3 and 1
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2009-003")]
    public async Task QualifiedAndGenericSelfCallsAreRecursion()
    {
        const string source = """
                              class C
                              {
                                  static int {|#0:S|}(int n)
                                  {
                                      if (n > 0)
                                      {
                                          if (n > 1)
                                          {
                                              return C.S(n - 1);
                                          }
                                      }

                                      return n;
                                  }

                                  int {|#1:G|}<T>(int n)
                                  {
                                      if (n > 0)
                                      {
                                          if (n > 1)
                                          {
                                              return G<T>(n - 1);
                                          }
                                      }

                                      return n;
                                  }
                              }
                              """;

        await VerifyComplexity(source, Limit(1), Complexity(0, 4, 1), Complexity(1, 4, 1));
    }

    /// <summary>
    /// Verifies the example of RH2009-003 on recursion on another instance: <c>Depth</c> calls itself on
    /// <c>Left</c> and <c>Right</c> and has a cognitive complexity of 4 — <c>if</c> 1, <c>if</c> 2 (nesting 1),
    /// recursion 1, once for both calls — and is reported at a limit of 3
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2009-003")]
    public async Task RecursionOnAnotherInstanceExample()
    {
        const string source = """
                              using System;

                              class Node
                              {
                                  public Node Left;

                                  public Node Right;

                                  public int {|#0:Depth|}()
                                  {
                                      if (Left != null)
                                      {
                                          if (Right != null)
                                          {
                                              return 1 + Math.Max(Left.Depth(), Right.Depth());
                                          }
                                      }

                                      return 1;
                                  }
                              }
                              """;

        await VerifyComplexity(source, Limit(3), Complexity(0, 4, 3));
    }

    /// <summary>
    /// Verifies that a call of a member of the same name and signature that another type declares is not recursion,
    /// also on an instance: <c>Depth</c> calls <c>Leaf.Depth</c> and has a cognitive complexity of 3, so it is not
    /// reported at a limit of 3
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2009-003")]
    public async Task SameNamedMemberOfAnotherTypeIsNotRecursion()
    {
        const string source = """
                              using System;

                              class Leaf
                              {
                                  public int Depth()
                                  {
                                      return 1;
                                  }
                              }

                              class Node
                              {
                                  public Leaf Left;

                                  public Leaf Right;

                                  public int Depth()
                                  {
                                      if (Left != null)
                                      {
                                          if (Right != null)
                                          {
                                              return 1 + Math.Max(Left.Depth(), Right.Depth());
                                          }
                                      }

                                      return 1;
                                  }
                              }
                              """;

        await VerifyComplexity(source, Limit(3));
    }

    /// <summary>
    /// Verifies the example of RH2009-003 that a constructor creating its own type with <c>new</c> and a compound
    /// assignment score nothing: both members have 3 and are not reported at a limit of 3
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2009-003")]
    public async Task NewOfOwnTypeAndCompoundAssignmentScoreNothingExample()
    {
        await VerifyComplexity(Unmarked(NodeExample()), Limit(3));
    }

    /// <summary>
    /// Verifies the complexities the example of RH2009-003 states for the constructor of <c>Node</c> and for
    /// <c>All</c>: 3 each
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2009-003")]
    public async Task NewOfOwnTypeAndCompoundAssignmentScoreNothing()
    {
        await VerifyComplexity(NodeExample(), Limit(2), Complexity(0, 3, 2), Complexity(1, 3, 2));
    }

    /// <summary>
    /// Verifies that an accessor that uses its own property and an operator that applies itself are not recursion:
    /// each has 3
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2009-003")]
    public async Task OwnPropertyInAccessorAndOwnOperatorInOperatorAreNotRecursion()
    {
        const string source = """
                              class C
                              {
                                  private bool _a, _b;

                                  public int Value
                                  {
                                      {|#0:get|}
                                      {
                                          if (_a)
                                          {
                                              if (_b)
                                              {
                                                  return Value;
                                              }
                                          }

                                          return 0;
                                      }
                                  }

                                  public static C operator {|#1:+|}(C left, C right)
                                  {
                                      if (left._a)
                                      {
                                          if (right._a)
                                          {
                                              return left + right;
                                          }
                                      }

                                      return left;
                                  }
                              }
                              """;

        await VerifyComplexity(source, Limit(1), Complexity(0, 3, 1), Complexity(1, 3, 1));
    }

    /// <summary>
    /// Verifies the example of RH2009-003 that <c>lock</c>, <c>using</c>, <c>try</c>, <c>finally</c>, a type pattern,
    /// <c>throw</c>, and <c>??</c> score nothing and do not nest: at a limit of 3 it is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2009-003")]
    public async Task StatementsThatDoNotNestExample()
    {
        await VerifyComplexity(Unmarked(LockUsingTryExample()), Limit(3));
    }

    /// <summary>
    /// Verifies the complexity the example of RH2009-003 states for the method holding <c>lock</c>, <c>using</c>, and
    /// <c>try</c>: 3, the outer <c>if</c> 1 and the inner <c>if</c> 2
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2009-003")]
    public async Task StatementsThatDoNotNestLeaveTheInnerIfAtNestingOne()
    {
        await VerifyComplexity(LockUsingTryExample(), Limit(2), Complexity(0, 3, 2));
    }

    /// <summary>
    /// Verifies that none of the statements RH2009-003 lists as not nesting raises the nesting of an <c>if</c> inside
    /// it: the outer <c>if</c> 1 and the inner <c>if</c> 2
    /// </summary>
    /// <param name="statement">Statement inside the outer <c>if</c> that holds the inner <c>if</c></param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2009-003")]
    [DataRow("lock (_gate) { if (b) { x++; } }")]
    [DataRow("using (var stream = new System.IO.MemoryStream()) { if (b) { x++; } }")]
    [DataRow("using var stream = new System.IO.MemoryStream(); if (b) { x++; }")]
    [DataRow("checked { if (b) { x++; } }")]
    [DataRow("unchecked { if (b) { x++; } }")]
    [DataRow("{ if (b) { x++; } }")]
    [DataRow("try { if (b) { x++; } } finally { }")]
    [DataRow("try { } finally { if (b) { x++; } }")]
    [DataRow("label: { if (b) { x++; } }")]
    [DataRow("unsafe { if (b) { x++; } }")]
    [DataRow("fixed (int* p = xs) { if (b) { x++; } }")]
    public async Task StatementThatDoesNotNestLeavesTheNestingUnchanged(string statement)
    {
        await VerifyComplexity(NestedStatementTemplate.Replace("STATEMENT", statement), Limit(1), AllowUnsafe, Complexity(0, 3, 1));
    }

    /// <summary>
    /// Verifies that <c>??=</c>, <c>?[]</c>, <c>?.</c>, <c>??</c>, and the null-forgiving operator score nothing: two
    /// nested <c>if</c> statements give 3
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2009-003")]
    public async Task NullHandlingOperatorsScoreNothing()
    {
        const string source = """
                              #nullable enable

                              class C
                              {
                                  void {|#0:M|}(bool a, bool b, string? text, string?[]? items, string? fallback)
                                  {
                                      if (a)
                                      {
                                          if (b)
                                          {
                                              text ??= items?[0]?.Trim() ?? fallback!;
                                          }
                                      }
                                  }
                              }
                              """;

        await VerifyComplexity(source, Limit(1), Complexity(0, 3, 1));
    }

    /// <summary>
    /// Verifies that <c>continue</c>, <c>break</c>, <c>throw</c>, <c>yield return</c>, and <c>yield break</c> score
    /// nothing: <c>foreach</c> 1 and three <c>if</c> statements 2 each
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2009-003")]
    public async Task JumpsOtherThanGotoScoreNothing()
    {
        const string source = """
                              using System;
                              using System.Collections.Generic;

                              class C
                              {
                                  IEnumerable<int> {|#0:M|}(int[] values)
                                  {
                                      foreach (var value in values)
                                      {
                                          if (value < 0)
                                          {
                                              continue;
                                          }

                                          if (value > 100)
                                          {
                                              break;
                                          }

                                          if (value == 50)
                                          {
                                              throw new InvalidOperationException();
                                          }

                                          yield return value;
                                      }

                                      yield break;
                                  }
                              }
                              """;

        await VerifyComplexity(source, Limit(1), Complexity(0, 7, 1));
    }

    /// <summary>
    /// Verifies the example of RH2009-003 on query expressions: a query clause neither scores nor nests, while the
    /// operators and conditional expressions in it count at the level where the query stands
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2009-003")]
    public async Task QueryClausesNeitherScoreNorNest()
    {
        const string source = """
                              using System.Linq;

                              class C
                              {
                                  int[] {|#0:M|}(int[] values, bool enabled)
                                  {
                                      if (enabled)
                                      {
                                          return (from value in values
                                                  where value > 0 && value < 10
                                                  select value > 5 ? value : 0).ToArray();
                                      }

                                      return values;
                                  }
                              }
                              """;

        await VerifyComplexity(source, Limit(3), Complexity(0, 4, 3));
    }

    /// <summary>
    /// Verifies that a <c>foreach</c> with a deconstruction and an <c>await foreach</c> score like <c>foreach</c> and
    /// nest their body: 1 and 2
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2009-003")]
    public async Task DeconstructingAndAwaitedForeachAreLoops()
    {
        const string source = """
                              using System.Collections.Generic;
                              using System.Threading.Tasks;

                              class C
                              {
                                  void {|#0:M|}(List<(int, int)> pairs)
                                  {
                                      foreach (var (left, right) in pairs)
                                      {
                                          if (left > right)
                                          {
                                              System.Console.WriteLine(left);
                                          }
                                      }
                                  }

                                  async Task {|#1:N|}(IAsyncEnumerable<int> values)
                                  {
                                      await foreach (var value in values)
                                      {
                                          if (value > 0)
                                          {
                                              System.Console.WriteLine(value);
                                          }
                                      }
                                  }
                              }
                              """;

        await VerifyComplexity(source, Limit(1), Complexity(0, 3, 1), Complexity(1, 3, 1));
    }

    /// <summary>
    /// Verifies that every loop kind nests its body: each loop 1 with an <c>if</c> 2 inside it
    /// </summary>
    /// <param name="loop">Loop whose body holds the <c>if</c></param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2009-003")]
    [DataRow("for (var i = 0; i < x; i++) { if (b) { x--; } }")]
    [DataRow("foreach (var item in xs) { if (b) { x--; } }")]
    [DataRow("while (x > 0) { if (b) { x--; } }")]
    [DataRow("do { if (b) { x--; } } while (x > 0);")]
    public async Task EveryLoopNestsItsBody(string loop)
    {
        var source = FlatStatementTemplate.Replace("void M(", "void {|#0:M|}(").Replace("STATEMENT", loop);

        await VerifyComplexity(source, Limit(1), Complexity(0, 3, 1));
    }

    /// <summary>
    /// Verifies that an <c>if</c> without braces inside another nests: 1 and 2
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2009-003")]
    [Requirement("RH2009-004")]
    public async Task IfWithoutBracesInsideAnotherNests()
    {
        const string source = """
                              class C
                              {
                                  void {|#0:M|}(bool a, bool b)
                                  {
                                      if (a)
                                          if (b)
                                              System.Console.WriteLine();
                                  }
                              }
                              """;

        await VerifyComplexity(source, Limit(1), Complexity(0, 3, 1));
    }

    /// <summary>
    /// Verifies that an <c>if … else</c> in the block of an <c>else</c> clause nests: <c>if</c> 1, <c>else</c> 1, the
    /// nested <c>if</c> 2, its <c>else</c> 1
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2009-003")]
    [Requirement("RH2009-004")]
    public async Task IfElseInElseBlockNests()
    {
        const string source = """
                              class C
                              {
                                  int {|#0:M|}(int value)
                                  {
                                      if (value == 1)
                                      {
                                          return 10;
                                      }
                                      else
                                      {
                                          if (value > 5)
                                          {
                                              value++;
                                          }
                                          else
                                          {
                                              value--;
                                          }
                                      }

                                      return value;
                                  }
                              }
                              """;

        await VerifyComplexity(source, Limit(1), Complexity(0, 5, 1));
    }

    /// <summary>
    /// Verifies that a conditional expression in a condition, a loop header, a governing expression, or an exception
    /// filter lies outside the nested part of its construct: inside an <c>if</c> (1), the construct scores 2 and the
    /// conditional expression 2, not 3
    /// </summary>
    /// <param name="statement">Statement inside the outer <c>if</c></param>
    /// <param name="complexity">Cognitive complexity of <c>M</c></param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2009-003")]
    [DataRow("if (b ? a : x > 0) { }", 5)]
    [DataRow("if (a) { } else if (b ? a : x > 0) { }", 6)]
    [DataRow("while (b ? a : x > 0) { }", 5)]
    [DataRow("do { } while (b ? a : x > 0);", 5)]
    [DataRow("for (var i = b ? 1 : 0; i < 3; i++) { }", 5)]
    [DataRow("for (var i = 0; i < (b ? 2 : 3); i++) { }", 5)]
    [DataRow("for (var i = 0; i < 3; i += b ? 1 : 2) { }", 5)]
    [DataRow("foreach (var item in b ? xs : ys) { }", 5)]
    [DataRow("switch (b ? x : 0) { default: break; }", 5)]
    [DataRow("x = (b ? x : 0) switch { 0 => 1, _ => 2 };", 5)]
    [DataRow("x = (b ? a : x > 0) ? 1 : 2;", 5)]
    [DataRow("try { } catch (System.Exception) when (b ? a : x > 0) { }", 5)]
    public async Task StructureInConditionIsOutsideTheNestedPartOfItsConstruct(string statement, int complexity)
    {
        await VerifyComplexity(NestedStatementTemplate.Replace("STATEMENT", statement), Limit(1), AllowUnsafe, Complexity(0, complexity, 1));
    }

    /// <summary>
    /// Verifies that a conditional directive scores nothing and does not nest: an <c>if</c> inside an active
    /// <c>#if</c> group inside an <c>if</c> scores 2
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2009-003")]
    public async Task ConditionalDirectiveNeitherScoresNorNests()
    {
        await VerifyComplexity(DirectiveExample(), Limit(1), DefineSymbols("FEATURE"), Complexity(0, 3, 1));
    }

    /// <summary>
    /// Verifies that an <c>if</c> inside an active <c>#if</c> group at the top of a body has no nesting increment, so
    /// the member is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2009-003")]
    [Requirement("RH2009-004")]
    public async Task ConditionalDirectiveDoesNotMakeItsCodeNested()
    {
        const string source = """
                              class C
                              {
                                  int M(int value)
                                  {
                              #if DEBUG
                                      if (value > 0)
                                      {
                                          value++;
                                      }
                              #endif

                                      if (value > 1)
                                      {
                                          value--;
                                      }

                                      return value;
                                  }
                              }
                              """;

        await VerifyComplexity(source, Limit(1), DefineSymbols("DEBUG"));
    }

    #endregion // Tests — cognitive complexity (RH2009-003)

    #region Tests — nesting exception (RH2009-004)

    /// <summary>
    /// Verifies the first example of RH2009-004: a body with a complexity of 9 and no nesting increment is not reported
    /// at a limit of 1
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2009-004")]
    public async Task BodyWithoutNestingIncrementIsNotReported()
    {
        await VerifyComplexity(FlatExample(false), Limit(1));
    }

    /// <summary>
    /// Verifies the first example of RH2009-004 with the conditional expression moved into the block of the second
    /// <c>if</c>: it scores 2 there, and the member is reported with 10
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2009-004")]
    public async Task SameBodyWithOneNestedStructureIsReportedByItsWholeComplexity()
    {
        await VerifyComplexity(FlatExample(true), Limit(1), Complexity(0, 10, 1));
    }

    /// <summary>
    /// Verifies the second example of RH2009-004: an <c>if</c> in the block of an <c>else</c> clause nests, and the
    /// member is reported with 4
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2009-004")]
    public async Task IfInElseBlockMakesTheMemberReportable()
    {
        const string source = """
                              class C
                              {
                                  int {|#0:M|}(int value)
                                  {
                                      if (value == 1)
                                      {
                                          return 10;
                                      }
                                      else
                                      {
                                          if (value > 5)
                                          {
                                              value++;
                                          }
                                      }

                                      return value;
                                  }
                              }
                              """;

        await VerifyComplexity(source, Limit(1), Complexity(0, 4, 1));
    }

    /// <summary>
    /// Verifies the second example of RH2009-004 written as <c>else if</c>: the chain holds no nesting increment and is
    /// not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2009-004")]
    public async Task ElseIfChainIsNotReported()
    {
        const string source = """
                              class C
                              {
                                  int M(int value)
                                  {
                                      if (value == 1)
                                      {
                                          return 10;
                                      }
                                      else if (value > 5)
                                      {
                                          value++;
                                      }

                                      return value;
                                  }
                              }
                              """;

        await VerifyComplexity(source, Limit(1));
    }

    /// <summary>
    /// Verifies the third example of RH2009-004: an <c>if</c> in a <c>try</c> block does not nest, an <c>if</c> in a
    /// <c>catch</c> block does
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2009-004")]
    public async Task IfInCatchBlockNestsAndIfInTryBlockDoesNot()
    {
        const string source = """
                              class C
                              {
                                  void M(int value)
                                  {
                                      try
                                      {
                                          if (value > 0)
                                          {
                                              Log(value);
                                          }
                                      }
                                      finally
                                      {
                                          Log(0);
                                      }
                                  }

                                  void {|#0:N|}(int value)
                                  {
                                      try
                                      {
                                          Log(value);
                                      }
                                      catch (System.InvalidOperationException)
                                      {
                                          if (value > 0)
                                          {
                                              Log(0);
                                          }
                                      }
                                  }

                                  void Log(int value)
                                  {
                                  }
                              }
                              """;

        await VerifyComplexity(source, Limit(1), Complexity(0, 3, 1));
    }

    /// <summary>
    /// Verifies that a structure in a condition, a loop header, a governing expression, or an exception filter does not
    /// lie in a nested part, so a body that holds nothing else is not reported
    /// </summary>
    /// <param name="statement">Statement at the top of the body</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2009-004")]
    [DataRow("if (b ? a : x > 0) { }")]
    [DataRow("if (a) { } else if (b ? a : x > 0) { }")]
    [DataRow("while (b ? a : x > 0) { }")]
    [DataRow("do { } while (b ? a : x > 0);")]
    [DataRow("for (var i = b ? 1 : 0; i < (b ? 2 : 3); i += b ? 1 : 2) { }")]
    [DataRow("foreach (var item in b ? xs : ys) { }")]
    [DataRow("switch (b ? x : 0) { default: break; }")]
    [DataRow("x = (b ? x : 0) switch { 0 => 1, _ => 2 };")]
    [DataRow("x = (b ? a : x > 0) ? 1 : 2;")]
    [DataRow("try { } catch (System.Exception) when (b ? a : x > 0) { }")]
    public async Task StructureInConditionDoesNotMakeTheMemberReportable(string statement)
    {
        await VerifyComplexity(FlatStatementTemplate.Replace("STATEMENT", statement), Limit(1));
    }

    /// <summary>
    /// Verifies the fourth example of RH2009-004: a conditional expression in a lambda scores 2, but nesting that comes
    /// only from the lambda does not make the member reportable
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2009-004")]
    public async Task NestingOnlyThroughLambdaIsNotReported()
    {
        const string source = """
                              using System.Linq;

                              class C
                              {
                                  int[] M(int[] values) => values.Select(value => value > 0 ? value : 0).ToArray();
                              }
                              """;

        await VerifyComplexity(source, Limit(1));
    }

    /// <summary>
    /// Verifies that an <c>if</c> in a block lambda and in an anonymous method, whose only enclosing nesting constructs
    /// are the lambda and the anonymous method, does not make the member reportable
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2009-004")]
    public async Task NestingOnlyThroughBlockLambdaOrAnonymousMethodIsNotReported()
    {
        const string source = """
                              using System;
                              using System.Collections.Generic;

                              class C
                              {
                                  void M(List<int> values)
                                  {
                                      values.ForEach(value =>
                                      {
                                          if (value > 0)
                                          {
                                              Console.WriteLine(value);
                                          }
                                      });

                                      values.ForEach(delegate (int value)
                                      {
                                          if (value > 1)
                                          {
                                              Console.WriteLine(value);
                                          }
                                      });
                                  }
                              }
                              """;

        await VerifyComplexity(source, Limit(1));
    }

    /// <summary>
    /// Verifies that an <c>if</c> nested in another <c>if</c> inside a lambda makes the member reportable: the outer
    /// <c>if</c> scores 2 (nesting 1, from the lambda) and the inner one 3
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2009-004")]
    [Requirement("RH2009-005")]
    public async Task NestingInsideLambdaMakesTheMemberReportable()
    {
        const string source = """
                              using System;
                              using System.Collections.Generic;

                              class C
                              {
                                  void {|#0:M|}(List<int> values)
                                  {
                                      values.ForEach(value =>
                                      {
                                          if (value > 0)
                                          {
                                              if (value > 1)
                                              {
                                                  Console.WriteLine(value);
                                              }
                                          }
                                      });
                                  }
                              }
                              """;

        await VerifyComplexity(source, Limit(1), Complexity(0, 5, 1));
    }

    /// <summary>
    /// Verifies that code a conditional directive excludes counts as absent: the nested <c>if</c> in an inactive
    /// <c>#if</c> group does not make the member reportable
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2009-004")]
    [Requirement("RH2009-006")]
    public async Task NestingInExcludedCodeDoesNotMakeTheMemberReportable()
    {
        await VerifyComplexity(Unmarked(DirectiveExample()), Limit(1));
    }

    #endregion // Tests — nesting exception (RH2009-004)

    #region Tests — lambdas and local functions (RH2009-005)

    /// <summary>
    /// Verifies the first example of RH2009-005: the <c>if</c> in a lambda inside an <c>if</c> scores 3
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2009-005")]
    public async Task LambdaBodyCountsTowardTheMemberOneLevelDeeper()
    {
        await VerifyComplexity(LambdaExample("value =>"), Limit(3), Complexity(0, 4, 3));
    }

    /// <summary>
    /// Verifies the first example of RH2009-005 with an anonymous method instead of the lambda: same complexity
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2009-005")]
    public async Task AnonymousMethodBodyCountsTowardTheMemberOneLevelDeeper()
    {
        await VerifyComplexity(LambdaExample("delegate (int value)"), Limit(3), Complexity(0, 4, 3));
    }

    /// <summary>
    /// Verifies that an expression lambda inside a nested part nests its conditional expression two levels deep:
    /// <c>if</c> 1, conditional expression 3
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2009-005")]
    public async Task ExpressionLambdaInsideNestedPartAddsItsLevel()
    {
        const string source = """
                              using System.Linq;

                              class C
                              {
                                  int[] {|#0:M|}(int[] values, bool enabled)
                                  {
                                      if (enabled)
                                      {
                                          return values.Select(value => value > 0 ? value : 0).ToArray();
                                      }

                                      return values;
                                  }
                              }
                              """;

        await VerifyComplexity(source, Limit(1), Complexity(0, 4, 1));
    }

    /// <summary>
    /// Verifies that a lambda inside a lambda adds a level of its own: <c>if</c> 1, the <c>if</c> inside both lambdas 4
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2009-005")]
    public async Task LambdaInsideLambdaAddsAnotherLevel()
    {
        const string source = """
                              using System;
                              using System.Collections.Generic;

                              class C
                              {
                                  void {|#0:M|}(List<List<int>> groups, bool enabled)
                                  {
                                      if (enabled)
                                      {
                                          groups.ForEach(group => group.ForEach(value =>
                                          {
                                              if (value > 0)
                                              {
                                                  Console.WriteLine(value);
                                              }
                                          }));
                                      }
                                  }
                              }
                              """;

        await VerifyComplexity(source, Limit(1), Complexity(0, 5, 1));
    }

    /// <summary>
    /// Verifies the second example of RH2009-005 at a limit of 2: the local function is judged on its own with 3, and
    /// the member that holds it has 1
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2009-005")]
    [Requirement("RH2009-006")]
    public async Task LocalFunctionIsJudgedOnItsOwn()
    {
        await VerifyComplexity(LocalFunctionExample(), Limit(2), Complexity(0, 3, 2));
    }

    /// <summary>
    /// Verifies the second example of RH2009-005 at a limit of 3: neither the member nor its local function is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2009-005")]
    public async Task LocalFunctionAtTheLimitIsNotReported()
    {
        await VerifyComplexity(Unmarked(LocalFunctionExample()), Limit(3));
    }

    /// <summary>
    /// Verifies that a local function declared in a nested part starts its nesting at 0 and adds nothing to its holder,
    /// and that the holder above the limit is reported as well: 3 each
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2009-005")]
    [Requirement("RH2009-006")]
    public async Task LocalFunctionInNestedPartStartsAtNestingZero()
    {
        const string source = """
                              class C
                              {
                                  void {|#0:M|}(bool a, bool b)
                                  {
                                      if (a)
                                      {
                                          if (b)
                                          {
                                              Local(1);

                                              void {|#1:Local|}(int x)
                                              {
                                                  if (x > 0)
                                                  {
                                                      if (x > 1)
                                                      {
                                                          System.Console.WriteLine(x);
                                                      }
                                                  }
                                              }
                                          }
                                      }
                                  }
                              }
                              """;

        await VerifyComplexity(source, Limit(1), Complexity(0, 3, 1), Complexity(1, 3, 1));
    }

    /// <summary>
    /// Verifies that a local function that calls itself scores 1 for its own recursion, which does not count toward the
    /// member that holds it: the member has 3, the local function 4
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2009-003")]
    [Requirement("RH2009-005")]
    public async Task RecursionOfLocalFunctionDoesNotCountTowardItsHolder()
    {
        const string source = """
                              class C
                              {
                                  void {|#0:M|}(bool a, bool b)
                                  {
                                      if (a)
                                      {
                                          if (b)
                                          {
                                              Local(3);
                                          }
                                      }

                                      void {|#1:Local|}(int x)
                                      {
                                          if (x > 0)
                                          {
                                              if (x > 1)
                                              {
                                                  Local(x - 1);
                                              }
                                          }
                                      }
                                  }
                              }
                              """;

        await VerifyComplexity(source, Limit(1), Complexity(0, 3, 1), Complexity(1, 4, 1));
    }

    /// <summary>
    /// Verifies that a local function inside a local function is judged on its own and adds nothing to the outer one,
    /// and that the member holding both has 0
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2009-005")]
    public async Task LocalFunctionInsideLocalFunctionIsJudgedOnItsOwn()
    {
        const string source = """
                              class C
                              {
                                  void M(int value)
                                  {
                                      Outer(value);

                                      void {|#0:Outer|}(int x)
                                      {
                                          if (x > 0)
                                          {
                                              if (x > 1)
                                              {
                                                  Inner(x);
                                              }
                                          }

                                          void {|#1:Inner|}(int y)
                                          {
                                              if (y > 0)
                                              {
                                                  if (y > 1)
                                                  {
                                                      System.Console.WriteLine(y);
                                                  }
                                              }
                                          }
                                      }
                                  }
                              }
                              """;

        await VerifyComplexity(source, Limit(1), Complexity(0, 3, 1), Complexity(1, 3, 1));
    }

    /// <summary>
    /// Verifies that a local function inside a lambda is judged on its own, with its nesting starting at 0, and adds
    /// nothing to the member that holds the lambda
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2009-005")]
    public async Task LocalFunctionInsideLambdaIsJudgedOnItsOwn()
    {
        const string source = """
                              using System;
                              using System.Collections.Generic;

                              class C
                              {
                                  void M(List<int> values)
                                  {
                                      values.ForEach(value =>
                                      {
                                          Console.WriteLine(Local(value));

                                          int {|#0:Local|}(int input)
                                          {
                                              if (input > 0)
                                              {
                                                  if (input > 1)
                                                  {
                                                      return 2;
                                                  }
                                              }

                                              return 0;
                                          }
                                      });
                                  }
                              }
                              """;

        await VerifyComplexity(source, Limit(1), Complexity(0, 3, 1));
    }

    /// <summary>
    /// Verifies that a local function inside a lambda of a field initializer is judged on its own, while the lambda is
    /// not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2009-005")]
    public async Task LocalFunctionInsideFieldInitializerLambdaIsJudged()
    {
        const string source = """
                              using System;

                              class C
                              {
                                  private static readonly Func<int, int> Compute = value =>
                                  {
                                      if (value > 0)
                                      {
                                          if (value > 1)
                                          {
                                              return Local(value);
                                          }
                                      }

                                      return 0;

                                      static int {|#0:Local|}(int input)
                                      {
                                          if (input > 0)
                                          {
                                              if (input > 1)
                                              {
                                                  return 2;
                                              }
                                          }

                                          return 0;
                                      }
                                  };
                              }
                              """;

        await VerifyComplexity(source, Limit(1), Complexity(0, 3, 1));
    }

    /// <summary>
    /// Verifies that a local function in top-level statements is judged on its own, while the top-level statements are
    /// not judged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2009-005")]
    public async Task LocalFunctionInTopLevelStatementsIsJudged()
    {
        const string source = """
                              if (args.Length > 0)
                              {
                                  if (args.Length > 1)
                                  {
                                      Local(args.Length);
                                  }
                              }

                              static void {|#0:Local|}(int x)
                              {
                                  if (x > 0)
                                  {
                                      if (x > 1)
                                      {
                                          System.Console.WriteLine(x);
                                      }
                                  }
                              }
                              """;

        await VerifyComplexity(source, Limit(1), test => test.TestState.OutputKind = OutputKind.ConsoleApplication, Complexity(0, 3, 1));
    }

    /// <summary>
    /// Verifies the third example of RH2009-005: a lambda in a field initializer is not judged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2009-005")]
    public async Task LambdaInFieldInitializerIsNotJudged()
    {
        const string source = """
                              using System;

                              class C
                              {
                                  private static readonly Func<int, int> Compute = value =>
                                  {
                                      if (value > 0)
                                      {
                                          if (value > 1)
                                          {
                                              return 2;
                                          }
                                      }

                                      return 0;
                                  };
                              }
                              """;

        await VerifyComplexity(source, Limit(1));
    }

    /// <summary>
    /// Verifies that a lambda in a property initializer, an anonymous method in a field initializer, a lambda in an
    /// event initializer, and a lambda in a constructor initializer are not judged, and that the constructor with an
    /// empty body is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2009-005")]
    public async Task LambdaOutsideEveryJudgedBodyIsNotJudged()
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
                                      if (Environment.TickCount > 0)
                                      {
                                          if (Environment.TickCount > 1)
                                          {
                                              return 2;
                                          }
                                      }

                                      return 0;
                                  };

                                  private readonly Func<int> _other = delegate
                                  {
                                      if (Environment.TickCount > 0)
                                      {
                                          if (Environment.TickCount > 1)
                                          {
                                              return 2;
                                          }
                                      }

                                      return 0;
                                  };

                                  public event Action Changed = () =>
                                  {
                                      if (Environment.TickCount > 0)
                                      {
                                          if (Environment.TickCount > 1)
                                          {
                                              Console.WriteLine();
                                          }
                                      }
                                  };

                                  public C()
                                      : base(() =>
                                             {
                                                 if (Environment.TickCount > 0)
                                                 {
                                                     if (Environment.TickCount > 1)
                                                     {
                                                         return 2;
                                                     }
                                                 }

                                                 return 0;
                                             })
                                  {
                                  }
                              }
                              """;

        await VerifyComplexity(source, Limit(1));
    }

    #endregion // Tests — lambdas and local functions (RH2009-005)

    #region Tests — location (RH2009-006)

    /// <summary>
    /// Verifies the example of RH2009-006 in a build that defines <c>FEATURE</c>: the indexer on <c>this</c>, the
    /// conversion operator on <c>operator</c>, and the finalizer on its identifier, 3 each
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2009-006")]
    public async Task MembersWithoutIdentifierAreReportedOnTheirNamingToken()
    {
        await VerifyComplexity(LocationExample(true), Limit(1), DefineSymbols("FEATURE"), Complexity(0, 3, 1), Complexity(1, 3, 1), Complexity(2, 3, 1));
    }

    /// <summary>
    /// Verifies the example of RH2009-006 in a build that does not define <c>FEATURE</c>: the conversion operator has 1
    /// and no nesting increment and is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2009-006")]
    [Requirement("RH2009-004")]
    public async Task ExcludedCodeCountsAsAbsent()
    {
        await VerifyComplexity(LocationExample(false), Limit(1), Complexity(0, 3, 1), Complexity(2, 3, 1));
    }

    /// <summary>
    /// Verifies that a verbatim identifier locates the diagnostic on the whole identifier token
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2009-006")]
    public async Task VerbatimIdentifierIsReportedOnTheWholeToken()
    {
        const string source = """
                              class C
                              {
                                  void {|#0:@class|}(bool a, bool b)
                                  {
                                      if (a)
                                      {
                                          if (b)
                                          {
                                              System.Console.WriteLine();
                                          }
                                      }
                                  }
                              }
                              """;

        await VerifyComplexity(source, Limit(1), Complexity(0, 3, 1));
    }

    #endregion // Tests — location (RH2009-006)

    #region Methods

    /// <summary>
    /// Creates the expected diagnostic at a markup location with the message that states the complexity and the limit
    /// </summary>
    /// <param name="markupKey">Markup key of the location</param>
    /// <param name="complexity">Cognitive complexity of the member's body</param>
    /// <param name="limit">Limit that applies</param>
    /// <returns>Expected diagnostic</returns>
    private static DiagnosticResult Complexity(int markupKey, int complexity, int limit)
    {
        return Diagnostic(RH2009MethodsShouldNotExceedTheComplexityLimitAnalyzer.DiagnosticId).WithLocation(markupKey, DiagnosticLocationOptions.InterpretAsMarkupKey)
                                                                                              .WithMessage(string.Format(MessageFormat, complexity, limit));
    }

    /// <summary>
    /// Creates the text of a <c>reihitsu.json</c> that sets the complexity limit
    /// </summary>
    /// <param name="limit">Value of <c>design.maximumMethodComplexity</c></param>
    /// <returns>Text of <c>reihitsu.json</c></returns>
    private static string Limit(int limit)
    {
        return $$"""{ "design": { "maximumMethodComplexity": {{limit}} } }""";
    }

    /// <summary>
    /// Verifies the analyzer in a production project with the given configuration file
    /// </summary>
    /// <param name="source">Source with markup</param>
    /// <param name="configuration">Text of <c>reihitsu.json</c>, or <see langword="null"/> for no file</param>
    /// <param name="expected">Expected diagnostics</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private static async Task VerifyComplexity(string source, string configuration, params DiagnosticResult[] expected)
    {
        await VerifyComplexity(source, configuration, null, expected);
    }

    /// <summary>
    /// Verifies the analyzer with the given configuration file and further configuration of the test
    /// </summary>
    /// <param name="source">Source with markup</param>
    /// <param name="configuration">Text of <c>reihitsu.json</c>, or <see langword="null"/> for no file</param>
    /// <param name="onConfigure">Further configuration of the test, or <see langword="null"/> for none</param>
    /// <param name="expected">Expected diagnostics</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private static async Task VerifyComplexity(string source,
                                               string configuration,
                                               Action<CSharpAnalyzerVerifierTest<RH2009MethodsShouldNotExceedTheComplexityLimitAnalyzer>> onConfigure,
                                               params DiagnosticResult[] expected)
    {
        await Verify(source,
                     test =>
                     {
                         if (configuration != null)
                         {
                             test.TestState.AdditionalFiles.Add(("reihitsu.json", configuration));
                         }

                         onConfigure?.Invoke(test);
                     },
                     expected);
    }

    /// <summary>
    /// Adds a global analyzer config, as the build writes the MSBuild properties it makes visible
    /// </summary>
    /// <param name="test">Test</param>
    /// <param name="lines">Lines after <c>is_global = true</c></param>
    private static void AddGlobalConfiguration(CSharpAnalyzerVerifierTest<RH2009MethodsShouldNotExceedTheComplexityLimitAnalyzer> test, string lines)
    {
        test.TestState.AnalyzerConfigFiles.Add(("/.globalconfig", $"is_global = true\n{lines}\n"));
    }

    /// <summary>
    /// Allows unsafe code in the test project
    /// </summary>
    /// <param name="test">Test</param>
    private static void AllowUnsafe(CSharpAnalyzerVerifierTest<RH2009MethodsShouldNotExceedTheComplexityLimitAnalyzer> test)
    {
        test.SolutionTransforms.Add((solution, projectId) => solution.GetProject(projectId)?.CompilationOptions is CSharpCompilationOptions options
                                                                 ? solution.WithProjectCompilationOptions(projectId, options.WithAllowUnsafe(true))
                                                                 : solution);
    }

    /// <summary>
    /// Creates a configuration that defines the given preprocessor symbols for the test project
    /// </summary>
    /// <param name="symbols">Preprocessor symbols</param>
    /// <returns>Configuration of the test</returns>
    private static Action<CSharpAnalyzerVerifierTest<RH2009MethodsShouldNotExceedTheComplexityLimitAnalyzer>> DefineSymbols(params string[] symbols)
    {
        return test => test.SolutionTransforms.Add((solution, projectId) => solution.GetProject(projectId)?.ParseOptions is CSharpParseOptions parseOptions
                                                                                ? solution.WithProjectParseOptions(projectId, parseOptions.WithPreprocessorSymbols(symbols))
                                                                                : solution);
    }

    /// <summary>
    /// Removes the markup of every location from the source, so no diagnostic is expected there
    /// </summary>
    /// <param name="source">Source with markup</param>
    /// <returns>Source without markup</returns>
    private static string Unmarked(string source)
    {
        var builder = new StringBuilder(source);

        for (var key = 0; key < 10; key++)
        {
            builder.Replace($"{{|#{key}:", string.Empty);
        }

        return builder.Replace("|}", string.Empty).ToString();
    }

    /// <summary>
    /// Creates a method with the given number of <c>if</c> statements one after another; with
    /// <paramref name="nestLast"/>, the last <c>if</c> holds a nested <c>if</c> and the method is marked with the
    /// markup key 0
    /// </summary>
    /// <param name="count">Number of <c>if</c> statements</param>
    /// <param name="nestLast">Whether the last <c>if</c> holds a nested <c>if</c></param>
    /// <returns>Source</returns>
    private static string CreateIfSequence(int count, bool nestLast)
    {
        var builder = new StringBuilder();

        builder.Append("class C\n{\n    int ")
               .Append(nestLast ? "{|#0:M|}" : "M")
               .Append("(int value)\n    {\n");

        for (var index = 0; index < count; index++)
        {
            builder.Append("        if (value == ").Append(index).Append(")\n        {\n");

            if (nestLast && index == count - 1)
            {
                builder.Append("            if (value > 0)\n            {\n                value++;\n            }\n");
            }
            else
            {
                builder.Append("            value++;\n");
            }

            builder.Append("        }\n\n");
        }

        builder.Append("        return value;\n    }\n}\n");

        return builder.ToString();
    }

    /// <summary>
    /// Creates a property whose <c>get</c> and <c>set</c> accessors have a cognitive complexity of 3 each
    /// </summary>
    /// <param name="isMarked">Whether the accessor keywords are marked with the markup keys 0 and 1</param>
    /// <returns>Source</returns>
    private static string CreatePropertyWithTwoAccessors(bool isMarked)
    {
        var source = """
                     class C
                     {
                         private int _value;

                         public int Value
                         {
                             {|#0:get|}
                             {
                                 if (_value > 0)
                                 {
                                     if (_value > 1)
                                     {
                                         return 2;
                                     }
                                 }

                                 return 0;
                             }

                             {|#1:set|}
                             {
                                 if (value > 0)
                                 {
                                     if (value > 1)
                                     {
                                         _value = value;
                                     }
                                 }
                             }
                         }
                     }
                     """;

        return isMarked
                   ? source
                   : Unmarked(source);
    }

    /// <summary>
    /// Creates the first example of RH2009-003: <c>M</c> nests three <c>if</c> statements (6), <c>N</c> holds three in
    /// a row (3)
    /// </summary>
    /// <returns>Source with markup</returns>
    private static string NestedAndSequentialIfs()
    {
        return """
               class C
               {
                   void {|#0:M|}(int a)
                   {
                       if (a > 0)
                       {
                           if (a > 1)
                           {
                               if (a > 2)
                               {
                                   a--;
                               }
                           }
                       }
                   }

                   void N(int a)
                   {
                       if (a > 0)
                       {
                           a--;
                       }

                       if (a > 1)
                       {
                           a--;
                       }

                       if (a > 2)
                       {
                           a--;
                       }
                   }
               }
               """;
    }

    /// <summary>
    /// Creates the second example of RH2009-003, whose method has a cognitive complexity of 11
    /// </summary>
    /// <returns>Source with markup</returns>
    private static string ElevenExample()
    {
        return """
               using System;

               class C
               {
                   int {|#0:M|}(int[] values, string text)
                   {
                       var total = 0;

                       foreach (var value in values)
                       {
                           if (value > 0 && value < 10)
                           {
                               total += value;
                           }
                           else if (value is 10 or 20)
                           {
                               total -= value;
                           }
                           else
                           {
                               total += value > 100 ? 1 : 2;
                           }
                       }

                       try
                       {
                           total = checked(total * 2);
                       }
                       catch (OverflowException)
                       {
                           total = text?.Length ?? 0;
                       }

                       return total;
                   }
               }
               """;
    }

    /// <summary>
    /// Creates the example of RH2009-003 with <c>new</c> of its own type and a compound assignment: the constructor of
    /// <c>Node</c> and <c>All</c> have 3 each
    /// </summary>
    /// <returns>Source with markup</returns>
    private static string NodeExample()
    {
        return """
               class Node
               {
                   public Node Child;

                   public {|#0:Node|}(int depth)
                   {
                       if (depth > 0)
                       {
                           if (depth % 2 == 0)
                           {
                               Child = new Node(depth - 1);
                           }
                       }
                   }

                   bool {|#1:All|}(bool[] checks, bool strict)
                   {
                       var ok = true;

                       if (strict)
                       {
                           foreach (var check in checks)
                           {
                               ok &= check;
                           }
                       }

                       return ok;
                   }
               }
               """;
    }

    /// <summary>
    /// Creates the example of RH2009-003 with <c>lock</c>, <c>using</c>, <c>try</c>, and <c>finally</c>, whose method
    /// has a cognitive complexity of 3
    /// </summary>
    /// <returns>Source with markup</returns>
    private static string LockUsingTryExample()
    {
        return """
               using System;

               class C
               {
                   private readonly object _gate = new object();

                   void {|#0:M|}(IDisposable resource, object value)
                   {
                       if (value != null)
                       {
                           lock (_gate)
                           {
                               using (resource)
                               {
                                   try
                                   {
                                       if (value is int number)
                                       {
                                           throw new InvalidOperationException(number.ToString() ?? string.Empty);
                                       }
                                   }
                                   finally
                                   {
                                       resource.Dispose();
                                   }
                               }
                           }
                       }
                   }
               }
               """;
    }

    /// <summary>
    /// Creates a method whose only nested <c>if</c> stands in an <c>#if FEATURE</c> group inside an <c>if</c>: 3 in a
    /// build that defines <c>FEATURE</c>, 1 otherwise
    /// </summary>
    /// <returns>Source with markup</returns>
    private static string DirectiveExample()
    {
        return """
               class C
               {
                   void {|#0:M|}(bool a, bool b)
                   {
                       if (a)
                       {
               #if FEATURE
                           if (b)
                           {
                               System.Console.WriteLine();
                           }
               #endif
                       }
                   }
               }
               """;
    }

    /// <summary>
    /// Creates the first example of RH2009-004, a body with a cognitive complexity of 9 and no nesting increment; with
    /// <paramref name="moveConditional"/>, the conditional expression stands in the block of the second <c>if</c>,
    /// where it scores 2, and the method is marked with the markup key 0
    /// </summary>
    /// <param name="moveConditional">Whether the conditional expression is moved into the block of the second <c>if</c></param>
    /// <returns>Source</returns>
    private static string FlatExample(bool moveConditional)
    {
        var conditional = "value = value > 5 || value < 0 ? 0 : 1;";
        var source = """
                     class C
                     {
                         int NAME(int value)
                         {
                             if (value == 1)
                             {
                                 return 10;
                             }
                             else if (value == 2 && value > 0)
                             {
                                 return 20;
                             }
                             else
                             {
                                 value++;
                             }

                             for (var i = 0; i < value; i++)
                             {
                                 value--;
                             }

                             OUTER

                             if (value > 4)
                             {
                                 value--;
                                 INNER
                             }

                             switch (value)
                             {
                                 case 3:
                                     return 30;

                                 default:
                                     return value;
                             }
                         }
                     }
                     """;

        return moveConditional
                   ? source.Replace("NAME", "{|#0:M|}").Replace("OUTER", string.Empty).Replace("INNER", conditional)
                   : source.Replace("NAME", "M").Replace("OUTER", conditional).Replace("INNER", string.Empty);
    }

    /// <summary>
    /// Creates the first example of RH2009-005 with the given head of the function passed to <c>ForEach</c>
    /// </summary>
    /// <param name="functionHead">Head of the lambda or anonymous method</param>
    /// <returns>Source with markup</returns>
    private static string LambdaExample(string functionHead)
    {
        return $$"""
                 using System;
                 using System.Collections.Generic;

                 class C
                 {
                     void {|#0:M|}(List<int> values, bool enabled)
                     {
                         if (enabled)
                         {
                             values.ForEach({{functionHead}}
                             {
                                 if (value > 0)
                                 {
                                     Console.WriteLine(value);
                                 }
                             });
                         }
                     }
                 }
                 """;
    }

    /// <summary>
    /// Creates the second example of RH2009-005: <c>M</c> has 1, its local function <c>Local</c> has 3
    /// </summary>
    /// <returns>Source with markup</returns>
    private static string LocalFunctionExample()
    {
        return """
               class C
               {
                   int M(int value)
                   {
                       if (value > 0)
                       {
                           return Local(value);
                       }

                       return 0;

                       int {|#0:Local|}(int input)
                       {
                           foreach (var digit in input.ToString())
                           {
                               if (digit == '0')
                               {
                                   return 0;
                               }
                           }

                           return input;
                       }
                   }
               }
               """;
    }

    /// <summary>
    /// Creates the example of RH2009-006; without <paramref name="markOperator"/>, the conversion operator is not
    /// marked
    /// </summary>
    /// <param name="markOperator">Whether the <c>operator</c> keyword of the conversion operator is marked with the markup key 1</param>
    /// <returns>Source with markup</returns>
    private static string LocationExample(bool markOperator)
    {
        var source = """
                     class C
                     {
                         private int _value;

                         public int {|#0:this|}[int index] => index > 0 ? (index > 1 ? 2 : 1) : 0;

                         public static explicit {|#1:operator|} int(C value)
                         {
                             if (value._value > 0)
                             {
                     #if FEATURE
                                 if (value._value > 1)
                                 {
                                     return 2;
                                 }
                     #endif
                                 return 1;
                             }

                             return 0;
                         }

                         ~{|#2:C|}()
                         {
                             while (_value > 0)
                             {
                                 if (_value > 1)
                                 {
                                     _value--;
                                 }

                                 _value--;
                             }
                         }
                     }
                     """;

        return markOperator
                   ? source
                   : source.Replace("{|#1:operator|}", "operator");
    }

    #endregion // Methods
}