using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.BlankLines;

/// <summary>
/// Tests for <see cref="Reihitsu.Formatter.Pipeline.FormattingPipeline"/>
/// </summary>
[TestClass]
public class BlankLineBeforeIfDirectiveAfterStatementTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that a single blank line already present between a statement and a following
    /// <c>#if</c> directive is preserved rather than doubled (issue #695)
    /// </summary>
    [TestMethod]
    public void SingleBlankLineBeforeIfDirectiveIsPreserved()
    {
        // Arrange
        const string input = """
                             public class Implementation
                             {
                                 public void Start()
                                 {
                                     var bar = "foo1";

                             #if DEBUG
                                     bar = "foo2";
                             #endif

                                     bar = "foo2";
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    #endregion // Methods
}