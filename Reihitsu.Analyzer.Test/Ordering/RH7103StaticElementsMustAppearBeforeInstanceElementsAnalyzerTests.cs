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
    /// Verifying the blank line that separated the two members survives the reorder under CRLF line endings,
    /// so the fix does not introduce mixed line endings while relocating the separator (issue #727)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task StaticMembersAreReportedAndFixedUnderCarriageReturnLineFeed()
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

        await Verify(NormalizeToCarriageReturnLineFeed(testCode),
                     NormalizeToCarriageReturnLineFeed(fixedCode),
                     Diagnostics(RH7103StaticElementsMustAppearBeforeInstanceElementsAnalyzer.DiagnosticId, AnalyzerResources.RH7103MessageFormat));
    }

    /// <summary>
    /// Verifying the blank line already above the moved member's documentation stays at its position instead of
    /// vanishing under the type's opening brace once the fix moves the documented member to the front (issue #727)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task StaticMemberWithDocumentationKeepsTheSeparatorWhenReordered()
    {
        const string testCode = """
                                public class TestClass
                                {
                                    public void Run()
                                    {
                                    }

                                    /// <summary>
                                    /// Creates a new instance
                                    /// </summary>
                                    public static void {|#0:Create|}()
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class TestClass
                                 {
                                     /// <summary>
                                     /// Creates a new instance
                                     /// </summary>
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
    /// Verifying the separator before a documented target member already survives the reorder today, because the
    /// blank-line boundary before a comment reinserts it independently of this fix; this locks the already-correct
    /// output in place so a change to the trivia split does not regress it (issue #727)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task SeparatorBeforeADocumentedTargetSurvivesTheReorder()
    {
        const string testCode = """
                                public class TestClass
                                {
                                    /// <summary>
                                    /// Runs the instance
                                    /// </summary>
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

                                     /// <summary>
                                     /// Runs the instance
                                     /// </summary>
                                     public void Run()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH7103StaticElementsMustAppearBeforeInstanceElementsAnalyzer.DiagnosticId, AnalyzerResources.RH7103MessageFormat));
    }

    /// <summary>
    /// Verifying the separator survives the reorder today when both members carry documentation, and continues to
    /// after the fix (issue #727)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task SeparatorSurvivesTheReorderWhenBothMembersAreDocumented()
    {
        const string testCode = """
                                public class TestClass
                                {
                                    /// <summary>
                                    /// Runs the instance
                                    /// </summary>
                                    public void Run()
                                    {
                                    }

                                    /// <summary>
                                    /// Creates a new instance
                                    /// </summary>
                                    public static void {|#0:Create|}()
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class TestClass
                                 {
                                     /// <summary>
                                     /// Creates a new instance
                                     /// </summary>
                                     public static void Create()
                                     {
                                     }

                                     /// <summary>
                                     /// Runs the instance
                                     /// </summary>
                                     public void Run()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH7103StaticElementsMustAppearBeforeInstanceElementsAnalyzer.DiagnosticId, AnalyzerResources.RH7103MessageFormat));
    }

    /// <summary>
    /// Verifying the separator stays at the position it already occupied when only the target member (not the
    /// moved member) was preceded by a blank line, instead of following the moved member to its new position
    /// (issue #727)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task SeparatorStaysAtTargetPositionWhenOnlyTheTargetHadABlankLineBefore()
    {
        const string testCode = """
                                public class TestClass
                                {
                                    private int _value;

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
                                     private int _value;

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
    /// Verifying that a separator between two unrelated members is relocated rather than lost when the target
    /// member is not the first member of the type, proving the fix repositions the existing separator instead of
    /// merely re-adding one that lands under the type's opening brace (issue #727)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task RelocationPreservesGapCountWhenTargetIsNotTheFirstMember()
    {
        const string testCode = """
                                public class TestClass
                                {
                                    private int _value;
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
                                     private int _value;
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
    /// Verifying every separator keeps its position when the move crosses another member and the crossed gaps are
    /// uniform, so the swap only permutes which member sits at each position (issue #727)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task CodeFixPreservesGapSequenceWhenMoveCrossesMultipleMembers()
    {
        const string testCode = """
                                public class TestClass
                                {
                                    public void Run()
                                    {
                                    }

                                    public void Reset()
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

                                     public void Reset()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH7103StaticElementsMustAppearBeforeInstanceElementsAnalyzer.DiagnosticId, AnalyzerResources.RH7103MessageFormat));
    }

    /// <summary>
    /// Verifying every separator keeps its position when the move crosses another member and the crossed gaps are
    /// not uniform, so the gap sequence is preserved rather than exchanged between the moved and target member
    /// (issue #727)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task CodeFixPreservesGapSequenceWhenCrossedGapsAreNotUniform()
    {
        const string testCode = """
                                public class TestClass
                                {
                                    public void Run()
                                    {
                                    }
                                    public void Reset()
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

                                     public void Reset()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH7103StaticElementsMustAppearBeforeInstanceElementsAnalyzer.DiagnosticId, AnalyzerResources.RH7103MessageFormat));
    }

    /// <summary>
    /// Verifying that several blank lines above the moved member collapse to exactly one at the relocated
    /// separator position, since the raw move relocates the whole gap and the formatter's own blank-line collapse
    /// still applies afterward (issue #727)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task MultipleBlankLinesAboveTheMovedMemberCollapseToOneAfterReordering()
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
    /// Verifying that a balanced <c>#if false</c> block sitting entirely inside the moved member's own leading
    /// trivia travels with the member intact, while the blank line preceding the directive stays positional
    /// (issue #727)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task CodeFixTravelsWithABalancedDisabledDirectiveBlockAboveTheMovedMember()
    {
        const string testCode = """
                                public class TestClass
                                {
                                    public void Run()
                                    {
                                    }

                                #if false
                                    public static void Old()
                                    {
                                    }
                                #endif
                                    public static void {|#0:Create|}()
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class TestClass
                                 {
                                 #if false
                                     public static void Old()
                                     {
                                     }
                                 #endif
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
    /// Verifying that Fix All keeps every separator at its position when two static members are reordered in the
    /// same document, so the gap sequence converges correctly instead of depending on application order (issue #727)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task FixAllPreservesGapSequenceAcrossMultipleDiagnosticsInOneDocument()
    {
        const string testCode = """
                                public class TestClass
                                {
                                    public void Run()
                                    {
                                    }

                                    public static void {|#0:CreateA|}()
                                    {
                                    }

                                    public static void {|#1:CreateB|}()
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class TestClass
                                 {
                                     public static void CreateA()
                                     {
                                     }

                                     public static void CreateB()
                                     {
                                     }

                                     public void Run()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode,
                     fixedCode,
                     test => test.NumberOfFixAllIterations = 2,
                     Diagnostics(RH7103StaticElementsMustAppearBeforeInstanceElementsAnalyzer.DiagnosticId, AnalyzerResources.RH7103MessageFormat, 2));
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
    /// Verifying no code fix is offered when the move would separate a preprocessor directive from its partner,
    /// leaving the region opened around the target member but closed after the moved member
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
    /// Verifying no code fix is offered when a preprocessor directive sits between the attribute list and the declaration keyword,
    /// since the directive attaches to a later token and moving the member would split the conditional-compilation pair
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NoCodeFixWhenDirectiveFollowsAttributeList()
    {
        const string testCode = """
                                public class TestClass
                                {
                                    public void Run()
                                    {
                                    }

                                    [System.Obsolete]
                                #if DEBUG
                                    public static void Create()
                                    {
                                    }
                                #endif
                                }
                                """;

        var actions = await GetCodeFixActionsAsync(testCode,
                                                   RH7103StaticElementsMustAppearBeforeInstanceElementsAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<MethodDeclarationSyntax>()
                                                               .Single(method => method.Identifier.ValueText == "Create")
                                                               .Identifier
                                                               .GetLocation(),
                                                   "DEBUG");

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifying the code fix stays available when a crossed member carries a conditional directive pair completely
    /// inside its own body, since the pair stays intact and the moved member is outside it before and after the move
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task CodeFixIsOfferedWhenCrossedMemberBodyContainsBalancedConditional()
    {
        const string testCode = """
                                public class TestClass
                                {
                                    public void Run()
                                    {
                                #if DEBUG
                                        System.Console.WriteLine();
                                #endif
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
                                 #if DEBUG
                                         System.Console.WriteLine();
                                 #endif
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH7103StaticElementsMustAppearBeforeInstanceElementsAnalyzer.DiagnosticId, AnalyzerResources.RH7103MessageFormat));
    }

    /// <summary>
    /// Verifying the code fix stays available when the crossed member's conditional directive pair is active,
    /// so the guard decides on the directive trivia rather than on whether the enclosed code is compiled
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task CodeFixIsOfferedWhenCrossedMemberBodyContainsActiveBalancedConditional()
    {
        const string testCode = """
                                public class TestClass
                                {
                                    public void Run()
                                    {
                                #if DEBUG
                                        System.Console.WriteLine();
                                #endif
                                    }

                                    public static void Create()
                                    {
                                    }
                                }
                                """;

        var actions = await GetCodeFixActionsAsync(testCode,
                                                   RH7103StaticElementsMustAppearBeforeInstanceElementsAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<MethodDeclarationSyntax>()
                                                               .Single(method => method.Identifier.ValueText == "Create")
                                                               .Identifier
                                                               .GetLocation(),
                                                   "DEBUG");

        Assert.HasCount(1, actions);
    }

    /// <summary>
    /// Verifying no code fix is offered when a preprocessor directive sits between two modifiers of the moved member
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NoCodeFixWhenDirectiveFollowsModifier()
    {
        const string testCode = """
                                public class TestClass
                                {
                                    public void Run()
                                    {
                                    }

                                    public
                                #if DEBUG
                                    static void Create()
                                    {
                                    }
                                #endif
                                }
                                """;

        var actions = await GetCodeFixActionsAsync(testCode,
                                                   RH7103StaticElementsMustAppearBeforeInstanceElementsAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<MethodDeclarationSyntax>()
                                                               .Single(method => method.Identifier.ValueText == "Create")
                                                               .Identifier
                                                               .GetLocation(),
                                                   "DEBUG");

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