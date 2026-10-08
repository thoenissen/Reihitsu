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

    /// <summary>
    /// Verifies that the opening brace of a type glued behind a delimited documentation comment behind the type name is moved onto its own line in one pass
    /// </summary>
    [TestMethod]
    public void MovesTypeOpeningBraceBehindGluedDocumentationOntoOwnLine()
    {
        const string input = """
                             public class TestClass /** Doc */ {
                                 private int _a;
                             }
                             """;
        const string expected = """
                                public class TestClass /** Doc */
                                {
                                    private int _a;
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the opening brace of a namespace glued behind a delimited documentation comment behind the namespace name is moved onto its own line in one pass
    /// </summary>
    [TestMethod]
    public void MovesNamespaceOpeningBraceBehindGluedDocumentationOntoOwnLine()
    {
        const string input = """
                             namespace N /** Doc */ {
                                 public class TestClass
                                 {
                                     private int _a;
                                 }
                             }
                             """;
        const string expected = """
                                namespace N /** Doc */
                                {
                                    public class TestClass
                                    {
                                        private int _a;
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the opening brace of an enum glued behind a delimited documentation comment behind the enum name is moved onto its own line in one pass
    /// </summary>
    [TestMethod]
    public void MovesEnumOpeningBraceBehindGluedDocumentationOntoOwnLine()
    {
        const string input = """
                             public enum E /** Doc */ {
                                 A
                             }
                             """;
        const string expected = """
                                public enum E /** Doc */
                                {
                                    A
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the opening brace of a switch statement glued behind a delimited documentation comment behind its header is moved onto its own line in one pass
    /// </summary>
    [TestMethod]
    public void MovesSwitchOpeningBraceBehindGluedDocumentationOntoOwnLine()
    {
        const string input = """
                             public class TestClass
                             {
                                 public void M(int v)
                                 {
                                     switch (v) /** Doc */ {
                                         case 1:
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
                                        switch (v) /** Doc */
                                        {
                                            case 1:
                                                break;
                                        }
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the closing brace of a block glued behind a delimited documentation comment behind its last statement is moved onto its own line in one pass
    /// </summary>
    [TestMethod]
    public void MovesBlockClosingBraceBehindGluedDocumentationOntoOwnLine()
    {
        const string input = """
                             public class TestClass
                             {
                                 public void M()
                                 {
                                     M(); /** Doc */ }
                             }
                             """;
        const string expected = """
                                public class TestClass
                                {
                                    public void M()
                                    {
                                        M(); /** Doc */
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the closing brace of a type glued behind a delimited documentation comment behind its last member is moved onto its own line in one pass
    /// </summary>
    [TestMethod]
    public void MovesTypeClosingBraceBehindGluedDocumentationOntoOwnLine()
    {
        const string input = """
                             public class TestClass
                             {
                                 private int _a; /** Doc */ }
                             """;
        const string expected = """
                                public class TestClass
                                {
                                    private int _a; /** Doc */
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the opening brace of an anonymous object glued behind a delimited documentation comment behind its new keyword is moved onto its own line in one pass
    /// </summary>
    [TestMethod]
    public void MovesAnonymousObjectOpeningBraceBehindGluedDocumentationOntoOwnLine()
    {
        const string input = """
                             public class TestClass
                             {
                                 public object M()
                                 {
                                     return new /** Doc */ {
                                         A = 1
                                     };
                                 }
                             }
                             """;
        const string expected = """
                                public class TestClass
                                {
                                    public object M()
                                    {
                                        return new /** Doc */
                                               {
                                                   A = 1
                                               };
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the closing bracket of a multi-line collection expression glued behind a delimited documentation comment behind its last element is moved onto its own line in one pass
    /// </summary>
    [TestMethod]
    public void MovesCollectionClosingBracketBehindGluedDocumentationOntoOwnLine()
    {
        const string input = """
                             public class TestClass
                             {
                                 private int[] _a = [
                                     1,
                                     2 /** d */ ];
                             }
                             """;
        const string expected = """
                                public class TestClass
                                {
                                    private int[] _a = [
                                                           1,
                                                           2 /** d */
                                                       ];
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a delimited documentation comment and a block comment behind a statement and glued to a block statement get the blank line above them in one pass
    /// </summary>
    [TestMethod]
    public void SeparatesDelimitedDocumentationAndGluedCommentBeforeBlockStatementInOnePass()
    {
        const string input = """
                             public class TestClass
                             {
                                 public void M()
                                 {
                                     var a = 0; /** Doc */ /* c */ {
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

                                        /** Doc */ /* c */ {
                                            a = 2;
                                        }
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the opening brace of a type glued behind a delimited documentation comment and a block comment behind the type name is moved onto its own line in one pass
    /// </summary>
    [TestMethod]
    public void MovesTypeOpeningBraceBehindGluedDocumentationAndCommentOntoOwnLine()
    {
        const string input = """
                             public class TestClass /** Doc */ /* c */ {
                                 private int _a;
                             }
                             """;
        const string expected = """
                                public class TestClass /** Doc */ /* c */
                                {
                                    private int _a;
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a delimited documentation comment and a line comment behind a statement get the blank line above them in one pass while the block statement stays below them
    /// </summary>
    [TestMethod]
    public void SeparatesDelimitedDocumentationAndLineCommentBeforeBlockStatementInOnePass()
    {
        const string input = """
                             public class TestClass
                             {
                                 public void M()
                                 {
                                     var a = 0; /** Doc */ // c
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

                                        /** Doc */ // c
                                        {
                                            a = 2;
                                        }
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    #endregion // Methods
}