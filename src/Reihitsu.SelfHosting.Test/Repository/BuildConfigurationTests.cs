using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Xml.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.SelfHosting.Test.Utilities;

namespace Reihitsu.SelfHosting.Test.Repository;

/// <summary>
/// Verifies how the repository's own build configures the Reihitsu analyzers it runs on its code: the limits of its
/// root <c>reihitsu.json</c>, which it passes to every project, and the MSBuild properties that tell the analyzers a
/// test project from a production project, as the package's build files tell them to every other project
/// </summary>
[TestClass]
public sealed class BuildConfigurationTests
{
    #region Constants

    /// <summary>
    /// File name of the configuration file the analyzers read
    /// </summary>
    private const string ConfigurationFileName = "reihitsu.json";

    /// <summary>
    /// Path of the package's build file that makes the MSBuild properties visible to the analyzers, relative to the
    /// repository root
    /// </summary>
    private const string PackageBuildFilePath = "src/Reihitsu.Analyzer.Package/buildTransitive/Reihitsu.Analyzer.targets";

    #endregion // Constants

    #region Fields

    /// <summary>
    /// The shipped default of every line-count limit setting of the <c>design</c> section (RH2008-001, RH2111-001)
    /// </summary>
    private static readonly Dictionary<string, int> _shippedDefaultLimits = new(StringComparer.Ordinal)
                                                                            {
                                                                                ["maximumMethodLines"] = 60,
                                                                                ["maximumTestMethodLines"] = 120,
                                                                                ["maximumTypeLines"] = 1500,
                                                                                ["maximumTestTypeLines"] = 3000
                                                                            };

    #endregion // Fields

    #region Tests

    /// <summary>
    /// Verifies that the repository's root configuration holds its production types to 500 lines of code and sets
    /// nothing else
    /// </summary>
    [TestMethod]
    public void RootConfigurationSetsOnlyTheProductionTypeLimitOfFiveHundred()
    {
        // Arrange
        var configuration = LoadRootConfiguration();

        // Act
        var sections = configuration.EnumerateObject().Select(property => property.Name).ToList();
        var settings = configuration.GetProperty("design").EnumerateObject().Select(property => $"{property.Name}={property.Value.GetRawText()}").ToList();

        // Assert
        Assert.AreSequenceEqual(new[] { "design" }, sections);
        Assert.AreSequenceEqual(new[] { "maximumTypeLines=500" }, settings);
    }

    /// <summary>
    /// Verifies that every line-count limit the repository's root configuration sets is lower than the shipped default
    /// of that setting, so the repository never relaxes a default it is meant to test
    /// </summary>
    [TestMethod]
    public void RootConfigurationSetsNoLimitAtOrAboveTheShippedDefault()
    {
        // Arrange
        var configuration = LoadRootConfiguration();

        // Act
        var violations = configuration.TryGetProperty("design", out var design)
                             ? design.EnumerateObject()
                                     .Where(setting => _shippedDefaultLimits.TryGetValue(setting.Name, out var shippedDefault) == false
                                                       || setting.Value.ValueKind != JsonValueKind.Number
                                                       || setting.Value.TryGetInt32(out var limit) == false
                                                       || limit >= shippedDefault)
                                     .Select(setting => $"{setting.Name}={setting.Value.GetRawText()}")
                                     .ToList()
                             : [];

        // Assert
        Assert.IsEmpty(violations, $"Settings that are no line-count limit below its shipped default: {string.Join(", ", violations)}");
    }

    /// <summary>
    /// Verifies that the repository's build passes its root configuration to the analyzers of every project
    /// </summary>
    [TestMethod]
    public void RepositoryBuildPassesRootConfigurationToTheAnalyzers()
    {
        // Arrange
        var repositoryRoot = RepositoryRoot.Find();

        // Act
        var configurationItems = LoadRootBuildFiles(repositoryRoot).SelectMany(buildFile => buildFile.Document.Descendants(buildFile.Document.Root.Name.Namespace + "AdditionalFiles"))
                                                                   .Select(item => item.Attribute("Include")?.Value ?? string.Empty)
                                                                   .Where(include => include.Replace('\\', '/').EndsWith("/" + ConfigurationFileName, StringComparison.OrdinalIgnoreCase))
                                                                   .ToList();

        // Assert
        Assert.HasCount(1, configurationItems, "The root build files must add the root reihitsu.json as one AdditionalFiles item.");
    }

    /// <summary>
    /// Verifies that the repository's build takes the MSBuild properties the analyzers see from the package's own build
    /// file, so the list of properties is stated once
    /// </summary>
    [TestMethod]
    public void RepositoryBuildImportsThePackageBuildFile()
    {
        // Arrange
        var repositoryRoot = RepositoryRoot.Find();

        // Act
        var imports = LoadRootBuildFiles(repositoryRoot).SelectMany(buildFile => buildFile.Document.Descendants(buildFile.Document.Root.Name.Namespace + "Import"))
                                                        .Select(import => import.Attribute("Project")?.Value ?? string.Empty)
                                                        .Where(project => project.Replace('\\', '/').EndsWith(PackageBuildFilePath, StringComparison.Ordinal))
                                                        .ToList();

        // Assert
        Assert.HasCount(1, imports, $"The root build files must import {PackageBuildFilePath} once.");
    }

    /// <summary>
    /// Verifies that the build gives the analyzers of a test project <c>IsTestProject</c> as <c>true</c>, and makes
    /// <c>IsTestingPlatformApplication</c> visible to them, as MSBuild wrote both into the project's generated analyzer
    /// configuration
    /// </summary>
    [TestMethod]
    public void BuildMarksTestProjectAsTestProjectForTheAnalyzers()
    {
        // Act
        var properties = ReadGeneratedBuildProperties("Reihitsu.SelfHosting.Test");

        // Assert
        Assert.AreEqual("true", properties.GetValueOrDefault("build_property.IsTestProject"), "build_property.IsTestProject");
        Assert.IsTrue(properties.ContainsKey("build_property.IsTestingPlatformApplication"), "build_property.IsTestingPlatformApplication is not visible to the analyzers.");
    }

    /// <summary>
    /// Verifies that the build makes both properties visible to the analyzers of a production project, and neither is
    /// <c>true</c> there
    /// </summary>
    [TestMethod]
    public void BuildDoesNotMarkProductionProjectAsTestProjectForTheAnalyzers()
    {
        // Act
        var properties = ReadGeneratedBuildProperties("Reihitsu.Analyzer");

        // Assert
        Assert.IsTrue(properties.ContainsKey("build_property.IsTestProject"), "build_property.IsTestProject is not visible to the analyzers.");
        Assert.IsTrue(properties.ContainsKey("build_property.IsTestingPlatformApplication"), "build_property.IsTestingPlatformApplication is not visible to the analyzers.");
        Assert.AreNotEqual("true", properties["build_property.IsTestProject"], true, "build_property.IsTestProject");
        Assert.AreNotEqual("true", properties["build_property.IsTestingPlatformApplication"], true, "build_property.IsTestingPlatformApplication");
    }

    #endregion // Tests

    #region Methods

    /// <summary>
    /// Loads the repository's root configuration file
    /// </summary>
    /// <returns>The root element of the parsed configuration</returns>
    private static JsonElement LoadRootConfiguration()
    {
        var path = Path.Combine(RepositoryRoot.Find(), ConfigurationFileName);

        Assert.IsTrue(File.Exists(path), $"The repository has no root {ConfigurationFileName}.");

        using (var document = JsonDocument.Parse(File.ReadAllText(path)))
        {
            return document.RootElement.Clone();
        }
    }

    /// <summary>
    /// Loads the build files MSBuild imports into every project from the repository root
    /// </summary>
    /// <param name="repositoryRoot">Repository root</param>
    /// <returns>The build files that exist, with their paths</returns>
    private static List<(string Path, XDocument Document)> LoadRootBuildFiles(string repositoryRoot)
    {
        return new[] { "Directory.Build.props", "Directory.Build.targets" }.Select(name => Path.Combine(repositoryRoot, name))
                                                                           .Where(File.Exists)
                                                                           .Select(path => (path, XDocument.Load(path)))
                                                                           .ToList();
    }

    /// <summary>
    /// Reads the build properties MSBuild wrote into a project's generated analyzer configuration for the configuration
    /// this test assembly was built in, which builds the project as well
    /// </summary>
    /// <param name="projectName">Name of a project this test project references, or of this test project</param>
    /// <returns>The <c>build_property.*</c> entries, by key; for a key written twice, the last value</returns>
    private static Dictionary<string, string> ReadGeneratedBuildProperties(string projectName)
    {
        var configuration = new DirectoryInfo(AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar)).Parent?.Name;
        var intermediateDirectory = Path.Combine(RepositoryRoot.Find(), "src", projectName, "obj", configuration ?? string.Empty);
        var files = Directory.Exists(intermediateDirectory)
                        ? Directory.GetFiles(intermediateDirectory, $"{projectName}.GeneratedMSBuildEditorConfig.editorconfig", SearchOption.AllDirectories)
                        : [];

        Assert.HasCount(1, files, $"Expected one generated analyzer configuration of {projectName} under {intermediateDirectory}.");

        var properties = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var line in File.ReadLines(files[0]))
        {
            var separator = line.IndexOf('=', StringComparison.Ordinal);

            if (line.StartsWith("build_property.", StringComparison.Ordinal) && separator > 0)
            {
                properties[line[..separator].Trim()] = line[(separator + 1)..].Trim();
            }
        }

        return properties;
    }

    #endregion // Methods
}