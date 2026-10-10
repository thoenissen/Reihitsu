using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.LineBreaks;

/// <summary>
/// A wrapped type-parameter list whose elements only span lines because of attribute-list gaps settles in a single
/// formatter pass: the collapse decision sees the elements as attribute formatting emits them in the same pass
/// </summary>
[TestClass]
public class TypeParameterSplitAttributeListSinglePassTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that two type parameters with split attribute lists of the same target are merged and joined in a single pass
    /// </summary>
    [TestMethod]
    public void TwoTypeParametersWithSplitAttributeListsSettleInOnePass()
    {
        // Arrange
        const string input = """
                             interface IExample<[A]
                                                [B]
                                                out T,
                                                [A]
                                                [B]
                                                in U>;
                             """;
        const string expected = """
                                interface IExample<[A, B] out T, [A, B] in U>;
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the same shape on a class declaration settles in a single pass
    /// </summary>
    [TestMethod]
    public void ClassTwoTypeParametersWithSplitAttributeListsSettleInOnePass()
    {
        // Arrange
        const string input = """
                             class Example<[A]
                                           [B]
                                           T,
                                           [A]
                                           [B]
                                           U>;
                             """;
        const string expected = """
                                class Example<[A, B] T, [A, B] U>;
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the same shape on a method declaration settles in a single pass
    /// </summary>
    [TestMethod]
    public void MethodTwoTypeParametersWithSplitAttributeListsSettleInOnePass()
    {
        // Arrange
        const string input = """
                             class Example
                             {
                                 void M<[A]
                                        [B]
                                        T,
                                        [A]
                                        [B]
                                        U>()
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                class Example
                                {
                                    void M<[A, B] T, [A, B] U>()
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the type parameter list settles in a single pass when only the second type parameter carries split attribute lists
    /// </summary>
    [TestMethod]
    public void OnlySecondTypeParameterWithSplitAttributeListsSettlesInOnePass()
    {
        // Arrange
        const string input = """
                             interface IExample<T,
                                                [A]
                                                [B]
                                                U>;
                             """;
        const string expected = """
                                interface IExample<T, [A, B] U>;
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the type parameter list settles in a single pass when only the first type parameter carries split attribute lists
    /// </summary>
    [TestMethod]
    public void OnlyFirstTypeParameterWithSplitAttributeListsSettlesInOnePass()
    {
        // Arrange
        const string input = """
                             interface IExample<[A]
                                                [B]
                                                T,
                                                U>;
                             """;
        const string expected = """
                                interface IExample<[A, B] T, U>;
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the type parameter list settles in a single pass when only its middle type parameter carries split attribute lists
    /// </summary>
    [TestMethod]
    public void OnlyMiddleTypeParameterWithSplitAttributeListsSettlesInOnePass()
    {
        // Arrange
        const string input = """
                             class Example<T,
                                           [A]
                                           [B]
                                           U,
                                           V>;
                             """;
        const string expected = """
                                class Example<T, [A, B] U, V>;
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a single type parameter with split attribute lists settles in a single pass when the list also breaks after the opening angle bracket
    /// </summary>
    [TestMethod]
    public void SingleTypeParameterWithBreakAfterOpenAngleBracketSettlesInOnePass()
    {
        // Arrange
        const string input = """
                             interface IExample<
                                                [A]
                                                [B]
                                                T>;
                             """;
        const string expected = """
                                interface IExample<[A, B] T>;
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a single type parameter with split attribute lists settles in a single pass when the list also breaks before the closing angle bracket
    /// </summary>
    [TestMethod]
    public void SingleTypeParameterWithBreakBeforeCloseAngleBracketSettlesInOnePass()
    {
        // Arrange
        const string input = """
                             interface IExample<[A]
                                                [B]
                                                T
                                                >;
                             """;
        const string expected = """
                                interface IExample<[A, B] T>;
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a single attribute list that only needs to move onto its type parameter's line settles in a single pass
    /// </summary>
    [TestMethod]
    public void AttributeListPlacedOnTypeParameterLineSettlesInOnePass()
    {
        // Arrange
        const string input = """
                             interface IExample<[A]
                                                T,
                                                U>;
                             """;
        const string expected = """
                                interface IExample<[A] T, U>;
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that attribute lists of different targets stay separate and still settle in a single pass
    /// </summary>
    [TestMethod]
    public void AttributeListsOfDifferentTargetsSettleInOnePass()
    {
        // Arrange
        const string input = """
                             interface IExample<[typevar: A]
                                                [param: B]
                                                T,
                                                U>;
                             """;
        const string expected = """
                                interface IExample<[typevar: A] [param: B] T, U>;
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a type parameter list with a leading comma settles in a single pass
    /// </summary>
    [TestMethod]
    public void LeadingCommaTypeParameterListSettlesInOnePass()
    {
        // Arrange
        const string input = """
                             interface IExample<[A]
                                                [B]
                                                T
                                                , U>;
                             """;
        const string expected = """
                                interface IExample<[A, B] T, U>;
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a blank line between the split attribute lists does not prevent the list from settling in a single pass
    /// </summary>
    [TestMethod]
    public void BlankLineBetweenSplitAttributeListsSettlesInOnePass()
    {
        // Arrange
        const string input = """
                             interface IExample<[A]

                                                [B]
                                                T,
                                                U>;
                             """;
        const string expected = """
                                interface IExample<[A, B] T, U>;
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the shape on a delegate declaration settles in a single pass
    /// </summary>
    [TestMethod]
    public void DelegateTypeParametersWithSplitAttributeListsSettleInOnePass()
    {
        // Arrange
        const string input = """
                             delegate void D<[A]
                                             [B]
                                             T,
                                             U>();
                             """;
        const string expected = """
                                delegate void D<[A, B] T, U>();
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the shape on a local function settles in a single pass
    /// </summary>
    [TestMethod]
    public void LocalFunctionTypeParametersWithSplitAttributeListsSettleInOnePass()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     void L<[A]
                                            [B]
                                            T,
                                            U>()
                                     {
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        void L<[A, B] T, U>()
                                        {
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the shape on a method with a constraint clause settles in a single pass and keeps the constraint layout
    /// </summary>
    [TestMethod]
    public void MethodWithConstraintClauseTypeParametersSettleInOnePass()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M<[A]
                                        [B]
                                        T,
                                        U>()
                                     where T : class
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M<[A, B] T, U>()
                                        where T : class
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that nested generic owners both settle in a single pass
    /// </summary>
    [TestMethod]
    public void NestedGenericOwnersSettleInOnePass()
    {
        // Arrange
        const string input = """
                             class Outer<[A]
                                         [B]
                                         T,
                                         U>
                             {
                                 void M<[A]
                                        [B]
                                        V,
                                        W>()
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                class Outer<[A, B] T, U>
                                {
                                    void M<[A, B] V, W>()
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment between the attribute lists still keeps the type parameter list wrapped
    /// </summary>
    [TestMethod]
    public void CommentBetweenAttributeListsKeepsListWrapped()
    {
        // Arrange
        const string input = """
                             interface IExample<[A] // c
                                                [B]
                                                T,
                                                U>;
                             """;
        const string expected = """
                                interface IExample<[A] // c
                                                   [B] T,
                                                   U>;
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a type parameter that still spans lines after attribute formatting keeps the list wrapped
    /// </summary>
    [TestMethod]
    public void MultiLineAttributeArgumentsKeepListWrapped()
    {
        // Arrange
        const string input = """
                             interface IExample<[A(1,
                                                   2)] T,
                                                U>;
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a type parameter list whose attribute lists already sit on the type parameter's line is collapsed in a single pass
    /// </summary>
    [TestMethod]
    public void AttributeListsOnTypeParameterLineCollapseInOnePass()
    {
        // Arrange
        const string input = """
                             interface IExample<[A] [B] T,
                                                U>;
                             """;
        const string expected = """
                                interface IExample<[A, B] T, U>;
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Methods
}