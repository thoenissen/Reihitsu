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
/// Test methods for <see cref="RH5508StructAttributeListsMustFollowShapeRulesAnalyzer"/> and <see cref="RH5508StructAttributeListsMustFollowShapeRulesCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5508StructAttributeListsMustFollowShapeRulesAnalyzerTests : BatchCodeFixTestsBase<RH5508StructAttributeListsMustFollowShapeRulesAnalyzer, RH5508StructAttributeListsMustFollowShapeRulesCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that policy violations are detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5508-001")]
    [Requirement("RH5508-002")]
    [Requirement("RH5508-003")]
    [Requirement("SHARED-ATTR-013")]
    [Requirement("SHARED-ATTR-016")]
    [Requirement("SHARED-ATTR-017")]
    public async Task VerifyDiagnosticAndCodeFixForPolicyViolation()
    {
        const string testData = """
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
                                 internal struct Example { }
                                 sealed class FirstAttribute : System.Attribute
                                 {
                                 }
                                 sealed class SecondAttribute : System.Attribute
                                 {
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5508StructAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5508MessageFormat));
    }

    /// <summary>
    /// Verifies that compliant code is not flagged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5508-001")]
    [Requirement("SHARED-ATTR-013")]
    public async Task VerifyNoDiagnosticForCompliantCode()
    {
        const string testData = """
                                [First]
                                [Second]
                                internal struct Example { }
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
    [Requirement("RH5508-004")]
    [Requirement("SHARED-ATTR-019")]
    public async Task VerifyDiagnosticWithoutCodeFixWhenCommentsArePresent()
    {
        const string testData = """
                                {|#0:[First, /* keep */ Second]|}
                                internal struct Example { }
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                sealed class SecondAttribute : System.Attribute
                                {
                                }
                                """;
        const string codeFixData = """
                                   [First, /* keep */ Second]
                                   internal struct Example { }
                                   sealed class FirstAttribute : System.Attribute
                                   {
                                   }
                                   sealed class SecondAttribute : System.Attribute
                                   {
                                   }
                                   """;

        await Verify(testData,
                     Diagnostics(RH5508StructAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5508MessageFormat));

        var actions = await GetCodeFixActionsAsync(codeFixData,
                                                   RH5508StructAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<AttributeListSyntax>()
                                                               .First()
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifies that the multi-attribute lists of a struct and of a record struct are reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5508-001")]
    [Requirement("RH5508-002")]
    [Requirement("SHARED-ATTR-013")]
    [Requirement("SHARED-ATTR-016")]
    public async Task VerifyDiagnosticForMultiAttributeListsOfStructsAndRecordStructs()
    {
        const string testData = """
                                {|#0:[A, B]|}
                                struct S
                                {
                                }

                                {|#1:[A, B]|}
                                record struct R;
                                sealed class A : System.Attribute
                                {
                                }
                                sealed class B : System.Attribute
                                {
                                }
                                """;

        await Verify(testData,
                     Diagnostics(RH5508StructAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5508MessageFormat, 2));
    }

    /// <summary>
    /// Verifies that the code fix replaces the struct attribute list by one list per attribute, each on its own line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5508-003")]
    [Requirement("SHARED-ATTR-017")]
    public async Task VerifyCodeFixGivesEachStructAttributeAListOfItsOwn()
    {
        const string testData = """
                                {|#0:[A, B]|}
                                struct S
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
                                 struct S
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
                     Diagnostics(RH5508StructAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5508MessageFormat));
    }

    /// <summary>
    /// Verifies that the code fix replaces the struct attribute list by one list per attribute, each on its own line, under CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5508-003")]
    [Requirement("SHARED-ATTR-017")]
    [Requirement("SHARED-FIX-008")]
    public async Task VerifyCodeFixGivesEachStructAttributeAListOfItsOwnWithCarriageReturnLineFeed()
    {
        const string testData = """
                                {|#0:[A, B]|}
                                struct S
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
                                 struct S
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
                     Diagnostics(RH5508StructAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5508MessageFormat));

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
                                {|RH5508:[A,
                                #if DEBUG
                                 B,
                                #endif
                                 C1]|}
                                struct S
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
    /// Verifies that the attribute list of a struct with a region directive pair between its attributes is still reported without offering a code fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5508-004")]
    [Requirement("SHARED-ATTR-019")]
    public async Task VerifyDiagnosticWithoutCodeFixWhenRegionDirectiveIsBetweenTheAttributes()
    {
        const string testData = """
                                {|RH5508:[A,
                                #region Attributes
                                 B
                                #endregion
                                 ]|}
                                struct S
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

    /// <summary>
    /// Verifies the union example of RH5508-001: an attribute list of a union with two attributes is reported, and the
    /// fix gives each attribute a list of its own
    /// </summary>
    /// <param name="endOfLine">Line break of the source and the expected result</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5508-001")]
    [Requirement("RH5508-002")]
    [Requirement("RH5508-003")]
    [DataRow("\n", DisplayName = "LF")]
    [DataRow("\r\n", DisplayName = "CRLF")]
    public async Task UnionAttributeListWithSeveralAttributesIsReportedAndSplit(string endOfLine)
    {
        const string testCode = """
                                using System;

                                {|RH5508:[Serializable, Obsolete]|}
                                union U(int, string);

                                [Serializable]
                                [Obsolete]
                                union V(int, string);

                                public class C
                                {
                                    {|RH5508:[type: Serializable, Obsolete]|}
                                    union N(int, string);
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 [Serializable]
                                 [Obsolete]
                                 union U(int, string);

                                 [Serializable]
                                 [Obsolete]
                                 union V(int, string);

                                 public class C
                                 {
                                     [type: Serializable]
                                     [type: Obsolete]
                                     union N(int, string);
                                 }
                                 """;

        var unionSupport = await UnionSupport.GetReferenceAsync();

        await Verify(ConvertEndOfLine(testCode, endOfLine), ConvertEndOfLine(fixedCode, endOfLine), test => test.TestState.AdditionalReferences.Add(unionSupport));
    }

    /// <summary>
    /// Verifies that an attribute list of a union with a comment inside is reported and no fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5508-001")]
    [Requirement("RH5508-004")]
    public async Task UnionAttributeListWithCommentInsideIsReportedWithoutFix()
    {
        const string testCode = """
                                using System;

                                {|RH5508:[Serializable /* c */, Obsolete]|}
                                union U(int, string);
                                """;

        var unionSupport = await UnionSupport.GetReferenceAsync();

        await Verify(testCode, testCode, test => test.TestState.AdditionalReferences.Add(unionSupport));
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                {|#0:[First, Second]|} {|#1:[Third, Fourth]|}
                                internal struct Example { }
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
                                 internal struct Example { }
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

        // Two multi-attribute struct lists sit back to back on one line of the same declaration: each split changes
        // only its own list, so the second list's first attribute stays on the line on which the first list's last
        // attribute now stands.
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH5508StructAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5508MessageFormat, 2),
                                  Configure: config => config.NumberOfFixAllIterations = 1);
    }

    #endregion // BatchCodeFixTestsBase
}