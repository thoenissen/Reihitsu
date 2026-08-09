using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5417FunctionAndAccessorBodyBracesMustNotShareLineAnalyzer"/> and <see cref="RH5417FunctionAndAccessorBodyBracesMustNotShareLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5417FunctionAndAccessorBodyBracesMustNotShareLineAnalyzerTests : AnalyzerTestsBase<RH5417FunctionAndAccessorBodyBracesMustNotShareLineAnalyzer, RH5417FunctionAndAccessorBodyBracesMustNotShareLineCodeFixProvider>
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

        await Verify(testData, fixedData, Diagnostics(RH5417FunctionAndAccessorBodyBracesMustNotShareLineAnalyzer.DiagnosticId, AnalyzerResources.RH5417MessageFormat));
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

        await Verify(testData, fixedData, Diagnostics(RH5417FunctionAndAccessorBodyBracesMustNotShareLineAnalyzer.DiagnosticId, AnalyzerResources.RH5417MessageFormat));
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

        await Verify(testData, fixedData, Diagnostics(RH5417FunctionAndAccessorBodyBracesMustNotShareLineAnalyzer.DiagnosticId, AnalyzerResources.RH5417MessageFormat));
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

        await Verify(testData, fixedData, Diagnostics(RH5417FunctionAndAccessorBodyBracesMustNotShareLineAnalyzer.DiagnosticId, AnalyzerResources.RH5417MessageFormat));
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

        await Verify(testData, Diagnostics(RH5417FunctionAndAccessorBodyBracesMustNotShareLineAnalyzer.DiagnosticId, AnalyzerResources.RH5417MessageFormat));
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

        await Verify(testData, Diagnostics(RH5417FunctionAndAccessorBodyBracesMustNotShareLineAnalyzer.DiagnosticId, AnalyzerResources.RH5417MessageFormat));
    }

    /// <summary>
    /// Verifies that a single-line property get accessor body is detected and fixed, exercising the code fix path that
    /// formats the whole containing property when the diagnostic anchors on an accessor body
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyPropertyGetAccessorBodyIsDetectedAndFixed()
    {
        const string testData = """
                                public class C
                                {
                                    public int Value { get {|#0:{|} return 1; } }
                                }
                                """;
        const string fixedData = """
                                 public class C
                                 {
                                     public int Value
                                     {
                                         get
                                         {
                                             return 1;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5417FunctionAndAccessorBodyBracesMustNotShareLineAnalyzer.DiagnosticId, AnalyzerResources.RH5417MessageFormat));
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

        await Verify(testData, Diagnostics(RH5417FunctionAndAccessorBodyBracesMustNotShareLineAnalyzer.DiagnosticId, AnalyzerResources.RH5417MessageFormat));
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

        await Verify(testData, Diagnostics(RH5417FunctionAndAccessorBodyBracesMustNotShareLineAnalyzer.DiagnosticId, AnalyzerResources.RH5417MessageFormat));
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

        await Verify(testData, Diagnostics(RH5417FunctionAndAccessorBodyBracesMustNotShareLineAnalyzer.DiagnosticId, AnalyzerResources.RH5417MessageFormat));
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
    /// Verifies that a single-line local function body is detected
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifySingleLineLocalFunctionProducesDiagnostic()
    {
        const string testData = """
                                public class C
                                {
                                    public void Run()
                                    {
                                        void Local() {|#0:{|} }

                                        Local();
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH5417FunctionAndAccessorBodyBracesMustNotShareLineAnalyzer.DiagnosticId, AnalyzerResources.RH5417MessageFormat));
    }

    /// <summary>
    /// Verifies that lambda and anonymous-method function bodies are detected, including nested forms
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyLambdaAndAnonymousMethodBodiesProduceDiagnostics()
    {
        const string testData = """
                                using System;

                                public class C
                                {
                                    public void Run()
                                    {
                                        Action<int> simple = value => {|#0:{|} };
                                        Action<int> parenthesized = (value) => {|#1:{|} };
                                        Action anonymous = delegate {|#2:{|} };
                                        Action nested = () => {|#3:{|} Action inner = () => {|#4:{|} }; };
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH5417FunctionAndAccessorBodyBracesMustNotShareLineAnalyzer.DiagnosticId, AnalyzerResources.RH5417MessageFormat, 5));
    }

    /// <summary>
    /// Verifies that catch blocks remain outside the function-and-accessor policy
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCatchBodyDoesNotProduceDiagnostic()
    {
        const string testData = """
                                public class C
                                {
                                    public void Run()
                                    {
                                        try
                                        {
                                        }
                                        catch { }
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

    /// <summary>
    /// Verifies that the accurate primary names own the exports while the former public names remain obsolete
    /// compatibility shims
    /// </summary>
    [TestMethod]
    public void VerifyPrimaryExportsAndCompatibilityShimsAreUnique()
    {
        var compatibilityAnalyzer = typeof(RH5417FunctionAndAccessorBodyBracesMustNotShareLineAnalyzer).Assembly.GetType("Reihitsu.Analyzer.Rules.Layout.RH5417MemberDeclarationBracesMustNotShareLineAnalyzer",
                                                                                                                         throwOnError: true) ?? throw new System.InvalidOperationException("The RH5417 analyzer compatibility shim is missing.");
        var compatibilityProvider = typeof(RH5417FunctionAndAccessorBodyBracesMustNotShareLineCodeFixProvider).Assembly.GetType("Reihitsu.Analyzer.CodeFixes.Rules.Layout.RH5417MemberDeclarationBracesMustNotShareLineCodeFixProvider",
                                                                                                                                throwOnError: true) ?? throw new System.InvalidOperationException("The RH5417 code-fix compatibility shim is missing.");

        Assert.IsTrue(System.Attribute.IsDefined(compatibilityAnalyzer, typeof(System.ObsoleteAttribute), inherit: false));
        Assert.IsTrue(System.Attribute.IsDefined(compatibilityProvider, typeof(System.ObsoleteAttribute), inherit: false));
        Assert.AreEqual(1,
                        System.Convert.ToInt32(System.Attribute.IsDefined(typeof(RH5417FunctionAndAccessorBodyBracesMustNotShareLineAnalyzer), typeof(Microsoft.CodeAnalysis.Diagnostics.DiagnosticAnalyzerAttribute), inherit: false))
                        + System.Convert.ToInt32(System.Attribute.IsDefined(compatibilityAnalyzer, typeof(Microsoft.CodeAnalysis.Diagnostics.DiagnosticAnalyzerAttribute), inherit: false)));
        Assert.AreEqual(1,
                        System.Convert.ToInt32(System.Attribute.IsDefined(typeof(RH5417FunctionAndAccessorBodyBracesMustNotShareLineCodeFixProvider), typeof(Microsoft.CodeAnalysis.CodeFixes.ExportCodeFixProviderAttribute), inherit: false))
                        + System.Convert.ToInt32(System.Attribute.IsDefined(compatibilityProvider, typeof(Microsoft.CodeAnalysis.CodeFixes.ExportCodeFixProviderAttribute), inherit: false)));
    }

    #endregion // Tests
}