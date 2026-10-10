using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5513MethodAttributesMustFollowPlacementRulesAnalyzer"/> and <see cref="RH5513MethodAttributesMustFollowPlacementRulesCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5513MethodAttributesMustFollowPlacementRulesAnalyzerTests : BatchCodeFixTestsBase<RH5513MethodAttributesMustFollowPlacementRulesAnalyzer, RH5513MethodAttributesMustFollowPlacementRulesCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that policy violations are detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticAndCodeFixForPolicyViolation()
    {
        const string testData = """
                                internal class Example
                                {
                                    {|#0:[First]|} internal void M() { }
                                }
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                sealed class SecondAttribute : System.Attribute
                                {
                                }
                                """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     [First]
                                     internal void M() { }
                                 }
                                 sealed class FirstAttribute : System.Attribute
                                 {
                                 }
                                 sealed class SecondAttribute : System.Attribute
                                 {
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5513MethodAttributesMustFollowPlacementRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5513MessageFormat));
    }

    /// <summary>
    /// Verifies that splitting the attribute list inside an object initializer lands the member on the attribute
    /// list's own column rather than the enclosing brace-scope nesting level, which understates an anchor-derived
    /// column by not accounting for the initializer's own alignment
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCodeFixAlignsToAttributeListColumnInsideObjectInitializer()
    {
        const string testData = """
                                internal class Example
                                {
                                    private readonly Config _config = new Config
                                    {
                                        Handler = () =>
                                        {
                                                                          {|#0:[First]|} void Local()
                                                                          {
                                                                          }
                                        }
                                    };
                                }
                                internal sealed class Config
                                {
                                    public System.Action Handler { get; set; }
                                }
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     private readonly Config _config = new Config
                                     {
                                         Handler = () =>
                                         {
                                                                           [First]
                                                                           void Local()
                                                                           {
                                                                           }
                                         }
                                     };
                                 }
                                 internal sealed class Config
                                 {
                                     public System.Action Handler { get; set; }
                                 }
                                 sealed class FirstAttribute : System.Attribute
                                 {
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5513MethodAttributesMustFollowPlacementRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5513MessageFormat));
    }

    /// <summary>
    /// Verifies the same anchor-column alignment inside an anonymous object, which is recognized by
    /// <see cref="Reihitsu.Core.SyntaxIndentationUtilities.ComputeBaseIndentLevel"/> exactly like an object
    /// initializer
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCodeFixAlignsToAttributeListColumnInsideAnonymousObject()
    {
        const string testData = """
                                internal class Example
                                {
                                    private readonly object _config = new
                                    {
                                        Handler = (System.Action)(() =>
                                        {
                                                                          {|#0:[First]|} void Local()
                                                                          {
                                                                          }
                                        })
                                    };
                                }
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     private readonly object _config = new
                                     {
                                         Handler = (System.Action)(() =>
                                         {
                                                                           [First]
                                                                           void Local()
                                                                           {
                                                                           }
                                         })
                                     };
                                 }
                                 sealed class FirstAttribute : System.Attribute
                                 {
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5513MethodAttributesMustFollowPlacementRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5513MessageFormat));
    }

    /// <summary>
    /// Verifies that compliant code is not flagged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForCompliantCode()
    {
        const string testData = """
                                internal class Example
                                {
                                    [First]
                                    internal void M() { }
                                }
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                sealed class SecondAttribute : System.Attribute
                                {
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that commented violations are still reported without offering an unsafe code fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticWithoutCodeFixWhenCommentsArePresent()
    {
        const string testData = """
                                internal class Example
                                {
                                    {|#0:[First /* keep */]|} internal void M() { }
                                }
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                sealed class SecondAttribute : System.Attribute
                                {
                                }
                                """;
        const string codeFixData = """
                                   internal class Example
                                   {
                                       [First /* keep */] internal void M() { }
                                   }
                                   sealed class FirstAttribute : System.Attribute
                                   {
                                   }
                                   sealed class SecondAttribute : System.Attribute
                                   {
                                   }
                                   """;

        await Verify(testData,
                     Diagnostics(RH5513MethodAttributesMustFollowPlacementRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5513MessageFormat));

        var actions = await GetCodeFixActionsAsync(codeFixData,
                                                   RH5513MethodAttributesMustFollowPlacementRulesAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<AttributeListSyntax>()
                                                               .First()
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                internal class Example
                                {
                                    {|#0:[First]|} {|#1:[Second]|} internal void M() { }
                                }
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                sealed class SecondAttribute : System.Attribute
                                {
                                }
                                """;

        const string fixedCode = """
                                 internal class Example
                                 {
                                     [First]
                                     [Second]
                                     internal void M() { }
                                 }
                                 sealed class FirstAttribute : System.Attribute
                                 {
                                 }
                                 sealed class SecondAttribute : System.Attribute
                                 {
                                 }
                                 """;

        // The two attribute lists already share one line: under WellKnownFixAllProviders.BatchFixer both fixes
        // are computed against the same original document. The second list's own leading trivia is empty there
        // (the intervening whitespace belongs to the first list's close bracket's trailing trivia instead),
        // which used to make its derived indentation zero-width
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH5513MethodAttributesMustFollowPlacementRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5513MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}