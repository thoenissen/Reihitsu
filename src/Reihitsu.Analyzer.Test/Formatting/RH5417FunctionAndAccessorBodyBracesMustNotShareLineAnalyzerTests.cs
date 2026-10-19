using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5417FunctionAndAccessorBodyBracesMustNotShareLineAnalyzer"/> and <see cref="RH5417FunctionAndAccessorBodyBracesMustNotShareLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5417FunctionAndAccessorBodyBracesMustNotShareLineAnalyzerTests : BatchCodeFixTestsBase<RH5417FunctionAndAccessorBodyBracesMustNotShareLineAnalyzer, RH5417FunctionAndAccessorBodyBracesMustNotShareLineCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that a member body written with Allman braces does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5417-002")]
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
    [Requirement("RH5417-001")]
    [Requirement("RH5417-003")]
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
    [Requirement("RH5417-001")]
    [Requirement("RH5417-003")]
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
    [Requirement("RH5417-001")]
    [Requirement("RH5417-003")]
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
    [Requirement("RH5417-001")]
    [Requirement("RH5417-003")]
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
    [Requirement("RH5417-001")]
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
    [Requirement("RH5417-001")]
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
    [Requirement("RH5417-001")]
    [Requirement("RH5417-003")]
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
    [Requirement("RH5417-001")]
    [Requirement("RH5417-002")]
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
    [Requirement("RH5417-001")]
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
    [Requirement("RH5417-001")]
    [Requirement("RH5417-002")]
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
    [Requirement("RH5417-002")]
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
    [Requirement("RH5417-002")]
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
    [Requirement("RH5417-002")]
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
    [Requirement("RH5417-002")]
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
    [Requirement("RH5417-001")]
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
    [Requirement("RH5417-001")]
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
    [Requirement("RH5417-002")]
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
    [Requirement("RH5417-002")]
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
    /// Verifies that the code fix for block accessors written on one line places the second accessor on its own
    /// line instead of leaving it behind the first accessor's closing brace
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5417-001")]
    [Requirement("RH5417-003")]
    public async Task VerifySingleLineBlockAccessorsAreFixedOntoSeparateLines()
    {
        const string testData = """
                                public class C
                                {
                                    private int _x;

                                    public int Value { get {|#0:{|} var a = _x; return a; } set {|#1:{|} _x = value; _x++; } }
                                }
                                """;
        const string fixedData = """
                                 public class C
                                 {
                                     private int _x;

                                     public int Value
                                     {
                                         get
                                         {
                                             var a = _x;

                                             return a;
                                         }
                                         set
                                         {
                                             _x = value;
                                             _x++;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5417FunctionAndAccessorBodyBracesMustNotShareLineAnalyzer.DiagnosticId, AnalyzerResources.RH5417MessageFormat, 2));
    }

    /// <summary>
    /// Verifies the specification example of reported bodies: the one-line body of a constructor, a finalizer, a
    /// method, an operator, a conversion operator, each accessor kind, a local function, both lambda forms, and an
    /// anonymous method is reported on its opening brace, while the multi-line method holding them is not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5417-001")]
    public async Task VerifySpecificationExampleOfOneLineBodiesIsReported()
    {
        const string testData = """
                                using System;

                                class C
                                {
                                    int _x;

                                    C() {|#0:{|} }

                                    ~C() {|#1:{|} }

                                    int M() {|#2:{|} return 1; }

                                    public static C operator +(C a, C b) {|#3:{|} return a; }

                                    public static explicit operator int(C c) {|#4:{|} return 0; }

                                    int P { get {|#5:{|} return _x; } set {|#6:{|} _x = value; } }

                                    int Q { get => _x; init {|#7:{|} _x = value; } }

                                    event Action E { add {|#8:{|} } remove {|#9:{|} } }

                                    void N()
                                    {
                                        void L() {|#10:{|} }

                                        Action a = () => {|#11:{|} };
                                        Action<int> b = x => {|#12:{|} };
                                        Action c = delegate {|#13:{|} };
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH5417FunctionAndAccessorBodyBracesMustNotShareLineAnalyzer.DiagnosticId, AnalyzerResources.RH5417MessageFormat, 14));
    }

    /// <summary>
    /// Verifies the specification example of blocks that are not reported: an auto-property, an expression body, a
    /// nested block, a statement block, a <see langword="catch"/> block, and a body whose opening brace is followed by
    /// a comment and whose closing brace is on the next line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5417-002")]
    public async Task VerifySpecificationExampleOfOtherBlocksIsNotReported()
    {
        const string testData = """
                                class C
                                {
                                    int P { get; set; }

                                    int N() => 1;

                                    void M(bool a)
                                    {
                                        { }
                                        if (a) { return; }
                                        try
                                        {
                                        }
                                        catch { }
                                    }

                                    void O() { // c
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies the specification example of the fix: the braces of a method body start their own lines with the
    /// statement between them, a comment behind the opening brace stays on the brace's line, and an accessor's
    /// braces start their own lines together with the braces of its accessor list
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5417-003")]
    public async Task VerifySpecificationExampleOfBraceLayoutFix()
    {
        const string testData = """
                                class C
                                {
                                    int _x;

                                    int M() {|#0:{|} return 1; }

                                    void N() {|#1:{|} /* c */ }

                                    int P { get {|#2:{|} return _x; } }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     int _x;

                                     int M()
                                     {
                                         return 1;
                                     }

                                     void N()
                                     { /* c */
                                     }

                                     int P
                                     {
                                         get
                                         {
                                             return _x;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5417FunctionAndAccessorBodyBracesMustNotShareLineAnalyzer.DiagnosticId, AnalyzerResources.RH5417MessageFormat, 3));
    }

    /// <summary>
    /// Verifies that the fix writes the line breaks it inserts with the document's CRLF line ending
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5417-003")]
    public async Task VerifyBraceLayoutFixWithCarriageReturnLineFeeds()
    {
        const string testData = """
                                class C
                                {
                                    int M() { return 1; }
                                }

                                """;
        const string fixedData = """
                                 class C
                                 {
                                     int M()
                                     {
                                         return 1;
                                     }
                                 }

                                 """;

        var actual = await ApplyCodeFixAsync(NormalizeToCarriageReturnLineFeed(testData));

        Assert.AreEqual(NormalizeToCarriageReturnLineFeed(fixedData), actual);
    }

    /// <summary>
    /// Verifies that the one-line body of an operator is fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5417-003")]
    public async Task VerifyOperatorBodyIsFixed()
    {
        const string testData = """
                                public class C
                                {
                                    public static C operator +(C a, C b) {|#0:{|} return a; }
                                }
                                """;
        const string fixedData = """
                                 public class C
                                 {
                                     public static C operator +(C a, C b)
                                     {
                                         return a;
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5417FunctionAndAccessorBodyBracesMustNotShareLineAnalyzer.DiagnosticId, AnalyzerResources.RH5417MessageFormat));
    }

    /// <summary>
    /// Verifies that the one-line body of a conversion operator is fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5417-003")]
    public async Task VerifyConversionOperatorBodyIsFixed()
    {
        const string testData = """
                                public class C
                                {
                                    public static explicit operator int(C c) {|#0:{|} return 0; }
                                }
                                """;
        const string fixedData = """
                                 public class C
                                 {
                                     public static explicit operator int(C c)
                                     {
                                         return 0;
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5417FunctionAndAccessorBodyBracesMustNotShareLineAnalyzer.DiagnosticId, AnalyzerResources.RH5417MessageFormat));
    }

    /// <summary>
    /// Verifies that the one-line body of an indexer accessor is fixed together with the indexer's accessor list
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5417-003")]
    public async Task VerifyIndexerAccessorBodyIsFixed()
    {
        const string testData = """
                                public class C
                                {
                                    public int this[int index] { get {|#0:{|} return index; } }
                                }
                                """;
        const string fixedData = """
                                 public class C
                                 {
                                     public int this[int index]
                                     {
                                         get
                                         {
                                             return index;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5417FunctionAndAccessorBodyBracesMustNotShareLineAnalyzer.DiagnosticId, AnalyzerResources.RH5417MessageFormat));
    }

    /// <summary>
    /// Verifies that the one-line bodies of both event accessors are fixed together with the event's accessor list
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5417-003")]
    public async Task VerifyEventAccessorBodiesAreFixed()
    {
        const string testData = """
                                using System;

                                public class C
                                {
                                    public event Action E { add {|#0:{|} } remove {|#1:{|} } }
                                }
                                """;
        const string fixedData = """
                                 using System;

                                 public class C
                                 {
                                     public event Action E
                                     {
                                         add
                                         {
                                         }
                                         remove
                                         {
                                         }
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5417FunctionAndAccessorBodyBracesMustNotShareLineAnalyzer.DiagnosticId, AnalyzerResources.RH5417MessageFormat, 2));
    }

    /// <summary>
    /// Verifies that the one-line body of an <see langword="init"/> accessor is fixed while it keeps its block body,
    /// and that the expression body of its sibling accessor stays as it is
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5417-003")]
    public async Task VerifyInitAccessorBodyIsFixedAndKeepsItsBlockBody()
    {
        const string testData = """
                                public class C
                                {
                                    private int _x;

                                    public int Q { get => _x; init {|#0:{|} _x = value; } }
                                }
                                """;
        const string fixedData = """
                                 public class C
                                 {
                                     private int _x;

                                     public int Q
                                     {
                                         get => _x;
                                         init
                                         {
                                             _x = value;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5417FunctionAndAccessorBodyBracesMustNotShareLineAnalyzer.DiagnosticId, AnalyzerResources.RH5417MessageFormat));
    }

    /// <summary>
    /// Verifies the specification example of bodies without a fix: the one-line bodies of a local function, a lambda,
    /// and an anonymous method are reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5417-004")]
    public async Task VerifySpecificationExampleOfBodiesWithoutCodeFix()
    {
        const string testData = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        void L() {|#0:{|} }

                                        Action a = () => {|#1:{|} };
                                        Action c = delegate {|#2:{|} };
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData,
                                       test => test.ExpectedDiagnostics.AddRange(Diagnostics(RH5417FunctionAndAccessorBodyBracesMustNotShareLineAnalyzer.DiagnosticId, AnalyzerResources.RH5417MessageFormat, 3)));
    }

    /// <summary>
    /// Verifies that the one-line body of a simple lambda is reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5417-004")]
    public async Task VerifySimpleLambdaBodyIsReportedWithoutCodeFix()
    {
        const string testData = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action<int> b = x => {|#0:{|} };
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData,
                                       test => test.ExpectedDiagnostics.AddRange(Diagnostics(RH5417FunctionAndAccessorBodyBracesMustNotShareLineAnalyzer.DiagnosticId, AnalyzerResources.RH5417MessageFormat)));
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                public class C
                                {
                                    public void First() {|#0:{|} }
                                    public void Second() {|#1:{|} }
                                }
                                """;

        const string fixedCode = """
                                 public class C
                                 {
                                     public void First()
                                     {
                                     }
                                     public void Second()
                                     {
                                     }
                                 }
                                 """;

        // Both bodies resolve to their own distinct member declaration, so the two fixes reformat unrelated nodes;
        // the members are adjacent with no blank line between them, exercising the batch fixer's handling of
        // closely spaced but non-overlapping replacements
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH5417FunctionAndAccessorBodyBracesMustNotShareLineAnalyzer.DiagnosticId, AnalyzerResources.RH5417MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}