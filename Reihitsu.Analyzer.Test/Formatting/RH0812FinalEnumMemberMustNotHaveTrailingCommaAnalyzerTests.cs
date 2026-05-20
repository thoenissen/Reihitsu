using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0812FinalEnumMemberMustNotHaveTrailingCommaAnalyzer"/> and <see cref="RH0812FinalEnumMemberMustNotHaveTrailingCommaCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0812FinalEnumMemberMustNotHaveTrailingCommaAnalyzerTests : AnalyzerTestsBase<RH0812FinalEnumMemberMustNotHaveTrailingCommaAnalyzer, RH0812FinalEnumMemberMustNotHaveTrailingCommaCodeFixProvider>
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
                                internal enum RH0812
                                {
                                    First,
                                    Second{|#0:,|}
                                }
                                """;
        const string fixedData = """
                                 internal enum RH0812
                                 {
                                     First,
                                     Second
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH0812FinalEnumMemberMustNotHaveTrailingCommaAnalyzer.DiagnosticId, AnalyzerResources.RH0812MessageFormat));
    }

    /// <summary>
    /// Verifies that a trailing comma on a single enum member is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyTrailingCommaOnSingleEnumMemberIsDetectedAndFixed()
    {
        const string testData = """
                                internal enum RH0812
                                {
                                    Value{|#0:,|}
                                }
                                """;
        const string fixedData = """
                                 internal enum RH0812
                                 {
                                     Value
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH0812FinalEnumMemberMustNotHaveTrailingCommaAnalyzer.DiagnosticId, AnalyzerResources.RH0812MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment after the trailing comma is preserved by the code fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyTrailingCommaWithSameLineCommentIsDetectedAndFixed()
    {
        const string testData = """
                                internal enum RH0812
                                {
                                    Value{|#0:,|} // Comment
                                }
                                """;
        const string fixedData = """
                                 internal enum RH0812
                                 {
                                     Value // Comment
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH0812FinalEnumMemberMustNotHaveTrailingCommaAnalyzer.DiagnosticId, AnalyzerResources.RH0812MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment placed before a comma-only line is preserved by the code fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyTrailingCommaOnOwnLineAfterCommentIsDetectedAndFixed()
    {
        const string testData = """
                                internal enum RH0812
                                {
                                    Value
                                    // Comment
                                    {|#0:,|}
                                }
                                """;
        const string fixedData = """
                                 internal enum RH0812
                                 {
                                     Value
                                     // Comment
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH0812FinalEnumMemberMustNotHaveTrailingCommaAnalyzer.DiagnosticId, AnalyzerResources.RH0812MessageFormat));
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

        await Verify(testData, Diagnostics(RH0812FinalEnumMemberMustNotHaveTrailingCommaAnalyzer.DiagnosticId, AnalyzerResources.RH0812MessageFormat, 2));
    }

    /// <summary>
    /// Verifies that enums without trailing commas are not flagged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyEnumWithoutTrailingCommaIsNotFlagged()
    {
        const string testData = """
                                internal enum RH0812
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
                                internal enum RH0812
                                {
                                }
                                """;

        await Verify(testData);
    }

    #endregion // Tests
}