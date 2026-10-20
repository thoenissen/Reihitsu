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
public class RH7105DeclarationKeywordsMustFollowOrderAnalyzerTests : BatchCodeFixTestsBase<RH7105DeclarationKeywordsMustFollowOrderAnalyzer, RH7105DeclarationKeywordsMustFollowOrderCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying misordered declaration keywords are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7105-001")]
    [Requirement("RH7105-003")]
    [Requirement("RH7105-004")]
    [Requirement("SHARED-MODORDER-002")]
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
    [Requirement("RH7105-002")]
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
    [Requirement("RH7105-002")]
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
    [Requirement("RH7105-002")]
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
    [Requirement("RH7105-001")]
    [Requirement("RH7105-004")]
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
    /// Verifying that <c>ref</c> before <c>partial</c> on a struct is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7105-002")]
    public async Task RefBeforePartialStructIsNotReported()
    {
        const string testCode = """
                                public ref partial struct TestStruct
                                {
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying that <c>readonly ref</c> before <c>partial</c> on a struct is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7105-002")]
    public async Task ReadonlyRefBeforePartialStructIsNotReported()
    {
        const string testCode = """
                                public readonly ref partial struct TestStruct
                                {
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying that <c>unsafe ref</c> before <c>partial</c> on a struct is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7105-002")]
    public async Task UnsafeRefBeforePartialStructIsNotReported()
    {
        const string testCode = """
                                public unsafe ref partial struct TestStruct
                                {
                                }
                                """;

        await Verify(testCode, AllowUnsafe);
    }

    /// <summary>
    /// Verifying that <c>ref</c> on a struct without <c>partial</c> is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7105-002")]
    public async Task ReadonlyRefStructIsNotReported()
    {
        const string testCode = """
                                public readonly ref struct TestStruct
                                {
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying that a misordered accessibility modifier on a ref partial struct is reported and fixed to a
    /// compiling order
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7105-001")]
    [Requirement("RH7105-004")]
    public async Task MisorderedModifiersOnRefPartialStructAreReportedAndFixed()
    {
        const string testCode = """
                                {|#0:readonly|} public ref partial struct TestStruct
                                {
                                    public int Value { get; }
                                }
                                """;

        const string fixedCode = """
                                 public readonly ref partial struct TestStruct
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

    /// <summary>
    /// Verifying that a misordered modifier list is still reported without offering a fix when a comment sits
    /// between the modifiers, because rebuilding the modifiers keeps only the leading trivia of the first one
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7105-004")]
    [Requirement("SHARED-MODORDER-003")]
    public async Task MisorderedModifiersWithCommentAreReportedWithoutFix()
    {
        const string testCode = """
                                public class TestClass
                                {
                                    {|#0:static|} /* keep */ public int Value { get; set; }
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

    /// <summary>
    /// Verifying the specification example: modifiers out of the canonical order are reported on fields, on an
    /// interface method, and for <c>new</c> and <c>required</c>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7105-001")]
    [Requirement("RH7105-003")]
    public async Task ModifiersOutOfTheCanonicalOrderAreReported()
    {
        const string source = """
                              class B
                              {
                                  public int X;
                              }

                              class C : B
                              {
                                  {|RH7105:static|} public int A;
                                  public {|RH7105:readonly|} static int D = 1;
                                  {|RH7105:new|} public int X;
                                  {|RH7105:required|} public int Y;
                              }

                              interface I
                              {
                                  {|RH7105:abstract|} static void M();
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifying the specification example: modifiers in the canonical order are not reported, whatever the order of
    /// the accessibility keywords among themselves
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7105-002")]
    public async Task ModifiersInTheCanonicalOrderAreNotReported()
    {
        const string source = """
                              public readonly ref partial struct S
                              {
                              }

                              public unsafe partial class U
                              {
                                  internal protected static void M()
                                  {
                                  }
                              }
                              """;

        await Verify(source, AllowUnsafe);
    }

    /// <summary>
    /// Verifying the specification example: the diagnostic is on the first modifier, from the left, that differs from
    /// the canonical order
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7105-003")]
    public async Task DiagnosticIsOnTheFirstMisplacedModifier()
    {
        const string source = """
                              class C
                              {
                                  public {|RH7105:readonly|} static int A = 1;
                                  {|RH7105:static|} internal protected void M()
                                  {
                                  }
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifying the specification example: the fix writes the canonical order and keeps the accessibility keywords
    /// in their written order
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7105-004")]
    [Requirement("SHARED-MODORDER-002")]
    public async Task FixWritesTheCanonicalOrderAndKeepsTheAccessibilityOrder()
    {
        const string source = """
                              class C
                              {
                                  {|RH7105:static|} internal protected void M()
                                  {
                                  }
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                       internal protected static void M()
                                       {
                                       }
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifying the shared specification example: a class, a field, an event field, a property, a method, an
    /// operator, and a conversion operator are checked
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-MODORDER-001")]
    [Requirement("RH7105-001")]
    public async Task EveryCheckedDeclarationKindIsReported()
    {
        const string source = """
                              using System;

                              {|RH7105:static|} public class S
                              {
                              }

                              public class C
                              {
                                  {|RH7105:static|} public int F;

                                  {|RH7105:static|} public event Action E;

                                  {|RH7105:static|} public int P { get; set; }

                                  {|RH7105:static|} public void M()
                                  {
                                  }

                                  {|RH7105:static|} public C operator +(C a, C b)
                                  {
                                      return a;
                                  }

                                  {|RH7105:static|} public implicit operator int(C c)
                                  {
                                      return 0;
                                  }
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifying the shared specification example: the trivia in front of the first modifier stays in front of the
    /// new first modifier, and each modifier is followed by a single space
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-MODORDER-002")]
    [Requirement("RH7105-004")]
    public async Task FixKeepsTheLeadingTriviaAndWritesSingleSpaces()
    {
        const string source = """
                              class C
                              {
                                  /* lead */ {|RH7105:static|}   public int A;
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                       /* lead */ public static int A;
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifying the shared specification example: a comment after a modifier on its line withholds the fix, and the
    /// diagnostic is still reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-MODORDER-003")]
    [Requirement("RH7105-004")]
    public async Task CommentAfterAModifierWithholdsTheFix()
    {
        const string source = """
                              class C
                              {
                                  {|RH7105:static|} /* keep */ public int Value { get; set; }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifying that a comment after the last modifier on its line withholds the fix, and the diagnostic is still
    /// reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-MODORDER-003")]
    public async Task CommentAfterTheLastModifierWithholdsTheFix()
    {
        const string source = """
                              class C
                              {
                                  {|RH7105:static|} public /* keep */ int Value { get; set; }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifying that a pragma directive in front of a later modifier withholds the fix, and the diagnostic is still
    /// reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-MODORDER-003")]
    public async Task PragmaDirectiveInFrontOfALaterModifierWithholdsTheFix()
    {
        const string source = """
                              class C
                              {
                                  {|RH7105:static|}
                              #pragma warning disable CS0169
                                  public int Value { get; set; }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    #endregion // Tests

    #region Methods

    /// <summary>
    /// Enables unsafe code for the verifier so declarations using the <c>unsafe</c> modifier compile
    /// </summary>
    /// <param name="test">Test</param>
    private static void AllowUnsafe(CSharpAnalyzerVerifierTest<RH7105DeclarationKeywordsMustFollowOrderAnalyzer> test)
    {
        test.SolutionTransforms.Add(ApplyAllowUnsafeToTestProject);
    }

    #endregion // Methods

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                public class TestClass
                                {
                                    {|#0:static|} public int Value { get; set; }

                                    {|#1:static|} internal int Other { get; set; }
                                }
                                """;

        const string fixedCode = """
                                 public class TestClass
                                 {
                                     public static int Value { get; set; }

                                     internal static int Other { get; set; }
                                 }
                                 """;

        // Two misordered modifier lists inside one type, so the batch merges both member rewrites into the same
        // type declaration and a fix that reordered more than its own modifier list would lose the other
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH7105DeclarationKeywordsMustFollowOrderAnalyzer.DiagnosticId, AnalyzerResources.RH7105MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}