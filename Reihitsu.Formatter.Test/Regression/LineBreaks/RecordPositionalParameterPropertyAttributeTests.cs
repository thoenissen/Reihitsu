using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.LineBreaks;

/// <summary>
/// Positional record parameters carrying <c>property:</c> targeted attributes keep the attribute on the same line as the
/// parameter type and name
/// </summary>
[TestClass]
public class RecordPositionalParameterPropertyAttributeTests : FormatterTestsBase
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

    #endregion // Methods
}