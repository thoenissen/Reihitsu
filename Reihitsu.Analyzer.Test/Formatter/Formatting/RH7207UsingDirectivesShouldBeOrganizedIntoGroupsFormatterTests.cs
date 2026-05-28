using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Organization;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH7207UsingDirectivesShouldBeOrganizedIntoGroupsAnalyzer"/>
/// </summary>
[TestClass]
public class RH7207UsingDirectivesShouldBeOrganizedIntoGroupsFormatterTests : FormatterTestsBase<RH7207UsingDirectivesShouldBeOrganizedIntoGroupsAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that the formatter sorts and groups using directives
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesViolation()
    {
        const string input = """
                             using {|#0:Bravo|};
                             using Alpha.Sub;
                             using static System.Math;
                             using System;
                             using Alpha;
                             using TextAlias = System.String;
                             
                             internal class Example;
                             
                             namespace Alpha
                             {
                                 internal class Placeholder;
                             }
                             
                             namespace Alpha.Sub
                             {
                                 internal class Placeholder;
                             }
                             
                             namespace Bravo
                             {
                                 internal class Placeholder;
                             }
                             """;
        const string fixedData = """
                                 using System;
                                 
                                 using Alpha;
                                 using Alpha.Sub;
                                 
                                 using Bravo;
                                 
                                 using static System.Math;
                                 
                                 using TextAlias = System.String;
                                 
                                 internal class Example;
                                 
                                 namespace Alpha
                                 {
                                     internal class Placeholder;
                                 }
                                 
                                 namespace Alpha.Sub
                                 {
                                     internal class Placeholder;
                                 }
                                 
                                 namespace Bravo
                                 {
                                     internal class Placeholder;
                                 }
                                 """;

        await VerifyFormatterFix(input,
                                 fixedData,
                                 Diagnostics(RH7207UsingDirectivesShouldBeOrganizedIntoGroupsAnalyzer.DiagnosticId, AnalyzerResources.RH7207MessageFormat));
    }

    #endregion // Tests
}