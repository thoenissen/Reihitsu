using ArchUnitNET.Fluent;
using ArchUnitNET.MSTestV4;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.ArchitectureTests.Properties;

namespace Reihitsu.ArchitectureTests;

/// <summary>
/// Architecture tests
/// </summary>
[TestClass]
public class EnumerationsTests
{
    #region Tests

    /// <summary>
    /// Enumerations should be in an enumeration namespace
    /// </summary>
    [TestMethod]
    public void ShouldBeInEnumerationsNamespace()
    {
        ArchRuleDefinition.Types()
                          .That()
                          .AreEnums()
                          .Should()
                          .ResideInNamespaceMatching(@"^Reihitsu\..*\.Enumerations(\..*|)$")
                          .Check(Assemblies.All);
    }

    /// <summary>
    /// Other types should not be in an enumeration namespace
    /// </summary>
    [TestMethod]
    public void OtherTypesShouldNotBeInEnumerationsNamespace()
    {
        ArchRuleDefinition.Types()
                          .That()
                          .AreNotEnums()
                          .Should()
                          .NotResideInNamespaceMatching(@"^Reihitsu\..*\.Enumerations(\..*|)$")
                          .Check(Assemblies.All);
    }

    #endregion // Tests
}