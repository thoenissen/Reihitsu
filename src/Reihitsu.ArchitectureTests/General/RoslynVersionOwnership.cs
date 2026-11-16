using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace Reihitsu.ArchitectureTests.General;

/// <summary>
/// Checks the repository's one owner of the Roslyn version by reading the build files as written: every non-testing
/// <c>Microsoft.CodeAnalysis*</c> <c>PackageReference</c> or <c>PackageVersion</c> item that names a package through
/// <c>Include</c> or <c>Update</c> states exactly <c>Version="$(RoslynVersion)"</c>, and <c>RoslynVersion</c> is defined
/// exactly once, without a condition, in the root <c>Directory.Build.props</c>. It does not evaluate MSBuild: any
/// other literal form it reads is reported, whatever version MSBuild would compute for it, but deliberate MSBuild
/// indirection is not seen, such as a package identifier taken from a property or an item, a version set inside a
/// target, an import of a file with another extension, or a property from a response file. Every element and
/// attribute name is compared case-insensitively, as MSBuild compares item types, metadata names, and property names
/// </summary>
internal static class RoslynVersionOwnership
{
    #region Constants

    /// <summary>
    /// Name of the property that owns the Roslyn version
    /// </summary>
    internal const string VersionPropertyName = "RoslynVersion";

    /// <summary>
    /// The only version text a Roslyn package item may state
    /// </summary>
    internal const string VersionPropertyReference = "$(" + VersionPropertyName + ")";

    /// <summary>
    /// Name of the props file at the repository root that defines the property
    /// </summary>
    internal const string RootPropsFileName = "Directory.Build.props";

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
    /// Extensions of the build files that are read
    /// </summary>
    private static readonly string[] _buildFileExtensions = [".csproj", ".props", ".targets"];

    /// <summary>
    /// Item types that state a package version
    /// </summary>
    private static readonly string[] _packageItemTypes = ["PackageReference", "PackageVersion"];

    /// <summary>
    /// Characters that separate the directories of a path
    /// </summary>
    private static readonly char[] _directorySeparators = [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar];

    #endregion // Fields

    #region Methods

    /// <summary>
    /// Checks every build file below <paramref name="repositoryRoot"/>, skipping build output and directories whose name starts with a dot
    /// </summary>
    /// <param name="repositoryRoot">Repository root; its <c>Directory.Build.props</c> is the only place the property may be defined</param>
    /// <returns>The relative path and package of every Roslyn package item read, and one message per violation</returns>
    internal static (List<(string File, string Package)> Items, List<string> Violations) Check(string repositoryRoot)
    {
        var items = new List<(string File, string Package)>();
        var violations = new List<string>();
        var definitions = new List<(string File, XElement Property)>();
        var rootProps = Path.GetFullPath(Path.Combine(repositoryRoot, RootPropsFileName));

        foreach (var path in Directory.EnumerateFiles(repositoryRoot, "*", SearchOption.AllDirectories)
                                      .Where(path => _buildFileExtensions.Contains(Path.GetExtension(path), StringComparer.OrdinalIgnoreCase))
                                      .Where(path => IsSkipped(Path.GetRelativePath(repositoryRoot, path)) == false)
                                      .OrderBy(path => path, StringComparer.Ordinal))
        {
            var file = Path.GetRelativePath(repositoryRoot, path).Replace(Path.DirectorySeparatorChar, '/');
            var document = XDocument.Load(path, LoadOptions.SetLineInfo);

            foreach (var element in document.Descendants())
            {
                if (_packageItemTypes.Any(itemType => IsNamed(element, itemType)))
                {
                    CheckPackageItem(file, element, items, violations);
                }
                else if (IsNamed(element, VersionPropertyName)
                         && element.Parent != null
                         && IsNamed(element.Parent, "PropertyGroup"))
                {
                    definitions.Add((string.Equals(Path.GetFullPath(path), rootProps, StringComparison.Ordinal) ? null : file, element));
                }
            }
        }

        CheckDefinitions(definitions, violations);

        return (items, violations);
    }

    /// <summary>
    /// Checks one package item and records it when it names a Roslyn package
    /// </summary>
    /// <param name="file">Relative path of the build file</param>
    /// <param name="element">The item element</param>
    /// <param name="items">Roslyn package items read so far</param>
    /// <param name="violations">Violations found so far</param>
    private static void CheckPackageItem(string file, XElement element, List<(string File, string Package)> items, List<string> violations)
    {
        var identity = (string)FindAttributes(element, "Include").FirstOrDefault() ?? (string)FindAttributes(element, "Update").FirstOrDefault();

        if (identity == null)
        {
            return;
        }

        foreach (var package in identity.Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                                        .Where(IsRoslynPackage))
        {
            items.Add((file, package));

            var versions = FindAttributes(element, "Version").ToList();
            var statesOtherVersion = FindAttributes(element, "VersionOverride").Any()
                                     || element.Elements().Any(child => IsNamed(child, "Version") || IsNamed(child, "VersionOverride"));

            if (versions.Count != 1
                || string.Equals(versions[0].Value, VersionPropertyReference, StringComparison.Ordinal) == false
                || statesOtherVersion)
            {
                violations.Add($"{Location(file, element)}: {element.Name.LocalName} {package} must state exactly Version=\"{VersionPropertyReference}\" and nothing else as its version");
            }
        }
    }

    /// <summary>
    /// Checks that the property is defined exactly once, without a condition, in the root props file
    /// </summary>
    /// <param name="definitions">Every definition found; the file is <see langword="null"/> for the root props file</param>
    /// <param name="violations">Violations found so far</param>
    private static void CheckDefinitions(List<(string File, XElement Property)> definitions, List<string> violations)
    {
        if (definitions.Count == 0)
        {
            violations.Add($"{RootPropsFileName}: {VersionPropertyName} is not defined");

            return;
        }

        if (definitions.Count > 1)
        {
            violations.Add($"{VersionPropertyName} is defined {definitions.Count} times: {string.Join(", ", definitions.Select(definition => Location(definition.File ?? RootPropsFileName, definition.Property)))}");
        }

        foreach (var (file, property) in definitions)
        {
            if (file != null)
            {
                violations.Add($"{Location(file, property)}: {VersionPropertyName} is defined outside the root {RootPropsFileName}");
            }

            if (FindAttributes(property, "Condition").Any()
                || FindAttributes(property.Parent, "Condition").Any()
                || property.Parent.Parent != property.Document.Root)
            {
                violations.Add($"{Location(file ?? RootPropsFileName, property)}: {VersionPropertyName} is defined under a condition");
            }

            if (string.IsNullOrWhiteSpace(property.Value)
                || property.Value.Contains("$(", StringComparison.Ordinal))
            {
                violations.Add($"{Location(file ?? RootPropsFileName, property)}: {VersionPropertyName} must be a literal version");
            }
        }
    }

    /// <summary>
    /// Determines whether a package is a Roslyn package that shares the compiler's version line
    /// </summary>
    /// <param name="package">Package identifier</param>
    /// <returns><see langword="true"/> for <c>Microsoft.CodeAnalysis</c> and <c>Microsoft.CodeAnalysis.*</c> packages other than the testing packages</returns>
    private static bool IsRoslynPackage(string package)
    {
        var isRoslyn = package.Equals(RoslynPackagePrefix, StringComparison.OrdinalIgnoreCase)
                       || package.StartsWith(RoslynPackagePrefix + ".", StringComparison.OrdinalIgnoreCase);

        return isRoslyn
               && package.EndsWith(TestingPackageSuffix, StringComparison.OrdinalIgnoreCase) == false;
    }

    /// <summary>
    /// Determines whether a path lies in build output or in a directory whose name starts with a dot
    /// </summary>
    /// <param name="relativePath">Path relative to the repository root</param>
    /// <returns><see langword="true"/> when a directory of the path is <c>bin</c>, <c>obj</c>, or starts with a dot</returns>
    private static bool IsSkipped(string relativePath)
    {
        var segments = relativePath.Split(_directorySeparators);

        return segments.Take(segments.Length - 1)
                       .Any(segment => segment is "bin" or "obj" || segment.StartsWith('.'));
    }

    /// <summary>
    /// Determines whether an element has the given MSBuild name, compared case-insensitively
    /// </summary>
    /// <param name="element">The element</param>
    /// <param name="name">MSBuild name of an item type, metadata, property, or group</param>
    /// <returns><see langword="true"/> when the local name of the element equals <paramref name="name"/> in any casing</returns>
    private static bool IsNamed(XElement element, string name)
    {
        return string.Equals(element.Name.LocalName, name, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Finds the attributes of an element that have the given MSBuild name, compared case-insensitively
    /// </summary>
    /// <param name="element">The element</param>
    /// <param name="name">MSBuild name of the attribute</param>
    /// <returns>Every attribute whose local name equals <paramref name="name"/> in any casing</returns>
    private static IEnumerable<XAttribute> FindAttributes(XElement element, string name)
    {
        return element.Attributes().Where(attribute => string.Equals(attribute.Name.LocalName, name, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Formats the location of an element
    /// </summary>
    /// <param name="file">Relative path of the build file</param>
    /// <param name="element">The element</param>
    /// <returns>The path and line of the element</returns>
    private static string Location(string file, XElement element)
    {
        return $"{file}:{((IXmlLineInfo)element).LineNumber}";
    }

    #endregion // Methods
}