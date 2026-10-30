using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Indentation;

/// <summary>
/// Full-pipeline tests for lines around tokens without text: an omitted type argument, an omitted array size, and the
/// end of the file. Such a token never starts a line, so the line starts at the next token or trivia with text, which is
/// placed once at the column its own requirement gives it
/// </summary>
[TestClass]
public class ZeroWidthTokenLineStartTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies the specification example: behind the omitted type argument, the closing angle bracket starts its line
    /// and takes the block column
    /// </summary>
    [TestMethod]
    [Requirement("SHARED-BLANK-008")]
    [Requirement("FMT-INDENT-015")]
    public void ClosingAngleBracketBehindOmittedTypeArgumentTakesBlockColumn()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var type = typeof(System.Collections.Generic.Dictionary<,
                             >);
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var type = typeof(System.Collections.Generic.Dictionary<,
                                        >);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the issue's example: the closing angle bracket behind an omitted type argument of an unbound generic in
    /// column 0 takes the block column
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-015")]
    public void ClosingAngleBracketOfUnboundGenericInColumnZeroTakesBlockColumn()
    {
        // Arrange
        const string input = """
                             using System.Collections.Generic;

                             public class Sample
                             {
                                 public void Foo()
                                 {
                                     var type = typeof(Dictionary<,
                             >);
                                 }
                             }
                             """;
        const string expected = """
                                using System.Collections.Generic;

                                public class Sample
                                {
                                    public void Foo()
                                    {
                                        var type = typeof(Dictionary<,
                                        >);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the specification example: a comment behind the last closing brace of a file starts its line in front
    /// of the end of the file and takes column 0
    /// </summary>
    [TestMethod]
    [Requirement("SHARED-BLANK-008")]
    public void CommentBehindLastClosingBraceOfFileTakesColumnZero()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int _a;
                             }

                                 // end
                             """;
        const string expected = """
                                class C
                                {
                                    int _a;
                                }

                                // end
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an <c>#endregion</c> behind the last closing brace of a file starts its line in front of the end of
    /// the file and takes column 0
    /// </summary>
    [TestMethod]
    [Requirement("SHARED-BLANK-008")]
    public void EndRegionBehindLastClosingBraceOfFileTakesColumnZero()
    {
        // Arrange
        const string input = """
                             #region A
                             class C
                             {
                                 int _a;
                             }
                                 #endregion
                             """;
        const string expected = """
                                #region A

                                class C
                                {
                                    int _a;
                                }

                                #endregion // A
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an <c>#endregion</c> behind the last closing brace of a file that ends with a line break starts its
    /// line in front of the end of the file and takes column 0
    /// </summary>
    [TestMethod]
    [Requirement("SHARED-BLANK-008")]
    public void EndRegionBehindLastClosingBraceOfFileEndingWithLineBreakTakesColumnZero()
    {
        // Arrange
        const string input = "#region A\nclass C\n{\n    int _a;\n}\n    #endregion\n";
        const string expected = """
                                #region A

                                class C
                                {
                                    int _a;
                                }

                                #endregion // A
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the specification example: at the start of a file, the first token behind a documentation comment
    /// starts its line and takes column 0
    /// </summary>
    [TestMethod]
    [Requirement("SHARED-BLANK-008")]
    public void FirstTokenOfFileBehindDocumentationCommentTakesColumnZero()
    {
        // Arrange
        const string input = """
                             /// <summary>C</summary>
                                 public class C
                             {
                                 int _a;
                             }
                             """;
        const string expected = """
                                /// <summary>
                                /// C
                                /// </summary>
                                public class C
                                {
                                    int _a;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that, at the start of a file, the first token behind a directive starts its line and takes column 0
    /// </summary>
    [TestMethod]
    [Requirement("SHARED-BLANK-008")]
    public void FirstTokenOfFileBehindDirectiveTakesColumnZero()
    {
        // Arrange
        const string input = """
                             #nullable enable
                                 public class C
                             {
                                 int _a;
                             }
                             """;
        const string expected = """
                                #nullable enable
                                public class C
                                {
                                    int _a;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a separator that starts its line behind an omitted type argument takes the block column
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-015")]
    public void SeparatorBehindOmittedTypeArgumentTakesBlockColumn()
    {
        // Arrange
        const string input = """
                             using System;
                             using System.Collections.Generic;

                             class C
                             {
                                 void M()
                                 {
                                     var type = typeof(Dictionary<
                             ,>);
                                 }
                             }
                             """;
        const string expected = """
                                using System;
                                using System.Collections.Generic;

                                class C
                                {
                                    void M()
                                    {
                                        var type = typeof(Dictionary<
                                        ,>);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the closing angle bracket behind the only, omitted type argument takes the block column
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-015")]
    public void ClosingAngleBracketOfSingleOmittedTypeArgumentTakesBlockColumn()
    {
        // Arrange
        const string input = """
                             using System;
                             using System.Collections.Generic;

                             class C
                             {
                                 void M()
                                 {
                                     var type = typeof(Action<
                             >);
                                 }
                             }
                             """;
        const string expected = """
                                using System;
                                using System.Collections.Generic;

                                class C
                                {
                                    void M()
                                    {
                                        var type = typeof(Action<
                                        >);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the closing angle bracket behind three omitted type arguments takes the block column
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-015")]
    public void ClosingAngleBracketBehindThreeOmittedTypeArgumentsTakesBlockColumn()
    {
        // Arrange
        const string input = """
                             using System;
                             using System.Collections.Generic;

                             class C
                             {
                                 void M()
                                 {
                                     var type = typeof(Func<,,
                             >);
                                 }
                             }
                             """;
        const string expected = """
                                using System;
                                using System.Collections.Generic;

                                class C
                                {
                                    void M()
                                    {
                                        var type = typeof(Func<,,
                                        >);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a separator and the closing angle bracket behind it, which start their line between omitted type
    /// arguments, take the block column
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-015")]
    public void SeparatorAndClosingAngleBracketBehindOmittedTypeArgumentsTakeBlockColumn()
    {
        // Arrange
        const string input = """
                             using System;
                             using System.Collections.Generic;

                             class C
                             {
                                 void M()
                                 {
                                     var type = typeof(Func<,
                             ,>);
                                 }
                             }
                             """;
        const string expected = """
                                using System;
                                using System.Collections.Generic;

                                class C
                                {
                                    void M()
                                    {
                                        var type = typeof(Func<,
                                        ,>);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a closing angle bracket that the author indented deeper than the block behind an omitted type
    /// argument takes the block column
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-015")]
    public void OverIndentedClosingAngleBracketBehindOmittedTypeArgumentTakesBlockColumn()
    {
        // Arrange
        const string input = """
                             using System;
                             using System.Collections.Generic;

                             class C
                             {
                                 void M()
                                 {
                                     var type = typeof(Dictionary<,
                                                 >);
                                 }
                             }
                             """;
        const string expected = """
                                using System;
                                using System.Collections.Generic;

                                class C
                                {
                                    void M()
                                    {
                                        var type = typeof(Dictionary<,
                                        >);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the closing angle bracket behind an omitted type argument whose line ends with a comment takes the
    /// block column
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-015")]
    public void ClosingAngleBracketBehindOmittedTypeArgumentAndTrailingCommentTakesBlockColumn()
    {
        // Arrange
        const string input = """
                             using System;
                             using System.Collections.Generic;

                             class C
                             {
                                 void M()
                                 {
                                     var type = typeof(Dictionary<, // c
                             >);
                                 }
                             }
                             """;
        const string expected = """
                                using System;
                                using System.Collections.Generic;

                                class C
                                {
                                    void M()
                                    {
                                        var type = typeof(Dictionary<, // c
                                        >);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an own-line comment and the closing angle bracket behind an omitted type argument take the block
    /// column, and that no line is left holding only whitespace
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-015")]
    public void OwnLineCommentAndClosingAngleBracketBehindOmittedTypeArgumentTakeBlockColumn()
    {
        // Arrange
        const string input = """
                             using System;
                             using System.Collections.Generic;

                             class C
                             {
                                 void M()
                                 {
                                     var type = typeof(Dictionary<,
                             // c
                             >);
                                 }
                             }
                             """;
        const string expected = """
                                using System;
                                using System.Collections.Generic;

                                class C
                                {
                                    void M()
                                    {
                                        var type = typeof(Dictionary<,

                                        // c
                                        >);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an own-line comment and a separator behind an omitted type argument take the block column, and that
    /// no line is left holding only whitespace
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-015")]
    public void OwnLineCommentAndSeparatorBehindOmittedTypeArgumentTakeBlockColumn()
    {
        // Arrange
        const string input = """
                             using System;
                             using System.Collections.Generic;

                             class C
                             {
                                 void M()
                                 {
                                     var type = typeof(Dictionary<
                             // c
                             ,>);
                                 }
                             }
                             """;
        const string expected = """
                                using System;
                                using System.Collections.Generic;

                                class C
                                {
                                    void M()
                                    {
                                        var type = typeof(Dictionary<

                                        // c
                                        ,>);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the closing bracket of an array rank specifier behind an omitted array size takes the block column,
    /// as it does behind an array size with text
    /// </summary>
    [TestMethod]
    [Requirement("SHARED-BLANK-008")]
    public void ClosingBracketBehindOmittedArraySizeTakesBlockColumn()
    {
        // Arrange
        const string input = """
                             using System;
                             using System.Collections.Generic;

                             class C
                             {
                                 void M()
                                 {
                                     int[,
                             ] a = null;
                                 }
                             }
                             """;
        const string expected = """
                                using System;
                                using System.Collections.Generic;

                                class C
                                {
                                    void M()
                                    {
                                        int[,
                                        ] a = null;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a separator of an array rank specifier behind an omitted array size takes the block column
    /// </summary>
    [TestMethod]
    [Requirement("SHARED-BLANK-008")]
    public void SeparatorBehindOmittedArraySizeTakesBlockColumn()
    {
        // Arrange
        const string input = """
                             using System;
                             using System.Collections.Generic;

                             class C
                             {
                                 void M()
                                 {
                                     int[
                             ,] a = null;
                                 }
                             }
                             """;
        const string expected = """
                                using System;
                                using System.Collections.Generic;

                                class C
                                {
                                    void M()
                                    {
                                        int[
                                        ,] a = null;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the closing bracket of an array creation's rank specifier behind an omitted array size takes the
    /// block column
    /// </summary>
    [TestMethod]
    [Requirement("SHARED-BLANK-008")]
    public void ClosingBracketOfArrayCreationBehindOmittedArraySizeTakesBlockColumn()
    {
        // Arrange
        const string input = """
                             using System;
                             using System.Collections.Generic;

                             class C
                             {
                                 void M()
                                 {
                                     var a = new int[,
                             ] { { 1 } };
                                 }
                             }
                             """;
        const string expected = """
                                using System;
                                using System.Collections.Generic;

                                class C
                                {
                                    void M()
                                    {
                                        var a = new int[,
                                        ] { { 1 } };
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the closing bracket of the second rank specifier of a jagged array type behind an omitted array size
    /// takes the block column
    /// </summary>
    [TestMethod]
    [Requirement("SHARED-BLANK-008")]
    public void ClosingBracketOfSecondRankBehindOmittedArraySizeTakesBlockColumn()
    {
        // Arrange
        const string input = """
                             using System;
                             using System.Collections.Generic;

                             class C
                             {
                                 void M()
                                 {
                                     int[][,
                             ] a = null;
                                 }
                             }
                             """;
        const string expected = """
                                using System;
                                using System.Collections.Generic;

                                class C
                                {
                                    void M()
                                    {
                                        int[][,
                                        ] a = null;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the control of the omitted array size: an array size with text that starts its line takes the block
    /// column
    /// </summary>
    [TestMethod]
    public void ArraySizeWithTextOnWrappedLineTakesBlockColumn()
    {
        // Arrange
        const string input = """
                             using System;
                             using System.Collections.Generic;

                             class C
                             {
                                 void M()
                                 {
                                     var a = new int[2,
                             3];
                                 }
                             }
                             """;
        const string expected = """
                                using System;
                                using System.Collections.Generic;

                                class C
                                {
                                    void M()
                                    {
                                        var a = new int[2,
                                        3];
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the control of the omitted array size: the closing bracket behind an array size with text takes the block
    /// column
    /// </summary>
    [TestMethod]
    public void ClosingBracketBehindArraySizeWithTextTakesBlockColumn()
    {
        // Arrange
        const string input = """
                             using System;
                             using System.Collections.Generic;

                             class C
                             {
                                 void M()
                                 {
                                     var a = new int[2
                             ];
                                 }
                             }
                             """;
        const string expected = """
                                using System;
                                using System.Collections.Generic;

                                class C
                                {
                                    void M()
                                    {
                                        var a = new int[2
                                        ];
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Methods
}