using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Naming;
using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Naming;

/// <summary>
/// Test methods for <see cref="RH4121TypeParameterNameCasingAnalyzer"/> and <see cref="RH4121TypeParameterNameCasingCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH4121TypeParameterNameCasingAnalyzerTests : BatchCodeFixTestsBase<RH4121TypeParameterNameCasingAnalyzer, RH4121TypeParameterNameCasingCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying diagnostics for a class type parameter without the 'T' prefix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4121-001")]
    [Requirement("RH4121-002")]
    [Requirement("RH4121-003")]
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
    [Requirement("RH4121-001")]
    [Requirement("RH4121-002")]
    [Requirement("RH4121-003")]
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
    [Requirement("RH4121-001")]
    [Requirement("RH4121-003")]
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
    [Requirement("RH4121-001")]
    [Requirement("RH4121-003")]
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
    [Requirement("RH4121-001")]
    [Requirement("RH4121-003")]
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
    [Requirement("RH4121-002")]
    [Requirement("RH4121-003")]
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
    [Requirement("RH4121-002")]
    [Requirement("RH4121-003")]
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
    [Requirement("RH4121-002")]
    [Requirement("RH4121-003")]
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
    [Requirement("RH4121-001")]
    [Requirement("RH4121-003")]
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
    [Requirement("RH4121-002")]
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
    [Requirement("RH4121-002")]
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
    [Requirement("RH4121-002")]
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
    [Requirement("RH4121-002")]
    public async Task VerifyNoDiagnosticsForNumberedTypeParameters()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                public class Pair<T1, T2>;
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies the type parameters of classes, methods, local functions, structs, interfaces, records, and delegates are reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4121-001")]
    public async Task TypeParametersOfEveryGenericDeclarationAreChecked()
    {
        const string testCode = """
                                public class C<{|RH4121:Key|}>
                                {
                                    public void M<{|RH4121:item|}>()
                                    {
                                        void Local<{|RH4121:arg|}>()
                                        {
                                        }
                                    }
                                }

                                public struct S<{|RH4121:value|}>
                                {
                                }

                                public interface I<{|RH4121:element|}>
                                {
                                }

                                public record R<{|RH4121:entity|}>;

                                public delegate void D<{|RH4121:result|}>();
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies each part of a partial type reports its own type parameter
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4121-001")]
    public async Task PartialTypeTypeParameterIsReportedOnEachPart()
    {
        const string testCode = """
                                public partial class P<{|RH4121:key|}>
                                {
                                }

                                public partial class P<{|RH4121:key|}>
                                {
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies each part of a partial method reports its own type parameter
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4121-001")]
    public async Task PartialMethodTypeParameterIsReportedOnEachPart()
    {
        const string testCode = """
                                public partial class P
                                {
                                    partial void M<{|RH4121:item|}>();
                                }

                                public partial class P
                                {
                                    partial void M<{|RH4121:item|}>()
                                    {
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies T alone, T and a PascalCase name, and T and digits are accepted, while every other name is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4121-002")]
    public async Task TypeParameterNameIsTTAndPascalCaseOrTAndDigits()
    {
        const string testCode = """
                                public class C<T, TKey, TKEY, T1, T23, {|RH4121:Key|}, {|RH4121:tKey|}, {|RH4121:t1|}, {|RH4121:Type|}, {|RH4121:T1Key|}>
                                {
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies the fix corrects a leading t or T before an uppercase letter or digit, puts T in front of any other name, derives a PascalCase body, and renames every reference
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4121-003")]
    public async Task FixAddsOrCorrectsTheTPrefixAndRenamesEveryReference()
    {
        const string testCode = """
                                public class C<{|RH4121:Key|}, {|RH4121:tValue|}, {|RH4121:t1|}, {|RH4121:item_type|}, {|RH4121:Type|}>
                                {
                                    public Key M(tValue value, t1 index, item_type item, Type type) => default;
                                }

                                public class D<{|RH4121:t|}>
                                {
                                }
                                """;

        const string fixedCode = """
                                 public class C<TKey, TValue, T1, TItemType, TType>
                                 {
                                     public TKey M(TValue value, T1 index, TItemType item, TType type) => default;
                                 }

                                 public class D<T>
                                 {
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies a type parameter without a letter or digit and one whose derived body is neither PascalCase nor digits are reported, and no code fix is offered for either
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4121-004")]
    public async Task FixIsWithheldWhenNoUsableNameCanBeDerived()
    {
        const string testCode = """
                                public class C<{|RH4121:_|}, {|RH4121:T1Key|}>
                                {
                                }
                                """;

        await Verify(testCode);

        foreach (var name in new[] { "_", "T1Key" })
        {
            var actions = await GetCodeFixActionsAsync(testCode.Replace("{|RH4121:", string.Empty).Replace("|}", string.Empty),
                                                       RH4121TypeParameterNameCasingAnalyzer.DiagnosticId,
                                                       root => root.DescendantNodes()
                                                                   .OfType<TypeParameterSyntax>()
                                                                   .Single(typeParameter => typeParameter.Identifier.ValueText == name)
                                                                   .Identifier
                                                                   .GetLocation());

            Assert.IsEmpty(actions, name);
        }
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class Cache<{|#0:key|}, {|#1:value|}>
                                    {
                                        public key Key { get; set; }

                                        public value Value { get; set; }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     public class Cache<TKey, TValue>
                                     {
                                         public TKey Key { get; set; }

                                         public TValue Value { get; set; }
                                     }
                                 }
                                 """;

        // Both type parameters belong to the same parameter list and are used by the members, so each rename
        // has to reach references outside its own declaration
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH4121TypeParameterNameCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4121MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}