using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Spacing;
using Reihitsu.Analyzer.Rules.Spacing;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH6023AssignmentOperatorsMustBeSpacedCorrectlyAnalyzer"/> and <see cref="RH6023AssignmentOperatorsMustBeSpacedCorrectlyCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH6023AssignmentOperatorsMustBeSpacedCorrectlyAnalyzerTests : AnalyzerTestsBase<RH6023AssignmentOperatorsMustBeSpacedCorrectlyAnalyzer, RH6023AssignmentOperatorsMustBeSpacedCorrectlyCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that clean code does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsWhenCodeIsClean()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        var a = 2;
                                        var abc = 3;
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that padding before an assignment operator is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyPaddingBeforeAssignmentIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        var a   {|#0:=|} 2;
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method()
                                     {
                                         var a = 2;
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH6023AssignmentOperatorsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6023MessageFormat));
    }

    /// <summary>
    /// Verifies that a missing space before an assignment operator is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMissingSpaceBeforeAssignmentIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        var a{|#0:=|} 2;
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method()
                                     {
                                         var a = 2;
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH6023AssignmentOperatorsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6023MessageFormat));
    }

    /// <summary>
    /// Verifies that a missing space after an assignment operator is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMissingSpaceAfterAssignmentIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        var a {|#0:=|}2;
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method()
                                     {
                                         var a = 2;
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH6023AssignmentOperatorsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6023MessageFormat));
    }

    /// <summary>
    /// Verifies that a missing space on both sides of an assignment operator is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoSpaceAroundAssignmentIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        var a{|#0:=|}2;
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method()
                                     {
                                         var a = 2;
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH6023AssignmentOperatorsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6023MessageFormat));
    }

    /// <summary>
    /// Verifies that a compound assignment operator is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCompoundAssignmentIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(int a)
                                    {
                                        a  {|#0:+=|} 2;
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method(int a)
                                     {
                                         a += 2;
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH6023AssignmentOperatorsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6023MessageFormat));
    }

    /// <summary>
    /// Verifies that a name-equals assignment inside an anonymous object is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNameEqualsAssignmentIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(int x)
                                    {
                                        var value = new { Value  {|#0:=|} x };
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method(int x)
                                     {
                                         var value = new { Value = x };
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH6023AssignmentOperatorsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6023MessageFormat));
    }

    /// <summary>
    /// Verifies that an assignment operator that wraps to a new line does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyWrappedAssignmentIsIgnored()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        var a =
                                            2;
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that assignment-like characters inside a string literal do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyAssignmentInsideStringLiteralIsIgnored()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        var text = "a   =   b";
                                    }
                                }
                                """;

        await Verify(testData);
    }

    #endregion // Tests
}