using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Indentation;

/// <summary>
/// A line that starts inside a wrapped argument-like list at a separator comma - because a comment forbids joining it
/// to the previous line - is aligned with the list instead of the enclosing statement's block column
/// </summary>
[TestClass]
public class ArgumentContinuationLineAlignmentTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that a leading comma of a tuple expression kept on its own line by a line comment is aligned with the
    /// tuple's first element
    /// </summary>
    [TestMethod]
    public void TupleLeadingCommaAfterLineCommentStaysAligned()
    {
        // Arrange
        const string input = """
                             namespace Demo;

                             internal class Example
                             {
                                 internal void M()
                                 {
                                     var pair = (1 // keep
                                                 , 2);
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    #endregion // Methods
}