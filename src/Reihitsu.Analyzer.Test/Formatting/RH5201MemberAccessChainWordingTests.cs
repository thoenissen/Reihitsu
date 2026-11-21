using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for the wording of <see cref="RH5201MethodChainsShouldBeAlignedAnalyzer"/> and <see cref="RH5201MethodChainsShouldBeAlignedCodeFixProvider"/>:
/// the rule's title, its message, and the code fix's title name a member-access chain
/// </summary>
[TestClass]
public class RH5201MemberAccessChainWordingTests : BatchCodeFixTestsBase<RH5201MethodChainsShouldBeAlignedAnalyzer, RH5201MethodChainsShouldBeAlignedCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that the rule's title names a member-access chain, not a method chain
    /// </summary>
    [TestMethod]
    public void VerifyTitleNamesMemberAccessChain()
    {
        var descriptor = new RH5201MethodChainsShouldBeAlignedAnalyzer().SupportedDiagnostics.Single();

        Assert.AreEqual("Member-access chains should be aligned.", descriptor.Title.ToString(CultureInfo.InvariantCulture));
    }

    /// <summary>
    /// Verifies that a misaligned link of a chain without any call is reported with the message that names a member-access
    /// chain and is aligned under the link in front of it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMisalignedLinkOfCallLessChainIsReportedWithMemberAccessChainMessage()
    {
        const string testData = """
                                internal sealed class Example
                                {
                                    private static object Run(dynamic order)
                                    {
                                        return order.Customer.Address
                                                    {|#0:.|}City;
                                    }
                                }
                                """;
        const string resultData = """
                                  internal sealed class Example
                                  {
                                      private static object Run(dynamic order)
                                      {
                                          return order.Customer.Address
                                                               .City;
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH5201MethodChainsShouldBeAlignedAnalyzer.DiagnosticId, "Member-access chains should be aligned."));
    }

    /// <summary>
    /// Verifies that the only code fix offered for a misaligned link of a chain without any call is titled with a
    /// member-access chain
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCodeFixTitleNamesMemberAccessChain()
    {
        const string testData = """
                                internal sealed class Example
                                {
                                    private static object Run(dynamic order)
                                    {
                                        return order.Customer.Address
                                                    .City;
                                    }
                                }
                                """;
        var linkDot = testData.IndexOf(".City", StringComparison.Ordinal);

        var actions = await GetCodeFixActionsAsync(testData,
                                                   RH5201MethodChainsShouldBeAlignedAnalyzer.DiagnosticId,
                                                   root => Location.Create(root.SyntaxTree, new TextSpan(linkDot, 1)));

        Assert.HasCount(1, actions);
        Assert.AreEqual("Align member-access chain", actions[0].Title);
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testData = """
                                internal sealed class Example
                                {
                                    private static object Run(dynamic order)
                                    {
                                        return order.Customer.Address
                                                    {|#0:.|}City
                                                      {|#1:.|}Name;
                                    }
                                }
                                """;
        const string resultData = """
                                  internal sealed class Example
                                  {
                                      private static object Run(dynamic order)
                                      {
                                          return order.Customer.Address
                                                               .City
                                                               .Name;
                                      }
                                  }
                                  """;

        // Verifies that Fix All aligns several misaligned links of a chain without any call in one pass
        return new FixAllScenario(testData,
                                  resultData,
                                  Diagnostics(RH5201MethodChainsShouldBeAlignedAnalyzer.DiagnosticId, MessageFormat, 2),
                                  static config => config.NumberOfFixAllIterations = 1);
    }

    #endregion // BatchCodeFixTestsBase
}