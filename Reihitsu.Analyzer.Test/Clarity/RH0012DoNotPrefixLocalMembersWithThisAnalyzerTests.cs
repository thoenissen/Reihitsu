using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Clarity;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Clarity;

/// <summary>
/// Test methods for <see cref="RH0012DoNotPrefixLocalMembersWithThisAnalyzer"/> and <see cref="RH0012DoNotPrefixLocalMembersWithThisCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0012DoNotPrefixLocalMembersWithThisAnalyzerTests : AnalyzerTestsBase<RH0012DoNotPrefixLocalMembersWithThisAnalyzer, RH0012DoNotPrefixLocalMembersWithThisCodeFixProvider>
{
    #region Members

    /// <summary>
    /// Verifying unnecessary this qualifier on field access is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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

        await Verify(testCode, fixedCode, Diagnostics(RH0012DoNotPrefixLocalMembersWithThisAnalyzer.DiagnosticId, "Do not prefix local members with this."));
    }

    /// <summary>
    /// Verifying unnecessary this qualifier on property access is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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

        await Verify(testCode, fixedCode, Diagnostics(RH0012DoNotPrefixLocalMembersWithThisAnalyzer.DiagnosticId, "Do not prefix local members with this."));
    }

    /// <summary>
    /// Verifying unnecessary this qualifier on method invocation is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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

        await Verify(testCode, fixedCode, Diagnostics(RH0012DoNotPrefixLocalMembersWithThisAnalyzer.DiagnosticId, "Do not prefix local members with this."));
    }

    /// <summary>
    /// Verifying unnecessary this qualifier on chained member access is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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

        await Verify(testCode, fixedCode, Diagnostics(RH0012DoNotPrefixLocalMembersWithThisAnalyzer.DiagnosticId, "Do not prefix local members with this."));
    }

    /// <summary>
    /// Verifying multiple unnecessary this qualifiers are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task MultipleUnnecessaryThisQualifiersAreReportedAndFixed()
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

        await Verify(testCode, fixedCode, Diagnostics(RH0012DoNotPrefixLocalMembersWithThisAnalyzer.DiagnosticId, "Do not prefix local members with this.", 2));
    }

    /// <summary>
    /// Verifying necessary this qualifier when shadowed by parameter is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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

        await Verify(testCode, fixedCode, Diagnostics(RH0012DoNotPrefixLocalMembersWithThisAnalyzer.DiagnosticId, "Do not prefix local members with this."));
    }

    /// <summary>
    /// Verifying unnecessary this qualifier with method arguments is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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

        await Verify(testCode, fixedCode, Diagnostics(RH0012DoNotPrefixLocalMembersWithThisAnalyzer.DiagnosticId, "Do not prefix local members with this."));
    }

    /// <summary>
    /// Verifying unnecessary this qualifier in nested scope is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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

        await Verify(testCode, fixedCode, Diagnostics(RH0012DoNotPrefixLocalMembersWithThisAnalyzer.DiagnosticId, "Do not prefix local members with this."));
    }

    #endregion // Members
}