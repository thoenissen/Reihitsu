using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.LineBreaks;

/// <summary>
/// Attribute lists on parameters and type parameters follow the parameter layout whatever target they name: they stay
/// on the parameter's line, are not split, and are merged only with lists of the same target
/// </summary>
[TestClass]
public class ParameterAttributeTargetLayoutTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that a record whose positional parameters are aligned one per line and carry
    /// <c>property:</c> attributes is left unchanged
    /// </summary>
    [TestMethod]
    public void PropertyTargetedAttributesOnPositionalRecordParametersStayOnParameterLine()
    {
        // Arrange
        const string input = """
                             using System.Text.Json.Serialization;

                             namespace Demo;

                             internal sealed record PortainerEndpointItem([property: JsonPropertyName("Id")] int Id,
                                                                          [property: JsonPropertyName("Name")] string? Name);
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a <c>field:</c> attribute on a class primary-constructor parameter stays on the parameter's line
    /// </summary>
    [TestMethod]
    public void FieldTargetedAttributeOnClassPrimaryConstructorParameterStaysOnParameterLine()
    {
        // Arrange
        const string input = """
                             using System;

                             namespace Demo;

                             internal sealed class Example([field: Obsolete] int id)
                             {
                                 public int Id => id;
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a <c>property:</c> attribute on an ordinary method parameter stays on the parameter's line
    /// </summary>
    [TestMethod]
    public void PropertyTargetedAttributeOnMethodParameterStaysOnParameterLine()
    {
        // Arrange
        const string input = """
                             using System;

                             namespace Demo;

                             internal class Example
                             {
                                 internal void M([property: Obsolete] int value)
                                 {
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that an attribute list with an unrecognized target specifier on a parameter stays on the parameter's line
    /// </summary>
    [TestMethod]
    public void UnrecognizedTargetedAttributeOnParameterStaysOnParameterLine()
    {
        // Arrange
        const string input = """
                             using System;

                             namespace Demo;

                             internal sealed record Example([unknown: Obsolete] int Id);
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a <c>property:</c> attribute list with several attributes on a parameter is not split into one list per attribute
    /// </summary>
    [TestMethod]
    public void PropertyTargetedAttributeListWithSeveralAttributesIsNotSplit()
    {
        // Arrange
        const string input = """
                             using System;

                             namespace Demo;

                             internal sealed record Example([property: Obsolete, CLSCompliant(false)] int Id);
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that two <c>property:</c> attribute lists on one parameter are merged into one list
    /// </summary>
    [TestMethod]
    public void PropertyTargetedAttributeListsOnOneParameterAreMerged()
    {
        // Arrange
        const string input = """
                             using System;

                             namespace Demo;

                             internal sealed record Example([property: Obsolete] [property: CLSCompliant(false)] int Id);
                             """;
        const string expected = """
                                using System;

                                namespace Demo;

                                internal sealed record Example([property: Obsolete, CLSCompliant(false)] int Id);
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a <c>property:</c> list and a list without a specifier on one parameter are not merged, because the merged list would move the second attribute to the property
    /// </summary>
    [TestMethod]
    public void PropertyTargetedAndImplicitAttributeListsAreNotMerged()
    {
        // Arrange
        const string input = """
                             using System;

                             namespace Demo;

                             internal sealed record Example([property: Obsolete] [CLSCompliant(false)] int Id);
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a <c>property:</c> list and a <c>field:</c> list on one parameter are not merged
    /// </summary>
    [TestMethod]
    public void PropertyAndFieldTargetedAttributeListsAreNotMerged()
    {
        // Arrange
        const string input = """
                             using System;

                             namespace Demo;

                             internal sealed record Example([property: Obsolete] [field: CLSCompliant(false)] int Id);
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a <c>param:</c> list and a list without a specifier on one parameter are still merged, because both apply to the parameter
    /// </summary>
    [TestMethod]
    public void ParameterTargetedAndImplicitAttributeListsAreMerged()
    {
        // Arrange
        const string input = """
                             using System;

                             namespace Demo;

                             internal sealed record Example([param: Obsolete] [CLSCompliant(false)] int Id);
                             """;
        const string expected = """
                                using System;

                                namespace Demo;

                                internal sealed record Example([param: Obsolete, CLSCompliant(false)] int Id);
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a <c>typevar:</c> list and a list without a specifier on one parameter are not merged, because they resolve to different targets
    /// </summary>
    [TestMethod]
    public void TypeVariableTargetedAndImplicitAttributeListsOnParameterAreNotMerged()
    {
        // Arrange
        const string input = """
                             using System;

                             namespace Demo;

                             internal sealed record Example([typevar: Obsolete] [CLSCompliant(false)] int Id);
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a <c>typevar:</c> list and a <c>param:</c> list on one type parameter are not merged, because they resolve to different targets
    /// </summary>
    [TestMethod]
    public void TypeVariableAndParameterTargetedAttributeListsOnTypeParameterAreNotMerged()
    {
        // Arrange
        const string input = """
                             using System;

                             namespace Demo;

                             internal class Example<[typevar: Obsolete] [param: CLSCompliant(false)] T>;
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that <c>property:</c> attributes placed on their own lines before positional record parameters are joined onto the parameters' lines
    /// </summary>
    [TestMethod]
    public void PropertyTargetedAttributesOnBrokenPositionalRecordParametersAreJoined()
    {
        // Arrange
        const string input = """
                             using System;

                             namespace Demo;

                             internal sealed record Example([property: Obsolete]
                                                            int Id,
                                                            [property: CLSCompliant(false)]
                                                            string? Name);
                             """;
        const string expected = """
                                using System;

                                namespace Demo;

                                internal sealed record Example([property: Obsolete] int Id,
                                                               [property: CLSCompliant(false)] string? Name);
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a <c>property:</c> attribute followed by a block comment on the parameter's line is left unchanged
    /// </summary>
    [TestMethod]
    public void PropertyTargetedAttributeFollowedByBlockCommentStaysOnParameterLine()
    {
        // Arrange
        const string input = """
                             using System;

                             namespace Demo;

                             internal sealed record Example([property: Obsolete] /* keep */ int Id);
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a <c>property:</c> attribute on a type parameter stays on the type parameter's line
    /// </summary>
    [TestMethod]
    public void PropertyTargetedAttributeOnTypeParameterStaysOnTypeParameterLine()
    {
        // Arrange
        const string input = """
                             using System;

                             namespace Demo;

                             internal class Example<[property: Obsolete] T>;
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a <c>property:</c> attribute list with several attributes on a type parameter is not split
    /// </summary>
    [TestMethod]
    public void PropertyTargetedAttributeListWithSeveralAttributesOnTypeParameterIsNotSplit()
    {
        // Arrange
        const string input = """
                             using System;

                             namespace Demo;

                             internal class Example<[property: Obsolete, CLSCompliant(false)] T>;
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a <c>property:</c> attribute on a parameter that follows a plain parameter stays on the parameter's line
    /// </summary>
    [TestMethod]
    public void PropertyTargetedAttributeAfterPlainParameterStaysOnParameterLine()
    {
        // Arrange
        const string input = """
                             using System;

                             namespace Demo;

                             internal sealed record Example(int Other, [property: Obsolete] int Id);
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a <c>property:</c> attribute on a property declaration is still moved onto its own line
    /// </summary>
    [TestMethod]
    public void PropertyTargetedAttributeOnPropertyIsStillPlacedOnItsOwnLine()
    {
        // Arrange
        const string input = """
                             using System;

                             namespace Demo;

                             internal class Example
                             {
                                 [property: Obsolete] public int Value { get; set; }
                             }
                             """;
        const string expected = """
                                using System;

                                namespace Demo;

                                internal class Example
                                {
                                    [property: Obsolete]
                                    public int Value { get; set; }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a <c>property:</c> attribute list with several attributes on a property declaration is still split into one list per attribute
    /// </summary>
    [TestMethod]
    public void PropertyTargetedAttributeListWithSeveralAttributesOnPropertyIsStillSplit()
    {
        // Arrange
        const string input = """
                             using System;

                             namespace Demo;

                             internal class Example
                             {
                                 [property: Obsolete, CLSCompliant(false)]
                                 public int Value { get; set; }
                             }
                             """;
        const string expected = """
                                using System;

                                namespace Demo;

                                internal class Example
                                {
                                    [property: Obsolete]
                                    [property: CLSCompliant(false)]
                                    public int Value { get; set; }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Methods
}