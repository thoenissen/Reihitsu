using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5501AssemblyAttributesMustFollowPlacementRulesAnalyzer"/> and <see cref="RH5501AssemblyAttributesMustFollowPlacementRulesCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5501AssemblyAttributesMustFollowPlacementRulesAnalyzerTests : AnalyzerTestsBase<RH5501AssemblyAttributesMustFollowPlacementRulesAnalyzer, RH5501AssemblyAttributesMustFollowPlacementRulesCodeFixProvider>
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
                                {|#0:[assembly: First]|} internal class Example { }
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                sealed class SecondAttribute : System.Attribute
                                {
                                }
                                """;
        const string fixedData = """
                                 [assembly: First]
                                 internal class Example { }
                                 sealed class FirstAttribute : System.Attribute
                                 {
                                 }
                                 sealed class SecondAttribute : System.Attribute
                                 {
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5501AssemblyAttributesMustFollowPlacementRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5501MessageFormat));
    }

    /// <summary>
    /// Verifies that the code fix does not insert a blank line when the declaration that follows has a non-empty body
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCodeFixDoesNotInsertBlankLineWhenDeclarationHasBody()
    {
        const string testData = """
                                {|#0:[assembly: First]|} internal class Example
                                {
                                    private void Run()
                                    {
                                    }
                                }
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                """;
        const string fixedData = """
                                 [assembly: First]
                                 internal class Example
                                 {
                                     private void Run()
                                     {
                                     }
                                 }
                                 sealed class FirstAttribute : System.Attribute
                                 {
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5501AssemblyAttributesMustFollowPlacementRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5501MessageFormat));
    }

    /// <summary>
    /// Verifies that the code fix does not insert a blank line when only the second of two attribute lists violates
    /// the policy
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCodeFixDoesNotInsertBlankLineWhenSecondAttributeListViolates()
    {
        const string testData = """
                                [assembly: First]
                                {|#0:[assembly: Second]|} internal class Example { }
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                sealed class SecondAttribute : System.Attribute
                                {
                                }
                                """;
        const string fixedData = """
                                 [assembly: First]
                                 [assembly: Second]
                                 internal class Example { }
                                 sealed class FirstAttribute : System.Attribute
                                 {
                                 }
                                 sealed class SecondAttribute : System.Attribute
                                 {
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5501AssemblyAttributesMustFollowPlacementRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5501MessageFormat));
    }

    /// <summary>
    /// Verifies that Fix All converges to a single line break per attribute list when both lists on the same line
    /// violate the policy
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFixAllDoesNotInsertBlankLineForTwoViolatingAttributeListsOnOneLine()
    {
        const string testData = """
                                {|#0:[assembly: First]|} {|#1:[assembly: Second]|} internal class Example { }
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                sealed class SecondAttribute : System.Attribute
                                {
                                }
                                """;
        const string fixedData = """
                                 [assembly: First]
                                 [assembly: Second]
                                 internal class Example { }
                                 sealed class FirstAttribute : System.Attribute
                                 {
                                 }
                                 sealed class SecondAttribute : System.Attribute
                                 {
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5501AssemblyAttributesMustFollowPlacementRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5501MessageFormat, 2));
    }

    /// <summary>
    /// Verifies that compliant code is not flagged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForCompliantCode()
    {
        const string testData = """
                                [assembly: First]
                                internal class Example { }
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
                                {|#0:[assembly: First /* keep */]|} internal class Example { }
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                sealed class SecondAttribute : System.Attribute
                                {
                                }
                                """;
        const string codeFixData = """
                                   [assembly: First /* keep */] internal class Example { }
                                   sealed class FirstAttribute : System.Attribute
                                   {
                                   }
                                   sealed class SecondAttribute : System.Attribute
                                   {
                                   }
                                   """;

        await Verify(testData,
                     Diagnostics(RH5501AssemblyAttributesMustFollowPlacementRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5501MessageFormat));

        var actions = await GetCodeFixActionsAsync(codeFixData,
                                                   RH5501AssemblyAttributesMustFollowPlacementRulesAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<AttributeListSyntax>()
                                                               .First()
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    #endregion // Tests
}