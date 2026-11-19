using System.Threading.Tasks;

using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Documentation;
using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Test methods for <see cref="RH8201InheritdocShouldBeUsedAnalyzer"/> and <see cref="RH8201InheritdocShouldBeUsedCodeFixProvider"/>
/// </summary>
public partial class RH8201InheritdocShouldBeUsedAnalyzerTests
{
    #region Methods

    /// <summary>
    /// Verifies that a documented member re-implementing an interface member with new is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-001")]
    public async Task VerifyDiagnosticForReimplementationWithNew()
    {
        const string testData = """
                                internal interface ITest
                                {
                                    void TestMethod();
                                }

                                internal class TestBase : ITest
                                {
                                    /// <inheritdoc/>
                                    public void TestMethod()
                                    {
                                    }
                                }

                                internal class TestImplementation : TestBase, ITest
                                {
                                    ///{|#0: <summary>Implementation documentation</summary>
                                |}    public new void TestMethod()
                                    {
                                    }
                                }
                                """;

        const string resultData = """
                                  internal interface ITest
                                  {
                                      void TestMethod();
                                  }

                                  internal class TestBase : ITest
                                  {
                                      /// <inheritdoc/>
                                      public void TestMethod()
                                      {
                                      }
                                  }

                                  internal class TestImplementation : TestBase, ITest
                                  {
                                      /// <inheritdoc/>
                                      public new void TestMethod()
                                      {
                                      }
                                  }
                                  """;

        await Verify(testData,
                     resultData,
                     Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, MessageFormat, 1));
    }

    /// <summary>
    /// Verifies that a documented member hiding an inherited implementation is not reported when its type does not list the interface itself
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-002")]
    public async Task VerifyNoDiagnosticForHidingMemberWithoutReimplementation()
    {
        const string testData = """
                                internal interface ITest
                                {
                                    void TestMethod();
                                }

                                internal class TestBase : ITest
                                {
                                    /// <inheritdoc/>
                                    public void TestMethod()
                                    {
                                    }
                                }

                                internal class TestImplementation : TestBase
                                {
                                    /// <summary>Implementation documentation</summary>
                                    public new void TestMethod()
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a documented base member is not reported when only a derived type lists the interface it satisfies
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-002")]
    public async Task VerifyNoDiagnosticForBaseMemberWhenOnlyDerivedTypeListsInterface()
    {
        const string testData = """
                                internal interface ITest
                                {
                                    void TestMethod();
                                }

                                internal class TestBase
                                {
                                    /// <summary>Implementation documentation</summary>
                                    public void TestMethod()
                                    {
                                    }
                                }

                                internal class TestImplementation : TestBase, ITest
                                {
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a documented overload of an implemented member is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-002")]
    public async Task VerifyNoDiagnosticForOverloadOfImplementedMember()
    {
        const string testData = """
                                internal interface ITest
                                {
                                    void TestMethod();
                                }

                                internal class TestImplementation : ITest
                                {
                                    /// <inheritdoc/>
                                    public void TestMethod()
                                    {
                                    }

                                    /// <summary>Implementation documentation</summary>
                                    public void TestMethod(int value)
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a documented public member is not reported when an explicit implementation of the same interface member exists
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-002")]
    public async Task VerifyNoDiagnosticForPublicMemberBesideExplicitImplementation()
    {
        const string testData = """
                                internal interface ITest
                                {
                                    void TestMethod();
                                }

                                internal class TestImplementation : ITest
                                {
                                    /// <inheritdoc/>
                                    void ITest.TestMethod()
                                    {
                                    }

                                    /// <summary>Implementation documentation</summary>
                                    public void TestMethod()
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a documented member of a type implementing an interface is not reported when it implements no interface member
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-002")]
    public async Task VerifyNoDiagnosticForMemberImplementingNoInterface()
    {
        const string testData = """
                                internal interface ITest
                                {
                                    void TestMethod();
                                }

                                internal class TestImplementation : ITest
                                {
                                    /// <inheritdoc/>
                                    public void TestMethod()
                                    {
                                    }

                                    /// <summary>Implementation documentation</summary>
                                    public void OtherMethod()
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that the documented defining declaration of a partial method implementing an interface member is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-001")]
    public async Task VerifyDiagnosticForDocumentedDefiningPartialInterfaceImplementation()
    {
        const string testData = """
                                internal interface ITest
                                {
                                    void TestMethod();
                                }

                                internal partial class TestImplementation : ITest
                                {
                                    ///{|#0: <summary>Implementation documentation</summary>
                                |}    public partial void TestMethod();

                                    public partial void TestMethod()
                                    {
                                    }
                                }
                                """;

        const string resultData = """
                                  internal interface ITest
                                  {
                                      void TestMethod();
                                  }

                                  internal partial class TestImplementation : ITest
                                  {
                                      /// <inheritdoc/>
                                      public partial void TestMethod();

                                      public partial void TestMethod()
                                      {
                                      }
                                  }
                                  """;

        await Verify(testData,
                     resultData,
                     Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, MessageFormat, 1));
    }

    /// <summary>
    /// Verifies that the documented implementing declaration of a partial method implementing an interface member is
    /// detected and fixed, as the defining declaration is
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-001")]
    [Requirement("RH8201-005")]
    public async Task VerifyDiagnosticForDocumentedImplementingPartialInterfaceImplementation()
    {
        const string testData = """
                                internal interface ITest
                                {
                                    void TestMethod();
                                }

                                internal partial class TestImplementation : ITest
                                {
                                    public partial void TestMethod();

                                    ///{|#0: <summary>Implementation documentation</summary>
                                |}    public partial void TestMethod()
                                    {
                                    }
                                }
                                """;

        const string resultData = """
                                  internal interface ITest
                                  {
                                      void TestMethod();
                                  }

                                  internal partial class TestImplementation : ITest
                                  {
                                      public partial void TestMethod();

                                      /// <inheritdoc/>
                                      public partial void TestMethod()
                                      {
                                      }
                                  }
                                  """;

        await Verify(testData,
                     resultData,
                     Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, MessageFormat, 1));
    }

    /// <summary>
    /// Verifies that override, implicit, and explicit interface implementation diagnostics in one document are fixed together
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-001")]
    [Requirement("RH8201-005")]
    public async Task VerifyDiagnosticsForOverrideAndInterfaceImplementationsAreFixedTogether()
    {
        const string testData = """
                                internal interface ITest
                                {
                                    void First();

                                    void Second();
                                }

                                internal abstract class TestBase
                                {
                                    public abstract void Third();
                                }

                                internal class TestImplementation : TestBase, ITest
                                {
                                    ///{|#0: <summary>Implementation documentation</summary>
                                |}    public void First()
                                    {
                                    }

                                    ///{|#1: <summary>Implementation documentation</summary>
                                |}    void ITest.Second()
                                    {
                                    }

                                    ///{|#2: <summary>Implementation documentation</summary>
                                |}    public override void Third()
                                    {
                                    }
                                }
                                """;

        const string resultData = """
                                  internal interface ITest
                                  {
                                      void First();

                                      void Second();
                                  }

                                  internal abstract class TestBase
                                  {
                                      public abstract void Third();
                                  }

                                  internal class TestImplementation : TestBase, ITest
                                  {
                                      /// <inheritdoc/>
                                      public void First()
                                      {
                                      }

                                      /// <inheritdoc/>
                                      void ITest.Second()
                                      {
                                      }

                                      /// <inheritdoc/>
                                      public override void Third()
                                      {
                                      }
                                  }
                                  """;

        await Verify(testData,
                     resultData,
                     Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, MessageFormat, 3));
    }

    /// <summary>
    /// Verifies that a documented implicit interface implementation is reported without a code fix when the interface list of its type is conditionally compiled
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-003")]
    [Requirement("SHARED-DIAG-008")]
    public async Task VerifyDiagnosticWithoutCodeFixForImplicitImplementationWhenInterfaceListIsConditional()
    {
        const string testData = """
                                using System;

                                internal class TestImplementation
                                #if !FEATURE
                                    : IDisposable
                                #endif
                                {
                                    ///{|RH8201: <summary>Implementation documentation</summary>
                                |}    public void Dispose()
                                    {
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
        await VerifyWithPreprocessorSymbols(RemoveMarkup(testData), "FEATURE");
    }

    /// <summary>
    /// Verifies that a documented implicit interface implementation is reported without a code fix when the interface list of its type contains disabled text
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-003")]
    [Requirement("SHARED-DIAG-008")]
    public async Task VerifyDiagnosticWithoutCodeFixForImplicitImplementationWhenAnotherInterfaceIsDisabled()
    {
        const string testData = """
                                using System;

                                internal class TestImplementation : IDisposable
                                #if FEATURE
                                    , ICloneable
                                #endif
                                {
                                    ///{|RH8201: <summary>Implementation documentation</summary>
                                |}    public void Dispose()
                                    {
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that a documented implicit interface implementation is reported without a code fix when another partial declaration of its type has a conditionally compiled interface list
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-003")]
    [Requirement("SHARED-DIAG-008")]
    public async Task VerifyDiagnosticWithoutCodeFixForImplicitImplementationWhenOtherPartialDeclarationIsConditional()
    {
        const string testData = """
                                using System;

                                internal partial class TestImplementation
                                #if !FEATURE
                                    : IDisposable
                                #endif
                                {
                                }

                                internal partial class TestImplementation
                                {
                                    ///{|RH8201: <summary>Implementation documentation</summary>
                                |}    public void Dispose()
                                    {
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that a documented field-like event implementing an interface event is reported without a code fix when the interface list of its type is conditionally compiled
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-003")]
    [Requirement("SHARED-DIAG-008")]
    public async Task VerifyDiagnosticWithoutCodeFixForFieldLikeEventWhenInterfaceListIsConditional()
    {
        const string testData = """
                                using System;

                                internal interface ITest
                                {
                                    event EventHandler TestEvent;
                                }

                                internal class TestImplementation
                                #if !FEATURE
                                    : ITest
                                #endif
                                {
                                    ///{|RH8201: <summary>Implementation documentation</summary>
                                |}    public event EventHandler TestEvent;
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that a documented explicit interface implementation is detected and fixed even when the interface list of its type is conditionally compiled
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-003")]
    public async Task VerifyDiagnosticForExplicitImplementationWhenInterfaceListIsConditional()
    {
        const string testData = """
                                using System;

                                internal class TestImplementation
                                #if !FEATURE
                                    : IDisposable
                                #endif
                                {
                                    ///{|#0: <summary>Implementation documentation</summary>
                                |}    void IDisposable.Dispose()
                                    {
                                    }
                                }
                                """;

        const string resultData = """
                                  using System;

                                  internal class TestImplementation
                                  #if !FEATURE
                                      : IDisposable
                                  #endif
                                  {
                                      /// <inheritdoc/>
                                      void IDisposable.Dispose()
                                      {
                                      }
                                  }
                                  """;

        await Verify(testData,
                     resultData,
                     Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, MessageFormat, 1));
    }

    /// <summary>
    /// Verifies that a documented override is detected and fixed even when the base list of its type is conditionally compiled
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-003")]
    public async Task VerifyDiagnosticForOverrideWhenBaseListIsConditional()
    {
        const string testData = """
                                internal abstract class TestBase
                                {
                                    public abstract void TestMethod();
                                }

                                internal class TestImplementation
                                #if !FEATURE
                                    : TestBase
                                #endif
                                {
                                    ///{|#0: <summary>Implementation documentation</summary>
                                |}    public override void TestMethod()
                                    {
                                    }
                                }
                                """;

        const string resultData = """
                                  internal abstract class TestBase
                                  {
                                      public abstract void TestMethod();
                                  }

                                  internal class TestImplementation
                                  #if !FEATURE
                                      : TestBase
                                  #endif
                                  {
                                      /// <inheritdoc/>
                                      public override void TestMethod()
                                      {
                                      }
                                  }
                                  """;

        await Verify(testData,
                     resultData,
                     Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, MessageFormat, 1));
    }

    /// <summary>
    /// Verifies that a documented implicit interface implementation is detected and fixed when directives appear only inside the type body
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-003")]
    public async Task VerifyDiagnosticForImplicitImplementationWhenDirectiveIsInsideTypeBody()
    {
        const string testData = """
                                using System;

                                internal class TestImplementation : IDisposable
                                {
                                    #region Methods

                                    ///{|#0: <summary>Implementation documentation</summary>
                                |}    public void Dispose()
                                    {
                                    }

                                    #endregion // Methods
                                }
                                """;

        const string resultData = """
                                  using System;

                                  internal class TestImplementation : IDisposable
                                  {
                                      #region Methods

                                      /// <inheritdoc/>
                                      public void Dispose()
                                      {
                                      }

                                      #endregion // Methods
                                  }
                                  """;

        await Verify(testData,
                     resultData,
                     Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, MessageFormat, 1));
    }

    /// <summary>
    /// Verifies that a documented implicit interface implementation is reported without a code fix when a conditional region encloses the partial declaration that lists the interface
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-003")]
    [Requirement("SHARED-DIAG-008")]
    public async Task VerifyDiagnosticWithoutCodeFixWhenConditionalRegionEnclosesPartialDeclaration()
    {
        const string testData = """
                                using System;

                                internal partial class TestImplementation
                                {
                                    ///{|RH8201: <summary>Implementation documentation</summary>
                                |}    public void Dispose()
                                    {
                                    }
                                }

                                #if !FEATURE
                                internal partial class TestImplementation : IDisposable
                                {
                                }
                                #endif
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that a documented implicit interface implementation is reported without a code fix when conditional regions supply alternative headers for its type
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-003")]
    [Requirement("SHARED-DIAG-008")]
    public async Task VerifyDiagnosticWithoutCodeFixWhenConditionalRegionSuppliesAlternativeHeaders()
    {
        const string testData = """
                                using System;

                                #if !FEATURE
                                internal class TestImplementation : IDisposable
                                {
                                #else
                                internal class TestImplementation
                                {
                                #endif
                                    ///{|RH8201: <summary>Implementation documentation</summary>
                                |}    public void Dispose()
                                    {
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that a documented implicit interface implementation is reported without a code fix when the implemented interface member is conditionally compiled
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-003")]
    [Requirement("SHARED-DIAG-008")]
    public async Task VerifyDiagnosticWithoutCodeFixWhenInterfaceMemberIsConditional()
    {
        const string testData = """
                                internal interface ITest
                                {
                                #if !FEATURE
                                    void TestMethod();
                                #endif
                                }

                                internal class TestImplementation : ITest
                                {
                                    ///{|RH8201: <summary>Implementation documentation</summary>
                                |}    public void TestMethod()
                                    {
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that a documented implicit interface implementation is reported without a code fix when the base list of an implemented interface is conditionally compiled
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-003")]
    [Requirement("SHARED-DIAG-008")]
    public async Task VerifyDiagnosticWithoutCodeFixWhenInterfaceBaseListIsConditional()
    {
        const string testData = """
                                internal interface IBase
                                {
                                    void TestMethod();
                                }

                                internal interface IDerived
                                #if !FEATURE
                                    : IBase
                                #endif
                                {
                                }

                                internal class TestImplementation : IDerived
                                {
                                    ///{|RH8201: <summary>Implementation documentation</summary>
                                |}    public void TestMethod()
                                    {
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that a documented implicit interface implementation is reported without a code fix when the file declaring its type contains any conditional region
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-003")]
    [Requirement("SHARED-DIAG-008")]
    public async Task VerifyDiagnosticWithoutCodeFixWhenFileOfTypeContainsUnrelatedConditionalRegion()
    {
        const string testData = """
                                using System;

                                internal class TestImplementation : IDisposable
                                {
                                    ///{|RH8201: <summary>Implementation documentation</summary>
                                |}    public void Dispose()
                                    {
                                #if DEBUG
                                        Console.WriteLine();
                                #endif
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that a documented implicit interface implementation is detected and fixed when the file contains directives other than conditional regions
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-003")]
    public async Task VerifyDiagnosticForImplicitImplementationWhenFileContainsOnlyPragmaDirective()
    {
        const string testData = """
                                using System;

                                internal class TestImplementation : IDisposable
                                {
                                #pragma warning disable CS0168
                                    ///{|#0: <summary>Implementation documentation</summary>
                                |}    public void Dispose()
                                    {
                                    }
                                #pragma warning restore CS0168
                                }
                                """;

        const string resultData = """
                                  using System;

                                  internal class TestImplementation : IDisposable
                                  {
                                  #pragma warning disable CS0168
                                      /// <inheritdoc/>
                                      public void Dispose()
                                      {
                                      }
                                  #pragma warning restore CS0168
                                  }
                                  """;

        await Verify(testData,
                     resultData,
                     Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, MessageFormat, 1));
    }

    /// <summary>
    /// Verifies that a documented implicit interface implementation is reported without a code fix when the file declaring the
    /// implemented interface contains a conditional region, even though the file declaring the type contains none
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-003")]
    [Requirement("SHARED-DIAG-008")]
    public async Task VerifyDiagnosticWithoutCodeFixWhenInterfaceFileContainsConditionalRegion()
    {
        const string testData = """
                                internal class TestImplementation : ITest
                                {
                                    ///{|RH8201: <summary>Implementation documentation</summary>
                                |}    public void TestMethod()
                                    {
                                    }
                                }
                                """;

        const string interfaceData = """
                                     internal interface ITest
                                     {
                                         void TestMethod();
                                     #if FEATURE
                                         void OtherMethod();
                                     #endif
                                     }
                                     """;

        await Verify(testData,
                     testData,
                     test =>
                     {
                         test.TestState.Sources.Add(("/0/Test1.cs", interfaceData));
                         test.FixedState.Sources.Add(("/0/Test1.cs", interfaceData));
                     });
    }

    /// <summary>
    /// Verifies that a documented implicit interface implementation is detected and fixed when only a file that
    /// declares neither its type nor its interfaces contains a conditional region
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-003")]
    public async Task VerifyDiagnosticWhenOnlyUnrelatedFileContainsConditionalRegion()
    {
        const string testData = """
                                internal class TestImplementation : ITest
                                {
                                    ///{|#0: <summary>Implementation documentation</summary>
                                |}    public void TestMethod()
                                    {
                                    }
                                }
                                """;

        const string resultData = """
                                  internal class TestImplementation : ITest
                                  {
                                      /// <inheritdoc/>
                                      public void TestMethod()
                                      {
                                      }
                                  }
                                  """;

        const string interfaceData = """
                                     internal interface ITest
                                     {
                                         void TestMethod();
                                     }
                                     """;

        const string unrelatedData = """
                                     internal class Unrelated
                                     {
                                     #if FEATURE
                                         public void OtherMethod()
                                         {
                                         }
                                     #endif
                                     }
                                     """;

        await Verify(testData,
                     resultData,
                     test =>
                     {
                         test.TestState.Sources.Add(("/0/Test1.cs", interfaceData));
                         test.TestState.Sources.Add(("/0/Test2.cs", unrelatedData));
                         test.FixedState.Sources.Add(("/0/Test1.cs", interfaceData));
                         test.FixedState.Sources.Add(("/0/Test2.cs", unrelatedData));
                     },
                     Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, MessageFormat, 1));
    }

    /// <summary>
    /// Verifies the specification example: the documentation comment of an override, an implicit implementation, and a field-like event whose events all implement interface events is reported, while a nested inheritdoc satisfies the rule
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-001")]
    [Requirement("RH8201-004")]
    public async Task VerifySpecificationExampleReportsDocumentedMembersThatInheritTheirDocumentation()
    {
        const string source = """
                              using System;

                              /// <summary>
                              /// Base
                              /// </summary>
                              public abstract class B
                              {
                                  /// <summary>
                                  /// Runs
                                  /// </summary>
                                  public abstract void Run();

                                  /// <summary>
                                  /// Value
                                  /// </summary>
                                  public abstract int Value { get; }
                              }

                              /// <summary>
                              /// Notifier
                              /// </summary>
                              public interface INotifier
                              {
                                  /// <summary>
                                  /// Raised first
                                  /// </summary>
                                  event EventHandler First;

                                  /// <summary>
                                  /// Raised second
                                  /// </summary>
                                  event EventHandler Second;

                                  /// <summary>
                                  /// Raised third
                                  /// </summary>
                                  event EventHandler Third;
                              }

                              /// <summary>
                              /// Derived
                              /// </summary>
                              public class C : B, IDisposable, INotifier
                              {
                                  ///{|#0: <summary>Raised</summary>
                              |}    public event EventHandler First, Second;

                                  /// <summary>
                                  /// Raised as well
                                  /// </summary>
                                  public event EventHandler Third, Fourth;

                                  ///{|#1: <summary>Runs here</summary>
                              |}    public override void Run()
                                  {
                                  }

                                  /// <inheritdoc/>
                                  /// <remarks>
                                  /// <inheritdoc/>
                                  /// </remarks>
                                  public override int Value => 0;

                                  ///{|#2: <summary>Disposes</summary>
                              |}    public void Dispose()
                                  {
                                  }

                                  public override string ToString() => "";
                              }
                              """;

        await Verify(source, Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, MessageFormat, 3));
    }

    /// <summary>
    /// Verifies the specification example: an overload, a member beside an explicit implementation, a base member only a derived type maps, an interface member hiding a base interface member, and an explicit implementation with inheritdoc are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-002")]
    public async Task VerifySpecificationExampleDoesNotReportMembersThatOnlyResembleInheritedOnes()
    {
        const string source = """
                              using System;

                              /// <summary>
                              /// Contract
                              /// </summary>
                              public interface I
                              {
                                  /// <summary>
                                  /// Runs
                                  /// </summary>
                                  void Run();
                              }

                              /// <summary>
                              /// Derived contract
                              /// </summary>
                              public interface J : I
                              {
                                  /// <summary>
                                  /// Runs differently
                                  /// </summary>
                                  new void Run();
                              }

                              /// <summary>
                              /// Base
                              /// </summary>
                              public class B
                              {
                                  /// <summary>
                                  /// Runs
                                  /// </summary>
                                  public void Run()
                                  {
                                  }
                              }

                              /// <summary>
                              /// Derived
                              /// </summary>
                              public class D : B, I
                              {
                              }

                              /// <summary>
                              /// Service
                              /// </summary>
                              public class C : I
                              {
                                  /// <inheritdoc/>
                                  void I.Run()
                                  {
                                  }

                                  /// <summary>
                                  /// Runs as well
                                  /// </summary>
                                  public void Run()
                                  {
                                  }

                                  /// <summary>
                                  /// Runs with a value
                                  /// </summary>
                                  /// <param name="value">Value</param>
                                  public void Run(int value)
                                  {
                                  }
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies the specification example: with an #if in the file of the type, the override and the implicit
    /// implementation are both reported; the fix replaces the documentation of the override and is withheld for the
    /// implicit implementation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-003")]
    [Requirement("RH8201-004")]
    [Requirement("SHARED-DIAG-008")]
    public async Task VerifySpecificationExampleConditionalInterfaceListWithholdsTheFixOnlyForImplicitImplementations()
    {
        const string source = """
                              using System;

                              /// <summary>
                              /// Base
                              /// </summary>
                              public abstract class B
                              {
                                  /// <summary>
                                  /// Runs
                                  /// </summary>
                                  public abstract void Run();
                              }

                              /// <summary>
                              /// Service
                              /// </summary>
                              public class C : B, IDisposable
                              #if FEATURE
                                  , ICloneable
                              #endif
                              {
                                  ///{|#0: <summary>Runs here</summary>
                              |}    public override void Run()
                                  {
                                  }

                                  ///{|RH8201: <summary>
                                  /// Disposes
                                  /// </summary>
                              |}    public void Dispose()
                                  {
                                  }

                              #if FEATURE
                                  /// <inheritdoc/>
                                  public object Clone() => this;
                              #endif
                              }
                              """;

        const string fixedSource = """
                                   using System;

                                   /// <summary>
                                   /// Base
                                   /// </summary>
                                   public abstract class B
                                   {
                                       /// <summary>
                                       /// Runs
                                       /// </summary>
                                       public abstract void Run();
                                   }

                                   /// <summary>
                                   /// Service
                                   /// </summary>
                                   public class C : B, IDisposable
                                   #if FEATURE
                                       , ICloneable
                                   #endif
                                   {
                                       /// <inheritdoc/>
                                       public override void Run()
                                       {
                                       }

                                       ///{|RH8201: <summary>
                                       /// Disposes
                                       /// </summary>
                                   |}    public void Dispose()
                                       {
                                       }

                                   #if FEATURE
                                       /// <inheritdoc/>
                                       public object Clone() => this;
                                   #endif
                                   }
                                   """;

        await Verify(source,
                     fixedSource,
                     test =>
                     {
                         // The diagnostic whose fix is withheld stays and is marked in the expected source
                         test.FixedState.MarkupHandling = MarkupMode.Allow;
                         test.BatchFixedState.MarkupHandling = MarkupMode.Allow;
                     },
                     Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies the specification example: a region directive in the file does not keep an implicit implementation from being reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-003")]
    [Requirement("RH8201-004")]
    public async Task VerifySpecificationExampleRegionDirectiveDoesNotSuppressImplicitImplementation()
    {
        const string source = """
                              using System;

                              /// <summary>
                              /// Service
                              /// </summary>
                              public class C : IDisposable
                              {
                                  #region Methods

                                  ///{|#0: <summary>Disposes</summary>
                              |}    public void Dispose()
                                  {
                                  }

                                  #endregion
                              }
                              """;

        await Verify(source, Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies the specification example: the fix replaces a single-line and a delimited documentation comment with inheritdoc, keeping the ordinary comment before it, the attribute list after it, and the comment that followed the closing delimiter on its own line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-004")]
    [Requirement("RH8201-005")]
    public async Task VerifySpecificationExampleReplacesCommentWithInheritdoc()
    {
        const string source = """
                              /// <summary>
                              /// Base
                              /// </summary>
                              public abstract class B
                              {
                                  /// <summary>
                                  /// Runs
                                  /// </summary>
                                  public abstract void Run();

                                  /// <summary>
                                  /// Stops
                                  /// </summary>
                                  public abstract void Stop();
                              }

                              /// <summary>
                              /// Derived
                              /// </summary>
                              public class C : B
                              {
                                  // Ordinary comment
                                  ///{|#0: <summary>
                                  /// Runs here
                                  /// </summary>
                                  /// <remarks>
                                  /// Differs
                                  /// </remarks>
                              |}    [System.Obsolete]
                                  public override void Run()
                                  {
                                  }

                                  /**{|#1: <summary>Stops here</summary> */|} // Trailing
                                  public override void Stop()
                                  {
                                  }
                              }
                              """;

        const string fixedSource = """
                                   /// <summary>
                                   /// Base
                                   /// </summary>
                                   public abstract class B
                                   {
                                       /// <summary>
                                       /// Runs
                                       /// </summary>
                                       public abstract void Run();

                                       /// <summary>
                                       /// Stops
                                       /// </summary>
                                       public abstract void Stop();
                                   }

                                   /// <summary>
                                   /// Derived
                                   /// </summary>
                                   public class C : B
                                   {
                                       // Ordinary comment
                                       /// <inheritdoc/>
                                       [System.Obsolete]
                                       public override void Run()
                                       {
                                       }

                                       /// <inheritdoc/>
                                       // Trailing
                                       public override void Stop()
                                       {
                                       }
                                   }
                                   """;

        await Verify(source, fixedSource, Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, MessageFormat, 2));
    }

    /// <summary>
    /// Verifies the fix keeps preprocessor directives at column 0 before and after the replaced comment in the leading trivia of the member
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-005")]
    public async Task VerifyFixKeepsColumnZeroDirectivesAroundComment()
    {
        const string source = """
                              /// <summary>Base</summary>
                              public abstract class B
                              {
                                  /// <summary>Runs</summary>
                                  public abstract void Run();
                              }

                              /// <summary>Derived</summary>
                              public class C : B
                              {
                              #pragma warning disable CS0618
                                  ///{|#0: <summary>Runs here</summary>
                              |}#pragma warning restore CS0618
                                  public override void Run()
                                  {
                                  }
                              }
                              """;

        const string fixedSource = """
                                   /// <summary>Base</summary>
                                   public abstract class B
                                   {
                                       /// <summary>Runs</summary>
                                       public abstract void Run();
                                   }

                                   /// <summary>Derived</summary>
                                   public class C : B
                                   {
                                   #pragma warning disable CS0618
                                       /// <inheritdoc/>
                                   #pragma warning restore CS0618
                                       public override void Run()
                                       {
                                       }
                                   }
                                   """;

        await Verify(source, fixedSource, Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies the specification example with an indentation other than four spaces per level: the fix keeps the
    /// indented directives, the member lines, and the braces in their columns, and puts a member that followed the
    /// closing delimiter on the line after the inheritdoc line, in the column of the opening delimiter
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-005")]
    public async Task VerifySpecificationExampleKeepsIndentationDirectivesAndMemberBehindDelimitedComment()
    {
        await Verify(IndentedSpecificationExampleSource, IndentedSpecificationExampleFixedSource, Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, MessageFormat, 2));
    }

    /// <summary>
    /// Verifies the specification example with an indentation other than four spaces per level in a document whose
    /// line breaks are CRLF: every line break of the result is CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-005")]
    public async Task VerifySpecificationExampleKeepsIndentationDirectivesAndMemberBehindDelimitedCommentWithCarriageReturnLineFeed()
    {
        await Verify(NormalizeToCarriageReturnLineFeed(IndentedSpecificationExampleSource),
                     NormalizeToCarriageReturnLineFeed(IndentedSpecificationExampleFixedSource),
                     Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, MessageFormat, 2));
    }

    /// <summary>
    /// Verifies the first specification example of the fix in a document whose line breaks are CRLF: the ordinary
    /// comment, the attribute list, and the comment that followed the closing delimiter keep their lines, and every
    /// line break of the result is CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-005")]
    public async Task VerifySpecificationExampleReplacesCommentWithInheritdocWithCarriageReturnLineFeed()
    {
        var source = NormalizeToCarriageReturnLineFeed("""
                                                       /// <summary>
                                                       /// Base
                                                       /// </summary>
                                                       public abstract class B
                                                       {
                                                           /// <summary>
                                                           /// Runs
                                                           /// </summary>
                                                           public abstract void Run();

                                                           /// <summary>
                                                           /// Stops
                                                           /// </summary>
                                                           public abstract void Stop();
                                                       }

                                                       /// <summary>
                                                       /// Derived
                                                       /// </summary>
                                                       public class C : B
                                                       {
                                                           // Ordinary comment
                                                           ///{|#0: <summary>
                                                           /// Runs here
                                                           /// </summary>
                                                           /// <remarks>
                                                           /// Differs
                                                           /// </remarks>
                                                       |}    [System.Obsolete]
                                                           public override void Run()
                                                           {
                                                           }

                                                           /**{|#1: <summary>Stops here</summary> */|} // Trailing
                                                           public override void Stop()
                                                           {
                                                           }
                                                       }
                                                       """);
        var fixedSource = NormalizeToCarriageReturnLineFeed("""
                                                            /// <summary>
                                                            /// Base
                                                            /// </summary>
                                                            public abstract class B
                                                            {
                                                                /// <summary>
                                                                /// Runs
                                                                /// </summary>
                                                                public abstract void Run();

                                                                /// <summary>
                                                                /// Stops
                                                                /// </summary>
                                                                public abstract void Stop();
                                                            }

                                                            /// <summary>
                                                            /// Derived
                                                            /// </summary>
                                                            public class C : B
                                                            {
                                                                // Ordinary comment
                                                                /// <inheritdoc/>
                                                                [System.Obsolete]
                                                                public override void Run()
                                                                {
                                                                }

                                                                /// <inheritdoc/>
                                                                // Trailing
                                                                public override void Stop()
                                                                {
                                                                }
                                                            }
                                                            """);

        await Verify(source, fixedSource, Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, MessageFormat, 2));
    }

    /// <summary>
    /// Verifies that the fix keeps every line break of a CRLF document as CRLF when nothing but the reported comment
    /// stands in the member's leading trivia
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-005")]
    public async Task VerifyFixKeepsCarriageReturnLineFeedOfPlainMember()
    {
        var source = NormalizeToCarriageReturnLineFeed("""
                                                       /// <summary>Base</summary>
                                                       public abstract class B
                                                       {
                                                           /// <summary>Runs</summary>
                                                           public abstract void Run();
                                                       }

                                                       /// <summary>Derived</summary>
                                                       public class C : B
                                                       {
                                                           ///{|#0: <summary>
                                                           /// Runs here
                                                           /// </summary>
                                                       |}    public override void Run()
                                                           {
                                                           }
                                                       }
                                                       """);
        var fixedSource = NormalizeToCarriageReturnLineFeed("""
                                                            /// <summary>Base</summary>
                                                            public abstract class B
                                                            {
                                                                /// <summary>Runs</summary>
                                                                public abstract void Run();
                                                            }

                                                            /// <summary>Derived</summary>
                                                            public class C : B
                                                            {
                                                                /// <inheritdoc/>
                                                                public override void Run()
                                                                {
                                                                }
                                                            }
                                                            """);

        await Verify(source, fixedSource, Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix keeps an indented region directive and the blank line behind it in front of the replaced
    /// comment, with every line break of a CRLF document kept as CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-005")]
    public async Task VerifyFixKeepsIndentedRegionAndBlankLineWithCarriageReturnLineFeed()
    {
        var source = NormalizeToCarriageReturnLineFeed("""
                                                       /// <summary>Base</summary>
                                                       public abstract class B
                                                       {
                                                           /// <summary>Runs</summary>
                                                           public abstract void Run();
                                                       }

                                                       /// <summary>Derived</summary>
                                                       public class C : B
                                                       {
                                                           #region B

                                                           ///{|#0: <summary>
                                                           /// Runs here
                                                           /// </summary>
                                                       |}    public override void Run()
                                                           {
                                                           }

                                                           #endregion // B
                                                       }
                                                       """);
        var fixedSource = NormalizeToCarriageReturnLineFeed("""
                                                            /// <summary>Base</summary>
                                                            public abstract class B
                                                            {
                                                                /// <summary>Runs</summary>
                                                                public abstract void Run();
                                                            }

                                                            /// <summary>Derived</summary>
                                                            public class C : B
                                                            {
                                                                #region B

                                                                /// <inheritdoc/>
                                                                public override void Run()
                                                                {
                                                                }

                                                                #endregion // B
                                                            }
                                                            """);

        await Verify(source, fixedSource, Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix keeps an indented nullable directive and an indented delimited ordinary comment between
    /// the replaced comment and the member in their columns
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-005")]
    public async Task VerifyFixKeepsIndentedNullableDirectiveAndDelimitedCommentBehindComment()
    {
        const string source = """
                              /// <summary>Base</summary>
                              public abstract class B
                              {
                                  /// <summary>Runs</summary>
                                  public abstract void Run();
                              }

                              /// <summary>Derived</summary>
                              public class C : B
                              {
                                  ///{|#0: <summary>Runs here</summary>
                              |}    #nullable enable
                                  /* Note */
                                  public override void Run()
                                  {
                                  }
                              }
                              """;
        const string fixedSource = """
                                   /// <summary>Base</summary>
                                   public abstract class B
                                   {
                                       /// <summary>Runs</summary>
                                       public abstract void Run();
                                   }

                                   /// <summary>Derived</summary>
                                   public class C : B
                                   {
                                       /// <inheritdoc/>
                                       #nullable enable
                                       /* Note */
                                       public override void Run()
                                       {
                                       }
                                   }
                                   """;

        await Verify(source, fixedSource, Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix keeps an indented if group with the code it excludes, and its else and endif
    /// directives, in front of the replaced comment in their columns
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-005")]
    public async Task VerifyFixKeepsIndentedConditionalGroupWithExcludedCodeInFrontOfComment()
    {
        const string source = """
                              /// <summary>Base</summary>
                              public abstract class B
                              {
                                  /// <summary>Runs</summary>
                                  public abstract void Run();
                              }

                              /// <summary>Derived</summary>
                              public class C : B
                              {
                                  #if NEVER
                                  int _excluded;
                                  #else
                                  #endif
                                  ///{|#0: <summary>Runs here</summary>
                              |}    public override void Run()
                                  {
                                  }
                              }
                              """;
        const string fixedSource = """
                                   /// <summary>Base</summary>
                                   public abstract class B
                                   {
                                       /// <summary>Runs</summary>
                                       public abstract void Run();
                                   }

                                   /// <summary>Derived</summary>
                                   public class C : B
                                   {
                                       #if NEVER
                                       int _excluded;
                                       #else
                                       #endif
                                       /// <inheritdoc/>
                                       public override void Run()
                                       {
                                       }
                                   }
                                   """;

        await Verify(source, fixedSource, Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies the specification example of a partial member judged part by part: both documented parts of a method and
    /// the documented implementing part of a property are reported, and the defining part of the property, which uses
    /// inheritdoc, is not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-001")]
    [Requirement("RH8201-004")]
    public async Task VerifySpecificationExamplePartialMemberIsJudgedPartByPart()
    {
        const string source = """
                              /// <summary>
                              /// Contract
                              /// </summary>
                              public interface I
                              {
                                  /// <summary>
                                  /// Runs
                                  /// </summary>
                                  void M();

                                  /// <summary>
                                  /// Value
                                  /// </summary>
                                  int P { get; }
                              }

                              /// <summary>
                              /// Service
                              /// </summary>
                              public partial class C : I
                              {
                                  ///{|RH8201: <summary>Runs here</summary>
                              |}    public partial void M();

                                  ///{|RH8201: <summary>Runs there</summary>
                              |}    public partial void M()
                                  {
                                  }

                                  /// <inheritdoc/>
                                  public partial int P { get; }

                                  ///{|RH8201: <summary>Value here</summary>
                              |}    public partial int P => 0;
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies the specification example of the fix on a partial member: the fix of each part replaces only that part's
    /// comment, and applying both gives inheritdoc on both parts
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-005")]
    public async Task VerifySpecificationExampleFixOfPartialMemberReplacesEachPart()
    {
        const string testData = """
                                /// <summary>
                                /// Contract
                                /// </summary>
                                public interface I
                                {
                                    /// <summary>
                                    /// Runs
                                    /// </summary>
                                    void M();
                                }

                                /// <summary>
                                /// Service
                                /// </summary>
                                public partial class C : I
                                {
                                    ///{|RH8201: <summary>Runs here</summary>
                                |}    public partial void M();

                                    ///{|RH8201: <summary>Runs there</summary>
                                |}    public partial void M()
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 /// <summary>
                                 /// Contract
                                 /// </summary>
                                 public interface I
                                 {
                                     /// <summary>
                                     /// Runs
                                     /// </summary>
                                     void M();
                                 }

                                 /// <summary>
                                 /// Service
                                 /// </summary>
                                 public partial class C : I
                                 {
                                     /// <inheritdoc/>
                                     public partial void M();

                                     /// <inheritdoc/>
                                     public partial void M()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData);
    }

    /// <summary>
    /// Verifies that the fix of the implementing part of a partial method replaces only that part's comment and leaves the
    /// defining part, which stays reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-001")]
    [Requirement("RH8201-005")]
    public async Task VerifyFixOfImplementingPartLeavesDefiningPartReported()
    {
        const string testData = """
                                /// <summary>
                                /// Contract
                                /// </summary>
                                public interface I
                                {
                                    /// <summary>
                                    /// Runs
                                    /// </summary>
                                    void M();
                                }

                                /// <summary>
                                /// Service
                                /// </summary>
                                public partial class C : I
                                {
                                    /// <summary>Runs here</summary>
                                    public partial void M();

                                    /// <summary>Runs there</summary>
                                    public partial void M()
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 /// <summary>
                                 /// Contract
                                 /// </summary>
                                 public interface I
                                 {
                                     /// <summary>
                                     /// Runs
                                     /// </summary>
                                     void M();
                                 }

                                 /// <summary>
                                 /// Service
                                 /// </summary>
                                 public partial class C : I
                                 {
                                     ///{|RH8201: <summary>Runs here</summary>
                                 |}    public partial void M();

                                     /// <inheritdoc/>
                                     public partial void M()
                                     {
                                     }
                                 }
                                 """;
        const string implementingComment = " <summary>Runs there</summary>\n";
        var implementingStart = testData.IndexOf(implementingComment, System.StringComparison.Ordinal);

        Assert.IsGreaterThanOrEqualTo(0, implementingStart);

        const string markedData = """
                                  /// <summary>
                                  /// Contract
                                  /// </summary>
                                  public interface I
                                  {
                                      /// <summary>
                                      /// Runs
                                      /// </summary>
                                      void M();
                                  }

                                  /// <summary>
                                  /// Service
                                  /// </summary>
                                  public partial class C : I
                                  {
                                      ///{|RH8201: <summary>Runs here</summary>
                                  |}    public partial void M();

                                      ///{|RH8201: <summary>Runs there</summary>
                                  |}    public partial void M()
                                      {
                                      }
                                  }
                                  """;

        await Verify(markedData);

        var actual = await TryApplyCodeFixAtAsync(testData,
                                                  RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId,
                                                  root => Microsoft.CodeAnalysis.Location.Create(root.SyntaxTree, new Microsoft.CodeAnalysis.Text.TextSpan(implementingStart, implementingComment.Length)));

        Assert.AreEqual(RemoveMarkup(fixedData), actual);

        await Verify(fixedData);
    }

    /// <summary>
    /// Verifies that the documented parts of a partial method whose defining part uses inheritdoc and whose implementing
    /// part carries a summary are judged on their own: only the implementing part is reported, and the mirrored shape
    /// reports only the defining part
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-001")]
    public async Task VerifyOnlyThePartWithoutInheritdocOfPartialMethodIsReported()
    {
        const string source = """
                              /// <summary>
                              /// Contract
                              /// </summary>
                              public interface I
                              {
                                  /// <summary>
                                  /// Runs
                                  /// </summary>
                                  void M();

                                  /// <summary>
                                  /// Stops
                                  /// </summary>
                                  void N();
                              }

                              /// <summary>
                              /// Service
                              /// </summary>
                              public partial class C : I
                              {
                                  /// <inheritdoc/>
                                  public partial void M();

                                  ///{|RH8201: <summary>Runs there</summary>
                              |}    public partial void M()
                                  {
                                  }

                                  ///{|RH8201: <summary>Stops here</summary>
                              |}    public partial void N();

                                  /// <inheritdoc/>
                                  public partial void N()
                                  {
                                  }
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies that both documented parts of a partial property and of a partial indexer implementing interface members
    /// are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-001")]
    [Requirement("RH8201-005")]
    public async Task VerifyBothPartsOfPartialPropertyAndIndexerAreDetectedAndFixed()
    {
        const string testData = """
                                /// <summary>
                                /// Contract
                                /// </summary>
                                public interface I
                                {
                                    /// <summary>
                                    /// Value
                                    /// </summary>
                                    int P { get; }

                                    /// <summary>
                                    /// Item
                                    /// </summary>
                                    /// <param name="index">Index</param>
                                    int this[int index] { get; }
                                }

                                /// <summary>
                                /// Service
                                /// </summary>
                                public partial class C : I
                                {
                                    ///{|RH8201: <summary>Value here</summary>
                                |}    public partial int P { get; }

                                    ///{|RH8201: <summary>Value there</summary>
                                |}    public partial int P => 0;

                                    ///{|RH8201: <summary>Item here</summary>
                                |}    public partial int this[int index] { get; }

                                    ///{|RH8201: <summary>Item there</summary>
                                |}    public partial int this[int index] => index;
                                }
                                """;
        const string fixedData = """
                                 /// <summary>
                                 /// Contract
                                 /// </summary>
                                 public interface I
                                 {
                                     /// <summary>
                                     /// Value
                                     /// </summary>
                                     int P { get; }

                                     /// <summary>
                                     /// Item
                                     /// </summary>
                                     /// <param name="index">Index</param>
                                     int this[int index] { get; }
                                 }

                                 /// <summary>
                                 /// Service
                                 /// </summary>
                                 public partial class C : I
                                 {
                                     /// <inheritdoc/>
                                     public partial int P { get; }

                                     /// <inheritdoc/>
                                     public partial int P => 0;

                                     /// <inheritdoc/>
                                     public partial int this[int index] { get; }

                                     /// <inheritdoc/>
                                     public partial int this[int index] => index;
                                 }
                                 """;

        await Verify(testData, fixedData);
    }

    /// <summary>
    /// Verifies that both documented parts of a partial event implementing an interface event, whose defining part is a
    /// field-like event, are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-001")]
    [Requirement("RH8201-005")]
    public async Task VerifyBothPartsOfPartialEventAreDetectedAndFixed()
    {
        const string testData = """
                                using System;

                                /// <summary>
                                /// Contract
                                /// </summary>
                                public interface I
                                {
                                    /// <summary>
                                    /// Raised
                                    /// </summary>
                                    event EventHandler E;
                                }

                                /// <summary>
                                /// Service
                                /// </summary>
                                public partial class C : I
                                {
                                    ///{|RH8201: <summary>Raised here</summary>
                                |}    public partial event EventHandler E;

                                    ///{|RH8201: <summary>Raised there</summary>
                                |}    public partial event EventHandler E
                                    {
                                        add { }
                                        remove { }
                                    }
                                }
                                """;
        const string fixedData = """
                                 using System;

                                 /// <summary>
                                 /// Contract
                                 /// </summary>
                                 public interface I
                                 {
                                     /// <summary>
                                     /// Raised
                                     /// </summary>
                                     event EventHandler E;
                                 }

                                 /// <summary>
                                 /// Service
                                 /// </summary>
                                 public partial class C : I
                                 {
                                     /// <inheritdoc/>
                                     public partial event EventHandler E;

                                     /// <inheritdoc/>
                                     public partial event EventHandler E
                                     {
                                         add { }
                                         remove { }
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData);
    }

    /// <summary>
    /// Verifies that both documented parts of a partial method declared override are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-001")]
    [Requirement("RH8201-005")]
    public async Task VerifyBothPartsOfOverridingPartialMethodAreDetectedAndFixed()
    {
        const string testData = """
                                /// <summary>
                                /// Base
                                /// </summary>
                                public abstract class B
                                {
                                    /// <summary>
                                    /// Runs
                                    /// </summary>
                                    public abstract void M();
                                }

                                /// <summary>
                                /// Service
                                /// </summary>
                                public partial class C : B
                                {
                                    ///{|RH8201: <summary>Runs here</summary>
                                |}    public override partial void M();

                                    ///{|RH8201: <summary>Runs there</summary>
                                |}    public override partial void M()
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 /// <summary>
                                 /// Base
                                 /// </summary>
                                 public abstract class B
                                 {
                                     /// <summary>
                                     /// Runs
                                     /// </summary>
                                     public abstract void M();
                                 }

                                 /// <summary>
                                 /// Service
                                 /// </summary>
                                 public partial class C : B
                                 {
                                     /// <inheritdoc/>
                                     public override partial void M();

                                     /// <inheritdoc/>
                                     public override partial void M()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData);
    }

    /// <summary>
    /// Verifies that the documented parts of a partial method implementing an interface member, declared in two
    /// documents, are each reported, and that fixing every diagnostic replaces the comment in both documents
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-001")]
    [Requirement("RH8201-005")]
    public async Task VerifyPartsOfPartialMethodInTwoDocumentsAreDetectedAndFixed()
    {
        const string testData = """
                                /// <summary>
                                /// Contract
                                /// </summary>
                                public interface I
                                {
                                    /// <summary>
                                    /// Runs
                                    /// </summary>
                                    void M();
                                }

                                /// <summary>
                                /// Service
                                /// </summary>
                                public partial class C : I
                                {
                                    ///{|RH8201: <summary>Runs here</summary>
                                |}    public partial void M();
                                }
                                """;
        const string fixedData = """
                                 /// <summary>
                                 /// Contract
                                 /// </summary>
                                 public interface I
                                 {
                                     /// <summary>
                                     /// Runs
                                     /// </summary>
                                     void M();
                                 }

                                 /// <summary>
                                 /// Service
                                 /// </summary>
                                 public partial class C : I
                                 {
                                     /// <inheritdoc/>
                                     public partial void M();
                                 }
                                 """;
        const string otherData = """
                                 /// <summary>
                                 /// Service
                                 /// </summary>
                                 public partial class C
                                 {
                                     ///{|RH8201: <summary>Runs there</summary>
                                 |}    public partial void M()
                                     {
                                     }
                                 }
                                 """;
        const string otherFixedData = """
                                      /// <summary>
                                      /// Service
                                      /// </summary>
                                      public partial class C
                                      {
                                          /// <inheritdoc/>
                                          public partial void M()
                                          {
                                          }
                                      }
                                      """;

        await Verify(testData,
                     fixedData,
                     test =>
                     {
                         test.TestState.Sources.Add(("/0/Test1.cs", otherData));
                         test.FixedState.Sources.Add(("/0/Test1.cs", otherFixedData));
                     });
    }

    /// <summary>
    /// Verifies that the documented parts of a partial method that inherits nothing are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-001")]
    public async Task VerifyDocumentedPartsOfPartialMethodInheritingNothingAreNotReported()
    {
        const string source = """
                              /// <summary>
                              /// Service
                              /// </summary>
                              public partial class C
                              {
                                  /// <summary>Runs here</summary>
                                  public partial void M();

                                  /// <summary>Runs there</summary>
                                  public partial void M()
                                  {
                                  }
                              }
                              """;

        await Verify(source);
    }

    #endregion // Methods
}