using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Clarity;
using Reihitsu.Analyzer.Rules.Clarity;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Clarity;

/// <summary>
/// Test methods for <see cref="RH3003UseStringEmptyForEmptyStringsAnalyzer"/> and <see cref="RH3003UseStringEmptyForEmptyStringsCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH3003UseStringEmptyForEmptyStringsAnalyzerTests : BatchCodeFixTestsBase<RH3003UseStringEmptyForEmptyStringsAnalyzer, RH3003UseStringEmptyForEmptyStringsCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying empty string literal in return statement is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3003-001")]
    [Requirement("RH3003-003")]
    [Requirement("RH3003-004")]
    public async Task EmptyStringInReturnIsReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    public string Run()
                                    {
                                        return {|#0:""|};
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public string Run()
                                     {
                                         return string.Empty;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3003UseStringEmptyForEmptyStringsAnalyzer.DiagnosticId, "Use string.Empty for empty strings."));
    }

    /// <summary>
    /// Verifying empty string literal in variable assignment is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3003-001")]
    [Requirement("RH3003-003")]
    [Requirement("RH3003-004")]
    public async Task EmptyStringInVariableAssignmentIsReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    public void Run()
                                    {
                                        string text = {|#0:""|};
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public void Run()
                                     {
                                         string text = string.Empty;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3003UseStringEmptyForEmptyStringsAnalyzer.DiagnosticId, "Use string.Empty for empty strings."));
    }

    /// <summary>
    /// Verifying empty string literal in method argument is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3003-001")]
    [Requirement("RH3003-003")]
    [Requirement("RH3003-004")]
    public async Task EmptyStringInMethodArgumentIsReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    public void Method(string value)
                                    {
                                    }

                                    public void Run()
                                    {
                                        Method({|#0:""|});
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public void Method(string value)
                                     {
                                     }

                                     public void Run()
                                     {
                                         Method(string.Empty);
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3003UseStringEmptyForEmptyStringsAnalyzer.DiagnosticId, "Use string.Empty for empty strings."));
    }

    /// <summary>
    /// Verifying empty string in field initializer is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3003-001")]
    [Requirement("RH3003-003")]
    [Requirement("RH3003-004")]
    public async Task EmptyStringInFieldInitializerIsReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    private string _field = {|#0:""|};
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private string _field = string.Empty;
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3003UseStringEmptyForEmptyStringsAnalyzer.DiagnosticId, "Use string.Empty for empty strings."));
    }

    /// <summary>
    /// Verifying empty string in property initializer is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3003-001")]
    [Requirement("RH3003-003")]
    [Requirement("RH3003-004")]
    public async Task EmptyStringInPropertyInitializerIsReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    public string Value { get; set; } = {|#0:""|};
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public string Value { get; set; } = string.Empty;
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3003UseStringEmptyForEmptyStringsAnalyzer.DiagnosticId, "Use string.Empty for empty strings."));
    }

    /// <summary>
    /// Verifying empty string in const field is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3003-002")]
    [Requirement("SHARED-DIAG-011")]
    public async Task EmptyStringInConstFieldIsNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    private const string Empty = "";
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying empty string in const local variable is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3003-002")]
    public async Task EmptyStringInConstLocalIsNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    public void Run()
                                    {
                                        const string empty = "";
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying empty strings nested in const initializers are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3003-002")]
    public async Task EmptyStringInNestedConstInitializerIsNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    private const string First = true ? "" : "value";
                                    private const string Second = "" + First;

                                    public void Run()
                                    {
                                        const string third = false ? "value" : "";
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying empty string in parameter default value is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3003-002")]
    public async Task EmptyStringInParameterDefaultIsNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    public void Method(string value = "")
                                    {
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying empty string nested in a parameter default value is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3003-002")]
    public async Task EmptyStringInNestedParameterDefaultIsNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    public void Method(string value = true ? "" : "value")
                                    {
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying empty string in attribute argument is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3003-002")]
    public async Task EmptyStringInAttributeArgumentIsNotReported()
    {
        const string testCode = """
                                using System.ComponentModel;

                                public class Test
                                {
                                    [Description("")]
                                    public void Method()
                                    {
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying empty string in switch case is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3003-002")]
    public async Task EmptyStringInSwitchCaseIsNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    public int Run(string value)
                                    {
                                        switch (value)
                                        {
                                            case "":
                                                return 1;
                                            default:
                                                return 0;
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying empty string in pattern matching is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3003-002")]
    public async Task EmptyStringInPatternMatchingIsNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    public bool Run(string value)
                                    {
                                        return value is "";
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying empty string in string concatenation is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3003-001")]
    [Requirement("RH3003-003")]
    [Requirement("RH3003-004")]
    public async Task EmptyStringInConcatenationIsReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    public string Run()
                                    {
                                        return "Hello" + {|#0:""|};
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public string Run()
                                     {
                                         return "Hello" + string.Empty;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3003UseStringEmptyForEmptyStringsAnalyzer.DiagnosticId, "Use string.Empty for empty strings."));
    }

    /// <summary>
    /// Verifying empty string in a goto case statement is not reported, because the case requires a constant and <c>string.Empty</c> would not compile
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3003-002")]
    public async Task EmptyStringInGotoCaseIsNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    public int Run(string value)
                                    {
                                        switch (value)
                                        {
                                            case "a":
                                                goto case "";
                                            case "":
                                                return 1;
                                            default:
                                                return 0;
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies regular and verbatim empty literals in a field initializer, an assignment, an argument, and a
    /// concatenation are reported on the literal and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3003-001")]
    [Requirement("RH3003-003")]
    public async Task RegularAndVerbatimEmptyLiteralsAreReportedOnTheLiteral()
    {
        const string testCode = """
                                class C
                                {
                                    string _f = {|RH3003:""|};

                                    string M(string s)
                                    {
                                        s = {|RH3003:@""|};
                                        M({|RH3003:""|});
                                        return {|RH3003:""|} + s;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     string _f = string.Empty;

                                     string M(string s)
                                     {
                                         s = string.Empty;
                                         M(string.Empty);
                                         return string.Empty + s;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies empty literals in every position that requires a constant are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3003-002")]
    [Requirement("SHARED-DIAG-011")]
    public async Task EmptyLiteralsWhereAConstantIsRequiredAreNotReported()
    {
        const string testCode = """
                                using System.ComponentModel;

                                class C
                                {
                                    const string F = "";

                                    [DefaultValue("")]
                                    void M(string s = "")
                                    {
                                        const string L = "";
                                        switch (s)
                                        {
                                            case "":
                                                goto case "x";
                                            case "x":
                                                break;
                                        }

                                        if (s is "")
                                        {
                                        }

                                        var b = s switch { "" => 1, _ => 2 };
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies the fix writes <c>string.Empty</c> and keeps the comments in front of and after the literal
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3003-004")]
    public async Task FixKeepsTheCommentsAroundTheLiteral()
    {
        const string testCode = """
                                class C
                                {
                                    string M(string s)
                                    {
                                        var a = {|RH3003:""|};
                                        return /* a */ {|RH3003:""|} /* b */ + s;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     string M(string s)
                                     {
                                         var a = string.Empty;
                                         return /* a */ string.Empty /* b */ + s;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Characterizes that an empty literal in the <c>when</c> clause of a <c>case</c> label is not reported, while
    /// the same literal in the <c>when</c> clause of a <c>switch</c> expression arm is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task EmptyLiteralInCaseLabelWhenClauseIsNotReported()
    {
        const string testCode = """
                                class C
                                {
                                    int M(string s)
                                    {
                                        switch (s)
                                        {
                                            case var x when x == "":
                                                return 1;
                                        }

                                        return s switch { var y when y == {|RH3003:""|} => 2, _ => 3 };
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     int M(string s)
                                     {
                                         switch (s)
                                         {
                                             case var x when x == "":
                                                 return 1;
                                         }

                                         return s switch { var y when y == string.Empty => 2, _ => 3 };
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies the specification example of a position that requires a constant: the empty literal that initializes
    /// a <c>const</c> field is not reported, because <c>string.Empty</c> is not a constant, while the empty literal
    /// that initializes an ordinary field is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3003-002")]
    [Requirement("SHARED-DIAG-011")]
    public async Task StringEmptyIsAskedForOnlyWhereNoConstantIsRequired()
    {
        const string testCode = """
                                class C
                                {
                                    const string F = "";
                                    string _g = {|RH3003:""|};
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     const string F = "";
                                     string _g = string.Empty;
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies the fix of a literal on the line after the operator, with a comment in front of it, keeps the line break in front of
    /// it, under LF and CRLF
    /// </summary>
    /// <param name="endOfLine">The line ending of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-FIX-006")]
    [Requirement("RH3003-004")]
    public async Task FixOfCommentedLiteralOnTheNextLineKeepsTheLineBreakInFrontOfIt(string endOfLine)
    {
        const string testCode = """
                                class C
                                {
                                    string M(string a, string b)
                                    {
                                        return a +
                                            /* x */ {|RH3003:""|};
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     string M(string a, string b)
                                     {
                                         return a +
                                             /* x */ string.Empty;
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    /// <summary>
    /// Verifies the fix of a literal behind a line that holds only a comment keeps the line break behind the operator and the one
    /// behind the comment, under LF and CRLF
    /// </summary>
    /// <param name="endOfLine">The line ending of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-FIX-006")]
    [Requirement("RH3003-004")]
    public async Task FixOfLiteralBehindACommentLineKeepsBothLineBreaksInFrontOfIt(string endOfLine)
    {
        const string testCode = """
                                class C
                                {
                                    string M(string a, string b)
                                    {
                                        return a +
                                            // c
                                            {|RH3003:""|};
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     string M(string a, string b)
                                     {
                                         return a +
                                             // c
                                             string.Empty;
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    /// <summary>
    /// Verifies the fix of a variable initializer on the line after <c>=</c>, with a comment in front of it, keeps the line break
    /// in front of it, under LF and CRLF
    /// </summary>
    /// <param name="endOfLine">The line ending of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-FIX-006")]
    [Requirement("RH3003-004")]
    public async Task FixOfCommentedInitializerOnTheNextLineKeepsTheLineBreakInFrontOfIt(string endOfLine)
    {
        const string testCode = """
                                class C
                                {
                                    string M(string a, string b)
                                    {
                                        var s =
                                            /* x */ {|RH3003:""|};
                                        return s + a + b;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     string M(string a, string b)
                                     {
                                         var s =
                                             /* x */ string.Empty;
                                         return s + a + b;
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    /// <summary>
    /// Verifies the fix of an argument on the line after the opening parenthesis, with a comment in front of it, keeps the line
    /// break in front of it, under LF and CRLF
    /// </summary>
    /// <param name="endOfLine">The line ending of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-FIX-006")]
    [Requirement("RH3003-004")]
    public async Task FixOfCommentedArgumentOnTheNextLineKeepsTheLineBreakInFrontOfIt(string endOfLine)
    {
        const string testCode = """
                                class C
                                {
                                    string M(string a, string b)
                                    {
                                        N(
                                            /* x */ {|RH3003:""|}, b);
                                        return a;
                                    }
                                    void N(string a, string b) { }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     string M(string a, string b)
                                     {
                                         N(
                                             /* x */ string.Empty, b);
                                         return a;
                                     }
                                     void N(string a, string b) { }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    /// <summary>
    /// Verifies the fix of a literal with a comment in front of it, on the line after an operator followed by a comment, keeps
    /// the line break in front of it, under LF and CRLF
    /// </summary>
    /// <param name="endOfLine">The line ending of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-FIX-006")]
    [Requirement("RH3003-004")]
    public async Task FixOfCommentedLiteralBehindALineEndingInACommentKeepsTheLineBreakInFrontOfIt(string endOfLine)
    {
        const string testCode = """
                                class C
                                {
                                    string M(string a, string b)
                                    {
                                        return a + // c
                                            /* x */ {|RH3003:""|};
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     string M(string a, string b)
                                     {
                                         return a + // c
                                             /* x */ string.Empty;
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    /// <summary>
    /// Verifies the fix of a literal on the line after the operator, without a comment, keeps the line break in front of it, under
    /// LF and CRLF
    /// </summary>
    /// <param name="endOfLine">The line ending of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-FIX-006")]
    [Requirement("RH3003-004")]
    public async Task FixOfLiteralOnTheNextLineKeepsTheLineBreakInFrontOfIt(string endOfLine)
    {
        const string testCode = """
                                class C
                                {
                                    string M(string a, string b)
                                    {
                                        return a +
                                            {|RH3003:""|};
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     string M(string a, string b)
                                     {
                                         return a +
                                             string.Empty;
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    /// <summary>
    /// Verifies the fix of a literal on the line after an operator followed by a delimited comment keeps the line break in front of
    /// it, under LF and CRLF
    /// </summary>
    /// <param name="endOfLine">The line ending of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-FIX-006")]
    [Requirement("RH3003-004")]
    public async Task FixOfLiteralBehindAnOperatorFollowedByACommentKeepsTheLineBreakInFrontOfIt(string endOfLine)
    {
        const string testCode = """
                                class C
                                {
                                    string M(string a, string b)
                                    {
                                        return a + /* x */
                                            {|RH3003:""|};
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     string M(string a, string b)
                                     {
                                         return a + /* x */
                                             string.Empty;
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    /// <summary>
    /// Verifies the fix of a literal with a comment in front of it, behind an operator that starts its line, keeps the line break
    /// in front of the operator, under LF and CRLF
    /// </summary>
    /// <param name="endOfLine">The line ending of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-FIX-006")]
    [Requirement("RH3003-004")]
    public async Task FixOfCommentedLiteralBehindAnOperatorThatStartsItsLineKeepsTheLineBreakInFrontOfTheOperator(string endOfLine)
    {
        const string testCode = """
                                class C
                                {
                                    string M(string a, string b)
                                    {
                                        return a
                                            + /* x */ {|RH3003:""|};
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     string M(string a, string b)
                                     {
                                         return a
                                             + /* x */ string.Empty;
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    /// <summary>
    /// Verifies the fix of a literal written directly behind <c>return</c>, without trivia between them, separates
    /// <c>string.Empty</c> from the keyword with one space, under LF and CRLF
    /// </summary>
    /// <param name="endOfLine">The line ending of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-FIX-003")]
    [Requirement("RH3003-004")]
    public async Task FixOfLiteralDirectlyBehindReturnSeparatesTheReplacementFromTheKeyword(string endOfLine)
    {
        const string testCode = """
                                class C
                                {
                                    string M()
                                    {
                                        return{|RH3003:""|};
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     string M()
                                     {
                                         return string.Empty;
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    /// <summary>
    /// Verifies the fix of a literal written directly behind <c>yield return</c>, without trivia between them, separates
    /// <c>string.Empty</c> from the keyword with one space, under LF and CRLF
    /// </summary>
    /// <param name="endOfLine">The line ending of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-FIX-003")]
    [Requirement("RH3003-004")]
    public async Task FixOfLiteralDirectlyBehindYieldReturnSeparatesTheReplacementFromTheKeyword(string endOfLine)
    {
        const string testCode = """
                                class C
                                {
                                    System.Collections.Generic.IEnumerable<string> M()
                                    {
                                        yield return{|RH3003:""|};
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     System.Collections.Generic.IEnumerable<string> M()
                                     {
                                         yield return string.Empty;
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    /// <summary>
    /// Verifies the fix of a literal written directly behind the <c>in</c> keyword of a <c>foreach</c>, without trivia between
    /// them, separates <c>string.Empty</c> from the keyword with one space, under LF and CRLF
    /// </summary>
    /// <param name="endOfLine">The line ending of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-FIX-003")]
    [Requirement("RH3003-004")]
    public async Task FixOfLiteralDirectlyBehindTheInKeywordOfAForEachSeparatesTheReplacementFromTheKeyword(string endOfLine)
    {
        const string testCode = """
                                class C
                                {
                                    void M()
                                    {
                                        foreach (var c in{|RH3003:""|})
                                        {
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     void M()
                                     {
                                         foreach (var c in string.Empty)
                                         {
                                         }
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    /// <summary>
    /// Verifies the fix of a literal written directly behind the <c>select</c> keyword of a query, without trivia between them,
    /// separates <c>string.Empty</c> from the keyword with one space, under LF and CRLF
    /// </summary>
    /// <param name="endOfLine">The line ending of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-FIX-003")]
    [Requirement("RH3003-004")]
    public async Task FixOfLiteralDirectlyBehindSelectSeparatesTheReplacementFromTheKeyword(string endOfLine)
    {
        const string testCode = """
                                using System.Linq;

                                class C
                                {
                                    object M(int[] a)
                                    {
                                        return from x in a select{|RH3003:""|};
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System.Linq;

                                 class C
                                 {
                                     object M(int[] a)
                                     {
                                         return from x in a select string.Empty;
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    /// <summary>
    /// Verifies the fix of a literal written directly in front of the <c>is</c> keyword, without trivia between them, separates
    /// <c>string.Empty</c> from the keyword with one space, under LF and CRLF
    /// </summary>
    /// <param name="endOfLine">The line ending of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-FIX-003")]
    [Requirement("RH3003-004")]
    public async Task FixOfLiteralDirectlyInFrontOfIsSeparatesTheReplacementFromTheKeyword(string endOfLine)
    {
        const string testCode = """
                                class C
                                {
                                    bool M()
                                    {
                                        return {|RH3003:""|}is string;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     bool M()
                                     {
                                         return string.Empty is string;
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    /// <summary>
    /// Verifies the fix of a literal written directly in front of the <c>as</c> keyword, without trivia between them, separates
    /// <c>string.Empty</c> from the keyword with one space, under LF and CRLF
    /// </summary>
    /// <param name="endOfLine">The line ending of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-FIX-003")]
    [Requirement("RH3003-004")]
    public async Task FixOfLiteralDirectlyInFrontOfAsSeparatesTheReplacementFromTheKeyword(string endOfLine)
    {
        const string testCode = """
                                class C
                                {
                                    object M()
                                    {
                                        return {|RH3003:""|}as object;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     object M()
                                     {
                                         return string.Empty as object;
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    /// <summary>
    /// Verifies the fix of a literal written directly in front of the <c>switch</c> keyword of a switch expression, without trivia
    /// between them, separates <c>string.Empty</c> from the keyword with one space, under LF and CRLF
    /// </summary>
    /// <param name="endOfLine">The line ending of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-FIX-003")]
    [Requirement("RH3003-004")]
    public async Task FixOfLiteralDirectlyInFrontOfSwitchSeparatesTheReplacementFromTheKeyword(string endOfLine)
    {
        const string testCode = """
                                class C
                                {
                                    int M()
                                    {
                                        return {|RH3003:""|}switch { _ => 1 };
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     int M()
                                     {
                                         return string.Empty switch { _ => 1 };
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    /// <summary>
    /// Verifies the fix of a literal written directly between <c>return</c> and <c>is</c>, without trivia on either side, separates
    /// <c>string.Empty</c> from both keywords with one space each, under LF and CRLF
    /// </summary>
    /// <param name="endOfLine">The line ending of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-FIX-003")]
    [Requirement("RH3003-004")]
    public async Task FixOfLiteralBetweenTwoKeywordsWithoutTriviaSeparatesTheReplacementFromBoth(string endOfLine)
    {
        const string testCode = """
                                class C
                                {
                                    bool M()
                                    {
                                        return{|RH3003:""|}is string;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     bool M()
                                     {
                                         return string.Empty is string;
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    /// <summary>
    /// Verifies the fix of a literal written directly behind an operator, without trivia between them, adds no space and keeps the
    /// author's spacing, under LF and CRLF
    /// </summary>
    /// <param name="endOfLine">The line ending of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-FIX-006")]
    [Requirement("RH3003-004")]
    public async Task FixOfLiteralDirectlyBehindAnOperatorKeepsTheAuthorsSpacing(string endOfLine)
    {
        const string testCode = """
                                class C
                                {
                                    string M(string a)
                                    {
                                        return a+{|RH3003:""|};
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     string M(string a)
                                     {
                                         return a+string.Empty;
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    /// <summary>
    /// Verifies the fix of a literal written directly between an opening parenthesis and an operator, without trivia on either
    /// side, adds no space and keeps the author's spacing, under LF and CRLF
    /// </summary>
    /// <param name="endOfLine">The line ending of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-FIX-006")]
    [Requirement("RH3003-004")]
    public async Task FixOfLiteralBetweenAParenthesisAndAnOperatorKeepsTheAuthorsSpacing(string endOfLine)
    {
        const string testCode = """
                                class C
                                {
                                    string M(string s)
                                    {
                                        return ({|RH3003:""|}+s);
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     string M(string s)
                                     {
                                         return (string.Empty+s);
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    /// <summary>
    /// Verifies the fix of a literal written directly between <c>=</c> and <c>;</c>, without trivia on either side, adds no space
    /// and keeps the author's spacing, under LF and CRLF
    /// </summary>
    /// <param name="endOfLine">The line ending of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-FIX-006")]
    [Requirement("RH3003-004")]
    public async Task FixOfLiteralDirectlyBetweenAnAssignmentAndASemicolonKeepsTheAuthorsSpacing(string endOfLine)
    {
        const string testCode = """
                                class C
                                {
                                    void M(string s)
                                    {
                                        s={|RH3003:""|};
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     void M(string s)
                                     {
                                         s=string.Empty;
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                public class Test
                                {
                                    public string Run()
                                    {
                                        return {|#0:""|} /* separator */ + {|#1:""|};
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public string Run()
                                     {
                                         return string.Empty /* separator */ + string.Empty;
                                     }
                                 }
                                 """;

        // A comment sits between the two literals, so the batch has to preserve trivia that lies in the gap
        // between its two replacements
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH3003UseStringEmptyForEmptyStringsAnalyzer.DiagnosticId, "Use string.Empty for empty strings.", 2));
    }

    #endregion // BatchCodeFixTestsBase
}