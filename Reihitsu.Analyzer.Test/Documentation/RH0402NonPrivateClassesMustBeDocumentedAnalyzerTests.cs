using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH0402NonPrivateClassesMustBeDocumentedAnalyzer"/>.
/// </summary>
[TestClass]
public class RH0402NonPrivateClassesMustBeDocumentedAnalyzerTests : AnalyzerTestsBase<RH0402NonPrivateClassesMustBeDocumentedAnalyzer>
{
    #region Diagnostic cases

    /// <summary>
    /// Verifies a diagnostic is reported for an internal class that has a remarks tag but no summary.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForClassWithoutSummary()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <remarks>Missing summary.</remarks>
                              internal class {|#0:TestClass|}
                              {
                              }
                              """;

        await Verify(source, Diagnostics(RH0402NonPrivateClassesMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH0402MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for a public class without any documentation comment.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForPublicClassWithoutDocumentation()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              public class {|#0:PublicService|}
                              {
                              }
                              """;

        await Verify(source, Diagnostics(RH0402NonPrivateClassesMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH0402MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for a top-level class without an explicit access modifier and without documentation.
    /// A class declared at namespace scope without a modifier defaults to internal accessibility.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForTopLevelClassWithNoModifierAndNoDocumentation()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              class {|#0:ImplicitInternalClass|}
                              {
                              }
                              """;

        await Verify(source, Diagnostics(RH0402NonPrivateClassesMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH0402MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for a nested protected class without documentation.
    /// Protected is a non-private modifier and falls under the non-private accessibility group.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForNestedProtectedClassWithoutDocumentation()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <summary>Container.</summary>
                              public class Container
                              {
                                  protected class {|#0:ProtectedInner|}
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH0402NonPrivateClassesMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH0402MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for a nested private protected class without documentation.
    /// The private protected modifier includes a protected qualifier, which makes the declaration
    /// non-pure-private and therefore subject to the non-private documentation rule.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForNestedPrivateProtectedClassWithoutDocumentation()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <summary>Container.</summary>
                              public class Container
                              {
                                  private protected class {|#0:PrivateProtectedInner|}
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH0402NonPrivateClassesMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH0402MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for a generic internal class without documentation.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForGenericClassWithoutDocumentation()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              internal class {|#0:Repository|}<T>
                              {
                              }
                              """;

        await Verify(source, Diagnostics(RH0402NonPrivateClassesMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH0402MessageFormat));
    }

    #endregion // Diagnostic cases

    #region No-diagnostic cases

    /// <summary>
    /// Verifies no diagnostic is reported for an internal class with a summary tag.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForInternalClassWithSummary()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <summary>A documented internal class.</summary>
                              internal class DocumentedClass
                              {
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies no diagnostic is reported for a public class documented with inheritdoc.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForPublicClassWithInheritdoc()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <summary>Base class.</summary>
                              public class BaseEntity
                              {
                              }
                              
                              /// <inheritdoc/>
                              public class DerivedEntity : BaseEntity
                              {
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies no diagnostic is reported for an undocumented pure private nested class.
    /// Pure private declarations are covered by RH0403, not by this analyzer.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForPurePrivateNestedClass()
    {
        const string source = """
                              namespace TestNamespace;
                              
                               /// <summary>Container.</summary>
                               internal class Container
                               {
                                   private class UndocumentedInner
                                   {
                                   }
                               }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies no diagnostic is reported for a partial internal class with a summary tag.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForPartialClassWithSummary()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <summary>A partial class.</summary>
                              internal partial class PartialHandler
                              {
                              }
                              """;

        await Verify(source);
    }

    #endregion // No-diagnostic cases
}