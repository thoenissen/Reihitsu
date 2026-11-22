using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Organization;
using Reihitsu.Analyzer.Rules.Organization;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Ordering;

/// <summary>
/// Test methods for <see cref="RH7109ReadonlyElementsMustAppearBeforeNonReadonlyElementsCodeFixProvider"/> on conditional directives and excluded code in and between the moved and the crossed fields
/// </summary>
[TestClass]
public class RH7109ReadonlyElementsMustAppearBeforeNonReadonlyElementsExcludedCodeTests : BatchCodeFixTestsBase<RH7109ReadonlyElementsMustAppearBeforeNonReadonlyElementsAnalyzer, RH7109ReadonlyElementsMustAppearBeforeNonReadonlyElementsCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying the shared specification example: without <c>DEBUG</c> defined, the move would cross an initialized field
    /// of excluded code, whose initializer a build with <c>DEBUG</c> would then run after the moved one, so no code fix
    /// is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-ORDER-007")]
    [Requirement("RH7109-006")]
    public async Task NoCodeFixWhenTheMoveCrossesAnInitializedFieldOfExcludedCode()
    {
        const string source = """
                              public class C
                              {
                                  private int _a;
                              #if DEBUG
                                  private int _c = Log("c");
                              #endif
                                  private int _d;
                                  private readonly int {|RH7109:_b|} = Log("b");

                                  private static int Log(string name) => 0;
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifying that a field of excluded code without an initializer does not withhold the move that crosses it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-ORDER-007")]
    public async Task FixCrossesAFieldOfExcludedCodeWithoutInitializer()
    {
        const string testCode = """
                                public class C
                                {
                                    private int _a;
                                #if DEBUG
                                    private int _c;
                                #endif
                                    private int _d;
                                    private readonly int {|RH7109:_b|} = Log("b");

                                    private static int Log(string name)
                                    {
                                        return 0;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class C
                                 {
                                     private readonly int _b = Log("b");
                                     private int _a;
                                 #if DEBUG
                                     private int _c;
                                 #endif
                                     private int _d;

                                     private static int Log(string name)
                                     {
                                         return 0;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifying the shared specification example: excluded code in front of the moved field moves with it, so the order
    /// of the initializers stays the same in every build, and the fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-ORDER-007")]
    public async Task FixMovesExcludedCodeInFrontOfTheMovedFieldWithIt()
    {
        const string testCode = """
                                public class C
                                {
                                    private int _a;
                                    private int _d;
                                #if DEBUG
                                    private int _c = Log("c");
                                #endif
                                    private readonly int {|RH7109:_b|} = Log("b");

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
                                     private int _c = Log("c");
                                 #endif
                                     private readonly int _b = Log("b");
                                     private int _a;
                                     private int _d;

                                     private static int Log(string name)
                                     {
                                         return 0;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifying that a field of excluded code in front of the moved field moves with it and counts as moved: without
    /// <c>DEBUG</c> defined, the move would carry its initializer across an initialized field, whose initializer a build
    /// with <c>DEBUG</c> would then run after it, so no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-ORDER-007")]
    [Requirement("RH7109-006")]
    public async Task NoCodeFixWhenExcludedCodeInFrontOfTheMovedFieldWouldCarryAnInitializerAcrossAnInitializedField()
    {
        const string source = """
                              public class C
                              {
                                  private int _a = Log("a");
                              #if DEBUG
                                  private int _c = Log("c");
                              #endif
                                  private readonly int {|RH7109:_b|};

                                  private static int Log(string name) => 0;
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifying the other side of the condition: an initialized field of excluded code in front of the moved field moves
    /// with it across a field without initializer, so the order of the initializers stays the same in every build, and
    /// the fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-ORDER-007")]
    public async Task FixCarriesAnInitializedFieldOfExcludedCodeInFrontOfTheMovedFieldAcrossAFieldWithoutInitializer()
    {
        const string testCode = """
                                public class C
                                {
                                    private int _a;
                                #if DEBUG
                                    private int _c = Log("c");
                                #endif
                                    private readonly int {|RH7109:_b|};

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
                                     private int _c = Log("c");
                                 #endif
                                     private readonly int _b;
                                     private int _a;

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
    [Requirement("SHARED-ORDER-007")]
    [Requirement("SHARED-FIX-003")]
    [Requirement("RH7109-006")]
    public async Task NoCodeFixWhenExcludedCodeInFrontOfTheMovedStaticFieldWouldReorderTheInstanceFieldsOfAStruct()
    {
        const string source = """
                              public struct S
                              {
                                  private static int _s;
                                  private int _x;
                              #if DEBUG
                                  private int _y;
                              #endif
                                  private static readonly int {|RH7109:_r|};
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
    [Requirement("SHARED-ORDER-007")]
    [Requirement("SHARED-FIX-003")]
    public async Task FixCarriesAnInstanceFieldOfExcludedCodeInFrontOfTheMovedStaticFieldOfAClass()
    {
        const string testCode = """
                                public class S
                                {
                                    private static int _s;
                                    private int _x;
                                #if DEBUG
                                    private int _y;
                                #endif
                                    private static readonly int {|RH7109:_r|};
                                }
                                """;

        const string fixedCode = """
                                 public class S
                                 {
                                 #if DEBUG
                                     private int _y;
                                 #endif
                                     private static readonly int _r;
                                     private static int _s;
                                     private int _x;
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifying the other side of the condition: without excluded code in front of it, the moved static field crosses
    /// the instance field of a struct but reorders no instance field, and the fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-003")]
    public async Task FixMovesAStaticFieldOfAStructAcrossAnInstanceField()
    {
        const string testCode = """
                                public struct S
                                {
                                    private static int _s;
                                    private int _x;
                                    private static readonly int {|RH7109:_r|};
                                }
                                """;

        const string fixedCode = """
                                 public struct S
                                 {
                                     private static readonly int _r;
                                     private static int _s;
                                     private int _x;
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifying the other side of the crossing condition: an instance field of excluded code in front of the moved
    /// static field of a struct moves with it, but the move crosses only a static field and reorders no instance field,
    /// so the fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-ORDER-007")]
    [Requirement("RH7109-006")]
    public async Task FixCarriesAnInstanceFieldOfExcludedCodeInFrontOfTheMovedStaticFieldOfAStructAcrossOnlyAStaticField()
    {
        const string testCode = """
                                public struct S
                                {
                                    private int _x;
                                    private static int _s;
                                #if DEBUG
                                    private int _y;
                                #endif
                                    private static readonly int {|RH7109:_r|};
                                }
                                """;

        const string fixedCode = """
                                 public struct S
                                 {
                                     private int _x;
                                 #if DEBUG
                                     private int _y;
                                 #endif
                                     private static readonly int _r;
                                     private static int _s;
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifying that a conditional group that completes the moved field's initializer stands between two tokens of the
    /// declaration, at no boundary of whole units, so no code fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task NoCodeFixWhenTheMovedFieldHasAnInitializerOnlyInAnotherConfiguration(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private int _a = Log("a");
                                  private readonly int {|RH7109:_b|}
                              #if DEBUG
                                      = Log("b")
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
    /// Verifying that a conditional group that completes a crossed field's initializer stands between two tokens of the
    /// declaration, at no boundary of whole units, so no code fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task NoCodeFixWhenTheCrossedFieldHasAnInitializerOnlyInAnotherConfiguration(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private int _a
                              #if DEBUG
                                      = Log("a")
                              #endif
                                      ;
                                  private readonly int {|RH7109:_b|} = Log("b");

                                  private static int Log(string name)
                                  {
                                      return 0;
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying that a conditional group in front of a crossed member whose excluded branch holds the initializer of the
    /// property before it, which is neither whole members nor whole attribute lists, withholds the fix
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task NoCodeFixWhenACrossedPropertyHasAnInitializerBehindItsAccessorListOnlyInAnotherConfiguration(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private int _a;
                                  private int Count { get; set; }
                              #if DEBUG
                                      = Log("p");
                              #endif
                                  private int _c;
                                  private readonly int {|RH7109:_b|} = Log("b");

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
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task NoCodeFixWhenACrossedFieldIsStaticOnlyInAGroupInFrontOfItsFirstToken(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private static int _t;
                              #if DEBUG
                                  private static
                              #else
                                  private
                              #endif
                                  int _b = Log("b");
                                  private static readonly int {|RH7109:_c|} = Log("c");

                                  private static int Log(string name)
                                  {
                                      return 0;
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying that an excluded branch in front of a crossed field that holds only <c>static</c>, which is neither whole
    /// members nor whole attribute lists, withholds the fix
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task NoCodeFixWhenACrossedFieldIsStaticOnlyThroughAWholeGroupInFrontOfItsFirstToken(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private static int _t;
                              #if DEBUG
                                  static
                              #endif
                                  private int _b = Log("b");
                                  private static readonly int {|RH7109:_c|} = Log("c");

                                  private static int Log(string name)
                                  {
                                      return 0;
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying that a conditional group between two modifiers of a crossed field stands at no boundary of whole units, so
    /// no code fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task NoCodeFixWhenACrossedFieldIsStaticOnlyInAGroupBehindItsFirstModifier(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private static int _t;
                                  private
                              #if DEBUG
                                  static
                              #endif
                                  int _b = Log("b");
                                  private static readonly int {|RH7109:_c|} = Log("c");

                                  private static int Log(string name)
                                  {
                                      return 0;
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying that a conditional group whose branches write <c>static readonly</c> or <c>const</c> in front of a crossed
    /// declaration splits the declaration's modifiers, so no code fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task NoCodeFixWhenACrossedConstantIsAStaticReadonlyFieldOnlyInAGroupInFrontOfItsFirstToken(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private static int _t;
                              #if DEBUG
                                  private static readonly
                              #else
                                  private const
                              #endif
                                  int K = 1;
                                  private static readonly int {|RH7109:_c|} = Log(K);

                                  private static int Log(int value)
                                  {
                                      return value;
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying that a conditional group whose branches write the moved field's modifiers splits the moved declaration, so
    /// no code fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task NoCodeFixWhenTheMovedFieldIsStaticOnlyInAGroupInFrontOfItsFirstToken(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private int _a;
                                  private static int _s = Log("s");
                              #if DEBUG
                                  private static readonly
                              #else
                                  private readonly
                              #endif
                                  int {|RH7109:_c|} = Log("c");

                                  private static int Log(string name)
                                  {
                                      return 0;
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying that a conditional group in front of the moved field whose excluded branch holds a whole attribute list
    /// with a named argument wraps whole attribute lists, so the fix moves the group with the field
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task FixMovesAFieldWithAnExcludedAttributeWithANamedArgumentInItsLeadingTrivia(string endOfLine)
    {
        const string source = """
                              using System;

                              public class C
                              {
                                  private int _a = Log("a");
                              #if DEBUG
                                  [Obsolete("x", DiagnosticId = "y")]
                              #endif
                                  private readonly int {|RH7109:_b|};

                                  private static int Log(string name)
                                  {
                                      return 0;
                                  }
                              }
                              """;

        const string fixedSource = """
                                   using System;

                                   public class C
                                   {
                                   #if DEBUG
                                       [Obsolete("x", DiagnosticId = "y")]
                                   #endif
                                       private readonly int _b;
                                       private int _a = Log("a");

                                       private static int Log(string name)
                                       {
                                           return 0;
                                       }
                                   }
                                   """;

        await Verify(WithEndOfLine(source, endOfLine), WithEndOfLine(fixedSource, endOfLine));
    }

    /// <summary>
    /// Verifying that a conditional group behind an active attribute list of the moved field whose excluded branch holds a
    /// whole attribute list wraps whole attribute lists, so the fix moves the group with the field
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task FixMovesAFieldWithAnExcludedAttributeWithANamedArgumentBehindAnActiveAttribute(string endOfLine)
    {
        const string source = """
                              using System;

                              public class C
                              {
                                  private int _a = Log("a");
                                  [NonSerialized]
                              #if DEBUG
                                  [Obsolete("x", DiagnosticId = "y")]
                              #endif
                                  private readonly int {|RH7109:_b|};

                                  private static int Log(string name)
                                  {
                                      return 0;
                                  }
                              }
                              """;

        const string fixedSource = """
                                   using System;

                                   public class C
                                   {
                                       [NonSerialized]
                                   #if DEBUG
                                       [Obsolete("x", DiagnosticId = "y")]
                                   #endif
                                       private readonly int _b;
                                       private int _a = Log("a");

                                       private static int Log(string name)
                                       {
                                           return 0;
                                       }
                                   }
                                   """;

        await Verify(WithEndOfLine(source, endOfLine), WithEndOfLine(fixedSource, endOfLine));
    }

    /// <summary>
    /// Verifying that a conditional group in front of the moved field whose excluded branch holds a whole attribute list
    /// wraps whole attribute lists, so the fix moves the group with the field
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task FixMovesAFieldWithAnExcludedAttributeInItsLeadingTrivia(string endOfLine)
    {
        const string source = """
                              using System;

                              public class C
                              {
                                  private int _a = Log("a");
                              #if DEBUG
                                  [Obsolete("x")]
                              #endif
                                  private readonly int {|RH7109:_b|};

                                  private static int Log(string name)
                                  {
                                      return 0;
                                  }
                              }
                              """;

        const string fixedSource = """
                                   using System;

                                   public class C
                                   {
                                   #if DEBUG
                                       [Obsolete("x")]
                                   #endif
                                       private readonly int _b;
                                       private int _a = Log("a");

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
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task NoCodeFixWhenACrossedFieldIsStaticOnlyInAnExcludedBranchThatOpensInTheInitializerOfThePreviousField(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private static int _t;
                                  private int _a =
                              #if DEBUG
                                      1;
                                  private static
                              #else
                                      2;
                              #endif
                                  int _b = Log("b");
                                  private static readonly int {|RH7109:_c|} = Log("c");

                                  private static int Log(string name)
                                  {
                                      return 0;
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying that a conditional group that opens behind the previous field's declarator, and whose excluded branch ends
    /// that declaration and begins the next one, withholds the fix
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task NoCodeFixWhenACrossedFieldIsStaticOnlyInAnExcludedBranchThatOpensBehindThePreviousDeclarator(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private static int _t;
                                  private int _a
                              #if DEBUG
                                  ;
                                  private static
                              #else
                                  ;
                              #endif
                                  int _b = Log("b");
                                  private static readonly int {|RH7109:_c|} = Log("c");

                                  private static int Log(string name)
                                  {
                                      return 0;
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying that a conditional group that opens behind the previous field's declarator, and whose excluded
    /// <c>#else</c> branch ends that declaration and begins the next one, withholds the fix
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task NoCodeFixWhenACrossedFieldIsStaticOnlyInAnExcludedElseBranchThatOpensBehindThePreviousDeclarator(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private static int _t;
                                  private int _a
                              #if !DEBUG
                                  ;
                              #else
                                  ;
                                  private static
                              #endif
                                  int _b = Log("b");
                                  private static readonly int {|RH7109:_c|} = Log("c");

                                  private static int Log(string name)
                                  {
                                      return 0;
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying that a conditional group with an <c>#elif</c> that opens behind the previous field's declarator, and whose
    /// branches end that declaration and begin the next one, withholds the fix
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task NoCodeFixWhenACrossedFieldIsStaticOnlyInTheSecondExcludedBranchOfAGroupThatOpensBehindThePreviousDeclarator(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private static int _t;
                                  private int _a
                              #if DEBUG
                                  ;
                                  private
                              #elif TRACE
                                  ;
                                  private static
                              #else
                                  ;
                              #endif
                                  int _b = Log("b");
                                  private static readonly int {|RH7109:_c|} = Log("c");

                                  private static int Log(string name)
                                  {
                                      return 0;
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying that a conditional group whose branches span the previous field's terminator and the crossed field's
    /// modifiers withholds the fix
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task NoCodeFixWhenACrossedFieldIsStaticOnlyInAnExcludedBranchThatSpansThePreviousDeclaratorAndItsModifiers(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private static int _t;
                                  private int _a
                              #if DEBUG
                                  ;
                                  private static
                              #else
                                  ;
                                  private
                              #endif
                                  int _b = Log("b");
                                  private static readonly int {|RH7109:_c|} = Log("c");

                                  private static int Log(string name)
                                  {
                                      return 0;
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying that a conditional group whose branches span the previous field's initializer and the crossed field's
    /// modifiers withholds the fix
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task NoCodeFixWhenACrossedFieldIsStaticOnlyInAnExcludedBranchThatSpansThePreviousInitializerAndItsModifiers(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private static int _t;
                                  private int _a =
                              #if DEBUG
                                      1;
                                  private static
                              #else
                                      2;
                                  private
                              #endif
                                  int _b = Log("b");
                                  private static readonly int {|RH7109:_c|} = Log("c");

                                  private static int Log(string name)
                                  {
                                      return 0;
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying that a conditional group whose branches span the previous static field's terminator and the crossed
    /// field's modifiers withholds the move of an instance field
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task NoCodeFixWhenACrossedFieldIsAnInstanceFieldOnlyInAnExcludedBranchThatSpansThePreviousDeclaratorAndItsModifiers(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private int _t;
                                  private static int _a
                              #if DEBUG
                                  ;
                                  internal
                              #else
                                  ;
                                  private static
                              #endif
                                  int _b = Log("b");
                                  private readonly int {|RH7109:_c|} = Log("c");

                                  private static int Log(string name)
                                  {
                                      return 0;
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying that a conditional group in front of a crossed field's first token whose branches write different
    /// modifiers withholds the move of an instance field
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task NoCodeFixWhenACrossedFieldLacksStaticOnlyInAnExcludedBranchInFrontOfItsFirstToken(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private int _a;
                              #if DEBUG
                                  internal
                              #else
                                  private static
                              #endif
                                  int _b = Log("b");
                                  private readonly int {|RH7109:_c|} = Log("c");

                                  private static int Log(string name)
                                  {
                                      return 0;
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying that a conditional group whose branches write <c>readonly</c> or <c>const</c> in front of a crossed
    /// declaration splits its modifiers, so no code fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task NoCodeFixWhenACrossedDeclarationIsAnInstanceFieldInsteadOfAConstantOnlyInAnExcludedBranch(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private int _a;
                              #if DEBUG
                                  internal readonly
                              #else
                                  private const
                              #endif
                                  int K = 1;
                                  private readonly int {|RH7109:_c|} = Log("c");

                                  private static int Log(string name)
                                  {
                                      return 0;
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying that a conditional group with an <c>#else</c> between two modifiers of a crossed field stands at no
    /// boundary of whole units, so no code fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task NoCodeFixWhenACrossedFieldLacksStaticOnlyInAnExcludedBranchBehindItsFirstModifier(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private int _a;
                                  private
                              #if DEBUG
                                  readonly
                              #else
                                  static
                              #endif
                                  int _b = Log("b");
                                  private readonly int {|RH7109:_c|} = Log("c");

                                  private static int Log(string name)
                                  {
                                      return 0;
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying that a conditional group nested in another between two modifiers of a crossed field withholds the fix
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task NoCodeFixWhenACrossedFieldIsStaticOnlyInAnExcludedInnerGroupBehindItsFirstModifier(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private static int _t;
                                  private
                              #if !DEBUG
                              #if TRACE
                                  static
                              #endif
                              #endif
                                  int _b = Log("b");
                                  private static readonly int {|RH7109:_c|} = Log("c");

                                  private static int Log(string name)
                                  {
                                      return 0;
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying that a conditional group with an <c>#elif</c> whose branches write a crossed field's modifiers splits its
    /// declaration, so no code fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task NoCodeFixWhenACrossedFieldLacksStaticOnlyInAnExcludedElifBranch(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private int _a;
                              #if DEBUG
                                  private static readonly
                              #elif TRACE
                                  internal
                              #else
                                  private static
                              #endif
                                  int _b = Log("b");
                                  private readonly int {|RH7109:_c|} = Log("c");

                                  private static int Log(string name)
                                  {
                                      return 0;
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying that a conditional group whose branches write different modifiers of the moved field splits the moved
    /// declaration, so no code fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task NoCodeFixWhenTheMovedFieldLacksStaticOnlyInAnExcludedBranch(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private static int _a;
                                  private int _s = Log("s");
                              #if DEBUG
                                  internal readonly
                              #else
                                  private static readonly
                              #endif
                                  int {|RH7109:_c|} = Log("c");

                                  private static int Log(string name)
                                  {
                                      return 0;
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying the SHARED-ORDER-008 example of an <c>#if</c> that splits a member's modifiers: without <c>DEBUG</c>
    /// defined, <c>K</c> is a constant, and a static initialized field with <c>DEBUG</c>, so no code fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task NoCodeFixWhenAnIfSplitsTheModifiersOfACrossedDeclaration(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private int _a;
                              #if DEBUG
                                  internal static readonly
                              #else
                                  private const
                              #endif
                                      int K = 1;
                                  private readonly int {|RH7109:_b|} = Log("b");

                                  private static int Log(string name)
                                  {
                                      return 0;
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying that a conditional group whose branches write <c>static readonly</c> or <c>readonly</c> in front of a
    /// crossed declaration splits its modifiers, so the move of the field behind it is withheld
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task NoCodeFixWhenACrossedDeclarationIsAnInstanceFieldInsteadOfAConstantInTheActiveBranch(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private int _a;
                              #if DEBUG
                                  internal static readonly
                              #else
                                  private readonly
                              #endif
                                      int {|RH7109:K|} = 1;
                                  private readonly int {|RH7109:_b|} = Log("b");

                                  private static int Log(string name)
                                  {
                                      return 0;
                                  }
                              }
                              """;

        await Verify(WithEndOfLine(source, endOfLine));

        TestFileMarkupParser.GetSpans(WithEndOfLine(source, endOfLine), out var plainSource, out ImmutableDictionary<string, ImmutableArray<TextSpan>> _);

        var actions = await GetCodeFixActionsAsync(plainSource,
                                                   RH7109ReadonlyElementsMustAppearBeforeNonReadonlyElementsAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<VariableDeclaratorSyntax>()
                                                               .Single(declarator => declarator.Identifier.ValueText == "_b")
                                                               .Identifier
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifying that nine conditional groups of whole attribute lists in front of a crossed field wrap whole units, and
    /// that the guard sets no limit on their number, so the fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task FixMovesAFieldAcrossAFieldWithNineConditionalAttributeLists(string endOfLine)
    {
        const string source = """
                              using System;

                              public class C
                              {
                                  private int _a;
                              #if A1
                                  [Obsolete]
                              #endif
                              #if A2
                                  [NonSerialized]
                              #endif
                              #if A3
                                  [Obsolete]
                              #endif
                              #if A4
                                  [NonSerialized]
                              #endif
                              #if A5
                                  [Obsolete]
                              #endif
                              #if A6
                                  [NonSerialized]
                              #endif
                              #if A7
                                  [Obsolete]
                              #endif
                              #if A8
                                  [NonSerialized]
                              #endif
                              #if A9
                                  [Obsolete]
                              #endif
                                  private static int _b = Log("b");
                                  private readonly int {|RH7109:_c|} = Log("c");

                                  private static int Log(string name)
                                  {
                                      return 0;
                                  }
                              }
                              """;

        const string fixedSource = """
                                   using System;

                                   public class C
                                   {
                                       private readonly int _c = Log("c");
                                       private int _a;
                                   #if A1
                                       [Obsolete]
                                   #endif
                                   #if A2
                                       [NonSerialized]
                                   #endif
                                   #if A3
                                       [Obsolete]
                                   #endif
                                   #if A4
                                       [NonSerialized]
                                   #endif
                                   #if A5
                                       [Obsolete]
                                   #endif
                                   #if A6
                                       [NonSerialized]
                                   #endif
                                   #if A7
                                       [Obsolete]
                                   #endif
                                   #if A8
                                       [NonSerialized]
                                   #endif
                                   #if A9
                                       [Obsolete]
                                   #endif
                                       private static int _b = Log("b");

                                       private static int Log(string name)
                                       {
                                           return 0;
                                       }
                                   }
                                   """;

        await Verify(WithEndOfLine(source, endOfLine), WithEndOfLine(fixedSource, endOfLine));
    }

    /// <summary>
    /// Verifying that a conditional group between two entries of a crossed field's collection initializer stands at no
    /// boundary of whole units, so no code fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task NoCodeFixWhenAGroupStandsInTheCollectionInitializerOfACrossedField(string endOfLine)
    {
        const string source = """
                              using System.Collections.Generic;

                              public class C
                              {
                                  private int _a;
                                  private static List<int> _b = new List<int>
                                                                {
                                                                    1,
                              #if DEBUG
                                                                    2,
                              #endif
                                                                };
                                  private readonly int {|RH7109:_c|} = Log("c");

                                  private static int Log(string name)
                                  {
                                      return 0;
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying that nine conditional groups of whole members in the crossed span, none with an initializer, keep the
    /// move: the guard sets no limit on the number of groups or configurations
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task FixMovesAFieldAcrossNineGroupsOfWholeMembers(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private int _a;
                              #if A1
                                  private int _x1;
                              #endif
                              #if A2
                                  private int _x2;
                              #endif
                              #if A3
                                  private int _x3;
                              #endif
                              #if A4
                                  private int _x4;
                              #endif
                              #if A5
                                  private int _x5;
                              #endif
                              #if A6
                                  private int _x6;
                              #endif
                              #if A7
                                  private int _x7;
                              #endif
                              #if A8
                                  private int _x8;
                              #endif
                              #if A9
                                  private int _x9;
                              #endif
                                  private int _b;
                                  private readonly int {|RH7109:_c|};
                              }
                              """;

        const string fixedSource = """
                                   public class C
                                   {
                                       private readonly int _c;
                                       private int _a;
                                   #if A1
                                       private int _x1;
                                   #endif
                                   #if A2
                                       private int _x2;
                                   #endif
                                   #if A3
                                       private int _x3;
                                   #endif
                                   #if A4
                                       private int _x4;
                                   #endif
                                   #if A5
                                       private int _x5;
                                   #endif
                                   #if A6
                                       private int _x6;
                                   #endif
                                   #if A7
                                       private int _x7;
                                   #endif
                                   #if A8
                                       private int _x8;
                                   #endif
                                   #if A9
                                       private int _x9;
                                   #endif
                                       private int _b;
                                   }
                                   """;

        await Verify(WithEndOfLine(source, endOfLine), WithEndOfLine(fixedSource, endOfLine));
    }

    /// <summary>
    /// Verifying that a conditional group behind the moved field, whose branches each close the type, lies outside both
    /// spans and is neither read nor refuses the move, so the fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task FixMovesAFieldWhenAConditionalGroupOfTheBodyClosesBehindTheClosingBraceOfTheType(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private int _a;
                                  private readonly int {|RH7109:_b|};
                              #if DEBUG
                                  private int _d;
                              }
                              #else
                              }
                              #endif
                              """;

        const string fixedSource = """
                                   public class C
                                   {
                                       private readonly int _b;
                                       private int _a;
                                   #if DEBUG
                                       private int _d;
                                   }
                                   #else
                                   }
                                   #endif
                                   """;

        await Verify(WithEndOfLine(source, endOfLine), WithEndOfLine(fixedSource, endOfLine));
    }

    /// <summary>
    /// Verifying the counterpart of a conditional group outside both spans: the move crosses an instance initialized field,
    /// so no code fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-007")]
    [Requirement("RH7109-006")]
    public async Task NoCodeFixWhenAConditionalGroupOfTheBodyClosesBehindTheClosingBraceOfTheTypeAndAnInitializedFieldIsCrossed(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private int _a = Log("a");
                                  private readonly int {|RH7109:_b|} = Log("b");

                                  private static int Log(string name)
                                  {
                                      return 0;
                                  }
                              #if DEBUG
                                  private int _d;
                              }
                              #else
                              }
                              #endif
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying that a conditional group that opens inside a crossed field's argument list, and that holds another group
    /// in its excluded branch, withholds the fix
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task NoCodeFixWhenAnExcludedBranchThatClosesTheActiveBracketHoldsAGroupDeclaringAStaticInitializedField(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private static int _t;
                                  private int _a = Run(1
                              #if DEBUG
                                      );
                              #if TRACE
                                  private static int _z = Log("z");
                              #endif
                                  private int _y = Run(2
                              #endif
                                      );
                                  private static readonly int {|RH7109:_c|} = Log("c");

                                  private static int Run(int a, int b = 0)
                                  {
                                      return a + b;
                                  }

                                  private static int Log(string name)
                                  {
                                      return 0;
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying that conditional groups that open inside a crossed field's argument list withhold the fix
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task NoCodeFixWhenAGroupBesideAGroupThatClosesTheActiveBracketDeclaresAStaticInitializedField(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private static int _t;
                                  private int _a = Run(1
                              #if DEBUG
                                      );
                              #else
                                      , 2
                              #endif
                              #if TRACE && DEBUG
                                  private static int _z = Log("z");
                              #endif
                              #if !DEBUG
                                      );
                              #endif
                                  private int _d;
                                  private static readonly int {|RH7109:_c|} = Log("c");

                                  private static int Run(int a, int b = 0)
                                  {
                                      return a + b;
                                  }

                                  private static int Log(string name)
                                  {
                                      return 0;
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying that a group of whole members nested in another group of whole members in the crossed span withholds the
    /// fix, although both would wrap whole members on their own
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task NoCodeFixWhenTheNestedGroupDeclaringAStaticInitializedFieldIsAtMemberLevelInEveryBuild(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private static int _t;
                                  private int _a = Run(1);
                              #if DEBUG
                              #if TRACE
                                  private static int _z = Log("z");
                              #endif
                                  private int _y = Run(2);
                              #endif
                                  private int _d;
                                  private static readonly int {|RH7109:_c|} = Log("c");

                                  private static int Run(int a, int b = 0)
                                  {
                                      return a + b;
                                  }

                                  private static int Log(string name)
                                  {
                                      return 0;
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying that an excluded branch that closes a crossed method's body block, declares a member, and opens another
    /// method withholds the fix
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task NoCodeFixWhenAnExcludedBranchClosesAMethodBlockAndDeclaresAStaticInitializedField(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private static int _t;

                                  public void M()
                                  {
                              #if DEBUG
                                  }

                                  private static int _z = Log("z");

                                  public void N()
                                  {
                              #endif
                                  }

                                  private static readonly int {|RH7109:_c|} = Log("c");

                                  private static int Run(int a, int b = 0)
                                  {
                                      return a + b;
                                  }

                                  private static int Log(string name)
                                  {
                                      return 0;
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying that a conditional group of whole statements in the body block of a crossed method wraps whole units, so
    /// the move keeps its fix
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task FixMovesAFieldAcrossAMethodWhoseBlockHoldsAnExcludedStatement(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private static int _t;

                                  public void M()
                                  {
                              #if DEBUG
                                      var z = Log("z");
                              #endif
                                  }

                                  private static readonly int {|RH7109:_c|} = Log("c");

                                  private static int Run(int a, int b = 0)
                                  {
                                      return a + b;
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
                                       private static readonly int _c = Log("c");

                                       private static int _t;

                                       public void M()
                                       {
                                   #if DEBUG
                                           var z = Log("z");
                                   #endif
                                       }

                                       private static int Run(int a, int b = 0)
                                       {
                                           return a + b;
                                       }

                                       private static int Log(string name)
                                       {
                                           return 0;
                                       }
                                   }
                                   """;

        await Verify(WithEndOfLine(source, endOfLine), WithEndOfLine(fixedSource, endOfLine));
    }

    /// <summary>
    /// Verifying that an excluded branch that ends a crossed property's expression body, declares a member, and begins
    /// another property withholds the fix
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task NoCodeFixWhenAnExcludedBranchClosesAnExpressionBodyAndDeclaresAStaticInitializedField(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private static int _t;

                                  public int P => Run(1
                              #if DEBUG
                                  );

                                  private static int _z = Log("z");

                                  public int Q => Run(2
                              #endif
                                  );

                                  private static readonly int {|RH7109:_c|} = Log("c");

                                  private static int Run(int a, int b = 0)
                                  {
                                      return a + b;
                                  }

                                  private static int Log(string name)
                                  {
                                      return 0;
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying that a conditional group between two arguments in the expression body of a crossed property stands at no
    /// boundary of whole units, so no code fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task NoCodeFixWhenAGroupStandsInTheExpressionBodyOfACrossedProperty(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private static int _t;
                                  public int P => Run(1
                              #if DEBUG
                                                      , Log("z")
                              #endif
                                                      );
                                  private static readonly int {|RH7109:_c|} = Log("c");

                                  private static int Run(int a, int b = 0)
                                  {
                                      return a + b;
                                  }

                                  private static int Log(string name)
                                  {
                                      return 0;
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying that an excluded branch that closes a crossed nested type's body, declares a member, and opens another
    /// nested type withholds the fix
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task NoCodeFixWhenAnExcludedBranchClosesANestedTypeAndDeclaresAStaticInitializedField(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private static int _t;

                                  private class N
                                  {
                              #if DEBUG
                                  }

                                  private static int _z = Log("z");

                                  private class M
                                  {
                              #endif
                                  }

                                  private static readonly int {|RH7109:_c|} = Log("c");

                                  private static int Run(int a, int b = 0)
                                  {
                                      return a + b;
                                  }

                                  private static int Log(string name)
                                  {
                                      return 0;
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying that a conditional group of whole members in the body of a crossed nested type wraps whole units and is
    /// not counted, because the nested type's members take no part in the move, so the fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task FixMovesAFieldAcrossANestedTypeWhoseBodyHoldsAnExcludedStaticInitializedField(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private static int _t;

                                  private class N
                                  {
                              #if DEBUG
                                      private static int _z = Log("z");
                              #endif
                                  }

                                  private static readonly int {|RH7109:_c|} = Log("c");

                                  private static int Run(int a, int b = 0)
                                  {
                                      return a + b;
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
                                       private static readonly int _c = Log("c");

                                       private static int _t;

                                       private class N
                                       {
                                   #if DEBUG
                                           private static int _z = Log("z");
                                   #endif
                                       }

                                       private static int Run(int a, int b = 0)
                                       {
                                           return a + b;
                                       }

                                       private static int Log(string name)
                                       {
                                           return 0;
                                       }
                                   }
                                   """;

        await Verify(WithEndOfLine(source, endOfLine), WithEndOfLine(fixedSource, endOfLine));
    }

    /// <summary>
    /// Verifying that two whole members of one name in branches that exclude each other both count as present: the static
    /// one has no initializer and the initialized one is an instance field, so no static initialized member is crossed and
    /// the move keeps its fix
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-007")]
    [Requirement("RH7109-006")]
    public async Task FixMovesAStaticFieldAcrossTwoDeclarationsOfOneNameOfWhichNoneIsAStaticInitializedField(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private static int _t;
                              #if DEBUG
                                  private static int _b;
                              #else
                                  private int _b = Log("b");
                              #endif
                                  private int _d;
                                  private static readonly int {|RH7109:_c|} = Log("c");

                                  private static int Run(int a, int b = 0)
                                  {
                                      return a + b;
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
                                       private static readonly int _c = Log("c");
                                       private static int _t;
                                   #if DEBUG
                                       private static int _b;
                                   #else
                                       private int _b = Log("b");
                                   #endif
                                       private int _d;

                                       private static int Run(int a, int b = 0)
                                       {
                                           return a + b;
                                       }

                                       private static int Log(string name)
                                       {
                                           return 0;
                                       }
                                   }
                                   """;

        await Verify(WithEndOfLine(source, endOfLine), WithEndOfLine(fixedSource, endOfLine));
    }

    /// <summary>
    /// Verifying that two whole members of one name in branches that exclude each other both count as present: the
    /// initialized one is static and the instance one has no initializer, so no instance initialized member is crossed and
    /// the move keeps its fix
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-007")]
    [Requirement("RH7109-006")]
    public async Task FixMovesAnInstanceFieldAcrossTwoDeclarationsOfOneNameOfWhichNoneIsAnInstanceInitializedField(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private int _t;
                              #if DEBUG
                                  private static int _b = Log("b");
                              #else
                                  private int _b;
                              #endif
                                  private static int _d;
                                  private readonly int {|RH7109:_c|} = Log("c");

                                  private static int Run(int a, int b = 0)
                                  {
                                      return a + b;
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
                                       private readonly int _c = Log("c");
                                       private int _t;
                                   #if DEBUG
                                       private static int _b = Log("b");
                                   #else
                                       private int _b;
                                   #endif
                                       private static int _d;

                                       private static int Run(int a, int b = 0)
                                       {
                                           return a + b;
                                       }

                                       private static int Log(string name)
                                       {
                                           return 0;
                                       }
                                   }
                                   """;

        await Verify(WithEndOfLine(source, endOfLine), WithEndOfLine(fixedSource, endOfLine));
    }

    /// <summary>
    /// Verifying the counterpart with distinct names: a static field without initializer in one branch and an instance
    /// initialized field in the other, both counted as present, cross no static initialized member, so the move keeps its
    /// fix
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-007")]
    [Requirement("RH7109-006")]
    public async Task FixMovesAStaticFieldAcrossTwoDeclarationsOfDifferentNamesOfWhichNoneIsAStaticInitializedField(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private static int _t;
                              #if DEBUG
                                  private static int _b;
                              #else
                                  private int _e = Log("b");
                              #endif
                                  private int _d;
                                  private static readonly int {|RH7109:_c|} = Log("c");

                                  private static int Run(int a, int b = 0)
                                  {
                                      return a + b;
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
                                       private static readonly int _c = Log("c");
                                       private static int _t;
                                   #if DEBUG
                                       private static int _b;
                                   #else
                                       private int _e = Log("b");
                                   #endif
                                       private int _d;

                                       private static int Run(int a, int b = 0)
                                       {
                                           return a + b;
                                       }

                                       private static int Log(string name)
                                       {
                                           return 0;
                                       }
                                   }
                                   """;

        await Verify(WithEndOfLine(source, endOfLine), WithEndOfLine(fixedSource, endOfLine));
    }

    /// <summary>
    /// Verifying that a static initialized field in one of two branches that exclude each other counts as present, so
    /// moving a static initialized field across it is withheld
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-007")]
    [Requirement("RH7109-006")]
    public async Task NoCodeFixWhenOneOfTwoDeclarationsOfOneNameIsAStaticInitializedField(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private static int _t;
                              #if DEBUG
                                  private static int _b = Log("b");
                              #else
                                  private int _b;
                              #endif
                                  private int _d;
                                  private static readonly int {|RH7109:_c|} = Log("c");

                                  private static int Run(int a, int b = 0)
                                  {
                                      return a + b;
                                  }

                                  private static int Log(string name)
                                  {
                                      return 0;
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying that an instance initialized field in one of two branches that exclude each other counts as present, so
    /// moving an instance initialized field across it is withheld
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-007")]
    [Requirement("RH7109-006")]
    public async Task NoCodeFixWhenOneOfTwoDeclarationsOfOneNameIsAnInstanceInitializedField(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private int _t;
                              #if DEBUG
                                  private static int _b;
                              #else
                                  private int _b = Log("b");
                              #endif
                                  private static int _d;
                                  private readonly int {|RH7109:_c|} = Log("c");

                                  private static int Run(int a, int b = 0)
                                  {
                                      return a + b;
                                  }

                                  private static int Log(string name)
                                  {
                                      return 0;
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying that a conditional group whose branches each begin a crossed declaration that ends behind the group
    /// withholds the fix
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task NoCodeFixWhenACrossedDeclarationSplitIntoAStaticFieldAndAnInstanceInitializedFieldIsCrossedByAnInstanceInitializedField(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private static int _t;
                              #if DEBUG
                                  private static int _b
                              #else
                                  private int _b = Log("b")
                              #endif
                                      ;
                                  private int _d;
                                  private readonly int {|RH7109:_c|} = Log("c");

                                  private static int Log(string name)
                                  {
                                      return 0;
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying that an excluded branch that ends a crossed property's expression body, declares a member, and begins
    /// another property withholds the fix, although its brackets balance
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task NoCodeFixWhenABalancedExcludedBranchEndsAnExpressionBodyAndDeclaresAStaticInitializedField(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private static int _t;

                                  public int P => 1
                              #if DEBUG
                                      ;

                                  private static int _z = Log("z");

                                  public int Q => 2
                              #endif
                                      ;

                                  private static readonly int {|RH7109:_c|} = Log("c");

                                  private static int Log(string name)
                                  {
                                      return 0;
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying the SHARED-ORDER-008 example of a branch that ends the type's body: without <c>DEBUG</c> defined, the
    /// excluded branch closes a brace it did not open and opens another type, so no code fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task NoCodeFixWhenAnExcludedBranchClosesTheTypeAndOpensAnotherOneDeclaringAStaticInitializedField(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private static int _t;
                              #if DEBUG
                              }

                              public class D
                              {
                                  private static int _z = Log("z");
                              #endif
                                  private int _d;
                                  private static readonly int {|RH7109:_c|} = Log("c");

                                  private static int Log(string name)
                                  {
                                      return 0;
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying the SHARED-ORDER-008 example of a group that opens a nested type around the moved field, which a later
    /// group closes, so no code fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task NoCodeFixWhenAnExcludedBranchOpensANestedTypeAroundTheMovedField(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private static int _t;
                              #if DEBUG
                                  private class N
                                  {
                              #endif
                                      private int _d;
                                      private static readonly int {|RH7109:_c|} = Log("c");

                                      private static int Log(string name)
                                      {
                                          return 0;
                                      }
                              #if DEBUG
                                  }
                              #endif
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying that an excluded branch in the moved field's leading trivia that closes a brace it did not open, a nested
    /// type that an earlier group opens, withholds the fix
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task NoCodeFixWhenAnExcludedBranchInFrontOfTheMovedFieldClosesANestedTypeThatAnotherGroupOpens(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private static int _t;
                              #if DEBUG
                                  private class N
                                  {
                              #endif
                                      private int _d;
                              #if DEBUG
                                  }
                              #endif
                                  private static readonly int {|RH7109:_c|} = Log("c");

                                  private static int Log(string name)
                                  {
                                      return 0;
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying that two conditional groups that split a crossed declaration's modifiers and its initializer withhold the
    /// fix
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task NoCodeFixWhenTwoGroupsSplitTheModifiersAndTheInitializerOfACrossedDeclaration(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private static int _t;
                                  private
                              #if DEBUG
                                      static
                              #endif
                                      int _b
                              #if TRACE
                                      = Log("b")
                              #endif
                                      ;
                                  private int _d;
                                  private static readonly int {|RH7109:_c|} = Log("c");

                                  private static int Log(string name)
                                  {
                                      return 0;
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying that conditional groups behind the first modifier of the moved field and of a crossed field withhold the
    /// fix
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task NoCodeFixWhenGroupsBehindTheFirstModifierSplitTheMovedAndACrossedField(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private int _t;
                                  private
                              #if DEBUG
                                      static
                              #endif
                                      int _x = Log("x");
                                  private int _d;
                                  private
                              #if DEBUG
                                      static
                              #endif
                                      readonly int {|RH7109:_c|} = Log("c");

                                  private static int Log(string name)
                                  {
                                      return 0;
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying the SHARED-FIX-009 example: without <c>DEBUG</c> defined, whether the crossed <c>_b</c> has an initializer
    /// depends on the build, and the fix does not work it out, so no code fix is offered although no build would change
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-FIX-009")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task NoCodeFixWhenAnIfCompletesTheInitializerOfACrossedField(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private int _a;
                                  private int _b
                              #if DEBUG
                                      = 1
                              #endif
                                      ;
                                  private readonly int {|RH7109:_c|};
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying the counterpart of the SHARED-FIX-009 example: without the conditional group, the fix moves <c>_c</c> in
    /// front of <c>_a</c>
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-FIX-009")]
    [Requirement("RH7109-005")]
    public async Task FixMovesTheFieldOfTheSharedFixExampleWithoutTheConditionalGroup(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private int _a;
                                  private int _b;
                                  private readonly int {|RH7109:_c|};
                              }
                              """;

        const string fixedSource = """
                                   public class C
                                   {
                                       private readonly int _c;
                                       private int _a;
                                       private int _b;
                                   }
                                   """;

        await Verify(WithEndOfLine(source, endOfLine), WithEndOfLine(fixedSource, endOfLine));
    }

    /// <summary>
    /// Verifying the SHARED-ORDER-008 example of an excluded branch in front of the moved field that holds <c>static</c>,
    /// which is neither whole members nor whole attribute lists, so no code fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task NoCodeFixWhenAnExcludedBranchInFrontOfTheMovedFieldHoldsAModifier(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private int _a;
                              #if DEBUG
                                  static
                              #endif
                                  private readonly int {|RH7109:_c|};
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying the SHARED-ORDER-008 example of a nested group: without <c>FEATURE</c> defined, the <c>#if DEBUG</c> group
    /// lies inside the <c>#if FEATURE</c> group, so no code fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task NoCodeFixWhenAConditionalGroupIsNestedInTheMovedFieldsLeadingTrivia(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private int _a;
                              #if FEATURE
                              #if DEBUG
                                  private int _d;
                              #endif
                              #endif
                                  private readonly int {|RH7109:_b|};
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying the SHARED-ORDER-008 example of whole statements in a lambda inside a crossed field initializer, which is
    /// no body block of a member, so no code fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task NoCodeFixWhenAGroupOfStatementsStandsInALambdaOfACrossedFieldInitializer(string endOfLine)
    {
        const string source = """
                              using System;

                              public class C
                              {
                                  private Action _a = () =>
                                  {
                              #if DEBUG
                                      Console.WriteLine();
                              #endif
                                  };

                                  private readonly int {|RH7109:_c|};
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying the change doc's example of an <c>#if false</c> branch in front of the moved field that holds prose, which
    /// does not read as whole members, so no code fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task NoCodeFixWhenAnExcludedBranchInFrontOfTheMovedFieldHoldsTextThatIsNotCode(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private int _a;
                              #if false
                                  Notes: _b used to be computed here.
                              #endif
                                  private readonly int {|RH7109:_b|};
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying the SHARED-ORDER-008 example of a group of whole members in the body of a crossed nested type: <c>_d</c>
    /// is a member of <c>N</c>, which takes no part in the move, so the fix moves <c>_c</c> in front of <c>_a</c>
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task FixMovesAFieldAcrossACrossedNestedTypeWithAGroupOfWholeMembers(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private int _a;

                                  private class N
                                  {
                              #if DEBUG
                                      private int _d;
                              #endif
                                  }

                                  private readonly int {|RH7109:_c|};
                              }
                              """;

        const string fixedSource = """
                                   public class C
                                   {
                                       private readonly int _c;

                                       private int _a;

                                       private class N
                                       {
                                   #if DEBUG
                                           private int _d;
                                   #endif
                                       }
                                   }
                                   """;

        await Verify(WithEndOfLine(source, endOfLine), WithEndOfLine(fixedSource, endOfLine));
    }

    /// <summary>
    /// Verifying the SHARED-ORDER-008 example of a group of whole attribute lists on the moved field, with an attribute
    /// list in both branches: the group moves with the field
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task FixMovesAFieldWithAGroupOfWholeAttributeListsInEveryBranch(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private int _a;
                              #if DEBUG
                                  [System.Obsolete]
                              #else
                                  [System.CLSCompliant(false)]
                              #endif
                                  private readonly int {|RH7109:_c|};
                              }
                              """;

        const string fixedSource = """
                                   public class C
                                   {
                                   #if DEBUG
                                       [System.Obsolete]
                                   #else
                                       [System.CLSCompliant(false)]
                                   #endif
                                       private readonly int _c;
                                       private int _a;
                                   }
                                   """;

        await Verify(WithEndOfLine(source, endOfLine), WithEndOfLine(fixedSource, endOfLine));
    }

    /// <summary>
    /// Verifying that a group of whole attribute lists in front of a crossed field wraps whole units, so the fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task FixMovesAFieldAcrossAFieldWithAGroupOfWholeAttributeLists(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private int _a;
                              #if DEBUG
                                  [System.Obsolete]
                              #endif
                                  private int _b;
                                  private readonly int {|RH7109:_c|};
                              }
                              """;

        const string fixedSource = """
                                   public class C
                                   {
                                       private readonly int _c;
                                       private int _a;
                                   #if DEBUG
                                       [System.Obsolete]
                                   #endif
                                       private int _b;
                                   }
                                   """;

        await Verify(WithEndOfLine(source, endOfLine), WithEndOfLine(fixedSource, endOfLine));
    }

    /// <summary>
    /// Verifying the SHARED-ORDER-007 example of members in branches that exclude each other: <c>_c</c> counts as crossed,
    /// although the build without <c>DEBUG</c> compiles <c>_e</c> in its place, so no code fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-007")]
    [Requirement("RH7109-006")]
    public async Task NoCodeFixWhenWholeMembersInBranchesThatExcludeEachOtherHoldACrossedInitializer(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private int _a;
                              #if DEBUG
                                  private int _c = Log("c");
                              #else
                                  private int _e;
                              #endif
                                  private readonly int {|RH7109:_b|} = Log("b");

                                  private static int Log(string name)
                                  {
                                      return 0;
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying that an excluded branch that holds only a comment is a sequence of no units and keeps the move
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task FixMovesAFieldAcrossAGroupWhoseExcludedBranchHoldsOnlyAComment(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private int _a;
                              #if DEBUG
                                  // Members that only debug builds need go here
                              #endif
                                  private int _b;
                                  private readonly int {|RH7109:_c|};
                              }
                              """;

        const string fixedSource = """
                                   public class C
                                   {
                                       private readonly int _c;
                                       private int _a;
                                   #if DEBUG
                                       // Members that only debug builds need go here
                                   #endif
                                       private int _b;
                                   }
                                   """;

        await Verify(WithEndOfLine(source, endOfLine), WithEndOfLine(fixedSource, endOfLine));
    }

    /// <summary>
    /// Verifying that a group of whole members in front of the moved field whose active <c>#else</c> branch holds nothing
    /// but a comment wraps whole units, so the group moves with the field
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task FixMovesAFieldWithAGroupWhoseActiveElseBranchHoldsOnlyAComment(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private int _a;
                              #if DEBUG
                                  private int _d;
                              #else // nothing in release
                              #endif
                                  private readonly int {|RH7109:_c|};
                              }
                              """;

        const string fixedSource = """
                                   public class C
                                   {
                                   #if DEBUG
                                       private int _d;
                                   #else // nothing in release
                                   #endif
                                       private readonly int _c;
                                       private int _a;
                                   }
                                   """;

        await Verify(WithEndOfLine(source, endOfLine), WithEndOfLine(fixedSource, endOfLine));
    }

    /// <summary>
    /// Verifying that <c>#region</c> and <c>#endregion</c> lines around whole members inside an excluded branch keep the
    /// group a group of whole members, so the group moves with the field
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task FixMovesAFieldWithAGroupWhoseExcludedBranchHoldsARegionAroundWholeMembers(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private int _a;
                              #if DEBUG
                                  #region Diagnostics
                                  private int _d;
                                  #endregion
                              #endif
                                  private readonly int {|RH7109:_c|};
                              }
                              """;

        const string fixedSource = """
                                   public class C
                                   {
                                   #if DEBUG
                                       #region Diagnostics
                                       private int _d;
                                       #endregion
                                   #endif
                                       private readonly int _c;
                                       private int _a;
                                   }
                                   """;

        await Verify(WithEndOfLine(source, endOfLine), WithEndOfLine(fixedSource, endOfLine));
    }

    /// <summary>
    /// Verifying the <c>No nesting</c> clause for a group whose enclosing group lies outside both spans: the <c>DEBUG</c>
    /// group around <c>_x</c> stands inside the <c>!FEATURE</c> group, so no code fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task NoCodeFixWhenAGroupInTheCrossedSpanIsNestedInAGroupOutsideBothSpans(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                              #if !FEATURE
                                  private readonly int _z;
                                  private int _a;
                              #if DEBUG
                                  private int _x;
                              #endif
                                  private readonly int {|RH7109:_c|};
                              #endif
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying the counterpart of the nested group: without the <c>DEBUG</c> group around <c>_x</c>, the <c>!FEATURE</c>
    /// group lies outside both spans, is not read, and the fix moves <c>_c</c> in front of <c>_a</c>
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task FixMovesAFieldInsideAGroupOutsideBothSpansWithNoGroupInTheSpans(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                              #if !FEATURE
                                  private readonly int _z;
                                  private int _a;
                                  private int _x;
                                  private readonly int {|RH7109:_c|};
                              #endif
                              }
                              """;

        const string fixedSource = """
                                   public class C
                                   {
                                   #if !FEATURE
                                       private readonly int _z;
                                       private readonly int _c;
                                       private int _a;
                                       private int _x;
                                   #endif
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

    #endregion // Methods

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                public class TestClass
                                {
                                    private int _value;
                                    private readonly int {|#0:_first|};
                                    private readonly int {|#1:_second|};
                                }
                                """;

        const string fixedCode = """
                                 public class TestClass
                                 {
                                     private readonly int _first;
                                     private readonly int _second;
                                     private int _value;
                                 }
                                 """;

        // Both readonly fields are moved in front of the same mutable field. The mutable field carries no
        // initializer, so neither move changes initializer execution order and both fixes stay available
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH7109ReadonlyElementsMustAppearBeforeNonReadonlyElementsAnalyzer.DiagnosticId, MessageFormat, 2),
                                  static config => config.NumberOfFixAllIterations = 1);
    }

    #endregion // BatchCodeFixTestsBase
}