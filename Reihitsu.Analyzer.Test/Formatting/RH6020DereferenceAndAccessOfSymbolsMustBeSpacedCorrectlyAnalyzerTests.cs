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
public class RH6020DereferenceAndAccessOfSymbolsMustBeSpacedCorrectlyAnalyzerTests : AnalyzerTestsBase<RH6020DereferenceAndAccessOfSymbolsMustBeSpacedCorrectlyAnalyzer, RH6020DereferenceAndAccessOfSymbolsMustBeSpacedCorrectlyCodeFixProvider>
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
    /// Verifies that no diagnostic is reported when the space separates two address-of operators, because removing it would glue them into the logical-and operator (issue #413)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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
    /// Verifies that the fix is not offered when deleting the space would glue two address-of operators into the logical-and operator (issue #413)
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

    #endregion // Tests
}