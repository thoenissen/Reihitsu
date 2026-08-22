using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.BlankLines;

/// <summary>
/// Tests for <see cref="Reihitsu.Formatter.Pipeline.FormattingPipeline"/> — blank line preservation
/// before a comment that is the final content of a scope (see issue #694)
/// </summary>
[TestClass]
public class BlankLineBeforeTrailingScopeCommentTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that a blank line separating a statement from a trailing comment that is the
    /// last content of a constructor body is preserved (see issue #694)
    /// </summary>
    [TestMethod]
    public void BlankLineBeforeTrailingCommentInConstructorBodyIsPreserved()
    {
        // Arrange
        const string input = """
                             public class Implementation
                             {
                                 public Implementation(Data data)
                                 {
                                     _data = data;

                                     // Comment
                                 }
                             }
                             """;

        const string expected = input;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a blank line separating a statement from a trailing multi-line comment that is
    /// the last content of a block is preserved (see issue #694)
    /// </summary>
    [TestMethod]
    public void BlankLineBeforeTrailingMultiLineCommentIsPreserved()
    {
        // Arrange
        const string input = """
                             public class Implementation
                             {
                                 public void Method()
                                 {
                                     DoSomething();

                                     /* Comment
                                        continues */
                                 }
                             }
                             """;

        const string expected = input;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a blank line separating a member from a trailing comment that is the last
    /// content of a nested type declaration is preserved (see issue #694)
    /// </summary>
    [TestMethod]
    public void BlankLineBeforeTrailingCommentInTypeDeclarationIsPreserved()
    {
        // Arrange
        const string input = """
                             public class Outer
                             {
                                 private class Inner
                                 {
                                     private int _value;

                                     // Comment
                                 }
                             }
                             """;

        const string expected = input;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a blank line separating a statement from a trailing comment that is the last
    /// content of a local function body is preserved (see issue #694)
    /// </summary>
    [TestMethod]
    public void BlankLineBeforeTrailingCommentInLocalFunctionIsPreserved()
    {
        // Arrange
        const string input = """
                             public class Implementation
                             {
                                 public void Method()
                                 {
                                     void Local()
                                     {
                                         DoSomething();

                                         // Comment
                                     }

                                     Local();
                                 }
                             }
                             """;

        const string expected = input;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a blank line separating an element from a trailing comment that is the last
    /// content of an object initializer is preserved (see issue #694)
    /// </summary>
    [TestMethod]
    public void BlankLineBeforeTrailingCommentInObjectInitializerIsPreserved()
    {
        // Arrange
        const string input = """
                             public class Implementation
                             {
                                 public void Method()
                                 {
                                     var data = new Data
                                     {
                                         Value = 1,

                                         // Comment
                                     };
                                 }
                             }
                             """;

        const string expected = """
                                public class Implementation
                                {
                                    public void Method()
                                    {
                                        var data = new Data
                                                   {
                                                       Value = 1,

                                                       // Comment
                                                   };
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that when a blank line separates a statement from a trailing comment, and another
    /// blank line separates that comment from the closing brace, only the blank line above the
    /// comment is preserved — the one directly before the closing brace is removed, matching RH5024
    /// (see issue #694)
    /// </summary>
    [TestMethod]
    public void BlankLineDirectlyBeforeClosingBraceIsRemovedWhenTrailingCommentPrecedesIt()
    {
        // Arrange
        const string input = """
                             public class Implementation
                             {
                                 public Implementation(Data data)
                                 {
                                     _data = data;

                                     // Comment

                                 }
                             }
                             """;

        const string expected = """
                                public class Implementation
                                {
                                    public Implementation(Data data)
                                    {
                                        _data = data;

                                        // Comment
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a trailing comment with no blank line above it gains one, matching the blank
    /// line RH5020 already requires before an own-line comment (see issue #694)
    /// </summary>
    [TestMethod]
    public void BlankLineIsInsertedBeforeTrailingCommentThatHasNone()
    {
        // Arrange
        const string input = """
                             public class Implementation
                             {
                                 public void Method()
                                 {
                                     DoSomething();
                                     // Comment
                                 }
                             }
                             """;

        const string expected = """
                                public class Implementation
                                {
                                    public void Method()
                                    {
                                        DoSomething();

                                        // Comment
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment immediately following the opening brace of a block is not preceded by
    /// a blank line, since it is the first content of the scope rather than trailing content (see
    /// issue #694)
    /// </summary>
    [TestMethod]
    public void NoBlankLineIsInsertedBeforeCommentFirstInBlock()
    {
        // Arrange
        const string input = """
                             public class Implementation
                             {
                                 public void Method()
                                 {
                                     // Comment
                                     DoSomething();
                                 }
                             }
                             """;

        const string expected = input;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Methods
}