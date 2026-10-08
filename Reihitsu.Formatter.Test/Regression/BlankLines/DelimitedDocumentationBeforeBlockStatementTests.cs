using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.BlankLines;

/// <summary>
/// Regression tests for a documentation comment written behind a statement when a block statement follows it
/// </summary>
[TestClass]
public class DelimitedDocumentationBeforeBlockStatementTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that a delimited documentation comment behind a statement is separated from the following block statement in one pass
    /// </summary>
    [TestMethod]
    public void SeparatesDelimitedDocumentationBeforeBlockStatementInOnePass()
    {
        const string input = """
                             public class TestClass
                             {
                                 public void M()
                                 {
                                     var a = 0; /** Doc */
                                     {
                                         a = 2;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                public class TestClass
                                {
                                    public void M()
                                    {
                                        var a = 0;

                                        /** Doc */
                                        {
                                            a = 2;
                                        }
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a single-line documentation comment behind a statement is separated from the following block statement in one pass
    /// </summary>
    [TestMethod]
    public void SeparatesSingleLineDocumentationBeforeBlockStatementInOnePass()
    {
        const string input = """
                             public class TestClass
                             {
                                 public void M()
                                 {
                                     var a = 0; /// Doc
                                     {
                                         a = 2;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                public class TestClass
                                {
                                    public void M()
                                    {
                                        var a = 0;

                                        /// Doc
                                        {
                                            a = 2;
                                        }
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a delimited documentation comment behind a statement is separated from a following block statement inside a braced switch section in one pass
    /// </summary>
    [TestMethod]
    public void SeparatesDelimitedDocumentationBeforeBlockStatementInSwitchSectionInOnePass()
    {
        const string input = """
                             public class TestClass
                             {
                                 public void M(int v)
                                 {
                                     switch (v)
                                     {
                                         case 1:
                                             {
                                                 M(2); /** Doc */
                                                 {
                                                     M(3);
                                                 }
                                             }

                                             break;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                public class TestClass
                                {
                                    public void M(int v)
                                    {
                                        switch (v)
                                        {
                                            case 1:
                                                {
                                                    M(2);

                                                    /** Doc */
                                                    {
                                                        M(3);
                                                    }
                                                }

                                                break;
                                        }
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an own-line delimited documentation comment directly below a statement and above a block statement
    /// gets the blank line above it in one pass
    /// </summary>
    [TestMethod]
    public void SeparatesOwnLineDelimitedDocumentationBeforeBlockStatementInOnePass()
    {
        const string input = """
                             public class TestClass
                             {
                                 public void M()
                                 {
                                     var a = 0;
                                     /** Doc */
                                     {
                                         a = 2;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                public class TestClass
                                {
                                    public void M()
                                    {
                                        var a = 0;

                                        /** Doc */
                                        {
                                            a = 2;
                                        }
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a delimited documentation comment behind a statement is separated from a following if statement in one pass
    /// </summary>
    [TestMethod]
    public void SeparatesDelimitedDocumentationBeforeIfStatementInOnePass()
    {
        const string input = """
                             public class TestClass
                             {
                                 public void M(bool b)
                                 {
                                     var a = 0; /** Doc */
                                     if (b)
                                     {
                                         a = 2;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                public class TestClass
                                {
                                    public void M(bool b)
                                    {
                                        var a = 0;

                                        /** Doc */
                                        if (b)
                                        {
                                            a = 2;
                                        }
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a delimited documentation comment behind an if header is separated from the contained block in one pass
    /// </summary>
    [TestMethod]
    public void SeparatesDelimitedDocumentationBeforeContainedBlockInOnePass()
    {
        const string input = """
                             public class TestClass
                             {
                                 public void M(bool b)
                                 {
                                     var a = 0;

                                     if (b) /** Doc */
                                     {
                                         a = 2;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                public class TestClass
                                {
                                    public void M(bool b)
                                    {
                                        var a = 0;

                                        if (b)

                                        /** Doc */
                                        {
                                            a = 2;
                                        }
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a delimited documentation comment behind a method signature is separated from the method body in one pass
    /// </summary>
    [TestMethod]
    public void SeparatesDelimitedDocumentationBeforeMethodBodyInOnePass()
    {
        const string input = """
                             public class TestClass
                             {
                                 public void M() /** Doc */
                                 {
                                     M();
                                 }
                             }
                             """;
        const string expected = """
                                public class TestClass
                                {
                                    public void M()

                                    /** Doc */
                                    {
                                        M();
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a multi-line delimited documentation comment behind a statement is separated from the following block statement in one pass
    /// </summary>
    [TestMethod]
    public void SeparatesMultiLineDelimitedDocumentationBeforeBlockStatementInOnePass()
    {
        const string input = """
                             public class TestClass
                             {
                                 public void M(bool b)
                                 {
                                     var a = 0; /** Doc
                                      * More
                                      */
                                     {
                                         a = 2;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                public class TestClass
                                {
                                    public void M(bool b)
                                    {
                                        var a = 0;

                                        /** Doc
                                         * More
                                         */
                                        {
                                            a = 2;
                                        }
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a delimited documentation comment glued to the opening brace of a block statement behind a statement gets the blank line above it in one pass while the brace stays on the comment's line
    /// </summary>
    [TestMethod]
    public void SeparatesDelimitedDocumentationGluedToBlockStatementInOnePass()
    {
        const string input = """
                             public class TestClass
                             {
                                 public void M(bool b)
                                 {
                                     var a = 0; /** Doc */ {
                                         a = 2;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                public class TestClass
                                {
                                    public void M(bool b)
                                    {
                                        var a = 0;

                                        /** Doc */ {
                                            a = 2;
                                        }
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a delimited documentation comment behind a statement keeps the blank line above it in one pass when a region directive sits between it and a block statement
    /// </summary>
    [TestMethod]
    public void SeparatesDelimitedDocumentationBeforeRegionAndBlockStatementInOnePass()
    {
        const string input = """
                             public class TestClass
                             {
                                 public void M(bool b)
                                 {
                                     var a = 0; /** Doc */
                                     #region R
                                     {
                                         a = 2;
                                     }
                                     #endregion // R
                                 }
                             }
                             """;
        const string expected = """
                                public class TestClass
                                {
                                    public void M(bool b)
                                    {
                                        var a = 0;

                                        /** Doc */

                                        #region R

                                        {
                                            a = 2;
                                        }

                                        #endregion // R
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a region directive directly before a block statement keeps its layout
    /// </summary>
    [TestMethod]
    public void KeepsRegionDirectiveBeforeBlockStatement()
    {
        const string input = """
                             public class TestClass
                             {
                                 public void M(bool b)
                                 {
                                     var a = 0;

                                     #region R

                                     {
                                         a = 2;
                                     }

                                     #endregion // R
                                 }
                             }
                             """;

        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that an own-line delimited documentation comment glued to the opening brace of a block statement keeps its layout
    /// </summary>
    [TestMethod]
    public void KeepsOwnLineDelimitedDocumentationGluedToBlockStatement()
    {
        const string input = """
                             public class TestClass
                             {
                                 public void M(bool b)
                                 {
                                     var a = 0;

                                     /** Doc */ {
                                         a = 2;
                                     }
                                 }
                             }
                             """;

        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that an own-line ordinary block comment above a block statement keeps its layout
    /// </summary>
    [TestMethod]
    public void KeepsOwnLineBlockCommentBeforeBlockStatement()
    {
        const string input = """
                             public class TestClass
                             {
                                 public void M(bool b)
                                 {
                                     var a = 0;

                                     /* c */
                                     {
                                         a = 2;
                                     }
                                 }
                             }
                             """;

        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a delimited documentation comment behind a property name is separated from the accessor list in one pass
    /// </summary>
    [TestMethod]
    public void SeparatesDelimitedDocumentationBeforeAccessorListInOnePass()
    {
        const string input = """
                             public class TestClass
                             {
                                 public int P /** Doc */
                                 {
                                     get => 1;
                                 }
                             }
                             """;
        const string expected = """
                                public class TestClass
                                {
                                    public int P

                                    /** Doc */
                                    {
                                        get => 1;
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a delimited documentation comment behind an object creation is separated from its initializer in one pass
    /// </summary>
    [TestMethod]
    public void SeparatesDelimitedDocumentationBeforeObjectInitializerInOnePass()
    {
        const string input = """
                             public class TestClass
                             {
                                 public int A { get; set; }

                                 public void M()
                                 {
                                     var c = new TestClass /** Doc */
                                             {
                                                 A = 1
                                             };
                                 }
                             }
                             """;
        const string expected = """
                                public class TestClass
                                {
                                    public int A { get; set; }

                                    public void M()
                                    {
                                        var c = new TestClass

                                                /** Doc */
                                                {
                                                    A = 1
                                                };
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    #endregion // Methods
}