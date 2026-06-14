using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Organization;
using Reihitsu.Analyzer.Rules.Organization;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH7301RegionsShouldMatchAnalyzer"/> and <see cref="RH7301RegionsShouldMatchCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH7301RegionsShouldMatchAnalyzerTests : AnalyzerTestsBase<RH7301RegionsShouldMatchAnalyzer, RH7301RegionsShouldMatchCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying that mismatched region and endregion descriptions are detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMismatchedRegionsAreDetectedAndFixed()
    {
        const string testData = """
                                using System;
                                using System.Collections.Generic;
                                using System.Text;

                                namespace Reihitsu.Analyzer.Test.Design.Resources
                                {
                                    internal class RH2001
                                    {
                                        #region Fields
                                        private bool _field;
                                        {|#0:#endregion Fields|}
                                        #region Properties

                                        public bool Property { get { return _field; } }

                                        {|#1:#endregion // Fields|}

                                        #region Methods

                                        public bool GetValue() => _field;

                                        #endregion // Methods

                                        #region Outer 1

                                        #region Inner 1

                                        #endregion // Inner 1

                                        #endregion // Outer 1

                                        #region Outer 2

                                        #region Inner 2

                                        {|#2:#endregion|}

                                        #endregion // Outer 2

                                        #region Outer 3

                                        #region Inner 3

                                        #endregion // Inner 3

                                        {|#3:#endregion|}
                                    }
                                }
                                """;

        const string resultData = """
                                  using System;
                                  using System.Collections.Generic;
                                  using System.Text;

                                  namespace Reihitsu.Analyzer.Test.Design.Resources
                                  {
                                      internal class RH2001
                                      {
                                          #region Fields
                                          private bool _field;
                                          #endregion // Fields
                                          #region Properties

                                          public bool Property { get { return _field; } }

                                          #endregion // Properties

                                          #region Methods

                                          public bool GetValue() => _field;

                                          #endregion // Methods

                                          #region Outer 1

                                          #region Inner 1

                                          #endregion // Inner 1

                                          #endregion // Outer 1

                                          #region Outer 2

                                          #region Inner 2

                                          #endregion // Inner 2

                                          #endregion // Outer 2

                                          #region Outer 3

                                          #region Inner 3

                                          #endregion // Inner 3

                                          #endregion // Outer 3
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH7301RegionsShouldMatchAnalyzer.DiagnosticId, AnalyzerResources.RH7301MessageFormat, 4));
    }

    /// <summary>
    /// Verifies that the synthesized endregion comment uses the document's detected CRLF end-of-line sequence
    /// instead of <see cref="System.Environment.NewLine"/>, so the fix does not introduce mixed line endings
    /// (issue #257)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifySynthesizedEndRegionCommentUsesDetectedCarriageReturnLineFeedEndOfLine()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    #region Methods

                                    public void Method()
                                    {
                                    }

                                    #endregion
                                }
                                """;

        var fixedSource = await ApplyCodeFixAsync(NormalizeToCarriageReturnLineFeed(testData));

        Assert.Contains(" // Methods\r\n", fixedSource);
    }

    #endregion // Tests
}