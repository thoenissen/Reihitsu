using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5107CommaMustBeOnSameLineAsPreviousParameterAnalyzer"/> and <see cref="RH5107CommaMustBeOnSameLineAsPreviousParameterCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5107CommaMustBeOnSameLineAsPreviousParameterAnalyzerTests : AnalyzerTestsBase<RH5107CommaMustBeOnSameLineAsPreviousParameterAnalyzer, RH5107CommaMustBeOnSameLineAsPreviousParameterCodeFixProvider>
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
                                    void Method(
                                        int first,
                                        int second)
                                    {
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
                                    void Method(int first
                                                {|#0:,|} int second)
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method(int first,
                                                 int second)
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5107CommaMustBeOnSameLineAsPreviousParameterAnalyzer.DiagnosticId, AnalyzerResources.RH5107MessageFormat));
    }

    /// <summary>
    /// Verifies that the violation is still reported without offering a fix when the token gap contains a preprocessor
    /// directive, so hoisting the comma can never move it across the directive boundary
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticWithoutCodeFixWhenDirectivesArePresent()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(int first
                                #if FEATURE
                                #endif
                                                {|#0:,|} int second)
                                    {
                                    }
                                }
                                """;
        const string codeFixData = """
                                   internal class TestClass
                                   {
                                       void Method(int first
                                   #if FEATURE
                                   #endif
                                                   , int second)
                                       {
                                       }
                                   }
                                   """;

        await Verify(testData, Diagnostics(RH5107CommaMustBeOnSameLineAsPreviousParameterAnalyzer.DiagnosticId, AnalyzerResources.RH5107MessageFormat));

        var actions = await GetCodeFixActionsAsync(codeFixData,
                                                   RH5107CommaMustBeOnSameLineAsPreviousParameterAnalyzer.DiagnosticId,
                                                   root => GetFirstSeparatorLocation(root));

        Assert.IsEmpty(actions);
    }

    #endregion // Tests

    #region Methods

    /// <summary>
    /// Gets the location of the first separator of the first parameter list of the first method declaration
    /// </summary>
    /// <param name="root">Syntax root</param>
    /// <returns>The location of the first parameter separator</returns>
    private static Location GetFirstSeparatorLocation(SyntaxNode root)
    {
        var parameterList = root.DescendantNodes()
                                .OfType<MethodDeclarationSyntax>()
                                .First()
                                .ParameterList;

        return parameterList.Parameters.GetSeparator(0).GetLocation();
    }

    #endregion // Methods
}