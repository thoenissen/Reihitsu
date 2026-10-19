using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5504ModuleAttributeListsMustFollowShapeRulesAnalyzer"/> and <see cref="RH5504ModuleAttributeListsMustFollowShapeRulesCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5504ModuleAttributeListsMustFollowShapeRulesAnalyzerTests : BatchCodeFixTestsBase<RH5504ModuleAttributeListsMustFollowShapeRulesAnalyzer, RH5504ModuleAttributeListsMustFollowShapeRulesCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that policy violations are detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5504-001")]
    [Requirement("RH5504-002")]
    [Requirement("RH5504-003")]
    [Requirement("SHARED-ATTR-013")]
    [Requirement("SHARED-ATTR-016")]
    [Requirement("SHARED-ATTR-017")]
    public async Task VerifyDiagnosticAndCodeFixForPolicyViolation()
    {
        const string testData = """
                                {|#0:[module: First, Second]|}
                                internal class Example { }
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                sealed class SecondAttribute : System.Attribute
                                {
                                }
                                """;
        const string fixedData = """
                                 [module: First]
                                 [module: Second]
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
                     Diagnostics(RH5504ModuleAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5504MessageFormat));
    }

    /// <summary>
    /// Verifies that compliant code is not flagged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5504-001")]
    [Requirement("SHARED-ATTR-013")]
    public async Task VerifyNoDiagnosticForCompliantCode()
    {
        const string testData = """
                                [module: First]
                                [module: Second]
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
    [Requirement("RH5504-004")]
    [Requirement("SHARED-ATTR-019")]
    public async Task VerifyDiagnosticWithoutCodeFixWhenCommentsArePresent()
    {
        const string testData = """
                                {|#0:[module: First, /* keep */ Second]|}
                                internal class Example { }
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                sealed class SecondAttribute : System.Attribute
                                {
                                }
                                """;
        const string codeFixData = """
                                   [module: First, /* keep */ Second]
                                   internal class Example { }
                                   sealed class FirstAttribute : System.Attribute
                                   {
                                   }
                                   sealed class SecondAttribute : System.Attribute
                                   {
                                   }
                                   """;

        await Verify(testData,
                     Diagnostics(RH5504ModuleAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5504MessageFormat));

        var actions = await GetCodeFixActionsAsync(codeFixData,
                                                   RH5504ModuleAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<AttributeListSyntax>()
                                                               .First()
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifies that a merged module-specifier list on an accessor of a single-line property is not reported, because the formatter keeps the list inline and merged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForMergedModuleSpecifierListOnSingleLinePropertyAccessor()
    {
        const string testData = """
                                internal class Example
                                {
                                    public int Value { [module: First, Second] get; set; }
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
    /// Verifies that only the module attribute list that holds several attributes is reported, on the whole list
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5504-001")]
    [Requirement("RH5504-002")]
    [Requirement("SHARED-ATTR-013")]
    [Requirement("SHARED-ATTR-016")]
    public async Task VerifyDiagnosticOnlyForMultiAttributeModuleList()
    {
        const string testData = """
                                {|#0:[module: A, B]|}
                                [module: D]
                                [module: E]
                                sealed class A : System.Attribute
                                {
                                }
                                sealed class B : System.Attribute
                                {
                                }
                                sealed class D : System.Attribute
                                {
                                }
                                sealed class E : System.Attribute
                                {
                                }
                                """;

        await Verify(testData,
                     Diagnostics(RH5504ModuleAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5504MessageFormat));
    }

    /// <summary>
    /// Verifies that the code fix replaces the module attribute list by one list per attribute, each on its own line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5504-003")]
    [Requirement("SHARED-ATTR-017")]
    public async Task VerifyCodeFixGivesEachModuleAttributeAListOfItsOwn()
    {
        const string testData = """
                                {|#0:[module: A, B]|}
                                class C
                                {
                                }
                                sealed class A : System.Attribute
                                {
                                }
                                sealed class B : System.Attribute
                                {
                                }
                                """;
        const string fixedData = """
                                 [module: A]
                                 [module: B]
                                 class C
                                 {
                                 }
                                 sealed class A : System.Attribute
                                 {
                                 }
                                 sealed class B : System.Attribute
                                 {
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5504ModuleAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5504MessageFormat));
    }

    /// <summary>
    /// Verifies that the code fix replaces the module attribute list by one list per attribute, each on its own line, under CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5504-003")]
    [Requirement("SHARED-ATTR-017")]
    public async Task VerifyCodeFixGivesEachModuleAttributeAListOfItsOwnWithCarriageReturnLineFeed()
    {
        const string testData = """
                                {|#0:[module: A, B]|}
                                class C
                                {
                                }
                                sealed class A : System.Attribute
                                {
                                }
                                sealed class B : System.Attribute
                                {
                                }
                                """;
        const string fixedData = """
                                 [module: A]
                                 [module: B]
                                 class C
                                 {
                                 }
                                 sealed class A : System.Attribute
                                 {
                                 }
                                 sealed class B : System.Attribute
                                 {
                                 }
                                 """;

        await Verify(NormalizeToCarriageReturnLineFeed(testData),
                     NormalizeToCarriageReturnLineFeed(fixedData),
                     Diagnostics(RH5504ModuleAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5504MessageFormat));
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                {|#0:[module: First, Second]|} {|#1:[module: Third, Fourth]|}
                                internal class Example { }
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                sealed class SecondAttribute : System.Attribute
                                {
                                }
                                sealed class ThirdAttribute : System.Attribute
                                {
                                }
                                sealed class FourthAttribute : System.Attribute
                                {
                                }
                                """;

        const string fixedCode = """
                                 [module: First]
                                 [module: Second]
                                 [module: Third]
                                 [module: Fourth]
                                 internal class Example { }
                                 sealed class FirstAttribute : System.Attribute
                                 {
                                 }
                                 sealed class SecondAttribute : System.Attribute
                                 {
                                 }
                                 sealed class ThirdAttribute : System.Attribute
                                 {
                                 }
                                 sealed class FourthAttribute : System.Attribute
                                 {
                                 }
                                 """;

        // Two multi-attribute module lists sit back to back on the same compilation unit: each fix computes its
        // split against the same original document and both target the same compilation-unit attribute-list run,
        // so the batch fixer discards one of the two conflicting changes in its first pass and needs a second
        // pass to correct the diagnostic that survives
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH5504ModuleAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5504MessageFormat, 2),
                                  Configure: config => config.NumberOfFixAllIterations = 2);
    }

    #endregion // BatchCodeFixTestsBase
}