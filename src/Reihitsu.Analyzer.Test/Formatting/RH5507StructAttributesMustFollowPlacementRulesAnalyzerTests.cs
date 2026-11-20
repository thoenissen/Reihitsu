using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5507StructAttributesMustFollowPlacementRulesAnalyzer"/> and <see cref="RH5507StructAttributesMustFollowPlacementRulesCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5507StructAttributesMustFollowPlacementRulesAnalyzerTests : BatchCodeFixTestsBase<RH5507StructAttributesMustFollowPlacementRulesAnalyzer, RH5507StructAttributesMustFollowPlacementRulesCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that policy violations are detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5507-001")]
    [Requirement("RH5507-002")]
    [Requirement("RH5507-003")]
    public async Task VerifyDiagnosticAndCodeFixForPolicyViolation()
    {
        const string testData = """
                                {|#0:[First]|} internal struct Example { }
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                sealed class SecondAttribute : System.Attribute
                                {
                                }
                                """;
        const string fixedData = """
                                 [First]
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
                     Diagnostics(RH5507StructAttributesMustFollowPlacementRulesAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that compliant code is not flagged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5507-001")]
    public async Task VerifyNoDiagnosticForCompliantCode()
    {
        const string testData = """
                                [First]
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
    [Requirement("RH5507-004")]
    [Requirement("SHARED-ATTR-011")]
    public async Task VerifyDiagnosticWithoutCodeFixWhenCommentsArePresent()
    {
        const string testData = """
                                {|#0:[First /* keep */]|} internal struct Example { }
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                sealed class SecondAttribute : System.Attribute
                                {
                                }
                                """;
        const string codeFixData = """
                                   [First /* keep */] internal struct Example { }
                                   sealed class FirstAttribute : System.Attribute
                                   {
                                   }
                                   sealed class SecondAttribute : System.Attribute
                                   {
                                   }
                                   """;

        await Verify(testData,
                     Diagnostics(RH5507StructAttributesMustFollowPlacementRulesAnalyzer.DiagnosticId, MessageFormat));

        var actions = await GetCodeFixActionsAsync(codeFixData,
                                                   RH5507StructAttributesMustFollowPlacementRulesAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<AttributeListSyntax>()
                                                               .First()
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifies that an attribute list of a record struct is checked as a struct attribute list
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5507-001")]
    [Requirement("SHARED-ATTR-001")]
    public async Task VerifyDiagnosticAndCodeFixForRecordStruct()
    {
        const string testData = """
                                {|#0:[First]|} internal record struct Example;
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                """;
        const string fixedData = """
                                 [First]
                                 internal record struct Example;
                                 sealed class FirstAttribute : System.Attribute
                                 {
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5507StructAttributesMustFollowPlacementRulesAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies the union example of RH5507-001: an attribute list of a union that shares its line with the union is
    /// reported, and the fix moves the union onto the next line
    /// </summary>
    /// <param name="endOfLine">Line break of the source and the expected result</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5507-001")]
    [Requirement("RH5507-002")]
    [Requirement("RH5507-003")]
    [DataRow("\n", DisplayName = "LF")]
    [DataRow("\r\n", DisplayName = "CRLF")]
    public async Task UnionAttributeListOnDeclarationLineIsReportedAndMoved(string endOfLine)
    {
        const string testCode = """
                                using System;

                                {|RH5507:[Serializable]|} union U(int, string);

                                [Serializable]
                                union V(int, string);

                                public class C
                                {
                                    {|RH5507:[type: Serializable]|} union N(int, string);
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 [Serializable]
                                 union U(int, string);

                                 [Serializable]
                                 union V(int, string);

                                 public class C
                                 {
                                     [type: Serializable]
                                     union N(int, string);
                                 }
                                 """;

        var unionSupport = await UnionSupport.GetReferenceAsync();

        await Verify(ConvertEndOfLine(testCode, endOfLine), ConvertEndOfLine(fixedCode, endOfLine), test => test.TestState.AdditionalReferences.Add(unionSupport));
    }

    /// <summary>
    /// Verifies the struct example of RH5507-001 with the fix of RH5507-003: the list of a struct and the list of a
    /// record struct declared with a semicolon behind it are reported and moved, and the list on its own line is not
    /// reported
    /// </summary>
    /// <param name="endOfLine">Line break of the source and the expected result</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5507-001")]
    [Requirement("RH5507-002")]
    [Requirement("RH5507-003")]
    [DataRow("\n", DisplayName = "LF")]
    [DataRow("\r\n", DisplayName = "CRLF")]
    public async Task StructSpecificationExampleIsReportedAndFixed(string endOfLine)
    {
        const string testCode = """
                                {|RH5507:[A]|} struct S
                                {
                                }

                                {|RH5507:[A]|} record struct R;

                                [A]
                                struct T
                                {
                                }

                                sealed class AAttribute : System.Attribute
                                {
                                }
                                """;
        const string fixedCode = """
                                 [A]
                                 struct S
                                 {
                                 }

                                 [A]
                                 record struct R;

                                 [A]
                                 struct T
                                 {
                                 }

                                 sealed class AAttribute : System.Attribute
                                 {
                                 }
                                 """;

        await Verify(ConvertEndOfLine(testCode, endOfLine), ConvertEndOfLine(fixedCode, endOfLine));
    }

    /// <summary>
    /// Verifies that the list of a struct declared with a semicolon is reported and moved when other code precedes it
    /// </summary>
    /// <param name="endOfLine">Line break of the source and the expected result</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5507-001")]
    [Requirement("RH5507-003")]
    [DataRow("\n", DisplayName = "LF")]
    [DataRow("\r\n", DisplayName = "CRLF")]
    public async Task StructDeclaredWithSemicolonAfterOtherCodeIsReportedAndMoved(string endOfLine)
    {
        const string testCode = """
                                using System;

                                {|RH5507:[Serializable]|} internal struct Empty;
                                """;
        const string fixedCode = """
                                 using System;

                                 [Serializable]
                                 internal struct Empty;
                                 """;

        await Verify(ConvertEndOfLine(testCode, endOfLine), ConvertEndOfLine(fixedCode, endOfLine));
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                {|#0:[First]|} {|#1:[Second]|} internal struct Example { }
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                sealed class SecondAttribute : System.Attribute
                                {
                                }
                                """;

        const string fixedCode = """
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

        // Two struct attribute lists share one line: the first fix rewrites the gap up to the second list's
        // opening bracket, and the second fix rewrites the gap up to the struct keyword, so their fix spans abut
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH5507StructAttributesMustFollowPlacementRulesAnalyzer.DiagnosticId, MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}