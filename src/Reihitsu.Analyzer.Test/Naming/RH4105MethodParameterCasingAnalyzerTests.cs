using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Testing;
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

    /// <summary>
    /// Verifies that a parameter keeps its diagnostic but gets no fix when an excluded <c>#if</c> branch of its method references it, because the rename would leave that reference behind
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-007")]
    [Requirement("SHARED-FIX-003")]
    public async Task NoFixIsOfferedWhenAnExcludedBranchReferencesTheParameter()
    {
        const string testCode = """
                                public class C
                                {
                                    public int M(int {|RH4105:Count|})
                                    {
                                #if NEVER
                                        return Count + 1;
                                #else
                                        return Count;
                                #endif
                                    }
                                }
                                """;

        await Verify(testCode, testCode);
    }

    /// <summary>
    /// Verifies that a parameter of a partial method gets no fix when an excluded <c>#if</c> branch of the other part names that part's differently spelled parameter, because the rename renames that parameter with it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-007")]
    [Requirement("SHARED-FIX-003")]
    public async Task NoFixIsOfferedWhenAnExcludedBranchOfTheOtherPartOfAPartialMethodNamesItsDifferentlySpelledParameter()
    {
        const string testCode = """
                                public partial class C
                                {
                                    public void Use()
                                    {
                                        Run(1);
                                    }

                                    partial void Run(int {|RH4105:Arg|});
                                }
                                """;

        const string otherPartCode = """
                                     public partial class C
                                     {
                                         partial void Run(int other)
                                         {
                                     #if NEVER
                                             other++;
                                     #endif
                                         }
                                     }
                                     """;

        await Verify(testCode,
                     testCode,
                     test =>
                     {
                         test.TestState.Sources.Add(("/0/Test1.cs", otherPartCode));
                         test.FixedState.Sources.Add(("/0/Test1.cs", otherPartCode));
                     });
    }

    /// <summary>
    /// Verifies that neither parameter of a partial method gets a fix when an excluded <c>#if</c> branch on the defining part names its parameter and the implementing part spells the parameter differently, because renaming either parameter renames the other with it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-007")]
    [Requirement("SHARED-FIX-003")]
    public async Task NoFixIsOfferedWhenAnExcludedBranchOfTheDefiningPartOfAPartialMethodNamesItsDifferentlySpelledParameter()
    {
        const string testCode = """
                                public partial class C
                                {
                                    public void Use()
                                    {
                                        Run(1);
                                    }

                                #if NEVER
                                    [System.Obsolete(nameof(Arg))]
                                #endif
                                    partial void Run(int {|RH4105:Arg|});
                                }
                                """;

        const string otherPartCode = """
                                     public partial class C
                                     {
                                         partial void Run(int {|RH4105:Other|})
                                         {
                                         }
                                     }
                                     """;

        await Verify(testCode,
                     testCode,
                     test =>
                     {
                         test.TestState.Sources.Add(("/0/Test1.cs", otherPartCode));
                         test.FixedState.Sources.Add(("/0/Test1.cs", otherPartCode));
                     });
    }

    /// <summary>
    /// Verifies that a parameter of a partial indexer gets no fix when an excluded <c>#if</c> branch of the other part names that part's differently spelled parameter, because the rename renames that parameter with it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-007")]
    [Requirement("SHARED-FIX-003")]
    public async Task NoFixIsOfferedWhenAnExcludedBranchOfTheOtherPartOfAPartialIndexerNamesItsDifferentlySpelledParameter()
    {
        const string testCode = """
                                public partial class C
                                {
                                    public partial int this[int {|RH4105:Index|}] { get; }
                                }
                                """;

        const string otherPartCode = """
                                     public partial class C
                                     {
                                         public partial int this[int position]
                                         {
                                             get
                                             {
                                     #if NEVER
                                                 return position;
                                     #endif
                                                 return 0;
                                             }
                                         }
                                     }
                                     """;

        await Verify(testCode,
                     testCode,
                     test =>
                     {
                         test.TestState.Sources.Add(("/0/Test1.cs", otherPartCode));
                         test.FixedState.Sources.Add(("/0/Test1.cs", otherPartCode));
                     });
    }

    /// <summary>
    /// Verifies that a parameter of a partial method gets no fix when an excluded <c>#if</c> branch of the other part names the new name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-007")]
    [Requirement("SHARED-FIX-003")]
    public async Task NoFixIsOfferedWhenAnExcludedBranchOfTheOtherPartOfAPartialMethodNamesTheNewName()
    {
        const string testCode = """
                                public partial class C
                                {
                                    public void Use()
                                    {
                                        Run(1);
                                    }

                                    partial void Run(int {|RH4105:Arg|});
                                }
                                """;

        const string otherPartCode = """
                                     public partial class C
                                     {
                                         partial void Run(int other)
                                         {
                                     #if NEVER
                                             int arg = 0;
                                     #endif
                                         }
                                     }
                                     """;

        await Verify(testCode,
                     testCode,
                     test =>
                     {
                         test.TestState.Sources.Add(("/0/Test1.cs", otherPartCode));
                         test.FixedState.Sources.Add(("/0/Test1.cs", otherPartCode));
                     });
    }

    /// <summary>
    /// Verifies that the fix renames the parameter in both parts of a partial method, also where the other part spells it differently, and every reference to it, when no excluded <c>#if</c> branch names an old name or the new name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-007")]
    public async Task FixRenamesTheDifferentlySpelledParameterInBothPartsOfAPartialMethodWhenNoExcludedBranchNamesIt()
    {
        const string testCode = """
                                public partial class C
                                {
                                    public void Use()
                                    {
                                        Run(1);
                                    }

                                    partial void Run(int {|RH4105:Arg|});
                                }
                                """;

        const string otherPartCode = """
                                     public partial class C
                                     {
                                         partial void Run(int other)
                                         {
                                             other++;
                                     #if NEVER
                                             System.Console.WriteLine();
                                     #endif
                                         }
                                     }
                                     """;

        const string fixedCode = """
                                 public partial class C
                                 {
                                     public void Use()
                                     {
                                         Run(1);
                                     }

                                     partial void Run(int arg);
                                 }
                                 """;

        const string fixedOtherPartCode = """
                                          public partial class C
                                          {
                                              partial void Run(int arg)
                                              {
                                                  arg++;
                                          #if NEVER
                                                  System.Console.WriteLine();
                                          #endif
                                              }
                                          }
                                          """;

        await Verify(testCode,
                     fixedCode,
                     test =>
                     {
                         test.TestState.Sources.Add(("/0/Test1.cs", otherPartCode));
                         test.FixedState.Sources.Add(("/0/Test1.cs", fixedOtherPartCode));
                     });
    }

    /// <summary>
    /// Verifies that a parameter named by the string literal of a caller-argument expression is still reported but not
    /// fixed, because after the rename the attribute would name no parameter
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-012")]
    [Requirement("RH4105-003")]
    public async Task NoFixIsOfferedWhenACallerArgumentExpressionLiteralNamesTheOldName()
    {
        const string testCode = """
                                using System.Runtime.CompilerServices;

                                public static class Guard
                                {
                                    public static string Check(bool {|RH4105:Condition|}, [CallerArgumentExpression("Condition")] string expression = null)
                                    {
                                        return expression;
                                    }
                                }
                                """;

        await Verify(testCode, testCode);
    }

    /// <summary>
    /// Verifies that a parameter whose new name is the string literal of a caller-argument expression is still reported
    /// but not fixed, because after the rename the attribute would start to name the renamed parameter
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-012")]
    [Requirement("RH4105-003")]
    public async Task NoFixIsOfferedWhenACallerArgumentExpressionLiteralNamesTheNewName()
    {
        const string testCode = """
                                using System.Runtime.CompilerServices;

                                public static class Guard
                                {
                                    public static string Check(bool {|RH4105:Condition|}, [CallerArgumentExpression("condition")] string expression = null)
                                    {
                                        return expression;
                                    }
                                }
                                """;

        await Verify(testCode, testCode);
    }

    /// <summary>
    /// Verifies that a parameter named by a constant of a caller-argument expression is still reported but not fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-012")]
    [Requirement("RH4105-003")]
    public async Task NoFixIsOfferedWhenACallerArgumentExpressionConstantNamesTheOldName()
    {
        const string testCode = """
                                using System.Runtime.CompilerServices;

                                public static class Guard
                                {
                                    private const string ConditionName = "Condition";

                                    public static string Check(bool {|RH4105:Condition|}, [CallerArgumentExpression(ConditionName)] string expression = null)
                                    {
                                        return expression;
                                    }
                                }
                                """;

        await Verify(testCode, testCode);
    }

    /// <summary>
    /// Verifies that a constructor parameter named by a caller-argument expression is still reported but not fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-012")]
    [Requirement("RH4105-003")]
    public async Task NoFixIsOfferedWhenACallerArgumentExpressionNamesAConstructorParameter()
    {
        const string testCode = """
                                using System.Runtime.CompilerServices;

                                public class Guard
                                {
                                    public Guard(bool {|RH4105:Condition|}, [CallerArgumentExpression("Condition")] string expression = null)
                                    {
                                        Expression = expression;
                                    }

                                    public string Expression { get; }
                                }
                                """;

        await Verify(testCode, testCode);
    }

    /// <summary>
    /// Verifies that a primary constructor parameter of a class named by a caller-argument expression is still reported
    /// but not fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-012")]
    [Requirement("RH4105-003")]
    public async Task NoFixIsOfferedWhenACallerArgumentExpressionNamesAPrimaryConstructorParameter()
    {
        const string testCode = """
                                using System.Runtime.CompilerServices;

                                public class Guard(bool {|RH4105:Condition|}, [CallerArgumentExpression("Condition")] string expression = null)
                                {
                                    public bool Value => Condition;

                                    public string Expression => expression;
                                }
                                """;

        await Verify(testCode, testCode);
    }

    /// <summary>
    /// Verifies that a local function parameter named by a caller-argument expression is still reported but not fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-012")]
    [Requirement("RH4105-003")]
    public async Task NoFixIsOfferedWhenACallerArgumentExpressionNamesALocalFunctionParameter()
    {
        const string testCode = """
                                using System.Runtime.CompilerServices;

                                public static class Guard
                                {
                                    public static string Run()
                                    {
                                        return Check(1 > 0);

                                        static string Check(bool {|RH4105:Condition|}, [CallerArgumentExpression("Condition")] string expression = null)
                                        {
                                            return expression;
                                        }
                                    }
                                }
                                """;

        await Verify(testCode, testCode);
    }

    /// <summary>
    /// Verifies that a delegate parameter named by a caller-argument expression is still reported but not fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-012")]
    [Requirement("RH4105-003")]
    public async Task NoFixIsOfferedWhenACallerArgumentExpressionNamesADelegateParameter()
    {
        const string testCode = """
                                using System.Runtime.CompilerServices;

                                public delegate string Check(bool {|RH4105:Condition|}, [CallerArgumentExpression("Condition")] string expression = null);
                                """;

        await Verify(testCode, testCode);
    }

    /// <summary>
    /// Verifies that a parameter of a partial method is still reported but not fixed when the caller-argument expression
    /// that names it is declared on the other part of the method
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-012")]
    [Requirement("RH4105-003")]
    public async Task NoFixIsOfferedWhenACallerArgumentExpressionOnTheOtherPartOfAPartialMethodNamesTheParameter()
    {
        const string testCode = """
                                using System.Runtime.CompilerServices;

                                public partial class Guard
                                {
                                    public partial string Check(bool {|RH4105:Condition|}, [CallerArgumentExpression("Condition")] string expression = null);
                                }

                                public partial class Guard
                                {
                                    public partial string Check(bool {|RH4105:Condition|}, string expression)
                                    {
                                        return expression;
                                    }
                                }
                                """;

        await Verify(testCode, testCode);
    }

    /// <summary>
    /// Verifies that a parameter named by a <c>nameof</c> caller-argument expression is renamed together with the
    /// <c>nameof</c> argument
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-012")]
    [Requirement("RH4105-003")]
    public async Task FixIsOfferedWhenACallerArgumentExpressionNamesTheParameterWithNameof()
    {
        const string testCode = """
                                using System.Runtime.CompilerServices;

                                public static class Guard
                                {
                                    public static string Check(bool {|RH4105:Condition|}, [CallerArgumentExpression(nameof(Condition))] string expression = null)
                                    {
                                        return expression;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System.Runtime.CompilerServices;

                                 public static class Guard
                                 {
                                     public static string Check(bool condition, [CallerArgumentExpression(nameof(condition))] string expression = null)
                                     {
                                         return expression;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that a parameter is renamed when the caller-argument expression of its parameter list names another
    /// parameter, neither by the old nor by the new name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-012")]
    [Requirement("RH4105-003")]
    public async Task FixIsOfferedWhenACallerArgumentExpressionNamesAnotherParameter()
    {
        const string testCode = """
                                using System.Runtime.CompilerServices;

                                public static class Guard
                                {
                                    public static string Check(bool {|RH4105:Enabled|}, bool condition, [CallerArgumentExpression("condition")] string expression = null)
                                    {
                                        return Enabled ? expression : null;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System.Runtime.CompilerServices;

                                 public static class Guard
                                 {
                                     public static string Check(bool enabled, bool condition, [CallerArgumentExpression("condition")] string expression = null)
                                     {
                                         return enabled ? expression : null;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that Fix All leaves a parameter named by a caller-argument expression, and renames the other parameter of
    /// the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-012")]
    [Requirement("RH4105-003")]
    public async Task FixAllSkipsTheParameterNamedByACallerArgumentExpression()
    {
        const string testCode = """
                                using System.Runtime.CompilerServices;

                                public static class Guard
                                {
                                    public static string Check(bool {|RH4105:Condition|}, [CallerArgumentExpression("Condition")] string expression = null)
                                    {
                                        return expression;
                                    }

                                    public static int Twice(int {|RH4105:Value|})
                                    {
                                        return Value * 2;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System.Runtime.CompilerServices;

                                 public static class Guard
                                 {
                                     public static string Check(bool {|RH4105:Condition|}, [CallerArgumentExpression("Condition")] string expression = null)
                                     {
                                         return expression;
                                     }

                                     public static int Twice(int value)
                                     {
                                         return value * 2;
                                     }
                                 }
                                 """;

        await Verify(testCode,
                     fixedCode,
                     static test =>
                            {
                                // The diagnostics that keep no fix are marked in the expected source
                                test.FixedState.MarkupHandling = MarkupMode.Allow;
                                test.BatchFixedState.MarkupHandling = MarkupMode.Allow;
                                test.NumberOfFixAllIterations = 1;
                            });
    }

    /// <summary>
    /// Verifies that a parameter is still reported but not fixed when a caller-argument expression of its parameter list
    /// is a qualified <c>nameof</c> of another symbol with the parameter's old name, because the rename leaves that
    /// argument's value as it is and the attribute would name no parameter
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-012")]
    [Requirement("RH4105-003")]
    public async Task NoFixIsOfferedWhenACallerArgumentExpressionIsAQualifiedNameofOfAnotherSymbolWithTheOldName()
    {
        const string testCode = """
                                using System.Runtime.CompilerServices;

                                public static class Names
                                {
                                    public const int Condition = 0;
                                }

                                public static class Guard
                                {
                                    public static string Check(bool {|RH4105:Condition|}, [CallerArgumentExpression(nameof(Names.Condition))] string expression = null)
                                    {
                                        return expression;
                                    }
                                }
                                """;

        await Verify(testCode, testCode);
    }

    /// <summary>
    /// Verifies that a parameter is still reported but not fixed when a caller-argument expression of its parameter list
    /// is a <c>nameof</c> of another symbol with the parameter's new name, because after the rename the attribute would
    /// start to name the renamed parameter
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-012")]
    [Requirement("RH4105-003")]
    public async Task NoFixIsOfferedWhenACallerArgumentExpressionIsANameofOfAnotherSymbolWithTheNewName()
    {
        const string testCode = """
                                using System.Runtime.CompilerServices;

                                public static class Guard
                                {
                                    private const int condition = 0;

                                    public static string Check(bool {|RH4105:Condition|}, [CallerArgumentExpression(nameof(condition))] string expression = null)
                                    {
                                        return expression;
                                    }
                                }
                                """;

        await Verify(testCode, testCode);
    }

    /// <summary>
    /// Verifies that the receiver parameter of an extension block is still reported but not fixed when a member of the
    /// block names it by the string literal of a caller-argument expression
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-012")]
    [Requirement("RH4105-003")]
    public async Task NoFixIsOfferedWhenAMemberOfAnExtensionBlockNamesTheReceiverByACallerArgumentExpressionLiteral()
    {
        const string testCode = """
                                using System.Runtime.CompilerServices;

                                public static class GuardExtensions
                                {
                                    extension(bool {|RH4105:Condition|})
                                    {
                                        public string Check([CallerArgumentExpression("Condition")] string expression = null)
                                        {
                                            return Condition ? expression : null;
                                        }
                                    }
                                }
                                """;

        await Verify(testCode, testCode);
    }

    /// <summary>
    /// Verifies that the receiver parameter of an extension block is still reported but not fixed when a member of the
    /// block names it by a <c>nameof</c> caller-argument expression, which is not exempt in another parameter list
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-012")]
    [Requirement("RH4105-003")]
    public async Task NoFixIsOfferedWhenAMemberOfAnExtensionBlockNamesTheReceiverByANameofCallerArgumentExpression()
    {
        const string testCode = """
                                using System.Runtime.CompilerServices;

                                public static class GuardExtensions
                                {
                                    extension(bool {|RH4105:Condition|})
                                    {
                                        public string Check([CallerArgumentExpression(nameof(Condition))] string expression = null)
                                        {
                                            return Condition ? expression : null;
                                        }
                                    }
                                }
                                """;

        await Verify(testCode, testCode);
    }

    /// <summary>
    /// Verifies that the receiver parameter of an extension block is renamed when no member of the block carries a
    /// caller-argument expression
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-012")]
    [Requirement("RH4105-003")]
    public async Task FixRenamesTheReceiverOfAnExtensionBlockWithoutACallerArgumentExpression()
    {
        const string testCode = """
                                public static class GuardExtensions
                                {
                                    extension(bool {|RH4105:Condition|})
                                    {
                                        public string Check(string expression = null)
                                        {
                                            return Condition ? expression : null;
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public static class GuardExtensions
                                 {
                                     extension(bool condition)
                                     {
                                         public string Check(string expression = null)
                                         {
                                             return condition ? expression : null;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that a parameter is still reported but not fixed under C# 10 when a caller-argument expression of its
    /// parameter list is an unqualified <c>nameof</c> of the old name, because before C# 11 that <c>nameof</c> refers to a
    /// member of the type with the same name, which the rename leaves as it is
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-012")]
    [Requirement("RH4105-003")]
    public async Task NoFixIsOfferedForAnUnqualifiedNameofOfTheOldNameBeforeCSharp11()
    {
        const string testCode = """
                                using System.Runtime.CompilerServices;

                                public static class Guard
                                {
                                    public const int Condition = 0;

                                    public static string Check(bool {|RH4105:Condition|}, [CallerArgumentExpression(nameof(Condition))] string expression = null)
                                    {
                                        return expression;
                                    }
                                }
                                """;

        await Verify(testCode, testCode, static test => test.SolutionTransforms.Add(static (solution, projectId) => ApplyLanguageVersion(solution, projectId, LanguageVersion.CSharp10)));
    }

    /// <summary>
    /// Verifies that, from C# 11 on, an unqualified <c>nameof</c> of the old name refers to the parameter even where the
    /// type declares a member with the same name, so the argument is exempt and renamed together with the parameter
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-012")]
    [Requirement("RH4105-003")]
    public async Task FixRenamesAnUnqualifiedNameofOfTheOldNameFromCSharp11OnWhenTheTypeHasAMemberWithThatName()
    {
        const string testCode = """
                                using System.Runtime.CompilerServices;

                                public static class Guard
                                {
                                    public const int Condition = 0;

                                    public static string Check(bool {|RH4105:Condition|}, [CallerArgumentExpression(nameof(Condition))] string expression = null)
                                    {
                                        return expression;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System.Runtime.CompilerServices;

                                 public static class Guard
                                 {
                                     public const int Condition = 0;

                                     public static string Check(bool condition, [CallerArgumentExpression(nameof(condition))] string expression = null)
                                     {
                                         return expression;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, static test => test.SolutionTransforms.Add(static (solution, projectId) => ApplyLanguageVersion(solution, projectId, LanguageVersion.CSharp11)));
    }

    /// <summary>
    /// Verifies the example of a derived name that the rule still reports: <c>_ϒx</c> becomes <c>ϒx</c>, which does not
    /// start with a lowercase letter, so the parameter is still reported but not fixed
    /// </summary>
    /// <param name="oldName">The reported parameter name</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-008")]
    [Requirement("RH4105-003")]
    [DataRow("_ϒx")]
    [DataRow("_注文")]
    [DataRow("ϒ_x")]
    public async Task NoFixIsOfferedWhenTheRuleStillReportsTheDerivedName(string oldName)
    {
        var testCode = $$"""
                         public class C
                         {
                             public void M(int {|RH4105:{{oldName}}|})
                             {
                             }
                         }
                         """;

        await Verify(testCode, testCode);
    }

    /// <summary>
    /// Verifies that a parameter whose first word starts with <c>ŉ</c>, a lowercase letter without a one-character
    /// uppercase mapping, is renamed, because the rule accepts the derived name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-008")]
    [Requirement("RH4105-003")]
    public async Task FixIsOfferedWhenTheRuleAcceptsTheDerivedNameOfAnUncoveredOldName()
    {
        const string testCode = """
                                public class C
                                {
                                    public int M(int {|RH4105:_ŉx|})
                                    {
                                        return _ŉx;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class C
                                 {
                                     public int M(int ŉx)
                                     {
                                         return ŉx;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that Fix All leaves a parameter whose derived name the rule still reports, and renames the other
    /// parameter of the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-008")]
    [Requirement("RH4105-003")]
    public async Task FixAllSkipsTheParameterWhoseDerivedNameTheRuleStillReports()
    {
        const string testCode = """
                                public class C
                                {
                                    public void M(int {|RH4105:_ϒx|}, int {|RH4105:ItemCount|})
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class C
                                 {
                                     public void M(int {|RH4105:_ϒx|}, int itemCount)
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode,
                     fixedCode,
                     static test =>
                            {
                                // The diagnostics that keep no fix are marked in the expected source
                                test.FixedState.MarkupHandling = MarkupMode.Allow;
                                test.BatchFixedState.MarkupHandling = MarkupMode.Allow;
                                test.NumberOfFixAllIterations = 1;
                            });
    }

    #endregion // Tests

    #region Methods

    /// <summary>
    /// Sets the language version of the test project
    /// </summary>
    /// <param name="solution">Solution</param>
    /// <param name="projectId">Test project</param>
    /// <param name="languageVersion">Language version</param>
    /// <returns>The solution with the language version applied</returns>
    private static Solution ApplyLanguageVersion(Solution solution, ProjectId projectId, LanguageVersion languageVersion)
    {
        var project = solution.GetProject(projectId);

        if (project?.ParseOptions is CSharpParseOptions parseOptions)
        {
            solution = solution.WithProjectParseOptions(projectId, parseOptions.WithLanguageVersion(languageVersion));
        }

        return solution;
    }

    #endregion // Methods

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