using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Naming;
using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Naming;

/// <summary>
/// Test methods for <see cref="RH4128LocalConstantCasingAnalyzer"/> and <see cref="RH4128LocalConstantCasingCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH4128LocalConstantCasingAnalyzerTests : BatchCodeFixTestsBase<RH4128LocalConstantCasingAnalyzer, RH4128LocalConstantCasingCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies every local constant whose name is not in camelCase is reported once per variable, in a constructor, an
    /// anonymous method, a method, a local function, and a lambda, while a camelCase constant and a local variable are not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4128-001")]
    public async Task LocalConstantsThatAreNotInCamelCaseAreReported()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    C()
                                    {
                                        const int {|RH4128:Initial|} = 0;
                                        Action a = delegate { const int {|RH4128:Delay|} = Initial; };
                                    }

                                    int M()
                                    {
                                        const int maxValue = 3, {|RH4128:MaxCount|} = 5;
                                        const string {|RH4128:default_name|} = "x";
                                        int Total = maxValue;

                                        int Local()
                                        {
                                            const int {|RH4128:Limit|} = 1;

                                            return Limit;
                                        }

                                        Func<int> f = () =>
                                        {
                                            const int {|RH4128:Step|} = 2;

                                            return Step;
                                        };

                                        return Total + MaxCount + default_name.Length + Local() + f();
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies local constants in an accessor, an operator, a finalizer, and a switch section are reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4128-001")]
    public async Task LocalConstantsInAccessorsOperatorsFinalizersAndSwitchSectionsAreReported()
    {
        const string testCode = """
                                class C
                                {
                                    ~C()
                                    {
                                        const int {|RH4128:Cleanup|} = 1;
                                    }

                                    int Value
                                    {
                                        get
                                        {
                                            const int {|RH4128:Fallback|} = 2;

                                            return Fallback;
                                        }
                                    }

                                    public static C operator +(C left, C right)
                                    {
                                        const int {|RH4128:Weight|} = 3;

                                        return Weight > 0 ? left : right;
                                    }

                                    int M(int value)
                                    {
                                        switch (value)
                                        {
                                            case 1:
                                                const int {|RH4128:Offset|} = 4;

                                                return Offset;
                                        }

                                        return 0;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies a local constant of the top-level statements is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4128-001")]
    public async Task LocalConstantOfTopLevelStatementsIsReported()
    {
        const string testCode = """
                                const int {|RH4128:MaxValue|} = 3;

                                System.Console.WriteLine(MaxValue);
                                """;

        await Verify(testCode, test => test.TestState.OutputKind = OutputKind.ConsoleApplication);
    }

    /// <summary>
    /// Verifies local variables without <c>const</c> are not reported, whatever their name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4128-001")]
    public async Task LocalVariablesWithoutConstAreNotReported()
    {
        const string testCode = """
                                class C
                                {
                                    int M()
                                    {
                                        int Total = 1, Max_Count = 2;
                                        var Name = "x";

                                        return Total + Max_Count + Name.Length;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies a local constant named <c>_</c> is not reported, while a name that only starts with <c>_</c> is
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4128-002")]
    public async Task LocalConstantNamedDiscardIsNotReported()
    {
        const string testCode = """
                                class C
                                {
                                    void M()
                                    {
                                        const int _ = 1;
                                        const int {|RH4128:_limit|} = 2;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies the fix renames local constants and every reference to them, also in another constant's value and in a
    /// case label
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4128-003")]
    public async Task FixRenamesLocalConstantsAndTheirReferences()
    {
        const string testCode = """
                                class C
                                {
                                    int M(int value)
                                    {
                                        const int {|RH4128:MaxValue|} = 3;
                                        const int {|RH4128:Max_Count|} = MaxValue + 1;

                                        switch (value)
                                        {
                                            case MaxValue:
                                                return Max_Count;
                                        }

                                        return MaxValue;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     int M(int value)
                                     {
                                         const int maxValue = 3;
                                         const int maxCount = maxValue + 1;

                                         switch (value)
                                         {
                                             case maxValue:
                                                 return maxCount;
                                         }

                                         return maxValue;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies the fix renames a reference to a local constant in an attribute argument of a local function
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4128-003")]
    public async Task FixRenamesAReferenceInAnAttributeArgumentOfALocalFunction()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        const string {|RH4128:Message|} = "x";

                                        [Obsolete(Message)]
                                        void Local()
                                        {
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 class C
                                 {
                                     void M()
                                     {
                                         const string message = "x";

                                         [Obsolete(message)]
                                         void Local()
                                         {
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that a local constant keeps its diagnostic but gets no fix when its new name would collide with a local
    /// variable of the same scope
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-009")]
    public async Task NoFixIsOfferedWhenTheNewNameWouldCollideWithALocalVariable()
    {
        const string testCode = """
                                class C
                                {
                                    int M()
                                    {
                                        const int {|RH4128:MaxValue|} = 1;
                                        int maxValue = 2;

                                        return MaxValue + maxValue;
                                    }
                                }
                                """;

        await Verify(testCode, testCode);
    }

    /// <summary>
    /// Verifies that a local constant keeps its diagnostic but gets no fix when its derived name is a reserved keyword
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-013")]
    public async Task NoFixIsOfferedWhenTheDerivedNameIsAReservedKeyword()
    {
        const string testCode = """
                                class C
                                {
                                    int M()
                                    {
                                        const int {|RH4128:Class|} = 1;

                                        return Class;
                                    }
                                }
                                """;

        await Verify(testCode, testCode);
    }

    /// <summary>
    /// Verifies the fix keeps the name of an anonymous object member whose whole expression is the renamed constant
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-015")]
    [Requirement("RH4128-003")]
    public async Task FixKeepsTheNameOfAnAnonymousObjectMemberNamedAfterTheConstant()
    {
        const string testCode = """
                                class C
                                {
                                    string M()
                                    {
                                        const int {|RH4128:MaxValue|} = 3;

                                        return new { MaxValue }.ToString();
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     string M()
                                     {
                                         const int maxValue = 3;

                                         return new { MaxValue = maxValue }.ToString();
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    [TestMethod]
    [Requirement("RH4128-003")]
    [Requirement("SHARED-CASING-010")]
    public override async Task FixAllCorrectsEveryDiagnostic()
    {
        await base.FixAllCorrectsEveryDiagnostic();
    }

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                class C
                                {
                                    int M()
                                    {
                                        const int {|#0:MaxValue|} = 3, {|#1:Max_Count|} = 4;

                                        return MaxValue + Max_Count;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     int M()
                                     {
                                         const int maxValue = 3, maxCount = 4;

                                         return maxValue + maxCount;
                                     }
                                 }
                                 """;

        // Verifies the constants of one declaration are renamed together by Fix All in one iteration
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH4128LocalConstantCasingAnalyzer.DiagnosticId, MessageFormat, 2),
                                  static config => config.NumberOfFixAllIterations = 1);
    }

    #endregion // BatchCodeFixTestsBase
}