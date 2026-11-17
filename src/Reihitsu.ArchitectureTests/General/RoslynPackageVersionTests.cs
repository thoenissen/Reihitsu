using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reihitsu.ArchitectureTests.General;

/// <summary>
/// Guards that every project compiles and runs against one Roslyn version, owned by one property: every non-testing
/// <c>Microsoft.CodeAnalysis*</c> package item in the repository's build files states <c>Version="$(RoslynVersion)"</c>,
/// and the root <c>Directory.Build.props</c> defines <c>RoslynVersion</c> once, without a condition
/// </summary>
[TestClass]
public sealed class RoslynPackageVersionTests
{
    #region Constants

    /// <summary>
    /// Root props file that defines the version property once, as the repository does
    /// </summary>
    private const string RootPropsWithVersion = """
                                                <Project>
                                                  <PropertyGroup>
                                                    <RoslynVersion>5.12.0</RoslynVersion>
                                                  </PropertyGroup>
                                                </Project>
                                                """;

    #endregion // Constants

    #region Methods

    /// <summary>
    /// Verifies that the repository's build files name the Roslyn version only through the one property
    /// </summary>
    [TestMethod]
    public void EveryRoslynPackageItemUsesTheRepositoryVersionProperty()
    {
        // Arrange
        var repositoryRoot = FindRepositoryRoot();

        // Act
        var (items, violations) = RoslynVersionOwnership.Check(repositoryRoot);

        // Assert
        var files = items.Select(item => item.File).ToHashSet(StringComparer.Ordinal);

        foreach (var shippedProject in new[] { "src/Reihitsu.Core/Reihitsu.Core.csproj", "src/Reihitsu.Analyzer/Reihitsu.Analyzer.csproj", "src/Reihitsu.Analyzer.CodeFixes/Reihitsu.Analyzer.CodeFixes.csproj", "src/Reihitsu.Formatter/Reihitsu.Formatter.csproj" })
        {
            Assert.Contains(shippedProject, files, $"The guard found no Roslyn package item in {shippedProject}.");
        }

        Assert.Contains("scripts/proof/verify-text-only.cs", files, "The guard found no Roslyn #:package directive in scripts/proof/verify-text-only.cs.");

        Assert.IsEmpty(violations, $"Roslyn versions not owned by {RoslynVersionOwnership.VersionPropertyName}: {string.Join("; ", violations)}");
    }

    /// <summary>
    /// Verifies that items naming the property, testing packages with their own version, and items that only remove a package are accepted
    /// </summary>
    [TestMethod]
    public void ItemsNamingTheVersionPropertyAreAccepted()
    {
        // Arrange
        using (var fixture = new RoslynVersionFixtureDirectory())
        {
            fixture.Write("Directory.Build.props", RootPropsWithVersion);
            fixture.Write("src/A/A.csproj",
                          Project("""
                                  <PackageReference Include="Microsoft.CodeAnalysis" Version="$(RoslynVersion)" />
                                  <PackageReference Include="Microsoft.CodeAnalysis.CSharp.Analyzer.Testing" Version="1.1.4" />
                                  <PackageReference Include="SonarAnalyzer.CSharp" Version="10.36.0.15337" />
                                  """));
            fixture.Write("src/B/B.csproj",
                          Project("""
                                  <PackageReference Include="Microsoft.CodeAnalysis.CSharp" Version="$(RoslynVersion)" />
                                  <PackageReference Update="Microsoft.CodeAnalysis.CSharp" Version="$(RoslynVersion)" />
                                  <PackageReference Remove="Microsoft.CodeAnalysis.CSharp.Workspaces" />
                                  """));

            // Act
            var (items, violations) = RoslynVersionOwnership.Check(fixture.Root);

            // Assert
            Assert.IsEmpty(violations, string.Join("; ", violations));
            Assert.HasCount(3, items);
        }
    }

    /// <summary>
    /// Verifies that a literal version in a project file is reported
    /// </summary>
    [TestMethod]
    public void LiteralVersionInProjectIsReported()
    {
        // Arrange
        using (var fixture = new RoslynVersionFixtureDirectory())
        {
            fixture.Write("Directory.Build.props", RootPropsWithVersion);
            fixture.Write("src/A/A.csproj", Project("""<PackageReference Include="Microsoft.CodeAnalysis.CSharp" Version="$(RoslynVersion)" />"""));
            fixture.Write("src/B/B.csproj", Project("""<PackageReference Include="Microsoft.CodeAnalysis.CSharp.Workspaces" Version="5.12.0" />"""));

            // Act
            var (_, violations) = RoslynVersionOwnership.Check(fixture.Root);

            // Assert
            AssertSingleViolation(violations, "src/B/B.csproj:1: PackageReference Microsoft.CodeAnalysis.CSharp.Workspaces");
        }
    }

    /// <summary>
    /// Verifies that an <c>Update</c> item that changes the version of a Roslyn reference is reported
    /// </summary>
    [TestMethod]
    public void UpdateItemWithLiteralVersionIsReported()
    {
        // Arrange
        using (var fixture = new RoslynVersionFixtureDirectory())
        {
            fixture.Write("Directory.Build.props", RootPropsWithVersion);
            fixture.Write("src/A/A.csproj", Project("""<PackageReference Include="Microsoft.CodeAnalysis.CSharp" Version="$(RoslynVersion)" />"""));
            fixture.Write("src/B/B.csproj",
                          Project("""
                                  <PackageReference Include="Microsoft.CodeAnalysis.CSharp" Version="$(RoslynVersion)" />
                                  <PackageReference Update="Microsoft.CodeAnalysis.CSharp" Version="5.6.0" />
                                  """));

            // Act
            var (_, violations) = RoslynVersionOwnership.Check(fixture.Root);

            // Assert
            AssertSingleViolation(violations, "src/B/B.csproj:2: PackageReference Microsoft.CodeAnalysis.CSharp");
        }
    }

    /// <summary>
    /// Verifies that a Roslyn reference a props file adds with a literal version is reported
    /// </summary>
    [TestMethod]
    public void ReferenceInPropsFileWithLiteralVersionIsReported()
    {
        // Arrange
        using (var fixture = new RoslynVersionFixtureDirectory())
        {
            fixture.Write("Directory.Build.props",
                          """
                          <Project>
                            <PropertyGroup>
                              <RoslynVersion>5.12.0</RoslynVersion>
                            </PropertyGroup>
                            <ItemGroup>
                              <PackageReference Include="Microsoft.CodeAnalysis.Workspaces.Common" Version="5.6.0" />
                            </ItemGroup>
                          </Project>
                          """);
            fixture.Write("src/A/A.csproj", Project("""<PackageReference Include="Microsoft.CodeAnalysis.CSharp" Version="$(RoslynVersion)" />"""));

            // Act
            var (_, violations) = RoslynVersionOwnership.Check(fixture.Root);

            // Assert
            AssertSingleViolation(violations, "Directory.Build.props:6: PackageReference Microsoft.CodeAnalysis.Workspaces.Common");
        }
    }

    /// <summary>
    /// Verifies that a Roslyn reference a targets file adds with a literal version is reported
    /// </summary>
    [TestMethod]
    public void ReferenceInTargetsFileWithLiteralVersionIsReported()
    {
        // Arrange
        using (var fixture = new RoslynVersionFixtureDirectory())
        {
            fixture.Write("Directory.Build.props", RootPropsWithVersion);
            fixture.Write("src/A/build/A.targets", Project("""<PackageReference Include="Microsoft.CodeAnalysis.CSharp" Version="5.6.0" />"""));

            // Act
            var (_, violations) = RoslynVersionOwnership.Check(fixture.Root);

            // Assert
            AssertSingleViolation(violations, "src/A/build/A.targets:1: PackageReference Microsoft.CodeAnalysis.CSharp");
        }
    }

    /// <summary>
    /// Verifies that a central <c>PackageVersion</c> item with a literal version is reported
    /// </summary>
    [TestMethod]
    public void CentralPackageVersionWithLiteralVersionIsReported()
    {
        // Arrange
        using (var fixture = new RoslynVersionFixtureDirectory())
        {
            fixture.Write("Directory.Build.props", RootPropsWithVersion);
            fixture.Write("Directory.Packages.props", Project("""<PackageVersion Include="Microsoft.CodeAnalysis.CSharp" Version="5.6.0" />"""));

            // Act
            var (_, violations) = RoslynVersionOwnership.Check(fixture.Root);

            // Assert
            AssertSingleViolation(violations, "Directory.Packages.props:1: PackageVersion Microsoft.CodeAnalysis.CSharp");
        }
    }

    /// <summary>
    /// Verifies that a <c>VersionOverride</c> attribute next to the property is reported
    /// </summary>
    [TestMethod]
    public void VersionOverrideAttributeIsReported()
    {
        // Arrange
        using (var fixture = new RoslynVersionFixtureDirectory())
        {
            fixture.Write("Directory.Build.props", RootPropsWithVersion);
            fixture.Write("src/A/A.csproj", Project("""<PackageReference Include="Microsoft.CodeAnalysis.CSharp" Version="$(RoslynVersion)" VersionOverride="5.6.0" />"""));

            // Act
            var (_, violations) = RoslynVersionOwnership.Check(fixture.Root);

            // Assert
            AssertSingleViolation(violations, "src/A/A.csproj:1: PackageReference Microsoft.CodeAnalysis.CSharp");
        }
    }

    /// <summary>
    /// Verifies that a version stated as child metadata next to the property is reported
    /// </summary>
    [TestMethod]
    public void VersionMetadataElementIsReported()
    {
        // Arrange
        using (var fixture = new RoslynVersionFixtureDirectory())
        {
            fixture.Write("Directory.Build.props", RootPropsWithVersion);
            fixture.Write("src/A/A.csproj", Project("""<PackageReference Include="Microsoft.CodeAnalysis.CSharp" Version="$(RoslynVersion)"><VersionOverride>5.6.0</VersionOverride></PackageReference>"""));

            // Act
            var (_, violations) = RoslynVersionOwnership.Check(fixture.Root);

            // Assert
            AssertSingleViolation(violations, "src/A/A.csproj:1: PackageReference Microsoft.CodeAnalysis.CSharp");
        }
    }

    /// <summary>
    /// Verifies that a Roslyn package inside an <c>Include</c> list of several packages is checked
    /// </summary>
    [TestMethod]
    public void RoslynPackageInIncludeListIsReported()
    {
        // Arrange
        using (var fixture = new RoslynVersionFixtureDirectory())
        {
            fixture.Write("Directory.Build.props", RootPropsWithVersion);
            fixture.Write("src/A/A.csproj", Project("""<PackageReference Include="SonarAnalyzer.CSharp; Microsoft.CodeAnalysis.CSharp" Version="5.6.0" />"""));

            // Act
            var (_, violations) = RoslynVersionOwnership.Check(fixture.Root);

            // Assert
            AssertSingleViolation(violations, "src/A/A.csproj:1: PackageReference Microsoft.CodeAnalysis.CSharp");
        }
    }

    /// <summary>
    /// Verifies that conditioned definitions of the property, which let projects resolve different versions, are reported
    /// </summary>
    [TestMethod]
    public void ConditionedVersionPropertyIsReported()
    {
        // Arrange
        using (var fixture = new RoslynVersionFixtureDirectory())
        {
            fixture.Write("Directory.Build.props",
                          """
                          <Project>
                            <PropertyGroup>
                              <RoslynVersion Condition="'$(MSBuildProjectName)'=='B'">5.6.0</RoslynVersion>
                              <RoslynVersion Condition="'$(RoslynVersion)'==''">5.12.0</RoslynVersion>
                            </PropertyGroup>
                          </Project>
                          """);
            fixture.Write("src/A/A.csproj", Project("""<PackageReference Include="Microsoft.CodeAnalysis.CSharp" Version="$(RoslynVersion)" />"""));
            fixture.Write("src/B/B.csproj", Project("""<PackageReference Include="Microsoft.CodeAnalysis.CSharp" Version="$(RoslynVersion)" />"""));

            // Act
            var (_, violations) = RoslynVersionOwnership.Check(fixture.Root);

            // Assert
            Assert.Contains("RoslynVersion is defined 2 times: Directory.Build.props:3, Directory.Build.props:4", violations);
            Assert.Contains("Directory.Build.props:3: RoslynVersion is defined under a condition", violations);
            Assert.Contains("Directory.Build.props:4: RoslynVersion is defined under a condition", violations);
            Assert.HasCount(3, violations);
        }
    }

    /// <summary>
    /// Verifies that a single definition inside a conditioned property group is reported
    /// </summary>
    [TestMethod]
    public void VersionPropertyInConditionedPropertyGroupIsReported()
    {
        // Arrange
        using (var fixture = new RoslynVersionFixtureDirectory())
        {
            fixture.Write("Directory.Build.props",
                          """
                          <Project>
                            <PropertyGroup Condition="'$(Configuration)'=='Release'">
                              <RoslynVersion>5.12.0</RoslynVersion>
                            </PropertyGroup>
                          </Project>
                          """);

            // Act
            var (_, violations) = RoslynVersionOwnership.Check(fixture.Root);

            // Assert
            AssertSingleViolation(violations, "Directory.Build.props:3: RoslynVersion is defined under a condition");
        }
    }

    /// <summary>
    /// Verifies that a single definition inside a <c>Choose</c> branch is reported
    /// </summary>
    [TestMethod]
    public void VersionPropertyInChooseBranchIsReported()
    {
        // Arrange
        using (var fixture = new RoslynVersionFixtureDirectory())
        {
            fixture.Write("Directory.Build.props",
                          """
                          <Project>
                            <Choose>
                              <When Condition="'$(Configuration)'=='Release'">
                                <PropertyGroup>
                                  <RoslynVersion>5.12.0</RoslynVersion>
                                </PropertyGroup>
                              </When>
                            </Choose>
                          </Project>
                          """);

            // Act
            var (_, violations) = RoslynVersionOwnership.Check(fixture.Root);

            // Assert
            AssertSingleViolation(violations, "Directory.Build.props:5: RoslynVersion is defined under a condition");
        }
    }

    /// <summary>
    /// Verifies that a definition in a nested props file next to the root one is reported
    /// </summary>
    [TestMethod]
    public void RedefinitionInNestedPropsIsReported()
    {
        // Arrange
        using (var fixture = new RoslynVersionFixtureDirectory())
        {
            fixture.Write("Directory.Build.props", RootPropsWithVersion);
            fixture.Write("src/B/Directory.Build.props", RootPropsWithVersion);
            fixture.Write("src/B/B.csproj", Project("""<PackageReference Include="Microsoft.CodeAnalysis.CSharp" Version="$(RoslynVersion)" />"""));

            // Act
            var (_, violations) = RoslynVersionOwnership.Check(fixture.Root);

            // Assert
            Assert.Contains("RoslynVersion is defined 2 times: Directory.Build.props:3, src/B/Directory.Build.props:3", violations);
            Assert.Contains("src/B/Directory.Build.props:3: RoslynVersion is defined outside the root Directory.Build.props", violations);
            Assert.HasCount(2, violations);
        }
    }

    /// <summary>
    /// Verifies that a single definition outside the root props file is reported
    /// </summary>
    [TestMethod]
    public void VersionPropertyOutsideRootPropsIsReported()
    {
        // Arrange
        using (var fixture = new RoslynVersionFixtureDirectory())
        {
            fixture.Write("src/Directory.Build.props", RootPropsWithVersion);

            // Act
            var (_, violations) = RoslynVersionOwnership.Check(fixture.Root);

            // Assert
            AssertSingleViolation(violations, "src/Directory.Build.props:3: RoslynVersion is defined outside the root Directory.Build.props");
        }
    }

    /// <summary>
    /// Verifies that a missing definition is reported
    /// </summary>
    [TestMethod]
    public void MissingVersionPropertyIsReported()
    {
        // Arrange
        using (var fixture = new RoslynVersionFixtureDirectory())
        {
            fixture.Write("src/A/A.csproj", Project("""<PackageReference Include="Microsoft.CodeAnalysis.CSharp" Version="$(RoslynVersion)" />"""));

            // Act
            var (_, violations) = RoslynVersionOwnership.Check(fixture.Root);

            // Assert
            AssertSingleViolation(violations, "Directory.Build.props: RoslynVersion is not defined");
        }
    }

    /// <summary>
    /// Verifies that a definition that is not a literal version is reported
    /// </summary>
    [TestMethod]
    public void VersionPropertyNamingAnotherPropertyIsReported()
    {
        // Arrange
        using (var fixture = new RoslynVersionFixtureDirectory())
        {
            fixture.Write("Directory.Build.props", """<Project><PropertyGroup><RoslynVersion>$(CompilerVersion)</RoslynVersion></PropertyGroup></Project>""");

            // Act
            var (_, violations) = RoslynVersionOwnership.Check(fixture.Root);

            // Assert
            AssertSingleViolation(violations, "Directory.Build.props:1: RoslynVersion must be a literal version");
        }
    }

    /// <summary>
    /// Verifies that item types and version metadata names in another casing, which MSBuild matches case-insensitively, are read as the item and version they are
    /// </summary>
    [TestMethod]
    public void ItemNamesInAnotherCasingNamingTheVersionPropertyAreAccepted()
    {
        // Arrange
        using (var fixture = new RoslynVersionFixtureDirectory())
        {
            fixture.Write("Directory.Build.props", RootPropsWithVersion);
            fixture.Write("src/A/A.csproj", Project("""<packagereference Include="Microsoft.CodeAnalysis.CSharp" version="$(RoslynVersion)" />"""));
            fixture.Write("Directory.Packages.props", Project("""<PACKAGEVERSION Include="Microsoft.CodeAnalysis.CSharp" VERSION="$(RoslynVersion)" />"""));

            // Act
            var (items, violations) = RoslynVersionOwnership.Check(fixture.Root);

            // Assert
            Assert.IsEmpty(violations, string.Join("; ", violations));
            Assert.HasCount(2, items);
        }
    }

    /// <summary>
    /// Verifies that a <c>PackageReference</c> item whose type is written in another casing is read and its literal version reported
    /// </summary>
    [TestMethod]
    public void PackageReferenceItemTypeInAnotherCasingIsReported()
    {
        // Arrange
        using (var fixture = new RoslynVersionFixtureDirectory())
        {
            fixture.Write("Directory.Build.props", RootPropsWithVersion);
            fixture.Write("src/A/A.csproj", Project("""<packagereference Include="Microsoft.CodeAnalysis.CSharp" Version="5.6.0" />"""));

            // Act
            var (_, violations) = RoslynVersionOwnership.Check(fixture.Root);

            // Assert
            AssertSingleViolation(violations, "src/A/A.csproj:1: packagereference Microsoft.CodeAnalysis.CSharp");
        }
    }

    /// <summary>
    /// Verifies that a <c>PackageVersion</c> item whose type is written in another casing is read and its literal version reported
    /// </summary>
    [TestMethod]
    public void PackageVersionItemTypeInAnotherCasingIsReported()
    {
        // Arrange
        using (var fixture = new RoslynVersionFixtureDirectory())
        {
            fixture.Write("Directory.Build.props", RootPropsWithVersion);
            fixture.Write("Directory.Packages.props", Project("""<packageversion Include="Microsoft.CodeAnalysis.CSharp" Version="5.6.0" />"""));

            // Act
            var (_, violations) = RoslynVersionOwnership.Check(fixture.Root);

            // Assert
            AssertSingleViolation(violations, "Directory.Packages.props:1: packageversion Microsoft.CodeAnalysis.CSharp");
        }
    }

    /// <summary>
    /// Verifies that a version metadata element written in another casing next to the property is reported
    /// </summary>
    [TestMethod]
    public void VersionMetadataElementInAnotherCasingIsReported()
    {
        // Arrange
        using (var fixture = new RoslynVersionFixtureDirectory())
        {
            fixture.Write("Directory.Build.props", RootPropsWithVersion);
            fixture.Write("src/A/A.csproj", Project("""<PackageReference Include="Microsoft.CodeAnalysis.CSharp" Version="$(RoslynVersion)"><version>5.6.0</version></PackageReference>"""));

            // Act
            var (_, violations) = RoslynVersionOwnership.Check(fixture.Root);

            // Assert
            AssertSingleViolation(violations, "src/A/A.csproj:1: PackageReference Microsoft.CodeAnalysis.CSharp");
        }
    }

    /// <summary>
    /// Verifies that a version metadata element written in another casing on an <c>Update</c> item next to the property is reported
    /// </summary>
    [TestMethod]
    public void VersionMetadataElementInAnotherCasingOnUpdateItemIsReported()
    {
        // Arrange
        using (var fixture = new RoslynVersionFixtureDirectory())
        {
            fixture.Write("Directory.Build.props", RootPropsWithVersion);
            fixture.Write("src/A/A.csproj", Project("""<PackageReference Include="Microsoft.CodeAnalysis.CSharp" Version="$(RoslynVersion)" />"""));
            fixture.Write("src/B/B.props", Project("""<PackageReference Update="Microsoft.CodeAnalysis.CSharp" Version="$(RoslynVersion)"><version>5.6.0</version></PackageReference>"""));

            // Act
            var (_, violations) = RoslynVersionOwnership.Check(fixture.Root);

            // Assert
            AssertSingleViolation(violations, "src/B/B.props:1: PackageReference Microsoft.CodeAnalysis.CSharp");
        }
    }

    /// <summary>
    /// Verifies that a <c>VersionOverride</c> attribute written in another casing next to the property is reported
    /// </summary>
    [TestMethod]
    public void VersionOverrideAttributeInAnotherCasingIsReported()
    {
        // Arrange
        using (var fixture = new RoslynVersionFixtureDirectory())
        {
            fixture.Write("Directory.Build.props", RootPropsWithVersion);
            fixture.Write("src/A/A.csproj", Project("""<PackageReference Include="Microsoft.CodeAnalysis.CSharp" Version="$(RoslynVersion)" versionoverride="5.6.0" />"""));

            // Act
            var (_, violations) = RoslynVersionOwnership.Check(fixture.Root);

            // Assert
            AssertSingleViolation(violations, "src/A/A.csproj:1: PackageReference Microsoft.CodeAnalysis.CSharp");
        }
    }

    /// <summary>
    /// Verifies that a second version attribute written in another casing next to the property, which MSBuild reads as the version, is reported
    /// </summary>
    [TestMethod]
    public void SecondVersionAttributeInAnotherCasingIsReported()
    {
        // Arrange
        using (var fixture = new RoslynVersionFixtureDirectory())
        {
            fixture.Write("Directory.Build.props", RootPropsWithVersion);
            fixture.Write("src/A/A.csproj", Project("""<PackageReference Include="Microsoft.CodeAnalysis.CSharp" Version="$(RoslynVersion)" version="5.6.0" />"""));

            // Act
            var (_, violations) = RoslynVersionOwnership.Check(fixture.Root);

            // Assert
            AssertSingleViolation(violations, "src/A/A.csproj:1: PackageReference Microsoft.CodeAnalysis.CSharp");
        }
    }

    /// <summary>
    /// Verifies that a lone version attribute written in another casing that states a literal version is reported
    /// </summary>
    [TestMethod]
    public void LoneVersionAttributeInAnotherCasingWithLiteralIsReported()
    {
        // Arrange
        using (var fixture = new RoslynVersionFixtureDirectory())
        {
            fixture.Write("Directory.Build.props", RootPropsWithVersion);
            fixture.Write("src/A/A.csproj", Project("""<PackageReference Include="Microsoft.CodeAnalysis.CSharp" version="5.6.0" />"""));

            // Act
            var (_, violations) = RoslynVersionOwnership.Check(fixture.Root);

            // Assert
            AssertSingleViolation(violations, "src/A/A.csproj:1: PackageReference Microsoft.CodeAnalysis.CSharp");
        }
    }

    /// <summary>
    /// Verifies that build output and directories whose name starts with a dot are not read
    /// </summary>
    [TestMethod]
    public void BuildOutputAndDotDirectoriesAreSkipped()
    {
        // Arrange
        using (var fixture = new RoslynVersionFixtureDirectory())
        {
            fixture.Write("Directory.Build.props", RootPropsWithVersion);
            fixture.Write("src/A/obj/A.csproj.nuget.g.props", Project("""<PackageReference Include="Microsoft.CodeAnalysis.CSharp" Version="5.6.0" />"""));
            fixture.Write("src/A/bin/Release/A.targets", Project("""<PackageReference Include="Microsoft.CodeAnalysis.CSharp" Version="5.6.0" />"""));
            fixture.Write(".tools/Tool.csproj", Project("""<PackageReference Include="Microsoft.CodeAnalysis.CSharp" Version="5.6.0" />"""));

            // Act
            var (items, violations) = RoslynVersionOwnership.Check(fixture.Root);

            // Assert
            Assert.IsEmpty(violations, string.Join("; ", violations));
            Assert.IsEmpty(items);
        }
    }

    /// <summary>
    /// Verifies that a file-based program whose <c>#:package</c> directive pins a Roslyn package at the repository's Roslyn version is accepted and read
    /// </summary>
    [TestMethod]
    public void PackageDirectiveAtTheRepositoryVersionIsAccepted()
    {
        // Arrange
        using (var fixture = new RoslynVersionFixtureDirectory())
        {
            fixture.Write("Directory.Build.props", RootPropsWithVersion);
            fixture.Write("scripts/proof/Tool.cs", FileBasedProgram("#:package Microsoft.CodeAnalysis.CSharp@5.12.0"));

            // Act
            var (items, violations) = RoslynVersionOwnership.Check(fixture.Root);

            // Assert
            Assert.IsEmpty(violations, string.Join("; ", violations));
            Assert.Contains(("scripts/proof/Tool.cs", "Microsoft.CodeAnalysis.CSharp"), items);
            Assert.HasCount(1, items);
        }
    }

    /// <summary>
    /// Verifies that a file-based program whose <c>#:package</c> directive pins a Roslyn package at another version is reported
    /// </summary>
    [TestMethod]
    public void PackageDirectiveWithAnotherVersionIsReported()
    {
        // Arrange
        using (var fixture = new RoslynVersionFixtureDirectory())
        {
            fixture.Write("Directory.Build.props", RootPropsWithVersion);
            fixture.Write("scripts/proof/Tool.cs", FileBasedProgram("#:package Microsoft.CodeAnalysis.CSharp@5.6.0"));

            // Act
            var (_, violations) = RoslynVersionOwnership.Check(fixture.Root);

            // Assert
            AssertSingleViolation(violations, "scripts/proof/Tool.cs:1: #:package Microsoft.CodeAnalysis.CSharp");
        }
    }

    /// <summary>
    /// Verifies that the pin is compared with the version the root props file defines, not with a fixed version
    /// </summary>
    [TestMethod]
    public void PackageDirectiveIsComparedWithTheDefinedVersion()
    {
        // Arrange
        using (var fixture = new RoslynVersionFixtureDirectory())
        {
            fixture.Write("Directory.Build.props",
                          """
                          <Project>
                            <PropertyGroup>
                              <RoslynVersion>5.6.0</RoslynVersion>
                            </PropertyGroup>
                          </Project>
                          """);
            fixture.Write("scripts/A.cs", FileBasedProgram("#:package Microsoft.CodeAnalysis.CSharp@5.6.0"));
            fixture.Write("scripts/B.cs", FileBasedProgram("#:package Microsoft.CodeAnalysis.CSharp@5.12.0"));

            // Act
            var (_, violations) = RoslynVersionOwnership.Check(fixture.Root);

            // Assert
            AssertSingleViolation(violations, "scripts/B.cs:1: #:package Microsoft.CodeAnalysis.CSharp");
        }
    }

    /// <summary>
    /// Verifies that a pin with a floating version, which does not state the repository's version, is reported
    /// </summary>
    [TestMethod]
    public void PackageDirectiveWithFloatingVersionIsReported()
    {
        // Arrange
        using (var fixture = new RoslynVersionFixtureDirectory())
        {
            fixture.Write("Directory.Build.props", RootPropsWithVersion);
            fixture.Write("scripts/Tool.cs", FileBasedProgram("#:package Microsoft.CodeAnalysis.CSharp@5.*"));

            // Act
            var (_, violations) = RoslynVersionOwnership.Check(fixture.Root);

            // Assert
            AssertSingleViolation(violations, "scripts/Tool.cs:1: #:package Microsoft.CodeAnalysis.CSharp");
        }
    }

    /// <summary>
    /// Verifies that a pin whose version only starts like the repository's version is reported
    /// </summary>
    [TestMethod]
    public void PackageDirectiveWithLongerVersionIsReported()
    {
        // Arrange
        using (var fixture = new RoslynVersionFixtureDirectory())
        {
            fixture.Write("Directory.Build.props", RootPropsWithVersion);
            fixture.Write("scripts/Tool.cs", FileBasedProgram("#:package Microsoft.CodeAnalysis.CSharp@5.12.0-beta1"));

            // Act
            var (_, violations) = RoslynVersionOwnership.Check(fixture.Root);

            // Assert
            AssertSingleViolation(violations, "scripts/Tool.cs:1: #:package Microsoft.CodeAnalysis.CSharp");
        }
    }

    /// <summary>
    /// Verifies that every Roslyn package of a program is checked, and that the reported line is the directive's line behind a shebang and other directives
    /// </summary>
    [TestMethod]
    public void EveryPackageDirectiveOfAProgramIsCheckedOnItsLine()
    {
        // Arrange
        using (var fixture = new RoslynVersionFixtureDirectory())
        {
            fixture.Write("Directory.Build.props", RootPropsWithVersion);
            fixture.Write("scripts/Tool.cs",
                          FileBasedProgram("""
                                           #!/usr/bin/env dotnet
                                           #:sdk Microsoft.NET.Sdk
                                           #:property LangVersion=preview
                                           #:package Microsoft.CodeAnalysis.CSharp@5.12.0
                                           #:package Microsoft.CodeAnalysis.Workspaces.Common@5.6.0
                                           """));

            // Act
            var (items, violations) = RoslynVersionOwnership.Check(fixture.Root);

            // Assert
            AssertSingleViolation(violations, "scripts/Tool.cs:5: #:package Microsoft.CodeAnalysis.Workspaces.Common");
            Assert.HasCount(2, items);
        }
    }

    /// <summary>
    /// Verifies that a pin whose package identifier is written in another casing, which NuGet matches case-insensitively, is read and its version reported
    /// </summary>
    [TestMethod]
    public void PackageDirectiveInAnotherCasingIsReported()
    {
        // Arrange
        using (var fixture = new RoslynVersionFixtureDirectory())
        {
            fixture.Write("Directory.Build.props", RootPropsWithVersion);
            fixture.Write("scripts/Tool.cs", FileBasedProgram("#:package microsoft.codeanalysis.csharp@5.6.0"));

            // Act
            var (_, violations) = RoslynVersionOwnership.Check(fixture.Root);

            // Assert
            AssertSingleViolation(violations, "scripts/Tool.cs:1: #:package microsoft.codeanalysis.csharp");
        }
    }

    /// <summary>
    /// Verifies that pins of the Roslyn testing packages and of packages outside Roslyn keep their own versions
    /// </summary>
    [TestMethod]
    public void PackageDirectivesOutsideTheRoslynVersionLineAreAccepted()
    {
        // Arrange
        using (var fixture = new RoslynVersionFixtureDirectory())
        {
            fixture.Write("Directory.Build.props", RootPropsWithVersion);
            fixture.Write("scripts/Tool.cs",
                          FileBasedProgram("""
                                           #:package Microsoft.CodeAnalysis.CSharp.Analyzer.Testing@1.1.4
                                           #:package Microsoft.CodeAnalysisExtras@1.0.0
                                           #:package SonarAnalyzer.CSharp@10.36.0.15337
                                           """));

            // Act
            var (items, violations) = RoslynVersionOwnership.Check(fixture.Root);

            // Assert
            Assert.IsEmpty(violations, string.Join("; ", violations));
            Assert.IsEmpty(items);
        }
    }

    /// <summary>
    /// Verifies that text that looks like a <c>#:package</c> directive after the first token of a file, such as a line of a string literal, is not read as a directive
    /// </summary>
    [TestMethod]
    public void PackageDirectiveTextAfterTheFirstTokenIsNotRead()
    {
        // Arrange
        using (var fixture = new RoslynVersionFixtureDirectory())
        {
            fixture.Write("Directory.Build.props", RootPropsWithVersion);
            fixture.Write("src/A/Fixture.cs",
                          """"
                          namespace A;

                          internal static class Fixture
                          {
                              internal const string Text = """
                          #:package Microsoft.CodeAnalysis.CSharp@5.6.0
                          """;
                          }
                          """");

            // Act
            var (items, violations) = RoslynVersionOwnership.Check(fixture.Root);

            // Assert
            Assert.IsEmpty(violations, string.Join("; ", violations));
            Assert.IsEmpty(items);
        }
    }

    /// <summary>
    /// Verifies that file-based programs in build output and in directories whose name starts with a dot are not read
    /// </summary>
    [TestMethod]
    public void PackageDirectivesInBuildOutputAndDotDirectoriesAreSkipped()
    {
        // Arrange
        using (var fixture = new RoslynVersionFixtureDirectory())
        {
            fixture.Write("Directory.Build.props", RootPropsWithVersion);
            fixture.Write("scripts/obj/Tool.cs", FileBasedProgram("#:package Microsoft.CodeAnalysis.CSharp@5.6.0"));
            fixture.Write("scripts/bin/Release/Tool.cs", FileBasedProgram("#:package Microsoft.CodeAnalysis.CSharp@5.6.0"));
            fixture.Write(".tools/Tool.cs", FileBasedProgram("#:package Microsoft.CodeAnalysis.CSharp@5.6.0"));

            // Act
            var (items, violations) = RoslynVersionOwnership.Check(fixture.Root);

            // Assert
            Assert.IsEmpty(violations, string.Join("; ", violations));
            Assert.IsEmpty(items);
        }
    }

    /// <summary>
    /// Asserts that exactly one violation is reported and that it starts with the expected text
    /// </summary>
    /// <param name="violations">Reported violations</param>
    /// <param name="expectedStart">Expected start of the one violation</param>
    private static void AssertSingleViolation(List<string> violations, string expectedStart)
    {
        Assert.HasCount(1, violations, string.Join("; ", violations));
        Assert.StartsWith(expectedStart, violations[0]);
    }

    /// <summary>
    /// Wraps items in a minimal project
    /// </summary>
    /// <param name="items">Item elements</param>
    /// <returns>The project file text</returns>
    private static string Project(string items)
    {
        return $"<Project><ItemGroup>{items}</ItemGroup></Project>";
    }

    /// <summary>
    /// Builds a minimal file-based program that starts with the given directives
    /// </summary>
    /// <param name="directives">Directive lines at the start of the file</param>
    /// <returns>The program text</returns>
    private static string FileBasedProgram(string directives)
    {
        return $"{directives}\n\nSystem.Console.WriteLine();\n";
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