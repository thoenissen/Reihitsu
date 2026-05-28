using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Organization;
using Reihitsu.Analyzer.Rules.Organization;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Ordering;

/// <summary>
/// Test methods for <see cref="RH7102ConstantsMustAppearBeforeFieldsAnalyzer"/> and <see cref="RH7102ConstantsMustAppearBeforeFieldsCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH7102ConstantsMustAppearBeforeFieldsAnalyzerTests : AnalyzerTestsBase<RH7102ConstantsMustAppearBeforeFieldsAnalyzer, RH7102ConstantsMustAppearBeforeFieldsCodeFixProvider>
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

    #endregion // Tests
}