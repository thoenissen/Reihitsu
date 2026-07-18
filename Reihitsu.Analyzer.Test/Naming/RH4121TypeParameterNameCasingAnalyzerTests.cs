using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Naming;
using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Naming;

/// <summary>
/// Test methods for <see cref="RH4121TypeParameterNameCasingAnalyzer"/> and <see cref="RH4121TypeParameterNameCasingCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH4121TypeParameterNameCasingAnalyzerTests : AnalyzerTestsBase<RH4121TypeParameterNameCasingAnalyzer, RH4121TypeParameterNameCasingCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying diagnostics for a class type parameter without the 'T' prefix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticsForClassTypeParameterWithoutPrefix()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                public class Cache<{|#0:Key|}>
                                {
                                    public Key GetKey()
                                    {
                                        return default;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                 public class Cache<TKey>
                                 {
                                     public TKey GetKey()
                                     {
                                         return default;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH4121TypeParameterNameCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4121MessageFormat));
    }

    /// <summary>
    /// Verifying diagnostics for a camelCase method type parameter
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticsForCamelCaseMethodTypeParameter()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                public class TestClass
                                {
                                    public value Get<{|#0:value|}>(value input)
                                    {
                                        return input;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                 public class TestClass
                                 {
                                     public TValue Get<TValue>(TValue input)
                                     {
                                         return input;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH4121TypeParameterNameCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4121MessageFormat));
    }

    /// <summary>
    /// Verifying diagnostics for a delegate type parameter without the 'T' prefix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticsForDelegateTypeParameterWithoutPrefix()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                public delegate void Handler<{|#0:Message|}>(Message message);
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                 public delegate void Handler<TMessage>(TMessage message);
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH4121TypeParameterNameCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4121MessageFormat));
    }

    /// <summary>
    /// Verifying diagnostics for a local function type parameter without the 'T' prefix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticsForLocalFunctionTypeParameterWithoutPrefix()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                public class TestClass
                                {
                                    public void Process()
                                    {
                                        void Local<{|#0:Item|}>(Item item)
                                        {
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                 public class TestClass
                                 {
                                     public void Process()
                                     {
                                         void Local<TItem>(TItem item)
                                         {
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH4121TypeParameterNameCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4121MessageFormat));
    }

    /// <summary>
    /// Verifying diagnostics for a record type parameter without the 'T' prefix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticsForRecordTypeParameterWithoutPrefix()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                public record Box<{|#0:Content|}>(Content Value);
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                 public record Box<TContent>(TContent Value);
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH4121TypeParameterNameCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4121MessageFormat));
    }

    /// <summary>
    /// Verifying diagnostics for a type parameter with a lowercase 't' prefix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticsForTypeParameterWithLowercasePrefix()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                public class TestClass
                                {
                                    public void Process<{|#0:tValue|}>()
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                 public class TestClass
                                 {
                                     public void Process<TValue>()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH4121TypeParameterNameCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4121MessageFormat));
    }

    /// <summary>
    /// Verifying diagnostics for a type parameter with a lowercase 't' prefix followed by a digit
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticsForTypeParameterWithLowercasePrefixAndDigitSuffix()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                public class TestClass
                                {
                                    public void Process<{|#0:t1|}>()
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                 public class TestClass
                                 {
                                     public void Process<T1>()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH4121TypeParameterNameCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4121MessageFormat));
    }

    /// <summary>
    /// Verifying diagnostics for a type parameter named like a word starting with 'T'
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticsForTypeParameterNamedLikeWordStartingWithT()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                public class TestClass
                                {
                                    public void Process<{|#0:Type|}>()
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                 public class TestClass
                                 {
                                     public void Process<TType>()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH4121TypeParameterNameCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4121MessageFormat));
    }

    /// <summary>
    /// Verifying diagnostics for multiple type parameters without the 'T' prefix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticsForMultipleTypeParametersWithoutPrefix()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                public class Pair<{|#0:First|}, {|#1:Second|}>;
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                 public class Pair<TFirst, TSecond>;
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH4121TypeParameterNameCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4121MessageFormat, 2));
    }

    /// <summary>
    /// Verifying no diagnostics for a single 'T' type parameter
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForSingleTTypeParameter()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                public class Repository<T>;
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying no diagnostics for 'T'-prefixed PascalCase type parameters
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForPrefixedTypeParameters()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                public class Cache<TKey, TValue>;
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying no diagnostics for a 'T'-prefixed method type parameter
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForPrefixedMethodTypeParameter()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                public class TestClass
                                {
                                    public TResult Get<TResult>()
                                    {
                                        return default;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying no diagnostics for BCL-standard numbered type parameters such as 'T1'/'T2'
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForNumberedTypeParameters()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                public class Pair<T1, T2>;
                                """;

        await Verify(testCode);
    }

    #endregion // Tests
}