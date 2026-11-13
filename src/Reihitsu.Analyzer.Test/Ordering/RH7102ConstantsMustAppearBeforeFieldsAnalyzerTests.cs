using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Organization;
using Reihitsu.Analyzer.Rules.Organization;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Ordering;

/// <summary>
/// Test methods for <see cref="RH7102ConstantsMustAppearBeforeFieldsAnalyzer"/> and <see cref="RH7102ConstantsMustAppearBeforeFieldsCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH7102ConstantsMustAppearBeforeFieldsAnalyzerTests : BatchCodeFixTestsBase<RH7102ConstantsMustAppearBeforeFieldsAnalyzer, RH7102ConstantsMustAppearBeforeFieldsCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying const fields are reported and fixed when they appear after mutable fields
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7102-001")]
    [Requirement("RH7102-004")]
    [Requirement("RH7102-005")]
    public async Task ConstFieldsAreReportedAndFixedWhenTheyAppearAfterMutableFields()
    {
        const string testCode = """
                                public class TestClass
                                {
                                    private int _value;
                                    private const int {|#0:MaxValue|} = 1;
                                }
                                """;

        const string fixedCode = """
                                 public class TestClass
                                 {
                                     private const int MaxValue = 1;
                                     private int _value;
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH7102ConstantsMustAppearBeforeFieldsAnalyzer.DiagnosticId, AnalyzerResources.RH7102MessageFormat));
    }

    /// <summary>
    /// Verifying the blank line that already separated the const field from the mutable field survives the
    /// reorder
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7102-005")]
    public async Task SeparatorSurvivesTheReorderWhenConstFieldWasAlreadySeparated()
    {
        const string testCode = """
                                public class TestClass
                                {
                                    private int _value;

                                    private const int {|#0:MaxValue|} = 1;
                                }
                                """;

        const string fixedCode = """
                                 public class TestClass
                                 {
                                     private const int MaxValue = 1;

                                     private int _value;
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH7102ConstantsMustAppearBeforeFieldsAnalyzer.DiagnosticId, AnalyzerResources.RH7102MessageFormat));
    }

    /// <summary>
    /// Verifying no code fix is offered when the move would separate a preprocessor directive from its partner,
    /// with the region opening in the moved field's own leading trivia and closing after it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NoCodeFixWhenDirectivesAreInLeadingTrivia()
    {
        const string testCode = """
                                public class TestClass
                                {
                                    private int _value;

                                    #region Constants
                                    private const int MaxValue = 1;
                                    #endregion
                                }
                                """;

        var actions = await GetCodeFixActionsAsync(testCode,
                                                   RH7102ConstantsMustAppearBeforeFieldsAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<VariableDeclaratorSyntax>()
                                                               .Single(declarator => declarator.Identifier.ValueText == "MaxValue")
                                                               .Identifier
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifying the specification example: a constant after a field of its accessibility group is reported, static
    /// readonly fields and both spellings of <c>protected internal</c> included
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7102-001")]
    [Requirement("RH7102-004")]
    public async Task ConstantsAfterFieldsOfTheirAccessibilityGroupAreReported()
    {
        const string source = """
                              class C
                              {
                                  private int _value;
                                  private static readonly int _shared = 1;
                                  private const int {|RH7102:Max|} = 1;
                                  internal protected int Count;
                                  protected internal const int {|RH7102:Min|} = 0;
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifying the specification example: a constant that follows only fields of another accessibility group is not
    /// reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7102-002")]
    public async Task ConstantAfterFieldsOfAnotherAccessibilityIsNotReported()
    {
        const string source = """
                              class C
                              {
                                  public int Value;
                                  private const int Max = 1;
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifying the specification example: a constant in a region nested in the top-level region of an earlier field
    /// is reported, and no code fix is offered because the region opens in the crossed span
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7102-003")]
    [Requirement("RH7102-006")]
    public async Task ConstantInANestedRegionOfTheSameTopLevelRegionIsReported()
    {
        const string source = """
                              class C
                              {
                                  #region Fields

                                  private int _value;

                                  #region Limits

                                  private const int {|RH7102:Max|} = 1;

                                  #endregion

                                  #endregion
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifying the specification example: the diagnostic is on the first declarator of the misplaced declaration
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7102-004")]
    public async Task DiagnosticIsOnTheFirstDeclarator()
    {
        const string source = """
                              class C
                              {
                                  private int _value;
                                  private const int {|RH7102:Min|} = 0, Max = 1;
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifying the specification example: the fix moves the constant in front of the first field of its group,
    /// across a field of another group
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7102-005")]
    [Requirement("SHARED-ORDER-001")]
    public async Task FixMovesTheConstantInFrontOfTheFirstFieldOfItsGroup()
    {
        const string source = """
                              class C
                              {
                                  private int _a;
                                  public int B;
                                  private int _c;
                                  private const int {|RH7102:D|} = 1;
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                       private const int D = 1;
                                       private int _a;
                                       public int B;
                                       private int _c;
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifying the specification example: the documentation comment travels with the moved constant, and the blank
    /// line stays where it was
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7102-005")]
    [Requirement("SHARED-ORDER-002")]
    public async Task FixMovesTheDocumentationCommentWithTheConstant()
    {
        const string source = """
                              class C
                              {
                                  private int _value;

                                  /// <summary>
                                  /// The largest value
                                  /// </summary>
                                  private const int {|RH7102:Max|} = 1;
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                       /// <summary>
                                       /// The largest value
                                       /// </summary>
                                       private const int Max = 1;

                                       private int _value;
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifying the specification example: a pragma pair around a crossed field withholds the fix, and the diagnostic
    /// is still reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7102-006")]
    public async Task PragmaDirectivesAroundACrossedFieldWithholdTheFix()
    {
        const string source = """
                              class C
                              {
                                  private int _value;
                              #pragma warning disable CS0414
                                  private int _unused = 1;
                              #pragma warning restore CS0414
                                  private const int {|RH7102:Max|} = 1;
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Characterizes current behavior that no requirement states: a private constant after a field without
    /// accessibility modifier is not reported, because the written modifiers form different groups
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task PrivateConstantAfterFieldWithoutAccessibilityModifierIsNotReported()
    {
        const string source = """
                              class C
                              {
                                  int _a;
                                  private const int B = 1;
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifying that the fix leaves the other members of the type as written, and an empty nested class keeps its braces
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-006")]
    public async Task FixLeavesTheOtherMembersOfTheTypeAsWritten()
    {
        const string source = """
                              class C
                              {
                                  private int _a;
                                  public class Inner { }
                                  private const int {|RH7102:D|} = 1;
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                       private const int D = 1;
                                       private int _a;
                                       public class Inner { }
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifying the specification example: without <c>DEBUG</c> defined, an instance field of excluded code in front of
    /// the constant would move with it in front of an instance field of a struct, so a build with <c>DEBUG</c> would lay
    /// out the fields in another order; the constant is reported, and no code fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH7102-006")]
    [Requirement("SHARED-ORDER-007")]
    public async Task NoCodeFixWhenExcludedCodeInFrontOfTheConstantWouldReorderTheInstanceFieldsOfAStruct(string endOfLine)
    {
        const string source = """
                              public struct S
                              {
                                  private int _x;
                              #if DEBUG
                                  private int _y;
                              #endif
                                  private const int {|RH7102:K|} = 1;
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying that the instance fields of a record struct, whose field order sets its layout, withhold the move of
    /// an instance field of excluded code in front of the constant
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH7102-006")]
    [Requirement("SHARED-ORDER-007")]
    public async Task NoCodeFixWhenExcludedCodeInFrontOfTheConstantWouldReorderTheInstanceFieldsOfARecordStruct(string endOfLine)
    {
        const string source = """
                              public record struct R
                              {
                                  private int _x;
                              #if DEBUG
                                  private int _y;
                              #endif
                                  private const int {|RH7102:K|} = 1;
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying that the instance fields of a class with sequential layout withhold the move of an instance field of
    /// excluded code in front of the constant
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH7102-006")]
    [Requirement("SHARED-ORDER-007")]
    public async Task NoCodeFixWhenExcludedCodeInFrontOfTheConstantWouldReorderTheInstanceFieldsOfASequentialClass(string endOfLine)
    {
        const string source = """
                              using System.Runtime.InteropServices;

                              [StructLayout(LayoutKind.Sequential)]
                              public class C
                              {
                                  private int _x;
                              #if DEBUG
                                  private int _y;
                              #endif
                                  private const int {|RH7102:K|} = 1;
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying that an auto-property of excluded code in front of the constant counts as an instance field of a struct
    /// and withholds the move across an instance field
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH7102-006")]
    [Requirement("SHARED-ORDER-007")]
    public async Task NoCodeFixWhenAnAutoPropertyOfExcludedCodeInFrontOfTheConstantWouldReorderTheInstanceFieldsOfAStruct(string endOfLine)
    {
        const string source = """
                              public struct S
                              {
                                  private int _x;
                              #if DEBUG
                                  private int Y { get; set; }
                              #endif
                                  private const int {|RH7102:K|} = 1;
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying the other side of the layout condition: in a struct with explicit layout, whose fields keep their
    /// offsets in every order, an instance field of excluded code moves with the constant, and the fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH7102-006")]
    [Requirement("SHARED-ORDER-007")]
    public async Task FixCarriesAnInstanceFieldOfExcludedCodeWithTheConstantInAnExplicitLayoutStruct(string endOfLine)
    {
        const string source = """
                              using System.Runtime.InteropServices;

                              [StructLayout(LayoutKind.Explicit)]
                              public struct S
                              {
                                  [FieldOffset(0)]
                                  private int _x;
                              #if DEBUG
                                  [FieldOffset(4)]
                                  private int _y;
                              #endif
                                  private const int {|RH7102:K|} = 1;
                              }
                              """;

        const string fixedSource = """
                                   using System.Runtime.InteropServices;

                                   [StructLayout(LayoutKind.Explicit)]
                                   public struct S
                                   {
                                   #if DEBUG
                                       [FieldOffset(4)]
                                       private int _y;
                                   #endif
                                       private const int K = 1;
                                       [FieldOffset(0)]
                                       private int _x;
                                   }
                                   """;

        await Verify(WithEndOfLine(source, endOfLine), WithEndOfLine(fixedSource, endOfLine));
    }

    /// <summary>
    /// Verifying the other side of the layout condition: a static field of excluded code in front of the constant takes
    /// no place in the layout of the struct, so it moves with the constant, and the fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH7102-006")]
    [Requirement("SHARED-ORDER-007")]
    public async Task FixCarriesAStaticFieldOfExcludedCodeWithTheConstantInAStruct(string endOfLine)
    {
        const string source = """
                              public struct S
                              {
                                  private int _x;
                              #if DEBUG
                                  private static int _y;
                              #endif
                                  private const int {|RH7102:K|} = 1;
                              }
                              """;

        const string fixedSource = """
                                   public struct S
                                   {
                                   #if DEBUG
                                       private static int _y;
                                   #endif
                                       private const int K = 1;
                                       private int _x;
                                   }
                                   """;

        await Verify(WithEndOfLine(source, endOfLine), WithEndOfLine(fixedSource, endOfLine));
    }

    /// <summary>
    /// Verifying that excluded code in the crossed span keeps its order: the instance field of excluded code between the
    /// target and the constant is crossed, not moved, so the fix is offered also in a struct
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH7102-006")]
    [Requirement("SHARED-ORDER-007")]
    public async Task FixCrossesAnInstanceFieldOfExcludedCodeInAStruct(string endOfLine)
    {
        const string source = """
                              public struct S
                              {
                                  private int _x;
                              #if DEBUG
                                  private int _y;
                              #endif
                                  private int _z;
                                  private const int {|RH7102:K|} = 1;
                              }
                              """;

        const string fixedSource = """
                                   public struct S
                                   {
                                       private const int K = 1;
                                       private int _x;
                                   #if DEBUG
                                       private int _y;
                                   #endif
                                       private int _z;
                                   }
                                   """;

        await Verify(WithEndOfLine(source, endOfLine), WithEndOfLine(fixedSource, endOfLine));
    }

    /// <summary>
    /// Verifying the specification example: without <c>DEBUG</c> defined, an initialized field of excluded code in front
    /// of the constant would move with it across an initialized field, so a build with <c>DEBUG</c> would run the
    /// initializers in another order; no code fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH7102-006")]
    [Requirement("SHARED-ORDER-007")]
    public async Task NoCodeFixWhenAnInitializedFieldOfExcludedCodeInFrontOfTheConstantWouldCrossAnInitializedField(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private int _i = Log("i");
                              #if DEBUG
                                  private int _c = Log("c");
                              #endif
                                  private const int {|RH7102:K|} = 1;

                                  private static int Log(string name)
                                  {
                                      return 0;
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying the specification example on the other side of the initializer condition: the crossed field has no
    /// initializer, so the initialized field of excluded code moves with the constant, and the fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH7102-006")]
    [Requirement("SHARED-ORDER-007")]
    public async Task FixCarriesAnInitializedFieldOfExcludedCodeWithTheConstantAcrossAFieldWithoutInitializer(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private int _i;
                              #if DEBUG
                                  private int _c = Log("c");
                              #endif
                                  private const int {|RH7102:K|} = 1;

                                  private static int Log(string name)
                                  {
                                      return 0;
                                  }
                              }
                              """;

        const string fixedSource = """
                                   public class C
                                   {
                                   #if DEBUG
                                       private int _c = Log("c");
                                   #endif
                                       private const int K = 1;
                                       private int _i;

                                       private static int Log(string name)
                                       {
                                           return 0;
                                       }
                                   }
                                   """;

        await Verify(WithEndOfLine(source, endOfLine), WithEndOfLine(fixedSource, endOfLine));
    }

    /// <summary>
    /// Verifying that a field of excluded code without initializer moves with the constant across a field without
    /// initializer of a class, and the fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH7102-006")]
    [Requirement("SHARED-ORDER-007")]
    public async Task FixCarriesAFieldOfExcludedCodeWithoutInitializerWithTheConstant(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private int _i;
                              #if DEBUG
                                  private int _c;
                              #endif
                                  private const int {|RH7102:K|} = 1;
                              }
                              """;

        const string fixedSource = """
                                   public class C
                                   {
                                   #if DEBUG
                                       private int _c;
                                   #endif
                                       private const int K = 1;
                                       private int _i;
                                   }
                                   """;

        await Verify(WithEndOfLine(source, endOfLine), WithEndOfLine(fixedSource, endOfLine));
    }

    /// <summary>
    /// Verifying that an initialized static field of excluded code in front of the constant withholds the move across an
    /// initialized static field
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH7102-006")]
    [Requirement("SHARED-ORDER-007")]
    public async Task NoCodeFixWhenAnInitializedStaticFieldOfExcludedCodeInFrontOfTheConstantWouldCrossAnInitializedStaticField(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private static int _i = Log("i");
                              #if DEBUG
                                  private static int _c = Log("c");
                              #endif
                                  private const int {|RH7102:K|} = 1;

                                  private static int Log(string name)
                                  {
                                      return 0;
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying the other side of the context condition: an initialized static field of excluded code crosses only an
    /// instance initializer, which runs at another time, so the fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH7102-006")]
    [Requirement("SHARED-ORDER-007")]
    public async Task FixCarriesAnInitializedStaticFieldOfExcludedCodeWithTheConstantAcrossAnInstanceInitializer(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private int _i = Log("i");
                              #if DEBUG
                                  private static int _c = Log("c");
                              #endif
                                  private const int {|RH7102:K|} = 1;

                                  private static int Log(string name)
                                  {
                                      return 0;
                                  }
                              }
                              """;

        const string fixedSource = """
                                   public class C
                                   {
                                   #if DEBUG
                                       private static int _c = Log("c");
                                   #endif
                                       private const int K = 1;
                                       private int _i = Log("i");

                                       private static int Log(string name)
                                       {
                                           return 0;
                                       }
                                   }
                                   """;

        await Verify(WithEndOfLine(source, endOfLine), WithEndOfLine(fixedSource, endOfLine));
    }

    /// <summary>
    /// Verifying that a property initializer of excluded code in front of the constant withholds the move across an
    /// initialized field
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH7102-006")]
    [Requirement("SHARED-ORDER-007")]
    public async Task NoCodeFixWhenAnInitializedPropertyOfExcludedCodeInFrontOfTheConstantWouldCrossAnInitializedField(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private int _i = Log("i");
                              #if DEBUG
                                  private int P { get; } = Log("p");
                              #endif
                                  private const int {|RH7102:K|} = 1;

                                  private static int Log(string name)
                                  {
                                      return 0;
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying that an initialized event field of excluded code in front of the constant withholds the move across an
    /// initialized field
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH7102-006")]
    [Requirement("SHARED-ORDER-007")]
    public async Task NoCodeFixWhenAnInitializedEventFieldOfExcludedCodeInFrontOfTheConstantWouldCrossAnInitializedField(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private int _i = Log("i");
                              #if DEBUG
                                  private event System.EventHandler Changed = delegate { };
                              #endif
                                  private const int {|RH7102:K|} = 1;

                                  private static int Log(string name)
                                  {
                                      return 0;
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying that every crossed member counts, not only the target: the move crosses an initialized property of
    /// another accessibility between the target and the constant, so the initialized field of excluded code in front of
    /// the constant withholds the fix
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH7102-006")]
    [Requirement("SHARED-ORDER-007")]
    public async Task NoCodeFixWhenAnInitializedFieldOfExcludedCodeInFrontOfTheConstantWouldCrossAnInitializedPropertyBehindTheTarget(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private int _i;
                                  public int Count { get; } = Log("p");
                              #if DEBUG
                                  private int _c = Log("c");
                              #endif
                                  private const int {|RH7102:K|} = 1;

                                  private static int Log(string name)
                                  {
                                      return 0;
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying that code an <c>#if false</c> block excludes counts like any other excluded code
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH7102-006")]
    [Requirement("SHARED-ORDER-007")]
    public async Task NoCodeFixWhenAnInitializedFieldOfAnIfFalseBlockInFrontOfTheConstantWouldCrossAnInitializedField(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private int _i = Log("i");
                              #if false
                                  private int _c = Log("c");
                              #endif
                                  private const int {|RH7102:K|} = 1;

                                  private static int Log(string name)
                                  {
                                      return 0;
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying that the members of every excluded branch of an <c>#if</c>/<c>#elif</c> block in front of the constant
    /// count
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH7102-006")]
    [Requirement("SHARED-ORDER-007")]
    public async Task NoCodeFixWhenAnInitializedFieldOfAnExcludedElifBranchInFrontOfTheConstantWouldCrossAnInitializedField(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private int _i = Log("i");
                              #if DEBUG
                                  private int _c = Log("c");
                              #elif TRACE
                                  private int _d = Log("d");
                              #endif
                                  private const int {|RH7102:K|} = 1;

                                  private static int Log(string name)
                                  {
                                      return 0;
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying that a crossed declaration whose modifiers an <c>#if</c> splits into <c>readonly</c> and <c>const</c>
    /// withholds the move of the constant: the group stands between two modifiers, at no boundary of whole units
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH7102-006")]
    [Requirement("SHARED-ORDER-008")]
    public async Task NoCodeFixWhenTheMoveCrossesADeclarationThatIsConstOnlyInSomeConfigurations(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private int _i;
                                  public
                              #if DEBUG
                                      readonly
                              #else
                                      const
                              #endif
                                      int J = 1;
                              #if DEBUG
                                  private int _c = Log("c");
                              #endif
                                  private const int {|RH7102:K|} = 1;

                                  private static int Log(string name)
                                  {
                                      return 0;
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying that a constant whose <c>const</c> modifier an <c>#if</c> splits, and that is a static initialized field
    /// in a build with <c>DEBUG</c>, is not reported
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH7102-001")]
    [Requirement("SHARED-DIAG-008")]
    public async Task ConstantWhoseModifiersAnIfSplitsIntoAStaticFieldIsNotReported(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private static int _s = Log("s");
                                  private
                              #if DEBUG
                                      static readonly
                              #else
                                      const
                              #endif
                                      int K = 1;

                                  private static int Log(string name)
                                  {
                                      return 0;
                                  }
                              }
                              """;

        await Verify(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying that a constant whose <c>const</c> modifier an <c>#if</c> splits, and that is an instance field of a
    /// struct in a build with <c>DEBUG</c>, is not reported
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH7102-001")]
    [Requirement("SHARED-DIAG-008")]
    public async Task ConstantWhoseModifiersAnIfSplitsIntoAnInstanceFieldIsNotReported(string endOfLine)
    {
        const string source = """
                              public struct S
                              {
                                  private int _x;
                                  private
                              #if DEBUG
                                      readonly
                              #else
                                      const
                              #endif
                                      int K = 1;
                              }
                              """;

        await Verify(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying that Fix All moves the constant whose fix is offered and leaves the constant whose move would reorder
    /// the initializers of a build with <c>DEBUG</c> reported without a fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7102-006")]
    [Requirement("SHARED-FIX-002")]
    public async Task FixAllSkipsTheConstantWhoseMoveWouldReorderTheInitializersOfAnotherConfiguration()
    {
        const string source = """
                              public class C
                              {
                                  private int _i = Log("i");
                              #if DEBUG
                                  private int _c = Log("c");
                              #endif
                                  private const int {|RH7102:K|} = 1;
                                  protected int _p;
                                  protected const int {|RH7102:L|} = 2;

                                  private static int Log(string name)
                                  {
                                      return 0;
                                  }
                              }
                              """;

        const string fixedSource = """
                                   public class C
                                   {
                                       private int _i = Log("i");
                                   #if DEBUG
                                       private int _c = Log("c");
                                   #endif
                                       private const int {|RH7102:K|} = 1;
                                       protected const int L = 2;
                                       protected int _p;

                                       private static int Log(string name)
                                       {
                                           return 0;
                                       }
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
    /// Verifying that a conditional group in front of a crossed field's first token whose branches hold its modifiers,
    /// which are neither whole members nor whole attribute lists, withholds the move of the constant
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH7102-006")]
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
                              #if DEBUG
                                  private static int _x = Log("x");
                              #endif
                                  private const int {|RH7102:K|} = 1;

                                  private static int Log(string name)
                                  {
                                      return 0;
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies that a comment the formatter refuses in another member of the type no longer withholds the fix, which moves
    /// the constant without formatting the type
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
                                  private int _field;
                                  private const int {|RH7102:Max|} = 1;

                                  private void M()
                                  {
                                      int /* c */ y = 1;
                                  }
                              }
                              """;

        const string fixedSource = """
                                   internal class C
                                   {
                                       private const int Max = 1;
                                       private int _field;

                                       private void M()
                                       {
                                           int /* c */ y = 1;
                                       }
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifying that a group in front of the moved and the crossed span whose excluded branch closes the type's body keeps
    /// the move of a constant, which reorders no instance field, and the fix moves it
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-008")]
    [Requirement("RH7102-006")]
    public async Task FixMovesAConstantBehindAGroupInFrontOfTheSpansThatClosesTheTypeAndOpensAStruct(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                              #if X
                              }

                              public struct D
                              {
                              #endif
                                  private const int Y = 0;
                                  private int _a;
                                  private const int {|RH7102:Max|} = 1;
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
                                       private const int Y = 0;
                                       private const int Max = 1;
                                       private int _a;
                                   }
                                   """;

        await Verify(WithEndOfLine(source, endOfLine), WithEndOfLine(fixedSource, endOfLine));
    }

    /// <summary>
    /// Verifying that the continuation line of a moved constant keeps its column when the constant moves into a slot
    /// indented as it is
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-FIX-007")]
    public async Task ContinuationLineOfAMovedConstantKeepsItsColumn(string endOfLine)
    {
        const string source = """
                              class C
                              {
                                  private int _a;
                                  private const int {|RH7102:D|} = 1
                                          + 2;
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                       private const int D = 1
                                               + 2;
                                       private int _a;
                                   }
                                   """;

        await VerifyCodeFixWithEndOfLine(source, fixedSource, endOfLine);
    }

    /// <summary>
    /// Verifying that no code fix is offered when the moved constant shares its line with the field in front of it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-ORDER-009")]
    [Requirement("RH7102-006")]
    public async Task NoCodeFixWhenTheMovedConstantSharesItsLineWithTheFieldInFrontOfIt()
    {
        const string source = """
                              class C
                              {
                                  private int _a; private const int {|RH7102:D|} = 1;
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifying that no code fix is offered when a constant indented deeper than its slot holds a verbatim string over
    /// two lines
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-ORDER-010")]
    [Requirement("RH7102-006")]
    public async Task NoCodeFixWhenAShiftedConstantHoldsAVerbatimStringOverTwoLines()
    {
        const string source = """
                              class C
                              {
                                  private int _a;
                                      private const string {|RH7102:D|} = @"x
                              y";
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
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
                                    private const int {|#0:MaxValue|} = 1;
                                    private const int {|#1:MinValue|} = 2;
                                }
                                """;

        const string fixedCode = """
                                 public class TestClass
                                 {
                                     private const int MaxValue = 1;
                                     private const int MinValue = 2;
                                     private int _value;
                                 }
                                 """;

        // Both constants are moved in front of the same mutable field, so each fix rewrites the span from that
        // field to its own declaration and the two text changes overlap. The batch keeps one of them per pass
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH7102ConstantsMustAppearBeforeFieldsAnalyzer.DiagnosticId, AnalyzerResources.RH7102MessageFormat, 2),
                                  static config => config.NumberOfFixAllIterations = 2);
    }

    #endregion // BatchCodeFixTestsBase
}