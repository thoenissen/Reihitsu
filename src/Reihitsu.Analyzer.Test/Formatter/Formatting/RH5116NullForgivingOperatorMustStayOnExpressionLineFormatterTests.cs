using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH5116NullForgivingOperatorMustStayOnExpressionLineAnalyzer"/>
/// </summary>
[TestClass]
public class RH5116NullForgivingOperatorMustStayOnExpressionLineFormatterTests : FormatterTestsBase<RH5116NullForgivingOperatorMustStayOnExpressionLineAnalyzer>
{
    #region Constants

    /// <summary>
    /// Members and statements that hold a null-forgiving operator below its expression in many enclosing contexts
    /// </summary>
    private const string ContextsTestData = """
                                            #nullable enable
                                            using System;

                                            class C
                                            {
                                                private static readonly string? Name = null;
                                                private string _first = Name
                                                    {|RH5116:!|};

                                                public string Second => Name
                                                    {|RH5116:!|};

                                                int M(string? s, Func<int, int>? f, int[]? n)
                                                {
                                                    var a = s
                                                        {|RH5116:!|} is { };
                                                    Func<string> g = () => s
                                                        {|RH5116:!|};
                                                    var h = $"{s
                                                        {|RH5116:!|}}";
                                                    var i = Math.Abs(f
                                                        {|RH5116:!|}(1));
                                                    var j = n
                                                        {|RH5116:!|}[0];
                                                    var k = n
                                                        {|RH5116:!|}?[0];
                                                    var l = s
                                                        {|RH5116:!|} + "";

                                                    if (s
                                                        {|RH5116:!|} == "")
                                                    {
                                                        throw new Exception(s
                                                            {|RH5116:!|});
                                                    }

                                                    return j;
                                                }
                                            }
                                            """;

    /// <summary>
    /// The formatted <see cref="ContextsTestData"/>: every operator is joined to its expression
    /// </summary>
    private const string ContextsResultData = """
                                              #nullable enable
                                              using System;

                                              class C
                                              {
                                                  private static readonly string? Name = null;
                                                  private string _first = Name!;

                                                  public string Second => Name!;

                                                  int M(string? s, Func<int, int>? f, int[]? n)
                                                  {
                                                      var a = s! is { };
                                                      Func<string> g = () => s!;
                                                      var h = $"{s!}";
                                                      var i = Math.Abs(f!(1));
                                                      var j = n![0];
                                                      var k = n!?[0];
                                                      var l = s! + "";

                                                      if (s! == "")
                                                      {
                                                          throw new Exception(s!);
                                                      }

                                                      return j;
                                                  }
                                              }
                                              """;

    /// <summary>
    /// A chain with links whose attached null-forgiving operator stands below the code in front of it
    /// </summary>
    private const string ChainTestData = """
                                         #nullable enable

                                         class C
                                         {
                                             void M(string s)
                                             {
                                                 var a = s
                                                     {|RH5116:!|}[0].ToString()
                                                     .Trim();
                                             }
                                         }
                                         """;

    /// <summary>
    /// The formatted <see cref="ChainTestData"/>: the operator is joined and the chain laid out
    /// </summary>
    private const string ChainResultData = """
                                           #nullable enable

                                           class C
                                           {
                                               void M(string s)
                                               {
                                                   var a = s![0].ToString()
                                                                .Trim();
                                               }
                                           }
                                           """;

    #endregion // Constants

    #region Tests

    /// <summary>
    /// Verifies that null-forgiving operators below their expression in many enclosing contexts are reported, that the formatter
    /// joins them, and that the formatted code is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5116-001")]
    [Requirement("SHARED-DIAG-010")]
    public async Task VerifyFormatterJoinsReportedOperatorsInEnclosingContexts()
    {
        await VerifyFormatter(ContextsTestData, ContextsResultData);
    }

    /// <summary>
    /// Verifies that a null-forgiving operator of a chain with links is reported, that the formatter joins it and lays out the
    /// chain, and that the formatted code is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5116-001")]
    [Requirement("SHARED-DIAG-010")]
    public async Task VerifyFormatterJoinsAReportedOperatorInAChainWithLinks()
    {
        await VerifyFormatter(ChainTestData, ChainResultData);
    }

    /// <summary>
    /// Verifies that the formatted code of every context stays unchanged and unreported when it is formatted again
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5116-001")]
    [Requirement("SHARED-DIAG-010")]
    public async Task VerifyFormattedContextsStayUnchangedAndUnreported()
    {
        await VerifyFormatter(ContextsResultData);
        await VerifyFormatter(ChainResultData);
    }

    #endregion // Tests
}