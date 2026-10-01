using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Indentation;

/// <summary>
/// A wrapped line inside a nested list of a parameter - a tuple type or the value of a named attribute argument - is
/// aligned within that nested list instead of falling back to an outer column
/// </summary>
[TestClass]
public class NestedListInParameterAlignmentTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that the second element of a wrapped tuple type stays aligned with the first element
    /// </summary>
    [TestMethod]
    public void WrappedTupleTypeElementInParameterStaysAlignedWithFirstElement()
    {
        // Arrange
        const string input = """
                             namespace Demo;

                             internal class Example
                             {
                                 internal void M((int First,
                                                  int Second) pair,
                                                 int value)
                                 {
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that the value of a wrapped named attribute argument stays aligned under the argument name
    /// </summary>
    [TestMethod]
    public void WrappedNamedAttributeArgumentValueInParameterStaysAligned()
    {
        // Arrange
        const string input = """
                             using System;

                             namespace Demo;

                             internal class Example
                             {
                                 internal void M(int pair,
                                                 [Obsolete(message:
                                                               "x")] int value)
                                 {
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies the reported scenario of both nested lists in one parameter list stays unchanged
    /// </summary>
    [TestMethod]
    public void ReportedScenarioStaysUnchanged()
    {
        // Arrange
        const string input = """
                             using System;

                             namespace Demo;

                             internal class Example
                             {
                                 internal void M((int First,
                                                  int Second) pair,
                                                 [Obsolete(message:
                                                               "x")] int value)
                                 {
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    #endregion // Methods
}