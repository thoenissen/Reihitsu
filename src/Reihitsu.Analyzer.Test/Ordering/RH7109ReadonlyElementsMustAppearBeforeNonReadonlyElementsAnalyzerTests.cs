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
    /// Verifying the shared specification example: without <c>DEBUG</c> defined, the moved field has an initializer only
    /// in a build with <c>DEBUG</c>, and moving it in front of an initialized field would run the initializers of that
    /// build in another order, so no code fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-007")]
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
    /// Verifying that a crossed field which has an initializer only in a build with <c>DEBUG</c> counts as initialized,
    /// so moving an initialized field across it is withheld
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-007")]
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
    /// Verifying that a property initializer which only a build with <c>DEBUG</c> compiles behind the accessor list of a
    /// crossed property counts, although the excluded text does not form a member on its own
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-007")]
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
    /// Verifying that a crossed field whose <c>static</c> modifier only a build with <c>DEBUG</c> compiles, in a conditional
    /// group that opens in front of its first token, counts as a static initialized field of that build, so moving a static
    /// initialized field across it is withheld
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-007")]
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
    /// Verifying that a crossed field whose <c>static</c> modifier an <c>#if DEBUG</c> group in front of its first token adds
    /// counts as a static initialized field of that build, so moving a static initialized field across it is withheld
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-007")]
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
    /// Verifying the counterpart with the <c>#if</c> behind the crossed field's first modifier: the field is static in a
    /// build with <c>DEBUG</c>, so moving a static initialized field across it is withheld
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-007")]
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
    /// Verifying that a crossed declaration which a group in front of its first token makes a constant in the active code
    /// and a static readonly field in a build with <c>DEBUG</c> counts as a static initialized field of that build, so
    /// moving a static initialized field that reads it across it is withheld
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-007")]
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
    /// Verifying that a moved field whose <c>static</c> modifier only a build with <c>DEBUG</c> compiles, in a conditional
    /// group that opens in front of its first token, counts as a static initialized field of that build, so moving it
    /// across a static initialized field is withheld
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-007")]
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
    /// Verifying that a named attribute argument in excluded code in front of the moved field is no initializer: the field
    /// has none in any build, so the fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-007")]
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
    /// Verifying that a named attribute argument in excluded code behind an active attribute of the moved field is no
    /// initializer: the field has none in any build, so the fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-007")]
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
    /// Verifying the counterpart without a named argument: an excluded attribute in front of the moved field is no
    /// initializer, so the fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-007")]
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
    /// Verifying that a crossed field whose <c>static</c> modifier only an excluded branch gives it counts as a static
    /// initialized field of that build, also when the branch opens inside the initializer of the previous field and the
    /// group closes in front of the crossed field's first token, so moving a static initialized field across it is withheld
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-007")]
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
    /// Verifying that a crossed field whose <c>static</c> modifier only an excluded branch gives it counts as a static
    /// initialized field of that build, also when every branch writes the previous field's terminator and the group closes
    /// in front of the crossed field's first token, so moving a static initialized field across it is withheld
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-007")]
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
    /// Verifying the counterpart of a group that opens behind the previous declarator: no branch gives the crossed field
    /// <c>static</c>, so it is an instance field in every build and the static field's move keeps its fix
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-007")]
    [Requirement("RH7109-006")]
    public async Task FixMovesAFieldAcrossAFieldThatNoBranchOpeningBehindThePreviousDeclaratorMakesStatic(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private static int _t;
                                  private int _a
                              #if DEBUG
                                  ;
                                  private
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

        const string fixedSource = """
                                   public class C
                                   {
                                       private static readonly int _c = Log("c");
                                       private static int _t;
                                       private int _a
                                   #if DEBUG
                                       ;
                                       private
                                   #else
                                       ;
                                   #endif
                                       int _b = Log("b");

                                       private static int Log(string name)
                                       {
                                           return 0;
                                       }
                                   }
                                   """;

        await Verify(WithEndOfLine(source, endOfLine), WithEndOfLine(fixedSource, endOfLine));
    }

    /// <summary>
    /// Verifying that the excluded <c>#else</c> branch of a group that opens behind the previous declarator gives the
    /// crossed field its <c>static</c> modifier in that build, so moving a static initialized field across it is withheld
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-007")]
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
    /// Verifying the counterpart of an excluded <c>#else</c> branch behind the previous declarator that writes no
    /// <c>static</c>: the crossed field is an instance field in every build, so the fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-007")]
    [Requirement("RH7109-006")]
    public async Task FixMovesAFieldAcrossAFieldThatNoElseBranchOpeningBehindThePreviousDeclaratorMakesStatic(string endOfLine)
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

        const string fixedSource = """
                                   public class C
                                   {
                                       private static readonly int _c = Log("c");
                                       private static int _t;
                                       private int _a
                                   #if !DEBUG
                                       ;
                                   #else
                                       ;
                                       private
                                   #endif
                                       int _b = Log("b");

                                       private static int Log(string name)
                                       {
                                           return 0;
                                       }
                                   }
                                   """;

        await Verify(WithEndOfLine(source, endOfLine), WithEndOfLine(fixedSource, endOfLine));
    }

    /// <summary>
    /// Verifying that every excluded branch of a group is read: only the excluded <c>#elif</c> branch gives the crossed
    /// field <c>static</c>, so moving a static initialized field across it is withheld
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-007")]
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
    /// Verifying that a crossed field whose group opens behind the previous declarator and closes behind its own
    /// modifiers counts as static in the build whose branch writes <c>static</c>, so moving a static initialized field
    /// across it is withheld
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-007")]
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
    /// Verifying that a crossed field whose group opens inside the initializer of the previous field and closes behind its
    /// own modifiers counts as static in the build whose branch writes <c>static</c>, so moving a static initialized field
    /// across it is withheld
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-007")]
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
    /// Verifying the counterpart of a group that spans the previous declarator and the crossed field's modifiers with no
    /// branch writing <c>static</c>: the crossed field is an instance field in every build, so the fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-007")]
    [Requirement("RH7109-006")]
    public async Task FixMovesAFieldAcrossAFieldThatNoBranchSpanningThePreviousDeclaratorMakesStatic(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private static int _t;
                                  private int _a
                              #if DEBUG
                                  ;
                                  internal
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

        const string fixedSource = """
                                   public class C
                                   {
                                       private static readonly int _c = Log("c");
                                       private static int _t;
                                       private int _a
                                   #if DEBUG
                                       ;
                                       internal
                                   #else
                                       ;
                                       private
                                   #endif
                                       int _b = Log("b");

                                       private static int Log(string name)
                                       {
                                           return 0;
                                       }
                                   }
                                   """;

        await Verify(WithEndOfLine(source, endOfLine), WithEndOfLine(fixedSource, endOfLine));
    }

    /// <summary>
    /// Verifying that a crossed field whose every branch, spanning the previous declarator and its modifiers, writes
    /// <c>static</c> is static in every build, so moving an instance initialized field across it keeps its fix
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-007")]
    [Requirement("RH7109-006")]
    public async Task FixMovesAnInstanceFieldAcrossAFieldThatEveryBranchSpanningThePreviousDeclaratorMakesStatic(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private int _t;
                                  private static int _a
                              #if DEBUG
                                  ;
                                  internal static
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

        const string fixedSource = """
                                   public class C
                                   {
                                       private readonly int _c = Log("c");
                                       private int _t;
                                       private static int _a
                                   #if DEBUG
                                       ;
                                       internal static
                                   #else
                                       ;
                                       private static
                                   #endif
                                       int _b = Log("b");

                                       private static int Log(string name)
                                       {
                                           return 0;
                                       }
                                   }
                                   """;

        await Verify(WithEndOfLine(source, endOfLine), WithEndOfLine(fixedSource, endOfLine));
    }

    /// <summary>
    /// Verifying the counterpart where the excluded branch spanning the previous declarator lacks <c>static</c>: the
    /// crossed field is an instance initialized field of that build, so moving an instance initialized field across it is
    /// withheld
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-007")]
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
    /// Verifying that an excluded branch behind the previous declarator that writes no <c>static</c> does not make the
    /// crossed field an instance field: its <c>static</c> stands behind the group in every build, so the fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-007")]
    [Requirement("RH7109-006")]
    public async Task FixMovesAnInstanceFieldAcrossAFieldThatIsStaticBehindAGroupThatOpensBehindThePreviousDeclarator(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private int _t;
                                  private static int _a
                              #if DEBUG
                                  ;
                                  private
                              #else
                                  ;
                              #endif
                                  static int _b = Log("b");
                                  private readonly int {|RH7109:_c|} = Log("c");

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
                                       private static int _a
                                   #if DEBUG
                                       ;
                                       private
                                   #else
                                       ;
                                   #endif
                                       static int _b = Log("b");

                                       private static int Log(string name)
                                       {
                                           return 0;
                                       }
                                   }
                                   """;

        await Verify(WithEndOfLine(source, endOfLine), WithEndOfLine(fixedSource, endOfLine));
    }

    /// <summary>
    /// Verifying that a complete group in front of the crossed field's first token that adds another modifier leaves the
    /// field static in every build, so moving an instance initialized field across it keeps its fix
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-007")]
    [Requirement("RH7109-006")]
    public async Task FixMovesAnInstanceFieldAcrossAStaticFieldWithAnExcludedGroupInFrontOfItsFirstToken(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private int _a;
                              #if DEBUG
                                  volatile
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
                                   public class C
                                   {
                                       private readonly int _c = Log("c");
                                       private int _a;
                                   #if DEBUG
                                       volatile
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
    /// Verifying that a crossed field whose every branch in front of its first token writes <c>static</c>, while only one
    /// writes <c>readonly</c>, is static in every build, so moving an instance initialized field across it keeps its fix
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-007")]
    [Requirement("RH7109-006")]
    public async Task FixMovesAnInstanceFieldAcrossAFieldThatIsStaticInEveryBranchWithAReadonlyModifierOnlyInOne(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private int _a;
                              #if DEBUG
                                  private static readonly
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

        const string fixedSource = """
                                   public class C
                                   {
                                       private readonly int _c = Log("c");
                                       private int _a;
                                   #if DEBUG
                                       private static readonly
                                   #else
                                       private static
                                   #endif
                                       int _b = Log("b");

                                       private static int Log(string name)
                                       {
                                           return 0;
                                       }
                                   }
                                   """;

        await Verify(WithEndOfLine(source, endOfLine), WithEndOfLine(fixedSource, endOfLine));
    }

    /// <summary>
    /// Verifying that a crossed field whose every branch in front of its first token writes <c>static</c>, with another
    /// accessibility in each, is static in every build, so moving an instance initialized field across it keeps its fix
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-007")]
    [Requirement("RH7109-006")]
    public async Task FixMovesAnInstanceFieldAcrossAFieldThatIsStaticInEveryBranchWithADifferentAccessibility(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private int _a;
                              #if DEBUG
                                  internal static
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

        const string fixedSource = """
                                   public class C
                                   {
                                       private readonly int _c = Log("c");
                                       private int _a;
                                   #if DEBUG
                                       internal static
                                   #else
                                       private static
                                   #endif
                                       int _b = Log("b");

                                       private static int Log(string name)
                                       {
                                           return 0;
                                       }
                                   }
                                   """;

        await Verify(WithEndOfLine(source, endOfLine), WithEndOfLine(fixedSource, endOfLine));
    }

    /// <summary>
    /// Verifying the counterpart where the excluded branch in front of the crossed field's first token lacks
    /// <c>static</c>: the field is an instance initialized field of that build, so moving an instance initialized field
    /// across it is withheld
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-007")]
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
    /// Verifying that a crossed declaration whose every branch writes <c>const</c>, with another accessibility in each,
    /// has no runtime initializer in any build, so moving an instance initialized field across it keeps its fix
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-007")]
    [Requirement("RH7109-006")]
    public async Task FixMovesAnInstanceFieldAcrossADeclarationThatIsAConstantInEveryBranch(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private int _a;
                              #if DEBUG
                                  internal const
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

        const string fixedSource = """
                                   public class C
                                   {
                                       private readonly int _c = Log("c");
                                       private int _a;
                                   #if DEBUG
                                       internal const
                                   #else
                                       private const
                                   #endif
                                       int K = 1;

                                       private static int Log(string name)
                                       {
                                           return 0;
                                       }
                                   }
                                   """;

        await Verify(WithEndOfLine(source, endOfLine), WithEndOfLine(fixedSource, endOfLine));
    }

    /// <summary>
    /// Verifying the counterpart where the excluded branch writes <c>readonly</c> instead of <c>const</c>: the crossed
    /// declaration is an instance initialized field of that build, so moving an instance initialized field across it is
    /// withheld
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-007")]
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
    /// Verifying that a crossed field whose every branch behind its first modifier writes <c>static</c> is static in every
    /// build, so moving an instance initialized field across it keeps its fix
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-007")]
    [Requirement("RH7109-006")]
    public async Task FixMovesAnInstanceFieldAcrossAFieldThatIsStaticInEveryBranchBehindItsFirstModifier(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private int _a;
                                  private
                              #if DEBUG
                                  static readonly
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

        const string fixedSource = """
                                   public class C
                                   {
                                       private readonly int _c = Log("c");
                                       private int _a;
                                       private
                                   #if DEBUG
                                       static readonly
                                   #else
                                       static
                                   #endif
                                       int _b = Log("b");

                                       private static int Log(string name)
                                       {
                                           return 0;
                                       }
                                   }
                                   """;

        await Verify(WithEndOfLine(source, endOfLine), WithEndOfLine(fixedSource, endOfLine));
    }

    /// <summary>
    /// Verifying the counterpart where the excluded branch behind the crossed field's first modifier lacks <c>static</c>:
    /// the field is an instance initialized field of that build, so moving an instance initialized field across it is
    /// withheld
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-007")]
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
    /// Verifying that an excluded branch of a group nested in an active branch behind the crossed field's first modifier
    /// gives the field <c>static</c> in that build, so moving a static initialized field across it is withheld
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-007")]
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
    /// Verifying that a crossed field whose <c>#if</c>, <c>#elif</c>, and <c>#else</c> branches all write <c>static</c> is
    /// static in every build, so moving an instance initialized field across it keeps its fix
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-007")]
    [Requirement("RH7109-006")]
    public async Task FixMovesAnInstanceFieldAcrossAFieldThatIsStaticInEveryBranchOfAnElifGroup(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private int _a;
                              #if DEBUG
                                  private static readonly
                              #elif TRACE
                                  internal static
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

        const string fixedSource = """
                                   public class C
                                   {
                                       private readonly int _c = Log("c");
                                       private int _a;
                                   #if DEBUG
                                       private static readonly
                                   #elif TRACE
                                       internal static
                                   #else
                                       private static
                                   #endif
                                       int _b = Log("b");

                                       private static int Log(string name)
                                       {
                                           return 0;
                                       }
                                   }
                                   """;

        await Verify(WithEndOfLine(source, endOfLine), WithEndOfLine(fixedSource, endOfLine));
    }

    /// <summary>
    /// Verifying the counterpart where only the excluded <c>#elif</c> branch lacks <c>static</c>: the crossed field is an
    /// instance initialized field of that build, so moving an instance initialized field across it is withheld
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-007")]
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
    /// Verifying that a moved field whose every branch in front of its first token writes <c>static</c> is static in every
    /// build, so moving it across an instance initialized field keeps its fix
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-007")]
    [Requirement("RH7109-006")]
    public async Task FixMovesAFieldThatIsStaticInEveryBranchAcrossAnInstanceInitializedField(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private static int _a;
                                  private int _s = Log("s");
                              #if DEBUG
                                  internal static readonly
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

        const string fixedSource = """
                                   public class C
                                   {
                                   #if DEBUG
                                       internal static readonly
                                   #else
                                       private static readonly
                                   #endif
                                       int _c = Log("c");
                                       private static int _a;
                                       private int _s = Log("s");

                                       private static int Log(string name)
                                       {
                                           return 0;
                                       }
                                   }
                                   """;

        await Verify(WithEndOfLine(source, endOfLine), WithEndOfLine(fixedSource, endOfLine));
    }

    /// <summary>
    /// Verifying the counterpart where the excluded branch in front of the moved field's first token lacks <c>static</c>:
    /// the moved field is an instance initialized field of that build, so moving it across an instance initialized field
    /// is withheld
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-007")]
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
    /// Verifying that a <c>static</c> lambda that an excluded branch writes as a crossed field's initializer value is no
    /// <c>static</c> modifier: the field is an instance field in every build, so moving a static initialized field across it
    /// is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-007")]
    [Requirement("RH7109-006")]
    public async Task FixIsOfferedWhenAnExcludedInitializerOfACrossedFieldIsAStaticLambda(string endOfLine)
    {
        const string source = """
                              using System;

                              public class C
                              {
                                  private static int _t;
                                  private Func<int> _f =
                              #if DEBUG
                                      static () => 1;
                              #else
                                      () => 2;
                              #endif
                                  private static int _u;
                                  private static readonly int {|RH7109:_c|} = Log("c");

                                  private static int Log(string name)
                                  {
                                      return 0;
                                  }
                              }
                              """;

        await VerifyCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying that a <c>static</c> lambda that an excluded branch writes inside an argument list of a crossed field's
    /// initializer is no <c>static</c> modifier: the field is an instance field in every build, so moving a static
    /// initialized field across it is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-007")]
    [Requirement("RH7109-006")]
    public async Task FixIsOfferedWhenAnExcludedArgumentOfACrossedFieldsInitializerIsAStaticLambda(string endOfLine)
    {
        const string source = """
                              using System;

                              public class C
                              {
                                  private static int _t;
                                  private int _f = Run(
                              #if DEBUG
                                      static () => 1);
                              #else
                                      () => 2);
                              #endif
                                  private static int _u;
                                  private static readonly int {|RH7109:_c|} = Log("c");

                                  private static int Log(string name)
                                  {
                                      return 0;
                                  }

                                  private static int Run(Func<int> f)
                                  {
                                      return f();
                                  }
                              }
                              """;

        await VerifyCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying that a crossed declaration that one branch declares <c>static readonly</c> and the other branch <c>const</c>
    /// gives no build an instance initializer, because a constant contributes nothing to the initializer order, so
    /// moving an instance initialized field across it is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-007")]
    [Requirement("RH7109-006")]
    public async Task FixIsOfferedWhenACrossedDeclarationIsAStaticReadonlyFieldInOneBranchAndAConstantInTheOther(string endOfLine)
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

        await VerifyCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying the counterpart where the active branch declares the crossed declaration <c>readonly</c> instead of
    /// <c>const</c>: that build gives it an instance initializer, so moving an instance initialized field across it is
    /// withheld
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-007")]
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
    /// Verifying that conditional groups that only write attribute lists in front of a crossed field change nothing the
    /// field contributes, so moving an instance initialized field across the static field keeps its fix, however many
    /// such groups there are
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-007")]
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
    /// Verifying that conditional groups that only write entries of a crossed field's collection initializer change
    /// nothing the field contributes, so moving an instance initialized field across the static field keeps its fix,
    /// however many such groups there are
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-007")]
    [Requirement("RH7109-006")]
    public async Task FixMovesAFieldAcrossAFieldWhoseCollectionInitializerHasNineConditionalEntries(string endOfLine)
    {
        const string source = """
                              using System.Collections.Generic;

                              public class C
                              {
                                  private int _a;
                                  private static List<int> _b = new List<int>
                                                                {
                              #if A1
                                      1,
                              #endif
                              #if A2
                                      2,
                              #endif
                              #if A3
                                      3,
                              #endif
                              #if A4
                                      4,
                              #endif
                              #if A5
                                      5,
                              #endif
                              #if A6
                                      6,
                              #endif
                              #if A7
                                      7,
                              #endif
                              #if A8
                                      8,
                              #endif
                              #if A9
                                      9,
                              #endif
                                                                };
                                  private readonly int {|RH7109:_c|} = Log("c");

                                  private static int Log(string name)
                                  {
                                      return 0;
                                  }
                              }
                              """;

        const string fixedSource = """
                                   using System.Collections.Generic;

                                   public class C
                                   {
                                       private readonly int _c = Log("c");
                                       private int _a;
                                       private static List<int> _b = new List<int>
                                                                     {
                                   #if A1
                                           1,
                                   #endif
                                   #if A2
                                           2,
                                   #endif
                                   #if A3
                                           3,
                                   #endif
                                   #if A4
                                           4,
                                   #endif
                                   #if A5
                                           5,
                                   #endif
                                   #if A6
                                           6,
                                   #endif
                                   #if A7
                                           7,
                                   #endif
                                   #if A8
                                           8,
                                   #endif
                                   #if A9
                                           9,
                                   #endif
                                                                     };

                                       private static int Log(string name)
                                       {
                                           return 0;
                                       }
                                   }
                                   """;

        await Verify(WithEndOfLine(source, endOfLine), WithEndOfLine(fixedSource, endOfLine));
    }

    /// <summary>
    /// Verifying that eight conditional groups in front of a crossed field, whose 256 build configurations declare no
    /// initialized member, keep the fix
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-007")]
    [Requirement("RH7109-006")]
    public async Task FixMovesAFieldAcrossEightConditionalGroupsInFrontOfACrossedField(string endOfLine)
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
                                       private int _b;
                                   }
                                   """;

        await Verify(WithEndOfLine(source, endOfLine), WithEndOfLine(fixedSource, endOfLine));
    }

    /// <summary>
    /// Verifying that nine conditional groups in front of a crossed field, whose 512 build configurations exceed the
    /// number the member-order guard reads, are treated as a changed order when the move crosses their members, so no
    /// code fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-007")]
    [Requirement("RH7109-006")]
    public async Task NoCodeFixWhenConditionalGroupsInFrontOfACrossedFieldExceedTheConfigurationLimit(string endOfLine)
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

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying that a conditional group of the type's body whose <c>#endif</c> lies behind the type's closing brace
    /// keeps the fix when the move crosses no initialized member
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-007")]
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
    /// Verifying the counterpart where the move crosses an instance initialized field: with a conditional group of the
    /// type's body whose <c>#endif</c> lies behind the type's closing brace, no code fix is offered
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
    /// Verifying that a group nested in an excluded branch counts at member level in the builds where that branch
    /// closes the bracket the active code opens, so the static initialized field it declares is crossed and moving a
    /// static initialized field across it is withheld
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-007")]
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
    /// Verifying that a group next to an earlier group that closes the active bracket in another build counts at member
    /// level in that build, so the static initialized field it declares is crossed and moving a static initialized
    /// field across it is withheld
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-007")]
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
    /// Verifying the counterpart of a group nested in a branch that closes a bracket: the nested group stands at member
    /// level in every build, so the static initialized field it declares is crossed and the move is withheld
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-007")]
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
    /// Verifying that an excluded branch that closes the enclosing method block, declares a static initialized field,
    /// and opens another method contributes that field at member level in its build, so moving a static initialized
    /// field across it is withheld
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-007")]
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
    /// Verifying the counterpart of an excluded branch that closes a method block: a group that stays inside the method
    /// block in every build declares no member, so the move keeps its fix
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-007")]
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
    /// Verifying that an excluded branch that closes an expression body, declares a static initialized field, and opens
    /// another expression-bodied property contributes that field at member level in its build, so moving a static
    /// initialized field across it is withheld
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-007")]
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
    /// Verifying the counterpart of an excluded branch that closes an expression body: a group that stays inside the
    /// expression body in every build declares no member, so the move keeps its fix
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-007")]
    [Requirement("RH7109-006")]
    public async Task FixMovesAFieldAcrossAPropertyWhoseExpressionBodyHoldsAnExcludedArgument(string endOfLine)
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

        const string fixedSource = """
                                   public class C
                                   {
                                       private static readonly int _c = Log("c");

                                       private static int _t;

                                       public int P => Run(1
                                   #if DEBUG
                                                           , Log("z")
                                   #endif
                                                           );

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
    /// Verifying that an excluded branch that closes a nested type's body, declares a static initialized field, and
    /// opens another nested type contributes that field to the outer type in its build, so moving a static initialized
    /// field across it is withheld
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-007")]
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
    /// Verifying the counterpart of an excluded branch that closes a nested type's body: a group that stays inside the
    /// nested type in every build declares a member of the nested type only, so the move keeps its fix
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-007")]
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
    /// Verifying that two declarations of one name in mutually exclusive branches are judged each in its own build: the
    /// static one has no initializer and the initialized one is an instance field, so no build crosses a static
    /// initialized field and the move keeps its fix
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
    /// Verifying that two declarations of one name in mutually exclusive branches are judged each in its own build: the
    /// initialized one is static and the instance one has no initializer, so no build crosses an instance initialized
    /// field and the move keeps its fix
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
    /// initialized field in the other give no build a crossed static initialized field, so the move keeps its fix
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
    /// Verifying the other side of judging each build on its own: one of two declarations of one name is a static
    /// initialized field in its build, so moving a static initialized field across it is withheld
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
    /// Verifying the other side of judging each build on its own: one of two declarations of one name is an instance
    /// initialized field in its build, so moving an instance initialized field across it is withheld
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
    /// Verifying that a declaration split into a static field without an initializer in one build and an instance field
    /// with an initializer in the other is judged in each build on its own: no build gives it a static initializer, so
    /// moving a static initialized field across it keeps the fix
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-007")]
    [Requirement("RH7109-006")]
    public async Task FixIsOfferedWhenACrossedDeclarationIsSplitIntoAStaticFieldAndAnInstanceInitializedField(string endOfLine)
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
                                  private static readonly int {|RH7109:_c|} = Log("c");

                                  private static int Log(string name)
                                  {
                                      return 0;
                                  }
                              }
                              """;

        await VerifyCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying the counterpart of a declaration split into a static field and an instance initialized field: an
    /// instance initialized field moved across it would run its initializer before the split one in the build that
    /// compiles the instance field, so no code fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-007")]
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
    /// Verifying that an excluded branch whose brackets balance but which ends an expression-bodied property and declares
    /// a static initialized field contributes that field to the type in its build, so moving a static initialized field
    /// across it is withheld
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-007")]
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
    /// Verifying the counterpart of a balanced excluded branch that ends an expression body: a group whose branches each
    /// end the expression body but declare no member keeps the fix. Only whether the fix is offered is under test: the
    /// layout of the continuation lines in the branches is decided by whole-type formatting
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-007")]
    [Requirement("RH7109-006")]
    public async Task FixIsOfferedAcrossAPropertyWhoseExcludedBranchEndsTheExpressionBodyWithoutDeclaringAMember(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private static int _t;

                                  public int P =>
                              #if DEBUG
                                      1;
                              #else
                                      2;
                              #endif
                                  private int _e;

                                  private static readonly int {|RH7109:_c|} = Log("c");

                                  private static int Log(string name)
                                  {
                                      return 0;
                                  }
                              }
                              """;

        await VerifyCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying that conditional groups whose build configurations exceed the number the member-order guard reads only
    /// withhold a move that touches them: a move that neither crosses nor moves a member of those groups keeps its fix
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-007")]
    [Requirement("RH7109-006")]
    public async Task FixMovesAFieldWhenConditionalGroupsThatExceedTheConfigurationLimitLieAwayFromTheMove(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private int _t;
                                  private readonly int {|RH7109:_c|};

                              #if FEATURE
                              #if A1
                                  private int _a1;
                              #endif
                              #if A2
                                  private int _a2;
                              #endif
                              #if A3
                                  private int _a3;
                              #endif
                              #if A4
                                  private int _a4;
                              #endif
                              #if A5
                                  private int _a5;
                              #endif
                              #if A6
                                  private int _a6;
                              #endif
                              #if A7
                                  private int _a7;
                              #endif
                              #if A8
                                  private int _a8;
                              #endif
                              #if A9
                                  private int _a9;
                              #endif
                                  private int _f;
                              #endif
                              }
                              """;

        const string fixedSource = """
                                   public class C
                                   {
                                       private readonly int _c;
                                       private int _t;

                                   #if FEATURE
                                   #if A1
                                       private int _a1;
                                   #endif
                                   #if A2
                                       private int _a2;
                                   #endif
                                   #if A3
                                       private int _a3;
                                   #endif
                                   #if A4
                                       private int _a4;
                                   #endif
                                   #if A5
                                       private int _a5;
                                   #endif
                                   #if A6
                                       private int _a6;
                                   #endif
                                   #if A7
                                       private int _a7;
                                   #endif
                                   #if A8
                                       private int _a8;
                                   #endif
                                   #if A9
                                       private int _a9;
                                   #endif
                                       private int _f;
                                   #endif
                                   }
                                   """;

        await Verify(WithEndOfLine(source, endOfLine), WithEndOfLine(fixedSource, endOfLine));
    }

    /// <summary>
    /// Verifying that an excluded branch which closes the type and opens another one puts the moved field into that other
    /// type in its build: with <c>DEBUG</c> defined, the fix would move <c>_c</c> out of <c>D</c> across a static initialized
    /// field, so no code fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-007")]
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
    /// Verifying that an excluded branch which closes the type and opens another one withholds the move even when no
    /// initializer is crossed: with <c>DEBUG</c> defined, the moved field is a member of <c>D</c>, not of the type the fix
    /// moves it in, so no code fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-007")]
    [Requirement("RH7109-006")]
    public async Task NoCodeFixWhenAnExcludedBranchClosesTheTypeAndOpensAnotherOneAroundTheMovedField(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private static int _t;
                              #if DEBUG
                              }

                              public class D
                              {
                                  private static int _z;
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
    /// Verifying that an excluded branch which opens a nested type around the moved field, closed by another group behind
    /// it, makes the moved field a member of the nested type in its build: the fix would move it out of the nested type, so
    /// no code fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-001")]
    [Requirement("SHARED-ORDER-007")]
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
    /// Verifying that an excluded branch in the moved field's leading trivia which closes a nested type is not carried
    /// across the members in front of it: with <c>DEBUG</c> defined, the moved closing brace would close the type itself, so
    /// no code fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-007")]
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
    /// Verifying the counterpart of a nested type that excluded branches open and close: when it closes in front of a member
    /// that precedes the moved field, the moved field is a member of the type in every build, and the static initialized
    /// field inside the nested type belongs to the nested type, so the code fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-007")]
    [Requirement("RH7109-006")]
    public async Task FixMovesAFieldAcrossANestedTypeThatExcludedBranchesOpenAndCloseInFrontOfIt(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private static int _t;
                              #if DEBUG
                                  private class N
                                  {
                                      private static int _z = Log("z");
                              #endif
                              #if DEBUG
                                  }
                              #endif
                                  private int _d;
                                  private static readonly int {|RH7109:_c|} = Log("c");

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
                                       private class N
                                       {
                                           private static int _z = Log("z");
                                   #endif
                                   #if DEBUG
                                       }
                                   #endif
                                       private int _d;

                                       private static int Log(string name)
                                       {
                                           return 0;
                                       }
                                   }
                                   """;

        await Verify(WithEndOfLine(source, endOfLine), WithEndOfLine(fixedSource, endOfLine));
    }

    /// <summary>
    /// Verifying the counterpart of a nested type that excluded branches open around a member: when a member of the type
    /// stands between the closing group and the moved field, the moved field is a member of the type in every build and its
    /// leading trivia holds no directive, so the code fix is offered. Only whether the fix is offered is under test: the
    /// column of the member inside the excluded nested type is decided by whole-type formatting
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-007")]
    [Requirement("RH7109-006")]
    public async Task FixIsOfferedAcrossANestedTypeThatExcludedBranchesOpenAroundAMemberInFrontOfAnotherMember(string endOfLine)
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
                                  private int _e;
                                  private static readonly int {|RH7109:_c|} = Log("c");

                                  private static int Log(string name)
                                  {
                                      return 0;
                                  }
                              }
                              """;

        await VerifyCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying that two groups with negated conditions that split one crossed declaration are read only in the builds a
    /// definition of <c>DEBUG</c> selects: the build with <c>DEBUG</c> compiles a static field without an initializer and
    /// the other an instance initialized field, so no build crosses a static initializer and the code fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-007")]
    [Requirement("RH7109-006")]
    public async Task FixIsOfferedWhenGroupsWithNegatedConditionsSplitACrossedDeclarationIntoAStaticFieldAndAnInstanceInitializedField(string endOfLine)
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
                              #if !DEBUG
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

        await VerifyCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying the counterpart of groups with negated conditions that split a crossed declaration: when the second group
    /// tests another symbol, the build with both symbols defined compiles a static initialized field, so moving a static
    /// initialized field across it is withheld
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-007")]
    [Requirement("RH7109-006")]
    public async Task NoCodeFixWhenGroupsWithIndependentConditionsSplitACrossedDeclarationIntoAStaticInitializedField(string endOfLine)
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
    /// Verifying that two groups with the same condition that split one crossed declaration are read only in the builds a
    /// definition of <c>DEBUG</c> selects: the build with <c>DEBUG</c> compiles a static initialized field and the other an
    /// instance field without an initializer, so no build crosses an instance initializer and the code fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-007")]
    [Requirement("RH7109-006")]
    public async Task FixIsOfferedWhenGroupsWithTheSameConditionSplitACrossedDeclarationIntoAStaticInitializedFieldAndAnInstanceField(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private int _t;
                                  private
                              #if DEBUG
                                      static
                              #endif
                                      int _b
                              #if DEBUG
                                      = Log("b")
                              #endif
                                      ;
                                  private static int _d;
                                  private readonly int {|RH7109:_c|} = Log("c");

                                  private static int Log(string name)
                                  {
                                      return 0;
                                  }
                              }
                              """;

        await VerifyCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying the counterpart of groups with the same condition that split a crossed declaration: with negated
    /// conditions, the build without <c>DEBUG</c> compiles an instance initialized field, so moving an instance initialized
    /// field across it is withheld
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-007")]
    [Requirement("RH7109-006")]
    public async Task NoCodeFixWhenGroupsWithNegatedConditionsSplitACrossedDeclarationIntoAnInstanceInitializedField(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private int _t;
                                  private
                              #if DEBUG
                                      static
                              #endif
                                      int _b
                              #if !DEBUG
                                      = Log("b")
                              #endif
                                      ;
                                  private static int _d;
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
    /// Verifying that a moved and a crossed declaration split by groups with negated conditions are judged together in the
    /// builds a definition of <c>DEBUG</c> selects: in each build one of them is static and the other an instance field, so
    /// no build moves two initializers of one context and the code fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-007")]
    [Requirement("RH7109-006")]
    public async Task FixIsOfferedWhenTheMovedAndACrossedFieldAreStaticInBuildsThatExcludeEachOther(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private int _t;
                                  private
                              #if !DEBUG
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

        await VerifyCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying the counterpart of a moved and a crossed declaration split by groups with negated conditions: with the
    /// same condition, both are static initialized fields in the build with <c>DEBUG</c> and instance initialized fields in
    /// the other, so no code fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-007")]
    [Requirement("RH7109-006")]
    public async Task NoCodeFixWhenTheMovedAndACrossedFieldAreStaticInTheSameBuild(string endOfLine)
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
    /// Verifying that the symbols of every conditional directive in the crossed span count together toward the
    /// configuration limit, also when active members separate the groups: two runs of five groups name ten symbols, so no
    /// code fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-007")]
    [Requirement("RH7109-006")]
    public async Task NoCodeFixWhenSeparatedConditionalGroupsInTheCrossedSpanTogetherExceedTheConfigurationLimit(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private int _t;
                              #if R1A1
                                  private int _r1a1;
                              #endif
                              #if R1A2
                                  private int _r1a2;
                              #endif
                              #if R1A3
                                  private int _r1a3;
                              #endif
                              #if R1A4
                                  private int _r1a4;
                              #endif
                              #if R1A5
                                  private int _r1a5;
                              #endif
                                  private int _m1;
                              #if R2A1
                                  private int _r2a1;
                              #endif
                              #if R2A2
                                  private int _r2a2;
                              #endif
                              #if R2A3
                                  private int _r2a3;
                              #endif
                              #if R2A4
                                  private int _r2a4;
                              #endif
                              #if R2A5
                                  private int _r2a5;
                              #endif
                                  private int _m2;
                                  private readonly int {|RH7109:_c|};
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    #endregion // Tests

    #region Methods

    /// <summary>
    /// Verifies that the analyzer reports exactly the diagnostics marked in the source, and that the code fix registers
    /// an action for the first of them whose result compiles and is not reported. Used where only whether the fix is
    /// offered is under test, and the layout of the result is decided elsewhere
    /// </summary>
    /// <param name="source">The source text to test, with markup for every expected diagnostic</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private static async Task VerifyCodeFixIsOffered(string source)
    {
        await Verify(source);

        TestFileMarkupParser.GetSpans(source, out var plainSource, out ImmutableDictionary<string, ImmutableArray<TextSpan>> _);

        var fixedSource = await TryApplyCodeFixAsync(plainSource);

        Assert.IsNotNull(fixedSource, "A code fix must be offered for this input.");

        await Verify(fixedSource);
    }

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