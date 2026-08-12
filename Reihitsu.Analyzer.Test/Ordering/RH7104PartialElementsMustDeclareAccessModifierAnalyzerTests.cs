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
public class RH7104PartialElementsMustDeclareAccessModifierAnalyzerTests : AnalyzerTestsBase<RH7104PartialElementsMustDeclareAccessModifierAnalyzer, RH7104PartialElementsMustDeclareAccessModifierCodeFixProvider>
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

    #endregion // Tests
}