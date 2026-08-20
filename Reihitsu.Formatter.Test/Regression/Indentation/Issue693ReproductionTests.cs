using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Indentation;

/// <summary>
/// Reproduction-gate test for issue #693 — asserted against the reporter's own chat clarification
/// (the issue body's stored code blocks lost the generic type-argument list), so a failing run shows
/// the issue's own observed-vs-expected difference
/// </summary>
[TestClass]
public class Issue693ReproductionTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Issue #693 — a stacked generic type-argument list inside a wrapped base-type list should be
    /// joined onto a single line when it fits, while the surrounding base-type list stays wrapped, per
    /// the reporter's own chat clarification (verbatim Input / Expected)
    /// </summary>
    [TestMethod]
    public void Issue693StackedTypeArgumentsInBaseListJoinOntoOneLine()
    {
        // Arrange — verbatim from the reporter's chat clarification "Input"
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

        // verbatim from the reporter's chat clarification "Expected"
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