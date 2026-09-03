using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp.Syntax;
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
    /// reorder (issue #727)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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
    /// Verifies that directive safety still withholds the fix for a mixed-accessibility interface violation
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