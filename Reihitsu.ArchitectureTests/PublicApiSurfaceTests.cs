using ArchUnitNET.Fluent;
using ArchUnitNET.MSTestV4;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.ArchitectureTests.Properties;

using Assembly = System.Reflection.Assembly;

namespace Reihitsu.ArchitectureTests;

/// <summary>
/// Guards the public API surface of <c>Reihitsu.Formatter</c> so it cannot widen accidentally before the 1.0 contract is locked
/// </summary>
[TestClass]
public class PublicApiSurfaceTests
{
    #region Tests

    /// <summary>
    /// ReihitsuFormatter is the only intended public entry point and must stay public
    /// </summary>
    [TestMethod]
    public void ReihitsuFormatterShouldBePublic()
    {
        ArchRuleDefinition.Types()
                          .That()
                          .ResideInAssembly(Assembly.Load("Reihitsu.Formatter"))
                          .And()
                          .HaveName("ReihitsuFormatter")
                          .Should()
                          .BePublic()
                          .Check(Assemblies.Implementation);
    }

    /// <summary>
    /// No other type in the formatter assembly may be public
    /// </summary>
    [TestMethod]
    public void OnlyReihitsuFormatterShouldBePublicInFormatterAssembly()
    {
        ArchRuleDefinition.Types()
                          .That()
                          .ResideInAssembly(Assembly.Load("Reihitsu.Formatter"))
                          .And()
                          .ArePublic()
                          .Should()
                          .HaveName("ReihitsuFormatter")
                          .Check(Assemblies.Implementation);
    }

    #endregion // Tests
}