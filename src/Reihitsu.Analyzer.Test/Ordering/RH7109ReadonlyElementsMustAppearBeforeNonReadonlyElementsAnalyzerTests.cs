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
/// Test methods for <see cref="RH7109ReadonlyElementsMustAppearBeforeNonReadonlyElementsAnalyzer"/> and <see cref="RH7109ReadonlyElementsMustAppearBeforeNonReadonlyElementsCodeFixProvider"/>
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

        await Verify(testCode, fixedCode, Diagnostics(RH7109ReadonlyElementsMustAppearBeforeNonReadonlyElementsAnalyzer.DiagnosticId, AnalyzerResources.RH7109MessageFormat));
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

        await Verify(testCode, fixedCode, Diagnostics(RH7109ReadonlyElementsMustAppearBeforeNonReadonlyElementsAnalyzer.DiagnosticId, AnalyzerResources.RH7109MessageFormat));
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

        await Verify(testCode, fixedCode, Diagnostics(RH7109ReadonlyElementsMustAppearBeforeNonReadonlyElementsAnalyzer.DiagnosticId, AnalyzerResources.RH7109MessageFormat));
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

        await Verify(testCode, Diagnostics(RH7109ReadonlyElementsMustAppearBeforeNonReadonlyElementsAnalyzer.DiagnosticId, AnalyzerResources.RH7109MessageFormat));

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
    /// Characterizes a readonly field in a region that follows a mutable field outside every region: it is reported, and
    /// directive safety withholds the fix. This test pins today's behavior; the decided behavior is stated by
    /// SHARED-ORDER-004, which compares a member in a top-level region only with members of that region, and its change
    /// belongs to an existing follow-up
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task MixedAccessibilityInterfaceViolationWithDirectivesHasNoCodeFix()
    {
        const string source = """
                              public interface ITest
                              {
                                  static int Value;

                                  #region Readonly
                                  public static readonly int ReadonlyValue;
                                  #endregion
                              }
                              """;
        var testCode = source.Replace("ReadonlyValue", "{|#0:ReadonlyValue|}");

        await Verify(testCode, Diagnostics(RH7109ReadonlyElementsMustAppearBeforeNonReadonlyElementsAnalyzer.DiagnosticId, AnalyzerResources.RH7109MessageFormat));

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

        await Verify(testCode, Diagnostics(RH7109ReadonlyElementsMustAppearBeforeNonReadonlyElementsAnalyzer.DiagnosticId, AnalyzerResources.RH7109MessageFormat, 2));
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

                                    #region Readonly
                                    private readonly int _readonlyValue = 1;
                                    #endregion
                                }
                                """;

        var actions = await GetCodeFixActionsAsync(testCode,
                                                   RH7109ReadonlyElementsMustAppearBeforeNonReadonlyElementsAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<VariableDeclaratorSyntax>()
                                                               .Single(declarator => declarator.Identifier.ValueText == "_readonlyValue")
                                                               .Identifier
                                                               .GetLocation());

        Assert.IsEmpty(actions);
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

        await Verify(testCode, fixedCode, Diagnostics(RH7109ReadonlyElementsMustAppearBeforeNonReadonlyElementsAnalyzer.DiagnosticId, AnalyzerResources.RH7109MessageFormat));
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

        // Both readonly fields are moved in front of the same mutable field, so each fix rewrites the span from
        // that field to its own declaration and the two text changes overlap. The mutable field carries no
        // initializer, so neither move changes initializer execution order and both fixes stay available
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH7109ReadonlyElementsMustAppearBeforeNonReadonlyElementsAnalyzer.DiagnosticId, AnalyzerResources.RH7109MessageFormat, 2),
                                  static config => config.NumberOfFixAllIterations = 2);
    }

    #endregion // BatchCodeFixTestsBase
}