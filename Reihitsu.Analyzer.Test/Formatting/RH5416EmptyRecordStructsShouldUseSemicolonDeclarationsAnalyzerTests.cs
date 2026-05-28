using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5416EmptyRecordStructsShouldUseSemicolonDeclarationsAnalyzer"/>
/// </summary>
[TestClass]
public class RH5416EmptyRecordStructsShouldUseSemicolonDeclarationsAnalyzerTests : AnalyzerTestsBase<RH5416EmptyRecordStructsShouldUseSemicolonDeclarationsAnalyzer, RH5416EmptyRecordStructsShouldUseSemicolonDeclarationsCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying that an empty record struct is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyEmptyRecordStructIsDetectedAndFixed()
    {
        const string testData = """
                                internal record struct {|#0:Example|}
                                {
                                }
                                """;
        const string fixedData = """
                                 internal record struct Example;
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5416EmptyRecordStructsShouldUseSemicolonDeclarationsAnalyzer.DiagnosticId, AnalyzerResources.RH5416MessageFormat));
    }

    /// <summary>
    /// Verifying that semicolon record struct declarations are not flagged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifySemicolonRecordStructIsNotFlagged()
    {
        const string testData = """
                                internal record struct Example;
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifying that directive-containing record structs are reported without offering an unsafe code fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDirectiveContainingRecordStructIsReportedWithoutCodeFix()
    {
        const string testData = """
                                internal record struct {|#0:Example|}
                                {
                                #if DEBUG
                                #endif
                                }
                                """;
        const string codeFixData = """
                                   internal record struct Example
                                   {
                                   #if DEBUG
                                   #endif
                                   }
                                   """;

        await Verify(testData,
                     Diagnostics(RH5416EmptyRecordStructsShouldUseSemicolonDeclarationsAnalyzer.DiagnosticId, AnalyzerResources.RH5416MessageFormat));

        var actions = await GetCodeFixActionsAsync(codeFixData,
                                                   RH5416EmptyRecordStructsShouldUseSemicolonDeclarationsAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<RecordDeclarationSyntax>()
                                                               .Single()
                                                               .Identifier
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    #endregion // Tests
}