using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Organization;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH7101DoNotCombineFieldsAnalyzer"/>
/// </summary>
[TestClass]
public class RH7101DoNotCombineFieldsFormatterTests : FormatterTestsBase<RH7101DoNotCombineFieldsAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that the formatter splits combined field declarations
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesViolation()
    {
        const string input = """
                             internal class Example
                             {
                                 private int firstField, {|#0:secondField|};
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     private int firstField;
                                     private int secondField;
                                 }
                                 """;

        await VerifyFormatter(input,
                              fixedData,
                              Diagnostics(RH7101DoNotCombineFieldsAnalyzer.DiagnosticId, AnalyzerResources.RH7101MessageFormat));
    }

    /// <summary>
    /// Verifies the specification example through the formatter: a line break behind the type and a line break between
    /// two modifiers are repeated in every field the split creates, a line break behind a <c>,</c> stands in no field,
    /// and no field is reported afterwards
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("FMT-STRUCT-021")]
    public async Task VerifyFormatterRepeatsLineBreaksInsideAndBehindTheHeadInEveryField()
    {
        const string input = """
                             using System;
                             using System.Collections.Generic;

                             class C
                             {
                                 private Dictionary<string, List<int>>
                                     _map1, {|#0:_map2|} = new();

                                 private static
                                     readonly int _g = 1, {|#1:_h|};

                                 [NonSerialized]
                                 private int _x,
                                     {|#2:_y|};
                             }
                             """;
        const string fixedData = """
                                 using System;
                                 using System.Collections.Generic;

                                 class C
                                 {
                                     private Dictionary<string, List<int>>
                                     _map1;
                                     private Dictionary<string, List<int>>
                                     _map2 = new();

                                     private static
                                     readonly int _g = 1;
                                     private static
                                     readonly int _h;

                                     [NonSerialized]
                                     private int _x;
                                     [NonSerialized]
                                     private int _y;
                                 }
                                 """;

        await VerifyFormatter(input,
                              fixedData,
                              Diagnostics(RH7101DoNotCombineFieldsAnalyzer.DiagnosticId, AnalyzerResources.RH7101MessageFormat, 3));
    }

    #endregion // Tests
}