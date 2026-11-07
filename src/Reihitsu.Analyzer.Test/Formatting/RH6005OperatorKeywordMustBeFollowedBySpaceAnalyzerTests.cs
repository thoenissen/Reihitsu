using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Spacing;
using Reihitsu.Analyzer.Rules.Spacing;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH6005OperatorKeywordMustBeFollowedBySpaceAnalyzer"/> and <see cref="RH6005OperatorKeywordMustBeFollowedBySpaceCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH6005OperatorKeywordMustBeFollowedBySpaceAnalyzerTests : BatchCodeFixTestsBase<RH6005OperatorKeywordMustBeFollowedBySpaceAnalyzer, RH6005OperatorKeywordMustBeFollowedBySpaceCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that clean code does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6005-002")]
    public async Task VerifyNoDiagnosticsWhenCodeIsClean()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    public static TestClass operator +(TestClass left, TestClass right) => left;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that the issue is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6005-001")]
    [Requirement("RH6005-003")]
    [Requirement("RH6005-004")]
    public async Task VerifyIssueIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    public static TestClass {|#0:operator|}+(TestClass left, TestClass right) => left;
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     public static TestClass operator +(TestClass left, TestClass right) => left;
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH6005OperatorKeywordMustBeFollowedBySpaceAnalyzer.DiagnosticId, AnalyzerResources.RH6005MessageFormat));
    }

    /// <summary>
    /// Verifies that an operator-overload reference inside a documentation-comment cref is not flagged, because
    /// its cref node kind is different from the declaration kinds this rule registers on and is therefore
    /// never dispatched to this rule at all
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6005-002")]
    public async Task VerifyOperatorReferenceInsideCrefIsIgnored()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /// <summary>
                                    /// See <see cref="operator+(TestClass, TestClass)"/>.
                                    /// </summary>
                                    public static TestClass operator +(TestClass left, TestClass right) => left;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a comment alone after the <c>operator</c> keyword of an operator and of a conversion operator
    /// does not separate it, and that the fix inserts the space directly after the keyword, in front of the comment
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6005-001")]
    [Requirement("RH6005-003")]
    [Requirement("RH6005-004")]
    public async Task VerifyCommentAloneAfterOperatorKeywordIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    public static TestClass {|#0:operator|}/* note */-(TestClass left, TestClass right) => left;

                                    public static explicit {|#1:operator|}/* note */long(TestClass value) => 0;
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     public static TestClass operator /* note */-(TestClass left, TestClass right) => left;

                                     public static explicit operator /* note */long(TestClass value) => 0;
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH6005OperatorKeywordMustBeFollowedBySpaceAnalyzer.DiagnosticId, AnalyzerResources.RH6005MessageFormat, 2));
    }

    /// <summary>
    /// Verifies that the <c>operator</c> keyword of a conversion operator followed by one space is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6005-002")]
    public async Task VerifyConversionOperatorFollowedBySpaceIsNotReported()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    public static implicit operator int(TestClass value) => 0;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies an <c>operator</c> keyword directly followed by a documentation comment, which ends
    /// the line, is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6005-001")]
    public async Task VerifyOperatorKeywordFollowedByStrayDocumentationCommentIsNotReported()
    {
        const string testData = """
                                internal class C
                                {
                                    public static C operator/// x
                                    +(C a, C b)
                                    {
                                        return a;
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies the <c>operator</c> keyword of a conversion operator directly followed by a
    /// documentation comment, which ends the line, is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6005-001")]
    public async Task VerifyConversionOperatorKeywordFollowedByStrayDocumentationCommentIsNotReported()
    {
        const string testData = """
                                internal class C
                                {
                                    public static implicit operator/// x
                                        int(C c)
                                    {
                                        return 0;
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies the control of the documentation-comment case: an <c>operator</c> keyword directly followed by a single-line comment, which ends
    /// the line, is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyOperatorKeywordFollowedBySingleLineCommentIsNotReported()
    {
        const string testData = """
                                internal class C
                                {
                                    public static C operator// x
                                    +(C a, C b)
                                    {
                                        return a;
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies the control of the documentation-comment case: the <c>operator</c> keyword of a conversion operator directly followed by a
    /// single-line comment, which ends the line, is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyConversionOperatorKeywordFollowedBySingleLineCommentIsNotReported()
    {
        const string testData = """
                                internal class C
                                {
                                    public static implicit operator// x
                                        int(C c)
                                    {
                                        return 0;
                                    }
                                }
                                """;

        await Verify(testData);
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    public static TestClass {|#0:operator|}+(TestClass left, TestClass right) => left;

                                    public static TestClass {|#1:operator|}-(TestClass left, TestClass right) => left;
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     public static TestClass operator +(TestClass left, TestClass right) => left;

                                     public static TestClass operator -(TestClass left, TestClass right) => left;
                                 }
                                 """;

        // Two operator overloads each carry their own unspaced operator keyword; the fixes only insert a space
        // at their own keyword's boundary, so the batch fixer converges in one pass
        return new FixAllScenario(testData,
                                  fixedData,
                                  Diagnostics(RH6005OperatorKeywordMustBeFollowedBySpaceAnalyzer.DiagnosticId, AnalyzerResources.RH6005MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}