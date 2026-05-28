using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Documentation;
using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH8401SingleLineCommentsMustNotUseDocumentationStyleSlashesAnalyzer"/> and
/// <see cref="RH8401SingleLineCommentsMustNotUseDocumentationStyleSlashesCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH8401SingleLineCommentsMustNotUseDocumentationStyleSlashesAnalyzerTests : AnalyzerTestsBase<RH8401SingleLineCommentsMustNotUseDocumentationStyleSlashesAnalyzer, RH8401SingleLineCommentsMustNotUseDocumentationStyleSlashesCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies a diagnostic and code fix for a comment using documentation slashes
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticAndCodeFixForDocumentationStyleComment()
    {
        const string source = """
                              namespace TestNamespace;

                              internal class TestClass
                              {
                                  {|#0:///|} just a comment
                                  internal void TestMethod()
                                  {
                                  }
                              }
                              """;

        const string fixedSource = """
                                   namespace TestNamespace;

                                   internal class TestClass
                                   {
                                       // just a comment
                                       internal void TestMethod()
                                       {
                                       }
                                   }
                                   """;

        await Verify(source, fixedSource, Diagnostics(RH8401SingleLineCommentsMustNotUseDocumentationStyleSlashesAnalyzer.DiagnosticId, AnalyzerResources.RH8401MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported when documentation mode is none
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsWhenDocumentationModeIsNone()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              internal class TestClass
                              {
                                  {|#0:///|} just a comment
                                  internal void TestMethod()
                                  {
                                  }
                              }
                              """;

        await Verify(source, test => test.SolutionTransforms.Add(ApplyDocumentationModeNoneToTestProject));
    }

    #endregion // Tests
}