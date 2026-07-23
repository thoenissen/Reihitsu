using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Documentation;
using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Test methods for <see cref="RH8309XmlDocumentationElementsMustFollowPrescribedOrderAnalyzer"/> and <see cref="RH8309XmlDocumentationElementsMustFollowPrescribedOrderCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH8309XmlDocumentationElementsMustFollowPrescribedOrderAnalyzerTests : AnalyzerTestsBase<RH8309XmlDocumentationElementsMustFollowPrescribedOrderAnalyzer, RH8309XmlDocumentationElementsMustFollowPrescribedOrderCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that documentation elements in the canonical order do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCanonicalOrderProducesNoDiagnostics()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /// <summary>
                                    /// Adds two numbers.
                                    /// </summary>
                                    /// <typeparam name="T">Item type.</typeparam>
                                    /// <param name="a">First value.</param>
                                    /// <param name="b">Second value.</param>
                                    /// <returns>The sum.</returns>
                                    /// <exception cref="System.Exception">Thrown on failure.</exception>
                                    /// <remarks>
                                    /// Additional notes.
                                    /// </remarks>
                                    public T Add<T>(int a, int b)
                                    {
                                        return default;
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a lone summary element does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifySummaryOnlyProducesNoDiagnostics()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /// <summary>
                                    /// Does something.
                                    /// </summary>
                                    public void Execute()
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that multiple parameters in declaration order do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyParametersInDeclarationOrderProduceNoDiagnostics()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /// <summary>
                                    /// Adds two numbers.
                                    /// </summary>
                                    /// <param name="a">First value.</param>
                                    /// <param name="b">Second value.</param>
                                    public void Add(int a, int b)
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a non-standard custom element before the summary is tolerated
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCustomElementBeforeSummaryProducesNoDiagnostics()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /// <custom>Note.</custom>
                                    /// <summary>
                                    /// Does something.
                                    /// </summary>
                                    public void Execute()
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a returns element placed before the parameters is reordered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyReturnsBeforeParametersIsReorderedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /// <summary>
                                    /// Adds two numbers.
                                    /// </summary>
                                    /// <returns>The sum.</returns>
                                    /// {|#0:<param name="a">First value.</param>|}
                                    /// <param name="b">Second value.</param>
                                    public int Add(int a, int b)
                                    {
                                        return a + b;
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     /// <summary>
                                     /// Adds two numbers.
                                     /// </summary>
                                     /// <param name="a">First value.</param>
                                     /// <param name="b">Second value.</param>
                                     /// <returns>The sum.</returns>
                                     public int Add(int a, int b)
                                     {
                                         return a + b;
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH8309XmlDocumentationElementsMustFollowPrescribedOrderAnalyzer.DiagnosticId, AnalyzerResources.RH8309MessageFormat));
    }

    /// <summary>
    /// Verifies that a returns element placed before the summary is reordered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyReturnsBeforeSummaryIsReorderedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /// <returns>The sum.</returns>
                                    /// {|#0:<summary>
                                    /// Adds two numbers.
                                    /// </summary>|}
                                    /// <param name="a">First value.</param>
                                    /// <param name="b">Second value.</param>
                                    public int Add(int a, int b)
                                    {
                                        return a + b;
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     /// <summary>
                                     /// Adds two numbers.
                                     /// </summary>
                                     /// <param name="a">First value.</param>
                                     /// <param name="b">Second value.</param>
                                     /// <returns>The sum.</returns>
                                     public int Add(int a, int b)
                                     {
                                         return a + b;
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH8309XmlDocumentationElementsMustFollowPrescribedOrderAnalyzer.DiagnosticId, AnalyzerResources.RH8309MessageFormat));
    }

    /// <summary>
    /// Verifies that a remarks element placed before the returns element is reordered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyRemarksBeforeReturnsIsReorderedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /// <summary>
                                    /// Provides a value.
                                    /// </summary>
                                    /// <remarks>
                                    /// Additional notes.
                                    /// </remarks>
                                    /// {|#0:<returns>The value.</returns>|}
                                    public int GetValue()
                                    {
                                        return 0;
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     /// <summary>
                                     /// Provides a value.
                                     /// </summary>
                                     /// <returns>The value.</returns>
                                     /// <remarks>
                                     /// Additional notes.
                                     /// </remarks>
                                     public int GetValue()
                                     {
                                         return 0;
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH8309XmlDocumentationElementsMustFollowPrescribedOrderAnalyzer.DiagnosticId, AnalyzerResources.RH8309MessageFormat));
    }

    /// <summary>
    /// Verifies that a type parameter element placed after a parameter element is reordered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyTypeParameterAfterParameterIsReorderedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /// <summary>
                                    /// Creates an item.
                                    /// </summary>
                                    /// <param name="value">Value.</param>
                                    /// {|#0:<typeparam name="T">Item type.</typeparam>|}
                                    public T Create<T>(int value)
                                    {
                                        return default;
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     /// <summary>
                                     /// Creates an item.
                                     /// </summary>
                                     /// <typeparam name="T">Item type.</typeparam>
                                     /// <param name="value">Value.</param>
                                     public T Create<T>(int value)
                                     {
                                         return default;
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH8309XmlDocumentationElementsMustFollowPrescribedOrderAnalyzer.DiagnosticId, AnalyzerResources.RH8309MessageFormat));
    }

    /// <summary>
    /// Verifies that an exception element placed before the returns element is reordered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyExceptionBeforeReturnsIsReorderedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /// <summary>
                                    /// Divides two numbers.
                                    /// </summary>
                                    /// <exception cref="System.Exception">Thrown on failure.</exception>
                                    /// {|#0:<returns>The quotient.</returns>|}
                                    public int Divide(int a, int b)
                                    {
                                        return a / b;
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     /// <summary>
                                     /// Divides two numbers.
                                     /// </summary>
                                     /// <returns>The quotient.</returns>
                                     /// <exception cref="System.Exception">Thrown on failure.</exception>
                                     public int Divide(int a, int b)
                                     {
                                         return a / b;
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH8309XmlDocumentationElementsMustFollowPrescribedOrderAnalyzer.DiagnosticId, AnalyzerResources.RH8309MessageFormat));
    }

    /// <summary>
    /// Verifies that a trailing seealso element remains last while the canonical elements are reordered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifySeealsoRemainsLastWhenCanonicalElementsAreReordered()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /// <summary>
                                    /// Adds two numbers.
                                    /// </summary>
                                    /// <returns>The sum.</returns>
                                    /// {|#0:<param name="a">First value.</param>|}
                                    /// <seealso cref="System.Object"/>
                                    public int Add(int a)
                                    {
                                        return a;
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     /// <summary>
                                     /// Adds two numbers.
                                     /// </summary>
                                     /// <param name="a">First value.</param>
                                     /// <returns>The sum.</returns>
                                     /// <seealso cref="System.Object"/>
                                     public int Add(int a)
                                     {
                                         return a;
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH8309XmlDocumentationElementsMustFollowPrescribedOrderAnalyzer.DiagnosticId, AnalyzerResources.RH8309MessageFormat));
    }

    /// <summary>
    /// Verifies that a leading inheritdoc element remains first while the canonical elements are reordered around it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyLeadingInheritdocRemainsFirstWhileCanonicalElementsAreReordered()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /// <inheritdoc/>
                                    /// <returns>The value.</returns>
                                    /// {|#0:<summary>
                                    /// Provides a value.
                                    /// </summary>|}
                                    public int GetValue()
                                    {
                                        return 0;
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     /// <inheritdoc/>
                                     /// <summary>
                                     /// Provides a value.
                                     /// </summary>
                                     /// <returns>The value.</returns>
                                     public int GetValue()
                                     {
                                         return 0;
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH8309XmlDocumentationElementsMustFollowPrescribedOrderAnalyzer.DiagnosticId, AnalyzerResources.RH8309MessageFormat));
    }

    /// <summary>
    /// Verifies that a non-standard custom element between canonical elements stays pinned while the canonical elements are reordered around it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCustomElementBetweenCanonicalElementsStaysPinnedWhileCanonicalElementsAreReordered()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /// <returns>The value.</returns>
                                    /// <custom>Note.</custom>
                                    /// {|#0:<summary>
                                    /// Provides a value.
                                    /// </summary>|}
                                    public int GetValue()
                                    {
                                        return 0;
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     /// <summary>
                                     /// Provides a value.
                                     /// </summary>
                                     /// <custom>Note.</custom>
                                     /// <returns>The value.</returns>
                                     public int GetValue()
                                     {
                                         return 0;
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH8309XmlDocumentationElementsMustFollowPrescribedOrderAnalyzer.DiagnosticId, AnalyzerResources.RH8309MessageFormat));
    }

    /// <summary>
    /// Verifies that an inheritdoc element placed after the summary is reordered to the front
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyInheritdocAfterSummaryIsReorderedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /// <summary>
                                    /// Does something.
                                    /// </summary>
                                    /// {|#0:<inheritdoc/>|}
                                    public void Execute()
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     /// <inheritdoc/>
                                     /// <summary>
                                     /// Does something.
                                     /// </summary>
                                     public void Execute()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH8309XmlDocumentationElementsMustFollowPrescribedOrderAnalyzer.DiagnosticId, AnalyzerResources.RH8309MessageFormat));
    }

    /// <summary>
    /// Verifies that an include element placed before the inheritdoc element is reordered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyIncludeBeforeInheritdocIsReorderedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /// <include file="doc.xml" path="//member"/>
                                    /// {|#0:<inheritdoc/>|}
                                    public void Execute()
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     /// <inheritdoc/>
                                     /// <include file="doc.xml" path="//member"/>
                                     public void Execute()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH8309XmlDocumentationElementsMustFollowPrescribedOrderAnalyzer.DiagnosticId, AnalyzerResources.RH8309MessageFormat));
    }

    /// <summary>
    /// Verifies that a seealso element placed before the summary is reordered to the end
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifySeealsoBeforeSummaryIsReorderedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /// <seealso cref="System.Object"/>
                                    /// {|#0:<summary>
                                    /// Does something.
                                    /// </summary>|}
                                    public void Execute()
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     /// <summary>
                                     /// Does something.
                                     /// </summary>
                                     /// <seealso cref="System.Object"/>
                                     public void Execute()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH8309XmlDocumentationElementsMustFollowPrescribedOrderAnalyzer.DiagnosticId, AnalyzerResources.RH8309MessageFormat));
    }

    /// <summary>
    /// Verifies that inheritdoc, include and seealso in their canonical positions do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDocumentationSourceAndSeealsoInCanonicalOrderProduceNoDiagnostics()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /// <inheritdoc/>
                                    /// <include file="doc.xml" path="//member"/>
                                    /// <summary>
                                    /// Provides a value.
                                    /// </summary>
                                    /// <returns>The value.</returns>
                                    /// <seealso cref="System.Object"/>
                                    public int GetValue()
                                    {
                                        return 0;
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a permission element after an exception element does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyPermissionAfterExceptionProducesNoDiagnostics()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /// <summary>
                                    /// Divides two numbers.
                                    /// </summary>
                                    /// <exception cref="System.Exception">Thrown on failure.</exception>
                                    /// <permission cref="System.Object">Requires elevated trust.</permission>
                                    public int Divide(int a, int b)
                                    {
                                        return a / b;
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a permission element placed before the exception element is reordered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyPermissionBeforeExceptionIsReorderedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /// <summary>
                                    /// Divides two numbers.
                                    /// </summary>
                                    /// <permission cref="System.Object">Requires elevated trust.</permission>
                                    /// {|#0:<exception cref="System.Exception">Thrown on failure.</exception>|}
                                    public int Divide(int a, int b)
                                    {
                                        return a / b;
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     /// <summary>
                                     /// Divides two numbers.
                                     /// </summary>
                                     /// <exception cref="System.Exception">Thrown on failure.</exception>
                                     /// <permission cref="System.Object">Requires elevated trust.</permission>
                                     public int Divide(int a, int b)
                                     {
                                         return a / b;
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH8309XmlDocumentationElementsMustFollowPrescribedOrderAnalyzer.DiagnosticId, AnalyzerResources.RH8309MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported when documentation mode is none
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsWhenDocumentationModeIsNone()
    {
        const string source = """
                              internal class TestClass
                              {
                                  /// <returns>The sum.</returns>
                                  /// <summary>
                                  /// Adds two numbers.
                                  /// </summary>
                                  internal int Execute()
                                  {
                                      return 0;
                                  }
                              }
                              """;

        await Verify(source, test => test.SolutionTransforms.Add(ApplyDocumentationModeNoneToTestProject));
    }

    #endregion // Tests
}