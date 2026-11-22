using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Organization;
using Reihitsu.Analyzer.Rules.Organization;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Ordering;

/// <summary>
/// Test methods for <see cref="RH7109ReadonlyElementsMustAppearBeforeNonReadonlyElementsCodeFixProvider"/> on conditional directives in, in front of, and behind the moved and the crossed fields
/// </summary>
[TestClass]
public class RH7109ReadonlyElementsMustAppearBeforeNonReadonlyElementsExcludedCodeTests : BatchCodeFixTestsBase<RH7109ReadonlyElementsMustAppearBeforeNonReadonlyElementsAnalyzer, RH7109ReadonlyElementsMustAppearBeforeNonReadonlyElementsCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying that the moved static field crosses the instance field of a struct but reorders no instance field, so
    /// the fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-003")]
    public async Task FixMovesAStaticFieldOfAStructAcrossAnInstanceField()
    {
        const string testCode = """
                                public struct S
                                {
                                    private static int _s;
                                    private int _x;
                                    private static readonly int {|RH7109:_r|};
                                }
                                """;

        const string fixedCode = """
                                 public struct S
                                 {
                                     private static readonly int _r;
                                     private static int _s;
                                     private int _x;
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifying that a conditional group of a whole attribute list in front of the moved field lies in the moved span,
    /// so no code fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task NoCodeFixWhenAGroupOfAWholeAttributeListStandsInFrontOfTheMovedField(string endOfLine)
    {
        const string source = """
                              using System;

                              public class C
                              {
                                  private int _a = Log("a");
                              #if DEBUG
                                  [Obsolete("x")]
                              #endif
                                  private readonly int {|RH7109:_b|};

                                  private static int Log(string name)
                                  {
                                      return 0;
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying that a conditional group between two entries of a crossed field's collection initializer lies in the
    /// crossed span, so no code fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task NoCodeFixWhenAGroupStandsInTheCollectionInitializerOfACrossedField(string endOfLine)
    {
        const string source = """
                              using System.Collections.Generic;

                              public class C
                              {
                                  private int _a;
                                  private static List<int> _b = new List<int>
                                                                {
                                                                    1,
                              #if DEBUG
                                                                    2,
                              #endif
                                                                };
                                  private readonly int {|RH7109:_c|} = Log("c");

                                  private static int Log(string name)
                                  {
                                      return 0;
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying that a conditional group behind the moved field, whose branches each close the type, lies outside both
    /// spans and is neither read nor refuses the move, so the fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task FixMovesAFieldWhenAConditionalGroupOfTheBodyClosesBehindTheClosingBraceOfTheType(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private int _a;
                                  private readonly int {|RH7109:_b|};
                              #if DEBUG
                                  private int _d;
                              }
                              #else
                              }
                              #endif
                              """;

        const string fixedSource = """
                                   public class C
                                   {
                                       private readonly int _b;
                                       private int _a;
                                   #if DEBUG
                                       private int _d;
                                   }
                                   #else
                                   }
                                   #endif
                                   """;

        await Verify(WithEndOfLine(source, endOfLine), WithEndOfLine(fixedSource, endOfLine));
    }

    /// <summary>
    /// Verifying the counterpart of a conditional group outside both spans: the move crosses an instance initialized field,
    /// so no code fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-007")]
    [Requirement("RH7109-006")]
    public async Task NoCodeFixWhenAConditionalGroupOfTheBodyClosesBehindTheClosingBraceOfTheTypeAndAnInitializedFieldIsCrossed(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private int _a = Log("a");
                                  private readonly int {|RH7109:_b|} = Log("b");

                                  private static int Log(string name)
                                  {
                                      return 0;
                                  }
                              #if DEBUG
                                  private int _d;
                              }
                              #else
                              }
                              #endif
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying that a conditional group between two arguments in the expression body of a crossed property lies in the
    /// crossed span, so no code fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task NoCodeFixWhenAGroupStandsInTheExpressionBodyOfACrossedProperty(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private static int _t;
                                  public int P => Run(1
                              #if DEBUG
                                                      , Log("z")
                              #endif
                                                      );
                                  private static readonly int {|RH7109:_c|} = Log("c");

                                  private static int Run(int a, int b = 0)
                                  {
                                      return a + b;
                                  }

                                  private static int Log(string name)
                                  {
                                      return 0;
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying the SHARED-FIX-009 example: without <c>DEBUG</c> defined, whether the crossed <c>_b</c> has an initializer
    /// depends on the build, and the fix does not work it out, so no code fix is offered although no build would change
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-FIX-009")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task NoCodeFixWhenAnIfCompletesTheInitializerOfACrossedField(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private int _a;
                                  private int _b
                              #if DEBUG
                                      = 1
                              #endif
                                      ;
                                  private readonly int {|RH7109:_c|};
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying the counterpart of the SHARED-FIX-009 example: without the conditional group, the fix moves <c>_c</c> in
    /// front of <c>_a</c>
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-FIX-009")]
    [Requirement("RH7109-005")]
    public async Task FixMovesTheFieldOfTheSharedFixExampleWithoutTheConditionalGroup(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private int _a;
                                  private int _b;
                                  private readonly int {|RH7109:_c|};
                              }
                              """;

        const string fixedSource = """
                                   public class C
                                   {
                                       private readonly int _c;
                                       private int _a;
                                       private int _b;
                                   }
                                   """;

        await Verify(WithEndOfLine(source, endOfLine), WithEndOfLine(fixedSource, endOfLine));
    }

    /// <summary>
    /// Verifying that a group of whole statements in a lambda inside a crossed field initializer lies in the crossed span,
    /// so no code fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task NoCodeFixWhenAGroupOfStatementsStandsInALambdaOfACrossedFieldInitializer(string endOfLine)
    {
        const string source = """
                              using System;

                              public class C
                              {
                                  private Action _a = () =>
                                  {
                              #if DEBUG
                                      Console.WriteLine();
                              #endif
                                  };

                                  private readonly int {|RH7109:_c|};
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying the SHARED-ORDER-008 place <i>In the reordered body, in front of the spans</i> for a group whose
    /// <c>#if</c> stands in front of the spans and whose <c>#endif</c> stands behind them, so that the spans lie in one of
    /// its branches: the type counts as one whose field order sets its layout, and no code fix is offered for a move that
    /// reorders instance fields
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task NoCodeFixWhenTheSpansLieInOneBranchOfAGroupThatOpensInTheBodyInFrontOfThem(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                              #if !FEATURE
                                  private readonly int _z;
                                  private int _a;
                                  private int _x;
                                  private readonly int {|RH7109:_c|};
                              #endif
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    #endregion // Tests

    #region Methods

    /// <summary>
    /// Writes the provided text with the given line break
    /// </summary>
    /// <param name="text">Text</param>
    /// <param name="endOfLine">Line break</param>
    /// <returns>The text with every line break replaced by <paramref name="endOfLine"/></returns>
    private static string WithEndOfLine(string text, string endOfLine)
    {
        return text.Replace("\r\n", "\n").Replace("\n", endOfLine);
    }

    #endregion // Methods

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                public class TestClass
                                {
                                    private int _value;
                                    private readonly int {|#0:_first|};
                                    private readonly int {|#1:_second|};
                                }
                                """;

        const string fixedCode = """
                                 public class TestClass
                                 {
                                     private readonly int _first;
                                     private readonly int _second;
                                     private int _value;
                                 }
                                 """;

        // Both readonly fields are moved in front of the same mutable field. The mutable field carries no
        // initializer, so neither move changes initializer execution order and both fixes stay available
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH7109ReadonlyElementsMustAppearBeforeNonReadonlyElementsAnalyzer.DiagnosticId, MessageFormat, 2),
                                  static config => config.NumberOfFixAllIterations = 1);
    }

    #endregion // BatchCodeFixTestsBase
}