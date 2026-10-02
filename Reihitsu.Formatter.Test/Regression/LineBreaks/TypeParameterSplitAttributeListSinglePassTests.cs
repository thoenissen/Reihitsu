using System.Threading;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Pipeline;
using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.LineBreaks;

/// <summary>
/// A multi-line type-parameter list whose elements carry split attribute lists settles in a single formatter pass
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
    /// Verifies that the counterpart shape on parameters of a record settles in a single pass
    /// </summary>
    [TestMethod]
    public void RecordParametersWithSplitAttributeListsSettleInOnePass()
    {
        // Arrange
        const string input = """
                             record Example([A]
                                            [B]
                                            int T,
                                            [A]
                                            [B]
                                            int U);
                             """;
        const string expected = """
                                record Example([A, B] int T,
                                               [A, B] int U);
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies per line ending that the first pass equals the settled layout and the second pass changes nothing
    /// </summary>
    [TestMethod]
    public void IssueScenarioFirstAndSecondPassPerLineEnding()
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
        const string expected = "interface IExample<[A, B] out T, [A, B] in U>;";
        var failures = new System.Collections.Generic.List<string>();

        foreach (var endOfLine in _lineEndings)
        {
            // Act
            var firstPass = Format(NormalizeLineEndings(input, endOfLine), endOfLine);
            var secondPass = Format(firstPass, endOfLine);
            var thirdPass = Format(secondPass, endOfLine);

            // Assert
            if (firstPass != expected)
            {
                failures.Add($"{DescribeLineEnding(endOfLine)} pass 1: {firstPass.Replace("\r", "\\r").Replace("\n", "\\n")}");
            }

            if (secondPass != expected)
            {
                failures.Add($"{DescribeLineEnding(endOfLine)} pass 2: {secondPass.Replace("\r", "\\r").Replace("\n", "\\n")}");
            }

            if (thirdPass != secondPass)
            {
                failures.Add($"{DescribeLineEnding(endOfLine)} pass 3 differs from pass 2");
            }
        }

        Assert.IsEmpty(failures, string.Join(" | ", failures));
    }

    #endregion // Methods

    #region Helpers

    /// <summary>
    /// Runs the complete formatting pipeline once
    /// </summary>
    /// <param name="input">The source text to format</param>
    /// <param name="endOfLine">The end-of-line sequence to format with</param>
    /// <returns>The formatted source text</returns>
    private static string Format(string input, string endOfLine)
    {
        var tree = CSharpSyntaxTree.ParseText(input);

        return FormattingPipeline.Execute(tree.GetRoot(), new FormattingContext(endOfLine), CancellationToken.None).ToFullString();
    }

    #endregion // Helpers
}