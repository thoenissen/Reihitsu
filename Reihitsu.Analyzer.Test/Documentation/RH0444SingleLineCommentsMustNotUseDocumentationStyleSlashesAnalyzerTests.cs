using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH0444SingleLineCommentsMustNotUseDocumentationStyleSlashesAnalyzer"/> and
/// <see cref="RH0444SingleLineCommentsMustNotUseDocumentationStyleSlashesCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0444SingleLineCommentsMustNotUseDocumentationStyleSlashesAnalyzerTests : AnalyzerTestsBase<RH0444SingleLineCommentsMustNotUseDocumentationStyleSlashesAnalyzer, RH0444SingleLineCommentsMustNotUseDocumentationStyleSlashesCodeFixProvider>
{
    #region Members

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

        await Verify(source, fixedSource, Diagnostics(RH0444SingleLineCommentsMustNotUseDocumentationStyleSlashesAnalyzer.DiagnosticId, AnalyzerResources.RH0444MessageFormat));
    }

    #endregion // Members
}