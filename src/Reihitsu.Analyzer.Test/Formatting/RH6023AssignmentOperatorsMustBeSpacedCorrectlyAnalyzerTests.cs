using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Spacing;
using Reihitsu.Analyzer.Rules.Spacing;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH6023AssignmentOperatorsMustBeSpacedCorrectlyAnalyzer"/> and <see cref="RH6023AssignmentOperatorsMustBeSpacedCorrectlyCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH6023AssignmentOperatorsMustBeSpacedCorrectlyAnalyzerTests : BatchCodeFixTestsBase<RH6023AssignmentOperatorsMustBeSpacedCorrectlyAnalyzer, RH6023AssignmentOperatorsMustBeSpacedCorrectlyCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that clean code does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6023-001")]
    [Requirement("SHARED-OPSPACE-002")]
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
    [Requirement("RH6023-001")]
    [Requirement("RH6023-002")]
    [Requirement("SHARED-OPSPACE-001")]
    [Requirement("SHARED-OPSPACE-003")]
    [Requirement("SHARED-OPSPACE-004")]
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
    [Requirement("RH6023-001")]
    [Requirement("RH6023-002")]
    [Requirement("SHARED-OPSPACE-001")]
    [Requirement("SHARED-OPSPACE-004")]
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
    [Requirement("RH6023-001")]
    [Requirement("RH6023-002")]
    [Requirement("SHARED-OPSPACE-001")]
    [Requirement("SHARED-OPSPACE-004")]
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
    [Requirement("RH6023-001")]
    [Requirement("RH6023-002")]
    [Requirement("SHARED-OPSPACE-001")]
    [Requirement("SHARED-OPSPACE-003")]
    [Requirement("SHARED-OPSPACE-004")]
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
    [Requirement("RH6023-001")]
    [Requirement("RH6023-002")]
    [Requirement("SHARED-OPSPACE-001")]
    [Requirement("SHARED-OPSPACE-003")]
    [Requirement("SHARED-OPSPACE-004")]
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
    [Requirement("RH6023-001")]
    [Requirement("RH6023-002")]
    [Requirement("SHARED-OPSPACE-001")]
    [Requirement("SHARED-OPSPACE-004")]
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
    [Requirement("RH6023-001")]
    [Requirement("SHARED-OPSPACE-002")]
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
    [Requirement("RH6023-001")]
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

    /// <summary>
    /// Verifies that the analyzer registers every <see cref="SyntaxKind"/> Roslyn classifies as an assignment
    /// expression, so a hand-maintained kind list can never silently drop an operator family from this rule's
    /// coverage the way the type-hierarchy check it replaced could not
    /// </summary>
    [TestMethod]
    [Requirement("RH6023-001")]
    public void VerifyRegisteredKindsCoverEveryAssignmentExpressionKind()
    {
        var expectedKinds = Enum.GetValues<SyntaxKind>()
                                .Where(SyntaxFacts.IsAssignmentExpression)
                                .OrderBy(static kind => (int)kind)
                                .ToImmutableArray();
        var registeredKinds = RH6023AssignmentOperatorsMustBeSpacedCorrectlyAnalyzer.AssignmentExpressionKinds.OrderBy(static kind => (int)kind).ToImmutableArray();

        Assert.AreSequenceEqual(expectedKinds, registeredKinds);
    }

    /// <summary>
    /// Verifies that the operators of simple and compound assignments, of field, property, parameter-default, local,
    /// constant, and enum-member initializers, and of name-equals clauses are reported once when a side is not exactly
    /// one space, also in object initializers, <c>with</c> expressions, and interpolation holes, and that a wrapped side
    /// and a string literal are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6023-001")]
    [Requirement("SHARED-OPSPACE-001")]
    [Requirement("SHARED-OPSPACE-002")]
    [Requirement("SHARED-OPSPACE-003")]
    public async Task VerifyEveryAssignmentAndInitializerOperatorIsChecked()
    {
        const string testData = """
                                using X{|RH6023:=|}System.String;

                                class A : System.Attribute
                                {
                                    public int N { get; set; }
                                }

                                record R(int V);

                                enum E
                                {
                                    One{|RH6023:=|}1,
                                }

                                class C
                                {
                                    int _f{|RH6023:=|}1;

                                    int P { get; }  {|RH6023:=|} 2;

                                    [A(N{|RH6023:=|}1)]
                                    void M(int a, int b{|RH6023:=|}2)
                                    {
                                        a{|RH6023:=|}1;
                                        a  {|RH6023:=|} 1;
                                        a→{|RH6023:=|} 1;
                                        a{|RH6023:+=|}1;
                                        a {|RH6023:<<=|}  1;

                                        object o = null;
                                        o{|RH6023:??=|}a;

                                        var n = new { V{|RH6023:=|}a };
                                        var c = new A { N{|RH6023:=|}1 };
                                        var r = new R(1) with { V{|RH6023:=|}2 };
                                        const int K{|RH6023:=|}3;
                                        var s = $"{a{|RH6023:=|}2}";

                                        a =
                                            1;
                                        var t = "a=1";
                                    }
                                }
                                """;

        await Verify(ExpandWhitespaceMarkers(testData));
    }

    /// <summary>
    /// Repeats <see cref="VerifyEveryAssignmentAndInitializerOperatorIsChecked"/> with the source normalized to CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6023-001")]
    [Requirement("SHARED-OPSPACE-001")]
    [Requirement("SHARED-OPSPACE-002")]
    [Requirement("SHARED-OPSPACE-003")]
    public async Task VerifyEveryAssignmentAndInitializerOperatorIsCheckedCarriageReturnLineFeed()
    {
        const string testData = """
                                using X{|RH6023:=|}System.String;

                                class A : System.Attribute
                                {
                                    public int N { get; set; }
                                }

                                record R(int V);

                                enum E
                                {
                                    One{|RH6023:=|}1,
                                }

                                class C
                                {
                                    int _f{|RH6023:=|}1;

                                    int P { get; }  {|RH6023:=|} 2;

                                    [A(N{|RH6023:=|}1)]
                                    void M(int a, int b{|RH6023:=|}2)
                                    {
                                        a{|RH6023:=|}1;
                                        a  {|RH6023:=|} 1;
                                        a→{|RH6023:=|} 1;
                                        a{|RH6023:+=|}1;
                                        a {|RH6023:<<=|}  1;

                                        object o = null;
                                        o{|RH6023:??=|}a;

                                        var n = new { V{|RH6023:=|}a };
                                        var c = new A { N{|RH6023:=|}1 };
                                        var r = new R(1) with { V{|RH6023:=|}2 };
                                        const int K{|RH6023:=|}3;
                                        var s = $"{a{|RH6023:=|}2}";

                                        a =
                                            1;
                                        var t = "a=1";
                                    }
                                }
                                """;

        await Verify(NormalizeToCarriageReturnLineFeed(ExpandWhitespaceMarkers(testData)));
    }

    /// <summary>
    /// Verifies that the fix puts one space on both sides of a <c>using</c>-alias, initializer, parameter-default,
    /// assignment, and compound-assignment operator
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6023-002")]
    [Requirement("SHARED-OPSPACE-004")]
    public async Task VerifyFixPutsOneSpaceOnBothSidesOfAssignmentOperators()
    {
        const string testData = """
                                using X{|RH6023:=|}System.String;

                                class C
                                {
                                    int _f{|RH6023:=|}1;

                                    void M(int a, int b{|RH6023:=|}2)
                                    {
                                        a  {|RH6023:=|} 1;
                                        a{|RH6023:+=|}1;
                                        a {|RH6023:<<=|}  1;
                                    }
                                }
                                """;
        const string fixedData = """
                                 using X = System.String;

                                 class C
                                 {
                                     int _f = 1;

                                     void M(int a, int b = 2)
                                     {
                                         a = 1;
                                         a += 1;
                                         a <<= 1;
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData);
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        var a{|#0:=|}1;
                                        var b{|#1:=|}2;
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method()
                                     {
                                         var a = 1;
                                         var b = 2;
                                     }
                                 }
                                 """;

        // Two statements on adjacent lines each carry their own unspaced assignment operator; the fixes only
        // rewrite their own operator's gaps, so the batch fixer converges in one pass
        return new FixAllScenario(testData,
                                  fixedData,
                                  Diagnostics(RH6023AssignmentOperatorsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6023MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}