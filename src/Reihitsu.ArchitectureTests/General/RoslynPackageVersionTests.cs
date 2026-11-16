using System;
using System.IO;
using System.Linq;
using System.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reihitsu.ArchitectureTests.General;

/// <summary>
/// Guards that every project compiles and runs against one Roslyn version: every non-testing
/// <c>Microsoft.CodeAnalysis*</c> package reference under <c>src/</c> resolves to the same version
/// </summary>
[TestClass]
public sealed class RoslynPackageVersionTests
{
    #region Methods

    /// <summary>
    /// Verifies that every non-testing Roslyn package reference of the repository's projects resolves to one version
    /// </summary>
    [TestMethod]
    public void EveryProjectReferencesOneRoslynVersion()
    {
        // Arrange
        var repositoryRoot = FindRepositoryRoot();

        // Act
        var references = RoslynPackageReferences.Collect(Path.Combine(repositoryRoot, "src"), repositoryRoot);

        // Assert
        var projects = references.Select(reference => reference.Project).ToHashSet(StringComparer.Ordinal);

        foreach (var shippedProject in new[] { "Reihitsu.Core.csproj", "Reihitsu.Analyzer.csproj", "Reihitsu.Analyzer.CodeFixes.csproj", "Reihitsu.Formatter.csproj" })
        {
            Assert.Contains(shippedProject, projects, $"The guard found no Roslyn reference in {shippedProject}.");
        }

        var mismatch = RoslynPackageReferences.DescribeMismatch(references);

        Assert.IsNull(mismatch, $"The projects reference different Roslyn versions: {mismatch}");
    }

    /// <summary>
    /// Verifies that two projects naming different literal Roslyn versions are reported as a mismatch
    /// </summary>
    [TestMethod]
    public void DifferentLiteralVersionsAreReportedAsMismatch()
    {
        // Arrange
        using (var fixture = new RoslynVersionFixtureDirectory())
        {
            fixture.Write("src/A/A.csproj", Project("""<PackageReference Include="Microsoft.CodeAnalysis.CSharp" Version="5.12.0" />"""));
            fixture.Write("src/B/B.csproj", Project("""<PackageReference Include="Microsoft.CodeAnalysis.CSharp.Workspaces" Version="5.6.0" />"""));

            // Act
            var mismatch = RoslynPackageReferences.DescribeMismatch(RoslynPackageReferences.Collect(fixture.Source, fixture.Root));

            // Assert
            Assert.IsNotNull(mismatch);
            Assert.Contains("5.12.0: A.csproj Microsoft.CodeAnalysis.CSharp", mismatch);
            Assert.Contains("5.6.0: B.csproj Microsoft.CodeAnalysis.CSharp.Workspaces", mismatch);
        }
    }

    /// <summary>
    /// Verifies that projects naming the same literal Roslyn version are not reported
    /// </summary>
    [TestMethod]
    public void SameLiteralVersionsAreNotReported()
    {
        // Arrange
        using (var fixture = new RoslynVersionFixtureDirectory())
        {
            fixture.Write("src/A/A.csproj", Project("""<PackageReference Include="Microsoft.CodeAnalysis" Version="5.12.0" />"""));
            fixture.Write("src/B/B.csproj", Project("""<PackageReference Include="Microsoft.CodeAnalysis.CSharp.Workspaces" Version="5.12.0" />"""));

            // Act
            var mismatch = RoslynPackageReferences.DescribeMismatch(RoslynPackageReferences.Collect(fixture.Source, fixture.Root));

            // Assert
            Assert.IsNull(mismatch);
        }
    }

    /// <summary>
    /// Verifies that a version named through a property of a <c>Directory.Build.props</c> resolves to the property's value
    /// </summary>
    [TestMethod]
    public void VersionPropertyFromDirectoryBuildPropsIsResolved()
    {
        // Arrange
        using (var fixture = new RoslynVersionFixtureDirectory())
        {
            fixture.Write("Directory.Build.props", """<Project><PropertyGroup><RoslynVersion>5.12.0</RoslynVersion></PropertyGroup></Project>""");
            fixture.Write("src/A/A.csproj", Project("""<PackageReference Include="Microsoft.CodeAnalysis.CSharp" Version="$(RoslynVersion)" />"""));
            fixture.Write("src/B/B.csproj", Project("""<PackageReference Include="Microsoft.CodeAnalysis.CSharp" Version="5.6.0" />"""));

            // Act
            var references = RoslynPackageReferences.Collect(fixture.Source, fixture.Root);

            // Assert
            Assert.Contains(("A.csproj", "Microsoft.CodeAnalysis.CSharp", "5.12.0"), references);
            Assert.IsNotNull(RoslynPackageReferences.DescribeMismatch(references));
        }
    }

    /// <summary>
    /// Verifies that a reference without a version resolves to its central <c>PackageVersion</c> in a <c>Directory.Packages.props</c>
    /// </summary>
    [TestMethod]
    public void CentralPackageVersionIsResolved()
    {
        // Arrange
        using (var fixture = new RoslynVersionFixtureDirectory())
        {
            fixture.Write("Directory.Packages.props", """<Project><ItemGroup><PackageVersion Include="Microsoft.CodeAnalysis.CSharp" Version="5.12.0" /></ItemGroup></Project>""");
            fixture.Write("src/A/A.csproj", Project("""<PackageReference Include="Microsoft.CodeAnalysis.CSharp" />"""));
            fixture.Write("src/B/B.csproj", Project("""<PackageReference Include="Microsoft.CodeAnalysis.CSharp" VersionOverride="5.6.0" />"""));

            // Act
            var references = RoslynPackageReferences.Collect(fixture.Source, fixture.Root);

            // Assert
            Assert.Contains(("A.csproj", "Microsoft.CodeAnalysis.CSharp", "5.12.0"), references);
            Assert.Contains(("B.csproj", "Microsoft.CodeAnalysis.CSharp", "5.6.0"), references);
            Assert.IsNotNull(RoslynPackageReferences.DescribeMismatch(references));
        }
    }

    /// <summary>
    /// Verifies that the Roslyn testing packages, which follow their own version line, are not collected
    /// </summary>
    [TestMethod]
    public void TestingPackagesAreNotCollected()
    {
        // Arrange
        using (var fixture = new RoslynVersionFixtureDirectory())
        {
            fixture.Write("src/A/A.csproj",
                          Project("""
                                  <PackageReference Include="Microsoft.CodeAnalysis" Version="5.12.0" />
                                  <PackageReference Include="Microsoft.CodeAnalysis.CSharp.Analyzer.Testing" Version="1.1.4" />
                                  """));

            // Act
            var references = RoslynPackageReferences.Collect(fixture.Source, fixture.Root);

            // Assert
            Assert.HasCount(1, references);
            Assert.IsNull(RoslynPackageReferences.DescribeMismatch(references));
        }
    }

    /// <summary>
    /// Verifies that a version the reader cannot resolve is reported, so the guard fails instead of passing unchecked
    /// </summary>
    [TestMethod]
    public void UnresolvableVersionIsReported()
    {
        // Arrange
        using (var fixture = new RoslynVersionFixtureDirectory())
        {
            fixture.Write("src/A/A.csproj", Project("""<PackageReference Include="Microsoft.CodeAnalysis.CSharp" Version="$(UndefinedRoslynVersion)" />"""));

            // Act
            var mismatch = RoslynPackageReferences.DescribeMismatch(RoslynPackageReferences.Collect(fixture.Source, fixture.Root));

            // Assert
            Assert.IsNotNull(mismatch);
            Assert.Contains(RoslynPackageReferences.UnresolvedVersionPrefix, mismatch);
        }
    }

    /// <summary>
    /// Wraps package references in a minimal SDK project
    /// </summary>
    /// <param name="packageReferences">Package reference elements</param>
    /// <returns>The project file text</returns>
    private static string Project(string packageReferences)
    {
        return $"<Project Sdk=\"Microsoft.NET.Sdk\"><ItemGroup>{packageReferences}</ItemGroup></Project>";
    }

    /// <summary>
    /// Locates the repository root by walking up from the test assembly
    /// </summary>
    /// <returns>The repository root directory</returns>
    private static string FindRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);

        while (current != null)
        {
            if (File.Exists(Path.Combine(current.FullName, "Reihitsu.sln")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate the Reihitsu repository root.");
    }

    #endregion // Methods
}