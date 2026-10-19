using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Spacing;
using Reihitsu.Analyzer.Rules.Spacing;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH6020DereferenceAndAccessOfSymbolsMustBeSpacedCorrectlyAnalyzer"/> and <see cref="RH6020DereferenceAndAccessOfSymbolsMustBeSpacedCorrectlyCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH6020DereferenceAndAccessOfSymbolsMustBeSpacedCorrectlyAnalyzerTests : BatchCodeFixTestsBase<RH6020DereferenceAndAccessOfSymbolsMustBeSpacedCorrectlyAnalyzer, RH6020DereferenceAndAccessOfSymbolsMustBeSpacedCorrectlyCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that clean code does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6020-001")]
    public async Task VerifyNoDiagnosticsWhenCodeIsClean()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    unsafe void Method()
                                    {
                                        int value = 0;
                                        int* pointer = &value;
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
    [Requirement("RH6020-001")]
    [Requirement("RH6020-003")]
    [Requirement("RH6020-004")]
    [Requirement("SHARED-WS-001")]
    public async Task VerifyIssueIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    unsafe void Method()
                                    {
                                        int value = 0;
                                        int* pointer = &{|#0: |}value;
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     unsafe void Method()
                                     {
                                         int value = 0;
                                         int* pointer = &value;
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH6020DereferenceAndAccessOfSymbolsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6020MessageFormat));
    }

    /// <summary>
    /// Verifies that no diagnostic is reported when the space separates two address-of operators, because removing it would glue them into the logical-and operator
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6020-002")]
    public async Task VerifyNoDiagnosticWhenAddressOfOperandStartsWithAddressOf()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    unsafe void Method(int x)
                                    {
                                        int** pp = & &x;
                                    }
                                }
                                """;

        await Verify(testData, test => test.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifies that the fix is not offered when deleting the space would glue two address-of operators into the logical-and operator
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFixIsNotOfferedForNestedAddressOf()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    unsafe void Method(int x)
                                    {
                                        int** pp = & &x;
                                    }
                                }
                                """;

        var actions = await GetCodeFixActionsAsync(testData,
                                                   RH6020DereferenceAndAccessOfSymbolsMustBeSpacedCorrectlyAnalyzer.DiagnosticId,
                                                   root =>
                                                   {
                                                       var addressOf = root.DescendantTokens().First(token => token.IsKind(SyntaxKind.AmpersandToken) && token.GetNextToken().IsKind(SyntaxKind.AmpersandToken));

                                                       return Location.Create(root.SyntaxTree, new TextSpan(addressOf.Span.End, 1));
                                                   });

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifies that spaces, a tab, or a run at the end of the line after the <c>&amp;</c> of an address-of or the
    /// <c>*</c> of a pointer indirection are reported once per operator, also before a nested operator or a
    /// parenthesized operand, and that an operator followed by a line break is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6020-001")]
    [Requirement("RH6020-003")]
    public async Task VerifyWhitespaceAfterAddressOfOrIndirectionIsReported()
    {
        const string testData = """
                                unsafe class C
                                {
                                    void M(int a, int* p, int** pp)
                                    {
                                        int* x = &{|RH6020: |}a;
                                        int y = *{|RH6020: |}p;
                                        int z = *{|RH6020:→|}p;
                                        int w = *{|RH6020: |}*pp;
                                        int* r = &{|RH6020: |}*p;
                                        int s = *{|RH6020: |}(p + 1);
                                        int t = *{|RH6020:···|}
                                            p;
                                        int u = *
                                            p;
                                    }
                                }
                                """;

        await Verify(ExpandWhitespaceMarkers(testData));
    }

    /// <summary>
    /// Repeats <see cref="VerifyWhitespaceAfterAddressOfOrIndirectionIsReported"/> with the source normalized to CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6020-001")]
    [Requirement("RH6020-003")]
    public async Task VerifyWhitespaceAfterAddressOfOrIndirectionIsReportedCarriageReturnLineFeed()
    {
        const string testData = """
                                unsafe class C
                                {
                                    void M(int a, int* p, int** pp)
                                    {
                                        int* x = &{|RH6020: |}a;
                                        int y = *{|RH6020: |}p;
                                        int z = *{|RH6020:→|}p;
                                        int w = *{|RH6020: |}*pp;
                                        int* r = &{|RH6020: |}*p;
                                        int s = *{|RH6020: |}(p + 1);
                                        int t = *{|RH6020:···|}
                                            p;
                                        int u = *
                                            p;
                                    }
                                }
                                """;

        await Verify(NormalizeToCarriageReturnLineFeed(ExpandWhitespaceMarkers(testData)));
    }

    /// <summary>
    /// Verifies that the whitespace after an address-of whose operand starts with <c>&amp;</c> is not reported; the
    /// example does not compile, so compiler diagnostics are not verified
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6020-002")]
    public async Task VerifyWhitespaceThatKeepsTwoAddressOfOperatorsApartIsNotReported()
    {
        const string testData = """
                                unsafe class C
                                {
                                    void M(int a)
                                    {
                                        int** q = & &a;
                                    }
                                }
                                """;

        await Verify(testData, test => test.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifies that the fix deletes spaces and a tab after an address-of or a pointer indirection
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6020-004")]
    [Requirement("SHARED-WS-001")]
    public async Task VerifyFixDeletesWhitespaceAfterAddressOfOrIndirection()
    {
        const string testData = """
                                unsafe class C
                                {
                                    void M(int a, int* p, int** pp)
                                    {
                                        int* x = &{|RH6020: |}a;
                                        int z = *{|RH6020:→|}p;
                                        int w = *{|RH6020: |}*pp;
                                    }
                                }
                                """;
        const string fixedData = """
                                 unsafe class C
                                 {
                                     void M(int a, int* p, int** pp)
                                     {
                                         int* x = &a;
                                         int z = *p;
                                         int w = **pp;
                                     }
                                 }
                                 """;

        await Verify(ExpandWhitespaceMarkers(testData), fixedData);
    }

    /// <summary>
    /// Verifies that a comment on the next line of the gap after a pointer indirection keeps the diagnostic and
    /// withholds the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6020-001")]
    [Requirement("RH6020-004")]
    [Requirement("SHARED-WS-002")]
    public async Task VerifyFixIsNotOfferedWhenCommentFollowsOnNextLine()
    {
        const string testData = """
                                unsafe class C
                                {
                                    void M(int* p)
                                    {
                                        int y = *{|RH6020:·|}
                                            // the pointer
                                            p;
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(ExpandWhitespaceMarkers(testData));
    }

    /// <summary>
    /// Repeats <see cref="VerifyFixIsNotOfferedWhenCommentFollowsOnNextLine"/> with the source normalized to CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6020-001")]
    [Requirement("RH6020-004")]
    [Requirement("SHARED-WS-002")]
    public async Task VerifyFixIsNotOfferedWhenCommentFollowsOnNextLineCarriageReturnLineFeed()
    {
        const string testData = """
                                unsafe class C
                                {
                                    void M(int* p)
                                    {
                                        int y = *{|RH6020:·|}
                                            // the pointer
                                            p;
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(NormalizeToCarriageReturnLineFeed(ExpandWhitespaceMarkers(testData)));
    }

    /// <summary>
    /// Verifies that a directive on the next line of the gap after a pointer indirection keeps the diagnostic and
    /// withholds the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6020-001")]
    [Requirement("RH6020-004")]
    [Requirement("SHARED-WS-002")]
    public async Task VerifyFixIsNotOfferedWhenDirectiveFollowsOnNextLine()
    {
        const string testData = """
                                unsafe class C
                                {
                                    void M(int* p)
                                    {
                                        int y = *{|RH6020:·|}
                                #if DEBUG
                                #endif
                                            p;
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(ExpandWhitespaceMarkers(testData));
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    unsafe void Method()
                                    {
                                        int first = 0;
                                        int second = 0;
                                        int* firstPointer = &{|#0: |}first;
                                        int* secondPointer = &{|#1: |}second;
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     unsafe void Method()
                                     {
                                         int first = 0;
                                         int second = 0;
                                         int* firstPointer = &first;
                                         int* secondPointer = &second;
                                     }
                                 }
                                 """;

        // Two statements on adjacent lines each carry their own unwanted whitespace run after the address-of
        // operator; the fixes only remove their own run, so the batch fixer converges in one pass
        return new FixAllScenario(testData,
                                  fixedData,
                                  Diagnostics(RH6020DereferenceAndAccessOfSymbolsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6020MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}