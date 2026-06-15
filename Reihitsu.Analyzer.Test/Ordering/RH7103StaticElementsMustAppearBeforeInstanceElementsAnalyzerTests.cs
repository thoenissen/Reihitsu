using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Organization;
using Reihitsu.Analyzer.Rules.Organization;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Ordering;

/// <summary>
/// Test methods for <see cref="RH7103StaticElementsMustAppearBeforeInstanceElementsAnalyzer"/> and <see cref="RH7103StaticElementsMustAppearBeforeInstanceElementsCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH7103StaticElementsMustAppearBeforeInstanceElementsAnalyzerTests : AnalyzerTestsBase<RH7103StaticElementsMustAppearBeforeInstanceElementsAnalyzer, RH7103StaticElementsMustAppearBeforeInstanceElementsCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying static members are reported and fixed when they appear after instance members of the same group
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task StaticMembersAreReportedAndFixedWhenTheyAppearAfterInstanceMembers()
    {
        const string testCode = """
                                public class TestClass
                                {
                                    public void Run()
                                    {
                                    }

                                    public static void {|#0:Create|}()
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class TestClass
                                 {
                                     public static void Create()
                                     {
                                     }
                                     public void Run()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH7103StaticElementsMustAppearBeforeInstanceElementsAnalyzer.DiagnosticId, AnalyzerResources.RH7103MessageFormat));
    }

    /// <summary>
    /// Verifying destructors do not crash analysis
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task DestructorsDoNotCrashAnalyzer()
    {
        const string testCode = """
                                public class TestClass
                                {
                                    public static int Count;

                                    ~TestClass()
                                    {
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying no code fix is offered when moving the static field over another static field would change initializer execution order
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NoCodeFixWhenMoveChangesStaticInitializerExecutionOrder()
    {
        const string testCode = """
                                public class TestClass
                                {
                                    private int _instance;
                                    private static int _a = Compute();
                                    private static int _b = _a;

                                    private static int Compute() => 1;
                                }
                                """;

        var actions = await GetCodeFixActionsAsync(testCode,
                                                   RH7103StaticElementsMustAppearBeforeInstanceElementsAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<VariableDeclaratorSyntax>()
                                                               .Single(declarator => declarator.Identifier.ValueText == "_b")
                                                               .Identifier
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifying no code fix is offered when preprocessor directives sit in the affected leading trivia
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NoCodeFixWhenDirectivesAreInLeadingTrivia()
    {
        const string testCode = """
                                public class TestClass
                                {
                                    private int _instance;

                                    #region Static
                                    private static int _static = 1;
                                    #endregion
                                }
                                """;

        var actions = await GetCodeFixActionsAsync(testCode,
                                                   RH7103StaticElementsMustAppearBeforeInstanceElementsAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<VariableDeclaratorSyntax>()
                                                               .Single(declarator => declarator.Identifier.ValueText == "_static")
                                                               .Identifier
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    #endregion // Tests
}