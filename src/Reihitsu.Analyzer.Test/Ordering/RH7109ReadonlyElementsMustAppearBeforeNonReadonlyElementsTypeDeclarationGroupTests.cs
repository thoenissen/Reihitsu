using System.Threading.Tasks;

using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Organization;
using Reihitsu.Analyzer.Rules.Organization;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Ordering;

/// <summary>
/// Test methods for <see cref="RH7109ReadonlyElementsMustAppearBeforeNonReadonlyElementsCodeFixProvider"/> on conditional directives in the declarations of the type, in its other partial declarations, and around the spans the fix moves
/// </summary>
[TestClass]
public class RH7109ReadonlyElementsMustAppearBeforeNonReadonlyElementsTypeDeclarationGroupTests : BatchCodeFixTestsBase<RH7109ReadonlyElementsMustAppearBeforeNonReadonlyElementsAnalyzer, RH7109ReadonlyElementsMustAppearBeforeNonReadonlyElementsCodeFixProvider>
{
    #region Tests

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