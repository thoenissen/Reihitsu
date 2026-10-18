using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5505ClassAttributesMustFollowPlacementRulesAnalyzer"/> and <see cref="RH5505ClassAttributesMustFollowPlacementRulesCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5505ClassAttributesMustFollowPlacementRulesAnalyzerTests : BatchCodeFixTestsBase<RH5505ClassAttributesMustFollowPlacementRulesAnalyzer, RH5505ClassAttributesMustFollowPlacementRulesCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that policy violations are detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5505-001")]
    [Requirement("RH5505-002")]
    [Requirement("RH5505-003")]
    public async Task VerifyDiagnosticAndCodeFixForPolicyViolation()
    {
        const string testData = """
                                {|#0:[First]|} internal class Example { }
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                sealed class SecondAttribute : System.Attribute
                                {
                                }
                                """;
        const string fixedData = """
                                 [First]
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
                     Diagnostics(RH5505ClassAttributesMustFollowPlacementRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5505MessageFormat));
    }

    /// <summary>
    /// Verifies that compliant code is not flagged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5505-001")]
    public async Task VerifyNoDiagnosticForCompliantCode()
    {
        const string testData = """
                                [First]
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
    [Requirement("RH5505-004")]
    [Requirement("SHARED-ATTR-011")]
    public async Task VerifyDiagnosticWithoutCodeFixWhenCommentsArePresent()
    {
        const string testData = """
                                {|#0:[First /* keep */]|} internal class Example { }
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                sealed class SecondAttribute : System.Attribute
                                {
                                }
                                """;
        const string codeFixData = """
                                   [First /* keep */] internal class Example { }
                                   sealed class FirstAttribute : System.Attribute
                                   {
                                   }
                                   sealed class SecondAttribute : System.Attribute
                                   {
                                   }
                                   """;

        await Verify(testData,
                     Diagnostics(RH5505ClassAttributesMustFollowPlacementRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5505MessageFormat));

        var actions = await GetCodeFixActionsAsync(codeFixData,
                                                   RH5505ClassAttributesMustFollowPlacementRulesAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<AttributeListSyntax>()
                                                               .First()
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifies that the inserted line break matches the document's detected CRLF end-of-line sequence instead of
    /// <see cref="System.Environment.NewLine"/>, so the fix does not introduce mixed line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5505-003")]
    [Requirement("SHARED-ATTR-008")]
    public async Task VerifyInsertedLineBreakUsesDetectedCarriageReturnLineFeedEndOfLine()
    {
        const string testData = """
                                [First] internal class Example
                                {
                                }

                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                """;

        var fixedSource = await ApplyCodeFixAsync(NormalizeToCarriageReturnLineFeed(testData));

        Assert.DoesNotContain("\n", fixedSource.Replace("\r\n", string.Empty));
    }

    /// <summary>
    /// Verifies that a comment between the closing bracket and the member keeps the fix from being offered. The
    /// fix rewrites the trailing trivia of the closing bracket and clears the leading trivia of the member token,
    /// so anything sitting in that gap would be deleted
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-ATTR-005")]
    public async Task VerifyCommentBetweenAttributeListAndMemberIsNotOfferedACodeFix()
    {
        const string codeFixData = """
                                   [First] /* keep me */ internal class Example { }
                                   sealed class FirstAttribute : System.Attribute
                                   {
                                   }
                                   """;

        var actions = await GetCodeFixActionsAsync(codeFixData,
                                                   RH5505ClassAttributesMustFollowPlacementRulesAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<AttributeListSyntax>()
                                                               .First()
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifies that a comment between the closing bracket and the member keeps the diagnostic from being reported.
    /// The fix refuses that gap under either placement, so reporting here would leave a diagnostic nobody can clear
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5505-001")]
    [Requirement("SHARED-ATTR-005")]
    public async Task VerifyNoDiagnosticWhenCommentSitsBetweenAttributeListAndMember()
    {
        const string testData = """
                                [First] /* keep me */ internal class Example { }
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that an attribute list of a record is checked as a class attribute list
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5505-001")]
    [Requirement("SHARED-ATTR-001")]
    public async Task VerifyDiagnosticAndCodeFixForRecordDeclaration()
    {
        const string testData = """
                                {|#0:[First]|} internal record Example;
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                """;
        const string fixedData = """
                                 [First]
                                 internal record Example;
                                 sealed class FirstAttribute : System.Attribute
                                 {
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5505ClassAttributesMustFollowPlacementRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5505MessageFormat));
    }

    /// <summary>
    /// Verifies that an attribute list of a record class is checked as a class attribute list
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5505-001")]
    [Requirement("SHARED-ATTR-001")]
    public async Task VerifyDiagnosticAndCodeFixForRecordClassDeclaration()
    {
        const string testData = """
                                {|#0:[First]|} internal record class Example;
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                """;
        const string fixedData = """
                                 [First]
                                 internal record class Example;
                                 sealed class FirstAttribute : System.Attribute
                                 {
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5505ClassAttributesMustFollowPlacementRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5505MessageFormat));
    }

    /// <summary>
    /// Verifies that an attribute list with the type specifier resolves to the class target
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5505-001")]
    [Requirement("SHARED-ATTR-001")]
    public async Task VerifyDiagnosticAndCodeFixForTypeSpecifier()
    {
        const string testData = """
                                {|#0:[type: First]|} internal class Example { }
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                """;
        const string fixedData = """
                                 [type: First]
                                 internal class Example { }
                                 sealed class FirstAttribute : System.Attribute
                                 {
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5505ClassAttributesMustFollowPlacementRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5505MessageFormat));
    }

    /// <summary>
    /// Verifies that an attribute list of a record struct is left to the struct rule
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5505-001")]
    [Requirement("SHARED-ATTR-001")]
    public async Task VerifyNoDiagnosticForRecordStruct()
    {
        const string testData = """
                                [First] internal record struct Example;
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that the code fix indents the class by one level inside a block-scoped namespace
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5505-003")]
    [Requirement("SHARED-ATTR-008")]
    public async Task VerifyCodeFixIndentsInsideBlockScopedNamespace()
    {
        const string testData = """
                                namespace Sample
                                {
                                    {|#0:[First]|} internal class Example { }
                                }
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                """;
        const string fixedData = """
                                 namespace Sample
                                 {
                                     [First]
                                     internal class Example { }
                                 }
                                 sealed class FirstAttribute : System.Attribute
                                 {
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5505ClassAttributesMustFollowPlacementRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5505MessageFormat));
    }

    /// <summary>
    /// Verifies that the code fix does not indent the class inside a file-scoped namespace, which has no braces
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5505-003")]
    [Requirement("SHARED-ATTR-008")]
    public async Task VerifyCodeFixDoesNotIndentInsideFileScopedNamespace()
    {
        const string testData = """
                                namespace Sample;

                                {|#0:[First]|} internal class Example { }
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                """;
        const string fixedData = """
                                 namespace Sample;

                                 [First]
                                 internal class Example { }
                                 sealed class FirstAttribute : System.Attribute
                                 {
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5505ClassAttributesMustFollowPlacementRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5505MessageFormat));
    }

    /// <summary>
    /// Verifies that a violation with a preprocessor directive inside the attribute list is still reported without offering a code fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5505-004")]
    [Requirement("SHARED-ATTR-011")]
    public async Task VerifyDiagnosticWithoutCodeFixWhenDirectiveIsInsideAttributeList()
    {
        const string testData = """
                                {|#0:[First
                                #if FEATURE
                                , Second
                                #endif
                                ]|} internal class Example { }
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                sealed class SecondAttribute : System.Attribute
                                {
                                }
                                """;
        const string codeFixData = """
                                   [First
                                   #if FEATURE
                                   , Second
                                   #endif
                                   ] internal class Example { }
                                   sealed class FirstAttribute : System.Attribute
                                   {
                                   }
                                   sealed class SecondAttribute : System.Attribute
                                   {
                                   }
                                   """;

        await Verify(testData,
                     Diagnostics(RH5505ClassAttributesMustFollowPlacementRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5505MessageFormat));

        var actions = await GetCodeFixActionsAsync(codeFixData,
                                                   RH5505ClassAttributesMustFollowPlacementRulesAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<AttributeListSyntax>()
                                                               .First()
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                {|#0:[First]|} {|#1:[Second]|} internal class Example { }
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
                                 internal class Example { }
                                 sealed class FirstAttribute : System.Attribute
                                 {
                                 }
                                 sealed class SecondAttribute : System.Attribute
                                 {
                                 }
                                 """;

        // Two class attribute lists share one line: the first fix rewrites the gap up to the second list's
        // opening bracket, and the second fix rewrites the gap up to the class keyword, so their fix spans abut
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH5505ClassAttributesMustFollowPlacementRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5505MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}