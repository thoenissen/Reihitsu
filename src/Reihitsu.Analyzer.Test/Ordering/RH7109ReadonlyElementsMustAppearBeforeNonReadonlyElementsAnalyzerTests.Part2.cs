using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Organization;
using Reihitsu.Analyzer.Rules.Organization;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Ordering;

/// <summary>
/// Test methods for <see cref="RH7109ReadonlyElementsMustAppearBeforeNonReadonlyElementsAnalyzer"/> and <see cref="RH7109ReadonlyElementsMustAppearBeforeNonReadonlyElementsCodeFixProvider"/>
/// </summary>
public partial class RH7109ReadonlyElementsMustAppearBeforeNonReadonlyElementsAnalyzerTests
{
    #region Tests

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

    /// <summary>
    /// Verifying that a conditional group around the <c>StructLayout</c> attribute of a class makes the class count as one
    /// whose field order sets its layout: in a build with <c>X</c>, the class has sequential layout, and moving <c>_c</c> in
    /// front of <c>_a</c> would reorder it, so no code fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task NoCodeFixWhenAConditionalGroupWrapsTheLayoutAttributeOfTheType(string endOfLine)
    {
        const string source = """
                              using System.Runtime.InteropServices;

                              #if X
                              [StructLayout(LayoutKind.Sequential)]
                              #endif
                              public class C
                              {
                                  private int _a;
                                  private readonly int {|RH7109:_c|};
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying that a conditional directive in the attribute lists of the type counts by its place, not by the
    /// attribute: a group around <c>[System.Obsolete]</c> makes the class count as one whose field order sets its layout,
    /// and moving the instance field <c>_c</c> in front of the instance field <c>_a</c> would reorder it, so no code fix is
    /// offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task NoCodeFixWhenAConditionalGroupWrapsAnAttributeListOfTheType(string endOfLine)
    {
        const string source = """
                              #if X
                              [System.Obsolete]
                              #endif
                              public class C
                              {
                                  private int _a;
                                  private readonly int {|RH7109:_c|};
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying the counterpart of a conditional group in the attribute lists of the type: with <c>[System.Obsolete]</c>
    /// outside any group, the fix moves <c>_c</c> in front of <c>_a</c>
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task FixMovesAFieldOfATypeWithAnAttributeListOutsideAnyGroup(string endOfLine)
    {
        const string source = """
                              [System.Obsolete]
                              public class C
                              {
                                  private int _a;
                                  private readonly int {|RH7109:_c|};
                              }
                              """;

        const string fixedSource = """
                                   [System.Obsolete]
                                   public class C
                                   {
                                       private readonly int _c;
                                       private int _a;
                                   }
                                   """;

        await Verify(WithEndOfLine(source, endOfLine), WithEndOfLine(fixedSource, endOfLine));
    }

    /// <summary>
    /// Verifying that a conditional group in the header of the type makes the type count as one whose field order sets its
    /// layout: in a build with <c>X</c>, the type is a struct, and moving <c>_c</c> in front of <c>_a</c> would reorder its
    /// layout, so no code fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task NoCodeFixWhenAConditionalGroupDecidesTheKindOfTheType(string endOfLine)
    {
        const string source = """
                              public
                              #if X
                                  struct
                              #else
                                  class
                              #endif
                                  C
                              {
                                  private int _a;
                                  private readonly int {|RH7109:_c|};
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying that a conditional group in the base list of the type, in front of its opening brace, makes the type count
    /// as one whose field order sets its layout: moving the instance field <c>_c</c> in front of the instance field
    /// <c>_a</c> would reorder it, so no code fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task NoCodeFixWhenAConditionalGroupStandsInTheBaseListOfTheType(string endOfLine)
    {
        const string source = """
                              public class C
                              #if X
                                  : System.IDisposable
                              #endif
                              {
                                  private int _a;
                                  private readonly int {|RH7109:_c|};
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying the counterpart of a conditional group around an attribute list of a class for fields that are no
    /// instance fields: moving the static <c>_c</c> in front of the static <c>_a</c> reorders no instance field, and the fix
    /// moves it
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task FixMovesAStaticFieldOfAClassWhoseAttributeListsHoldAConditionalGroup(string endOfLine)
    {
        const string source = """
                              #if X
                              [System.Obsolete]
                              #endif
                              public class C
                              {
                                  private static int _a;
                                  private static readonly int {|RH7109:_c|};
                              }
                              """;

        const string fixedSource = """
                                   #if X
                                   [System.Obsolete]
                                   #endif
                                   public class C
                                   {
                                       private static readonly int _c;
                                       private static int _a;
                                   }
                                   """;

        await Verify(WithEndOfLine(source, endOfLine), WithEndOfLine(fixedSource, endOfLine));
    }

    /// <summary>
    /// Verifying the counterpart of a conditional group in the base list of a class for fields that are no instance fields:
    /// moving the static <c>_c</c> in front of the static <c>_a</c> reorders no instance field, and the fix moves it
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task FixMovesAStaticFieldOfAClassWhoseBaseListHoldsAConditionalGroup(string endOfLine)
    {
        const string source = """
                              public class C
                              #if X
                                  : System.IDisposable
                              #endif
                              {
                                  private static int _a;
                                  private static readonly int {|RH7109:_c|};
                              }
                              """;

        const string fixedSource = """
                                   public class C
                                   #if X
                                       : System.IDisposable
                                   #endif
                                   {
                                       private static readonly int _c;
                                       private static int _a;
                                   }
                                   """;

        await Verify(WithEndOfLine(source, endOfLine), WithEndOfLine(fixedSource, endOfLine));
    }

    /// <summary>
    /// Verifying the counterpart of a conditional directive in front of the type's body: a group in the leading trivia of
    /// the type's first token whose excluded branch declares a whole type keeps the moves, and the fix moves <c>_c</c> in
    /// front of <c>_a</c>
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task FixMovesAFieldOfATypeBehindAGroupOfAWholeTypeInFrontOfIt(string endOfLine)
    {
        const string source = """
                              #if X
                              public class D
                              {
                              }
                              #endif
                              public class C
                              {
                                  private int _a;
                                  private readonly int {|RH7109:_c|};
                              }
                              """;

        const string fixedSource = """
                                   #if X
                                   public class D
                                   {
                                   }
                                   #endif
                                   public class C
                                   {
                                       private readonly int _c;
                                       private int _a;
                                   }
                                   """;

        await Verify(WithEndOfLine(source, endOfLine), WithEndOfLine(fixedSource, endOfLine));
    }

    /// <summary>
    /// Verifying the SHARED-ORDER-008 example of a conditional <c>StructLayout</c> attribute list on another declaration of
    /// a partial class: in a build with <c>NET</c>, the field order of <c>C</c> sets its layout, although the declaration
    /// whose members the fix reorders holds no directive, so no code fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task NoCodeFixWhenAConditionalGroupWrapsTheLayoutAttributeOfAnotherDeclarationOfAPartialClass(string endOfLine)
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
                                  private readonly int {|RH7109:_c|};

                                  public C(int c)
                                  {
                                      _c = c;
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying the SHARED-ORDER-008 example of a conditional <c>StructLayout</c> attribute list on another declaration of
    /// a partial class when its symbol is defined: the <c>#endif</c> stands behind the declaration's first token, and no
    /// code fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task NoCodeFixWhenAnActiveConditionalGroupWrapsTheLayoutAttributeOfAnotherDeclarationOfAPartialClass(string endOfLine)
    {
        const string source = """
                              #define NET
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
                                  private readonly int {|RH7109:_c|};
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying the SHARED-ORDER-008 example of a partial struct whose other declaration carries
    /// <c>[StructLayout(LayoutKind.Auto)]</c> inside <c>#if !X</c>: the analyzed build sees automatic layout, a build with
    /// <c>X</c> lays the struct out in field order, and no code fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task NoCodeFixWhenAConditionalGroupWrapsTheAutoLayoutAttributeOfAnotherDeclarationOfAPartialStruct(string endOfLine)
    {
        const string source = """
                              using System.Runtime.InteropServices;

                              #if !X
                              [StructLayout(LayoutKind.Auto)]
                              #endif
                              public partial struct S
                              {
                              }

                              public partial struct S
                              {
                                  private int _a;
                                  private readonly int {|RH7109:_c|};
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying that a conditional attribute list that sets no layout on another declaration of a partial class makes the
    /// class count as one whose field order sets its layout, as it does on the declaration whose members the fix reorders,
    /// so no code fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task NoCodeFixWhenAConditionalGroupWrapsAnAttributeListOfAnotherDeclarationOfAPartialClass(string endOfLine)
    {
        const string source = """
                              #if DEBUG
                              [System.Serializable]
                              #endif
                              public partial class C { }

                              public partial class C
                              {
                                  private int _a;
                                  private readonly int {|RH7109:_c|};
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying that a conditional group in the base list of another declaration of a partial class, in front of its
    /// body, makes the class count as one whose field order sets its layout, so no code fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task NoCodeFixWhenAConditionalGroupStandsInTheBaseListOfAnotherDeclarationOfAPartialClass(string endOfLine)
    {
        const string source = """
                              public partial class C
                              {
                                  private int _a;
                                  private readonly int {|RH7109:_c|};
                              }

                              public partial class C
                              #if X
                                  : System.IDisposable
                              #endif
                              {
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying the SHARED-ORDER-008 counterpart of a conditional directive on another declaration of a partial class:
    /// without a conditional directive in any declaration of <c>C</c>, the fix moves <c>_c</c> in front of <c>_a</c>
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task FixMovesAFieldOfAPartialClassWithoutAConditionalDirectiveInAnyDeclaration(string endOfLine)
    {
        const string source = """
                              public partial class C
                              {
                              }

                              public partial class C
                              {
                                  private int _a;
                                  private readonly int {|RH7109:_c|};
                              }
                              """;

        const string fixedSource = """
                                   public partial class C
                                   {
                                   }

                                   public partial class C
                                   {
                                       private readonly int _c;
                                       private int _a;
                                   }
                                   """;

        await Verify(WithEndOfLine(source, endOfLine), WithEndOfLine(fixedSource, endOfLine));
    }

    /// <summary>
    /// Verifying the SHARED-ORDER-008 example of a conditional <c>StructLayout</c> attribute list on another declaration of
    /// a partial class that ends in <c>;</c> instead of a body: the declaration is read like one with a body, in a build
    /// with <c>NET</c> the field order of <c>C</c> sets its layout, and no code fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task NoCodeFixWhenAConditionalGroupWrapsTheLayoutAttributeOfAnotherDeclarationOfAPartialClassThatEndsInASemicolon(string endOfLine)
    {
        const string source = """
                              using System.Runtime.InteropServices;

                              #if NET
                              [StructLayout(LayoutKind.Sequential)]
                              #endif
                              public partial class C;

                              public partial class C
                              {
                                  private int _a;
                                  private readonly int {|RH7109:_c|};
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying that a conditional <c>StructLayout</c> attribute list on another declaration of a partial struct that ends
    /// in <c>;</c> is read: a build with <c>X</c> lays the struct out in field order, and no code fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task NoCodeFixWhenAConditionalGroupWrapsTheAutoLayoutAttributeOfAnotherDeclarationOfAPartialStructThatEndsInASemicolon(string endOfLine)
    {
        const string source = """
                              using System.Runtime.InteropServices;

                              #if !X
                              [StructLayout(LayoutKind.Auto)]
                              #endif
                              public partial struct S;

                              public partial struct S
                              {
                                  private int _a;
                                  private readonly int {|RH7109:_c|};
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying that a conditional <c>StructLayout</c> attribute list on another declaration of a partial record that
    /// ends in <c>;</c> is read: in a build with <c>NET</c> the field order of <c>R</c> sets its layout, and no code fix is
    /// offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task NoCodeFixWhenAConditionalGroupWrapsTheLayoutAttributeOfAnotherDeclarationOfAPartialRecordThatEndsInASemicolon(string endOfLine)
    {
        const string source = """
                              using System.Runtime.InteropServices;

                              #if NET
                              [StructLayout(LayoutKind.Sequential)]
                              #endif
                              public partial record R;

                              public partial record R
                              {
                                  private int _a;
                                  private readonly int {|RH7109:_c|};
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying that a conditional <c>StructLayout</c> attribute list on another declaration of a partial record struct
    /// that ends in <c>;</c> is read: a build with <c>X</c> lays the record struct out in field order, and no code fix is
    /// offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task NoCodeFixWhenAConditionalGroupWrapsTheAutoLayoutAttributeOfAnotherDeclarationOfAPartialRecordStructThatEndsInASemicolon(string endOfLine)
    {
        const string source = """
                              using System.Runtime.InteropServices;

                              #if !X
                              [StructLayout(LayoutKind.Auto)]
                              #endif
                              public partial record struct S;

                              public partial record struct S
                              {
                                  private int _a;
                                  private readonly int {|RH7109:_c|};
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying that a conditional group in the base list of another declaration of a partial class that ends in
    /// <c>;</c>, behind the declaration's first token and in front of its <c>;</c>, makes the class count as one whose
    /// field order sets its layout, so no code fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task NoCodeFixWhenAConditionalGroupStandsInTheBaseListOfAnotherDeclarationOfAPartialClassThatEndsInASemicolon(string endOfLine)
    {
        const string source = """
                              public partial class C
                              {
                                  private int _a;
                                  private readonly int {|RH7109:_c|};
                              }

                              public partial class C
                              #if X
                                  : System.IDisposable
                              #endif
                                  ;
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying the SHARED-ORDER-008 counterpart of a declaration of a partial class that ends in <c>;</c>: without a
    /// conditional directive in any declaration of <c>C</c>, the fix moves <c>_c</c> in front of <c>_a</c>
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task FixMovesAFieldOfAPartialClassWhoseOtherDeclarationEndsInASemicolonWithoutAConditionalDirective(string endOfLine)
    {
        const string source = """
                              public partial class C;

                              public partial class C
                              {
                                  private int _a;
                                  private readonly int {|RH7109:_c|};
                              }
                              """;

        const string fixedSource = """
                                   public partial class C;

                                   public partial class C
                                   {
                                       private readonly int _c;
                                       private int _a;
                                   }
                                   """;

        await Verify(WithEndOfLine(source, endOfLine), WithEndOfLine(fixedSource, endOfLine));
    }

    /// <summary>
    /// Verifying that another declaration of a partial class with an attribute list that ends in <c>;</c>, in an excluded
    /// branch in front of the reordered declaration, is one of the type's declarations, so no code fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task NoCodeFixWhenAnExcludedBranchInFrontOfThePartialClassDeclaresAnotherDeclarationOfItWithAnAttributeListThatEndsInASemicolon(string endOfLine)
    {
        const string source = """
                              using System.Runtime.InteropServices;

                              #if NET
                              [StructLayout(LayoutKind.Sequential)]
                              public partial class C;
                              #endif
                              public partial class C
                              {
                                  private int _a;
                                  private readonly int {|RH7109:_c|};
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying that a conditional directive on another declaration of a partial class keeps a move that reorders no
    /// instance field: moving the static <c>_c</c> in front of the static <c>_a</c> changes no layout, and the fix moves it
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task FixMovesAStaticFieldOfAPartialClassWhoseOtherDeclarationHoldsAConditionalGroup(string endOfLine)
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
                                  private static int _a;
                                  private static readonly int {|RH7109:_c|};
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
                                       private static readonly int _c;
                                       private static int _a;
                                   }
                                   """;

        await Verify(WithEndOfLine(source, endOfLine), WithEndOfLine(fixedSource, endOfLine));
    }

    /// <summary>
    /// Verifying that a conditional group in the body of another declaration of a partial class is not read: it does not
    /// make the class count as one whose field order sets its layout, and the fix moves <c>_c</c> in front of <c>_a</c>
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task FixMovesAFieldOfAPartialClassWhoseOtherDeclarationHoldsAConditionalGroupInItsBody(string endOfLine)
    {
        const string source = """
                              public partial class C
                              {
                              #if DEBUG
                                  static
                              #endif
                                  private int _z;
                              }

                              public partial class C
                              {
                                  private int _a;
                                  private readonly int {|RH7109:_c|};
                              }
                              """;

        const string fixedSource = """
                                   public partial class C
                                   {
                                   #if DEBUG
                                       static
                                   #endif
                                       private int _z;
                                   }

                                   public partial class C
                                   {
                                       private readonly int _c;
                                       private int _a;
                                   }
                                   """;

        await Verify(WithEndOfLine(source, endOfLine), WithEndOfLine(fixedSource, endOfLine));
    }

    /// <summary>
    /// Verifying that a conditional group around the layout attribute of a type that encloses the reordered one does not
    /// count, and the fix moves <c>_c</c> in front of <c>_a</c>
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task FixMovesAFieldOfATypeWhoseEnclosingTypeHoldsAConditionalGroup(string endOfLine)
    {
        const string source = """
                              using System.Runtime.InteropServices;

                              #if NET
                              [StructLayout(LayoutKind.Sequential)]
                              #endif
                              public class Outer
                              {
                                  public class C
                                  {
                                      private int _a;
                                      private readonly int {|RH7109:_c|};
                                  }
                              }
                              """;

        const string fixedSource = """
                                   using System.Runtime.InteropServices;

                                   #if NET
                                   [StructLayout(LayoutKind.Sequential)]
                                   #endif
                                   public class Outer
                                   {
                                       public class C
                                       {
                                           private readonly int _c;
                                           private int _a;
                                       }
                                   }
                                   """;

        await Verify(WithEndOfLine(source, endOfLine), WithEndOfLine(fixedSource, endOfLine));
    }

    /// <summary>
    /// Verifying that a conditional <c>StructLayout</c> attribute list on another declaration of a partial class in another
    /// document makes the class count as one whose field order sets its layout, so no code fix is offered and the document
    /// stays as written
    /// </summary>
    /// <param name="endOfLine">Line break of the documents</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task NoCodeFixWhenAConditionalGroupWrapsTheLayoutAttributeOfADeclarationOfAPartialClassInAnotherDocument(string endOfLine)
    {
        const string source = """
                              public partial class C
                              {
                                  private int _a;
                                  private readonly int {|RH7109:_c|};
                              }
                              """;

        const string otherPartSource = """
                                       using System.Runtime.InteropServices;

                                       #if NET
                                       [StructLayout(LayoutKind.Sequential)]
                                       #endif
                                       public partial class C
                                       {
                                       }
                                       """;

        var otherPart = WithEndOfLine(otherPartSource, endOfLine);

        await Verify(WithEndOfLine(source, endOfLine),
                     WithEndOfLine(source, endOfLine),
                     test =>
                     {
                         test.TestState.Sources.Add(("/0/Test1.cs", otherPart));
                         test.FixedState.Sources.Add(("/0/Test1.cs", otherPart));
                         test.FixedState.MarkupHandling = MarkupMode.Allow;
                         test.BatchFixedState.MarkupHandling = MarkupMode.Allow;
                     });
    }

    /// <summary>
    /// Verifying the counterpart of a conditional directive on a declaration of a partial class in another document: when
    /// that declaration holds none, the fix moves <c>_c</c> in front of <c>_a</c>
    /// </summary>
    /// <param name="endOfLine">Line break of the documents</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task FixMovesAFieldOfAPartialClassWhoseDeclarationInAnotherDocumentHoldsNoConditionalDirective(string endOfLine)
    {
        const string source = """
                              public partial class C
                              {
                                  private int _a;
                                  private readonly int {|RH7109:_c|};
                              }
                              """;

        const string fixedSource = """
                                   public partial class C
                                   {
                                       private readonly int _c;
                                       private int _a;
                                   }
                                   """;

        const string otherPartSource = """
                                       [System.Serializable]
                                       public partial class C
                                       {
                                       }
                                       """;

        var otherPart = WithEndOfLine(otherPartSource, endOfLine);

        await Verify(WithEndOfLine(source, endOfLine),
                     WithEndOfLine(fixedSource, endOfLine),
                     test =>
                     {
                         test.TestState.Sources.Add(("/0/Test1.cs", otherPart));
                         test.FixedState.Sources.Add(("/0/Test1.cs", otherPart));
                     });
    }

    /// <summary>
    /// Verifying the SHARED-ORDER-008 example of another declaration of a partial class with an attribute list in an
    /// excluded branch in front of the reordered declaration: the branch reads as whole members, but one of them is a
    /// declaration of <c>C</c> with an attribute list, so no code fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task NoCodeFixWhenAnExcludedBranchInFrontOfThePartialClassDeclaresAnotherDeclarationOfItWithAnAttributeList(string endOfLine)
    {
        const string source = """
                              using System.Runtime.InteropServices;

                              #if NET
                              [StructLayout(LayoutKind.Sequential)]
                              public partial class C
                              {
                              }
                              #endif
                              public partial class C
                              {
                                  private int _a;
                                  private readonly int {|RH7109:_c|};
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying the SHARED-ORDER-008 counterpart of an excluded declaration of a partial class in front of it: without an
    /// attribute list, the excluded declaration gives the class no layout, and the fix moves <c>_c</c> in front of
    /// <c>_a</c>
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task FixMovesAFieldWhenAnExcludedDeclarationOfThePartialClassInFrontOfItHasNoAttributeList(string endOfLine)
    {
        const string source = """
                              #if NET
                              public partial class C
                              {
                              }
                              #endif
                              public partial class C
                              {
                                  private int _a;
                                  private readonly int {|RH7109:_c|};
                              }
                              """;

        const string fixedSource = """
                                   #if NET
                                   public partial class C
                                   {
                                   }
                                   #endif
                                   public partial class C
                                   {
                                       private readonly int _c;
                                       private int _a;
                                   }
                                   """;

        await Verify(WithEndOfLine(source, endOfLine), WithEndOfLine(fixedSource, endOfLine));
    }

    /// <summary>
    /// Verifying the SHARED-ORDER-008 counterpart of an excluded declaration of a partial class in front of it: a
    /// declaration of another name with an attribute list keeps the move
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task FixMovesAFieldWhenAnExcludedPartialDeclarationOfAnotherNameWithAnAttributeListStandsInFrontOfIt(string endOfLine)
    {
        const string source = """
                              using System.Runtime.InteropServices;

                              #if NET
                              [StructLayout(LayoutKind.Sequential)]
                              public partial class D
                              {
                              }
                              #endif
                              public partial class C
                              {
                                  private int _a;
                                  private readonly int {|RH7109:_c|};
                              }
                              """;

        const string fixedSource = """
                                   using System.Runtime.InteropServices;

                                   #if NET
                                   [StructLayout(LayoutKind.Sequential)]
                                   public partial class D
                                   {
                                   }
                                   #endif
                                   public partial class C
                                   {
                                       private readonly int _c;
                                       private int _a;
                                   }
                                   """;

        await Verify(WithEndOfLine(source, endOfLine), WithEndOfLine(fixedSource, endOfLine));
    }

    /// <summary>
    /// Verifying that an excluded partial declaration of the name of the class with another number of type parameters and
    /// with an attribute list declares another type and keeps the move
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task FixMovesAFieldWhenAnExcludedPartialDeclarationOfTheNameWithAnotherArityStandsInFrontOfIt(string endOfLine)
    {
        const string source = """
                              using System.Runtime.InteropServices;

                              #if NET
                              [StructLayout(LayoutKind.Sequential)]
                              public partial class C<T>
                              {
                              }
                              #endif
                              public partial class C
                              {
                                  private int _a;
                                  private readonly int {|RH7109:_c|};
                              }
                              """;

        const string fixedSource = """
                                   using System.Runtime.InteropServices;

                                   #if NET
                                   [StructLayout(LayoutKind.Sequential)]
                                   public partial class C<T>
                                   {
                                   }
                                   #endif
                                   public partial class C
                                   {
                                       private readonly int _c;
                                       private int _a;
                                   }
                                   """;

        await Verify(WithEndOfLine(source, endOfLine), WithEndOfLine(fixedSource, endOfLine));
    }

    /// <summary>
    /// Verifying that an excluded partial declaration of the name of the class with an attribute list counts only at the
    /// outermost level of the branch: nested in another type, it declares another type, and the fix moves <c>_c</c> in
    /// front of <c>_a</c>
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task FixMovesAFieldWhenAnExcludedPartialDeclarationOfTheNameIsNestedInAnotherType(string endOfLine)
    {
        const string source = """
                              using System.Runtime.InteropServices;

                              #if NET
                              public class Outer
                              {
                                  [StructLayout(LayoutKind.Sequential)]
                                  public partial class C
                                  {
                                  }
                              }
                              #endif
                              public partial class C
                              {
                                  private int _a;
                                  private readonly int {|RH7109:_c|};
                              }
                              """;

        const string fixedSource = """
                                   using System.Runtime.InteropServices;

                                   #if NET
                                   public class Outer
                                   {
                                       [StructLayout(LayoutKind.Sequential)]
                                       public partial class C
                                       {
                                       }
                                   }
                                   #endif
                                   public partial class C
                                   {
                                       private readonly int _c;
                                       private int _a;
                                   }
                                   """;

        await Verify(WithEndOfLine(source, endOfLine), WithEndOfLine(fixedSource, endOfLine));
    }

    /// <summary>
    /// Verifying that every excluded branch of a group with a directive in the leading trivia of the declaration's first
    /// token is read: the excluded <c>#if</c> branch holds a <c>StructLayout</c> attribute list in front of another
    /// declaration, only the <c>#endif</c> stands in front of <c>C</c>, and no code fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task NoCodeFixWhenAnExcludedBranchInFrontOfAnotherTypeOfTheGroupAroundTheFirstTokenHoldsAnAttributeList(string endOfLine)
    {
        const string source = """
                              using System.Runtime.InteropServices;

                              #if X
                              [StructLayout(LayoutKind.Sequential)]
                              #else
                              public class D
                              {
                              }
                              #endif
                              public class C
                              {
                                  private int _a;
                                  private readonly int {|RH7109:_c|};
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying that every excluded branch of a group with a directive in the leading trivia of the declaration's first
    /// token is read, also an excluded <c>#if</c> branch in front of an excluded <c>#elif</c> branch of whole members, so no
    /// code fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task NoCodeFixWhenAnExcludedBranchInFrontOfAnExcludedElifBranchOfTheGroupAroundTheFirstTokenHoldsAnAttributeList(string endOfLine)
    {
        const string source = """
                              using System.Runtime.InteropServices;

                              #if X
                              [StructLayout(LayoutKind.Sequential)]
                              #elif Y
                              public class E
                              {
                              }
                              #else
                              public class D
                              {
                              }
                              #endif
                              public class C
                              {
                                  private int _a;
                                  private readonly int {|RH7109:_c|};
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying that every excluded branch of a group around the first token of another declaration of a partial class is
    /// read: the excluded <c>#if</c> branch holds a <c>StructLayout</c> attribute list in front of another type, only the
    /// <c>#endif</c> stands in front of the other declaration of <c>C</c>, and no code fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task NoCodeFixWhenAnExcludedBranchOfTheGroupAroundTheFirstTokenOfAnotherDeclarationOfAPartialClassHoldsAnAttributeList(string endOfLine)
    {
        const string source = """
                              using System.Runtime.InteropServices;

                              #if X
                              [StructLayout(LayoutKind.Sequential)]
                              #else
                              public class D
                              {
                              }
                              #endif
                              public partial class C
                              {
                              }

                              public partial class C
                              {
                                  private int _a;
                                  private readonly int {|RH7109:_c|};
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying that a group in front of the declaration's first token whose excluded branch holds a nested group of a
    /// whole type keeps the move: every excluded branch reads as whole members, none of them is a declaration of the type,
    /// and the fix moves <c>_c</c> in front of <c>_a</c>
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task FixMovesAFieldOfATypeBehindANestedGroupOfAWholeTypeInFrontOfIt(string endOfLine)
    {
        const string source = """
                              #if X
                              #if Y
                              public class D
                              {
                              }
                              #endif
                              #endif
                              public class C
                              {
                                  private int _a;
                                  private readonly int {|RH7109:_c|};
                              }
                              """;

        const string fixedSource = """
                                   #if X
                                   #if Y
                                   public class D
                                   {
                                   }
                                   #endif
                                   #endif
                                   public class C
                                   {
                                       private readonly int _c;
                                       private int _a;
                                   }
                                   """;

        await Verify(WithEndOfLine(source, endOfLine), WithEndOfLine(fixedSource, endOfLine));
    }

    /// <summary>
    /// Verifying the SHARED-ORDER-008 example of a group in front of the moved and the crossed span whose excluded branch
    /// closes the type's body: in a build with <c>X</c>, the three fields are the fields of the struct <c>D</c>, and moving
    /// <c>_c</c> in front of <c>_a</c> would change its layout, so no code fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task NoCodeFixWhenAGroupInFrontOfTheSpansClosesTheTypeAndOpensAStruct(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                              #if X
                              }

                              public struct D
                              {
                              #endif
                                  private readonly int _y;
                                  private int _a;
                                  private readonly int {|RH7109:_c|};
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying the boundary of the crossed span: the group whose excluded branch closes the type's body stands in the
    /// leading trivia of the target <c>_a</c>, inside the crossed span, and no code fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task NoCodeFixWhenAGroupInTheLeadingTriviaOfTheTargetClosesTheTypeAndOpensAStruct(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                              #if X
                              }

                              public struct D
                              {
                              #endif
                                  private int _a;
                                  private readonly int {|RH7109:_c|};
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying the SHARED-ORDER-008 example of a group in front of the spans that holds a modifier of a member in front
    /// of them: the excluded branch is neither whole members nor whole attribute lists, so no code fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task NoCodeFixWhenAGroupInFrontOfTheSpansHoldsAModifierOfAMemberInFrontOfThem(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                              #if DEBUG
                                  static
                              #endif
                                  private readonly int _q;
                                  private int _a;
                                  private readonly int {|RH7109:_c|};
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying that a group with its <c>#if</c> in front of the spans and its other directives behind them, so that the
    /// spans lie in its active branch, makes the type count as one whose field order sets its layout when its excluded
    /// branch does not read as whole members, so no code fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task NoCodeFixWhenAGroupAroundTheSpansHasAnExcludedBranchThatIsNoWholeMember(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                              #if !X
                                  private readonly int _q;
                                  private int _a;
                                  private readonly int {|RH7109:_c|};
                              #else
                                  private int _z
                              #endif
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying the counterpart of a group around the spans: when its excluded branch holds a whole member behind the
    /// spans, the fix moves <c>_c</c> in front of <c>_a</c>
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task FixMovesAFieldWhenAGroupAroundTheSpansHasAnExcludedBranchOfAWholeMember(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                              #if !X
                                  private readonly int _q;
                                  private int _a;
                                  private readonly int {|RH7109:_c|};
                              #else
                                  private int _z;
                              #endif
                              }
                              """;

        const string fixedSource = """
                                   public class C
                                   {
                                   #if !X
                                       private readonly int _q;
                                       private readonly int _c;
                                       private int _a;
                                   #else
                                       private int _z;
                                   #endif
                                   }
                                   """;

        await Verify(WithEndOfLine(source, endOfLine), WithEndOfLine(fixedSource, endOfLine));
    }

    /// <summary>
    /// Verifying the SHARED-ORDER-008 example of a group of whole members in front of the spans: it keeps the move that
    /// reorders instance fields, and the fix moves <c>_c</c> in front of <c>_a</c>
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task FixMovesAFieldBehindAGroupOfWholeMembersInFrontOfTheSpans(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                              #if X
                                  private int _t;
                              #endif
                                  private readonly int _y;
                                  private int _a;
                                  private readonly int {|RH7109:_c|};
                              }
                              """;

        const string fixedSource = """
                                   public class C
                                   {
                                   #if X
                                       private int _t;
                                   #endif
                                       private readonly int _y;
                                       private readonly int _c;
                                       private int _a;
                                   }
                                   """;

        await Verify(WithEndOfLine(source, endOfLine), WithEndOfLine(fixedSource, endOfLine));
    }

    /// <summary>
    /// Verifying the SHARED-ORDER-008 statement that a group of whole statements in the body of a method in front of the
    /// spans keeps the move, and the fix moves <c>_c</c> in front of <c>_a</c>
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task FixMovesAFieldBehindAMethodWithAGroupOfWholeStatementsInFrontOfTheSpans(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  public void M()
                                  {
                              #if X
                                      System.Console.WriteLine();
                              #endif
                                  }

                                  private int _a;
                                  private readonly int {|RH7109:_c|};
                              }
                              """;

        const string fixedSource = """
                                   public class C
                                   {
                                       public void M()
                                       {
                                   #if X
                                           System.Console.WriteLine();
                                   #endif
                                       }

                                       private readonly int _c;
                                       private int _a;
                                   }
                                   """;

        await Verify(WithEndOfLine(source, endOfLine), WithEndOfLine(fixedSource, endOfLine));
    }

    /// <summary>
    /// Verifying that a group of whole attribute lists of a member in front of the spans keeps the move, and the fix moves
    /// <c>_c</c> in front of <c>_a</c>
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task FixMovesAFieldBehindAGroupOfWholeAttributeListsInFrontOfTheSpans(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                              #if X
                                  [System.Obsolete]
                              #endif
                                  private readonly int _y;
                                  private int _a;
                                  private readonly int {|RH7109:_c|};
                              }
                              """;

        const string fixedSource = """
                                   public class C
                                   {
                                   #if X
                                       [System.Obsolete]
                                   #endif
                                       private readonly int _y;
                                       private readonly int _c;
                                       private int _a;
                                   }
                                   """;

        await Verify(WithEndOfLine(source, endOfLine), WithEndOfLine(fixedSource, endOfLine));
    }

    /// <summary>
    /// Verifying that a group of whole members in the body of a nested type in front of the spans keeps the move, and the
    /// fix moves <c>_c</c> in front of <c>_a</c>
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task FixMovesAFieldBehindANestedTypeWithAGroupOfWholeMembersInFrontOfTheSpans(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private class N
                                  {
                              #if X
                                      private int _t;
                              #endif
                                  }

                                  private int _a;
                                  private readonly int {|RH7109:_c|};
                              }
                              """;

        const string fixedSource = """
                                   public class C
                                   {
                                       private class N
                                       {
                                   #if X
                                           private int _t;
                                   #endif
                                       }

                                       private readonly int _c;
                                       private int _a;
                                   }
                                   """;

        await Verify(WithEndOfLine(source, endOfLine), WithEndOfLine(fixedSource, endOfLine));
    }

    /// <summary>
    /// Verifying that a group behind the spans whose excluded branch is no whole member is not read, and the fix moves
    /// <c>_c</c> in front of <c>_a</c>
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7109-006")]
    public async Task FixMovesAFieldInFrontOfAGroupBehindTheSpansThatHoldsAModifier(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private int _a;
                                  private readonly int {|RH7109:_c|};
                              #if DEBUG
                                  static
                              #endif
                                  private int _q;
                              }
                              """;

        const string fixedSource = """
                                   public class C
                                   {
                                       private readonly int _c;
                                       private int _a;
                                   #if DEBUG
                                       static
                                   #endif
                                       private int _q;
                                   }
                                   """;

        await Verify(WithEndOfLine(source, endOfLine), WithEndOfLine(fixedSource, endOfLine));
    }

    /// <summary>
    /// Verifying that Fix All moves the field of a class without a conditional directive and leaves the field of a partial
    /// class whose other declaration wraps its layout attribute in a conditional group reported without a fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("SHARED-FIX-002")]
    public async Task FixAllSkipsTheFieldOfAPartialClassWhoseOtherDeclarationHoldsAConditionalLayoutAttribute()
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
                                  private readonly int {|RH7109:_c|};
                              }

                              public class D
                              {
                                  private int _a;
                                  private readonly int {|RH7109:_c|};
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
                                       private int _a;
                                       private readonly int {|RH7109:_c|};
                                   }

                                   public class D
                                   {
                                       private readonly int _c;
                                       private int _a;
                                   }
                                   """;

        await Verify(source,
                     fixedSource,
                     test =>
                     {
                         // The diagnostic whose fix is withheld stays and is marked in the expected source
                         test.FixedState.MarkupHandling = MarkupMode.Allow;
                         test.BatchFixedState.MarkupHandling = MarkupMode.Allow;
                     });
    }

    /// <summary>
    /// Verifying the specification example: a field that moves into a slot indented as it is keeps the column of its
    /// continuation line, although formatting the result would align that line under the initializer
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-FIX-007")]
    [Requirement("RH7109-005")]
    public async Task ContinuationLineOfAMovedFieldKeepsItsColumn(string endOfLine)
    {
        const string source = """
                              class C
                              {
                                  private int _a;
                                  private readonly int {|RH7109:_b|} = 1
                                          + 2;
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                       private readonly int _b = 1
                                               + 2;
                                       private int _a;
                                   }
                                   """;

        await VerifyCodeFixWithEndOfLine(source, fixedSource, endOfLine);
    }

    /// <summary>
    /// Verifying the specification example: a field indented deeper than its slot moves every one of its lines by the
    /// shift of its first token
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-FIX-007")]
    public async Task FieldIndentedDeeperThanItsSlotMovesAllItsLinesByTheSameShift(string endOfLine)
    {
        const string source = """
                              class C
                              {
                                  private int _a;
                                      private readonly int {|RH7109:_b|} = 1
                                          + 2;
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                       private readonly int _b = 1
                                           + 2;
                                       private int _a;
                                   }
                                   """;

        await VerifyCodeFixWithEndOfLine(source, fixedSource, endOfLine);
    }

    /// <summary>
    /// Verifying that a line break inside the moved field stays where it is written, although formatting the result
    /// would join the semicolon to the line in front of it
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-FIX-007")]
    public async Task LineBreakInsideTheMovedFieldStays(string endOfLine)
    {
        const string source = """
                              class C
                              {
                                  private int _a;
                                  private readonly int {|RH7109:_b|}
                                      ;
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                       private readonly int _b
                                           ;
                                       private int _a;
                                   }
                                   """;

        await VerifyCodeFixWithEndOfLine(source, fixedSource, endOfLine);
    }

    /// <summary>
    /// Verifying that an expression-bodied method the fix neither moves nor crosses keeps its expression body
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-006")]
    public async Task UntouchedExpressionBodiedMethodKeepsItsBodyWhenAFieldMoves()
    {
        const string source = """
                              class C
                              {
                                  private int _a;
                                  private readonly int {|RH7109:_b|};

                                  private static int Log(string name) => 0;
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                       private readonly int _b;
                                       private int _a;

                                       private static int Log(string name) => 0;
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifying the specification example: the moved field shares its line with the field in front of it, so no code fix
    /// is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-ORDER-009")]
    [Requirement("RH7109-006")]
    public async Task NoCodeFixWhenTheMovedFieldSharesItsLineWithTheFieldInFrontOfIt()
    {
        const string source = """
                              class C
                              {
                                  private int _a; private readonly int {|RH7109:_b|};
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifying the specification example: the target shares its line with the opening brace of the type, so no code fix
    /// is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-ORDER-009")]
    [Requirement("RH7109-006")]
    public async Task NoCodeFixWhenTheTargetSharesItsLineWithTheOpeningBraceOfTheType()
    {
        const string source = """
                              class C
                              {   private int _a;
                                  private readonly int {|RH7109:_b|};
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifying that no code fix is offered when the moved field shares its line with the field behind it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-ORDER-009")]
    public async Task NoCodeFixWhenTheMovedFieldSharesItsLineWithTheFieldBehindIt()
    {
        const string source = """
                              class C
                              {
                                  private int _a;
                                  private readonly int {|RH7109:_b|}; private int _c;
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifying that no code fix is offered when the moved field shares its line with the closing brace of the type
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-ORDER-009")]
    public async Task NoCodeFixWhenTheMovedFieldSharesItsLineWithTheClosingBraceOfTheType()
    {
        const string source = """
                              class C
                              {
                                  private int _a;
                                  private readonly int {|RH7109:_b|}; }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifying the specification example: whole lines move, and the comment behind the moved field on its line moves
    /// with it
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-009")]
    public async Task CommentBehindTheMovedFieldMovesWithIt(string endOfLine)
    {
        const string source = """
                              class C
                              {
                                  private int _a;
                                  private readonly int {|RH7109:_b|}; // cached
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                       private readonly int _b; // cached
                                       private int _a;
                                   }
                                   """;

        await VerifyCodeFixWithEndOfLine(source, fixedSource, endOfLine);
    }

    /// <summary>
    /// Verifying the specification example: a field indented deeper than its slot that holds a verbatim string over two
    /// lines is reported, and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-ORDER-010")]
    [Requirement("RH7109-006")]
    public async Task NoCodeFixWhenAShiftedFieldHoldsAVerbatimStringOverTwoLines()
    {
        const string source = """
                              class C
                              {
                                  private int _a;
                                      private readonly string {|RH7109:_b|} = @"x
                              y";
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifying the specification example: indented as its slot, a field that holds a verbatim string over two lines moves,
    /// and the string keeps its lines
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-010")]
    public async Task FieldIndentedAsItsSlotMovesWithAVerbatimStringOverTwoLines(string endOfLine)
    {
        const string source = """
                              class C
                              {
                                  private int _a;
                                  private readonly string {|RH7109:_b|} = @"x
                              y";
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                       private readonly string _b = @"x
                                   y";
                                       private int _a;
                                   }
                                   """;

        await VerifyCodeFixWithEndOfLine(source, fixedSource, endOfLine);
    }

    /// <summary>
    /// Verifying that no code fix is offered when a field indented deeper than its slot holds a comment over two lines in
    /// front of its first token
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-ORDER-010")]
    public async Task NoCodeFixWhenAShiftedFieldHoldsACommentOverTwoLinesInItsLeadingTrivia()
    {
        const string source = """
                              class C
                              {
                                  private int _a;
                                      /* one
                                         two */
                                      private readonly int {|RH7109:_b|};
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifying that a field indented as its slot moves with a comment over two lines in front of its first token, and the
    /// comment keeps its lines
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-ORDER-010")]
    public async Task FieldIndentedAsItsSlotMovesWithACommentOverTwoLinesInItsLeadingTrivia()
    {
        const string source = """
                              class C
                              {
                                  private int _a;
                                  /* one
                                        two */
                                  private readonly int {|RH7109:_b|};
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                       /* one
                                             two */
                                       private readonly int _b;
                                       private int _a;
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifying that a comment the formatter refuses in another member of the type no longer withholds the fix, which
    /// moves the field without formatting the type
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-006")]
    [Requirement("SHARED-FIX-010")]
    public async Task FixIsOfferedWhenAnotherMemberOfTheTypeHoldsACommentTheFormatterRefuses()
    {
        const string source = """
                              internal class C
                              {
                                  private int _a;
                                  private readonly int {|RH7109:_b|};

                                  private void M()
                                  {
                                      int /* c */ y = 1;
                                  }
                              }
                              """;

        const string fixedSource = """
                                   internal class C
                                   {
                                       private readonly int _b;
                                       private int _a;

                                       private void M()
                                       {
                                           int /* c */ y = 1;
                                       }
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Characterizes the input that a report described as moving a blank line into an active <c>#else</c> branch: a field
    /// moved across an expression-bodied property with a conditional pair in its body is reported, and no code fix is
    /// offered, so the reported layout change does not occur
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NoCodeFixWhenTheMoveCrossesAPropertyWithAConditionalPairInItsBody()
    {
        const string source = """
                              class C
                              {
                                  private int _a;

                                  public int P =>
                              #if DEBUG
                                      1;
                              #else
                                      2;
                              #endif

                                  private int _e;
                                  private readonly int {|RH7109:_c|};
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifying the specification example: no code fix is offered when a shift to the left would cut a moving line of
    /// the field off at column 0, because the comment line holds two whitespace characters and the shift removes four
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-ORDER-010")]
    public async Task NoCodeFixWhenALeftShiftWouldCutAMovingLineOffAtColumnZero()
    {
        const string source = """
                              class C
                              {
                                  private int _a;
                                      private readonly int {|RH7109:_b|} =
                                // two columns
                                          1;
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifying that a shift to the left moves a line that holds exactly as many whitespace characters as the shift
    /// removes into column 0, together with the other lines of the field
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-010")]
    [Requirement("SHARED-FIX-007")]
    public async Task LeftShiftMovesALineWithJustEnoughWhitespaceIntoColumnZero(string endOfLine)
    {
        const string source = """
                              class C
                              {
                                  private int _a;
                                      private readonly int {|RH7109:_b|} =
                                  // four columns
                                          1;
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                       private readonly int _b =
                                   // four columns
                                           1;
                                       private int _a;
                                   }
                                   """;

        await VerifyCodeFixWithEndOfLine(source, fixedSource, endOfLine);
    }

    /// <summary>
    /// Verifying that a tab at the start of a moving line counts as one column: the continuation line starts with one
    /// tab, fewer whitespace characters than the shift of four columns to the left removes, so no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-ORDER-010")]
    [Requirement("SHARED-FIX-007")]
    public async Task NoCodeFixWhenALeftShiftRemovesMoreColumnsThanATabAtTheStartOfAMovingLine()
    {
        const string source = """
                              class C
                              {
                                  private int _a;
                                      private readonly int {|RH7109:_b|} = 1
                              {TAB}+ 2;
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source.Replace("{TAB}", "\t"));
    }

    /// <summary>
    /// Verifying that a shift to the right writes spaces in front of a moving line that starts with a tab and keeps the
    /// tab: the field is indented two columns less than its slot, so every one of its lines moves two columns to the
    /// right
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-FIX-007")]
    public async Task RightShiftWritesSpacesInFrontOfATabAtTheStartOfAMovingLine(string endOfLine)
    {
        const string source = """
                              class C
                              {
                                  private int _a;
                                private readonly int {|RH7109:_b|} = 1
                              {TAB}+ 2;
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                       private readonly int _b = 1
                                     {TAB}+ 2;
                                       private int _a;
                                   }
                                   """;

        await VerifyCodeFixWithEndOfLine(source.Replace("{TAB}", "\t"), fixedSource.Replace("{TAB}", "\t"), endOfLine);
    }

    /// <summary>
    /// Verifying the specification example: with documentation comments parsed, the <c>///</c> comment behind the
    /// constant documents the target <c>_a</c>; a move in front of <c>_a</c> would hand the comment to the moved field,
    /// so the field is reported and no code fix is offered
    /// </summary>
    /// <param name="documentationMode">Documentation mode of the syntax tree</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow(DocumentationMode.Parse)]
    [DataRow(DocumentationMode.Diagnose)]
    [Requirement("SHARED-ORDER-009")]
    public async Task NoCodeFixWhenADocumentationCommentBehindCodeDocumentsTheTarget(DocumentationMode documentationMode)
    {
        const string source = """
                              class C
                              {
                                  private const int K = 1; /// <summary>A</summary>
                                  private int _a;
                                  private readonly int {|RH7109:_b|};
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source, documentationMode);
    }

    /// <summary>
    /// Verifying the specification example: the crossed fields <c>_a</c> and <c>_c</c> share a line; they keep it and
    /// take their position behind the moved field as one block
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-009")]
    public async Task CrossedFieldsThatShareALineKeepItBehindTheMovedField(string endOfLine)
    {
        const string source = """
                              class C
                              {
                                  private int _a; private int _c;
                                  private readonly int {|RH7109:_b|};
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                       private readonly int _b;
                                       private int _a; private int _c;
                                   }
                                   """;

        await VerifyCodeFixWithEndOfLine(source, fixedSource, endOfLine);
    }

    /// <summary>
    /// Verifying the specification example: a field indented less deeply than its slot holds an interpolated verbatim
    /// string whose second line starts at a hole; the shift to the right would write whitespace into the string's value,
    /// so no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-ORDER-010")]
    [Requirement("RH7109-006")]
    public async Task NoCodeFixWhenARightShiftedFieldHoldsAnInterpolatedVerbatimStringWhoseSecondLineStartsAtAHole()
    {
        const string source = """
                              class C
                              {
                                  private int _a;
                                private readonly string {|RH7109:_b|} = $@"a
                              {nameof(C)}";
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifying that no code fix is offered when a field indented less deeply than its slot holds an interpolated
    /// verbatim string whose second line starts at the closing quote
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-ORDER-010")]
    public async Task NoCodeFixWhenARightShiftedFieldHoldsAnInterpolatedVerbatimStringWhoseSecondLineStartsAtTheClosingQuote()
    {
        const string source = """
                              class C
                              {
                                  private int _a;
                                private readonly string {|RH7109:_b|} = $@"a{nameof(C)}
                              ";
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifying that no code fix is offered when a field indented less deeply than its slot holds an interpolated string
    /// whose hole spans lines
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-ORDER-010")]
    public async Task NoCodeFixWhenARightShiftedFieldHoldsAnInterpolatedStringWhoseHoleSpansLines()
    {
        const string source = """
                              class C
                              {
                                  private int _a;
                                private readonly string {|RH7109:_b|} = $"{1
                                    + 1}";
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifying that no code fix is offered when a field indented deeper than its slot holds an interpolated verbatim
    /// string whose second line starts with whitespace in front of a hole
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-ORDER-010")]
    public async Task NoCodeFixWhenALeftShiftedFieldHoldsAnInterpolatedVerbatimStringOverTwoLines()
    {
        const string source = """
                              class C
                              {
                                  private int _a;
                                      private readonly string {|RH7109:_b|} = $@"a
                                  {nameof(C)}";
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifying that a field indented as its slot moves with an interpolated verbatim string whose second line starts at
    /// a hole, and the string keeps its lines
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-010")]
    public async Task FieldIndentedAsItsSlotMovesWithAnInterpolatedVerbatimStringOverTwoLines(string endOfLine)
    {
        const string source = """
                              class C
                              {
                                  private int _a;
                                  private readonly string {|RH7109:_b|} = $@"a
                              {nameof(C)}";
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                       private readonly string _b = $@"a
                                   {nameof(C)}";
                                       private int _a;
                                   }
                                   """;

        await VerifyCodeFixWithEndOfLine(source, fixedSource, endOfLine);
    }

    /// <summary>
    /// Verifying the specification example: the moved field holds an LF line break in a document whose line breaks are
    /// otherwise CRLF, so no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-008")]
    public async Task NoCodeFixWhenTheMovedFieldHoldsALineBreakOtherThanTheDocumentsLineEnding()
    {
        var source = NormalizeToCarriageReturnLineFeed("""
                                                       class C
                                                       {
                                                           private int _a;
                                                           private readonly int {|RH7109:_b|} ={LF}
                                                               1;
                                                       }
                                                       """).Replace("{LF}\r\n", "\n");

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifying the specification example: with every line break alike, the field that holds a line break moves in front
    /// of <c>_a</c>, and every line break of the result is the document's line ending
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-FIX-008")]
    public async Task MovedFieldThatHoldsALineBreakMovesWhenEveryLineBreakIsAlike(string endOfLine)
    {
        const string source = """
                              class C
                              {
                                  private int _a;
                                  private readonly int {|RH7109:_b|} =
                                      1;
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                       private readonly int _b =
                                           1;
                                       private int _a;
                                   }
                                   """;

        await VerifyCodeFixWithEndOfLine(source, fixedSource, endOfLine);
    }

    /// <summary>
    /// Verifying that no code fix is offered in a document whose line breaks are all a lone carriage return, where the
    /// moved field holds a line break
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-008")]
    public async Task NoCodeFixWhenTheMovedFieldHoldsALoneCarriageReturnLineBreak()
    {
        var source = """
                     class C
                     {
                         private int _a;
                         private readonly int {|RH7109:_b|} = 1
                             + 2;
                     }
                     """.Replace("\r\n", "\n").Replace("\n", "\r");

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifying that an LF line break outside the moved code, in a document whose line breaks are otherwise CRLF, does not
    /// withhold the fix, and stays as it is
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-008")]
    public async Task LineBreakOtherThanTheDocumentsLineEndingOutsideTheMovedCodeKeepsTheFix()
    {
        var source = NormalizeToCarriageReturnLineFeed("""
                                                       class C{LF}
                                                       {
                                                           private int _a;
                                                           private readonly int _b =
                                                               1;
                                                       }
                                                       """).Replace("{LF}\r\n", "\n");

        var fixedSource = NormalizeToCarriageReturnLineFeed("""
                                                            class C{LF}
                                                            {
                                                                private readonly int _b =
                                                                    1;
                                                                private int _a;
                                                            }
                                                            """).Replace("{LF}\r\n", "\n");

        Assert.AreEqual(fixedSource, await ApplyCodeFixAsync(source));
    }

    /// <summary>
    /// Verifies the specification example in which a field outside every region and a readonly field in a region, and a
    /// field and a readonly field in two top-level regions, are not compared
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7109-003")]
    [Requirement("SHARED-ORDER-004")]
    public async Task FieldAndReadonlyFieldInDifferentRegionsAreNotCompared()
    {
        const string testData = """
                                class C
                                {
                                    private int _count;

                                    #region Identity

                                    private readonly int _id;

                                    #endregion
                                }

                                class D
                                {
                                    #region Fields

                                    private int _count;

                                    #endregion

                                    #region Identity

                                    private readonly int _id;

                                    #endregion
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a readonly field outside every region is not compared with a field of its group in a region
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7109-003")]
    public async Task ReadonlyFieldOutsideEveryRegionIsNotComparedWithAFieldInARegion()
    {
        const string testData = """
                                class C
                                {
                                    #region Fields

                                    private int _count;

                                    #endregion

                                    private readonly int _id;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies the specification example in which the first field of the group lies in an earlier region, so the code
    /// fix moves the readonly field in front of the field in its own region, which is not the first member of that
    /// region
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7109-005")]
    public async Task CodeFixTargetsTheFirstFieldOfTheGroupInTheReadonlyFieldsOwnRegion()
    {
        const string testData = """
                                class C
                                {
                                    #region A

                                    private int _a;

                                    #endregion

                                    #region B

                                    public int B;
                                    private int _b;
                                    private readonly int {|RH7109:_c|};

                                    #endregion
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     #region A

                                     private int _a;

                                     #endregion

                                     #region B

                                     public int B;
                                     private readonly int _c;
                                     private int _b;

                                     #endregion
                                 }
                                 """;

        await Verify(testData, fixedData);
    }

    /// <summary>
    /// Verifies that the diagnostic is reported without a code fix when the first field of the group in the readonly
    /// field's own region is the first member of that region, because the region's opening line lies in the crossed span
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7109-006")]
    [Requirement("SHARED-ORDER-004")]
    public async Task NoCodeFixWhenTheFirstFieldOfTheGroupInTheOwnRegionIsTheFirstMemberOfThatRegion()
    {
        const string source = """
                              class C
                              {
                                  #region A

                                  private int _a;

                                  #endregion

                                  #region B

                                  private int _b;
                                  private readonly int {|RH7109:_c|};

                                  #endregion
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies the union example of RH7109-001: a static readonly field after a static field of the same group in a
    /// union is reported, and the fix moves it in front of that field
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7109-001")]
    [Requirement("RH7109-005")]
    public async Task UnionReadonlyFieldAfterFieldIsReportedAndMoved()
    {
        const string testCode = """
                                union U(int, string)
                                {
                                    private static int _a;
                                    private static readonly int {|RH7109:_b|} = 1;
                                }
                                """;

        const string fixedCode = """
                                 union U(int, string)
                                 {
                                     private static readonly int _b = 1;
                                     private static int _a;
                                 }
                                 """;

        var unionSupport = await UnionSupport.GetReferenceAsync();

        await Verify(testCode, fixedCode, test => test.TestState.AdditionalReferences.Add(unionSupport));
    }

    #endregion // Tests
}