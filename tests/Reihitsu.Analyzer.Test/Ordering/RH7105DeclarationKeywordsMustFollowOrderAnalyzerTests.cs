using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;
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
    #region Constants

    /// <summary>
    /// Declaration of the attribute the compiler emits on a <c>closed</c> class, which the reference assemblies of the
    /// verifier do not declare
    /// </summary>
    private const string IsClosedTypeAttributeSource = """
                                                       namespace System.Runtime.CompilerServices
                                                       {
                                                           [System.AttributeUsage(System.AttributeTargets.Class, Inherited = false)]
                                                           public sealed class IsClosedTypeAttribute : System.Attribute
                                                           {
                                                           }
                                                       }
                                                       """;

    #endregion // Constants

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

        await Verify(testCode, fixedCode, Diagnostics(RH7105DeclarationKeywordsMustFollowOrderAnalyzer.DiagnosticId, MessageFormat));
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

        await Verify(testCode, fixedCode, Diagnostics(RH7105DeclarationKeywordsMustFollowOrderAnalyzer.DiagnosticId, MessageFormat));
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

        await Verify(testCode, fixedCode, Diagnostics(RH7105DeclarationKeywordsMustFollowOrderAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that a misordered modifier list with an empty conditional directive between two of its modifiers is
    /// reported, with and without the symbol defined, and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7105-001")]
    [Requirement("SHARED-DIAG-008")]
    public async Task MisorderedModifiersSplitByAConditionalDirectiveAreReportedWithoutFix()
    {
        const string testData = """
                                public class TestClass
                                {
                                    {|RH7105:static|}
                                #if FEATURE
                                #endif
                                    public int Value { get; set; }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
        await VerifyNoCodeFixIsOffered(testData, DocumentationMode.Parse, "FEATURE");
    }

    /// <summary>
    /// Verifies that a misordered modifier list is reported, and no code fix is offered, for every shape of conditional
    /// directive in front of a later modifier: the analyzer judges the modifiers the analyzed build compiles
    /// </summary>
    /// <param name="directives">The conditional directives, and the code they hold, between <c>static</c> and the type</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("#if FEATURE\n#endif\n    public", DisplayName = "Empty block")]
    [DataRow("#if NEVER\n    volatile\n#endif\n    public", DisplayName = "Excluded modifier")]
    [DataRow("#if !NEVER\n    public\n#endif", DisplayName = "Active modifier")]
    [DataRow("#if NEVER\n    internal\n#else\n    public\n#endif", DisplayName = "If and else")]
    [DataRow("#if NEVER\n    internal\n#elif !NEVER\n    public\n#endif", DisplayName = "If and elif")]
    [DataRow("#if false\n    volatile\n#endif\n    public", DisplayName = "If false")]
    [Requirement("RH7105-001")]
    [Requirement("SHARED-DIAG-008")]
    [Requirement("SHARED-MODORDER-003")]
    public async Task ConditionalDirectiveInFrontOfALaterModifierKeepsTheReportAndWithholdsTheFix(string directives)
    {
        var testData = $$"""
                         class C
                         {
                             {|RH7105:static|}
                         {{directives}}
                             int _a;
                         }
                         """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies the specification example in which an <c>#if</c> block that excludes a modifier stands between two
    /// modifiers: the analyzed build compiles <c>static public</c>, the declaration is reported, and no code fix is
    /// offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7105-001")]
    [Requirement("SHARED-DIAG-008")]
    public async Task ExcludedModifierBetweenTwoModifiersKeepsTheReportAndWithholdsTheFix()
    {
        const string testData = """
                                class C
                                {
                                    {|RH7105:static|}
                                #if DEBUG
                                    readonly
                                #endif
                                    public int _a;
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
        await VerifyNoCodeFixIsOffered(testData, DocumentationMode.Parse, "DEBUG");
    }

    /// <summary>
    /// Verifies that Fix All corrects the declaration without a conditional directive between its modifiers and leaves
    /// the one with such a directive reported and unchanged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7105-001")]
    [Requirement("SHARED-DIAG-008")]
    public async Task FixAllCorrectsOnlyTheDeclarationWithoutAConditionalDirectiveBetweenItsModifiers()
    {
        const string testData = """
                                class C
                                {
                                    {|RH7105:static|}
                                #if FEATURE
                                #endif
                                    public int _a;

                                    {|RH7105:static|} public int _b;

                                    {|RH7105:static|} public int _c;
                                }
                                """;

        const string fixedData = """
                                 class C
                                 {
                                     {|RH7105:static|}
                                 #if FEATURE
                                 #endif
                                     public int _a;

                                     public static int _b;

                                     public static int _c;
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     test =>
                     {
                         // The diagnostic whose fix is withheld stays and is marked in the expected source
                         test.FixedState.MarkupHandling = MarkupMode.Allow;
                         test.BatchFixedState.MarkupHandling = MarkupMode.Allow;
                     });
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

        await Verify(testCode, Diagnostics(RH7105DeclarationKeywordsMustFollowOrderAnalyzer.DiagnosticId, MessageFormat));

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

    /// <summary>
    /// Verifying the canonical positions of <c>virtual</c>, <c>sealed</c>, <c>override</c>, <c>volatile</c>,
    /// <c>extern</c>, <c>unsafe</c>, and <c>file</c>: each list out of order is reported on its first misplaced modifier
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7105-001")]
    [Requirement("RH7105-003")]
    public async Task ModifiersOutOfTheCanonicalOrderAreReportedForEveryKeywordRank()
    {
        const string source = """
                              class B
                              {
                                  {|RH7105:virtual|} public void M()
                                  {
                                  }
                              }

                              class C : B
                              {
                                  public {|RH7105:override|} sealed void M()
                                  {
                                  }

                                  public {|RH7105:volatile|} static int _value;

                                  public {|RH7105:extern|} static void External();

                                  public static {|RH7105:unsafe|} extern void Native();
                              }

                              {|RH7105:sealed|} file class F
                              {
                              }
                              """;

        await Verify(source, AllowUnsafe);
    }

    /// <summary>
    /// Verifying that <c>virtual</c>, <c>sealed</c>, <c>override</c>, <c>volatile</c>, <c>extern</c>, <c>unsafe</c>,
    /// and <c>file</c> in their canonical positions are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7105-002")]
    public async Task ModifiersInTheCanonicalOrderAreNotReportedForEveryKeywordRank()
    {
        const string source = """
                              class B
                              {
                                  public virtual void M()
                                  {
                                  }
                              }

                              class C : B
                              {
                                  public sealed override void M()
                                  {
                                  }

                                  public static volatile int _value;

                                  public static extern void External();

                                  public static extern unsafe void Native();
                              }

                              file sealed class F
                              {
                              }
                              """;

        await Verify(source, AllowUnsafe);
    }

    /// <summary>
    /// Verifying that the fix writes <c>sealed</c> before <c>override</c> after the accessibility
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7105-004")]
    public async Task FixWritesSealedBeforeOverride()
    {
        const string source = """
                              class B
                              {
                                  public virtual void M()
                                  {
                                  }
                              }

                              class C : B
                              {
                                  {|RH7105:sealed|} override public void M()
                                  {
                                  }
                              }
                              """;

        const string fixedSource = """
                                   class B
                                   {
                                       public virtual void M()
                                       {
                                       }
                                   }

                                   class C : B
                                   {
                                       public sealed override void M()
                                       {
                                       }
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifying that RH7105 checks the struct, interface, record, record struct, delegate, constructor, event, and
    /// indexer declarations too, so together with <see cref="EveryCheckedDeclarationKindIsReported"/> it checks every
    /// declaration kind of the shared specification
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-MODORDER-001")]
    public async Task RemainingCheckedDeclarationKindsAreReported()
    {
        const string source = """
                              using System;

                              public class C
                              {
                                  {|RH7105:readonly|} public struct S
                                  {
                                  }

                                  {|RH7105:new|} public interface I
                                  {
                                  }

                                  {|RH7105:sealed|} public record R;

                                  {|RH7105:readonly|} public record struct RS;

                                  {|RH7105:new|} public delegate void D();

                                  {|RH7105:unsafe|} public C()
                                  {
                                  }

                                  {|RH7105:static|} public event Action E
                                  {
                                      add
                                      {
                                      }

                                      remove
                                      {
                                      }
                                  }

                                  {|RH7105:virtual|} public int this[int index] => index;
                              }
                              """;

        await Verify(source, AllowUnsafe);
    }

    /// <summary>
    /// Verifying that a directive in front of the first modifier does not withhold the fix and stays in front of the
    /// new first modifier
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-MODORDER-002")]
    [Requirement("SHARED-MODORDER-003")]
    public async Task DirectiveInFrontOfTheFirstModifierDoesNotWithholdTheFix()
    {
        const string source = """
                              class C
                              {
                              #pragma warning disable CS0169
                                  {|RH7105:static|} public int A;
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                   #pragma warning disable CS0169
                                       public static int A;
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifying that the part of a partial method that writes <c>static public</c> is reported although the other part
    /// writes its modifiers in the canonical order, and that the fix reorders the reported part only and leaves the
    /// other part as it is
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7105-001")]
    [Requirement("RH7105-004")]
    [Requirement("SHARED-DIAG-004")]
    public async Task PartOfAPartialMethodIsReportedAndFixedOnItsOwn()
    {
        const string source = """
                              public partial class C
                              {
                                  {|RH7105:static|} public partial void M();

                                  public static partial void M()
                                  {
                                  }
                              }
                              """;

        const string fixedSource = """
                                   public partial class C
                                   {
                                       public static partial void M();

                                       public static partial void M()
                                       {
                                       }
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifying that the part of a partial class that writes <c>static public</c> is reported on that part although the
    /// first part writes its modifiers in the canonical order, and that the fix reorders the reported part only and
    /// leaves the first part as it is
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7105-001")]
    [Requirement("RH7105-004")]
    [Requirement("SHARED-DIAG-004")]
    public async Task PartOfAPartialTypeIsReportedAndFixedOnItsOwn()
    {
        const string source = """
                              public static partial class C
                              {
                                  public const int A = 1;
                              }

                              {|RH7105:static|} public partial class C
                              {
                                  public const int B = 2;
                              }
                              """;

        const string fixedSource = """
                                   public static partial class C
                                   {
                                       public const int A = 1;
                                   }

                                   public static partial class C
                                   {
                                       public const int B = 2;
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifies that the fix of a type that holds a region pair keeps the text of both region directives
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-028")]
    [Requirement("SHARED-FIX-006")]
    public async Task FixOfTypeWithRegionPairKeepsTheRegionDirectives()
    {
        const string testCode = """
                                namespace N
                                {
                                    {|#0:static|} public class C
                                    {
                                        #region    bar

                                        private static int _a;

                                        #endregion
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace N
                                 {
                                     public static class C
                                     {
                                         #region    bar

                                         private static int _a;

                                         #endregion
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH7105DeclarationKeywordsMustFollowOrderAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix of a type that holds a region pair with several spaces in front of an upper-case description keeps
    /// that separator
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-028")]
    [Requirement("SHARED-FIX-006")]
    public async Task FixOfTypeWithRegionPairKeepsTheSeparatorOfAnUpperCaseDescription()
    {
        const string testCode = """
                                namespace N
                                {
                                    {|#0:static|} public class C
                                    {
                                        #region    Bar

                                        private static int _a;

                                        #endregion // Bar
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace N
                                 {
                                     public static class C
                                     {
                                         #region    Bar

                                         private static int _a;

                                         #endregion // Bar
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH7105DeclarationKeywordsMustFollowOrderAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix of a type that holds a region pair with a tab behind each region keyword reorders only the
    /// modifiers and keeps both region directives as written, tabs included (<c>→</c> stands for a tab)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-006")]
    public async Task FixOfTypeWithRegionPairKeepsTabsBehindRegionKeywords()
    {
        const string testCode = """
                                namespace N
                                {
                                    {|#0:static|} public class C
                                    {
                                        #region→bar

                                        private static int _a;

                                        #endregion→// Stale
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace N
                                 {
                                     public static class C
                                     {
                                         #region→bar

                                         private static int _a;

                                         #endregion→// Stale
                                     }
                                 }
                                 """;

        await Verify(testCode.Replace("→", "\t"), fixedCode.Replace("→", "\t"), Diagnostics(RH7105DeclarationKeywordsMustFollowOrderAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifying the shared specification example: a directive between the last modifier and the type withholds the
    /// fix, and the diagnostic is still reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-MODORDER-003")]
    public async Task DirectiveBehindTheLastModifierWithholdsTheFix()
    {
        const string source = """
                              class C
                              {
                                  {|RH7105:static|} public
                              #pragma warning disable CS0169
                                  int _a;
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifying the shared specification example: a comment on its own line, and a comment in front of the type on its
    /// line, between the last modifier and the type withhold the fix, and the diagnostic is still reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-MODORDER-003")]
    public async Task CommentBetweenTheLastModifierAndTheTypeWithholdsTheFix()
    {
        const string source = """
                              class C
                              {
                                  {|RH7105:static|} public
                                  // the counter
                                  int _a;

                                  {|RH7105:static|} public
                                  /* the total */ int _b;
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifying that a nullable directive between the last modifier and the type withholds the fix, and the diagnostic
    /// is still reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-MODORDER-003")]
    public async Task NullableDirectiveBehindTheLastModifierWithholdsTheFix()
    {
        const string source = """
                              class C
                              {
                                  {|RH7105:static|} public
                              #nullable disable
                                  string _a;
                              #nullable restore
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifying that conditional directives between the last modifier and the type withhold the fix, and the diagnostic
    /// is still reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-MODORDER-003")]
    public async Task ConditionalDirectivesBehindTheLastModifierWithholdTheFix()
    {
        const string source = """
                              class C
                              {
                                  {|RH7105:static|} public
                              #if DEBUG
                                  int _a;
                              #else
                                  long _a;
                              #endif
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifying that a region directive between the last modifier and the type withholds the fix, and the diagnostic is
    /// still reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-MODORDER-003")]
    public async Task RegionDirectiveBehindTheLastModifierWithholdsTheFix()
    {
        const string source = """
                              class C
                              {
                                  {|RH7105:static|} public
                                  #region Fields
                                  int _a;
                                  #endregion
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifying that a directive between the last modifier and the <c>class</c> keyword of a nested type withholds the
    /// fix, and the diagnostic is still reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-MODORDER-003")]
    public async Task DirectiveBehindTheLastModifierOfATypeWithholdsTheFix()
    {
        const string source = """
                              class C
                              {
                                  {|RH7105:static|} public
                              #pragma warning disable CS0169
                                  class D
                                  {
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies that a comment the formatter refuses in the body of the member whose modifiers are reordered no longer
    /// withholds the fix, which rewrites the modifiers without formatting the member
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-006")]
    [Requirement("SHARED-FIX-010")]
    public async Task FixIsOfferedWhenTheBodyOfTheReorderedMemberHoldsACommentTheFormatterRefuses()
    {
        const string source = """
                              internal class C
                              {
                                  {|RH7105:static|} public void M()
                                  {
                                      int /* c */ y = 1;
                                  }
                              }
                              """;

        const string fixedSource = """
                                   internal class C
                                   {
                                       public static void M()
                                       {
                                           int /* c */ y = 1;
                                       }
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifying the reported example: the fix reorders only the modifiers of an expression-bodied method, which keeps its
    /// expression body
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-FIX-006")]
    [Requirement("SHARED-MODORDER-002")]
    public async Task FixKeepsTheExpressionBodyOfTheReorderedMethod(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  {|RH7105:override|} public string ToString() => "";
                              }
                              """;

        const string fixedSource = """
                                   public class C
                                   {
                                       public override string ToString() => "";
                                   }
                                   """;

        await VerifyCodeFixWithEndOfLine(source, fixedSource, endOfLine);
    }

    /// <summary>
    /// Verifying that the fix reorders only the modifiers of an operator, which keeps its expression body
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-006")]
    public async Task FixKeepsTheExpressionBodyOfTheReorderedOperator()
    {
        const string source = """
                              public class C
                              {
                                  {|RH7105:static|} public C operator +(C a, C b) => a;
                              }
                              """;

        const string fixedSource = """
                                   public class C
                                   {
                                       public static C operator +(C a, C b) => a;
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifying that the fix reorders only the modifiers of a type, and the members of the type stay as written
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-006")]
    public async Task FixOfATypeLeavesItsMembersAsWritten()
    {
        const string source = """
                              {|RH7105:static|} public class D { static int M( ) => 1; }
                              """;

        const string fixedSource = """
                                   public static class D { static int M( ) => 1; }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifies the specification example in which neither a conditional directive between two modifiers nor one behind
    /// the last modifier keeps the declaration from being reported, and no code fix is offered for either
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7105-001")]
    [Requirement("SHARED-DIAG-008")]
    [Requirement("SHARED-MODORDER-003")]
    public async Task ConditionalDirectiveBehindTheLastModifierDoesNotKeepTheDeclarationFromBeingReported()
    {
        const string testData = """
                                class C
                                {
                                    {|RH7105:static|}
                                #if FEATURE
                                #endif
                                    public int Value { get; set; }

                                    {|RH7105:static|} public
                                #if DEBUG
                                    readonly
                                #endif
                                    int _a;
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that a declaration whose conditional directive stands between two modifiers once the symbol is defined
    /// is reported in that build, and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7105-001")]
    [Requirement("SHARED-DIAG-008")]
    public async Task ConditionalDirectiveThatStandsBetweenModifiersWhereTheSymbolIsDefinedKeepsTheReportAndWithholdsTheFix()
    {
        const string testData = """
                                class C
                                {
                                    {|RH7105:static|} public
                                #if DEBUG
                                    readonly
                                #endif
                                    int _a;
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData, DocumentationMode.Parse, "DEBUG");
    }

    /// <summary>
    /// Verifies that a conditional directive behind the last modifier that selects the type of the declaration does not
    /// keep it from being reported, and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7105-001")]
    [Requirement("SHARED-MODORDER-003")]
    public async Task ConditionalDirectiveThatSelectsTheTypeBehindTheLastModifierDoesNotKeepTheDeclarationFromBeingReported()
    {
        const string testData = """
                                class C
                                {
                                    {|RH7105:static|} public
                                #if WIDE
                                    long
                                #else
                                    int
                                #endif
                                    _a;
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that both parts of a partial constructor are reported, each on its own modifiers
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7105-001")]
    [Requirement("SHARED-DIAG-004")]
    public async Task BothPartsOfAPartialConstructorAreReported()
    {
        const string testData = """
                                public partial class C
                                {
                                    {|RH7105:unsafe|} public partial C();

                                    {|RH7105:unsafe|} public partial C()
                                    {
                                    }
                                }
                                """;

        await Verify(testData, test => test.SolutionTransforms.Add(ApplyAllowUnsafeToTestProject));
    }

    /// <summary>
    /// Verifies that the defining part of a partial constructor is reported when its implementing part lies in another
    /// file
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7105-001")]
    [Requirement("SHARED-DIAG-004")]
    public async Task DefiningPartOfAPartialConstructorIsReportedWhenTheImplementingPartIsInAnotherFile()
    {
        const string definingPart = """
                                    public partial class C
                                    {
                                        {|RH7105:unsafe|} public partial C();
                                    }
                                    """;

        const string implementingPart = """
                                        public partial class C
                                        {
                                            {|RH7105:unsafe|} public partial C()
                                            {
                                            }
                                        }
                                        """;

        await Verify(definingPart,
                     test =>
                     {
                         test.TestState.Sources.Add(("/0/Test1.cs", implementingPart));
                         test.SolutionTransforms.Add(ApplyAllowUnsafeToTestProject);
                     });
    }

    /// <summary>
    /// Verifies that the defining part of a partial constructor is reported when its implementing part lies in
    /// generated code
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7105-001")]
    [Requirement("SHARED-DIAG-004")]
    public async Task DefiningPartOfAPartialConstructorIsReportedWhenTheImplementingPartIsGenerated()
    {
        const string definingPart = """
                                    public partial class C
                                    {
                                        {|RH7105:unsafe|} public partial C();
                                    }
                                    """;

        const string implementingPart = """
                                        public partial class C
                                        {
                                            unsafe public partial C()
                                            {
                                            }
                                        }
                                        """;

        await Verify(definingPart,
                     test =>
                     {
                         test.TestState.Sources.Add(("/0/C.g.cs", implementingPart));
                         test.SolutionTransforms.Add(ApplyAllowUnsafeToTestProject);
                     });
    }

    /// <summary>
    /// Verifies that a conditional directive in front of the first modifier, the <c>#endif</c> of a conditionally
    /// compiled member in front of the declaration, keeps the declaration neither from being reported nor from being
    /// fixed, whether the symbol is defined or not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7105-001")]
    [Requirement("RH7105-004")]
    [Requirement("SHARED-MODORDER-002")]
    public async Task ConditionalDirectiveInFrontOfTheFirstModifierDoesNotKeepTheDeclarationFromBeingReportedOrFixed()
    {
        const string source = """
                              class C
                              {
                              #if DEBUG
                                  void Log() { }
                              #endif
                                  {|RH7105:static|} public int A;
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                   #if DEBUG
                                       void Log() { }
                                   #endif
                                       public static int A;
                                   }
                                   """;

        await Verify(source, fixedSource);
        await Verify(source, fixedSource, test => test.SolutionTransforms.Add(DefineDebug));
    }

    /// <summary>
    /// Verifies that a <c>closed</c> partial class, which writes <c>closed</c> in the position of <c>sealed</c> in front of
    /// <c>partial</c>, is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7105-001")]
    [Requirement("RH7105-002")]
    public async Task ClosedInFrontOfPartialOnAClassIsNotReported()
    {
        const string source = """
                              /// <summary>
                              /// Doc
                              /// </summary>
                              public closed partial class Gate
                              {
                              }
                              """;

        await Verify(source, ClosedTypes);
    }

    /// <summary>
    /// Verifies that a <c>closed</c> partial record is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7105-002")]
    public async Task ClosedInFrontOfPartialOnARecordIsNotReported()
    {
        const string source = """
                              public closed partial record R;
                              """;

        await Verify(source, ClosedTypes);
    }

    /// <summary>
    /// Verifies that <c>closed</c> in front of <c>new</c> and <c>partial</c> on a nested class is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7105-002")]
    public async Task ClosedInFrontOfNewAndPartialIsNotReported()
    {
        const string source = """
                              public class A
                              {
                                  public class N
                                  {
                                  }
                              }

                              public class B : A
                              {
                                  public closed new partial class N
                                  {
                                  }
                              }
                              """;

        await Verify(source, ClosedTypes);
    }

    /// <summary>
    /// Verifies that <c>closed</c> in front of <c>unsafe</c> and <c>partial</c> is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7105-002")]
    public async Task ClosedInFrontOfUnsafeAndPartialIsNotReported()
    {
        const string source = """
                              internal closed unsafe partial class U
                              {
                              }
                              """;

        await Verify(source,
                     test =>
                     {
                         AllowUnsafe(test);
                         ClosedTypes(test);
                     });
    }

    /// <summary>
    /// Verifies that <c>new</c> in front of <c>closed</c> is reported on <c>new</c> and fixed to <c>closed new partial</c>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7105-001")]
    [Requirement("RH7105-003")]
    [Requirement("RH7105-004")]
    public async Task NewInFrontOfClosedIsReportedOnNewAndFixed()
    {
        const string source = """
                              public class A
                              {
                                  public class N
                                  {
                                  }
                              }

                              public class B : A
                              {
                                  public {|RH7105:new|} closed partial class N
                                  {
                                  }
                              }
                              """;

        const string fixedSource = """
                                   public class A
                                   {
                                       public class N
                                       {
                                       }
                                   }

                                   public class B : A
                                   {
                                       public closed new partial class N
                                       {
                                       }
                                   }
                                   """;

        await Verify(source, fixedSource, ClosedTypesInFix);
    }

    /// <summary>
    /// Verifies that <c>unsafe</c> in front of <c>closed</c> is reported on <c>unsafe</c> and fixed to
    /// <c>closed unsafe partial</c>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7105-001")]
    [Requirement("RH7105-003")]
    [Requirement("RH7105-004")]
    public async Task UnsafeInFrontOfClosedIsReportedOnUnsafeAndFixed()
    {
        const string source = """
                              internal {|RH7105:unsafe|} closed partial class U
                              {
                              }
                              """;

        const string fixedSource = """
                                   internal closed unsafe partial class U
                                   {
                                   }
                                   """;

        await Verify(source,
                     fixedSource,
                     test =>
                     {
                         test.SolutionTransforms.Add(ApplyAllowUnsafeToTestProject);
                         ClosedTypesInFix(test);
                     });
    }

    /// <summary>
    /// Verifies that <c>closed</c> in front of the accessibility is reported on <c>closed</c> and fixed behind it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7105-001")]
    [Requirement("RH7105-003")]
    [Requirement("RH7105-004")]
    public async Task ClosedInFrontOfTheAccessibilityIsReportedAndFixed()
    {
        const string source = """
                              {|RH7105:closed|} public class Gate
                              {
                              }
                              """;

        const string fixedSource = """
                                   public closed class Gate
                                   {
                                   }
                                   """;

        await Verify(source, fixedSource, ClosedTypesInFix);
    }

    /// <summary>
    /// Verifies that <c>closed</c> behind the accessibility on a class, a record class, and a file-local class without
    /// <c>partial</c> is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7105-002")]
    public async Task ClosedBehindTheAccessibilityWithoutPartialIsNotReported()
    {
        const string source = """
                              public closed class A
                              {
                              }

                              public closed record class R;

                              file closed class F
                              {
                              }
                              """;

        await Verify(source, ClosedTypes);
    }

    /// <summary>
    /// Verifies the RH7105-001 example for <c>closed</c> and the preview <c>safe</c>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7105-001")]
    [Requirement("RH7105-003")]
    public async Task SpecExampleClosedAndSafeOutOfOrderAreReported()
    {
        const string source = """
                              public class C
                              {
                                  public {|RH7105:new|} closed partial class N
                                  {
                                  }

                                  public static {|RH7105:safe|} extern void M();
                              }
                              """;

        await Verify(source,
                     test =>
                     {
                         PreviewLanguageVersion(test);
                         ClosedTypes(test);
                     });
    }

    /// <summary>
    /// Verifies the RH7105-002 example for <c>closed</c> and the preview <c>safe</c>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7105-002")]
    public async Task SpecExampleClosedAndSafeInOrderAreNotReported()
    {
        const string source = """
                              public closed partial class A
                              {
                                  public closed new partial class N
                                  {
                                  }
                              }

                              internal closed unsafe partial class U
                              {
                              }

                              public partial class C
                              {
                                  public static extern safe void M();

                                  public safe partial void P();

                                  public safe partial void P()
                                  {
                                  }

                                  public safe required int R { get; set; }
                              }

                              public safe ref struct S
                              {
                              }
                              """;

        await Verify(source,
                     test =>
                     {
                         AllowUnsafe(test);
                         PreviewLanguageVersion(test);
                         ClosedTypes(test);
                     });
    }

    /// <summary>
    /// Verifies the RH7105-004 example for <c>closed</c> and the preview <c>safe</c>: both declarations are fixed, and the
    /// fixed code compiles
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7105-004")]
    public async Task SpecExampleClosedAndSafeAreFixed()
    {
        const string source = """
                              public class C
                              {
                                  public {|RH7105:new|} closed partial class N
                                  {
                                  }

                                  public static {|RH7105:safe|} extern void M();
                              }
                              """;

        const string fixedSource = """
                                   public class C
                                   {
                                       public closed new partial class N
                                       {
                                       }

                                       public static extern safe void M();
                                   }
                                   """;

        await Verify(source,
                     fixedSource,
                     test =>
                     {
                         test.SolutionTransforms.Add(ApplyPreviewLanguageVersionToTestProject);
                         ClosedTypesInFix(test);
                     });
    }

    /// <summary>
    /// Verifies that both parts of a preview <c>safe</c> partial method with <c>safe</c> in front of <c>partial</c> are
    /// not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7105-001")]
    [Requirement("RH7105-002")]
    public async Task SafeInFrontOfPartialOnAMethodIsNotReported()
    {
        const string source = """
                              public partial class C
                              {
                                  public safe partial void P();

                                  public safe partial void P()
                                  {
                                  }
                              }
                              """;

        await Verify(source, PreviewLanguageVersion);
    }

    /// <summary>
    /// Verifies that the preview <c>safe</c> in front of <c>ref</c> on a struct is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7105-001")]
    [Requirement("RH7105-002")]
    public async Task SafeInFrontOfRefOnAStructIsNotReported()
    {
        const string source = """
                              public safe ref struct S
                              {
                              }
                              """;

        await Verify(source, PreviewLanguageVersion);
    }

    /// <summary>
    /// Verifies that the preview <c>safe</c> in front of <c>required</c> is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7105-002")]
    public async Task SafeInFrontOfRequiredIsNotReported()
    {
        const string source = """
                              public class C
                              {
                                  public safe required int R { get; set; }
                              }
                              """;

        await Verify(source, PreviewLanguageVersion);
    }

    /// <summary>
    /// Verifies that <c>required</c> in front of the preview <c>safe</c> is reported on <c>required</c> and fixed to
    /// <c>safe required</c>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7105-001")]
    [Requirement("RH7105-003")]
    [Requirement("RH7105-004")]
    public async Task RequiredInFrontOfSafeIsReportedOnRequiredAndFixed()
    {
        const string source = """
                              public class C
                              {
                                  public {|RH7105:required|} safe int R { get; set; }
                              }
                              """;

        const string fixedSource = """
                                   public class C
                                   {
                                       public safe required int R { get; set; }
                                   }
                                   """;

        await Verify(source, fixedSource, test => test.SolutionTransforms.Add(ApplyPreviewLanguageVersionToTestProject));
    }

    /// <summary>
    /// Verifies that the preview <c>safe</c> in front of <c>extern</c> is reported on <c>safe</c> and fixed behind
    /// <c>extern</c>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7105-001")]
    [Requirement("RH7105-003")]
    [Requirement("RH7105-004")]
    public async Task SafeInFrontOfExternIsReportedOnSafeAndFixed()
    {
        const string source = """
                              public class C
                              {
                                  public static {|RH7105:safe|} extern void M();
                              }
                              """;

        const string fixedSource = """
                                   public class C
                                   {
                                       public static extern safe void M();
                                   }
                                   """;

        await Verify(source, fixedSource, test => test.SolutionTransforms.Add(ApplyPreviewLanguageVersionToTestProject));
    }

    /// <summary>
    /// Verifies that the preview <c>safe</c> in front of <c>async</c> is reported on <c>safe</c> and fixed behind
    /// <c>async</c>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7105-001")]
    [Requirement("RH7105-003")]
    [Requirement("RH7105-004")]
    public async Task SafeInFrontOfAsyncIsReportedOnSafeAndFixed()
    {
        const string source = """
                              using System.Threading.Tasks;

                              public class C
                              {
                                  public {|RH7105:safe|} async Task M()
                                  {
                                      await Task.CompletedTask;
                                  }
                              }
                              """;

        const string fixedSource = """
                                   using System.Threading.Tasks;

                                   public class C
                                   {
                                       public async safe Task M()
                                       {
                                           await Task.CompletedTask;
                                       }
                                   }
                                   """;

        await Verify(source, fixedSource, test => test.SolutionTransforms.Add(ApplyPreviewLanguageVersionToTestProject));
    }

    /// <summary>
    /// Verifies that the preview <c>safe</c> in front of <c>readonly</c> on a ref partial struct is reported on
    /// <c>safe</c> and fixed to <c>readonly safe ref partial</c>, which compiles
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7105-001")]
    [Requirement("RH7105-003")]
    [Requirement("RH7105-004")]
    public async Task SafeInFrontOfReadonlyOnARefPartialStructIsReportedAndFixed()
    {
        const string source = """
                              public {|RH7105:safe|} readonly ref partial struct S
                              {
                              }
                              """;

        const string fixedSource = """
                                   public readonly safe ref partial struct S
                                   {
                                   }
                                   """;

        await Verify(source, fixedSource, test => test.SolutionTransforms.Add(ApplyPreviewLanguageVersionToTestProject));
    }

    /// <summary>
    /// Verifies the union example of SHARED-MODORDER-001 for RH7105: the modifiers of a union out of the canonical
    /// order are reported, and the fix writes the canonical order
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-MODORDER-001")]
    [Requirement("RH7105-001")]
    [Requirement("RH7105-004")]
    public async Task UnionModifiersOutOfOrderAreReportedAndOrdered()
    {
        const string testCode = """
                                {|RH7105:readonly|} public union U(int, string);

                                public class C
                                {
                                    {|RH7105:readonly|} private union N(int, string);
                                }
                                """;

        const string fixedCode = """
                                 public readonly union U(int, string);

                                 public class C
                                 {
                                     private readonly union N(int, string);
                                 }
                                 """;

        var unionSupport = await UnionSupport.GetReferenceAsync();

        await Verify(testCode, fixedCode, test => test.TestState.AdditionalReferences.Add(unionSupport));
    }

    #endregion // Tests

    #region Methods

    /// <summary>
    /// Defines the preprocessor symbol <c>DEBUG</c> for the test project
    /// </summary>
    /// <param name="solution">Solution</param>
    /// <param name="projectId">Project ID</param>
    /// <returns>Solution</returns>
    private static Solution DefineDebug(Solution solution, ProjectId projectId)
    {
        return solution.GetProject(projectId)?.ParseOptions is CSharpParseOptions parseOptions
                   ? solution.WithProjectParseOptions(projectId, parseOptions.WithPreprocessorSymbols("DEBUG"))
                   : solution;
    }

    /// <summary>
    /// Enables unsafe code for the verifier so declarations using the <c>unsafe</c> modifier compile
    /// </summary>
    /// <param name="test">Test</param>
    private static void AllowUnsafe(CSharpAnalyzerVerifierTest<RH7105DeclarationKeywordsMustFollowOrderAnalyzer> test)
    {
        test.SolutionTransforms.Add(ApplyAllowUnsafeToTestProject);
    }

    /// <summary>
    /// Adds the attribute the compiler requires for a <c>closed</c> class, which the verifier's reference assemblies do
    /// not declare, so declarations using the C# 15 <c>closed</c> modifier compile
    /// </summary>
    /// <param name="test">Test</param>
    private static void ClosedTypes(CSharpAnalyzerVerifierTest<RH7105DeclarationKeywordsMustFollowOrderAnalyzer> test)
    {
        test.TestState.Sources.Add(("/0/IsClosedTypeAttribute.cs", IsClosedTypeAttributeSource));
    }

    /// <summary>
    /// Adds the attribute the compiler requires for a <c>closed</c> class to the source and to the fixed code, so
    /// declarations using the C# 15 <c>closed</c> modifier compile before and after the fix
    /// </summary>
    /// <param name="test">Test</param>
    private static void ClosedTypesInFix(CSharpCodeFixVerifierTest<RH7105DeclarationKeywordsMustFollowOrderAnalyzer, RH7105DeclarationKeywordsMustFollowOrderCodeFixProvider> test)
    {
        test.TestState.Sources.Add(("/0/IsClosedTypeAttribute.cs", IsClosedTypeAttributeSource));
        test.FixedState.Sources.Add(("/0/IsClosedTypeAttribute.cs", IsClosedTypeAttributeSource));
    }

    /// <summary>
    /// Sets the language version of the verifier's project to preview, so declarations using the preview <c>safe</c>
    /// modifier compile
    /// </summary>
    /// <param name="test">Test</param>
    private static void PreviewLanguageVersion(CSharpAnalyzerVerifierTest<RH7105DeclarationKeywordsMustFollowOrderAnalyzer> test)
    {
        test.SolutionTransforms.Add(ApplyPreviewLanguageVersionToTestProject);
    }

    /// <summary>
    /// Sets the language version of the test project to preview
    /// </summary>
    /// <param name="solution">Solution</param>
    /// <param name="projectId">Project ID</param>
    /// <returns>Solution</returns>
    private static Solution ApplyPreviewLanguageVersionToTestProject(Solution solution, ProjectId projectId)
    {
        return solution.GetProject(projectId)?.ParseOptions is CSharpParseOptions parseOptions
                   ? solution.WithProjectParseOptions(projectId, parseOptions.WithLanguageVersion(LanguageVersion.Preview))
                   : solution;
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
                                  Diagnostics(RH7105DeclarationKeywordsMustFollowOrderAnalyzer.DiagnosticId, MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}