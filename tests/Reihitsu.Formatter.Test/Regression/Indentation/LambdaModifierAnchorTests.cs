using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Indentation;

/// <summary>
/// Full-pipeline tests for the anchor of a block lambda that carries a <c>static</c> modifier or an explicit return type:
/// the block aligns under <c>async</c>, or else under the opening parenthesis of the parameter list or the single
/// parameter, and never under the modifier or the return type
/// </summary>
[TestClass]
public class LambdaModifierAnchorTests : FormatterTestsBase
{
    #region Tests

    /// <summary>
    /// Verifies the spec example: a lambda with an explicit return type, behind <c>static</c> or not, aligns its block under
    /// the parameter list's <c>(</c>, a <c>static</c> simple lambda under its parameter, a <c>static async</c> lambda under
    /// <c>async</c>, and a <c>static</c> lambda without parameters under its <c>(</c>
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-028")]
    public void SpecExampleModifierAndReturnTypeAreNeverTheAnchor()
    {
        // Arrange
        const string input = """
                             using System;
                             using System.Threading.Tasks;

                             class C
                             {
                                 void M()
                                 {
                                     Func<int, int> a = int (x) =>
                                     {
                                         return x;
                                     };

                                     Func<int, int> b = static int (x) =>
                                     {
                                         return x;
                                     };

                                     Func<int, int> c = static x =>
                                     {
                                         return x;
                                     };

                                     Func<int, Task<int>> d = static async x =>
                                     {
                                         return x;
                                     };

                                     Func<int> e = static () =>
                                     {
                                         return 1;
                                     };
                                 }
                             }
                             """;
        const string expected = """
                                using System;
                                using System.Threading.Tasks;

                                class C
                                {
                                    void M()
                                    {
                                        Func<int, int> a = int (x) =>
                                                               {
                                                                   return x;
                                                               };

                                        Func<int, int> b = static int (x) =>
                                                                      {
                                                                          return x;
                                                                      };

                                        Func<int, int> c = static x =>
                                                                  {
                                                                      return x;
                                                                  };

                                        Func<int, Task<int>> d = static async x =>
                                                                        {
                                                                            return x;
                                                                        };

                                        Func<int> e = static () =>
                                                             {
                                                                 return 1;
                                                             };
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the sibling positions the spec states in prose: <c>async static x</c> aligns under <c>async</c>, an async
    /// lambda with an explicit return type under <c>async</c>, and a <c>static</c> lambda with an explicit return type as
    /// an argument under the parameter list's <c>(</c>
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-028")]
    public void AsyncInFrontOfModifierOrReturnTypeAndArgumentPositionKeepTheAnchor()
    {
        // Arrange
        const string input = """
                             using System;
                             using System.Threading.Tasks;

                             class C
                             {
                                 void M()
                                 {
                                     Func<int, Task<int>> e = async static x =>
                                     {
                                         return x;
                                     };

                                     Func<int, Task<int>> h = async Task<int> (x) =>
                                     {
                                         return x;
                                     };

                                     N(static int (x) =>
                                     {
                                         return x;
                                     });
                                 }

                                 void N(Func<int, int> f)
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                using System;
                                using System.Threading.Tasks;

                                class C
                                {
                                    void M()
                                    {
                                        Func<int, Task<int>> e = async static x =>
                                                                 {
                                                                     return x;
                                                                 };

                                        Func<int, Task<int>> h = async Task<int> (x) =>
                                                                 {
                                                                     return x;
                                                                 };

                                        N(static int (x) =>
                                                     {
                                                         return x;
                                                     });
                                    }

                                    void N(Func<int, int> f)
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Tests
}