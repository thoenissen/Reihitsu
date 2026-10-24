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
public class RH8309XmlDocumentationElementsMustFollowPrescribedOrderAnalyzerTests : BatchCodeFixTestsBase<RH8309XmlDocumentationElementsMustFollowPrescribedOrderAnalyzer, RH8309XmlDocumentationElementsMustFollowPrescribedOrderCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that documentation elements in the canonical order do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8309-002")]
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
    [Requirement("RH8309-002")]
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
    [Requirement("RH8309-002")]
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
    [Requirement("RH8309-002")]
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
    [Requirement("RH8309-001")]
    [Requirement("RH8309-003")]
    [Requirement("RH8309-004")]
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
    [Requirement("RH8309-001")]
    [Requirement("RH8309-003")]
    [Requirement("RH8309-004")]
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
    [Requirement("RH8309-001")]
    [Requirement("RH8309-003")]
    [Requirement("RH8309-004")]
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
    [Requirement("RH8309-001")]
    [Requirement("RH8309-003")]
    [Requirement("RH8309-004")]
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
    [Requirement("RH8309-001")]
    [Requirement("RH8309-003")]
    [Requirement("RH8309-004")]
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
    [Requirement("RH8309-001")]
    [Requirement("RH8309-003")]
    [Requirement("RH8309-004")]
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
    [Requirement("RH8309-001")]
    [Requirement("RH8309-003")]
    [Requirement("RH8309-004")]
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
    [Requirement("RH8309-001")]
    [Requirement("RH8309-003")]
    [Requirement("RH8309-004")]
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
    [Requirement("RH8309-001")]
    [Requirement("RH8309-003")]
    [Requirement("RH8309-004")]
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
    [Requirement("RH8309-001")]
    [Requirement("RH8309-003")]
    [Requirement("RH8309-004")]
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
    [Requirement("RH8309-001")]
    [Requirement("RH8309-003")]
    [Requirement("RH8309-004")]
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
    [Requirement("RH8309-002")]
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
    [Requirement("RH8309-002")]
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
    [Requirement("RH8309-001")]
    [Requirement("RH8309-003")]
    [Requirement("RH8309-004")]
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
    [Requirement("RH8309-001")]
    [Requirement("SHARED-DOC-008")]
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

    /// <summary>
    /// Verifies that an element standing after an element that comes later in the order is reported, whether it is
    /// closed by an end tag or empty
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8309-001")]
    [Requirement("RH8309-003")]
    public async Task VerifyElementsOutOfThePrescribedOrderAreReported()
    {
        const string testData = """
                                class C
                                {
                                    /// <returns>Sum</returns>
                                    /// {|#0:<summary>
                                    /// Adds
                                    /// </summary>|}
                                    /// <param name="a">First</param>
                                    int Add(int a)
                                    {
                                        return a;
                                    }

                                    /// <summary>
                                    /// Value
                                    /// </summary>
                                    /// <seealso cref="C"/>
                                    /// {|#1:<inheritdoc/>|}
                                    int A;
                                }
                                """;

        await Verify(testData, Diagnostics(RH8309XmlDocumentationElementsMustFollowPrescribedOrderAnalyzer.DiagnosticId, AnalyzerResources.RH8309MessageFormat, 2));
    }

    /// <summary>
    /// Verifies that elements of one kind in any order among themselves, a custom element, and a nested element are
    /// not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8309-002")]
    public async Task VerifySameKindCustomAndNestedElementsDoNotProduceDiagnostics()
    {
        const string testData = """
                                class C
                                {
                                    /// <summary>
                                    /// Adds
                                    /// </summary>
                                    /// <param name="b">Second</param>
                                    /// <param name="a">First</param>
                                    /// <custom>Custom</custom>
                                    /// <returns>Sum</returns>
                                    /// <remarks><returns>Nested</returns></remarks>
                                    int Add(int a, int b)
                                    {
                                        return a;
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that only the first element out of order is reported, even when later elements are out of order too
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8309-003")]
    public async Task VerifyOnlyTheFirstElementOutOfOrderIsReported()
    {
        const string testData = """
                                class C
                                {
                                    /// <exception cref="System.Exception">Fails</exception>
                                    /// {|#0:<returns>Value</returns>|}
                                    /// <example>Example</example>
                                    /// <value>Value</value>
                                    int A => 0;
                                }
                                """;

        await Verify(testData, Diagnostics(RH8309XmlDocumentationElementsMustFollowPrescribedOrderAnalyzer.DiagnosticId, AnalyzerResources.RH8309MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix sorts the elements into the prescribed order while a custom element keeps its position
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8309-004")]
    public async Task VerifyFixSortsTheElementsAroundACustomElement()
    {
        const string testData = """
                                class C
                                {
                                    /// <summary>
                                    /// Adds
                                    /// </summary>
                                    /// <remarks>Remarks</remarks>
                                    /// <custom>Custom</custom>
                                    /// {|#0:<param name="a">First</param>|}
                                    /// <returns>Sum</returns>
                                    int Add(int a)
                                    {
                                        return a;
                                    }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     /// <summary>
                                     /// Adds
                                     /// </summary>
                                     /// <param name="a">First</param>
                                     /// <custom>Custom</custom>
                                     /// <returns>Sum</returns>
                                     /// <remarks>Remarks</remarks>
                                     int Add(int a)
                                     {
                                         return a;
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH8309XmlDocumentationElementsMustFollowPrescribedOrderAnalyzer.DiagnosticId, AnalyzerResources.RH8309MessageFormat));
    }

    /// <summary>
    /// Verifies the specification example: no code fix is offered when a top-level element of the reported comment has
    /// no end tag, whether it is the reported element, an element the fix would move, or an element that keeps its
    /// position
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-DOC-017")]
    [Requirement("RH8309-004")]
    public async Task VerifySpecificationExampleWithholdsFixForElementsWithoutEndTag()
    {
        const string source = """
                              class C
                              {
                                  /// <returns>R</returns>
                                  /// {|RH8309:<summary>S
                              |}    int A() => 0;

                                  /// <returns>R</returns>
                                  /// {|RH8309:<summary>
                                  /// S
                                  /// </summary>|}
                                  /// <param name="a">P
                                  int M(int a) => a;

                                  /// <returns>R</returns>
                                  /// {|RH8309:<summary>
                                  /// S
                                  /// </summary>|}
                                  /// <seealso cref="C">X
                                  int N() => 0;
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies that no code fix is offered when the reported element is closed by an end tag of another name, so that
    /// end tag is not moved with it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-DOC-017")]
    public async Task VerifyNoCodeFixForReportedElementClosedByEndTagOfAnotherName()
    {
        const string source = """
                              class C
                              {
                                  /// <returns>R</returns>
                                  /// {|RH8309:<summary>S</returns>|}
                                  int A()
                                  {
                                      return 0;
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies that end-tag names are compared exactly: no code fix is offered when the reported summary's end tag
    /// differs only in case
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-DOC-017")]
    public async Task VerifyNoCodeFixForReportedElementClosedByEndTagDifferingOnlyInCase()
    {
        const string source = """
                              class C
                              {
                                  /// <returns>R</returns>
                                  /// {|RH8309:<summary>
                                  /// S
                                  /// </Summary>|}
                                  int A()
                                  {
                                      return 0;
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies the other side of the boundary: an element without an end tag of its own nested inside a top-level
    /// element closed by its own end tag does not withhold the fix, and its text is kept
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-DOC-017")]
    [Requirement("RH8309-004")]
    public async Task VerifyCodeFixIsOfferedWhenOnlyANestedElementLacksAnEndTagOfItsOwn()
    {
        const string source = """
                              class C
                              {
                                  /// <returns>R <see>x</y></returns>
                                  /// {|#0:<summary>
                                  /// S
                                  /// </summary>|}
                                  int A()
                                  {
                                      return 0;
                                  }
                              }
                              """;
        const string fixedSource = """
                                   class C
                                   {
                                       /// <summary>
                                       /// S
                                       /// </summary>
                                       /// <returns>R <see>x</y></returns>
                                       int A()
                                       {
                                           return 0;
                                       }
                                   }
                                   """;

        await Verify(source, fixedSource, Diagnostics(RH8309XmlDocumentationElementsMustFollowPrescribedOrderAnalyzer.DiagnosticId, AnalyzerResources.RH8309MessageFormat));
    }

    /// <summary>
    /// Verifies the specification example: no code fix is offered when text, an XML comment, a CDATA section, or a
    /// processing instruction stands at the top level of the reported comment, between, before, or after the elements
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8309-005")]
    public async Task VerifySpecificationExampleWithholdsFixForTextAndNodesBesideTheElements()
    {
        const string source = """
                              class C
                              {
                                  /// <remarks>R</remarks>
                                  /// Text between
                                  /// {|RH8309:<summary>
                                  /// S
                                  /// </summary>|}
                                  int A;

                                  /// <remarks>R</remarks>
                                  /// <!-- c -->
                                  /// {|RH8309:<summary>
                                  /// S
                                  /// </summary>|}
                                  int B;

                                  /// Intro
                                  /// <remarks>R</remarks>
                                  /// {|RH8309:<summary>
                                  /// S
                                  /// </summary>|}
                                  int D;

                                  /// <remarks>R</remarks>
                                  /// {|RH8309:<summary>
                                  /// S
                                  /// </summary>|}
                                  /// Outro
                                  int E;

                                  /// <remarks>R</remarks>
                                  /// <![CDATA[x]]>
                                  /// {|RH8309:<summary>
                                  /// S
                                  /// </summary>|}
                                  int F;

                                  /// <remarks>R</remarks>
                                  /// <?pi x?>
                                  /// {|RH8309:<summary>
                                  /// S
                                  /// </summary>|}
                                  int G;
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies that text between the elements withholds the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8309-005")]
    public async Task VerifyNoCodeFixForTextBetweenTheElements()
    {
        const string source = """
                              class C
                              {
                                  /// <remarks>R</remarks>
                                  /// Text between
                                  /// {|RH8309:<summary>
                                  /// S
                                  /// </summary>|}
                                  int A;
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies that an XML comment between the elements withholds the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8309-005")]
    public async Task VerifyNoCodeFixForXmlCommentBetweenTheElements()
    {
        const string source = """
                              class C
                              {
                                  /// <remarks>R</remarks>
                                  /// <!-- c -->
                                  /// {|RH8309:<summary>
                                  /// S
                                  /// </summary>|}
                                  int A;
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies that text before the first element withholds the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8309-005")]
    public async Task VerifyNoCodeFixForTextBeforeTheFirstElement()
    {
        const string source = """
                              class C
                              {
                                  /// Intro
                                  /// <remarks>R</remarks>
                                  /// {|RH8309:<summary>
                                  /// S
                                  /// </summary>|}
                                  int A;
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies that text after the last element withholds the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8309-005")]
    public async Task VerifyNoCodeFixForTextAfterTheLastElement()
    {
        const string source = """
                              class C
                              {
                                  /// <remarks>R</remarks>
                                  /// {|RH8309:<summary>
                                  /// S
                                  /// </summary>|}
                                  /// Outro
                                  int A;
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies that a CDATA section between the elements withholds the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8309-005")]
    public async Task VerifyNoCodeFixForCDataSectionBetweenTheElements()
    {
        const string source = """
                              class C
                              {
                                  /// <remarks>R</remarks>
                                  /// <![CDATA[x]]>
                                  /// {|RH8309:<summary>
                                  /// S
                                  /// </summary>|}
                                  int A;
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies that a processing instruction between the elements withholds the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8309-005")]
    public async Task VerifyNoCodeFixForProcessingInstructionBetweenTheElements()
    {
        const string source = """
                              class C
                              {
                                  /// <remarks>R</remarks>
                                  /// <?pi x?>
                                  /// {|RH8309:<summary>
                                  /// S
                                  /// </summary>|}
                                  int A;
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies that an entity reference standing alone between the elements is text that withholds the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8309-005")]
    public async Task VerifyNoCodeFixForEntityReferenceBetweenTheElements()
    {
        const string source = """
                              class C
                              {
                                  /// <remarks>R</remarks>
                                  /// &amp;
                                  /// {|RH8309:<summary>
                                  /// S
                                  /// </summary>|}
                                  int A;
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies the other side of the boundary: blank documentation lines between the elements are white space and do
    /// not withhold the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8309-005")]
    [Requirement("RH8309-004")]
    public async Task VerifyCodeFixIsOfferedWhenOnlyWhiteSpaceStandsBetweenTheElements()
    {
        const string source = """
                              class C
                              {
                                  /// <remarks>R</remarks>
                                  ///
                                  /// {|#0:<summary>
                                  /// S
                                  /// </summary>|}
                                  int A;
                              }
                              """;
        const string fixedSource = """
                                   class C
                                   {
                                       /// <summary>
                                       /// S
                                       /// </summary>
                                       ///
                                       /// <remarks>R</remarks>
                                       int A;
                                   }
                                   """;

        await Verify(source, fixedSource, Diagnostics(RH8309XmlDocumentationElementsMustFollowPrescribedOrderAnalyzer.DiagnosticId, AnalyzerResources.RH8309MessageFormat));
    }

    /// <summary>
    /// Verifies that fixing every diagnostic of a document sorts the comment that holds nothing but its elements and
    /// leaves the comments whose fix is withheld reported and unchanged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8309-005")]
    [Requirement("SHARED-DOC-017")]
    public async Task VerifyFixAllLeavesCommentsWithWithheldFixUnchanged()
    {
        const string source = """
                              class C
                              {
                                  /// <remarks>R</remarks>
                                  /// {|RH8309:<summary>
                                  /// S
                                  /// </summary>|}
                                  int A;

                                  /// <remarks>R</remarks>
                                  /// Text between
                                  /// {|RH8309:<summary>
                                  /// S
                                  /// </summary>|}
                                  int B;

                                  /// <returns>R</returns>
                                  /// {|RH8309:<summary>S</returns>|}
                                  int D()
                                  {
                                      return 0;
                                  }
                              }
                              """;
        const string fixedSource = """
                                   class C
                                   {
                                       /// <summary>
                                       /// S
                                       /// </summary>
                                       /// <remarks>R</remarks>
                                       int A;

                                       /// <remarks>R</remarks>
                                       /// Text between
                                       /// {|RH8309:<summary>
                                       /// S
                                       /// </summary>|}
                                       int B;

                                       /// <returns>R</returns>
                                       /// {|RH8309:<summary>S</returns>|}
                                       int D()
                                       {
                                           return 0;
                                       }
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifies that no code fix is offered for a reported summary without an end tag in a CRLF document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-DOC-017")]
    public async Task VerifyNoCodeFixForReportedElementWithoutEndTagWithCarriageReturnLineFeed()
    {
        const string source = """
                              class C
                              {
                                  /// <returns>R</returns>
                                  /// {|RH8309:<summary>S
                              |}    int A() => 0;
                              }
                              """;

        await VerifyNoCodeFixIsOffered(NormalizeToCarriageReturnLineFeed(source));
    }

    /// <summary>
    /// Verifies that text between the elements withholds the fix in a CRLF document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8309-005")]
    public async Task VerifyNoCodeFixForTextBetweenTheElementsWithCarriageReturnLineFeed()
    {
        const string source = """
                              class C
                              {
                                  /// <remarks>R</remarks>
                                  /// Text between
                                  /// {|RH8309:<summary>
                                  /// S
                                  /// </summary>|}
                                  int A;
                              }
                              """;

        await VerifyNoCodeFixIsOffered(NormalizeToCarriageReturnLineFeed(source));
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string source = """
                              internal class TestClass
                              {
                                  /// <summary>
                                  /// Adds a number.
                                  /// </summary>
                                  /// <returns>The sum.</returns>
                                  /// {|#0:<param name="a">First value.</param>|}
                                  public int Add(int a)
                                  {
                                      return a;
                                  }

                                  /// <summary>
                                  /// Subtracts a number.
                                  /// </summary>
                                  /// <returns>The difference.</returns>
                                  /// {|#1:<param name="b">Second value.</param>|}
                                  public int Subtract(int b)
                                  {
                                      return -b;
                                  }
                              }
                              """;
        const string fixedSource = """
                                   internal class TestClass
                                   {
                                       /// <summary>
                                       /// Adds a number.
                                       /// </summary>
                                       /// <param name="a">First value.</param>
                                       /// <returns>The sum.</returns>
                                       public int Add(int a)
                                       {
                                           return a;
                                       }

                                       /// <summary>
                                       /// Subtracts a number.
                                       /// </summary>
                                       /// <param name="b">Second value.</param>
                                       /// <returns>The difference.</returns>
                                       public int Subtract(int b)
                                       {
                                           return -b;
                                       }
                                   }
                                   """;

        // The two members are directly adjacent and each reorders around the one blank line separating them,
        // which is where a rewrite of one declaration could reach into the other's leading trivia
        return new FixAllScenario(source, fixedSource, Diagnostics(RH8309XmlDocumentationElementsMustFollowPrescribedOrderAnalyzer.DiagnosticId, AnalyzerResources.RH8309MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}