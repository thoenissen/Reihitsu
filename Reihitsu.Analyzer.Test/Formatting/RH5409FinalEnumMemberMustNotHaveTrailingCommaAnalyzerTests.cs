using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5409FinalEnumMemberMustNotHaveTrailingCommaAnalyzer"/> and <see cref="RH5409FinalEnumMemberMustNotHaveTrailingCommaCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5409FinalEnumMemberMustNotHaveTrailingCommaAnalyzerTests : AnalyzerTestsBase<RH5409FinalEnumMemberMustNotHaveTrailingCommaAnalyzer, RH5409FinalEnumMemberMustNotHaveTrailingCommaCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that trailing commas on final enum members are detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyTrailingCommaOnFinalEnumMemberIsDetectedAndFixed()
    {
        const string testData = """
                                internal enum RH5409
                                {
                                    First,
                                    Second{|#0:,|}
                                }
                                """;
        const string fixedData = """
                                 internal enum RH5409
                                 {
                                     First,
                                     Second
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5409FinalEnumMemberMustNotHaveTrailingCommaAnalyzer.DiagnosticId, AnalyzerResources.RH5409MessageFormat));
    }

    /// <summary>
    /// Verifies that a trailing comma on a single enum member is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyTrailingCommaOnSingleEnumMemberIsDetectedAndFixed()
    {
        const string testData = """
                                internal enum RH5409
                                {
                                    Value{|#0:,|}
                                }
                                """;
        const string fixedData = """
                                 internal enum RH5409
                                 {
                                     Value
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5409FinalEnumMemberMustNotHaveTrailingCommaAnalyzer.DiagnosticId, AnalyzerResources.RH5409MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment after the trailing comma is preserved by the code fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyTrailingCommaWithSameLineCommentIsDetectedAndFixed()
    {
        const string testData = """
                                internal enum RH5409
                                {
                                    Value{|#0:,|} // Comment
                                }
                                """;
        const string fixedData = """
                                 internal enum RH5409
                                 {
                                     Value // Comment
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5409FinalEnumMemberMustNotHaveTrailingCommaAnalyzer.DiagnosticId, AnalyzerResources.RH5409MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment placed before a comma-only line is preserved by the code fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyTrailingCommaOnOwnLineAfterCommentIsDetectedAndFixed()
    {
        const string testData = """
                                internal enum RH5409
                                {
                                    Value
                                    // Comment
                                    {|#0:,|}
                                }
                                """;
        const string fixedData = """
                                 internal enum RH5409
                                 {
                                     Value
                                     // Comment
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5409FinalEnumMemberMustNotHaveTrailingCommaAnalyzer.DiagnosticId, AnalyzerResources.RH5409MessageFormat));
    }

    /// <summary>
    /// Verifies that the enum code fix removes only the trailing comma and does not reformat surrounding code
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyTrailingCommaIsRemovedSurgically()
    {
        const string testData = """
                                internal enum RH5409
                                {
                                    Value{|#0:,|} // Comment
                                }

                                internal class Example
                                {
                                    private static void Method(){ }
                                }
                                """;
        const string fixedData = """
                                 internal enum RH5409
                                 {
                                     Value // Comment
                                 }

                                 internal class Example
                                 {
                                     private static void Method(){ }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5409FinalEnumMemberMustNotHaveTrailingCommaAnalyzer.DiagnosticId, AnalyzerResources.RH5409MessageFormat));
    }

    /// <summary>
    /// Verifies that multiple trailing commas are detected
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMultipleEnumsWithTrailingCommasAreDetected()
    {
        const string testData = """
                                internal enum FirstEnum
                                {
                                    Value{|#0:,|}
                                }
                                
                                internal enum SecondEnum
                                {
                                    Other{|#1:,|}
                                }
                                """;

        await Verify(testData, Diagnostics(RH5409FinalEnumMemberMustNotHaveTrailingCommaAnalyzer.DiagnosticId, AnalyzerResources.RH5409MessageFormat, 2));
    }

    /// <summary>
    /// Verifies that enums without trailing commas are not flagged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyEnumWithoutTrailingCommaIsNotFlagged()
    {
        const string testData = """
                                internal enum RH5409
                                {
                                    First,
                                    Second
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that empty enums are not flagged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyEmptyEnumIsNotFlagged()
    {
        const string testData = """
                                internal enum RH5409
                                {
                                }
                                """;

        await Verify(testData);
    }

    #endregion // Tests
}