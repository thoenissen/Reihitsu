using ArchUnitNET.Fluent;
using ArchUnitNET.MSTestV4;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.ArchitectureTests.Properties;

namespace Reihitsu.ArchitectureTests;

/// <summary>
/// Rules tests
/// </summary>
[TestClass]
public class RulesTests
{
    #region Tests

    /// <summary>
    /// Rule implementation should be in Rules namespace
    /// </summary>
    [TestMethod]
    public void ShouldBeInRulesNamespace()
    {
        ArchRuleDefinition.Types()
                          .That()
                          .HaveNameMatching(@"RH\d\d\d\d.*")
                          .Should()
                          .ResideInNamespaceMatching(@"^Reihitsu\..*\.Rules\..*$")
                          .Check(Assemblies.Implementation);
    }

    /// <summary>
    /// Other types should not be in Rules namespace
    /// </summary>
    [TestMethod]
    public void OtherTypesShouldNotBeInRulesNamespace()
    {
        ArchRuleDefinition.Types()
                          .That()
                          .DoNotHaveNameMatching(@"RH\d\d\d\d.*")
                          .Should()
                          .NotResideInNamespaceMatching(@"^Reihitsu\..*\.Rules\..*$")
                          .Check(Assemblies.Implementation);
    }

    #endregion // Tests
}