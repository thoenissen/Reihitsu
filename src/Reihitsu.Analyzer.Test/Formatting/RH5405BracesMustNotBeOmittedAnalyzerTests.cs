using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5405BracesMustNotBeOmittedAnalyzer"/> and <see cref="RH5405BracesMustNotBeOmittedCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5405BracesMustNotBeOmittedAnalyzerTests : BatchCodeFixTestsBase<RH5405BracesMustNotBeOmittedAnalyzer, RH5405BracesMustNotBeOmittedCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that clean code does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5405-001")]
    public async Task VerifyNoDiagnosticsWhenCodeIsClean()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        if (true)
                                        {
                                        }
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that the issue is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BRACE-001")]
    [Requirement("SHARED-BRACE-002")]
    [Requirement("RH5405-001")]
    [Requirement("RH5405-002")]
    public async Task VerifyIssueIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        if (true)
                                            {|#0:return;|}
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method()
                                     {
                                         if (true)
                                         {
                                             return;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5405BracesMustNotBeOmittedAnalyzer.DiagnosticId, AnalyzerResources.RH5405MessageFormat));
    }

    /// <summary>
    /// Verifies that the issue is fixed without deleting the header when the child shares the parent's line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BRACE-002")]
    [Requirement("RH5405-001")]
    [Requirement("RH5405-002")]
    public async Task VerifyIssueIsFixedWhenChildSharesParentLine()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(bool x)
                                    {
                                        if (x) {|#0:Foo();|}
                                    }

                                    void Foo()
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method(bool x)
                                     {
                                         if (x)
                                         {
                                             Foo();
                                         }
                                     }

                                     void Foo()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5405BracesMustNotBeOmittedAnalyzer.DiagnosticId, AnalyzerResources.RH5405MessageFormat));
    }

    /// <summary>
    /// Verifies that a single-line brace-less else child statement is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BRACE-001")]
    [Requirement("SHARED-BRACE-002")]
    [Requirement("RH5405-001")]
    [Requirement("RH5405-002")]
    public async Task VerifyElseChildStatementIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(bool x)
                                    {
                                        if (x)
                                        {
                                        }
                                        else
                                            {|#0:return;|}
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method(bool x)
                                     {
                                         if (x)
                                         {
                                         }
                                         else
                                         {
                                             return;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5405BracesMustNotBeOmittedAnalyzer.DiagnosticId, AnalyzerResources.RH5405MessageFormat));
    }

    /// <summary>
    /// Verifies that when both the if-branch and the else-branch are unbraced, the fix places the else keyword on
    /// its own line at the statement's indentation instead of appending it to the closing brace's line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BRACE-001")]
    [Requirement("SHARED-BRACE-002")]
    [Requirement("RH5405-001")]
    [Requirement("RH5405-002")]
    public async Task VerifyBothBranchesUnbracedPlacesElseOnItsOwnLine()
    {
        const string testData = """
                                internal class Sample
                                {
                                    void Method(bool x)
                                    {
                                        if (x)
                                            {|#0:Foo();|}
                                        else
                                            {|#1:Bar();|}
                                    }

                                    void Foo()
                                    {
                                    }

                                    void Bar()
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class Sample
                                 {
                                     void Method(bool x)
                                     {
                                         if (x)
                                         {
                                             Foo();
                                         }
                                         else
                                         {
                                             Bar();
                                         }
                                     }

                                     void Foo()
                                     {
                                     }

                                     void Bar()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5405BracesMustNotBeOmittedAnalyzer.DiagnosticId, AnalyzerResources.RH5405MessageFormat, 2));
    }

    /// <summary>
    /// Verifies that a multi-line brace-less child statement is not flagged by RH5405 (it is reported by RH5406),
    /// so the two rules never report the same statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5405-001")]
    public async Task VerifyMultiLineChildStatementIsNotFlagged()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        if (true)
                                            Other(1,
                                                  2);
                                    }

                                    void Other(int value1, int value2)
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that an "else if" does not produce a diagnostic for the nested if-statement header
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BRACE-001")]
    public async Task VerifyElseIfDoesNotProduceDiagnostic()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(bool x, bool y)
                                    {
                                        if (x)
                                        {
                                        }
                                        else if (y)
                                        {
                                        }
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that the fix keeps the blank line that separates the braced statement from the previous statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BRACE-002")]
    public async Task VerifyBlankLineAboveFixedStatementIsKept()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    int Method(int value)
                                    {
                                        var copy = value;

                                        if (copy > 0)
                                            {|#0:copy++;|}

                                        return copy;
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     int Method(int value)
                                     {
                                         var copy = value;

                                         if (copy > 0)
                                         {
                                             copy++;
                                         }

                                         return copy;
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5405BracesMustNotBeOmittedAnalyzer.DiagnosticId, AnalyzerResources.RH5405MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix inserts the blank line that separates the braced statement from a preceding declaration statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyBlankLineIsInsertedAboveFixedStatementAfterDeclaration()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    int Method(int value)
                                    {
                                        var copy = value;
                                        if (copy > 0)
                                            {|#0:copy++;|}

                                        return copy;
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     int Method(int value)
                                     {
                                         var copy = value;

                                         if (copy > 0)
                                         {
                                             copy++;
                                         }

                                         return copy;
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5405BracesMustNotBeOmittedAnalyzer.DiagnosticId, AnalyzerResources.RH5405MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix inserts the blank line that separates the braced statement from a preceding closing brace
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyBlankLineIsInsertedAboveFixedStatementAfterClosingBrace()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    int Method(int value)
                                    {
                                        if (value > 1)
                                        {
                                            value--;
                                        }
                                        if (value > 0)
                                            {|#0:value++;|}

                                        return value;
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     int Method(int value)
                                     {
                                         if (value > 1)
                                         {
                                             value--;
                                         }

                                         if (value > 0)
                                         {
                                             value++;
                                         }

                                         return value;
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5405BracesMustNotBeOmittedAnalyzer.DiagnosticId, AnalyzerResources.RH5405MessageFormat));
    }

    /// <summary>
    /// Verifies that fixing two adjacent statements inserts the blank line above each of them
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyBlankLinesAreInsertedAboveAdjacentFixedStatements()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    int Method(int value)
                                    {
                                        var copy = value;
                                        if (copy > 0)
                                            {|#0:copy++;|}
                                        if (copy > 1)
                                            {|#1:copy--;|}

                                        return copy;
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     int Method(int value)
                                     {
                                         var copy = value;

                                         if (copy > 0)
                                         {
                                             copy++;
                                         }

                                         if (copy > 1)
                                         {
                                             copy--;
                                         }

                                         return copy;
                                     }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5405BracesMustNotBeOmittedAnalyzer.DiagnosticId, AnalyzerResources.RH5405MessageFormat, 2));
    }

    /// <summary>
    /// Verifies that the bodies of an if, of an else if, and of an else without braces are each reported, while the
    /// body of a while statement is not checked
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BRACE-001")]
    [Requirement("RH5405-001")]
    [Requirement("RH5405-002")]
    public async Task VerifyBodiesOfIfElseIfAndElseAreCheckedButNotLoopBodies()
    {
        const string testCode = """
                                class C
                                {
                                    void M(bool a, bool b)
                                    {
                                        if (a)
                                            {|RH5405:M(a, b);|}
                                        else if (b)
                                            {|RH5405:M(a, b);|}
                                        else
                                            {|RH5405:M(a, b);|}
                                        while (a)
                                            M(a, b);
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that the unbraced bodies of do, for, foreach, using, lock, and fixed statements are not checked
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BRACE-001")]
    public async Task VerifyBodiesOfLoopAndResourceStatementsAreNotChecked()
    {
        const string testCode = """
                                class C
                                {
                                    unsafe void M(bool a, int[] v, System.IDisposable d)
                                    {
                                        do
                                            M(a, v, d);
                                        while (a);
                                        for (var i = 0; i < 1; i++)
                                            M(a, v, d);
                                        foreach (var x in v)
                                            M(a, v, d);
                                        using (d)
                                            M(a, v, d);
                                        lock (v)
                                            M(a, v, d);
                                        fixed (int* p = v)
                                            M(a, v, d);
                                    }
                                }
                                """;

        await Verify(testCode, test => test.SolutionTransforms.Add(ApplyAllowUnsafeToTestProject));
    }

    /// <summary>
    /// Verifies that a body without braces on one line is reported, whether it is on the line of its if or on a later
    /// line, while a body that spans several lines and a block are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5405-001")]
    [Requirement("RH5405-002")]
    public async Task VerifyUnbracedBodyOnOneLineIsReported()
    {
        const string testCode = """
                                class C
                                {
                                    void M(bool a, int b)
                                    {
                                        if (a)
                                            {|RH5405:M(a, b);|}
                                        if (a) {|RH5405:M(a, b);|}
                                        if (a)
                                            M(a,
                                              b);
                                        if (a)
                                        {
                                            M(a, b);
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that the fix wraps a body on the line of its if in braces on their own lines in the column of the if,
    /// with the body indented one level inside them
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BRACE-002")]
    public async Task VerifyFixWrapsBodyOnLineOfIfInBraces()
    {
        const string testCode = """
                                class C
                                {
                                    void M(bool a, bool b)
                                    {
                                        if (a) {|RH5405:M(a, b);|}
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     void M(bool a, bool b)
                                     {
                                         if (a)
                                         {
                                             M(a, b);
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that the fix wraps a body on the line of its if in braces on their own lines in the column of the if,
    /// with the body indented one level inside them, with CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BRACE-002")]
    public async Task VerifyFixWrapsBodyOnLineOfIfInBracesWithCarriageReturnLineFeed()
    {
        const string testCode = """
                                class C
                                {
                                    void M(bool a, bool b)
                                    {
                                        if (a) {|RH5405:M(a, b);|}
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     void M(bool a, bool b)
                                     {
                                         if (a)
                                         {
                                             M(a, b);
                                         }
                                     }
                                 }
                                 """;

        await Verify(NormalizeToCarriageReturnLineFeed(testCode), NormalizeToCarriageReturnLineFeed(fixedCode));
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                internal class TestClass
                                {
                                    void Method1(bool x)
                                    {
                                        if (x)
                                            {|#0:Foo();|}
                                    }

                                    void Method2(bool y)
                                    {
                                        if (y)
                                            {|#1:Bar();|}
                                    }

                                    void Foo()
                                    {
                                    }

                                    void Bar()
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class TestClass
                                 {
                                     void Method1(bool x)
                                     {
                                         if (x)
                                         {
                                             Foo();
                                         }
                                     }

                                     void Method2(bool y)
                                     {
                                         if (y)
                                         {
                                             Bar();
                                         }
                                     }

                                     void Foo()
                                     {
                                     }

                                     void Bar()
                                     {
                                     }
                                 }
                                 """;

        // RH5405 only reports single-line embedded child statements; nesting a second violation inside the first
        // would make the outer child multi-line, which RH5406 owns instead, so the only way two diagnostics could
        // share an owning statement is an if/else pair - and there both branches resolve to the identical
        // if-statement node, collapsing to what the batch fixer treats as a single reformat rather than two
        // interfering ones. Two independent if-statements in different methods are used instead, because
        // interference beyond that identical-owner collapse is unreachable for this rule
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH5405BracesMustNotBeOmittedAnalyzer.DiagnosticId, AnalyzerResources.RH5405MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}