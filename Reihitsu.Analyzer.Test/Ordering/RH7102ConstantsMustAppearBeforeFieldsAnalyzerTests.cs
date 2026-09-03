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
    /// reorder (issue #727)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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