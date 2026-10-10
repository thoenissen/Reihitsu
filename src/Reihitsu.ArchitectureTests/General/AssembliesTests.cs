using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.ArchitectureTests.Properties;

namespace Reihitsu.ArchitectureTests.General;

/// <summary>
/// Guards the assembly sets exercised by the architecture rules
/// </summary>
[TestClass]
public sealed class AssembliesTests
{
    #region Methods

    /// <summary>
    /// Verifies that the architecture-rule universe includes both tooling assemblies
    /// </summary>
    [TestMethod]
    public void AllIncludesToolingAssemblies()
    {
        // Arrange
        var assemblyNames = Assemblies.All.Assemblies.Select(assembly => assembly.Name).ToList();

        // Act and assert
        Assert.Contains("Reihitsu.Tooling", assemblyNames);
        Assert.Contains("Reihitsu.Tooling.Test", assemblyNames);
    }

    /// <summary>
    /// Verifies that the implementation-only architecture set remains limited to shipped implementation projects
    /// </summary>
    [TestMethod]
    public void ImplementationRemainsProductOnly()
    {
        // Arrange
        var assemblyNames = Assemblies.Implementation.Assemblies.Select(assembly => assembly.Name).ToList();
        var expected = new[]
                       {
                           "Reihitsu.Analyzer",
                           "Reihitsu.Analyzer.CodeFixes",
                           "Reihitsu.Core",
                           "Reihitsu.Cli",
                           "Reihitsu.Formatter"
                       };

        // Act and assert
        Assert.AreSequenceEqual(expected, assemblyNames, SequenceOrder.InAnyOrder);
    }

    #endregion // Methods
}