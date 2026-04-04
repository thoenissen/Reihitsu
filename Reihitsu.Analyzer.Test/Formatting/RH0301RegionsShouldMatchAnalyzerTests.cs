using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0301RegionsShouldMatchAnalyzer"/> and <see cref="RH0301RegionsShouldMatchCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0301RegionsShouldMatchAnalyzerTests : AnalyzerTestsBase<RH0301RegionsShouldMatchAnalyzer, RH0301RegionsShouldMatchCodeFixProvider>
{
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
                                    internal class RH0101
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
                                      internal class RH0101
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

        await Verify(testData, resultData, Diagnostics(RH0301RegionsShouldMatchAnalyzer.DiagnosticId, AnalyzerResources.RH0301MessageFormat, 4));
    }
}