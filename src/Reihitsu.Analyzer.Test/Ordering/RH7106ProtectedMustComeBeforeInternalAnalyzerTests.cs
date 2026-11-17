using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Organization;
using Reihitsu.Analyzer.Rules.Organization;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Ordering;

/// <summary>
/// Test methods for <see cref="RH7106ProtectedMustComeBeforeInternalAnalyzer"/> and <see cref="RH7106ProtectedMustComeBeforeInternalCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH7106ProtectedMustComeBeforeInternalAnalyzerTests : BatchCodeFixTestsBase<RH7106ProtectedMustComeBeforeInternalAnalyzer, RH7106ProtectedMustComeBeforeInternalCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying compound access modifiers are reported and fixed when the keyword order is wrong
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7106-001")]
    [Requirement("RH7106-003")]
    [Requirement("RH7106-004")]
    public async Task MisorderedCompoundAccessModifiersAreReportedAndFixed()
    {
        const string testCode = """
                                public class TestClass
                                {
                                    internal {|#0:protected|} void Execute()
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class TestClass
                                 {
                                     protected internal void Execute()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH7106ProtectedMustComeBeforeInternalAnalyzer.DiagnosticId, AnalyzerResources.RH7106MessageFormat));
    }

    /// <summary>
    /// Verifies that a misordered compound access modifier with an empty conditional directive between its two
    /// modifiers is reported, with and without the symbol defined, and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7106-001")]
    [Requirement("SHARED-DIAG-008")]
    public async Task MisorderedCompoundAccessModifiersSplitByAConditionalDirectiveAreReportedWithoutFix()
    {
        const string testData = """
                                public class TestClass
                                {
                                    internal
                                #if FEATURE
                                #endif
                                    {|RH7106:protected|} void Execute()
                                    {
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
        await VerifyNoCodeFixIsOffered(testData, DocumentationMode.Parse, "FEATURE");
    }

    /// <summary>
    /// Verifies that a misordered compound access modifier is reported, and no code fix is offered, for every shape of
    /// conditional directive between its two keywords: the analyzer judges the modifiers the analyzed build compiles
    /// </summary>
    /// <param name="directives">The conditional directives, and the code they hold, between <c>internal</c> and the type</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("#if FEATURE\n#endif\n    {|RH7106:protected|}", DisplayName = "Empty block")]
    [DataRow("#if NEVER\n    static\n#endif\n    {|RH7106:protected|}", DisplayName = "Excluded modifier")]
    [DataRow("#if !NEVER\n    {|RH7106:protected|}\n#endif", DisplayName = "Active modifier")]
    [DataRow("#if NEVER\n    static\n#else\n    {|RH7106:protected|}\n#endif", DisplayName = "If and else")]
    [DataRow("#if NEVER\n    static\n#elif !NEVER\n    {|RH7106:protected|}\n#endif", DisplayName = "If and elif")]
    [DataRow("#if false\n    static\n#endif\n    {|RH7106:protected|}", DisplayName = "If false")]
    [Requirement("RH7106-001")]
    [Requirement("SHARED-DIAG-008")]
    [Requirement("SHARED-MODORDER-003")]
    public async Task ConditionalDirectiveBetweenTheKeywordsKeepsTheReportAndWithholdsTheFix(string directives)
    {
        var testData = $$"""
                         class C
                         {
                             internal
                         {{directives}}
                             int _a;
                         }
                         """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that Fix All corrects the declaration without a conditional directive between its modifiers and leaves
    /// the one with such a directive reported and unchanged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7106-001")]
    [Requirement("SHARED-DIAG-008")]
    public async Task FixAllCorrectsOnlyTheDeclarationWithoutAConditionalDirectiveBetweenItsModifiers()
    {
        const string testData = """
                                class C
                                {
                                    internal
                                #if FEATURE
                                #endif
                                    {|RH7106:protected|} int _a;

                                    internal {|RH7106:protected|} int _b;

                                    internal {|RH7106:protected|} int _c;
                                }
                                """;

        const string fixedData = """
                                 class C
                                 {
                                     internal
                                 #if FEATURE
                                 #endif
                                     {|RH7106:protected|} int _a;

                                     protected internal int _b;

                                     protected internal int _c;
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
    /// Verifying the specification example: <c>internal protected</c> and <c>protected private</c> are reported on the
    /// second keyword, also with another modifier between the two keywords
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7106-001")]
    [Requirement("RH7106-003")]
    public async Task CompoundAccessibilitiesInTheOtherOrderAreReportedOnTheSecondKeyword()
    {
        const string source = """
                              class C
                              {
                                  internal {|RH7106:protected|} int A;
                                  protected {|RH7106:private|} int B;
                                  internal static {|RH7106:protected|} int D;
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifying the specification example: <c>protected internal</c> and <c>private protected</c> are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7106-002")]
    public async Task CanonicalCompoundAccessibilitiesAreNotReported()
    {
        const string source = """
                              class C
                              {
                                  protected internal int A;
                                  private protected int B;
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifying the specification example: the fix swaps the two keywords and leaves every other modifier at its
    /// position
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7106-004")]
    [Requirement("SHARED-MODORDER-002")]
    public async Task FixSwapsTheTwoKeywordsInPlace()
    {
        const string source = """
                              class C
                              {
                                  internal {|RH7106:protected|} void M()
                                  {
                                  }

                                  internal static {|RH7106:protected|} int D;
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                       protected internal void M()
                                       {
                                       }

                                       protected static internal int D;
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifying that <c>protected private</c> is fixed to <c>private protected</c>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7106-004")]
    public async Task FixSwapsProtectedPrivate()
    {
        const string source = """
                              class C
                              {
                                  protected {|RH7106:private|} int B;
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                       private protected int B;
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifying the shared specification example of a partial property: the part that writes <c>internal protected</c>
    /// is reported although the other part writes the same accessibility in the canonical order, and the fix swaps the
    /// keywords of the reported part only and leaves the other part as it is
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7106-001")]
    [Requirement("RH7106-004")]
    [Requirement("SHARED-DIAG-004")]
    public async Task PartOfAPartialPropertyIsReportedAndFixedOnItsOwn()
    {
        const string source = """
                              public partial class C
                              {
                                  internal {|RH7106:protected|} partial int P { get; }

                                  protected internal partial int P => 1;
                              }
                              """;

        const string fixedSource = """
                                   public partial class C
                                   {
                                       protected internal partial int P { get; }

                                       protected internal partial int P => 1;
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifying the shared specification example: a struct, an interface, a record, a record struct, a delegate, a
    /// constructor, an event, and an indexer are checked
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-MODORDER-001")]
    [Requirement("RH7106-001")]
    public async Task EveryCheckedDeclarationKindIsReported()
    {
        const string source = """
                              using System;

                              public class C
                              {
                                  internal {|RH7106:protected|} struct S
                                  {
                                  }

                                  internal {|RH7106:protected|} interface I
                                  {
                                  }

                                  internal {|RH7106:protected|} record R;

                                  internal {|RH7106:protected|} record struct RS;

                                  internal {|RH7106:protected|} delegate void D();

                                  internal {|RH7106:protected|} C()
                                  {
                                  }

                                  internal {|RH7106:protected|} event Action E
                                  {
                                      add
                                      {
                                      }

                                      remove
                                      {
                                      }
                                  }

                                  internal {|RH7106:protected|} int this[int index] => index;
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifying the shared specification example: a pragma directive in front of a later modifier withholds the fix,
    /// and the diagnostic is still reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-MODORDER-003")]
    [Requirement("RH7106-004")]
    [Requirement("SHARED-DIAG-008")]
    public async Task PragmaDirectiveInFrontOfALaterModifierWithholdsTheFix()
    {
        const string source = """
                              class C
                              {
                                  internal
                              #pragma warning disable CS0169
                                  {|RH7106:protected|} int _value;
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifying that a comment in front of the first modifier does not withhold the fix and stays in front of the new
    /// first modifier
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-MODORDER-002")]
    [Requirement("SHARED-MODORDER-003")]
    public async Task CommentInFrontOfTheFirstModifierDoesNotWithholdTheFix()
    {
        const string source = """
                              class C
                              {
                                  /* lead */ internal {|RH7106:protected|} int A;
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                       /* lead */ protected internal int A;
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifying that RH7106 checks the class, field, event field, property, and method declarations too, so together
    /// with <see cref="EveryCheckedDeclarationKindIsReported"/> and
    /// <see cref="OperatorsAreCheckedEvenThoughTheyCannotCompileWithACompoundAccessibility"/> it checks every
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
                                  internal {|RH7106:protected|} class N
                                  {
                                  }

                                  internal {|RH7106:protected|} int F;

                                  internal {|RH7106:protected|} event Action E;

                                  internal {|RH7106:protected|} int P { get; set; }

                                  internal {|RH7106:protected|} void M()
                                  {
                                  }
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifying that RH7106 checks operator and conversion operator declarations. Operators must be public, so the
    /// input does not compile and only the reported diagnostics are checked
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-MODORDER-001")]
    public async Task OperatorsAreCheckedEvenThoughTheyCannotCompileWithACompoundAccessibility()
    {
        const string source = """
                              public class C
                              {
                                  internal {|RH7106:protected|} static C operator +(C a, C b)
                                  {
                                      return a;
                                  }

                                  internal {|RH7106:protected|} static implicit operator int(C c)
                                  {
                                      return 0;
                                  }
                              }
                              """;

        await Verify(source, test => test.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifying that the part of a partial nested class that writes <c>internal protected</c> is reported on that part
    /// although the first part writes the same accessibility in the canonical order, and that the fix swaps the keywords
    /// of the reported part only and leaves the first part as it is
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7106-001")]
    [Requirement("RH7106-004")]
    [Requirement("SHARED-DIAG-004")]
    public async Task PartOfAPartialTypeIsReportedAndFixedOnItsOwn()
    {
        const string source = """
                              public class Outer
                              {
                                  protected internal partial class Inner
                                  {
                                      public const int A = 1;
                                  }

                                  internal {|RH7106:protected|} partial class Inner
                                  {
                                      public const int B = 2;
                                  }
                              }
                              """;

        const string fixedSource = """
                                   public class Outer
                                   {
                                       protected internal partial class Inner
                                       {
                                           public const int A = 1;
                                       }

                                       protected internal partial class Inner
                                       {
                                           public const int B = 2;
                                       }
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifying that a directive between the last modifier and the type withholds the fix, and the diagnostic is still
    /// reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-MODORDER-003")]
    public async Task DirectiveBehindTheLastModifierWithholdsTheFix()
    {
        const string source = """
                              class C
                              {
                                  internal {|RH7106:protected|}
                              #pragma warning disable CS0169
                                  int _a;
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifying that a comment between the last modifier and the type withholds the fix, and the diagnostic is still
    /// reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-MODORDER-003")]
    public async Task CommentBetweenTheLastModifierAndTheTypeWithholdsTheFix()
    {
        const string source = """
                              class C
                              {
                                  internal {|RH7106:protected|}
                                  // the counter
                                  int _a;
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifying the reported example: the fix swaps only the two accessibility keywords, and the spacing and the expression
    /// body of the method stay as written
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-FIX-006")]
    public async Task FixKeepsTheSpacingAndTheExpressionBodyOfTheMethod(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  internal {|RH7106:protected|} int M( ) => 1;
                              }
                              """;

        const string fixedSource = """
                                   public class C
                                   {
                                       protected internal int M( ) => 1;
                                   }
                                   """;

        await VerifyCodeFixWithEndOfLine(source, fixedSource, endOfLine);
    }

    /// <summary>
    /// Verifies the specification example in which neither a conditional directive between two modifiers nor one behind
    /// the last modifier keeps the declaration from being reported, and no code fix is offered for either
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7106-001")]
    [Requirement("SHARED-DIAG-008")]
    [Requirement("SHARED-MODORDER-003")]
    public async Task ConditionalDirectiveBehindTheLastModifierDoesNotKeepTheDeclarationFromBeingReported()
    {
        const string testData = """
                                class C
                                {
                                    internal
                                #if FEATURE
                                #endif
                                    {|RH7106:protected|} int A;

                                    internal {|RH7106:protected|}
                                #if DEBUG
                                    static
                                #endif
                                    int B;
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
    [Requirement("RH7106-001")]
    [Requirement("SHARED-DIAG-008")]
    public async Task ConditionalDirectiveThatStandsBetweenModifiersWhereTheSymbolIsDefinedKeepsTheReportAndWithholdsTheFix()
    {
        const string testData = """
                                class C
                                {
                                    internal {|RH7106:protected|}
                                #if DEBUG
                                    static
                                #endif
                                    int B;
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData, DocumentationMode.Parse, "DEBUG");
    }

    /// <summary>
    /// Verifies that both parts of a partial constructor are reported, each on its own modifiers, and the code fix
    /// corrects each part
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7106-001")]
    [Requirement("SHARED-DIAG-004")]
    public async Task BothPartsOfAPartialConstructorAreReported()
    {
        const string testData = """
                                public partial class C
                                {
                                    internal {|RH7106:protected|} partial C();

                                    internal {|RH7106:protected|} partial C()
                                    {
                                    }
                                }
                                """;

        const string fixedData = """
                                 public partial class C
                                 {
                                     protected internal partial C();

                                     protected internal partial C()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData);
    }

    /// <summary>
    /// Verifies that the defining part of a partial constructor is reported when its implementing part lies in another
    /// file
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7106-001")]
    [Requirement("SHARED-DIAG-004")]
    public async Task DefiningPartOfAPartialConstructorIsReportedWhenTheImplementingPartIsInAnotherFile()
    {
        const string definingPart = """
                                    public partial class C
                                    {
                                        internal {|RH7106:protected|} partial C();
                                    }
                                    """;

        const string implementingPart = """
                                        public partial class C
                                        {
                                            internal {|RH7106:protected|} partial C()
                                            {
                                            }
                                        }
                                        """;

        await Verify(definingPart,
                     test =>
                     {
                         test.TestState.Sources.Add(("/0/Test1.cs", implementingPart));
                     });
    }

    /// <summary>
    /// Verifies that the defining part of a partial constructor is reported when its implementing part lies in
    /// generated code
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7106-001")]
    [Requirement("SHARED-DIAG-004")]
    public async Task DefiningPartOfAPartialConstructorIsReportedWhenTheImplementingPartIsGenerated()
    {
        const string definingPart = """
                                    public partial class C
                                    {
                                        internal {|RH7106:protected|} partial C();
                                    }
                                    """;

        const string implementingPart = """
                                        public partial class C
                                        {
                                            internal protected partial C()
                                            {
                                            }
                                        }
                                        """;

        await Verify(definingPart,
                     test =>
                     {
                         test.TestState.Sources.Add(("/0/C.g.cs", implementingPart));
                     });
    }

    /// <summary>
    /// Verifies that both parts of a partial constructor of a record are reported, each on its own modifiers
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7106-001")]
    [Requirement("SHARED-DIAG-004")]
    public async Task BothPartsOfAPartialConstructorOfARecordAreReported()
    {
        const string testData = """
                                public partial record R
                                {
                                    internal {|RH7106:protected|} partial R(int value);

                                    internal {|RH7106:protected|} partial R(int value)
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a conditional directive in front of the first modifier, the <c>#endif</c> of a conditionally
    /// compiled member in front of the declaration, keeps the declaration neither from being reported nor from being
    /// fixed, whether the symbol is defined or not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7106-001")]
    [Requirement("RH7106-004")]
    [Requirement("SHARED-MODORDER-002")]
    public async Task ConditionalDirectiveInFrontOfTheFirstModifierDoesNotKeepTheDeclarationFromBeingReportedOrFixed()
    {
        const string source = """
                              class C
                              {
                              #if DEBUG
                                  void Log() { }
                              #endif
                                  internal {|RH7106:protected|} int A;
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                   #if DEBUG
                                       void Log() { }
                                   #endif
                                       protected internal int A;
                                   }
                                   """;

        await Verify(source, fixedSource);
        await Verify(source, fixedSource, test => test.SolutionTransforms.Add(DefineDebug));
    }

    /// <summary>
    /// Verifies the union example of SHARED-MODORDER-001 for RH7106: internal protected on a union is reported on
    /// protected, and the fix swaps the two keywords
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-MODORDER-001")]
    [Requirement("RH7106-001")]
    [Requirement("RH7106-004")]
    public async Task UnionInternalProtectedIsReportedAndSwapped()
    {
        const string testCode = """
                                public class C
                                {
                                    internal {|RH7106:protected|} union V(int, string);
                                }
                                """;

        const string fixedCode = """
                                 public class C
                                 {
                                     protected internal union V(int, string);
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

    #endregion // Methods

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                public class TestClass
                                {
                                    internal {|#0:protected|} void Execute()
                                    {
                                    }

                                    internal {|#1:protected|} void Run()
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class TestClass
                                 {
                                     protected internal void Execute()
                                     {
                                     }

                                     protected internal void Run()
                                     {
                                     }
                                 }
                                 """;

        // Two compound access modifiers inside one type, so both corrections are merged into the same type
        // declaration and each one has to leave the other member's modifier list untouched
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH7106ProtectedMustComeBeforeInternalAnalyzer.DiagnosticId, AnalyzerResources.RH7106MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}