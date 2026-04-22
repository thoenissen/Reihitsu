using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Clarity;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Clarity;

/// <summary>
/// Test methods for <see cref="RH0007UseShorthandForNullableTypesAnalyzer"/> and <see cref="RH0007UseShorthandForNullableTypesCodeFixProvider"/>.
/// </summary>
[TestClass]
public class RH0007UseShorthandForNullableTypesAnalyzerTests : AnalyzerTestsBase<RH0007UseShorthandForNullableTypesAnalyzer, RH0007UseShorthandForNullableTypesCodeFixProvider>
{
    /// <summary>
    /// Verifying Nullable generic types are reported and fixed.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task NullableGenericTypeIsReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    private {|#0:System.Nullable<int>|} _value;
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private int? _value;
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0007UseShorthandForNullableTypesAnalyzer.DiagnosticId, "Use shorthand for nullable types."));
    }
}