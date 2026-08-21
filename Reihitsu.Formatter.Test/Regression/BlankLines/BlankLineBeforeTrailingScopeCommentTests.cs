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

    #endregion // Methods
}