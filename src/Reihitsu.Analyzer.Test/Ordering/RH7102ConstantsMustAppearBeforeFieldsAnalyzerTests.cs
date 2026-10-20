using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp.Syntax;
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
    /// Characterizes current behavior that no requirement states: the fix also lays out the other members of the
    /// type, and an empty nested class becomes a declaration without body
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task FixAlsoLaysOutOtherMembersOfTheType()
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
                                       public class Inner;
                                   }
                                   """;

        await Verify(source, fixedSource);
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