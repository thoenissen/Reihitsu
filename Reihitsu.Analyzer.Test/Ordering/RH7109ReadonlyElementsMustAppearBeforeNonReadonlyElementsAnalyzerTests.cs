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
public class RH7109ReadonlyElementsMustAppearBeforeNonReadonlyElementsAnalyzerTests : AnalyzerTestsBase<RH7109ReadonlyElementsMustAppearBeforeNonReadonlyElementsAnalyzer, RH7109ReadonlyElementsMustAppearBeforeNonReadonlyElementsCodeFixProvider>
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
    /// Verifying no code fix is offered when preprocessor directives sit in the affected leading trivia
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
}