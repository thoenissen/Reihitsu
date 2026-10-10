using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Spacing;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH6017IncrementAndDecrementSymbolsMustBeSpacedCorrectlyAnalyzer"/>
/// </summary>
[TestClass]
public class RH6017IncrementAndDecrementSymbolsMustBeSpacedCorrectlyFormatterTests : FormatterTestsBase<RH6017IncrementAndDecrementSymbolsMustBeSpacedCorrectlyAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that the formatter fixes the targeted violation and clears the analyzer diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesViolation()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        int value = 0;
                                        value{|#0: |}++;
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method()
                                     {
                                         int value = 0;

                                         value++;
                                     }
                                 }
                                 """;

        await VerifyFormatter(testData,
                              fixedData,
                              Diagnostics(RH6017IncrementAndDecrementSymbolsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6017MessageFormat));
    }

    /// <summary>
    /// Verifies that increment and decrement operator overload declarations are formatter-stable and do not
    /// produce a diagnostic. The required space between the <c>operator</c> keyword and the operator symbol
    /// (RH6005) is a different, mandatory space that this rule must not treat as a violation to remove
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyOperatorOverloadDeclarationsAreIgnored()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    public static TestClass operator ++(TestClass value)
                                    {
                                        return value;
                                    }

                                    public static TestClass operator --(TestClass value)
                                    {
                                        return value;
                                    }

                                    public static TestClass operator checked ++(TestClass value)
                                    {
                                        return value;
                                    }
                                }
                                """;

        await VerifyFormatter(testData);
    }

    #endregion // Tests
}