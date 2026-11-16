using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Reihitsu.ArchitectureTests.General;

/// <summary>
/// Reads the Roslyn package references of every project file below a directory, with each version resolved the way
/// the project files state it: a literal <c>Version</c>, a <c>$(Property)</c> defined in the project or in a
/// <c>Directory.Build.props</c> or <c>Directory.Packages.props</c> above it, or a central <c>PackageVersion</c> item
/// </summary>
internal static class RoslynPackageReferences
{
    #region Constants

    /// <summary>
    /// Text a version starts with when the project files do not state it in a form this reader resolves
    /// </summary>
    internal const string UnresolvedVersionPrefix = "<unresolved>";

    /// <summary>
    /// Package identifier every Roslyn package starts with
    /// </summary>
    private const string RoslynPackagePrefix = "Microsoft.CodeAnalysis";

    /// <summary>
    /// Suffix of the Roslyn testing packages, which follow a version line of their own
    /// </summary>
    private const string TestingPackageSuffix = ".Testing";

    #endregion // Constants

    #region Fields

    /// <summary>
    /// Characters that separate the directories of a path
    /// </summary>
    private static readonly char[] _directorySeparators = [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar];

    /// <summary>
    /// Matches an MSBuild property reference such as <c>$(RoslynVersion)</c>
    /// </summary>
    private static readonly Regex _propertyReference = new(@"\$\((?<name>[A-Za-z_][A-Za-z0-9_.\-]*)\)", RegexOptions.CultureInvariant, Regex.InfiniteMatchTimeout);

    #endregion // Fields

    #region Methods

    /// <summary>
    /// Collects the non-testing Roslyn package references of every project file below <paramref name="sourceRoot"/>
    /// </summary>
    /// <param name="sourceRoot">Directory whose project files are read</param>
    /// <param name="repositoryRoot">Outermost directory whose <c>Directory.*.props</c> files contribute properties and central versions</param>
    /// <returns>The project file name, package identifier, and resolved version of every reference</returns>
    internal static List<(string Project, string Package, string Version)> Collect(string sourceRoot, string repositoryRoot)
    {
        var references = new List<(string Project, string Package, string Version)>();

        foreach (var projectPath in Directory.EnumerateFiles(sourceRoot, "*.csproj", SearchOption.AllDirectories)
                                             .Where(path => IsBuildOutput(path) == false)
                                             .OrderBy(path => path, StringComparer.Ordinal))
        {
            var (properties, centralVersions) = ReadInheritedDefinitions(Path.GetDirectoryName(projectPath), repositoryRoot);

            var project = XDocument.Load(projectPath);

            AddProperties(project, properties);

            foreach (var element in project.Descendants().Where(element => element.Name.LocalName == "PackageReference"))
            {
                var package = (string)element.Attribute("Include");

                if (IsRoslynPackage(package) == false)
                {
                    continue;
                }

                var version = (string)element.Attribute("VersionOverride")
                                  ?? (string)element.Attribute("Version")
                                  ?? element.Elements().FirstOrDefault(child => child.Name.LocalName == "Version")?.Value
                                  ?? centralVersions.GetValueOrDefault(package);

                references.Add((Path.GetFileName(projectPath), package, Resolve(version, properties)));
            }
        }

        return references;
    }

    /// <summary>
    /// Describes the versions of the references when they are not all one resolved version
    /// </summary>
    /// <param name="references">References returned by <see cref="Collect"/></param>
    /// <returns>A description of every version and the references naming it, or <see langword="null"/> when all name one resolved version</returns>
    internal static string DescribeMismatch(IReadOnlyCollection<(string Project, string Package, string Version)> references)
    {
        var versions = references.GroupBy(reference => reference.Version, StringComparer.Ordinal)
                                 .OrderBy(group => group.Key, StringComparer.Ordinal)
                                 .ToList();

        if (versions.Count == 1
            && versions[0].Key.StartsWith(UnresolvedVersionPrefix, StringComparison.Ordinal) == false)
        {
            return null;
        }

        return string.Join("; ", versions.Select(group => $"{group.Key}: {string.Join(", ", group.Select(reference => $"{reference.Project} {reference.Package}"))}"));
    }

    /// <summary>
    /// Determines whether a package is a Roslyn package that shares the compiler's version line
    /// </summary>
    /// <param name="package">Package identifier</param>
    /// <returns><see langword="true"/> for <c>Microsoft.CodeAnalysis</c> and <c>Microsoft.CodeAnalysis.*</c> packages other than the testing packages</returns>
    private static bool IsRoslynPackage(string package)
    {
        if (package == null)
        {
            return false;
        }

        var isRoslyn = package.Equals(RoslynPackagePrefix, StringComparison.OrdinalIgnoreCase)
                       || package.StartsWith(RoslynPackagePrefix + ".", StringComparison.OrdinalIgnoreCase);

        return isRoslyn
               && package.EndsWith(TestingPackageSuffix, StringComparison.OrdinalIgnoreCase) == false;
    }

    /// <summary>
    /// Determines whether a path lies in a build output directory
    /// </summary>
    /// <param name="path">Path of a project file</param>
    /// <returns><see langword="true"/> when a directory of the path is named <c>bin</c> or <c>obj</c></returns>
    private static bool IsBuildOutput(string path)
    {
        return path.Split(_directorySeparators)
                   .Any(segment => segment is "bin" or "obj");
    }

    /// <summary>
    /// Reads the properties and central package versions of the <c>Directory.Build.props</c> and
    /// <c>Directory.Packages.props</c> files from <paramref name="repositoryRoot"/> down to <paramref name="projectDirectory"/>
    /// </summary>
    /// <param name="projectDirectory">Directory of the project file</param>
    /// <param name="repositoryRoot">Outermost directory read</param>
    /// <returns>The properties and central versions, the inner definition winning over the outer one</returns>
    private static (Dictionary<string, string> Properties, Dictionary<string, string> CentralVersions) ReadInheritedDefinitions(string projectDirectory, string repositoryRoot)
    {
        var properties = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var centralVersions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var directories = new List<string>();
        var root = Path.GetFullPath(repositoryRoot).TrimEnd(Path.DirectorySeparatorChar);

        for (var current = new DirectoryInfo(projectDirectory); current != null; current = current.Parent)
        {
            directories.Add(current.FullName);

            if (string.Equals(current.FullName.TrimEnd(Path.DirectorySeparatorChar), root, StringComparison.Ordinal))
            {
                break;
            }
        }

        directories.Reverse();

        foreach (var directory in directories)
        {
            foreach (var fileName in new[] { "Directory.Build.props", "Directory.Packages.props" })
            {
                var path = Path.Combine(directory, fileName);

                if (File.Exists(path) == false)
                {
                    continue;
                }

                var document = XDocument.Load(path);

                AddProperties(document, properties);

                foreach (var element in document.Descendants().Where(element => element.Name.LocalName == "PackageVersion"))
                {
                    if ((string)element.Attribute("Include") is { } package
                        && (string)element.Attribute("Version") is { } version)
                    {
                        centralVersions[package] = version;
                    }
                }
            }
        }

        return (properties, centralVersions);
    }

    /// <summary>
    /// Adds the properties a document defines in its property groups
    /// </summary>
    /// <param name="document">Project or props document</param>
    /// <param name="properties">Properties to add to; a later definition replaces an earlier one</param>
    private static void AddProperties(XDocument document, Dictionary<string, string> properties)
    {
        foreach (var group in document.Descendants().Where(element => element.Name.LocalName == "PropertyGroup"))
        {
            foreach (var property in group.Elements())
            {
                properties[property.Name.LocalName] = property.Value;
            }
        }
    }

    /// <summary>
    /// Replaces the property references in a version
    /// </summary>
    /// <param name="version">Version as the project file states it</param>
    /// <param name="properties">Known properties</param>
    /// <returns>The resolved version, or a text starting with <see cref="UnresolvedVersionPrefix"/></returns>
    private static string Resolve(string version, Dictionary<string, string> properties)
    {
        if (string.IsNullOrWhiteSpace(version))
        {
            return UnresolvedVersionPrefix + " (no version)";
        }

        var resolved = version.Trim();

        for (var depth = 0; depth < 10 && _propertyReference.IsMatch(resolved); depth++)
        {
            resolved = _propertyReference.Replace(resolved, match => properties.TryGetValue(match.Groups["name"].Value, out var value) ? value.Trim() : match.Value);
        }

        return _propertyReference.IsMatch(resolved) || resolved.Length == 0
                   ? $"{UnresolvedVersionPrefix} ({version})"
                   : resolved;
    }

    #endregion // Methods
}