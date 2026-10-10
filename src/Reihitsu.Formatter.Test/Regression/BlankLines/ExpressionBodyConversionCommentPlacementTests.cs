using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.BlankLines;

/// <summary>
/// Regression tests verifying that a comment separated from the arrow of an expression-bodied void member by a blank line
/// is placed on its own line inside the block that replaces the expression body, rather than being joined to the opening
/// brace
/// </summary>
[TestClass]
public class ExpressionBodyConversionCommentPlacementTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies the comment placement for an expression-bodied void method
    /// </summary>
    [TestMethod]
    public void PlacesCommentOnOwnLineInConvertedMethodBody()
    {
        // Arrange
        const string input = """
                             public class C
                             {
                                 public void M() =>

                                     // c
                                     Call();

                                 public void Call()
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                public class C
                                {
                                    public void M()
                                    {
                                        // c
                                        Call();
                                    }

                                    public void Call()
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the comment placement for an expression-bodied constructor
    /// </summary>
    [TestMethod]
    public void PlacesCommentOnOwnLineInConvertedConstructorBody()
    {
        // Arrange
        const string input = """
                             public class C
                             {
                                 public C() =>

                                     // c
                                     Call();

                                 public void Call()
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                public class C
                                {
                                    public C()
                                    {
                                        // c
                                        Call();
                                    }

                                    public void Call()
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the comment placement for an expression-bodied local function
    /// </summary>
    [TestMethod]
    public void PlacesCommentOnOwnLineInConvertedLocalFunctionBody()
    {
        // Arrange
        const string input = """
                             public class C
                             {
                                 public void M()
                                 {
                                     void Local() =>

                                         // c
                                         Call();

                                     Local();
                                 }

                                 public void Call()
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                public class C
                                {
                                    public void M()
                                    {
                                        void Local()
                                        {
                                            // c
                                            Call();
                                        }

                                        Local();
                                    }

                                    public void Call()
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Methods
}