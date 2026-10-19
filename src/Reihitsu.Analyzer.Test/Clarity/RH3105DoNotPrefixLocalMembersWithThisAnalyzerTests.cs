using System.Threading.Tasks;

using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Clarity;
using Reihitsu.Analyzer.Rules.Clarity;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Clarity;

/// <summary>
/// Test methods for <see cref="RH3105DoNotPrefixLocalMembersWithThisAnalyzer"/> and <see cref="RH3105DoNotPrefixLocalMembersWithThisCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH3105DoNotPrefixLocalMembersWithThisAnalyzerTests : BatchCodeFixTestsBase<RH3105DoNotPrefixLocalMembersWithThisAnalyzer, RH3105DoNotPrefixLocalMembersWithThisCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying unnecessary this qualifier on field access is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3105-001")]
    [Requirement("RH3105-003")]
    [Requirement("RH3105-004")]
    public async Task UnnecessaryThisQualifierOnFieldIsReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int _value;

                                    public int Run()
                                    {
                                        return {|#0:this|}._value;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private int _value;

                                     public int Run()
                                     {
                                         return _value;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3105DoNotPrefixLocalMembersWithThisAnalyzer.DiagnosticId, "Do not prefix local members with this."));
    }

    /// <summary>
    /// Verifying unnecessary this qualifier on property access is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3105-001")]
    [Requirement("RH3105-003")]
    [Requirement("RH3105-004")]
    public async Task UnnecessaryThisQualifierOnPropertyIsReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    public int Value { get; set; }

                                    public int GetValue()
                                    {
                                        return {|#0:this|}.Value;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public int Value { get; set; }

                                     public int GetValue()
                                     {
                                         return Value;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3105DoNotPrefixLocalMembersWithThisAnalyzer.DiagnosticId, "Do not prefix local members with this."));
    }

    /// <summary>
    /// Verifying unnecessary this qualifier on method invocation is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3105-001")]
    [Requirement("RH3105-003")]
    [Requirement("RH3105-004")]
    public async Task UnnecessaryThisQualifierOnMethodIsReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int Calculate()
                                    {
                                        return 42;
                                    }

                                    public int Run()
                                    {
                                        return {|#0:this|}.Calculate();
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private int Calculate()
                                     {
                                         return 42;
                                     }

                                     public int Run()
                                     {
                                         return Calculate();
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3105DoNotPrefixLocalMembersWithThisAnalyzer.DiagnosticId, "Do not prefix local members with this."));
    }

    /// <summary>
    /// Verifying unnecessary this qualifier on chained member access is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3105-001")]
    [Requirement("RH3105-003")]
    [Requirement("RH3105-004")]
    public async Task UnnecessaryThisQualifierOnChainedAccessIsReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    private string _text = "Hello";

                                    public int Run()
                                    {
                                        return {|#0:this|}._text.Length;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private string _text = "Hello";

                                     public int Run()
                                     {
                                         return _text.Length;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3105DoNotPrefixLocalMembersWithThisAnalyzer.DiagnosticId, "Do not prefix local members with this."));
    }

    /// <summary>
    /// Verifying necessary this qualifier when shadowed by parameter is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3105-002")]
    public async Task NecessaryThisQualifierWithShadowingParameterIsNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int _value;

                                    public void SetValue(int _value)
                                    {
                                        this._value = _value;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying necessary this qualifier when shadowed by local variable is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3105-002")]
    public async Task NecessaryThisQualifierWithShadowingLocalIsNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int _value;

                                    public void Run()
                                    {
                                        int _value = 0;
                                        this._value = _value;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying unnecessary this qualifier in assignment target is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3105-001")]
    [Requirement("RH3105-003")]
    [Requirement("RH3105-004")]
    public async Task UnnecessaryThisQualifierInAssignmentIsReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int _value;

                                    public void Run()
                                    {
                                        {|#0:this|}._value = 42;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private int _value;

                                     public void Run()
                                     {
                                         _value = 42;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3105DoNotPrefixLocalMembersWithThisAnalyzer.DiagnosticId, "Do not prefix local members with this."));
    }

    /// <summary>
    /// Verifying unnecessary this qualifier with method arguments is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3105-001")]
    [Requirement("RH3105-003")]
    [Requirement("RH3105-004")]
    public async Task UnnecessaryThisQualifierWithMethodArgumentsIsReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int Add(int left, int right)
                                    {
                                        return left + right;
                                    }

                                    public int Run()
                                    {
                                        return {|#0:this|}.Add(1, 2);
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private int Add(int left, int right)
                                     {
                                         return left + right;
                                     }

                                     public int Run()
                                     {
                                         return Add(1, 2);
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3105DoNotPrefixLocalMembersWithThisAnalyzer.DiagnosticId, "Do not prefix local members with this."));
    }

    /// <summary>
    /// Verifying unnecessary this qualifier in nested scope is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3105-001")]
    [Requirement("RH3105-003")]
    [Requirement("RH3105-004")]
    public async Task UnnecessaryThisQualifierInNestedScopeIsReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int _value;

                                    public int Run()
                                    {
                                        if (true)
                                        {
                                            return {|#0:this|}._value;
                                        }

                                        return 0;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private int _value;

                                     public int Run()
                                     {
                                         if (true)
                                         {
                                             return _value;
                                         }

                                         return 0;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3105DoNotPrefixLocalMembersWithThisAnalyzer.DiagnosticId, "Do not prefix local members with this."));
    }

    /// <summary>
    /// Verifying a this-qualified access to a member that does not exist is not reported, since neither the
    /// original nor the speculative rebinding resolves to a symbol or a candidate
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3105-002")]
    public async Task ThisQualifierOnUnresolvableMemberIsNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    public void Run()
                                    {
                                        this.Missing = 1;
                                    }
                                }
                                """;

        await Verify(testCode, test => test.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifies <c>this.</c> accesses of a field, a property, and a method, also inside a lambda and inside an
    /// argument, are reported on <c>this</c> and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3105-001")]
    [Requirement("RH3105-003")]
    public async Task ThisAccessesAreReportedOnTheThisKeywordAlsoInsideALambda()
    {
        const string testCode = """
                                class C
                                {
                                    int _a;
                                    int P { get; set; }

                                    int M()
                                    {
                                        {|RH3105:this|}._a = {|RH3105:this|}.P;
                                        System.Func<int> f = () => {|RH3105:this|}._a;
                                        return {|RH3105:this|}.M2({|RH3105:this|}._a).GetHashCode();
                                    }

                                    int M2(int x) => x;
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     int _a;
                                     int P { get; set; }

                                     int M()
                                     {
                                         _a = P;
                                         System.Func<int> f = () => _a;
                                         return M2(_a).GetHashCode();
                                     }

                                     int M2(int x) => x;
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies a <c>this.</c> access that selects a field shadowed by a parameter is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3105-002")]
    public async Task ThisAccessThatSelectsAShadowedFieldIsNotReported()
    {
        const string testCode = """
                                class C
                                {
                                    int a;

                                    void M(int a)
                                    {
                                        this.a = a;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies the fix removes the qualifier of a field, a property, and a method access
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3105-004")]
    public async Task FixRemovesTheThisQualifier()
    {
        const string testCode = """
                                class C
                                {
                                    int _a;
                                    int P { get; set; }

                                    int M()
                                    {
                                        {|RH3105:this|}._a = {|RH3105:this|}.P;
                                        return {|RH3105:this|}.M2({|RH3105:this|}._a).GetHashCode();
                                    }

                                    int M2(int x) => x;
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     int _a;
                                     int P { get; set; }

                                     int M()
                                     {
                                         _a = P;
                                         return M2(_a).GetHashCode();
                                     }

                                     int M2(int x) => x;
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Characterizes that the fix removes comments on both sides of the dot together with the qualifier
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task FixRemovesCommentsAroundTheDotWithTheQualifier()
    {
        const string testCode = """
                                class C
                                {
                                    int _a;

                                    int M()
                                    {
                                        return {|RH3105:this|} /* a */ . /* b */ _a;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     int _a;

                                     int M()
                                     {
                                         return _a;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int _value;
                                    private int _count;

                                    public int Run()
                                    {
                                        return {|#0:this|}._value + {|#1:this|}._count;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private int _value;
                                     private int _count;

                                     public int Run()
                                     {
                                         return _value + _count;
                                     }
                                 }
                                 """;

        // Both qualifiers sit in the same expression, so the two rewrites replace siblings of one parent node
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH3105DoNotPrefixLocalMembersWithThisAnalyzer.DiagnosticId, "Do not prefix local members with this.", 2));
    }

    #endregion // BatchCodeFixTestsBase
}