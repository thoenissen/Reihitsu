using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Indentation;

/// <summary>
/// A wrapped line inside a list nested in a declaration's type or attribute syntax - a tuple type, an attribute list, an
/// unmanaged calling convention list, or a named attribute argument - is aligned within that nested list instead of falling
/// back to an outer column
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
    /// Verifies that a wrapped tuple type element placed under the opening parenthesis is moved under the first element
    /// </summary>
    [TestMethod]
    public void WrappedTupleTypeElementInParameterIsMovedUnderTheFirstElement()
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

        const string expected = """
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
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the value of a wrapped named attribute argument of a parameter is pulled up onto the argument name's line
    /// </summary>
    [TestMethod]
    public void WrappedNamedAttributeArgumentValueInParameterIsJoined()
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

        const string expected = """
                                using System;

                                namespace Demo;

                                internal class Example
                                {
                                    internal void M(int pair,
                                                    [Obsolete(message: "x")] int value)
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the reported scenario: the tuple type element is aligned with the first element, and the named attribute value is
    /// joined onto its argument name
    /// </summary>
    [TestMethod]
    public void ReportedScenarioAlignsTheTupleTypeAndJoinsTheNamedValue()
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

        const string expected = """
                                using System;

                                namespace Demo;

                                internal class Example
                                {
                                    internal void M((int First,
                                                     int Second) pair,
                                                    [Obsolete(message: "x")] int value)
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a wrapped tuple type of a field is aligned with the first element instead of the member column
    /// </summary>
    [TestMethod]
    public void WrappedTupleTypeOfFieldIsAlignedWithTheFirstElement()
    {
        // Arrange
        const string input = """
                             namespace Demo;

                             internal class Example
                             {
                                 private (int First,
                                 int Second) _pair;
                             }
                             """;

        const string expected = """
                                namespace Demo;

                                internal class Example
                                {
                                    private (int First,
                                             int Second) _pair;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a wrapped tuple type of a local declaration is aligned with the first element instead of the statement column
    /// </summary>
    [TestMethod]
    public void WrappedTupleTypeOfLocalIsAlignedWithTheFirstElement()
    {
        // Arrange
        const string input = """
                             namespace Demo;

                             internal class Example
                             {
                                 internal void M()
                                 {
                                     (int First,
                                     int Second) pair = default;
                                 }
                             }
                             """;

        const string expected = """
                                namespace Demo;

                                internal class Example
                                {
                                    internal void M()
                                    {
                                        (int First,
                                         int Second) pair = default;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a wrapped tuple return type is aligned with the first element
    /// </summary>
    [TestMethod]
    public void WrappedTupleTypeOfReturnTypeIsAlignedWithTheFirstElement()
    {
        // Arrange
        const string input = """
                             namespace Demo;

                             internal class Example
                             {
                                 internal (int First,
                                 int Second) Create()
                                 {
                                     return default;
                                 }
                             }
                             """;

        const string expected = """
                                namespace Demo;

                                internal class Example
                                {
                                    internal (int First,
                                              int Second) Create()
                                    {
                                        return default;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a tuple type used as a type argument aligns its wrapped element with its own first element
    /// </summary>
    [TestMethod]
    public void WrappedTupleTypeInTypeArgumentIsAlignedWithItsOwnFirstElement()
    {
        // Arrange
        const string input = """
                             namespace Demo;

                             internal class Example
                             {
                                 private System.Collections.Generic.List<(int First,
                                 int Second)> _pairs;
                             }
                             """;

        const string expected = """
                                namespace Demo;

                                internal class Example
                                {
                                    private System.Collections.Generic.List<(int First,
                                                                             int Second)> _pairs;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that each level of nested wrapped tuple types aligns with its own first element
    /// </summary>
    [TestMethod]
    public void NestedWrappedTupleTypesAreAlignedPerLevel()
    {
        // Arrange
        const string input = """
                             namespace Demo;

                             internal class Example
                             {
                                 internal void M(((int A,
                                 int B) First,
                                 int Second) pair)
                                 {
                                 }
                             }
                             """;

        const string expected = """
                                namespace Demo;

                                internal class Example
                                {
                                    internal void M(((int A,
                                                      int B) First,
                                                     int Second) pair)
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a leading comma of a tuple type kept on its own line by a line comment is aligned with the elements
    /// </summary>
    [TestMethod]
    public void TupleTypeLeadingCommaHeldByLineCommentIsAlignedWithTheElements()
    {
        // Arrange
        const string input = """
                             namespace Demo;

                             internal class Example
                             {
                                 private (int First // keep
                                 , int Second) _pair;
                             }
                             """;

        const string expected = """
                                namespace Demo;

                                internal class Example
                                {
                                    private (int First // keep
                                             , int Second) _pair;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a tuple type on one line is left unchanged
    /// </summary>
    [TestMethod]
    public void SingleLineTupleTypeIsUnchanged()
    {
        // Arrange
        const string input = """
                             namespace Demo;

                             internal class Example
                             {
                                 private (int First, int Second) _pair;
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that the second attribute of a wrapped parameter attribute list is aligned with the first attribute
    /// </summary>
    [TestMethod]
    public void WrappedAttributeListOfParameterIsAlignedWithTheFirstAttribute()
    {
        // Arrange
        const string input = """
                             using System;

                             namespace Demo;

                             internal class Example
                             {
                                 internal void M([Obsolete,
                                                 CLSCompliant(false)] int value)
                                 {
                                 }
                             }
                             """;

        const string expected = """
                                using System;

                                namespace Demo;

                                internal class Example
                                {
                                    internal void M([Obsolete,
                                                     CLSCompliant(false)] int value)
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the second attribute of a wrapped attribute list with a target specifier is aligned with the first attribute
    /// </summary>
    [TestMethod]
    public void WrappedTargetedAttributeListIsAlignedWithTheFirstAttribute()
    {
        // Arrange
        const string input = """
                             using System;

                             namespace Demo;

                             internal class Example
                             {
                                 [return: Obsolete, // keep
                                 CLSCompliant(false)]
                                 internal int M()
                                 {
                                     return 0;
                                 }
                             }
                             """;

        const string expected = """
                                using System;

                                namespace Demo;

                                internal class Example
                                {
                                    [return: Obsolete, // keep
                                             CLSCompliant(false)]
                                    internal int M()
                                    {
                                        return 0;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the second calling convention of a wrapped unmanaged calling convention list is aligned with the first one
    /// </summary>
    [TestMethod]
    public void WrappedUnmanagedCallingConventionListIsAlignedWithTheFirstConvention()
    {
        // Arrange
        const string input = """
                             namespace Demo;

                             internal class Example
                             {
                                 internal unsafe void M(delegate* unmanaged[Cdecl, // keep
                                 SuppressGCTransition]<void> callback)
                                 {
                                 }
                             }
                             """;

        const string expected = """
                                namespace Demo;

                                internal class Example
                                {
                                    internal unsafe void M(delegate* unmanaged[Cdecl, // keep
                                                                               SuppressGCTransition]<void> callback)
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Methods
}