using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter stability tests for parameter attribute lists followed by a line comment
/// </summary>
[TestClass]
public class RH5523ParameterAttributeLineCommentFormatterStabilityTests : FormatterTestsBase<RH5523ParameterAttributesMustFollowPlacementRulesAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that a record parameter type after an attribute list and a line comment stays untouched by the analyzer and the formatter
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyRecordParameterAfterAttributeListAndLineCommentIsStable()
    {
        const string input = """
                             using System;

                             namespace Demo;

                             internal sealed record Example([Marker] // keep
                                                            int Id);

                             internal sealed class MarkerAttribute : Attribute;
                             """;

        await VerifyFormatter(input);
    }

    /// <summary>
    /// Verifies that a method parameter type after an attribute list and a line comment stays untouched by the analyzer and the formatter
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMethodParameterAfterAttributeListAndLineCommentIsStable()
    {
        const string input = """
                             using System;

                             namespace Demo;

                             internal class Example
                             {
                                 internal void M([Marker] // keep
                                                 int value)
                                 {
                                 }
                             }

                             internal sealed class MarkerAttribute : Attribute;
                             """;

        await VerifyFormatter(input);
    }

    #endregion // Tests
}