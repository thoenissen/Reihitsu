using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Organization;
using Reihitsu.Analyzer.Rules.Organization;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Ordering;

/// <summary>
/// Test methods for <see cref="RH7104PartialElementsMustDeclareAccessModifierAnalyzer"/> and <see cref="RH7104PartialElementsMustDeclareAccessModifierCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH7104PartialElementsMustDeclareAccessModifierAnalyzerTests : BatchCodeFixTestsBase<RH7104PartialElementsMustDeclareAccessModifierAnalyzer, RH7104PartialElementsMustDeclareAccessModifierCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying partial types without an access modifier are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task PartialTypesWithoutAccessModifierAreReportedAndFixed()
    {
        const string testCode = """
                                partial class {|#0:TestClass|}
                                {
                                    public int Bar { get; set; }
                                }

                                partial struct {|#1:TestStruct|}
                                {
                                }

                                partial interface {|#2:ITest|}
                                {
                                }

                                partial record {|#3:TestRecord|};

                                partial record struct {|#4:TestRecordStruct|};
                                """;

        const string fixedCode = """
                                 internal partial class TestClass
                                 {
                                     public int Bar { get; set; }
                                 }

                                 internal partial struct TestStruct;

                                 internal partial interface ITest;

                                 internal partial record TestRecord;

                                 internal partial record struct TestRecordStruct;
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH7104PartialElementsMustDeclareAccessModifierAnalyzer.DiagnosticId, AnalyzerResources.RH7104MessageFormat, 5));
    }

    /// <summary>
    /// Verifying that partial parts inherit and declare the accessibility selected by another part
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task PartialTypesWithPublicAndCompoundAccessibilityAreFixed()
    {
        const string testCode = """
                                public partial class Sample
                                {
                                }

                                partial class {|#0:Sample|}
                                {
                                }

                                internal partial class Outer
                                {
                                    protected internal partial class Inner
                                    {
                                    }

                                    partial class {|#1:Inner|}
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public partial class Sample
                                 {
                                 }

                                 public partial class Sample;

                                 internal partial class Outer
                                 {
                                     protected internal partial class Inner
                                     {
                                     }
                                     protected internal partial class Inner;
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH7104PartialElementsMustDeclareAccessModifierAnalyzer.DiagnosticId, AnalyzerResources.RH7104MessageFormat, 2));
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                partial class {|#0:Outer|}
                                {
                                    partial class {|#1:Inner|}
                                    {
                                        public int Value { get; set; }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal partial class Outer
                                 {
                                     private partial class Inner
                                     {
                                         public int Value { get; set; }
                                     }
                                 }
                                 """;

        // The nested declaration sits inside the declaration the outer fix rewrites, so both corrections are
        // computed against overlapping subtrees while their text changes stay confined to the inserted keyword
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH7104PartialElementsMustDeclareAccessModifierAnalyzer.DiagnosticId, AnalyzerResources.RH7104MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}