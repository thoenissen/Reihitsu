using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Naming;
using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Naming;

/// <summary>
/// Test methods for <see cref="RH4105MethodParameterCasingAnalyzer"/> and <see cref="RH4105MethodParameterCasingCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH4105MethodParameterCasingAnalyzerTests : BatchCodeFixTestsBase<RH4105MethodParameterCasingAnalyzer, RH4105MethodParameterCasingCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4105-001")]
    [Requirement("RH4105-003")]
    public async Task VerifyDiagnostics()
    {
        const string testCode = """
                                using System;

                                namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                /// <summary>
                                /// Test class
                                /// </summary>
                                /// <param name="PrimaryParameterName">Primary parameter</param>
                                public class TestClass(int {|#0:PrimaryParameterName|})
                                {
                                    /// <summary>
                                    /// Test method
                                    /// </summary>
                                    /// <param name="MethodParameterName">Method parameter</param>
                                    public void TestMethod(int {|#1:MethodParameterName|})
                                    {
                                    }
                                }

                                /// <summary>
                                /// Test struct
                                /// </summary>
                                /// <param name="ParameterName">Parameter</param>
                                public struct TestStruct(int {|#2:ParameterName|});
                                """;

        const string fixedCode = """
                                 using System;

                                 namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                 /// <summary>
                                 /// Test class
                                 /// </summary>
                                 /// <param name="primaryParameterName">Primary parameter</param>
                                 public class TestClass(int primaryParameterName)
                                 {
                                     /// <summary>
                                     /// Test method
                                     /// </summary>
                                     /// <param name="methodParameterName">Method parameter</param>
                                     public void TestMethod(int methodParameterName)
                                     {
                                     }
                                 }

                                 /// <summary>
                                 /// Test struct
                                 /// </summary>
                                 /// <param name="parameterName">Parameter</param>
                                 public struct TestStruct(int parameterName);
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH4105MethodParameterCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4105MessageFormat, 3));
    }

    /// <summary>
    /// Verifying no diagnostics for record class primary constructor parameters
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4105-002")]
    public async Task VerifyNoDiagnosticsForRecordPrimaryConstructor()
    {
        const string testCode = """
                                using System;

                                namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                public record class TestRecord(int ParameterName);

                                public record struct TestRecordStruct(int ParameterName);

                                public class TestClass(int parameterName);

                                public struct TestStruct(int parameterName);
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying no diagnostics for a camelCase method parameter
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4105-001")]
    public async Task VerifyNoDiagnosticsForCamelCaseMethodParameter()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class TestClass
                                    {
                                        public void ProcessData(int itemCount)
                                        {
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying diagnostics for a constructor parameter with wrong casing
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4105-001")]
    [Requirement("RH4105-003")]
    public async Task VerifyDiagnosticsForConstructorParameterWrongCasing()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class TestClass
                                    {
                                        public TestClass(int {|#0:Count|})
                                        {
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     public class TestClass
                                     {
                                         public TestClass(int count)
                                         {
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH4105MethodParameterCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4105MessageFormat));
    }

    /// <summary>
    /// Verifying diagnostics for a local function parameter with wrong casing
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4105-001")]
    [Requirement("RH4105-003")]
    public async Task VerifyDiagnosticsForLocalFunctionParameterWrongCasing()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class TestClass
                                    {
                                        public void Method()
                                        {
                                            void LocalFunc(int {|#0:Count|})
                                            {
                                            }
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     public class TestClass
                                     {
                                         public void Method()
                                         {
                                             void LocalFunc(int count)
                                             {
                                             }
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH4105MethodParameterCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4105MessageFormat));
    }

    /// <summary>
    /// Verifying no diagnostics for a discard parameter named '_'
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4105-002")]
    public async Task VerifyNoDiagnosticsForDiscardParameter()
    {
        const string testCode = """
                                using System;

                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class TestClass
                                    {
                                        public void Method(object _, EventArgs e)
                                        {
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no code fix is offered for a letterless method parameter whose conversion cannot produce a valid identifier
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-008")]
    public async Task NoCodeFixForLetterlessMethodParameter()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class TestClass
                                    {
                                        public void Method(int __)
                                        {
                                        }
                                    }
                                }
                                """;

        var actions = await GetCodeFixActionsAsync(testCode,
                                                   RH4105MethodParameterCasingAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<ParameterSyntax>()
                                                               .Single(parameter => parameter.Identifier.ValueText == "__")
                                                               .Identifier
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifying diagnostics for a lambda parameter with wrong casing
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4105-001")]
    [Requirement("RH4105-003")]
    public async Task VerifyDiagnosticsForLambdaParameterWrongCasing()
    {
        const string testCode = """
                                using System;

                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class TestClass
                                    {
                                        public void Method()
                                        {
                                            Action<int> action = (int {|#0:Value|}) => { };
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     public class TestClass
                                     {
                                         public void Method()
                                         {
                                             Action<int> action = (int value) => { };
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH4105MethodParameterCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4105MessageFormat));
    }

    /// <summary>
    /// Verifying no diagnostics for a lambda parameter with camelCase name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4105-001")]
    public async Task VerifyNoDiagnosticsForLambdaCamelCaseParameter()
    {
        const string testCode = """
                                using System;

                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class TestClass
                                    {
                                        public void Method()
                                        {
                                            Action<int> action = (int value) => { };
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying diagnostics for an indexer parameter with wrong casing
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4105-001")]
    [Requirement("RH4105-003")]
    public async Task VerifyDiagnosticsForIndexerParameterWrongCasing()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class TestClass
                                    {
                                        public int this[int {|#0:Index|}] => 0;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     public class TestClass
                                     {
                                         public int this[int index] => 0;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH4105MethodParameterCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4105MessageFormat));
    }

    /// <summary>
    /// Verifies that camelCase accepts a lowercase start followed by letters and digits, and rejects an uppercase start, a separator, and a leading underscore
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-002")]
    [Requirement("RH4105-001")]
    public async Task CamelCaseAcceptsLettersAndDigitsAndRejectsUppercaseStartsAndSeparators()
    {
        const string testCode = """
                                class C
                                {
                                    void M(int count2, int {|RH4105:Count|}, int {|RH4105:item_count|}, int {|RH4105:_total|})
                                    {
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that parameters of delegates, primary constructors of classes and structs, constructors, indexers, operators, local functions, lambdas, and anonymous methods are each reported on their identifier
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4105-001")]
    public async Task ParametersOfEveryKindOfMemberAreChecked()
    {
        const string testCode = """
                                using System;

                                public delegate void D(int {|RH4105:Value|});

                                public class C(int {|RH4105:PrimaryValue|})
                                {
                                    public C(int {|RH4105:Count|}, int itemCount)
                                        : this(Count + itemCount)
                                    {
                                    }

                                    public int this[int {|RH4105:Index|}] => Index;

                                    public static C operator +(C {|RH4105:Left|}, C right) => Left;

                                    public void M()
                                    {
                                        void Local(int {|RH4105:Total|})
                                        {
                                        }

                                        Func<int, int> f = {|RH4105:Number|} => Number;
                                        Action<int> g = (int {|RH4105:Amount|}) => { };
                                        Action<int> h = delegate (int {|RH4105:Size|}) { };
                                    }
                                }

                                public struct S(int {|RH4105:ParameterName|});
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that discard parameters, lambda discards, and the primary constructor parameters of record classes and record structs are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4105-002")]
    public async Task DiscardsAndRecordPrimaryConstructorParametersAreNotChecked()
    {
        const string testCode = """
                                using System;

                                public record class R(int ParameterName);

                                public record struct S(int ParameterName);

                                public class C
                                {
                                    public void M(object _, EventArgs e)
                                    {
                                        Func<int, int, int> f = (_, _) => 0;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that the fix renames a constructor parameter and a lambda parameter to camelCase together with their uses, including the contextual keyword value
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4105-003")]
    public async Task FixRenamesConstructorAndLambdaParametersAndEveryReference()
    {
        const string testCode = """
                                using System;

                                public class C
                                {
                                    public C(int {|RH4105:Count|})
                                    {
                                        Func<int, int> f = {|RH4105:Value|} => Value + Count;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 public class C
                                 {
                                     public C(int count)
                                     {
                                         Func<int, int> f = value => value + count;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that the fix lowercases the first letter of a PascalCase name, lowercases a shouted word, and joins words separated by an underscore
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-006")]
    [Requirement("RH4105-003")]
    public async Task FixDerivesCamelCaseFromTheWordsOfTheOldName()
    {
        const string testCode = """
                                class C
                                {
                                    int M(int {|RH4105:FirstName|}, int {|RH4105:LAST_NAME|}, int {|RH4105:Input_Value|})
                                    {
                                        return FirstName + LAST_NAME + Input_Value;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     int M(int firstName, int lastName, int inputValue)
                                     {
                                         return firstName + lastName + inputValue;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that a derived name that is a contextual keyword, or await outside an async member, is written without the verbatim prefix at the declaration and at the reference
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-007")]
    [Requirement("RH4105-003")]
    public async Task FixRenamesToContextualKeywordsWithoutVerbatimPrefix()
    {
        const string testCode = """
                                class C
                                {
                                    int M(int {|RH4105:Var|}) => Var;

                                    int N(int {|RH4105:Nameof|}) => Nameof;

                                    int O(int {|RH4105:Await|}) => Await;
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     int M(int var) => var;

                                     int N(int nameof) => nameof;

                                     int O(int await) => await;
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that a parameter without letters and a parameter whose derived name would start with a digit are still reported, but no fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-008")]
    public async Task NoFixIsOfferedWhenNoUsableCamelCaseNameCanBeDerived()
    {
        const string testCode = """
                                class C
                                {
                                    void M(int {|RH4105:_1st|}, int {|RH4105:__|})
                                    {
                                    }
                                }
                                """;

        await Verify(testCode, testCode);
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    [TestMethod]
    [Requirement("SHARED-CASING-010")]
    public override async Task FixAllCorrectsEveryDiagnostic()
    {
        await base.FixAllCorrectsEveryDiagnostic();
    }

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class Handler
                                    {
                                        public int Handle(int {|#0:InputValue|})
                                        {
                                            int Compute(int {|#1:Input_Value|})
                                            {
                                                return Input_Value + 1;
                                            }

                                            return Compute(InputValue);
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     public class Handler
                                     {
                                         public int Handle(int inputValue)
                                         {
                                             int Compute(int inputValue)
                                             {
                                                 return inputValue + 1;
                                             }

                                             return Compute(inputValue);
                                         }
                                     }
                                 }
                                 """;

        // Both parameters convert to the same camelCase name in nested scopes. They belong to different
        // containers, so they are not a duplicate-target group: the batch renames both, and the local function
        // parameter legitimately shadows the enclosing method parameter afterwards
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH4105MethodParameterCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4105MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}