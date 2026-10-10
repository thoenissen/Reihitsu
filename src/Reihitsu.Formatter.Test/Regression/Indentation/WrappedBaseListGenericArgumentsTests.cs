using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Indentation;

/// <summary>
/// Regression tests for generic type-argument lists inside a wrapped base-type list
/// </summary>
[TestClass]
public class WrappedBaseListGenericArgumentsTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// A stacked generic type-argument list inside a wrapped base-type list should be joined onto a
    /// single line when it fits, while the surrounding base-type list stays wrapped
    /// </summary>
    [TestMethod]
    public void StackedTypeArgumentsInBaseListJoinOntoOneLine()
    {
        // Arrange
        const string input = """
                             public class Implementation : Base<T1,
                                                                T2,
                                                                T3,
                                                                T4>,
                                                           IInterface1,
                                                           IInterface2
                             {
                                 public void Foo()
                                 {
                                 }
                             }
                             """;

        const string expected = """
                                public class Implementation : Base<T1, T2, T3, T4>,
                                                              IInterface1,
                                                              IInterface2
                                {
                                    public void Foo()
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Methods
}