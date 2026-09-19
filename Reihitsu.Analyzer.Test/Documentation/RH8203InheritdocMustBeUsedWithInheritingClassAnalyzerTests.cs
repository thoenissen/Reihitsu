using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH8203InheritdocMustBeUsedWithInheritingClassAnalyzer"/>
/// </summary>
[TestClass]
public class RH8203InheritdocMustBeUsedWithInheritingClassAnalyzerTests : AnalyzerTestsBase<RH8203InheritdocMustBeUsedWithInheritingClassAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies a diagnostic is reported for inheritdoc on a standalone class
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForStandaloneInheritdoc()
    {
        const string source = """
                              namespace TestNamespace;

                              /// {|#0:<inheritdoc/>|}
                              internal class TestClass
                              {
                              }
                              """;

        await Verify(source, Diagnostics(RH8203InheritdocMustBeUsedWithInheritingClassAnalyzer.DiagnosticId, AnalyzerResources.RH8203MessageFormat));
    }

    /// <summary>
    /// Verifies that implicit interface implementations may use inheritdoc
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForImplicitInterfaceImplementation()
    {
        const string source = """
                              namespace TestNamespace;

                              internal interface ITest
                              {
                                  /// <summary>
                                  /// Does work.
                                  /// </summary>
                                  void Execute();
                              }

                              internal class TestClass : ITest
                              {
                                  /// <inheritdoc/>
                                  public void Execute()
                                  {
                                  }
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies a diagnostic is reported for inheritdoc on an operator that inherits nothing
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForOperatorInheritdocNotInheriting()
    {
        const string source = """
                              namespace TestNamespace;

                              internal readonly struct Money
                              {
                                  /// {|#0:<inheritdoc/>|}
                                  public static Money operator +(Money left, Money right) => left;
                              }
                              """;

        await Verify(source, Diagnostics(RH8203InheritdocMustBeUsedWithInheritingClassAnalyzer.DiagnosticId, AnalyzerResources.RH8203MessageFormat));
    }

    /// <summary>
    /// Verifies that an operator implicitly implementing a static abstract interface operator may use inheritdoc
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForImplicitInterfaceOperatorImplementation()
    {
        const string source = """
                              namespace TestNamespace;

                              internal interface IAddable<T> where T : IAddable<T>
                              {
                                  /// <summary>Adds two values.</summary>
                                  static abstract T operator +(T left, T right);
                              }

                              internal readonly struct Money : IAddable<Money>
                              {
                                  /// <inheritdoc/>
                                  public static Money operator +(Money left, Money right) => left;
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies that inheritdoc nested in remarks is ignored
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForInheritdocNestedInRemarks()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <remarks><inheritdoc/></remarks>
                              internal class TestClass
                              {
                              }
                              """;

        await Verify(source);
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
                              
                              /// {|#0:<inheritdoc/>|}
                              internal class TestClass
                              {
                              }
                              """;

        await Verify(source, test => test.SolutionTransforms.Add(ApplyDocumentationModeNoneToTestProject));
    }

    #endregion // Tests
}