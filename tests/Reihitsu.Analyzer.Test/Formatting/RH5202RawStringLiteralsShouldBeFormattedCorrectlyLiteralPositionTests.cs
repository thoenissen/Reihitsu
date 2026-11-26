using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5202RawStringLiteralsShouldBeFormattedCorrectlyCodeFixProvider"/> on a reported literal that
/// stands where a node around it has exactly the literal's span — a positional argument, a bracketed argument, a tuple element,
/// an attribute argument, a collection-expression element, or a constant pattern: the fix is offered for it as for any other
/// reported literal. In the fixtures, <c>→</c> stands for one tab
/// </summary>
[TestClass]
public class RH5202RawStringLiteralsShouldBeFormattedCorrectlyLiteralPositionTests : BatchCodeFixTestsBase<RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer, RH5202RawStringLiteralsShouldBeFormattedCorrectlyCodeFixProvider>
{
    #region Methods

    /// <summary>
    /// Verifies the diagnostic marked in the source and the result of the code fix, under LF and CRLF line endings
    /// </summary>
    /// <param name="source">The source text with placeholders and markup for the expected diagnostic</param>
    /// <param name="fixedSource">The expected fixed source text with placeholders</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private static async Task VerifyFixUnderBothLineEndings(string source, string fixedSource)
    {
        await VerifyCodeFixWithEndOfLine(ExpandWhitespaceMarkers(source), ExpandWhitespaceMarkers(fixedSource), "\n");
        await VerifyCodeFixWithEndOfLine(ExpandWhitespaceMarkers(source), ExpandWhitespaceMarkers(fixedSource), "\r\n");
    }

    #endregion // Methods

    #region Tests

    /// <summary>
    /// Verifies that a misaligned literal passed as a positional argument is fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-003")]
    public async Task VerifyMisalignedLiteralPassedAsPositionalArgumentIsFixed()
    {
        const string testData = """"
                                class C
                                {
                                    void M()
                                    {
                                        N({|RH5202:"""
                                            x
                                            """|});
                                    }

                                    void N(string s)
                                    {
                                    }
                                }
                                """";
        const string resultData = """"
                                  class C
                                  {
                                      void M()
                                      {
                                          N("""
                                            x
                                            """);
                                      }

                                      void N(string s)
                                      {
                                      }
                                  }
                                  """";

        await VerifyFixUnderBothLineEndings(testData, resultData);
    }

    /// <summary>
    /// Verifies that a misaligned interpolated literal passed as a positional argument is fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-003")]
    public async Task VerifyMisalignedInterpolatedLiteralPassedAsPositionalArgumentIsFixed()
    {
        const string testData = """"
                                class C
                                {
                                    void M(int value)
                                    {
                                        N({|RH5202:$"""
                                            x{value}
                                            """|});
                                    }

                                    void N(string s)
                                    {
                                    }
                                }
                                """";
        const string resultData = """"
                                  class C
                                  {
                                      void M(int value)
                                      {
                                          N($"""
                                             x{value}
                                             """);
                                      }

                                      void N(string s)
                                      {
                                      }
                                  }
                                  """";

        await VerifyFixUnderBothLineEndings(testData, resultData);
    }

    /// <summary>
    /// Verifies that a misaligned literal written as a positional attribute argument is fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-003")]
    public async Task VerifyMisalignedLiteralWrittenAsAttributeArgumentIsFixed()
    {
        const string testData = """"
                                using System;

                                class C
                                {
                                    [Obsolete({|RH5202:"""
                                        x
                                        """|})]
                                    void M()
                                    {
                                    }
                                }
                                """";
        const string resultData = """"
                                  using System;

                                  class C
                                  {
                                      [Obsolete("""
                                                x
                                                """)]
                                      void M()
                                      {
                                      }
                                  }
                                  """";

        await VerifyFixUnderBothLineEndings(testData, resultData);
    }

    /// <summary>
    /// Verifies that a misaligned literal written as an element of a tuple is fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-003")]
    public async Task VerifyMisalignedLiteralWrittenAsTupleElementIsFixed()
    {
        const string testData = """"
                                class C
                                {
                                    void M()
                                    {
                                        var t = ({|RH5202:"""
                                            x
                                            """|}, 1);
                                    }
                                }
                                """";
        const string resultData = """"
                                  class C
                                  {
                                      void M()
                                      {
                                          var t = ("""
                                                   x
                                                   """, 1);
                                      }
                                  }
                                  """";

        await VerifyFixUnderBothLineEndings(testData, resultData);
    }

    /// <summary>
    /// Verifies that a misaligned literal written as an element of a collection expression is fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-003")]
    public async Task VerifyMisalignedLiteralWrittenAsCollectionExpressionElementIsFixed()
    {
        const string testData = """"
                                class C
                                {
                                    void M()
                                    {
                                        string[] a = [{|RH5202:"""
                                            x
                                            """|}];
                                    }
                                }
                                """";
        const string resultData = """"
                                  class C
                                  {
                                      void M()
                                      {
                                          string[] a = ["""
                                                        x
                                                        """];
                                      }
                                  }
                                  """";

        await VerifyFixUnderBothLineEndings(testData, resultData);
    }

    /// <summary>
    /// Verifies that a misaligned literal written as a constant pattern is fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-003")]
    public async Task VerifyMisalignedLiteralWrittenAsConstantPatternIsFixed()
    {
        const string testData = """"
                                class C
                                {
                                    void M(object o)
                                    {
                                        var b = o is {|RH5202:"""
                                            x
                                            """|};
                                    }
                                }
                                """";
        const string resultData = """"
                                  class C
                                  {
                                      void M(object o)
                                      {
                                          var b = o is """
                                                       x
                                                       """;
                                      }
                                  }
                                  """";

        await VerifyFixUnderBothLineEndings(testData, resultData);
    }

    /// <summary>
    /// Verifies that a literal aligned in characters behind tabs and passed as a positional argument is fixed: the tabs in front
    /// of <c>x</c> and of the closing quotes become spaces
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-009")]
    public async Task VerifyLiteralAlignedBehindTabsPassedAsPositionalArgumentIsFixed()
    {
        const string testData = """"
                                class C
                                {
                                    void M()
                                    {
                                        N({|RH5202:"""
                                        →→x
                                        →→"""|});
                                    }

                                    void N(string s)
                                    {
                                    }
                                }
                                """";
        const string resultData = """"
                                  class C
                                  {
                                      void M()
                                      {
                                          N("""
                                            x
                                            """);
                                      }

                                      void N(string s)
                                      {
                                      }
                                  }
                                  """";

        await VerifyFixUnderBothLineEndings(testData, resultData);
    }

    /// <summary>
    /// Verifies that an interpolated literal aligned in characters behind tabs and passed as a positional argument is fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-009")]
    public async Task VerifyInterpolatedLiteralAlignedBehindTabsPassedAsPositionalArgumentIsFixed()
    {
        const string testData = """"
                                class C
                                {
                                    void M(int value)
                                    {
                                        N({|RH5202:$"""
                                        →→→x{value}
                                        →→→"""|});
                                    }

                                    void N(string s)
                                    {
                                    }
                                }
                                """";
        const string resultData = """"
                                  class C
                                  {
                                      void M(int value)
                                      {
                                          N($"""
                                             x{value}
                                             """);
                                      }

                                      void N(string s)
                                      {
                                      }
                                  }
                                  """";

        await VerifyFixUnderBothLineEndings(testData, resultData);
    }

    /// <summary>
    /// Verifies that a literal aligned in characters behind tabs and passed as a bracketed argument of an element access is fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-009")]
    public async Task VerifyLiteralAlignedBehindTabsPassedAsBracketedArgumentIsFixed()
    {
        const string testData = """"
                                using System.Collections.Generic;

                                class C
                                {
                                    void M(Dictionary<string, int> d)
                                    {
                                        var v = d[{|RH5202:"""
                                        →→→→→→→→→→x
                                        →→→→→→→→→→"""|}];
                                    }
                                }
                                """";
        const string resultData = """"
                                  using System.Collections.Generic;

                                  class C
                                  {
                                      void M(Dictionary<string, int> d)
                                      {
                                          var v = d["""
                                                    x
                                                    """];
                                      }
                                  }
                                  """";

        await VerifyFixUnderBothLineEndings(testData, resultData);
    }

    /// <summary>
    /// Verifies that a literal aligned in characters behind tabs and written as a positional attribute argument is fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-009")]
    public async Task VerifyLiteralAlignedBehindTabsWrittenAsAttributeArgumentIsFixed()
    {
        const string testData = """"
                                using System;

                                class C
                                {
                                    [Obsolete({|RH5202:"""
                                    →→→→→→→→→→x
                                    →→→→→→→→→→"""|})]
                                    void M()
                                    {
                                    }
                                }
                                """";
        const string resultData = """"
                                  using System;

                                  class C
                                  {
                                      [Obsolete("""
                                                x
                                                """)]
                                      void M()
                                      {
                                      }
                                  }
                                  """";

        await VerifyFixUnderBothLineEndings(testData, resultData);
    }

    /// <summary>
    /// Verifies that a literal aligned in characters behind tabs and written as an element of a tuple is fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-009")]
    public async Task VerifyLiteralAlignedBehindTabsWrittenAsTupleElementIsFixed()
    {
        const string testData = """"
                                class C
                                {
                                    void M()
                                    {
                                        var t = ({|RH5202:"""
                                        →→→→→→→→→x
                                        →→→→→→→→→"""|}, 1);
                                    }
                                }
                                """";
        const string resultData = """"
                                  class C
                                  {
                                      void M()
                                      {
                                          var t = ("""
                                                   x
                                                   """, 1);
                                      }
                                  }
                                  """";

        await VerifyFixUnderBothLineEndings(testData, resultData);
    }

    /// <summary>
    /// Verifies that a literal aligned in characters behind tabs and written as an element of a collection expression is fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-009")]
    public async Task VerifyLiteralAlignedBehindTabsWrittenAsCollectionExpressionElementIsFixed()
    {
        const string testData = """"
                                class C
                                {
                                    void M()
                                    {
                                        string[] a = [{|RH5202:"""
                                        →→→→→→→→→→→→→→x
                                        →→→→→→→→→→→→→→"""|}];
                                    }
                                }
                                """";
        const string resultData = """"
                                  class C
                                  {
                                      void M()
                                      {
                                          string[] a = ["""
                                                        x
                                                        """];
                                      }
                                  }
                                  """";

        await VerifyFixUnderBothLineEndings(testData, resultData);
    }

    /// <summary>
    /// Verifies that a literal aligned in characters behind tabs and written as a constant pattern is fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-009")]
    public async Task VerifyLiteralAlignedBehindTabsWrittenAsConstantPatternIsFixed()
    {
        const string testData = """"
                                class C
                                {
                                    void M(object o)
                                    {
                                        var b = o is {|RH5202:"""
                                        →→→→→→→→→→→→→x
                                        →→→→→→→→→→→→→"""|};
                                    }
                                }
                                """";
        const string resultData = """"
                                  class C
                                  {
                                      void M(object o)
                                      {
                                          var b = o is """
                                                       x
                                                       """;
                                      }
                                  }
                                  """";

        await VerifyFixUnderBothLineEndings(testData, resultData);
    }

    /// <summary>
    /// Verifies that a literal aligned in characters behind tabs and passed as a named argument, where the name widens the
    /// argument beyond the literal's span, is fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5202-009")]
    public async Task VerifyLiteralAlignedBehindTabsPassedAsNamedArgumentIsFixed()
    {
        const string testData = """"
                                class C
                                {
                                    void M()
                                    {
                                        N(s: {|RH5202:"""
                                        →→→→→x
                                        →→→→→"""|});
                                    }

                                    void N(string s)
                                    {
                                    }
                                }
                                """";
        const string resultData = """"
                                  class C
                                  {
                                      void M()
                                      {
                                          N(s: """
                                               x
                                               """);
                                      }

                                      void N(string s)
                                      {
                                      }
                                  }
                                  """";

        await VerifyFixUnderBothLineEndings(testData, resultData);
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """"
                                class C
                                {
                                    void M()
                                    {
                                        N({|#0:"""
                                        →→x
                                        →→"""|}, {|#1:"""
                                            y
                                            """|});
                                    }

                                    void N(string s, string t)
                                    {
                                    }
                                }
                                """";
        const string resultData = """"
                                  class C
                                  {
                                      void M()
                                      {
                                          N("""
                                            x
                                            """, """
                                                 y
                                                 """);
                                      }

                                      void N(string s, string t)
                                      {
                                      }
                                  }
                                  """";

        return new FixAllScenario(ExpandWhitespaceMarkers(testCode),
                                  ExpandWhitespaceMarkers(resultData),
                                  Diagnostics(RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}