using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
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
                     Diagnostics(RH5522InterfaceAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, MessageFormat));
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
                     Diagnostics(RH5522InterfaceAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, MessageFormat));

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
                     Diagnostics(RH5522InterfaceAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, MessageFormat));
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
                     Diagnostics(RH5522InterfaceAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the code fix replaces the interface attribute list by one list per attribute, each on its own line, under CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5522-003")]
    [Requirement("SHARED-ATTR-017")]
    [Requirement("SHARED-FIX-008")]
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
                     Diagnostics(RH5522InterfaceAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, MessageFormat));

        Assert.AreEqual(NormalizeToCarriageReturnLineFeed(fixedData), await ApplyCodeFixAsync(NormalizeToCarriageReturnLineFeed(RemoveMarkup(testData))), "Every line break the fix inserts must be CRLF.");
    }

    /// <summary>
    /// Verifies that a list with a conditional directive between its attributes is reported without a code fix, with and without the symbol defined, under LF and CRLF line endings
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-ATTR-019")]
    [Requirement("SHARED-DIAG-008")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task VerifyDiagnosticWithoutCodeFixWhenConditionalDirectiveIsBetweenTheAttributes(string endOfLine)
    {
        const string testData = """
                                {|RH5522:[A,
                                #if DEBUG
                                 B,
                                #endif
                                 C1]|}
                                interface I
                                {
                                }

                                sealed class A : System.Attribute
                                {
                                }
                                sealed class B : System.Attribute
                                {
                                }
                                sealed class C1 : System.Attribute
                                {
                                }
                                """;

        await VerifyNoCodeFixIsOffered(ConvertEndOfLine(testData, endOfLine));
        await VerifyNoCodeFixIsOffered(ConvertEndOfLine(testData, endOfLine), DocumentationMode.Parse, "DEBUG");
    }

    /// <summary>
    /// Verifies that the attribute list of an interface with a region directive pair between its attributes is still reported without offering a code fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5522-004")]
    [Requirement("SHARED-ATTR-019")]
    public async Task VerifyDiagnosticWithoutCodeFixWhenRegionDirectiveIsBetweenTheAttributes()
    {
        const string testData = """
                                {|RH5522:[A,
                                #region Attributes
                                 B
                                #endregion
                                 ]|}
                                interface I
                                {
                                }

                                sealed class A : System.Attribute
                                {
                                }
                                sealed class B : System.Attribute
                                {
                                }
                                sealed class C1 : System.Attribute
                                {
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
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
                                 [Second] [Third]
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

        // Two multi-attribute interface lists sit back to back on one line of the same declaration: each split changes
        // only its own list, so the second list's first attribute stays on the line on which the first list's last
        // attribute now stands.
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH5522InterfaceAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, MessageFormat, 2),
                                  Configure: config => config.NumberOfFixAllIterations = 1);
    }

    #endregion // BatchCodeFixTestsBase
}