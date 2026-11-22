using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Xml.Linq;

using Microsoft.CodeAnalysis;

namespace Reihitsu.SelfHosting.Test.Utilities;

/// <summary>
/// Metadata for a project participating in analyzer self-hosting
/// </summary>
internal sealed class SelfHostingProject
{
    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="SelfHostingProject"/> class
    /// </summary>
    /// <param name="projectFilePath">Project file path</param>
    /// <param name="projectDirectoryPath">Project directory path</param>
    /// <param name="projectName">Project name</param>
    /// <param name="assemblyName">Assembly name</param>
    /// <param name="targetFramework">Target framework</param>
    /// <param name="outputKind">Compilation output kind</param>
    /// <param name="sdk">The project's Sdk attribute value</param>
    /// <param name="projectReferencePaths">Project reference paths</param>
    /// <param name="isTestProject">Whether MSBuild marks the project as a test project</param>
    private SelfHostingProject(string projectFilePath,
                               string projectDirectoryPath,
                               string projectName,
                               string assemblyName,
                               string targetFramework,
                               OutputKind outputKind,
                               string sdk,
                               ImmutableArray<string> projectReferencePaths,
                               bool isTestProject)
    {
        ArgumentNullException.ThrowIfNull(projectFilePath);
        ArgumentNullException.ThrowIfNull(projectDirectoryPath);
        ArgumentNullException.ThrowIfNull(projectName);
        ArgumentNullException.ThrowIfNull(assemblyName);
        ArgumentNullException.ThrowIfNull(targetFramework);
        ArgumentNullException.ThrowIfNull(sdk);

        ProjectFilePath = projectFilePath;
        ProjectDirectoryPath = projectDirectoryPath;
        ProjectName = projectName;
        AssemblyName = assemblyName;
        TargetFramework = targetFramework;
        OutputKind = outputKind;
        Sdk = sdk;
        ProjectReferencePaths = projectReferencePaths;
        IsTestProject = isTestProject;
    }

    #endregion // Constructor

    #region Properties

    /// <summary>
    /// Absolute path to the project file
    /// </summary>
    internal string ProjectFilePath { get; }

    /// <summary>
    /// Absolute path to the project directory
    /// </summary>
    internal string ProjectDirectoryPath { get; }

    /// <summary>
    /// Project name
    /// </summary>
    internal string ProjectName { get; }

    /// <summary>
    /// Assembly name
    /// </summary>
    internal string AssemblyName { get; }

    /// <summary>
    /// Target framework
    /// </summary>
    internal string TargetFramework { get; }

    /// <summary>
    /// Output kind
    /// </summary>
    internal OutputKind OutputKind { get; }

    /// <summary>
    /// The project's Sdk attribute value
    /// </summary>
    internal string Sdk { get; }

    /// <summary>
    /// Absolute project reference paths
    /// </summary>
    internal ImmutableArray<string> ProjectReferencePaths { get; }

    /// <summary>
    /// Whether MSBuild sets <c>IsTestProject</c> to <c>true</c> for the project, so the analyzers judge its code as test
    /// code: the project file sets the property itself, or it references <c>Microsoft.NET.Test.Sdk</c>, whose build
    /// files set it
    /// </summary>
    internal bool IsTestProject { get; }

    /// <summary>
    /// Absolute path to the project's assets file
    /// </summary>
    internal string ProjectAssetsPath => Path.Combine(ProjectDirectoryPath, "obj", "project.assets.json");

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Loads project metadata from a csproj file
    /// </summary>
    /// <param name="projectFilePath">Absolute project file path</param>
    /// <returns>The loaded project metadata</returns>
    internal static SelfHostingProject Load(string projectFilePath)
    {
        ArgumentNullException.ThrowIfNull(projectFilePath);

        var fullProjectFilePath = Path.GetFullPath(projectFilePath);
        var projectDirectoryPath = Path.GetDirectoryName(fullProjectFilePath)
                                       ?? throw new InvalidOperationException($"Could not determine the directory for '{projectFilePath}'.");
        var projectDocument = XDocument.Load(fullProjectFilePath);
        var projectElement = projectDocument.Root
                                 ?? throw new InvalidOperationException($"Project file '{projectFilePath}' is empty.");
        var namespaceName = projectElement.Name.Namespace;
        var projectName = Path.GetFileNameWithoutExtension(fullProjectFilePath);
        var assemblyName = GetPropertyValue(projectElement, namespaceName + "AssemblyName") ?? projectName;
        var targetFramework = GetTargetFramework(projectElement, namespaceName, fullProjectFilePath);
        var outputKind = ParseOutputKind(GetPropertyValue(projectElement, namespaceName + "OutputType"));
        var sdk = projectElement.Attribute("Sdk")?.Value ?? string.Empty;
        var projectReferencePaths = projectElement.Descendants(namespaceName + "ProjectReference")
                                                  .Select(projectReference => projectReference.Attribute("Include")?.Value)
                                                  .Where(projectReferencePath => string.IsNullOrWhiteSpace(projectReferencePath) == false)
                                                  .Select(projectReferencePath => GetNormalizedFullPath(projectDirectoryPath, projectReferencePath))
                                                  .OrderBy(projectReferencePath => projectReferencePath, StringComparer.OrdinalIgnoreCase)
                                                  .ToImmutableArray();
        var isTestProject = IsMarkedAsTestProject(projectElement, namespaceName);

        return new SelfHostingProject(fullProjectFilePath,
                                      projectDirectoryPath,
                                      projectName,
                                      assemblyName,
                                      targetFramework,
                                      outputKind,
                                      sdk,
                                      projectReferencePaths,
                                      isTestProject);
    }

    /// <summary>
    /// Determines whether MSBuild marks the project as a test project: the value of an <c>IsTestProject</c> property the
    /// project file sets, otherwise whether it references the package <c>Microsoft.NET.Test.Sdk</c>
    /// </summary>
    /// <param name="projectElement">Project element</param>
    /// <param name="namespaceName">XML namespace</param>
    /// <returns><see langword="true"/> if the project is a test project; otherwise, <see langword="false"/></returns>
    private static bool IsMarkedAsTestProject(XElement projectElement, XNamespace namespaceName)
    {
        var isTestProject = GetPropertyValue(projectElement, namespaceName + "IsTestProject");

        if (isTestProject != null)
        {
            return string.Equals(isTestProject, "true", StringComparison.OrdinalIgnoreCase);
        }

        return projectElement.Descendants(namespaceName + "PackageReference")
                             .Any(packageReference => string.Equals(packageReference.Attribute("Include")?.Value, "Microsoft.NET.Test.Sdk", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Gets a property value from the project file
    /// </summary>
    /// <param name="projectElement">Project element</param>
    /// <param name="propertyName">Property name</param>
    /// <returns>The property value, or <see langword="null"/> when missing</returns>
    private static string GetPropertyValue(XElement projectElement, XName propertyName)
    {
        return projectElement.Descendants(propertyName)
                             .Select(property => property.Value?.Trim())
                             .FirstOrDefault(value => string.IsNullOrWhiteSpace(value) == false);
    }

    /// <summary>
    /// Gets the single target framework declared by the project
    /// </summary>
    /// <param name="projectElement">Project element</param>
    /// <param name="namespaceName">XML namespace</param>
    /// <param name="projectFilePath">Project file path</param>
    /// <returns>The target framework moniker</returns>
    private static string GetTargetFramework(XElement projectElement, XNamespace namespaceName, string projectFilePath)
    {
        var targetFramework = GetPropertyValue(projectElement, namespaceName + "TargetFramework");

        if (string.IsNullOrWhiteSpace(targetFramework) == false)
        {
            return targetFramework;
        }

        var targetFrameworks = GetPropertyValue(projectElement, namespaceName + "TargetFrameworks");

        if (string.IsNullOrWhiteSpace(targetFrameworks))
        {
            throw new InvalidOperationException($"Project '{projectFilePath}' does not declare TargetFramework.");
        }

        var frameworks = targetFrameworks.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (frameworks.Length != 1)
        {
            throw new InvalidOperationException($"Project '{projectFilePath}' declares multiple target frameworks, which self-hosting does not support yet.");
        }

        return frameworks[0];
    }

    /// <summary>
    /// Parses the compilation output kind from the project output type
    /// </summary>
    /// <param name="outputType">Project output type</param>
    /// <returns>The corresponding output kind</returns>
    private static OutputKind ParseOutputKind(string outputType)
    {
        return outputType switch
               {
                   "Exe" => OutputKind.ConsoleApplication,
                   "WinExe" => OutputKind.WindowsApplication,
                   _ => OutputKind.DynamicallyLinkedLibrary
               };
    }

    /// <summary>
    /// Gets a normalized absolute path for a project-relative include path
    /// </summary>
    /// <param name="projectDirectoryPath">Project directory path</param>
    /// <param name="relativePath">Relative include path</param>
    /// <returns>The normalized absolute path</returns>
    private static string GetNormalizedFullPath(string projectDirectoryPath, string relativePath)
    {
        var normalizedRelativePath = relativePath.Replace('\\', Path.DirectorySeparatorChar)
                                                 .Replace('/', Path.DirectorySeparatorChar);

        return Path.GetFullPath(Path.Combine(projectDirectoryPath, normalizedRelativePath));
    }

    #endregion // Methods
}