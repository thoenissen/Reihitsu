using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH5508StructAttributeListsMustFollowShapeRulesAnalyzer"/>
/// </summary>
[TestClass]
public class RH5508StructAttributeListsMustFollowShapeRulesFormatterTests : FormatterTestsBase<RH5508StructAttributeListsMustFollowShapeRulesAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that the formatter fixes the rule violation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesRuleViolation()
    {
        const string input = """
                                {|#0:[First, Second]|}
                                internal struct Example { }
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                sealed class SecondAttribute : System.Attribute
                                {
                                }
                                
                             """;
        const string fixedData = """
                                 [First]
                                 [Second]
                                 internal struct Example;
                                 sealed class FirstAttribute : System.Attribute;
                                 sealed class SecondAttribute : System.Attribute;
                                 """;

        await VerifyFormatter(input,
                              fixedData,
                              Diagnostics(RH5508StructAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5508MessageFormat));
    }

    /// <summary>
    /// Verifies that the formatter splits the attribute list of a union, whose lists resolve to the struct target, so the
    /// output carries no diagnostic, and that a second pass leaves it unchanged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("FMT-STRUCT-028")]
    [Requirement("RH5508-001")]
    public async Task VerifyFormatterSplitsUnionAttributeList()
    {
        const string input = """
                             using System;

                             {|#0:[Serializable, Obsolete]|} public union U(int, string);
                             """;
        const string fixedData = """
                                 using System;

                                 [Serializable]
                                 [Obsolete]
                                 public union U(int, string);
                                 """;

        var unionSupport = await UnionSupport.GetReferenceAsync();

        await VerifyFormatter(input,
                              fixedData,
                              test => test.TestState.AdditionalReferences.Add(unionSupport),
                              Diagnostics(RH5508StructAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5508MessageFormat));
    }

    #endregion // Tests
}