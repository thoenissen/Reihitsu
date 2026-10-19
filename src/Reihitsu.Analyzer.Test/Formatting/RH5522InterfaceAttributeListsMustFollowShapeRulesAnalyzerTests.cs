using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5522InterfaceAttributeListsMustFollowShapeRulesAnalyzer"/> and <see cref="RH5522InterfaceAttributeListsMustFollowShapeRulesCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5522InterfaceAttributeListsMustFollowShapeRulesAnalyzerTests : BatchCodeFixTestsBase<RH5522InterfaceAttributeListsMustFollowShapeRulesAnalyzer, RH5522InterfaceAttributeListsMustFollowShapeRulesCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that policy violations are detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5522-001")]
    [Requirement("RH5522-002")]
    [Requirement("RH5522-003")]
    [Requirement("SHARED-ATTR-013")]
    [Requirement("SHARED-ATTR-016")]
    [Requirement("SHARED-ATTR-017")]
    public async Task VerifyDiagnosticAndCodeFixForPolicyViolation()
    {
        const string testData = """
                                {|#0:[First, Second]|}
                                internal interface IExample { }
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
                                 internal interface IExample { }
                                 sealed class FirstAttribute : System.Attribute
                                 {
                                 }
                                 sealed class SecondAttribute : System.Attribute
                                 {
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5522InterfaceAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5522MessageFormat));
    }

    /// <summary>
    /// Verifies that compliant code is not flagged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5522-001")]
    [Requirement("SHARED-ATTR-013")]
    public async Task VerifyNoDiagnosticForCompliantCode()
    {
        const string testData = """
                                [First]
                                [Second]
                                internal interface IExample { }
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
    [Requirement("RH5522-004")]
    [Requirement("SHARED-ATTR-019")]
    public async Task VerifyDiagnosticWithoutCodeFixWhenCommentsArePresent()
    {
        const string testData = """
                                {|#0:[First, /* keep */ Second]|}
                                internal interface IExample { }
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                sealed class SecondAttribute : System.Attribute
                                {
                                }
                                """;
        const string codeFixData = """
                                   [First, /* keep */ Second]
                                   internal interface IExample { }
                                   sealed class FirstAttribute : System.Attribute
                                   {
                                   }
                                   sealed class SecondAttribute : System.Attribute
                                   {
                                   }
                                   """;

        await Verify(testData,
                     Diagnostics(RH5522InterfaceAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5522MessageFormat));

        var actions = await GetCodeFixActionsAsync(codeFixData,
                                                   RH5522InterfaceAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<AttributeListSyntax>()
                                                               .First()
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifies that the multi-attribute list of an interface is reported on the whole list
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5522-001")]
    [Requirement("RH5522-002")]
    [Requirement("SHARED-ATTR-013")]
    [Requirement("SHARED-ATTR-016")]
    public async Task VerifyDiagnosticForMultiAttributeInterfaceList()
    {
        const string testData = """
                                {|#0:[A, B]|}
                                interface I
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
                     Diagnostics(RH5522InterfaceAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5522MessageFormat));
    }

    /// <summary>
    /// Verifies that the code fix replaces the interface attribute list by one list per attribute, each on its own line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5522-003")]
    [Requirement("SHARED-ATTR-017")]
    public async Task VerifyCodeFixGivesEachInterfaceAttributeAListOfItsOwn()
    {
        const string testData = """
                                {|#0:[A, B]|}
                                interface I
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
                                 [A]
                                 [B]
                                 interface I
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
                     Diagnostics(RH5522InterfaceAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5522MessageFormat));
    }

    /// <summary>
    /// Verifies that the code fix replaces the interface attribute list by one list per attribute, each on its own line, under CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5522-003")]
    [Requirement("SHARED-ATTR-017")]
    public async Task VerifyCodeFixGivesEachInterfaceAttributeAListOfItsOwnWithCarriageReturnLineFeed()
    {
        const string testData = """
                                {|#0:[A, B]|}
                                interface I
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
                                 [A]
                                 [B]
                                 interface I
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
                     Diagnostics(RH5522InterfaceAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5522MessageFormat));
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                {|#0:[First, Second]|} {|#1:[Third, Fourth]|}
                                internal interface IExample { }
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
                                 [First]
                                 [Second]
                                 [Third]
                                 [Fourth]
                                 internal interface IExample { }
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

        // Two multi-attribute interface lists sit back to back on the same declaration: each fix computes its
        // split against the same original document and both target the same interface attribute-list run, so
        // the batch fixer discards one of the two conflicting changes in its first pass and needs a second pass
        // to correct the diagnostic that survives
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH5522InterfaceAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5522MessageFormat, 2),
                                  Configure: config => config.NumberOfFixAllIterations = 2);
    }

    #endregion // BatchCodeFixTestsBase
}