using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5113DeclarationSemicolonMustStayOnDeclarationLineAnalyzer"/>
/// </summary>
[TestClass]
public class RH5113DeclarationSemicolonMustStayOnDeclarationLineAnalyzerTests : AnalyzerTestsBase<RH5113DeclarationSemicolonMustStayOnDeclarationLineAnalyzer, RH5113DeclarationSemicolonMustStayOnDeclarationLineCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying diagnostics for field declaration with semicolon on a new line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticsForFieldDeclarationWithSemicolonOnNewLine()
    {
        const string testData = """
                                namespace TestNamespace
                                {
                                    internal class TestClass
                                    {
                                        private int _value
                                            {|#0:;|}
                                    }
                                }
                                """;

        const string fixedData = """
                                 namespace TestNamespace
                                 {
                                     internal class TestClass
                                     {
                                         private int _value;
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5113DeclarationSemicolonMustStayOnDeclarationLineAnalyzer.DiagnosticId, AnalyzerResources.RH5113MessageFormat));
    }

    /// <summary>
    /// Verifying diagnostics for constant field declaration with semicolon on a new line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticsForConstantFieldDeclarationWithSemicolonOnNewLine()
    {
        const string testData = """
                                namespace TestNamespace
                                {
                                    internal class TestClass
                                    {
                                        private const int Limit = 10
                                            {|#0:;|}
                                    }
                                }
                                """;

        const string fixedData = """
                                 namespace TestNamespace
                                 {
                                     internal class TestClass
                                     {
                                         private const int Limit = 10;
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5113DeclarationSemicolonMustStayOnDeclarationLineAnalyzer.DiagnosticId, AnalyzerResources.RH5113MessageFormat));
    }

    /// <summary>
    /// Verifying diagnostics for event field declaration with semicolon on a new line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticsForEventFieldDeclarationWithSemicolonOnNewLine()
    {
        const string testData = """
                                namespace TestNamespace
                                {
                                    internal class TestClass
                                    {
                                        public event System.EventHandler Changed
                                            {|#0:;|}
                                    }
                                }
                                """;

        const string fixedData = """
                                 namespace TestNamespace
                                 {
                                     internal class TestClass
                                     {
                                         public event System.EventHandler Changed;
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5113DeclarationSemicolonMustStayOnDeclarationLineAnalyzer.DiagnosticId, AnalyzerResources.RH5113MessageFormat));
    }

    /// <summary>
    /// Verifying diagnostics for delegate declaration with semicolon on a new line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticsForDelegateDeclarationWithSemicolonOnNewLine()
    {
        const string testData = """
                                namespace TestNamespace
                                {
                                    internal class TestClass
                                    {
                                        public delegate void Handler(int value)
                                            {|#0:;|}
                                    }
                                }
                                """;

        const string fixedData = """
                                 namespace TestNamespace
                                 {
                                     internal class TestClass
                                     {
                                         public delegate void Handler(int value);
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5113DeclarationSemicolonMustStayOnDeclarationLineAnalyzer.DiagnosticId, AnalyzerResources.RH5113MessageFormat));
    }

    /// <summary>
    /// Verifying diagnostics for field declaration with initializer and semicolon on a new line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticsForFieldDeclarationWithInitializerAndSemicolonOnNewLine()
    {
        const string testData = """
                                namespace TestNamespace
                                {
                                    internal class TestClass
                                    {
                                        private int _value = 42
                                            {|#0:;|}
                                    }
                                }
                                """;

        const string fixedData = """
                                 namespace TestNamespace
                                 {
                                     internal class TestClass
                                     {
                                         private int _value = 42;
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5113DeclarationSemicolonMustStayOnDeclarationLineAnalyzer.DiagnosticId, AnalyzerResources.RH5113MessageFormat));
    }

    /// <summary>
    /// Verifying no diagnostic is reported when the field declaration semicolon stays on the declaration line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForCompliantFieldDeclaration()
    {
        const string testData = """
                                namespace TestNamespace
                                {
                                    internal class TestClass
                                    {
                                        private int _value;
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifying no diagnostic is reported when the event field declaration semicolon stays on the declaration line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForCompliantEventFieldDeclaration()
    {
        const string testData = """
                                namespace TestNamespace
                                {
                                    internal class TestClass
                                    {
                                        public event System.EventHandler Changed;
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifying no diagnostic is reported when the delegate declaration semicolon stays on the declaration line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForCompliantDelegateDeclaration()
    {
        const string testData = """
                                namespace TestNamespace
                                {
                                    internal delegate void Handler(int value);
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifying the code fix is offered when an unrelated single-line comment sits above the field but the join
    /// gap itself is comment-free
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCodeFixOfferedWhenUnrelatedCommentPrecedesField()
    {
        const string testData = """
                                namespace TestNamespace
                                {
                                    internal class TestClass
                                    {
                                        // The current value
                                        private int _value
                                            {|#0:;|}
                                    }
                                }
                                """;

        const string fixedData = """
                                 namespace TestNamespace
                                 {
                                     internal class TestClass
                                     {
                                         // The current value
                                         private int _value;
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5113DeclarationSemicolonMustStayOnDeclarationLineAnalyzer.DiagnosticId, AnalyzerResources.RH5113MessageFormat));
    }

    #endregion // Tests
}