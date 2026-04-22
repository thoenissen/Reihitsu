using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Ordering;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Ordering;

/// <summary>
/// Test methods for <see cref="RH0601ConstantsMustAppearBeforeFieldsAnalyzer"/> and <see cref="RH0601ConstantsMustAppearBeforeFieldsCodeFixProvider"/>.
/// </summary>
[TestClass]
public class RH0601ConstantsMustAppearBeforeFieldsAnalyzerTests : AnalyzerTestsBase<RH0601ConstantsMustAppearBeforeFieldsAnalyzer, RH0601ConstantsMustAppearBeforeFieldsCodeFixProvider>
{
    /// <summary>
    /// Verifying const fields are reported and fixed when they appear after mutable fields.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
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

        await Verify(testCode, fixedCode, Diagnostics(RH0601ConstantsMustAppearBeforeFieldsAnalyzer.DiagnosticId, AnalyzerResources.RH0601MessageFormat));
    }
}