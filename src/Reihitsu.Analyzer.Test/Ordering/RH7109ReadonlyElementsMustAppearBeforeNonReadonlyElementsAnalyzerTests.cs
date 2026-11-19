using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Organization;
using Reihitsu.Analyzer.Rules.Organization;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Ordering;

/// <summary>
/// Test methods for <see cref="RH7109ReadonlyElementsMustAppearBeforeNonReadonlyElementsAnalyzer"/> and <see cref="RH7109ReadonlyElementsMustAppearBeforeNonReadonlyElementsCodeFixProvider"/>: what is reported, the layouts that withhold the move, and the lines the fix moves
/// </summary>
[TestClass]
public class RH7109ReadonlyElementsMustAppearBeforeNonReadonlyElementsAnalyzerTests : BatchCodeFixTestsBase<RH7109ReadonlyElementsMustAppearBeforeNonReadonlyElementsAnalyzer, RH7109ReadonlyElementsMustAppearBeforeNonReadonlyElementsCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying readonly fields are reported and fixed when they appear after mutable fields
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7109-001")]
    [Requirement("RH7109-004")]
    [Requirement("RH7109-005")]
    public async Task ReadonlyFieldsAreReportedAndFixedWhenTheyAppearAfterMutableFields()
    {
        const string testCode = """
                                public class TestClass
                                {
                                    private int _value;
                                    private readonly int {|#0:_readonlyValue|};
                                }
                                """;

        const string fixedCode = """
                                 public class TestClass
                                 {
                                     private readonly int _readonlyValue;
                                     private int _value;
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH7109ReadonlyElementsMustAppearBeforeNonReadonlyElementsAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifying the blank line that already separated the readonly field from the mutable field survives the
    /// reorder
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7109-005")]
    public async Task SeparatorSurvivesTheReorderWhenReadonlyFieldWasAlreadySeparated()
    {
        const string testCode = """
                                public class TestClass
                                {
                                    private int _value;

                                    private readonly int {|#0:_readonlyValue|};
                                }
                                """;

        const string fixedCode = """
                                 public class TestClass
                                 {
                                     private readonly int _readonlyValue;

                                     private int _value;
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH7109ReadonlyElementsMustAppearBeforeNonReadonlyElementsAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that an implicit-access mutable interface field and explicit-public readonly field share the same
    /// effective accessibility group and are safely reordered when neither has an initializer
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7109-001")]
    [Requirement("RH7109-005")]
    public async Task MixedImplicitAndExplicitPublicInterfaceFieldsAreDetectedAndFixedWhenSafe()
    {
        const string testCode = """
                                public interface ITest
                                {
                                    static int Value;
                                    public static readonly int {|#0:ReadonlyValue|};
                                }
                                """;
        const string fixedCode = """
                                 public interface ITest
                                 {
                                     public static readonly int ReadonlyValue;
                                     static int Value;
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH7109ReadonlyElementsMustAppearBeforeNonReadonlyElementsAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies the reverse accessibility spelling direction for initialized interface fields while the
    /// initializer-order safety guard withholds the code fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7109-001")]
    [Requirement("RH7109-006")]
    public async Task ExplicitPublicMutableAndImplicitReadonlyInterfaceFieldsReportWithoutCodeFixWhenInitialized()
    {
        const string source = """
                              public interface ITest
                              {
                                  public static int Value = 0;
                                  static readonly int ReadonlyValue = 1;
                              }
                              """;
        var testCode = source.Replace("ReadonlyValue", "{|#0:ReadonlyValue|}");

        await Verify(testCode, Diagnostics(RH7109ReadonlyElementsMustAppearBeforeNonReadonlyElementsAnalyzer.DiagnosticId, MessageFormat));

        var actions = await GetCodeFixActionsAsync(source,
                                                   RH7109ReadonlyElementsMustAppearBeforeNonReadonlyElementsAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<VariableDeclaratorSyntax>()
                                                               .Single(declarator => declarator.Identifier.ValueText == "ReadonlyValue")
                                                               .Identifier
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifies that an explicit-public readonly interface field ordered before an implicit-access mutable field does
    /// not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task OrderedReadonlyInterfaceFieldsDoNotProduceDiagnostics()
    {
        const string testCode = """
                                public interface ITest
                                {
                                    public static readonly int ReadonlyValue = 1;
                                    static int Value = 0;
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies the reverse accessibility spelling direction when an implicit-access readonly interface field is
    /// already ordered before an explicit-public mutable field
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ImplicitReadonlyBeforeExplicitPublicMutableInterfaceFieldDoesNotProduceDiagnostics()
    {
        const string testCode = """
                                public interface ITest
                                {
                                    static readonly int ReadonlyValue = 1;
                                    public static int Value = 0;
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that interface constants do not participate in readonly field ordering
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task InterfaceConstantsDoNotProduceDiagnostics()
    {
        const string testCode = """
                                public interface ITest
                                {
                                    const int Constant = 1;
                                    static readonly int ReadonlyValue = 2;
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that readonly and mutable interface fields in different accessibility groups do not conflict
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7109-002")]
    public async Task InterfaceFieldsInDifferentAccessibilityGroupsDoNotProduceDiagnostics()
    {
        const string testCode = """
                                public interface ITest
                                {
                                    public static int Value = 0;
                                    private static readonly int ReadonlyValue = 1;
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that implicit class accessibility and explicit private accessibility remain distinct syntactic groups
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ImplicitAndPrivateClassFieldsDoNotProduceDiagnostics()
    {
        const string testCode = """
                                internal class TestClass
                                {
                                    static int Value;
                                    private static readonly int ReadonlyValue;
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that a readonly interface field in a region that follows a mutable field outside every region is not
    /// compared with it, so nothing is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7109-003")]
    [Requirement("SHARED-ORDER-004")]
    public async Task ReadonlyInterfaceFieldInARegionAfterAMutableFieldOutsideEveryRegionIsNotReported()
    {
        const string testCode = """
                                public interface ITest
                                {
                                    static int Value;

                                    #region Readonly
                                    public static readonly int ReadonlyValue;
                                    #endregion
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that every same-group readonly interface field following a mutable field is reported at its first
    /// variable identifier
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7109-001")]
    [Requirement("RH7109-004")]
    public async Task MultipleMisorderedReadonlyInterfaceFieldsProduceDiagnostics()
    {
        const string testCode = """
                                public interface ITest
                                {
                                    public static int Value = 0;
                                    public static readonly int {|#0:FirstReadonlyValue|} = 1;
                                    public static readonly int {|#1:SecondReadonlyValue|} = 2;
                                }
                                """;

        await Verify(testCode, Diagnostics(RH7109ReadonlyElementsMustAppearBeforeNonReadonlyElementsAnalyzer.DiagnosticId, MessageFormat, 2));
    }

    /// <summary>
    /// Verifying no code fix is offered when moving the readonly field would change initializer execution order
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7109-006")]
    public async Task NoCodeFixWhenMoveChangesInitializerExecutionOrder()
    {
        const string testCode = """
                                public class TestClass
                                {
                                    private static int _a = F();
                                    private static readonly int _b = _a * 2;

                                    private static int F() => 21;
                                }
                                """;

        var actions = await GetCodeFixActionsAsync(testCode,
                                                   RH7109ReadonlyElementsMustAppearBeforeNonReadonlyElementsAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<VariableDeclaratorSyntax>()
                                                               .Single(declarator => declarator.Identifier.ValueText == "_b")
                                                               .Identifier
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifying no code fix is offered when moving the readonly field would jump over an event field initializer
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7109-006")]
    public async Task NoCodeFixWhenMoveChangesInitializerExecutionOrderAcrossEventField()
    {
        const string testCode = """
                                public class TestClass
                                {
                                    private static int _a;
                                    public static event System.EventHandler E = Handler;
                                    private static readonly int _b = 1;

                                    private static System.EventHandler Handler => null;
                                }
                                """;

        var actions = await GetCodeFixActionsAsync(testCode,
                                                   RH7109ReadonlyElementsMustAppearBeforeNonReadonlyElementsAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<VariableDeclaratorSyntax>()
                                                               .Single(declarator => declarator.Identifier.ValueText == "_b")
                                                               .Identifier
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifies that a field outside every region and a readonly field in a region that follows it are not compared, so
    /// nothing is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7109-003")]
    public async Task FieldOutsideEveryRegionAndReadonlyFieldInARegionAreNotCompared()
    {
        const string testCode = """
                                public class TestClass
                                {
                                    private int _value;

                                    #region Readonly
                                    private readonly int _readonlyValue = 1;
                                    #endregion
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying the code fix is still offered when the moved readonly field carries an initializer but no passed-over field does
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7109-005")]
    public async Task ReadonlyFieldWithInitializerIsFixedWhenNoPassedOverFieldHasInitializer()
    {
        const string testCode = """
                                public class TestClass
                                {
                                    private int _value;
                                    private readonly int {|#0:_readonlyValue|} = 1;
                                }
                                """;

        const string fixedCode = """
                                 public class TestClass
                                 {
                                     private readonly int _readonlyValue = 1;
                                     private int _value;
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH7109ReadonlyElementsMustAppearBeforeNonReadonlyElementsAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifying the fix reorders the fields of a struct with explicit layout, where each field's offset attribute
    /// moves with it, so the layout stays the same
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-003")]
    public async Task FixReordersTheFieldsOfAnExplicitLayoutStructAndKeepsEachOffset()
    {
        const string testCode = """
                                using System.Runtime.InteropServices;

                                [StructLayout(LayoutKind.Explicit)]
                                public struct S
                                {
                                    [FieldOffset(0)]
                                    public int _a;
                                    [FieldOffset(8)]
                                    public readonly long {|RH7109:_b|};
                                }
                                """;

        const string fixedCode = """
                                 using System.Runtime.InteropServices;

                                 [StructLayout(LayoutKind.Explicit)]
                                 public struct S
                                 {
                                     [FieldOffset(8)]
                                     public readonly long _b;
                                     [FieldOffset(0)]
                                     public int _a;
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifying the fix reorders the fields of a struct with automatic layout, whose field order sets no layout
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-003")]
    public async Task FixReordersTheFieldsOfAnAutoLayoutStruct()
    {
        const string testCode = """
                                using System.Runtime.InteropServices;

                                [StructLayout(LayoutKind.Auto)]
                                public struct S
                                {
                                    public int _a;
                                    public readonly long {|RH7109:_b|};
                                }
                                """;

        const string fixedCode = """
                                 using System.Runtime.InteropServices;

                                 [StructLayout(LayoutKind.Auto)]
                                 public struct S
                                 {
                                     public readonly long _b;
                                     public int _a;
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifying the fix reorders the fields of a class without a layout attribute, whose fields the runtime lays out
    /// in an order of its own
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-003")]
    public async Task FixReordersTheFieldsOfAClassWithoutLayoutAttribute()
    {
        const string testCode = """
                                public class C
                                {
                                    public int _a;
                                    public readonly long {|RH7109:_b|};
                                }
                                """;

        const string fixedCode = """
                                 public class C
                                 {
                                     public readonly long _b;
                                     public int _a;
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifying the specification example: a readonly field after a field of its accessibility and static group is
    /// reported on its first name, in a class and in an interface
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7109-001")]
    [Requirement("RH7109-004")]
    public async Task ReadonlyFieldsAfterFieldsOfTheirGroupAreReported()
    {
        const string source = """
                              class C
                              {
                                  private static int _shared;
                                  private int _count;
                                  public int Value;
                                  private readonly int {|RH7109:_id|};
                                  private static readonly int {|RH7109:_default|};
                                  public readonly int {|RH7109:Key|};
                                  private const int Max = 1;
                              }

                              interface I
                              {
                                  static int Counter;
                                  public static readonly int {|RH7109:Limit|};
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifying the specification example: readonly fields that follow only fields of other groups or constants are
    /// not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7109-002")]
    public async Task ReadonlyFieldsAfterFieldsOfOtherGroupsAreNotReported()
    {
        const string source = """
                              class C
                              {
                                  private int _count;
                                  private static readonly int _default = 1;
                                  private const int Max = 1;
                                  public readonly int Key;
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifying the specification example: fields in one top-level region are compared, and no code fix is offered
    /// because the region opens in the crossed span
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7109-003")]
    [Requirement("RH7109-006")]
    public async Task ReadonlyFieldInTheSameTopLevelRegionIsReported()
    {
        const string source = """
                              class C
                              {
                                  #region Fields

                                  private int _count;
                                  private int _other;
                                  private readonly int {|RH7109:_id|};

                                  #endregion
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifying the specification example: the fix moves the readonly field in front of the first field of its group
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7109-005")]
    [Requirement("SHARED-ORDER-001")]
    public async Task FixMovesTheReadonlyFieldInFrontOfTheFirstFieldOfItsGroup()
    {
        const string source = """
                              class C
                              {
                                  private int _count;
                                  private readonly int {|RH7109:_id|} = 2;
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                       private readonly int _id = 2;
                                       private int _count;
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifying the specification example: the documentation comment travels with the moved field, and the blank
    /// line stays where it was
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7109-005")]
    [Requirement("SHARED-ORDER-002")]
    public async Task FixMovesTheDocumentationCommentWithTheReadonlyField()
    {
        const string source = """
                              class C
                              {
                                  private int _count;

                                  /// <summary>
                                  /// The identifier
                                  /// </summary>
                                  private readonly int {|RH7109:_id|};
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                       /// <summary>
                                       /// The identifier
                                       /// </summary>
                                       private readonly int _id;

                                       private int _count;
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifying the specification example: when both fields have an instance initializer the fix is withheld, and
    /// the diagnostic is still reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7109-006")]
    [Requirement("SHARED-ORDER-007")]
    public async Task InitializersOfBothFieldsWithholdTheFix()
    {
        const string source = """
                              class C
                              {
                                  private int _count = 1;
                                  private readonly int {|RH7109:_id|} = 2;
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifying that a pragma pair around a crossed field withholds the fix, and the diagnostic is still reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7109-006")]
    public async Task PragmaDirectivesAroundACrossedFieldWithholdTheFix()
    {
        const string source = """
                              class C
                              {
                                  private int _count;
                              #pragma warning disable CS0169
                                  private int _unused;
                              #pragma warning restore CS0169
                                  private readonly int {|RH7109:_id|};
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifying that the fix is withheld for the instance fields of a struct without a layout attribute, whose fields
    /// the runtime lays out sequentially, and the diagnostic is still reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-003")]
    [Requirement("RH7109-006")]
    public async Task NoCodeFixForTheFieldsOfAStructWithDefaultLayout()
    {
        const string source = """
                              struct S
                              {
                                  public int A;
                                  public readonly int {|RH7109:B|};
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifying that a readonly field in a region nested in the top-level region of an earlier field of its group is
    /// reported, and no code fix is offered because the regions open in the crossed span
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7109-003")]
    [Requirement("RH7109-006")]
    public async Task ReadonlyFieldInANestedRegionOfTheSameTopLevelRegionIsReported()
    {
        const string source = """
                              class C
                              {
                                  #region Fields

                                  private int _count;

                                  #region Identity

                                  private readonly int {|RH7109:_id|};

                                  #endregion

                                  #endregion
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifying that the fix moves the readonly field in front of the first of several earlier fields of its group,
    /// across a field of another group
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7109-005")]
    public async Task FixMovesTheReadonlyFieldInFrontOfTheFirstOfSeveralFieldsOfItsGroup()
    {
        const string source = """
                              class C
                              {
                                  private int _a;
                                  public int B;
                                  private int _c;
                                  private readonly int {|RH7109:_d|};
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                       private readonly int _d;
                                       private int _a;
                                       public int B;
                                       private int _c;
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifying that <c>protected internal</c> and <c>private protected</c> are each one group regardless of the
    /// order of their keywords
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7109-001")]
    public async Task CompoundAccessibilitiesAreOneGroupRegardlessOfKeywordOrder()
    {
        const string source = """
                              class C
                              {
                                  internal protected int _a;
                                  protected internal readonly int {|RH7109:_b|};
                                  protected private int _c;
                                  private protected readonly int {|RH7109:_d|};
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifying the shared specification example: the fields of a public struct without a layout attribute are reported,
    /// and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-003")]
    [Requirement("RH7109-006")]
    public async Task NoCodeFixForTheSpecificationExampleStruct()
    {
        const string source = """
                              public struct S
                              {
                                  public int _a;
                                  public readonly int {|RH7109:_b|};
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifying that the fix is withheld for the instance fields of a record struct
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-003")]
    [Requirement("RH7109-006")]
    public async Task NoCodeFixForTheFieldsOfARecordStruct()
    {
        const string source = """
                              public record struct S
                              {
                                  public int _a;
                                  public readonly int {|RH7109:_b|};
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifying that the fix is withheld for the instance fields of a ref struct
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-003")]
    [Requirement("RH7109-006")]
    public async Task NoCodeFixForTheFieldsOfARefStruct()
    {
        const string source = """
                              public ref struct S
                              {
                                  public int _a;
                                  public readonly int {|RH7109:_b|};
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifying that the fix is withheld for the instance fields of a class declared with sequential layout
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-003")]
    [Requirement("RH7109-006")]
    public async Task NoCodeFixForTheFieldsOfAClassWithSequentialLayout()
    {
        const string source = """
                              using System.Runtime.InteropServices;

                              [StructLayout(LayoutKind.Sequential)]
                              public class C
                              {
                                  public int _a;
                                  public readonly int {|RH7109:_b|};
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifying that the fix is withheld for the instance fields of a record declared with sequential layout
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-003")]
    [Requirement("RH7109-006")]
    public async Task NoCodeFixForTheFieldsOfARecordWithSequentialLayout()
    {
        const string source = """
                              using System.Runtime.InteropServices;

                              [StructLayout(LayoutKind.Sequential)]
                              public record R
                              {
                                  public int _a;
                                  public readonly int {|RH7109:_b|};
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifying that the fix is withheld for a class whose sequential layout attribute is written with its full name and
    /// a named argument
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-003")]
    [Requirement("RH7109-006")]
    public async Task NoCodeFixForAClassWithAFullyQualifiedSequentialLayoutAttribute()
    {
        const string source = """
                              [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 1)]
                              public class C
                              {
                                  public int _a;
                                  public readonly int {|RH7109:_b|};
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifying that the fix is withheld for the instance fields of a struct nested in a class
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-003")]
    [Requirement("RH7109-006")]
    public async Task NoCodeFixForTheFieldsOfAStructNestedInAClass()
    {
        const string source = """
                              public class C
                              {
                                  public struct S
                                  {
                                      public int _a;
                                      public readonly int {|RH7109:_b|};
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifying that the fix is withheld for the instance fields of one part of a partial struct
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-003")]
    [Requirement("RH7109-006")]
    public async Task NoCodeFixForTheFieldsOfAPartOfAPartialStruct()
    {
        const string source = """
                              public partial struct S
                              {
                                  public int _a;
                                  public readonly int {|RH7109:_b|};
                              }

                              public partial struct S
                              {
                                  public void M()
                                  {
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifying the shared specification example: static fields are no part of a struct's layout, so the fix moves them
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-003")]
    public async Task FixReordersTheStaticFieldsOfAStruct()
    {
        const string source = """
                              public struct S
                              {
                                  public static int _a;
                                  public static readonly int {|RH7109:_b|};
                              }
                              """;

        const string fixedSource = """
                                   public struct S
                                   {
                                       public static readonly int _b;
                                       public static int _a;
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifying that the fix is withheld for the instance fields of a partial class whose sequential layout another part
    /// declares
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-003")]
    [Requirement("RH7109-006")]
    public async Task NoCodeFixForAPartialClassWithSequentialLayoutOnAnotherPart()
    {
        const string source = """
                              using System.Runtime.InteropServices;

                              [StructLayout(LayoutKind.Sequential)]
                              public partial class P
                              {
                              }

                              public partial class P
                              {
                                  public int _a;
                                  public readonly int {|RH7109:_b|};
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifying that the fix is withheld for the instance fields of a partial record whose sequential layout another part
    /// declares
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-003")]
    [Requirement("RH7109-006")]
    public async Task NoCodeFixForAPartialRecordWithSequentialLayoutOnAnotherPart()
    {
        const string source = """
                              using System.Runtime.InteropServices;

                              [StructLayout(LayoutKind.Sequential)]
                              public partial record R;

                              public partial record R
                              {
                                  public int _a;
                                  public readonly int {|RH7109:_b|};
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifying the fix reorders the fields of a partial struct whose automatic layout another part declares
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-003")]
    public async Task FixReordersTheFieldsOfAPartialStructWithAutoLayoutOnAnotherPart()
    {
        const string testCode = """
                                using System.Runtime.InteropServices;

                                [StructLayout(LayoutKind.Auto)]
                                public partial struct S
                                {
                                }

                                public partial struct S
                                {
                                    public int _a;
                                    public readonly long {|RH7109:_b|};
                                }
                                """;

        const string fixedCode = """
                                 using System.Runtime.InteropServices;

                                 [StructLayout(LayoutKind.Auto)]
                                 public partial struct S
                                 {
                                 }

                                 public partial struct S
                                 {
                                     public readonly long _b;
                                     public int _a;
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifying the fix reorders the fields of a partial struct whose explicit layout another part declares, where each
    /// field's offset attribute moves with it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-003")]
    public async Task FixReordersTheFieldsOfAPartialStructWithExplicitLayoutOnAnotherPart()
    {
        const string testCode = """
                                using System.Runtime.InteropServices;

                                [StructLayout(LayoutKind.Explicit)]
                                public partial struct S
                                {
                                }

                                public partial struct S
                                {
                                    [FieldOffset(0)]
                                    public int _a;
                                    [FieldOffset(8)]
                                    public readonly long {|RH7109:_b|};
                                }
                                """;

        const string fixedCode = """
                                 using System.Runtime.InteropServices;

                                 [StructLayout(LayoutKind.Explicit)]
                                 public partial struct S
                                 {
                                 }

                                 public partial struct S
                                 {
                                     [FieldOffset(8)]
                                     public readonly long _b;
                                     [FieldOffset(0)]
                                     public int _a;
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifying the fix reorders the fields of a struct whose automatic layout is written as a named argument
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-003")]
    public async Task FixReordersTheFieldsOfAStructWithAutoLayoutWrittenAsANamedArgument()
    {
        const string testCode = """
                                using System.Runtime.InteropServices;

                                [StructLayout(layoutKind: LayoutKind.Auto)]
                                public struct S
                                {
                                    public int _a;
                                    public readonly long {|RH7109:_b|};
                                }
                                """;

        const string fixedCode = """
                                 using System.Runtime.InteropServices;

                                 [StructLayout(layoutKind: LayoutKind.Auto)]
                                 public struct S
                                 {
                                     public readonly long _b;
                                     public int _a;
                                 }
                                 """;

        await Verify(testCode, fixedCode);
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