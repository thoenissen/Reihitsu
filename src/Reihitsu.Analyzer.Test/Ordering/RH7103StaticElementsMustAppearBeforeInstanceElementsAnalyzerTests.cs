using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Organization;
using Reihitsu.Analyzer.Rules.Organization;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Ordering;

/// <summary>
/// Test methods for <see cref="RH7103StaticElementsMustAppearBeforeInstanceElementsAnalyzer"/> and <see cref="RH7103StaticElementsMustAppearBeforeInstanceElementsCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH7103StaticElementsMustAppearBeforeInstanceElementsAnalyzerTests : BatchCodeFixTestsBase<RH7103StaticElementsMustAppearBeforeInstanceElementsAnalyzer, RH7103StaticElementsMustAppearBeforeInstanceElementsCodeFixProvider>
{
    #region Constants

    /// <summary>
    /// Input of the SHARED-FIX-003 RH7103 example, in which <c>_a</c> crosses only the instance field and <c>_b</c>
    /// would cross <c>_a</c>
    /// </summary>
    private const string CrossesOnlyAnInstanceFieldTestCode = """
                                                              public class C
                                                              {
                                                                  private int _instance;
                                                                  private static int _a = 1;
                                                                  private static int _b = _a;
                                                              }
                                                              """;

    /// <summary>
    /// Result of the SHARED-FIX-003 RH7103 example after one application of the action registered for <c>_a</c>
    /// </summary>
    private const string CrossesOnlyAnInstanceFieldFixedCode = """
                                                               public class C
                                                               {
                                                                   private static int _a = 1;
                                                                   private int _instance;
                                                                   private static int _b = _a;
                                                               }
                                                               """;

    #endregion // Constants

    #region Tests

    /// <summary>
    /// Verifying static members are reported and fixed when they appear after instance members of the same group
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7103-001")]
    [Requirement("RH7103-005")]
    [Requirement("RH7103-006")]
    [Requirement("SHARED-ORDER-001")]
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
    /// so the fix does not introduce mixed line endings while relocating the separator
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7103-006")]
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
    /// vanishing under the type's opening brace once the fix moves the documented member to the front
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7103-006")]
    [Requirement("SHARED-ORDER-002")]
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
    /// output in place so a change to the trivia split does not regress it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7103-006")]
    [Requirement("SHARED-ORDER-002")]
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
    /// after the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7103-006")]
    [Requirement("SHARED-ORDER-002")]
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
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7103-006")]
    [Requirement("SHARED-ORDER-002")]
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
    /// merely re-adding one that lands under the type's opening brace
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7103-006")]
    [Requirement("SHARED-ORDER-002")]
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
    /// uniform, so the swap only permutes which member sits at each position
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7103-006")]
    [Requirement("SHARED-ORDER-002")]
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
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7103-006")]
    [Requirement("SHARED-ORDER-002")]
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
    /// still applies afterward
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7103-006")]
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
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7103-006")]
    [Requirement("SHARED-ORDER-006")]
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
    /// Verifying destructors do not crash analysis
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7103-002")]
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
    [Requirement("RH7103-007")]
    [Requirement("SHARED-ORDER-007")]
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
    [Requirement("RH7103-007")]
    [Requirement("SHARED-ORDER-007")]
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
    [Requirement("RH7103-007")]
    [Requirement("SHARED-ORDER-004")]
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
    [Requirement("RH7103-007")]
    [Requirement("SHARED-ORDER-003")]
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
    [Requirement("RH7103-006")]
    [Requirement("RH7103-007")]
    [Requirement("SHARED-ORDER-006")]
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
    [Requirement("RH7103-006")]
    [Requirement("RH7103-007")]
    [Requirement("SHARED-ORDER-006")]
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

        var actions = await GetCodeFixActionsAsync(testCode,
                                                   RH7103StaticElementsMustAppearBeforeInstanceElementsAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<MethodDeclarationSyntax>()
                                                               .Single(method => method.Identifier.ValueText == "Create")
                                                               .Identifier
                                                               .GetLocation(),
                                                   "DEBUG");

        Assert.HasCount(1, actions);

        var operations = await actions[0].GetOperationsAsync(CancellationToken.None);
        var changedDocument = operations.OfType<ApplyChangesOperation>().Single().ChangedSolution.Projects.Single().Documents.Single();
        var changedText = (await changedDocument.GetTextAsync(CancellationToken.None)).ToString();

        Assert.AreEqual(fixedCode, changedText);
    }

    /// <summary>
    /// Verifying no code fix is offered when a preprocessor directive sits between two modifiers of the moved member
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7103-007")]
    [Requirement("SHARED-ORDER-003")]
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
    /// Verifying no code fix is offered when the move would cross a static initializer of another accessibility group,
    /// since the crossed member is not compared with the moved one but its initializer still runs in declaration order
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7103-007")]
    [Requirement("SHARED-ORDER-007")]
    public async Task NoCodeFixWhenMoveCrossesStaticInitializerOfAnotherAccessibility()
    {
        const string testCode = """
                                public class C
                                {
                                    private int _instance;
                                    public static int A = 1;
                                    private static int {|#0:_b|} = A;
                                }
                                """;

        // The fixed code equals the input, so the verifier also asserts that no code fix is offered
        await Verify(testCode, testCode, Diagnostics(RH7103StaticElementsMustAppearBeforeInstanceElementsAnalyzer.DiagnosticId, AnalyzerResources.RH7103MessageFormat));
    }

    /// <summary>
    /// Verifying no code fix is offered when a position-sensitive directive sits in the leading trivia of the moved
    /// member, since moving the member across or with the directive changes which code the directive covers
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7103-007")]
    [Requirement("SHARED-ORDER-005")]
    public async Task NoCodeFixWhenPragmaDirectivePrecedesMovedMember()
    {
        const string testCode = """
                                public class C
                                {
                                    public void Run()
                                    {
                                    }

                                #pragma warning disable CS0168
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
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifying RH7103 does not compare a static member against an instance member when they live in separate regions
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7103-004")]
    [Requirement("SHARED-ORDER-004")]
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
    [Requirement("RH7103-004")]
    [Requirement("SHARED-ORDER-004")]
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
    [Requirement("RH7103-004")]
    [Requirement("SHARED-ORDER-004")]
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
    [Requirement("RH7103-004")]
    [Requirement("SHARED-ORDER-004")]
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
    [Requirement("RH7103-004")]
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
    [Requirement("RH7103-004")]
    [Requirement("RH7103-006")]
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

                                     #endregion

                                     #region Factories

                                     public int Counter { get; set; }

                                     public static TestClass Create()
                                     {
                                         return new TestClass();
                                     }

                                     public void Reset()
                                     {
                                     }

                                     #endregion
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH7103StaticElementsMustAppearBeforeInstanceElementsAnalyzer.DiagnosticId, AnalyzerResources.RH7103MessageFormat));
    }

    /// <summary>
    /// Verifying a static member is compared only with instance members of the same kind and the same accessibility,
    /// so a static field and a private static method after a public instance method are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7103-001")]
    [Requirement("RH7103-002")]
    [Requirement("RH7103-003")]
    [Requirement("RH7103-006")]
    public async Task OnlyStaticMembersOfTheSameKindAndAccessibilityAreReported()
    {
        const string testCode = """
                                public class TestClass
                                {
                                    public void Run()
                                    {
                                    }

                                    public static int Count;

                                    private static void Helper()
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

                                     public static int Count;

                                     private static void Helper()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH7103StaticElementsMustAppearBeforeInstanceElementsAnalyzer.DiagnosticId, AnalyzerResources.RH7103MessageFormat));
    }

    /// <summary>
    /// Verifying a static abstract interface member counts as static and is reported after an instance member of the
    /// same kind
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7103-001")]
    [Requirement("RH7103-002")]
    public async Task StaticAbstractInterfaceMemberIsReported()
    {
        const string testCode = """
                                public interface ITest
                                {
                                    void Run();

                                    static abstract void {|#0:Create|}();
                                }
                                """;

        const string fixedCode = """
                                 public interface ITest
                                 {
                                     static abstract void Create();

                                     void Run();
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH7103StaticElementsMustAppearBeforeInstanceElementsAnalyzer.DiagnosticId, AnalyzerResources.RH7103MessageFormat));
    }

    /// <summary>
    /// Verifying a private static method after a public instance method is not reported, because the two members
    /// differ in accessibility
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7103-003")]
    public async Task NoDiagnosticWhenStaticMemberHasDifferentAccessibility()
    {
        const string testCode = """
                                public class TestClass
                                {
                                    public void Run()
                                    {
                                    }

                                    private static void Helper()
                                    {
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying both misplaced static fields are reported, and no code action is registered for the one whose move
    /// would run its initializer before the initializer it reads
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-003")]
    public async Task NoCodeActionIsRegisteredWhenTheMoveWouldChangeTheRunTimeValueOfAField()
    {
        const string testCode = """
                                public class C
                                {
                                    private int _instance;
                                    private static int {|RH7103:_a|} = 1;
                                    private static int {|RH7103:_b|} = _a;
                                }
                                """;

        await Verify(testCode);

        var actions = await GetCodeFixActionsAsync(testCode.Replace("{|RH7103:", string.Empty).Replace("|}", string.Empty),
                                                   RH7103StaticElementsMustAppearBeforeInstanceElementsAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<VariableDeclaratorSyntax>()
                                                               .Single(declarator => declarator.Identifier.ValueText == "_b")
                                                               .Identifier
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifying exactly one code action is registered for the static field that crosses only an instance field, whose
    /// initialization runs at a different time, and that it moves only that field and leaves the reading field reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-003")]
    public async Task CodeActionIsRegisteredWhenTheMoveCrossesOnlyAnInstanceField()
    {
        await VerifyTheActionForTheFieldThatCrossesOnlyAnInstanceField(CrossesOnlyAnInstanceFieldTestCode, CrossesOnlyAnInstanceFieldFixedCode);
    }

    /// <summary>
    /// Verifying exactly one code action is registered for the static field that crosses only an instance field when
    /// the document uses CRLF line endings, and that it moves only that field and keeps every line ending
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-003")]
    public async Task CodeActionIsRegisteredWhenTheMoveCrossesOnlyAnInstanceFieldWithCarriageReturnLineFeed()
    {
        await VerifyTheActionForTheFieldThatCrossesOnlyAnInstanceField(NormalizeToCarriageReturnLineFeed(CrossesOnlyAnInstanceFieldTestCode),
                                                                       NormalizeToCarriageReturnLineFeed(CrossesOnlyAnInstanceFieldFixedCode));
    }

    /// <summary>
    /// Verifying one Fix All application moves the static field whose fix is offered on the starting code, equals the
    /// result of that single fix, and leaves the field whose fix the guard withholds reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-002")]
    public async Task OneFixAllApplicationCorrectsOnlyTheDiagnosticsWhoseFixIsOfferedOnTheStartingCode()
    {
        await Verify(CrossesOnlyAnInstanceFieldTestCode.Replace("_a = 1", "{|RH7103:_a|} = 1", StringComparison.Ordinal)
                                                       .Replace("_b = _a", "{|RH7103:_b|} = _a", StringComparison.Ordinal),
                     CrossesOnlyAnInstanceFieldFixedCode.Replace("_b = _a", "{|RH7103:_b|} = _a", StringComparison.Ordinal),
                     test =>
                     {
                         // The field whose fix is withheld on the starting code is marked in the expected source
                         test.FixedState.MarkupHandling = MarkupMode.Allow;

                         // The single fix offered on the starting code is applied once, without the fix its result releases
                         test.CodeFixTestBehaviors |= CodeFixTestBehaviors.FixOne;
                         test.NumberOfIncrementalIterations = 1;
                         test.NumberOfFixAllIterations = 1;
                     });
    }

    /// <summary>
    /// Verifies the specification example of the region scope: members are compared only within their own top-level
    /// region, so a static method after an instance method of another top-level region is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-ORDER-004")]
    public async Task MembersAreComparedOnlyWithinTheirOwnTopLevelRegion()
    {
        const string testCode = """
                                public class C
                                {
                                    #region Instance

                                    private int _a;

                                    public void Run()
                                    {
                                    }

                                    #endregion

                                    #region Shared

                                    public int B;

                                    public static void Create()
                                    {
                                    }

                                    #endregion
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that a static member in a region nested in a top-level region is compared with the instance members of
    /// that top-level region outside the nested region
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-ORDER-004")]
    public async Task StaticMemberInANestedRegionIsComparedWithTheRestOfItsTopLevelRegion()
    {
        const string testCode = """
                                public class C
                                {
                                    #region Members

                                    public void Run()
                                    {
                                    }

                                    #region Factories

                                    public static void {|#0:Create|}()
                                    {
                                    }

                                    #endregion

                                    #endregion
                                }
                                """;

        await Verify(testCode, Diagnostics(RH7103StaticElementsMustAppearBeforeInstanceElementsAnalyzer.DiagnosticId, AnalyzerResources.RH7103MessageFormat));
    }

    /// <summary>
    /// Verifies that a static member outside every region is compared with an instance member outside every region,
    /// also when a top-level region lies between them
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-ORDER-004")]
    public async Task StaticMemberOutsideEveryRegionIsComparedWithInstanceMembersOutsideEveryRegion()
    {
        const string testCode = """
                                public class C
                                {
                                    public void Run()
                                    {
                                    }

                                    #region Lifecycle

                                    public void Stop()
                                    {
                                    }

                                    #endregion

                                    public static void {|#0:Create|}()
                                    {
                                    }
                                }
                                """;

        await Verify(testCode, Diagnostics(RH7103StaticElementsMustAppearBeforeInstanceElementsAnalyzer.DiagnosticId, AnalyzerResources.RH7103MessageFormat));
    }

    /// <summary>
    /// Verifies the specification example of a rule about the order of members: the static method is reported and moved,
    /// although the formatter never reorders members
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-DIAG-010")]
    public async Task OrderRuleReportsAShapeTheFormatterKeeps()
    {
        const string testCode = """
                                public class C
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
                                 public class C
                                 {
                                     public static void Create()
                                     {
                                     }

                                     public void Run()
                                     {
                                     }
                                 }
                                 """;

        AssertFormattingChangesNothing(testCode.Replace("{|#0:", string.Empty).Replace("|}", string.Empty));

        await Verify(testCode, fixedCode, Diagnostics(RH7103StaticElementsMustAppearBeforeInstanceElementsAnalyzer.DiagnosticId, AnalyzerResources.RH7103MessageFormat));
    }

    /// <summary>
    /// Verifies the specification example of a documentation comment behind code whose declaration a fix moves: the
    /// <c>///</c> comment behind the closing brace of <c>Run</c> documents <c>Create</c>, the static method is reported,
    /// and the fix carries the comment with <c>Create</c> in front of <c>Run</c> instead of leaving it behind the brace,
    /// so the comment still documents <c>Create</c> after the fix. The fix runs with documentation comments parsed
    /// (documentation mode <c>Parse</c>), the mode in which the comment documents <c>Create</c>; this input has no
    /// preprocessor directive between the comment and <c>Create</c>. Where the comment starts after the move, the
    /// layout of its lines, and the layout of the rest of the type are not asserted
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-DOC-004")]
    [Requirement("SHARED-FIX-003")]
    [Requirement("SHARED-ORDER-002")]
    public async Task FixCarriesADocumentationCommentBehindCodeWithTheMovedDeclaration(string endOfLine)
    {
        const string testCode = """
                                public class C
                                {
                                    public void Run()
                                    {
                                    } /// <summary>Creates</summary>
                                    public static void {|RH7103:Create|}()
                                    {
                                    }
                                }
                                """;

        await Verify(testCode);

        var plainCode = testCode.Replace("{|RH7103:", string.Empty)
                                .Replace("|}", string.Empty)
                                .Replace("\r\n", "\n")
                                .Replace("\n", endOfLine);

        var fixedCode = await ApplyCodeFixAsync(plainCode);

        var syntaxTree = CSharpSyntaxTree.ParseText(fixedCode, new CSharpParseOptions(documentationMode: DocumentationMode.Parse), cancellationToken: CancellationToken.None);
        var root = await syntaxTree.GetRootAsync(CancellationToken.None);
        var methods = root.DescendantNodes()
                          .OfType<MethodDeclarationSyntax>()
                          .ToList();

        Assert.HasCount(2, methods);
        Assert.AreEqual("Create", methods[0].Identifier.ValueText, "The fix must move Create in front of Run.");
        Assert.AreEqual("Run", methods[1].Identifier.ValueText, "The fix must move Create in front of Run.");

        var createDocumentation = methods[0].GetLeadingTrivia()
                                            .Where(trivia => trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia))
                                            .ToList();

        Assert.HasCount(1, createDocumentation, "The documentation comment must travel with Create.");

        var summary = createDocumentation[0].GetStructure()
                                            .DescendantNodes()
                                            .OfType<XmlElementSyntax>()
                                            .Single(element => element.StartTag.Name.LocalName.ValueText == "summary");
        var summaryText = string.Concat(summary.Content.Select(content => content.ToString()))
                                .Replace("///", string.Empty)
                                .Trim();

        Assert.AreEqual("Creates", summaryText, "The comment that travels must be the summary of Create.");

        var commentsInRun = methods[1].DescendantTrivia()
                                      .Where(trivia => trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia)
                                                       || trivia.IsKind(SyntaxKind.SingleLineCommentTrivia))
                                      .ToList();

        Assert.IsEmpty(commentsInRun, "No comment may stay behind the closing brace of Run.");
        Assert.AreEqual(fixedCode.IndexOf("Creates", StringComparison.Ordinal), fixedCode.LastIndexOf("Creates", StringComparison.Ordinal), "The fix must keep exactly one copy of the comment.");
    }

    /// <summary>
    /// Verifies the specification example of a documentation comment behind code, a conditional pair, and the
    /// declaration a fix moves: with documentation comments parsed (documentation mode <c>Parse</c>), the <c>///</c>
    /// comment behind the closing brace of <c>Run</c> and the <c>#if DEBUG</c>/<c>#endif</c> pair below it stand in the
    /// leading trivia of <c>Create</c>; the static method is reported, and the fix carries the comment and the pair
    /// with <c>Create</c> in front of <c>Run</c>, in their order, so the comment still documents <c>Create</c>. Where
    /// the comment starts after the move and the layout of the rest of the type are not asserted
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-DOC-004")]
    [Requirement("SHARED-FIX-003")]
    [Requirement("SHARED-ORDER-006")]
    public async Task FixCarriesADocumentationCommentBehindCodeAndAConditionalPairWithTheMovedDeclaration(string endOfLine)
    {
        const string testCode = """
                                public class C
                                {
                                    public void Run()
                                    {
                                    } /// <summary>Creates</summary>
                                #if DEBUG
                                #endif
                                    public static void {|RH7103:Create|}()
                                    {
                                    }
                                }
                                """;

        await Verify(testCode);

        var plainCode = testCode.Replace("{|RH7103:", string.Empty)
                                .Replace("|}", string.Empty)
                                .Replace("\r\n", "\n")
                                .Replace("\n", endOfLine);

        var fixedCode = await ApplyCodeFixAsync(plainCode);

        var syntaxTree = CSharpSyntaxTree.ParseText(fixedCode, new CSharpParseOptions(documentationMode: DocumentationMode.Parse), cancellationToken: CancellationToken.None);
        var root = await syntaxTree.GetRootAsync(CancellationToken.None);
        var methods = root.DescendantNodes()
                          .OfType<MethodDeclarationSyntax>()
                          .ToList();

        Assert.HasCount(2, methods);
        Assert.AreEqual("Create", methods[0].Identifier.ValueText, "The fix must move Create in front of Run.");
        Assert.AreEqual("Run", methods[1].Identifier.ValueText, "The fix must move Create in front of Run.");

        var createTrivia = methods[0].GetLeadingTrivia()
                                     .Where(trivia => trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia)
                                                      || trivia.IsKind(SyntaxKind.IfDirectiveTrivia)
                                                      || trivia.IsKind(SyntaxKind.EndIfDirectiveTrivia))
                                     .Select(trivia => trivia.Kind())
                                     .ToList();

        Assert.AreSequenceEqual([SyntaxKind.SingleLineDocumentationCommentTrivia, SyntaxKind.IfDirectiveTrivia, SyntaxKind.EndIfDirectiveTrivia],
                                createTrivia.ToArray(),
                                "The comment and the conditional pair must travel with Create, in their order.");

        var summary = methods[0].GetLeadingTrivia()
                                .Single(trivia => trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia))
                                .GetStructure()
                                .DescendantNodes()
                                .OfType<XmlElementSyntax>()
                                .Single(element => element.StartTag.Name.LocalName.ValueText == "summary");
        var summaryText = string.Concat(summary.Content.Select(content => content.ToString()))
                                .Replace("///", string.Empty)
                                .Trim();

        Assert.AreEqual("Creates", summaryText, "The comment that travels must be the summary of Create.");

        var triviaInRun = methods[1].DescendantTrivia(descendIntoTrivia: true)
                                    .Where(trivia => trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia)
                                                     || trivia.IsKind(SyntaxKind.SingleLineCommentTrivia)
                                                     || trivia.IsDirective)
                                    .ToList();

        Assert.IsEmpty(triviaInRun, "No comment and no directive may stay with Run.");
        Assert.AreEqual(fixedCode.IndexOf("Creates", StringComparison.Ordinal), fixedCode.LastIndexOf("Creates", StringComparison.Ordinal), "The fix must keep exactly one copy of the comment.");
        Assert.AreEqual(fixedCode.IndexOf("#if DEBUG", StringComparison.Ordinal), fixedCode.LastIndexOf("#if DEBUG", StringComparison.Ordinal), "The fix must keep exactly one copy of the #if directive.");
        Assert.AreEqual(fixedCode.IndexOf("#endif", StringComparison.Ordinal), fixedCode.LastIndexOf("#endif", StringComparison.Ordinal), "The fix must keep exactly one copy of the #endif directive.");
    }

    /// <summary>
    /// Verifies the specification example of a <c>///</c> comment behind code when documentation comments are not
    /// parsed (documentation mode <c>None</c>): the comment is ordinary trailing trivia of the closing brace of
    /// <c>Run</c>, the static method <c>Create</c> is reported, and the fix moves <c>Create</c> in front of <c>Run</c>
    /// and leaves the comment behind the closing brace of <c>Run</c>, as it leaves a <c>//</c> comment there. The fix
    /// runs under the documentation mode <c>None</c>, and the fixed code is compared in full, under LF and CRLF; the
    /// input has no blank line between the two methods, and the move adds none, since the separators stay where they
    /// were
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-DOC-004")]
    [Requirement("SHARED-ORDER-002")]
    public async Task FixLeavesACommentBehindCodeWhereItStandsWhenDocumentationCommentsAreNotParsed(string endOfLine)
    {
        const string testCode = """
                                public class C
                                {
                                    public void Run()
                                    {
                                    } /// <summary>Creates</summary>
                                    public static void {|RH7103:Create|}()
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class C
                                 {
                                     public static void Create()
                                     {
                                     }
                                     public void Run()
                                     {
                                     } /// <summary>Creates</summary>
                                 }
                                 """;

        await Verify(testCode.Replace("\r\n", "\n").Replace("\n", endOfLine),
                     fixedCode.Replace("\r\n", "\n").Replace("\n", endOfLine),
                     test =>
                     {
                         test.SolutionTransforms.Add(ApplyDocumentationModeNoneToTestProject);
                     });
    }

    /// <summary>
    /// Verifying the counterpart of the shared specification example: without <c>DEBUG</c> defined, the move would cross
    /// an initialized static field of excluded code, whose initializer a build with <c>DEBUG</c> would then run after the
    /// moved one, so no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-ORDER-007")]
    [Requirement("RH7103-007")]
    public async Task NoCodeFixWhenTheMoveCrossesAnInitializedStaticFieldOfExcludedCode()
    {
        const string source = """
                              public class C
                              {
                                  private int _instance;
                              #if DEBUG
                                  private static int _c = Log("c");
                              #endif
                                  private int _other;
                                  private static int {|RH7103:_b|} = Log("b");

                                  private static int Log(string name) => 0;
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifying the counterpart of the shared specification example: excluded code in front of the moved static field
    /// moves with it, so the order of the initializers stays the same in every build, and the fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-ORDER-007")]
    public async Task FixMovesExcludedCodeInFrontOfTheMovedStaticFieldWithIt()
    {
        const string testCode = """
                                public class C
                                {
                                    private int _instance;
                                    private int _other;
                                #if DEBUG
                                    private static int _c = Log("c");
                                #endif
                                    private static int {|RH7103:_b|} = Log("b");

                                    private static int Log(string name)
                                    {
                                        return 0;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class C
                                 {
                                 #if DEBUG
                                     private static int _c = Log("c");
                                 #endif
                                     private static int _b = Log("b");
                                     private int _instance;
                                     private int _other;

                                     private static int Log(string name)
                                     {
                                         return 0;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifying that an instance field of excluded code in front of the moved static field moves with it: without
    /// <c>DEBUG</c> defined, the move would carry it in front of an instance field of a struct, whose layout a build with
    /// <c>DEBUG</c> would then change, so no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-003")]
    [Requirement("SHARED-ORDER-007")]
    [Requirement("RH7103-007")]
    public async Task NoCodeFixWhenExcludedCodeInFrontOfTheMovedStaticFieldWouldReorderTheInstanceFieldsOfAStruct()
    {
        const string source = """
                              public struct S
                              {
                                  private int _x;
                              #if DEBUG
                                  private int _y;
                              #endif
                                  private static int {|RH7103:_s|};
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifying the other side of the condition: in a class, whose field order sets no layout, an instance field of
    /// excluded code in front of the moved static field moves with it, and the fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-003")]
    [Requirement("SHARED-ORDER-007")]
    public async Task FixCarriesAnInstanceFieldOfExcludedCodeInFrontOfTheMovedStaticFieldOfAClass()
    {
        const string testCode = """
                                public class S
                                {
                                    private int _x;
                                #if DEBUG
                                    private int _y;
                                #endif
                                    private static int {|RH7103:_s|};
                                }
                                """;

        const string fixedCode = """
                                 public class S
                                 {
                                 #if DEBUG
                                     private int _y;
                                 #endif
                                     private static int _s;
                                     private int _x;
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifying the counterpart of the shared specification example: without excluded code in front of it, the moved
    /// static field crosses the instance field of a struct but reorders no instance field, and the fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7103-007")]
    public async Task FixMovesAStaticFieldOfAStructAcrossAnInstanceField()
    {
        const string testCode = """
                                public struct S
                                {
                                    private int _x;
                                    private static int {|RH7103:_s|};
                                }
                                """;

        const string fixedCode = """
                                 public struct S
                                 {
                                     private static int _s;
                                     private int _x;
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifying the other side of the crossing condition: an instance field of excluded code in front of the moved
    /// static method of a struct moves with it, but the move crosses only an instance method and reorders no instance
    /// field, so the fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-ORDER-007")]
    [Requirement("RH7103-007")]
    public async Task FixCarriesAnInstanceFieldOfExcludedCodeInFrontOfTheMovedStaticMethodOfAStructAcrossOnlyAMethod()
    {
        const string testCode = """
                                public struct S
                                {
                                    public void M()
                                    {
                                    }

                                #if DEBUG
                                    private int _y;
                                #endif
                                    public static void {|RH7103:N|}()
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public struct S
                                 {
                                 #if DEBUG
                                     private int _y;
                                 #endif
                                     public static void N()
                                     {
                                     }

                                     public void M()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifying the fix, which formats the whole type, leaves a documented field declaration with several declarators
    /// as one declaration when the documentation mode is Parse, so its documentation keeps documenting every field
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("FMT-STRUCT-030")]
    [Requirement("SHARED-FIX-003")]
    public async Task CodeFixLeavesADocumentedCombinedFieldOfTheTypeUnsplit()
    {
        const string testCode = """
                                public class C
                                {
                                    /// <summary>
                                    /// The coordinates
                                    /// </summary>
                                    public int X, Y;

                                    public void Run()
                                    {
                                    }

                                    public static void {|#0:Create|}()
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class C
                                 {
                                     /// <summary>
                                     /// The coordinates
                                     /// </summary>
                                     public int X, Y;

                                     public static void Create()
                                     {
                                     }

                                     public void Run()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode,
                     fixedCode,
                     test => test.SolutionTransforms.Add(ApplyDocumentationModeParseToTestProject),
                     Diagnostics(RH7103StaticElementsMustAppearBeforeInstanceElementsAnalyzer.DiagnosticId, AnalyzerResources.RH7103MessageFormat));
    }

    /// <summary>
    /// Verifying the fix leaves a documented field declaration with several declarators as one declaration under CRLF
    /// line endings when the documentation mode is Parse
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("FMT-STRUCT-030")]
    [Requirement("SHARED-FIX-003")]
    public async Task CodeFixLeavesADocumentedCombinedFieldOfTheTypeUnsplitUnderCarriageReturnLineFeed()
    {
        const string testCode = """
                                public class C
                                {
                                    /// <summary>
                                    /// The coordinates
                                    /// </summary>
                                    public int X, Y;

                                    public void Run()
                                    {
                                    }

                                    public static void {|#0:Create|}()
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class C
                                 {
                                     /// <summary>
                                     /// The coordinates
                                     /// </summary>
                                     public int X, Y;

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
                     test => test.SolutionTransforms.Add(ApplyDocumentationModeParseToTestProject),
                     Diagnostics(RH7103StaticElementsMustAppearBeforeInstanceElementsAnalyzer.DiagnosticId, AnalyzerResources.RH7103MessageFormat));
    }

    /// <summary>
    /// Verifying that a static member whose <c>static</c> modifier only some configurations compile is not reported,
    /// because a conditional directive lies in the modifier list the rule judges
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-DIAG-008")]
    public async Task StaticMemberWhoseStaticModifierAnIfSplitsIsNotReported(string endOfLine)
    {
        const string source = """
                              public struct S
                              {
                                  private int _a;
                                  private
                              #if !DEBUG
                                      static
                              #endif
                                      int _b;
                              }
                              """;

        await Verify(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying the other side of the judged span: a conditional directive in the initializer of the static member, not in
    /// its modifiers, keeps it reported; the group stands between two tokens of the moved declaration, at no boundary of
    /// whole units, so no code fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-DIAG-008")]
    [Requirement("RH7103-007")]
    [Requirement("SHARED-ORDER-008")]
    public async Task NoCodeFixWhenTheMovedStaticFieldHasAnInitializerOnlyInAnotherConfiguration(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private int _i;
                                  public static int Shared = Log("p");
                                  private static int {|RH7103:_s|}
                              #if DEBUG
                                      = Log("s")
                              #endif
                                      ;

                                  private static int Log(string name)
                                  {
                                      return 0;
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying that a conditional group in front of a crossed field's first token whose branches hold its modifiers,
    /// which are neither whole members nor whole attribute lists, withholds the fix
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH7103-007")]
    [Requirement("SHARED-ORDER-008")]
    public async Task NoCodeFixWhenACrossedFieldIsStaticOnlyInAGroupInFrontOfItsFirstToken(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private int _a;
                              #if DEBUG
                                  private static
                              #else
                                  private
                              #endif
                                  int _b = Log("b");
                                  private static int {|RH7103:_c|} = Log("c");

                                  private static int Log(string name)
                                  {
                                      return 0;
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying that a conditional group in front of a moved static method whose branches each hold a whole attribute list
    /// wraps whole attribute lists, so the group moves with the method
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH7103-007")]
    [Requirement("SHARED-ORDER-008")]
    public async Task FixMovesAStaticMethodWithAnExcludedAttributeWithANamedArgument(string endOfLine)
    {
        const string source = """
                              using System.Runtime.InteropServices;

                              public class C
                              {
                                  private void Run()
                                  {
                                  }

                                  private int _b = 2;

                              #if DEBUG
                                  [DllImport("a", EntryPoint = "b")]
                              #else
                                  [DllImport("a")]
                              #endif
                                  private static extern void {|RH7103:M|}();
                              }
                              """;

        const string fixedSource = """
                                   using System.Runtime.InteropServices;

                                   public class C
                                   {
                                   #if DEBUG
                                       [DllImport("a", EntryPoint = "b")]
                                   #else
                                       [DllImport("a")]
                                   #endif
                                       private static extern void M();

                                       private void Run()
                                       {
                                       }

                                       private int _b = 2;
                                   }
                                   """;

        await Verify(WithEndOfLine(source, endOfLine), WithEndOfLine(fixedSource, endOfLine));
    }

    /// <summary>
    /// Verifying that a conditional group of whole accessors without <c>field</c> in a moved static property wraps whole
    /// units, also when the excluded accessor assigns a field, so moving the property across a static initialized field is
    /// offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH7103-007")]
    [Requirement("SHARED-ORDER-008")]
    public async Task FixMovesAStaticPropertyWithAnAssignmentInAnExcludedAccessor(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private static int _v;

                                  private int Instance { get; set; }

                                  public static int Counter = Log("c");

                                  private static int {|RH7103:Value|}
                                  {
                                      get
                                      {
                                          return _v;
                                      }
                              #if DEBUG
                                      set
                                      {
                                          _v = value;
                                      }
                              #endif
                                  }

                                  private static int Log(string name)
                                  {
                                      return 0;
                                  }
                              }
                              """;

        const string fixedSource = """
                                   public class C
                                   {
                                       private static int _v;

                                       private static int Value
                                       {
                                           get
                                           {
                                               return _v;
                                           }
                                   #if DEBUG
                                           set
                                           {
                                               _v = value;
                                           }
                                   #endif
                                       }

                                       private int Instance { get; set; }

                                       public static int Counter = Log("c");

                                       private static int Log(string name)
                                       {
                                           return 0;
                                       }
                                   }
                                   """;

        await Verify(WithEndOfLine(source, endOfLine), WithEndOfLine(fixedSource, endOfLine));
    }

    /// <summary>
    /// Verifying that a conditional group of whole accessors without <c>field</c> in a moved static property wraps whole
    /// units, so moving the property across a static initialized field is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH7103-007")]
    [Requirement("SHARED-ORDER-008")]
    public async Task FixMovesAStaticPropertyWithAnExcludedAccessor(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private static int _v;

                                  private int Instance { get; set; }

                                  public static int Counter = Log("c");

                                  private static int {|RH7103:Value|}
                                  {
                                      get
                                      {
                                          return _v;
                                      }
                              #if DEBUG
                                      set
                                      {
                                          Log("v");
                                      }
                              #endif
                                  }

                                  private static int Log(string name)
                                  {
                                      return 0;
                                  }
                              }
                              """;

        const string fixedSource = """
                                   public class C
                                   {
                                       private static int _v;

                                       private static int Value
                                       {
                                           get
                                           {
                                               return _v;
                                           }
                                   #if DEBUG
                                           set
                                           {
                                               Log("v");
                                           }
                                   #endif
                                       }

                                       private int Instance { get; set; }

                                       public static int Counter = Log("c");

                                       private static int Log(string name)
                                       {
                                           return 0;
                                       }
                                   }
                                   """;

        await Verify(WithEndOfLine(source, endOfLine), WithEndOfLine(fixedSource, endOfLine));
    }

    /// <summary>
    /// Verifying that a conditional group that opens inside the initializer of the field in front of a crossed field, and
    /// whose branches end that declaration and begin the next one, withholds the fix
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH7103-007")]
    [Requirement("SHARED-ORDER-008")]
    public async Task NoCodeFixWhenACrossedFieldIsStaticOnlyInAnExcludedBranchThatOpensInTheInitializerOfThePreviousField(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private int _a =
                              #if DEBUG
                                      1;
                                  private static
                              #else
                                      2;
                              #endif
                                  int _b = Log("b");
                                  private static int {|RH7103:_c|} = Log("c");

                                  private static int Log(string name)
                                  {
                                      return 0;
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying the SHARED-ORDER-008 example of a group of whole members in the moved member's leading trivia: the group
    /// moves with <c>Create</c>
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH7103-007")]
    [Requirement("SHARED-ORDER-008")]
    public async Task FixMovesAMemberWithAGroupOfWholeMembersInItsLeadingTrivia(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  public void Run()
                                  {
                                  }

                              #if DEBUG
                                  public void Trace()
                                  {
                                  }
                              #endif
                                  public static void {|RH7103:Create|}()
                                  {
                                  }
                              }
                              """;

        const string fixedSource = """
                                   public class C
                                   {
                                   #if DEBUG
                                       public void Trace()
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

        await Verify(WithEndOfLine(source, endOfLine), WithEndOfLine(fixedSource, endOfLine));
    }

    /// <summary>
    /// Verifying the SHARED-ORDER-008 example of a group of whole accessors in a crossed property: the fix moves
    /// <c>Shared</c> in front of <c>Value</c>
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH7103-007")]
    [Requirement("SHARED-ORDER-008")]
    public async Task FixMovesAMemberAcrossAPropertyWithAGroupOfWholeAccessors(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  public int Value
                                  {
                                      get;
                              #if DEBUG
                                      set;
                              #endif
                                  }

                                  public static int {|RH7103:Shared|} => 1;
                              }
                              """;

        const string fixedSource = """
                                   public class C
                                   {
                                       public static int Shared => 1;

                                       public int Value
                                       {
                                           get;
                                   #if DEBUG
                                           set;
                                   #endif
                                       }
                                   }
                                   """;

        await Verify(WithEndOfLine(source, endOfLine), WithEndOfLine(fixedSource, endOfLine));
    }

    /// <summary>
    /// Verifying the SHARED-ORDER-008 example of a group of whole statements in a switch section of a crossed method, which
    /// counts like a block: the fix moves <c>Create</c> in front of <c>Run</c>
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH7103-007")]
    [Requirement("SHARED-ORDER-008")]
    public async Task FixMovesAMemberAcrossAMethodWithAGroupOfWholeStatementsInASwitchSection(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  public int Run(int value)
                                  {
                                      switch (value)
                                      {
                                          case 1:
                              #if DEBUG
                                              System.Console.WriteLine();
                              #endif
                                              return 1;
                                          default:
                                              return 0;
                                      }
                                  }

                                  public static void {|RH7103:Create|}()
                                  {
                                  }
                              }
                              """;

        const string fixedSource = """
                                   public class C
                                   {
                                       public static void Create()
                                       {
                                       }

                                       public int Run(int value)
                                       {
                                           switch (value)
                                           {
                                               case 1:
                                   #if DEBUG
                                                   System.Console.WriteLine();
                                   #endif
                                                   return 1;
                                               default:
                                                   return 0;
                                           }
                                       }
                                   }
                                   """;

        await Verify(WithEndOfLine(source, endOfLine), WithEndOfLine(fixedSource, endOfLine));
    }

    /// <summary>
    /// Verifying the SHARED-ORDER-008 example of a conditional argument inside a statement of a crossed method: the
    /// directives stand between two tokens of one statement, so no code fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH7103-007")]
    [Requirement("SHARED-ORDER-008")]
    public async Task NoCodeFixWhenAConditionalArgumentStandsInAStatementOfACrossedMethod(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  public void Run()
                                  {
                                      System.Console.WriteLine(
                              #if DEBUG
                                          "debug"
                              #else
                                          "release"
                              #endif
                                          );
                                  }

                                  public static void {|RH7103:Create|}()
                                  {
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying the SHARED-ORDER-008 example of the <c>field</c> clause: without <c>TRACE</c> defined, a group of whole
    /// statements in a crossed accessor's body uses <c>field</c>, so no code fix is offered, although moving the static
    /// <c>Shared</c> reorders no instance field in any build
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH7103-007")]
    [Requirement("SHARED-ORDER-008")]
    public async Task NoCodeFixWhenAGroupInACrossedAccessorBodyUsesTheFieldKeyword(string endOfLine)
    {
        const string source = """
                              public struct S
                              {
                                  private int _x;

                                  public int Value
                                  {
                                      get
                                      {
                              #if TRACE
                                          return field;
                              #else
                                          return 0;
                              #endif
                                      }
                                  }

                                  public static int {|RH7103:Shared|} => 1;
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying the counterpart of the <c>field</c> clause: with <c>return 1;</c> in place of <c>return field;</c>, the
    /// group wraps whole statements, and the fix moves <c>Shared</c> in front of <c>Value</c>
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH7103-007")]
    [Requirement("SHARED-ORDER-008")]
    public async Task FixMovesAMemberAcrossAnAccessorWhoseGroupOfWholeStatementsDoesNotUseTheFieldKeyword(string endOfLine)
    {
        const string source = """
                              public struct S
                              {
                                  private int _x;

                                  public int Value
                                  {
                                      get
                                      {
                              #if TRACE
                                          return 1;
                              #else
                                          return 0;
                              #endif
                                      }
                                  }

                                  public static int {|RH7103:Shared|} => 1;
                              }
                              """;

        const string fixedSource = """
                                   public struct S
                                   {
                                       private int _x;

                                       public static int Shared => 1;

                                       public int Value
                                       {
                                           get
                                           {
                                   #if TRACE
                                               return 1;
                                   #else
                                               return 0;
                                   #endif
                                           }
                                       }
                                   }
                                   """;

        await Verify(WithEndOfLine(source, endOfLine), WithEndOfLine(fixedSource, endOfLine));
    }

    /// <summary>
    /// Verifying the <c>field</c> clause for the active branch: without <c>TRACE</c> defined, the active branch of a group of
    /// whole statements in a crossed accessor's body returns <c>field</c>, so no code fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH7103-007")]
    [Requirement("SHARED-ORDER-008")]
    public async Task NoCodeFixWhenTheActiveBranchOfAGroupInACrossedAccessorBodyUsesTheFieldKeyword(string endOfLine)
    {
        const string source = """
                              public struct S
                              {
                                  private int _x;

                                  public int Value
                                  {
                                      get
                                      {
                              #if !TRACE
                                          return field;
                              #else
                                          return 0;
                              #endif
                                      }
                                  }

                                  public static int {|RH7103:Shared|} => 1;
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying the <c>field</c> clause for a group of whole accessors: the excluded <c>set =&gt; field = value;</c> of a
    /// crossed property uses <c>field</c>, so no code fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH7103-007")]
    [Requirement("SHARED-ORDER-008")]
    public async Task NoCodeFixWhenAGroupOfWholeAccessorsOfACrossedPropertyUsesTheFieldKeyword(string endOfLine)
    {
        const string source = """
                              public struct S
                              {
                                  private int _x;

                                  public int Value
                                  {
                                      get => 0;
                              #if DEBUG
                                      set => field = value;
                              #endif
                                  }

                                  public static int {|RH7103:Shared|} => 1;
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying the counterpart of the <c>field</c> clause for a group of whole accessors: with <c>set { }</c> in place of
    /// <c>set =&gt; field = value;</c>, the group wraps whole accessors with a body, and the fix moves <c>Shared</c> in front
    /// of <c>Value</c>
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH7103-007")]
    [Requirement("SHARED-ORDER-008")]
    public async Task FixMovesAMemberAcrossAPropertyOfAStructWhoseGroupOfWholeAccessorsDoesNotUseTheFieldKeyword(string endOfLine)
    {
        const string source = """
                              public struct S
                              {
                                  private int _x;

                                  public int Value
                                  {
                                      get => 0;
                              #if DEBUG
                                      set { }
                              #endif
                                  }

                                  public static int {|RH7103:Shared|} => 1;
                              }
                              """;

        const string fixedSource = """
                                   public struct S
                                   {
                                       private int _x;

                                       public static int Shared => 1;

                                       public int Value
                                       {
                                           get => 0;
                                   #if DEBUG
                                           set { }
                                   #endif
                                       }
                                   }
                                   """;

        await Verify(WithEndOfLine(source, endOfLine), WithEndOfLine(fixedSource, endOfLine));
    }

    /// <summary>
    /// Verifying the <c>No backing field from a branch</c> clause in a class: the excluded <c>set;</c> behind
    /// <c>get =&gt; 0;</c> would give a crossed property a backing field in a build with <c>DEBUG</c> only, so no code fix is
    /// offered, although the field order of a class sets no layout
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH7103-007")]
    [Requirement("SHARED-ORDER-008")]
    public async Task NoCodeFixWhenAGroupOfWholeAccessorsOfACrossedPropertyOfAClassHoldsAnAccessorWithoutABody(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  public int Value
                                  {
                                      get => 0;
                              #if DEBUG
                                      set;
                              #endif
                                  }

                                  public static int {|RH7103:Shared|} => 1;
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying the <c>No backing field from a branch</c> clause for an active branch in a class: the active
    /// <c>set;</c> behind <c>get =&gt; 0;</c> gives a crossed property a backing field only in a build without
    /// <c>DEBUG</c>, so no code fix is offered, although the field order of a class sets no layout
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH7103-007")]
    [Requirement("SHARED-ORDER-008")]
    public async Task NoCodeFixWhenAnActiveGroupOfWholeAccessorsOfACrossedPropertyOfAClassHoldsAnAccessorWithoutABody(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  public int Value
                                  {
                                      get => 0;
                              #if !DEBUG
                                      set;
                              #endif
                                  }

                                  public static int {|RH7103:Shared|} => 1;
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying that a conditional attribute list on a class whose field order sets no layout keeps the move of a member
    /// that reorders no instance field: the fix moves the static <c>Create</c> in front of <c>Run</c>
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH7103-007")]
    [Requirement("SHARED-ORDER-008")]
    public async Task FixMovesAStaticMethodOfAClassWithAConditionalAttributeList(string endOfLine)
    {
        const string source = """
                              #if DEBUG
                              [System.Serializable]
                              #endif
                              public class C
                              {
                                  public void Run()
                                  {
                                  }

                                  public static void {|RH7103:Create|}()
                                  {
                                  }
                              }
                              """;

        const string fixedSource = """
                                   #if DEBUG
                                   [System.Serializable]
                                   #endif
                                   public class C
                                   {
                                       public static void Create()
                                       {
                                       }

                                       public void Run()
                                       {
                                       }
                                   }
                                   """;

        await Verify(WithEndOfLine(source, endOfLine), WithEndOfLine(fixedSource, endOfLine));
    }

    /// <summary>
    /// Verifying that a conditional entry in the base list of a class whose field order sets no layout keeps the move of a
    /// member that reorders no instance field: the fix moves the static <c>Create</c> in front of <c>Dispose</c>
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH7103-007")]
    [Requirement("SHARED-ORDER-008")]
    public async Task FixMovesAStaticMethodOfAClassWithAConditionalEntryInItsBaseList(string endOfLine)
    {
        const string source = """
                              public sealed class C : System.IDisposable
                              #if NET
                                  , System.IAsyncDisposable
                              #endif
                              {
                                  public void Dispose()
                                  {
                                  }

                                  public static void {|RH7103:Create|}()
                                  {
                                  }
                              }
                              """;

        const string fixedSource = """
                                   public sealed class C : System.IDisposable
                                   #if NET
                                       , System.IAsyncDisposable
                                   #endif
                                   {
                                       public static void Create()
                                       {
                                       }

                                       public void Dispose()
                                       {
                                       }
                                   }
                                   """;

        await Verify(WithEndOfLine(source, endOfLine), WithEndOfLine(fixedSource, endOfLine));
    }

    /// <summary>
    /// Verifying the SHARED-ORDER-008 example of a group in front of the moved and the crossed span whose excluded branch
    /// closes the type's body, for a move that reorders no instance field: moving the static <c>_c</c> in front of the
    /// instance <c>_a</c> changes no layout in any build, and the fix moves it
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7103-007")]
    public async Task FixMovesAStaticFieldBehindAGroupInFrontOfTheSpansThatClosesTheTypeAndOpensAStruct(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                              #if X
                              }

                              public struct D
                              {
                              #endif
                                  private static int _y;
                                  private int _a;
                                  private static int {|RH7103:_c|};
                              }
                              """;

        const string fixedSource = """
                                   public class C
                                   {
                                   #if X
                                   }

                                   public struct D
                                   {
                                   #endif
                                       private static int _y;
                                       private static int _c;
                                       private int _a;
                                   }
                                   """;

        await Verify(WithEndOfLine(source, endOfLine), WithEndOfLine(fixedSource, endOfLine));
    }

    /// <summary>
    /// Verifying that a conditional <c>StructLayout</c> attribute list on another declaration of a partial class keeps a
    /// move that reorders no instance field: moving the static <c>_c</c> in front of the instance <c>_a</c> changes no
    /// layout in any build, and the fix moves it
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7103-007")]
    public async Task FixMovesAStaticFieldOfAPartialClassWhoseOtherDeclarationHoldsAConditionalLayoutAttribute(string endOfLine)
    {
        const string source = """
                              using System.Runtime.InteropServices;

                              #if NET
                              [StructLayout(LayoutKind.Sequential)]
                              #endif
                              public partial class C
                              {
                              }

                              public partial class C
                              {
                                  private int _a;
                                  private static int {|RH7103:_c|};
                              }
                              """;

        const string fixedSource = """
                                   using System.Runtime.InteropServices;

                                   #if NET
                                   [StructLayout(LayoutKind.Sequential)]
                                   #endif
                                   public partial class C
                                   {
                                   }

                                   public partial class C
                                   {
                                       private static int _c;
                                       private int _a;
                                   }
                                   """;

        await Verify(WithEndOfLine(source, endOfLine), WithEndOfLine(fixedSource, endOfLine));
    }

    #endregion // Tests

    #region Methods

    /// <summary>
    /// Writes the provided text with the given line break
    /// </summary>
    /// <param name="text">Text</param>
    /// <param name="endOfLine">Line break</param>
    /// <returns>The text with every line break replaced by <paramref name="endOfLine"/></returns>
    private static string WithEndOfLine(string text, string endOfLine)
    {
        return text.Replace("\r\n", "\n").Replace("\n", endOfLine);
    }

    /// <summary>
    /// Applies the one code action registered for <c>_a</c>, asserts the exact result, and asserts that <c>_b</c> is
    /// still reported in it while <c>_a</c> is not
    /// </summary>
    /// <param name="testCode">Source text without markup, in which <c>_a</c> and <c>_b</c> are reported</param>
    /// <param name="fixedCode">Expected source text after the action, without markup</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private static async Task VerifyTheActionForTheFieldThatCrossesOnlyAnInstanceField(string testCode, string fixedCode)
    {
        var actions = await GetCodeFixActionsAsync(testCode,
                                                   RH7103StaticElementsMustAppearBeforeInstanceElementsAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<VariableDeclaratorSyntax>()
                                                               .Single(declarator => declarator.Identifier.ValueText == "_a")
                                                               .Identifier
                                                               .GetLocation());

        Assert.HasCount(1, actions);

        var operations = await actions[0].GetOperationsAsync(CancellationToken.None);
        var changedDocument = operations.OfType<ApplyChangesOperation>().Single().ChangedSolution.Projects.Single().Documents.Single();
        var changedText = (await changedDocument.GetTextAsync(CancellationToken.None)).ToString();

        Assert.AreEqual(fixedCode, changedText);

        await Verify(changedText.Replace("private static int _b", "private static int {|RH7103:_b|}", StringComparison.Ordinal));
    }

    #endregion // Methods

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    [TestMethod]
    [Requirement("RH7103-008")]
    public override async Task FixAllCorrectsEveryDiagnostic()
    {
        await base.FixAllCorrectsEveryDiagnostic();
    }

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
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

        // Verifying that Fix All keeps every separator at its position when two static members are reordered in the same document, so the gap sequence converges correctly instead of depending on application order
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH7103StaticElementsMustAppearBeforeInstanceElementsAnalyzer.DiagnosticId, AnalyzerResources.RH7103MessageFormat, 2),
                                  test => test.NumberOfFixAllIterations = 2);
    }

    #endregion // BatchCodeFixTestsBase
}