using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Xml.Linq;

using Microsoft.CodeAnalysis;

namespace Reihitsu.Analyzer.Test.SelfHosting;

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
    /// <param name="projectReferencePaths">Project reference paths</param>
    private SelfHostingProject(string projectFilePath,
                               string projectDirectoryPath,
                               string projectName,
                               string assemblyName,
                               string targetFramework,
                               OutputKind outputKind,
                               ImmutableArray<string> projectReferencePaths)
    {
        ArgumentNullException.ThrowIfNull(projectFilePath);
        ArgumentNullException.ThrowIfNull(projectDirectoryPath);
        ArgumentNullException.ThrowIfNull(projectName);
        ArgumentNullException.ThrowIfNull(assemblyName);
        ArgumentNullException.ThrowIfNull(targetFramework);

        ProjectFilePath = projectFilePath;
        ProjectDirectoryPath = projectDirectoryPath;
        ProjectName = projectName;
        AssemblyName = assemblyName;
        TargetFramework = targetFramework;
        OutputKind = outputKind;
        ProjectReferencePaths = projectReferencePaths;
    }

    #endregion // Constructor

    #region Properties

    /// <summary>
    /// Gets the absolute path to the project file
    /// </summary>
    internal string ProjectFilePath { get; }

    /// <summary>
    /// Gets the absolute path to the project directory
    /// </summary>
    internal string ProjectDirectoryPath { get; }

    /// <summary>
    /// Gets the project name
    /// </summary>
    internal string ProjectName { get; }

    /// <summary>
    /// Gets the assembly name
    /// </summary>
    internal string AssemblyName { get; }

    /// <summary>
    /// Gets the target framework
    /// </summary>
    internal string TargetFramework { get; }

    /// <summary>
    /// Gets the output kind
    /// </summary>
    internal OutputKind OutputKind { get; }

    /// <summary>
    /// Gets the absolute project reference paths
    /// </summary>
    internal ImmutableArray<string> ProjectReferencePaths { get; }

    /// <summary>
    /// Gets the absolute path to the project's assets file
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
        var projectReferencePaths = projectElement.Descendants(namespaceName + "ProjectReference")
                                                  .Select(projectReference => projectReference.Attribute("Include")?.Value)
                                                  .Where(projectReferencePath => string.IsNullOrWhiteSpace(projectReferencePath) == false)
                                                  .Select(projectReferencePath => Path.GetFullPath(Path.Combine(projectDirectoryPath, projectReferencePath!)))
                                                  .OrderBy(projectReferencePath => projectReferencePath, StringComparer.OrdinalIgnoreCase)
                                                  .ToImmutableArray();

        return new SelfHostingProject(fullProjectFilePath,
                                      projectDirectoryPath,
                                      projectName,
                                      assemblyName,
                                      targetFramework,
                                      outputKind,
                                      projectReferencePaths);
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

    #endregion // Methods
}