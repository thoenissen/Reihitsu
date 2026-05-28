using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5530AccessorAttributesMustFollowPlacementRulesAnalyzer"/> and <see cref="RH5530AccessorAttributesMustFollowPlacementRulesCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5530AccessorAttributesMustFollowPlacementRulesAnalyzerTests : AnalyzerTestsBase<RH5530AccessorAttributesMustFollowPlacementRulesAnalyzer, RH5530AccessorAttributesMustFollowPlacementRulesCodeFixProvider>
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
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                internal class Example
                                {
                                    internal int Value
                                    {
                                        {|#0:[First]|} get;
                                        set;
                                    }
                                }
                                """;
        const string fixedData = """
                                 sealed class FirstAttribute : System.Attribute
                                 {
                                 }
                                 internal class Example
                                 {
                                     internal int Value
                                     {
                                         [First]
                                         get;
                                         set;
                                     }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5530AccessorAttributesMustFollowPlacementRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5530MessageFormat));
    }

    /// <summary>
    /// Verifies that compliant code is not flagged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForCompliantCode()
    {
        const string testData = """
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                internal class Example
                                {
                                    internal int Value
                                    {
                                        [First]
                                        get;
                                        set;
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that single-line accessor attributes remain valid
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForSingleLinePropertyAccessorAttribute()
    {
        const string testData = """
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                internal class Example
                                {
                                    internal int Value { [First] get; set; }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that method attributes are not handled by this accessor rule
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForMethodAttributes()
    {
        const string testData = """
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                internal class Example
                                {
                                    [First] internal void M() { }
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
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                internal class Example
                                {
                                    internal int Value
                                    {
                                        {|#0:[First /* keep */]|} get;
                                        set;
                                    }
                                }
                                """;
        const string codeFixData = """
                                   sealed class FirstAttribute : System.Attribute
                                   {
                                   }
                                   internal class Example
                                   {
                                       internal int Value
                                       {
                                           [First /* keep */] get;
                                           set;
                                       }
                                   }
                                   """;

        await Verify(testData,
                     Diagnostics(RH5530AccessorAttributesMustFollowPlacementRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5530MessageFormat));

        var actions = await GetCodeFixActionsAsync(codeFixData,
                                                   RH5530AccessorAttributesMustFollowPlacementRulesAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<AttributeListSyntax>()
                                                               .First()
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    #endregion // Tests
}