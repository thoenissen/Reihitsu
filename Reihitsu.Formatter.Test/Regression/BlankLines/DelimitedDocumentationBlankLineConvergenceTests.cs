using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.BlankLines;

/// <summary>
/// Regression tests for the blank line the formatter puts in front of a delimited documentation comment that the
/// author wrote behind code. Roslyn files such a comment as the leading trivia of the following construct's first
/// token, so the boundary is short of two line breaks rather than one: the first ends the line the author filled
/// and the second is the blank line itself. Inserting only one of them spreads the layout over two passes, which
/// is what made <c>--check</c> report a file the formatter had just written (issue #637)
/// </summary>
[TestClass]
public class DelimitedDocumentationBlankLineConvergenceTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that a delimited documentation comment behind a local declaration is separated from it in one pass
    /// </summary>
    [TestMethod]
    public void SeparatesDelimitedDocumentationFromPrecedingStatementInOnePass()
    {
        const string input = """
                             internal class TestClass
                             {
                                 public void Method()
                                 {
                                     var x = 1; /** doc */
                                     var y = 2;
                                 }
                             }
                             """;
        const string expected = """
                                internal class TestClass
                                {
                                    public void Method()
                                    {
                                        var x = 1;

                                        /** doc */
                                        var y = 2;
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a delimited documentation comment behind an enum member is separated from it in one pass
    /// </summary>
    [TestMethod]
    public void SeparatesDelimitedDocumentationFromPrecedingEnumMemberInOnePass()
    {
        const string input = """
                             internal enum TestEnum
                             {
                                 First, /** doc */
                                 Second
                             }
                             """;
        const string expected = """
                                internal enum TestEnum
                                {
                                    First,

                                    /** doc */
                                    Second
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a delimited documentation comment behind a type declaration's closing brace is separated from
    /// it in one pass
    /// </summary>
    [TestMethod]
    public void SeparatesDelimitedDocumentationFromPrecedingTypeDeclarationInOnePass()
    {
        const string input = """
                             internal class First
                             {
                                 private int _x;
                             } /** doc */
                             internal class Second
                             {
                                 private int _y;
                             }
                             """;
        const string expected = """
                                internal class First
                                {
                                    private int _x;
                                }

                                /** doc */
                                internal class Second
                                {
                                    private int _y;
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a delimited documentation comment behind a using directive is separated from it in one pass
    /// </summary>
    [TestMethod]
    public void SeparatesDelimitedDocumentationFromPrecedingUsingDirectiveInOnePass()
    {
        const string input = """
                             using System; /** doc */
                             using System.Text;

                             internal class TestClass
                             {
                                 private int _x;
                             }
                             """;
        const string expected = """
                                using System;

                                /** doc */
                                using System.Text;

                                internal class TestClass
                                {
                                    private int _x;
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a delimited documentation comment behind a parameter is separated from it in one pass
    /// </summary>
    [TestMethod]
    public void SeparatesDelimitedDocumentationFromPrecedingParameterInOnePass()
    {
        const string input = """
                             internal class TestClass
                             {
                                 public void Method(int first, /** doc */
                                                    int second)
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                internal class TestClass
                                {
                                    public void Method(int first,

                                                       /** doc */
                                                       int second)
                                    {
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a delimited documentation comment behind an argument is separated from it in one pass
    /// </summary>
    [TestMethod]
    public void SeparatesDelimitedDocumentationFromPrecedingArgumentInOnePass()
    {
        const string input = """
                             internal class TestClass
                             {
                                 public void Method()
                                 {
                                     Other(1, /** doc */
                                           2);
                                 }

                                 public void Other(int first, int second)
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                internal class TestClass
                                {
                                    public void Method()
                                    {
                                        Other(1,

                                              /** doc */
                                              2);
                                    }

                                    public void Other(int first, int second)
                                    {
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a delimited documentation comment behind a collection element is separated from it in one pass
    /// </summary>
    [TestMethod]
    public void SeparatesDelimitedDocumentationFromPrecedingCollectionElementInOnePass()
    {
        const string input = """
                             internal class TestClass
                             {
                                 private int[] _values =
                                 [
                                     1, /** doc */
                                     2
                                 ];
                             }
                             """;
        const string expected = """
                                internal class TestClass
                                {
                                    private int[] _values = [
                                                                1,

                                                                /** doc */
                                                                2
                                                            ];
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a delimited documentation comment behind a statement inside a switch section is separated from
    /// it in one pass
    /// </summary>
    [TestMethod]
    public void SeparatesDelimitedDocumentationFromPrecedingSwitchSectionStatementInOnePass()
    {
        const string input = """
                             internal class TestClass
                             {
                                 public void Method(int value)
                                 {
                                     switch (value)
                                     {
                                         case 1:
                                             Other(); /** doc */
                                             break;
                                     }
                                 }

                                 public void Other()
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                internal class TestClass
                                {
                                    public void Method(int value)
                                    {
                                        switch (value)
                                        {
                                            case 1:
                                                Other();

                                                /** doc */
                                                break;
                                        }
                                    }

                                    public void Other()
                                    {
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the separation stays inside the owning conditional-compilation branch and completes in one pass
    /// </summary>
    [TestMethod]
    public void SeparatesDelimitedDocumentationInsideActiveDirectiveBranchInOnePass()
    {
        const string input = """
                             internal class TestClass
                             {
                             #if DEBUG
                                 private int _x; /** doc */
                                 private int _y;
                             #endif
                             }
                             """;
        const string expected = """
                                internal class TestClass
                                {
                                #if DEBUG
                                    private int _x;

                                    /** doc */
                                    private int _y;
                                #endif
                                }
                                """;
        var parseOptions = new CSharpParseOptions(preprocessorSymbols: ["DEBUG"]);

        AssertRuleResult(input, expected, parseOptions);
    }

    /// <summary>
    /// Verifies that the separation happens at the nested type's own indentation in one pass
    /// </summary>
    [TestMethod]
    public void SeparatesDelimitedDocumentationInsideNestedTypeInOnePass()
    {
        const string input = """
                             internal class TestClass
                             {
                                 internal class Inner
                                 {
                                     private int _x; /** doc */
                                     private int _y;
                                 }
                             }
                             """;
        const string expected = """
                                internal class TestClass
                                {
                                    internal class Inner
                                    {
                                        private int _x;

                                        /** doc */
                                        private int _y;
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a following region directive does not change the separation and that it still completes in one pass
    /// </summary>
    [TestMethod]
    public void SeparatesDelimitedDocumentationFollowedByRegionInOnePass()
    {
        const string input = """
                             internal class TestClass
                             {
                                 private int _x; /** doc */

                                 #region Members

                                 private int _y;

                                 #endregion // Members
                             }
                             """;
        const string expected = """
                                internal class TestClass
                                {
                                    private int _x;

                                    /** doc */

                                    #region Members

                                    private int _y;

                                    #endregion // Members
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    #endregion // Methods
}