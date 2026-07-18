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
    /// Verifying no code fix is offered when moving the static field would jump over a static event field initializer
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NoCodeFixWhenMoveChangesInitializerExecutionOrderAcrossEventField()
    {
        const string testCode = """
                                public class TestClass
                                {
                                    private int _instance;
                                    public static event System.EventHandler E = Handler;
                                    private static int _b = 1;

                                    private static System.EventHandler Handler => null;
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
                                    #region Fields
                                    private int _instance;
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

    /// <summary>
    /// Verifying RH7103 does not compare a static member against an instance member when they live in separate regions
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NoDiagnosticWhenStaticAndInstanceLiveInSeparateRegions()
    {
        const string testCode = """
                                public class TestClass
                                {
                                    #region Lifecycle

                                    public void Run()
                                    {
                                    }

                                    #endregion

                                    #region Factories

                                    public static TestClass Create()
                                    {
                                        return new TestClass();
                                    }

                                    #endregion
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying RH7103 does not compare an instance member outside any region against a static member inside a region
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NoDiagnosticWhenInstanceIsOutsideRegionAndStaticIsInsideRegion()
    {
        const string testCode = """
                                public class TestClass
                                {
                                    public void Run()
                                    {
                                    }

                                    #region Factories

                                    public static TestClass Create()
                                    {
                                        return new TestClass();
                                    }

                                    #endregion
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying RH7103 does not compare a static member outside any region against an instance member inside a region
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NoDiagnosticWhenStaticIsOutsideRegionAndInstanceIsInsideRegion()
    {
        const string testCode = """
                                public class TestClass
                                {
                                    #region Lifecycle

                                    public void Run()
                                    {
                                    }

                                    #endregion

                                    public static TestClass Create()
                                    {
                                        return new TestClass();
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying RH7103 still reports a static member that follows an instance member of the same group within the same region
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task StaticMembersAreReportedWhenTheyAppearAfterInstanceMembersInSameRegion()
    {
        const string testCode = """
                                public class TestClass
                                {
                                    #region Members

                                    public void Run()
                                    {
                                    }

                                    public static void {|#0:Create|}()
                                    {
                                    }

                                    #endregion
                                }
                                """;

        await Verify(testCode, Diagnostics(RH7103StaticElementsMustAppearBeforeInstanceElementsAnalyzer.DiagnosticId, AnalyzerResources.RH7103MessageFormat));
    }

    /// <summary>
    /// Verifying RH7103 reports a static member only within its own region when other regions contain unrelated instance members
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task DiagnosticIsScopedToContainingRegion()
    {
        const string testCode = """
                                public class TestClass
                                {
                                    #region Lifecycle

                                    public void Run()
                                    {
                                    }

                                    #endregion

                                    #region Factories

                                    public void Reset()
                                    {
                                    }

                                    public static TestClass {|#0:Create|}()
                                    {
                                        return new TestClass();
                                    }

                                    #endregion
                                }
                                """;

        await Verify(testCode, Diagnostics(RH7103StaticElementsMustAppearBeforeInstanceElementsAnalyzer.DiagnosticId, AnalyzerResources.RH7103MessageFormat));
    }

    /// <summary>
    /// Verifying the code fix reorders the static member within its own region and is not suppressed by an earlier instance member that lives in a different region
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task CodeFixReordersStaticWithinRegionAndIgnoresInstanceMembersInOtherRegions()
    {
        const string testCode = """
                                public class TestClass
                                {
                                    #region Decoy

                                    public void Decoy()
                                    {
                                    }

                                    #endregion

                                    #region Factories

                                    public int Counter
                                    {
                                        get;
                                        set;
                                    }

                                    public void Reset()
                                    {
                                    }

                                    public static TestClass {|#0:Create|}()
                                    {
                                        return new TestClass();
                                    }

                                    #endregion
                                }
                                """;

        const string fixedCode = """
                                 public class TestClass
                                 {
                                     #region Decoy

                                     public void Decoy()
                                     {
                                     }

                                     #endregion // Decoy

                                     #region Factories

                                     public int Counter { get; set; }

                                     public static TestClass Create()
                                     {
                                         return new TestClass();
                                     }

                                     public void Reset()
                                     {
                                     }

                                     #endregion // Factories
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH7103StaticElementsMustAppearBeforeInstanceElementsAnalyzer.DiagnosticId, AnalyzerResources.RH7103MessageFormat));
    }

    #endregion // Tests
}