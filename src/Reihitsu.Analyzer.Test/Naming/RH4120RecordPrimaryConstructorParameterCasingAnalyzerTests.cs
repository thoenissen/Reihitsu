using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Naming;
using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Naming;

/// <summary>
/// Test methods for <see cref="RH4120RecordPrimaryConstructorParameterCasingAnalyzer"/> and <see cref="RH4120RecordPrimaryConstructorParameterCasingCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH4120RecordPrimaryConstructorParameterCasingAnalyzerTests : BatchCodeFixTestsBase<RH4120RecordPrimaryConstructorParameterCasingAnalyzer, RH4120RecordPrimaryConstructorParameterCasingCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying diagnostics for camelCase record primary constructor parameters
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4120-001")]
    [Requirement("RH4120-003")]
    public async Task VerifyDiagnosticsForRecordPrimaryConstructorParameterWrongCasing()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                public record Product(string {|#0:productCode|});
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                 public record Product(string ProductCode);
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH4120RecordPrimaryConstructorParameterCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4120MessageFormat));
    }

    /// <summary>
    /// Verifying the code fix renames record primary constructor parameter references
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4120-001")]
    [Requirement("RH4120-003")]
    public async Task VerifyCodeFixRenamesRecordPrimaryConstructorParameterReferences()
    {
        const string testCode = """
                                public record R(string {|#0:badName|});

                                public class C
                                {
                                    void M()
                                    {
                                        var r = new R(badName: "x");
                                        var v = r.badName;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public record R(string BadName);

                                 public class C
                                 {
                                     void M()
                                     {
                                         var r = new R(BadName: "x");
                                         var v = r.BadName;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH4120RecordPrimaryConstructorParameterCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4120MessageFormat));
    }

    /// <summary>
    /// Verifying diagnostics for camelCase record struct primary constructor parameters
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4120-001")]
    [Requirement("RH4120-003")]
    public async Task VerifyDiagnosticsForRecordStructPrimaryConstructorParameterWrongCasing()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                public record struct ProductId(string {|#0:valueCode|});
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                 public record struct ProductId(string ValueCode);
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH4120RecordPrimaryConstructorParameterCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4120MessageFormat));
    }

    /// <summary>
    /// Verifying no diagnostics for PascalCase record primary constructor parameters
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4120-001")]
    public async Task VerifyNoDiagnosticsForPascalCaseRecordPrimaryConstructorParameter()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                public record Product(string ProductCode);
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying no diagnostics for method parameters
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4120-002")]
    public async Task VerifyNoDiagnosticsForMethodParameter()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                public class TestClass
                                {
                                    public void Process(string productCode)
                                    {
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies the primary constructor parameters of a record, record class, record struct, and readonly record struct are reported, while PascalCase names are not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4120-001")]
    public async Task PrimaryConstructorParametersOfEveryRecordKindAreChecked()
    {
        const string testCode = """
                                public record R(int {|RH4120:firstName|}, int LastName);

                                public record class C(int {|RH4120:last_name|});

                                public record struct S(int {|RH4120:count|});

                                public readonly record struct T(int {|RH4120:size|});
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies a record primary constructor parameter named with an underscore, an explicit record constructor, a record method, and class and struct primary constructors are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4120-002")]
    public async Task DiscardsAndParametersOfOtherConstructorsAndMembersAreNotChecked()
    {
        const string testCode = """
                                public record R(int _, int Id)
                                {
                                    public R(string text)
                                        : this(0, text.Length)
                                    {
                                    }

                                    public void M(int input)
                                    {
                                    }
                                }

                                public class C(int value);

                                public struct S(int value);
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies the fix renames the record parameter, the property it declares, and every reference to them to the derived PascalCase name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4120-003")]
    public async Task FixRenamesParameterItsPropertyAndEveryReferenceToPascalCase()
    {
        const string testCode = """
                                public record R(int {|RH4120:firstName|})
                                {
                                    public int Twice => firstName * 2;
                                }

                                public class U
                                {
                                    public int M(R r) => r.firstName;
                                }
                                """;

        const string fixedCode = """
                                 public record R(int FirstName)
                                 {
                                     public int Twice => FirstName * 2;
                                 }

                                 public class U
                                 {
                                     public int M(R r) => r.FirstName;
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
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public record Order(int {|#0:orderId|}, string {|#1:customerName|});

                                    public class OrderFactory
                                    {
                                        public string Describe()
                                        {
                                            var order = new Order(orderId: 1, customerName: "Ada");

                                            return order.customerName + order.orderId;
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     public record Order(int OrderId, string CustomerName);

                                     public class OrderFactory
                                     {
                                         public string Describe()
                                         {
                                             var order = new Order(OrderId: 1, CustomerName: "Ada");

                                             return order.CustomerName + order.OrderId;
                                         }
                                     }
                                 }
                                 """;

        // Both parameters belong to the same primary constructor, so each rename also rewrites the
        // synthesized property and the named argument that reads it
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH4120RecordPrimaryConstructorParameterCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4120MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}