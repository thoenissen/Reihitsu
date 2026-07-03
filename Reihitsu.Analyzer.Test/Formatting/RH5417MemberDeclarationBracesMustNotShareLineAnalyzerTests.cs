using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5417MemberDeclarationBracesMustNotShareLineAnalyzer"/> and <see cref="RH5417MemberDeclarationBracesMustNotShareLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5417MemberDeclarationBracesMustNotShareLineAnalyzerTests : AnalyzerTestsBase<RH5417MemberDeclarationBracesMustNotShareLineAnalyzer, RH5417MemberDeclarationBracesMustNotShareLineCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that a member body written with Allman braces does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsWhenMethodBodyIsAllman()
    {
        const string testData = """
                                public class C
                                {
                                    protected virtual void Write()
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that an empty single-line method body is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyEmptyMethodBodyIsDetectedAndFixed()
    {
        const string testData = """
                                public class C
                                {
                                    protected virtual void Write() {|#0:{|} }
                                }
                                """;
        const string fixedData = """
                                 public class C
                                 {
                                     protected virtual void Write()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5417MemberDeclarationBracesMustNotShareLineAnalyzer.DiagnosticId, AnalyzerResources.RH5417MessageFormat));
    }

    /// <summary>
    /// Verifies that a populated single-line method body is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyPopulatedMethodBodyIsDetectedAndFixed()
    {
        const string testData = """
                                public class C
                                {
                                    public int GetValue() {|#0:{|} return 1; }
                                }
                                """;
        const string fixedData = """
                                 public class C
                                 {
                                     public int GetValue()
                                     {
                                         return 1;
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5417MemberDeclarationBracesMustNotShareLineAnalyzer.DiagnosticId, AnalyzerResources.RH5417MessageFormat));
    }

    /// <summary>
    /// Verifies that a single-line constructor body is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyConstructorBodyIsDetectedAndFixed()
    {
        const string testData = """
                                public class C
                                {
                                    public C() {|#0:{|} }
                                }
                                """;
        const string fixedData = """
                                 public class C
                                 {
                                     public C()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5417MemberDeclarationBracesMustNotShareLineAnalyzer.DiagnosticId, AnalyzerResources.RH5417MessageFormat));
    }

    /// <summary>
    /// Verifies that a single-line destructor body is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDestructorBodyIsDetectedAndFixed()
    {
        const string testData = """
                                public class C
                                {
                                    ~C() {|#0:{|} }
                                }
                                """;
        const string fixedData = """
                                 public class C
                                 {
                                     ~C()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5417MemberDeclarationBracesMustNotShareLineAnalyzer.DiagnosticId, AnalyzerResources.RH5417MessageFormat));
    }

    /// <summary>
    /// Verifies that a single-line operator body is detected
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyOperatorBodyIsDetected()
    {
        const string testData = """
                                public class C
                                {
                                    public static C operator +(C a, C b) {|#0:{|} return a; }
                                }
                                """;

        await Verify(testData, Diagnostics(RH5417MemberDeclarationBracesMustNotShareLineAnalyzer.DiagnosticId, AnalyzerResources.RH5417MessageFormat));
    }

    /// <summary>
    /// Verifies that a single-line conversion operator body is detected
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyConversionOperatorBodyIsDetected()
    {
        const string testData = """
                                public class C
                                {
                                    public static explicit operator int(C c) {|#0:{|} return 0; }
                                }
                                """;

        await Verify(testData, Diagnostics(RH5417MemberDeclarationBracesMustNotShareLineAnalyzer.DiagnosticId, AnalyzerResources.RH5417MessageFormat));
    }

    /// <summary>
    /// Verifies that a single-line property get accessor body is detected
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyPropertyGetAccessorBodyIsDetected()
    {
        const string testData = """
                                public class C
                                {
                                    public int Value { get {|#0:{|} return 1; } }
                                }
                                """;

        await Verify(testData, Diagnostics(RH5417MemberDeclarationBracesMustNotShareLineAnalyzer.DiagnosticId, AnalyzerResources.RH5417MessageFormat));
    }

    /// <summary>
    /// Verifies that a single-line property set accessor body is detected while an expression-bodied get accessor is ignored
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyPropertySetAccessorBodyIsDetected()
    {
        const string testData = """
                                public class C
                                {
                                    private int _value;

                                    public int Value { get => _value; set {|#0:{|} _value = value; } }
                                }
                                """;

        await Verify(testData, Diagnostics(RH5417MemberDeclarationBracesMustNotShareLineAnalyzer.DiagnosticId, AnalyzerResources.RH5417MessageFormat));
    }

    /// <summary>
    /// Verifies that a single-line indexer accessor body is detected
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyIndexerAccessorBodyIsDetected()
    {
        const string testData = """
                                public class C
                                {
                                    public int this[int index] { get {|#0:{|} return index; } }
                                }
                                """;

        await Verify(testData, Diagnostics(RH5417MemberDeclarationBracesMustNotShareLineAnalyzer.DiagnosticId, AnalyzerResources.RH5417MessageFormat));
    }

    /// <summary>
    /// Verifies that a single-line event accessor body is detected
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyEventAccessorBodyIsDetected()
    {
        const string testData = """
                                using System;

                                public class C
                                {
                                    public event EventHandler E
                                    {
                                        add {|#0:{|} }
                                        remove
                                        {
                                        }
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH5417MemberDeclarationBracesMustNotShareLineAnalyzer.DiagnosticId, AnalyzerResources.RH5417MessageFormat));
    }

    /// <summary>
    /// Verifies that an expression-bodied method does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyExpressionBodiedMethodDoesNotProduceDiagnostics()
    {
        const string testData = """
                                public class C
                                {
                                    public int GetValue() => 1;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that an auto-property accessor list does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyAutoPropertyDoesNotProduceDiagnostics()
    {
        const string testData = """
                                public class C
                                {
                                    public int Value { get; set; }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that an expression-bodied property does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyExpressionBodiedPropertyDoesNotProduceDiagnostics()
    {
        const string testData = """
                                public class C
                                {
                                    public int Value => 1;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that an expression-bodied accessor does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyExpressionBodiedAccessorDoesNotProduceDiagnostics()
    {
        const string testData = """
                                public class C
                                {
                                    private int _value;

                                    public int Value
                                    {
                                        get => _value;
                                        set => _value = value;
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a single-line local function body is out of scope and does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifySingleLineLocalFunctionDoesNotProduceDiagnostics()
    {
        const string testData = """
                                public class C
                                {
                                    public void Run()
                                    {
                                        void Local() { }

                                        Local();
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a single-line control-flow block is out of scope and does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifySingleLineControlFlowBlockDoesNotProduceDiagnostics()
    {
        const string testData = """
                                public class C
                                {
                                    public void Run(bool flag)
                                    {
                                        if (flag) { return; }
                                    }
                                }
                                """;

        await Verify(testData);
    }

    #endregion // Tests
}