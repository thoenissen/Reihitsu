using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Organization;
using Reihitsu.Analyzer.Rules.Organization;
using Reihitsu.Analyzer.Test.Base;
using Reihitsu.Analyzer.Test.Verifiers;

namespace Reihitsu.Analyzer.Test.Ordering;

/// <summary>
/// Test methods for <see cref="RH7105DeclarationKeywordsMustFollowOrderAnalyzer"/> and <see cref="RH7105DeclarationKeywordsMustFollowOrderCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH7105DeclarationKeywordsMustFollowOrderAnalyzerTests : AnalyzerTestsBase<RH7105DeclarationKeywordsMustFollowOrderAnalyzer, RH7105DeclarationKeywordsMustFollowOrderCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying misordered declaration keywords are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task MisorderedDeclarationKeywordsAreReportedAndFixed()
    {
        const string testCode = """
                                public class TestClass
                                {
                                    {|#0:static|} public int Value { get; set; }
                                }
                                """;

        const string fixedCode = """
                                 public class TestClass
                                 {
                                     public static int Value { get; set; }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH7105DeclarationKeywordsMustFollowOrderAnalyzer.DiagnosticId, AnalyzerResources.RH7105MessageFormat));
    }

    /// <summary>
    /// Verifying that <c>readonly</c> before <c>partial</c> on a struct is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ReadonlyBeforePartialStructIsNotReported()
    {
        const string testCode = """
                                public readonly partial struct TestStruct
                                {
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying that <c>unsafe</c> before <c>partial</c> on a class is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task UnsafeBeforePartialClassIsNotReported()
    {
        const string testCode = """
                                public unsafe partial class TestClass
                                {
                                }
                                """;

        await Verify(testCode, AllowUnsafe);
    }

    /// <summary>
    /// Verifying that <c>async</c> before <c>partial</c> on a method is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task AsyncBeforePartialMethodIsNotReported()
    {
        const string testCode = """
                                using System.Threading.Tasks;

                                public partial class TestClass
                                {
                                    private partial Task DoAsync();

                                    private async partial Task DoAsync()
                                    {
                                        await Task.CompletedTask;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying that a misordered modifier before <c>partial</c> is reported and fixed to a compiling order
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task MisorderedModifiersWithPartialAreReportedAndFixed()
    {
        const string testCode = """
                                {|#0:readonly|} public partial struct TestStruct
                                {
                                    public int Value { get; }
                                }
                                """;

        const string fixedCode = """
                                 public readonly partial struct TestStruct
                                 {
                                     public int Value { get; }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH7105DeclarationKeywordsMustFollowOrderAnalyzer.DiagnosticId, AnalyzerResources.RH7105MessageFormat));
    }

    /// <summary>
    /// Verifying that a misordered modifier list is still reported without offering a fix when a preprocessor
    /// directive sits between the modifiers
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task MisorderedModifiersWithDirectiveAreReportedWithoutFix()
    {
        const string testCode = """
                                public class TestClass
                                {
                                    {|#0:static|}
                                #if FEATURE
                                #endif
                                    public int Value { get; set; }
                                }
                                """;

        await Verify(testCode, Diagnostics(RH7105DeclarationKeywordsMustFollowOrderAnalyzer.DiagnosticId, AnalyzerResources.RH7105MessageFormat));

        var actions = await GetCodeFixActionsAsync(testCode.Replace("{|#0:static|}", "static"),
                                                   RH7105DeclarationKeywordsMustFollowOrderAnalyzer.DiagnosticId,
                                                   root => root.DescendantTokens()
                                                               .First(token => token.IsKind(SyntaxKind.StaticKeyword))
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    #endregion // Tests

    #region Methods

    /// <summary>
    /// Enables unsafe code for the verifier so declarations using the <c>unsafe</c> modifier compile
    /// </summary>
    /// <param name="test">Test</param>
    private static void AllowUnsafe(CSharpAnalyzerVerifierTest<RH7105DeclarationKeywordsMustFollowOrderAnalyzer> test)
    {
        test.SolutionTransforms.Add((solution, projectId) => solution.GetProject(projectId)?.CompilationOptions is CSharpCompilationOptions compilationOptions
                                                                 ? solution.WithProjectCompilationOptions(projectId, compilationOptions.WithAllowUnsafe(true))
                                                                 : solution);
    }

    #endregion // Methods
}