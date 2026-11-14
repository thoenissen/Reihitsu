using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Clarity;
using Reihitsu.Analyzer.Rules.Clarity;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH3203ExpressionStyleConstructorsShouldNotBeUsedAnalyzer"/>
/// </summary>
[TestClass]
public class RH3203ExpressionStyleConstructorsShouldNotBeUsedAnalyzerTests : BatchCodeFixTestsBase<RH3203ExpressionStyleConstructorsShouldNotBeUsedAnalyzer, RH3203ExpressionStyleConstructorsShouldNotBeUsedCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying that an expression-bodied constructor is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3203-001")]
    [Requirement("SHARED-BODY-002")]
    [Requirement("SHARED-BODY-003")]
    public async Task VerifyExpressionBodiedConstructorsAreDetectedAndFixed()
    {
        const string testData = """
                                internal class RH3203
                                {
                                    public RH3203() {|#0:=> System.Console.WriteLine()|};
                                }
                                """;
        const string fixedData = """
                                 internal class RH3203
                                 {
                                     public RH3203()
                                     {
                                         System.Console.WriteLine();
                                     }
                                 }
                                 """;

        await Verify(testData.Replace("\r\n", "\n"),
                     fixedData.Replace("\r\n", "\n"),
                     Diagnostics(RH3203ExpressionStyleConstructorsShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH3203MessageFormat));
    }

    /// <summary>
    /// Verifying that multiple expression-bodied constructors are detected
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3203-001")]
    [Requirement("SHARED-BODY-002")]
    public async Task VerifyMultipleExpressionBodiedConstructorsAreDetected()
    {
        const string testData = """
                                internal class RH3203
                                {
                                    public RH3203() {|#0:=> System.Console.WriteLine()|};
                                    public RH3203(int i) {|#1:=> System.Console.WriteLine(i)|};
                                }

                                internal class RH3203Expression
                                {
                                    public RH3203Expression()
                                    {
                                        System.Console.WriteLine();
                                    }

                                    public RH3203Expression(int i)
                                    {
                                        System.Console.WriteLine(i);
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH3203ExpressionStyleConstructorsShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH3203MessageFormat, 2));
    }

    /// <summary>
    /// Verifies that a constructor whose expression body carries a directive before the expression is not
    /// reported, because the formatter refuses to rewrite it and the code fix could therefore not converge
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BODY-001")]
    public async Task VerifyExpressionBodyWithDirectiveBeforeExpressionIsNotReported()
    {
        const string testData = """
                                internal class RH3203
                                {
                                    private int _value;

                                    public RH3203() =>
                                #pragma warning disable CS0618
                                        _value = 1;
                                #pragma warning restore CS0618
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a constructor whose expression body carries a directive after the expression is still
    /// reported and fixed, because that directive travels into the generated statement
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BODY-001")]
    [Requirement("SHARED-BODY-003")]
    public async Task VerifyExpressionBodyWithDirectiveAfterExpressionIsFixed()
    {
        const string testData = """
                                internal class RH3203
                                {
                                    private int _value;

                                    public RH3203() {|#0:=> _value = 1|}
                                #pragma warning disable CS0168
                                        ;
                                }
                                """;

        const string resultData = """
                                  internal class RH3203
                                  {
                                      private int _value;

                                      public RH3203()
                                      {
                                          _value = 1
                                  #pragma warning disable CS0168
                                          ;
                                      }
                                  }
                                  """;

        await Verify(testData.Replace("\r\n", "\n"),
                     resultData.Replace("\r\n", "\n"),
                     Diagnostics(RH3203ExpressionStyleConstructorsShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH3203MessageFormat));
    }

    /// <summary>
    /// Verifies expression-bodied static constructors and constructors with an initializer are reported, while a
    /// constructor with a block body is not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3203-001")]
    public async Task StaticAndInitializedExpressionBodiedConstructorsAreReported()
    {
        const string testData = """
                                class B
                                {
                                    public B(int v)
                                    {
                                    }
                                }

                                class C : B
                                {
                                    private int _value;
                                    private static int s_count;

                                    static C() {|RH3203:=> s_count = 1|};

                                    public C(int value) : base(value) {|RH3203:=> _value = value|};

                                    public C() : this(0)
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies a constructor with a conditional directive between <c>=&gt;</c> and the expression is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BODY-001")]
    public async Task VerifyExpressionBodyWithConditionalBeforeExpressionIsNotReported()
    {
        const string testData = """
                                class C
                                {
                                    private int _value;

                                    public C(int value) =>
                                #if DEBUG
                                        _value = value;
                                #else
                                        _value = 0;
                                #endif
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies a constructor whose semicolon a region splits from the expression is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BODY-001")]
    public async Task VerifyExpressionBodyWithRegionAroundSemicolonIsNotReported()
    {
        const string testData = """
                                class C
                                {
                                    private int _value;

                                    public C(int value) => _value = value
                                    #region R
                                        ;
                                    #endregion
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies a balanced conditional group between the expression and the semicolon does not block the conversion;
    /// with the symbol undefined, the group's code is disabled text and the expression ends before it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BODY-001")]
    [Requirement("SHARED-BODY-003")]
    public async Task VerifyExpressionBodyWithBalancedGroupBeforeSemicolonIsFixed()
    {
        const string testData = """
                                class C
                                {
                                    private int _value;

                                    public C(int value) {|RH3203:=> _value = value|}
                                #if DEBUG
                                        + 1
                                #endif
                                        ;
                                }
                                """;

        const string resultData = """
                                  class C
                                  {
                                      private int _value;

                                      public C(int value)
                                      {
                                          _value = value
                                  #if DEBUG
                                          + 1
                                  #endif
                                          ;
                                      }
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies a static constructor and a throw-expression-bodied constructor are converted to the block bodies the
    /// formatter produces
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BODY-002")]
    [Requirement("SHARED-BODY-003")]
    public async Task VerifyStaticAndThrowingConstructorsAreConverted()
    {
        const string testData = """
                                using System;

                                class C
                                {
                                    private static int s_count;

                                    static C() {|RH3203:=> s_count = 1|};

                                    public C(string text) {|RH3203:=> throw new ArgumentException(text)|};
                                }
                                """;

        const string resultData = """
                                  using System;

                                  class C
                                  {
                                      private static int s_count;

                                      static C()
                                      {
                                          s_count = 1;
                                      }

                                      public C(string text)
                                      {
                                          throw new ArgumentException(text);
                                      }
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies the specified example: the fix replaces only the expression body, and keeps the documentation comment,
    /// the attribute on the constructor's line, and the spacing of the signature as written, under LF and CRLF
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-FIX-006")]
    [Requirement("SHARED-BODY-003")]
    public async Task VerifyFixFormatsOnlyTheReplacedBody(string endOfLine)
    {
        const string testData = """
                                using System;

                                class C
                                {
                                    private int _value;

                                    ///<summary>x</summary>
                                    [Obsolete] public  C( int value ) {|RH3203:=> _value = value|};
                                }
                                """;

        const string resultData = """
                                  using System;

                                  class C
                                  {
                                      private int _value;

                                      ///<summary>x</summary>
                                      [Obsolete] public  C( int value )
                                      {
                                          _value = value;
                                      }
                                  }
                                  """;

        await VerifyCodeFixWithEndOfLine(testData, resultData, endOfLine);
    }

    /// <summary>
    /// Verifies that the fix keeps a constructor initializer on the signature's line where it stands
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BODY-003")]
    public async Task VerifyFixKeepsTheConstructorInitializerOnTheSignaturesLine()
    {
        const string testData = """
                                using System;

                                class C
                                {
                                    public C(int value)
                                    {
                                    }

                                    public C(string s) : this(s.Length) {|RH3203:=> Console.WriteLine(s)|};
                                }
                                """;

        const string resultData = """
                                  using System;

                                  class C
                                  {
                                      public C(int value)
                                      {
                                      }

                                      public C(string s) : this(s.Length)
                                      {
                                          Console.WriteLine(s);
                                      }
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies that the fix keeps the spacing of a constructor initializer and of the signature in front of it as written
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BODY-003")]
    public async Task VerifyFixKeepsTheSpacingOfTheConstructorInitializer()
    {
        const string testData = """
                                class B
                                {
                                    public B(int value)
                                    {
                                    }
                                }

                                class C : B
                                {
                                    private int _value;

                                    public C( int value ) :  base( value ) {|RH3203:=> _value = value|};
                                }
                                """;

        const string resultData = """
                                  class B
                                  {
                                      public B(int value)
                                      {
                                      }
                                  }

                                  class C : B
                                  {
                                      private int _value;

                                      public C( int value ) :  base( value )
                                      {
                                          _value = value;
                                      }
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies that the fix keeps a constructor initializer on the line below the signature where it stands
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BODY-003")]
    public async Task VerifyFixKeepsAConstructorInitializerOnTheLineBelowTheSignature()
    {
        const string testData = """
                                class C
                                {
                                    public C(int value)
                                    {
                                    }

                                    public C( string s )
                                        : this( s.Length ) {|RH3203:=> s.ToString()|};
                                }
                                """;

        const string resultData = """
                                  class C
                                  {
                                      public C(int value)
                                      {
                                      }

                                      public C( string s )
                                          : this( s.Length )
                                      {
                                          s.ToString();
                                      }
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies that the fix keeps the two blank lines above the converted constructor
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-006")]
    public async Task VerifyFixKeepsTwoBlankLinesAboveTheConstructor()
    {
        const string testData = """
                                class C
                                {
                                    private int _value;


                                    public C() {|RH3203:=> _value = 1|};
                                }
                                """;

        const string resultData = """
                                  class C
                                  {
                                      private int _value;


                                      public C()
                                      {
                                          _value = 1;
                                      }
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies that Fix All converts two constructors of one type and keeps the spacing of both signatures
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BODY-003")]
    public async Task VerifyFixAllKeepsTheSignaturesOfTwoConstructors()
    {
        const string testData = """
                                class C
                                {
                                    private int _value;

                                    public  C( ) {|RH3203:=> _value = 1|};

                                    public C(  int value ) {|RH3203:=> _value = value|};
                                }
                                """;

        const string resultData = """
                                  class C
                                  {
                                      private int _value;

                                      public  C( )
                                      {
                                          _value = 1;
                                      }

                                      public C(  int value )
                                      {
                                          _value = value;
                                      }
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testData = """
                                internal class RH3203
                                {
                                    private int _value;

                                    public RH3203() {|#0:=> _value = 1|};

                                    public RH3203(int value) {|#1:=> _value = value|};
                                }
                                """;

        const string resultData = """
                                  internal class RH3203
                                  {
                                      private int _value;

                                      public RH3203()
                                      {
                                          _value = 1;
                                      }

                                      public RH3203(int value)
                                      {
                                          _value = value;
                                      }
                                  }
                                  """;

        // Verifies two expression-bodied constructors are fixed in one Fix All iteration
        return new FixAllScenario(testData.Replace("\r\n", "\n"),
                                  resultData.Replace("\r\n", "\n"),
                                  Diagnostics(RH3203ExpressionStyleConstructorsShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH3203MessageFormat, 2),
                                  static config => config.NumberOfFixAllIterations = 1);
    }

    #endregion // BatchCodeFixTestsBase
}