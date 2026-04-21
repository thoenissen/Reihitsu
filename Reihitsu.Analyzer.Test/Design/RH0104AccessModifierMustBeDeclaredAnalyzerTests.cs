using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Design;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Design;

/// <summary>
/// Test methods for <see cref="RH0104AccessModifierMustBeDeclaredAnalyzer"/> and <see cref="RH0104AccessModifierMustBeDeclaredCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0104AccessModifierMustBeDeclaredAnalyzerTests : AnalyzerTestsBase<RH0104AccessModifierMustBeDeclaredAnalyzer, RH0104AccessModifierMustBeDeclaredCodeFixProvider>
{
    /// <summary>
    /// Verifying that declarations without explicit accessibility trigger diagnostics and are fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticsAndCodeFix()
    {
        const string testData = """
                                using System;

                                namespace Reihitsu.Analyzer.Test.Design.Resources;

                                class {|#0:MissingAccessClass|} : IDisposable
                                {
                                    string {|#1:field|};

                                    void {|#2:DoWork|}()
                                    {
                                    }

                                    class {|#3:NestedClass|}
                                    {
                                    }

                                    void IDisposable.Dispose()
                                    {
                                    }
                                }

                                internal interface IContract
                                {
                                    void InterfaceMethod();

                                    class NestedInterfaceType
                                    {
                                        void NestedInterfaceMethod()
                                        {
                                        }
                                    }
                                }
                                """;

        const string resultData = """
                                  using System;

                                  namespace Reihitsu.Analyzer.Test.Design.Resources;

                                  internal class MissingAccessClass : IDisposable
                                  {
                                      private string field;

                                      private void DoWork()
                                      {
                                      }

                                      private class NestedClass
                                      {
                                      }

                                      void IDisposable.Dispose()
                                      {
                                      }
                                  }

                                  internal interface IContract
                                  {
                                      void InterfaceMethod();

                                      class NestedInterfaceType
                                      {
                                          void NestedInterfaceMethod()
                                          {
                                          }
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH0104AccessModifierMustBeDeclaredAnalyzer.DiagnosticId, AnalyzerResources.RH0104MessageFormat, 4));
    }

    /// <summary>
    /// Verifying that partial methods without explicit accessibility are allowed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyPartialMethodsWithoutModifierDoNotTriggerDiagnostics()
    {
        const string testData = """
                                namespace Reihitsu.Analyzer.Test.Design.Resources;

                                internal partial class Sample
                                {
                                    partial void OnChanged();
                                }

                                internal partial class Sample
                                {
                                    partial void OnChanged()
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }
}