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
    /// Verifying that a partial type without an explicit accessibility adopts the accessibility declared on
    /// another part instead of the syntactic default, so the fixed code does not introduce a conflicting
    /// modifier (CS0262)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyPartialTypeAdoptsPublicAccessibilityDeclaredOnAnotherPart()
    {
        const string testData = """
                                namespace Reihitsu.Analyzer.Test.Design.Resources;

                                public partial class Sample
                                {
                                }

                                partial class {|#0:Sample|}
                                {
                                }
                                """;

        const string resultData = """
                                  namespace Reihitsu.Analyzer.Test.Design.Resources;

                                  public partial class Sample
                                  {
                                  }

                                  public partial class Sample
                                  {
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH2004AccessModifierMustBeDeclaredAnalyzer.DiagnosticId, AnalyzerResources.RH2004MessageFormat, 1));
    }

    /// <summary>
    /// Verifying that a nested partial type adopts a compound accessibility (<see langword="protected"/>
    /// <see langword="internal"/>) declared on another part
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNestedPartialTypeAdoptsProtectedInternalAccessibilityDeclaredOnAnotherPart()
    {
        const string testData = """
                                namespace Reihitsu.Analyzer.Test.Design.Resources;

                                internal partial class Outer
                                {
                                    protected internal partial class Inner
                                    {
                                    }
                                }

                                internal partial class Outer
                                {
                                    partial class {|#0:Inner|}
                                    {
                                    }
                                }
                                """;

        const string resultData = """
                                  namespace Reihitsu.Analyzer.Test.Design.Resources;

                                  internal partial class Outer
                                  {
                                      protected internal partial class Inner
                                      {
                                      }
                                  }

                                  internal partial class Outer
                                  {
                                      protected internal partial class Inner
                                      {
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH2004AccessModifierMustBeDeclaredAnalyzer.DiagnosticId, AnalyzerResources.RH2004MessageFormat, 1));
    }

    /// <summary>
    /// Verifying that a single-part partial type at namespace level keeps the existing <see langword="internal"/>
    /// default
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifySinglePartPartialTypeAtNamespaceLevelReceivesInternal()
    {
        const string testData = """
                                namespace Reihitsu.Analyzer.Test.Design.Resources;

                                partial class {|#0:Sample|}
                                {
                                }
                                """;

        const string resultData = """
                                  namespace Reihitsu.Analyzer.Test.Design.Resources;

                                  internal partial class Sample
                                  {
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH2004AccessModifierMustBeDeclaredAnalyzer.DiagnosticId, AnalyzerResources.RH2004MessageFormat, 1));
    }

    /// <summary>
    /// Verifying that a nested single-part partial type keeps the existing <see langword="private"/> default
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNestedSinglePartPartialTypeReceivesPrivate()
    {
        const string testData = """
                                namespace Reihitsu.Analyzer.Test.Design.Resources;

                                internal class Outer
                                {
                                    partial class {|#0:Inner|}
                                    {
                                    }
                                }
                                """;

        const string resultData = """
                                  namespace Reihitsu.Analyzer.Test.Design.Resources;

                                  internal class Outer
                                  {
                                      private partial class Inner
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

    /// <summary>
    /// Verifying a file-scoped class does not trigger diagnostics, since <see langword="file"/> is the only
    /// accessibility a file-local type can declare and an additional modifier would not compile (CS9052)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFileScopedClassDoesNotTriggerDiagnostics()
    {
        const string testData = """
                                namespace Reihitsu.Analyzer.Test.Design.Resources;

                                file class Sample
                                {
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifying a file-scoped struct does not trigger diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFileScopedStructDoesNotTriggerDiagnostics()
    {
        const string testData = """
                                namespace Reihitsu.Analyzer.Test.Design.Resources;

                                file struct Sample
                                {
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifying a file-scoped interface does not trigger diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFileScopedInterfaceDoesNotTriggerDiagnostics()
    {
        const string testData = """
                                namespace Reihitsu.Analyzer.Test.Design.Resources;

                                file interface ISample
                                {
                                }
                                """;

        await Verify(testData);
    }

    #endregion // Tests
}