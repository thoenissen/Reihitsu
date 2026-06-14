using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Design;
using Reihitsu.Analyzer.Rules.Design;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Design;

/// <summary>
/// Test methods for <see cref="RH2004AccessModifierMustBeDeclaredAnalyzer"/> and <see cref="RH2004AccessModifierMustBeDeclaredCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH2004AccessModifierMustBeDeclaredAnalyzerTests : AnalyzerTestsBase<RH2004AccessModifierMustBeDeclaredAnalyzer, RH2004AccessModifierMustBeDeclaredCodeFixProvider>
{
    #region Tests

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

        await Verify(testData, resultData, Diagnostics(RH2004AccessModifierMustBeDeclaredAnalyzer.DiagnosticId, AnalyzerResources.RH2004MessageFormat, 4));
    }

    /// <summary>
    /// Verifying that a documented member keeps its XML documentation attached after the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDocumentedMemberWithoutModifiersKeepsDocumentationAttached()
    {
        const string testData = """
                                namespace Reihitsu.Analyzer.Test.Design.Resources;

                                internal class Sample
                                {
                                    /// <summary>
                                    /// Doc.
                                    /// </summary>
                                    void {|#0:DoWork|}()
                                    {
                                    }
                                }
                                """;

        const string resultData = """
                                  namespace Reihitsu.Analyzer.Test.Design.Resources;

                                  internal class Sample
                                  {
                                      /// <summary>
                                      /// Doc.
                                      /// </summary>
                                      private void DoWork()
                                      {
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH2004AccessModifierMustBeDeclaredAnalyzer.DiagnosticId, AnalyzerResources.RH2004MessageFormat, 1));
    }

    /// <summary>
    /// Verifying that a documented member with a non-accessibility modifier keeps its XML documentation attached after the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDocumentedMemberWithExistingModifierKeepsDocumentationAttached()
    {
        const string testData = """
                                namespace Reihitsu.Analyzer.Test.Design.Resources;

                                internal class Sample
                                {
                                    /// <summary>
                                    /// Doc.
                                    /// </summary>
                                    static void {|#0:DoWork|}()
                                    {
                                    }
                                }
                                """;

        const string resultData = """
                                  namespace Reihitsu.Analyzer.Test.Design.Resources;

                                  internal class Sample
                                  {
                                      /// <summary>
                                      /// Doc.
                                      /// </summary>
                                      private static void DoWork()
                                      {
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH2004AccessModifierMustBeDeclaredAnalyzer.DiagnosticId, AnalyzerResources.RH2004MessageFormat, 1));
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

    /// <summary>
    /// Verifying static constructors without access modifiers do not trigger diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyStaticConstructorsWithoutModifierDoNotTriggerDiagnostics()
    {
        const string testData = """
                                namespace Reihitsu.Analyzer.Test.Design.Resources;

                                internal class Sample
                                {
                                    static Sample()
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    #endregion // Tests
}