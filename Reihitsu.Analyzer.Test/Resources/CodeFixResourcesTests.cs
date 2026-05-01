using System.Linq;
using System.Reflection;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reihitsu.Analyzer.Test.Resources;

/// <summary>
/// Tests for <see cref="CodeFixResources"/>
/// </summary>
[TestClass]
public class CodeFixResourcesTests
{
    #region Tests

    /// <summary>
    /// Verifies that all non-public static string properties of the AnalyzerResources type are not null or empty
    /// </summary>
    [TestMethod]
    public void AllPropertiesShouldNotBeNullOrEmpty()
    {
        var properties = typeof(CodeFixResources).GetProperties(BindingFlags.NonPublic | BindingFlags.Static);

        Assert.IsGreaterThan(0, properties.Length);

        foreach (var property in properties.Where(p => p.PropertyType == typeof(string)))
        {
            var value = property.GetValue(null) as string;

            Assert.IsFalse(string.IsNullOrEmpty(value), $"Property {property.Name} is null or empty.");
        }
    }

    #endregion // Tests
}