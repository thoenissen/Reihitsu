using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Reihitsu.SelfHosting.Test.Utilities;

/// <summary>
/// Enumerates the repository source files the formatter self-hosting scan formats, one test row each
/// </summary>
internal static class FormatterSourceCatalog
{
    #region Fields

    /// <summary>
    /// Directories to scan for C# files, relative to the solution root
    /// </summary>
    internal static readonly string[] SourceDirectories = [
                                                              "src/Reihitsu.Core",
                                                              "src/Reihitsu.Analyzer",
                                                              "src/Reihitsu.Analyzer.CodeFixes",
                                                              "src/Reihitsu.Analyzer.Test",
                                                              "src/Reihitsu.Cli",
                                                              "src/Reihitsu.Cli.Test",
                                                              "src/Reihitsu.Formatter",
                                                              "src/Reihitsu.Formatter.Test",
                                                              "src/Reihitsu.SelfHosting.Test"
                                                          ];

    #endregion // Fields

    #region Methods

    /// <summary>
    /// Enumerates the C# source files of the configured directories, excluding generated files and build output
    /// </summary>
    /// <param name="solutionRoot">The absolute path to the solution root</param>
    /// <returns>Solution-relative file paths with forward slashes, in ordinal order</returns>
    internal static IEnumerable<string> EnumerateRelativeSourceFiles(string solutionRoot)
    {
        var files = new List<string>();

        foreach (var directory in SourceDirectories)
        {
            var fullPath = Path.Combine(solutionRoot, directory);

            if (Directory.Exists(fullPath) == false)
            {
                throw new DirectoryNotFoundException("Required directory missing: " + fullPath);
            }

            foreach (var file in Directory.EnumerateFiles(fullPath, "*.cs", SearchOption.AllDirectories))
            {
                var relativePath = Path.GetRelativePath(solutionRoot, file);

                if (RepositoryRoot.IsInBuildOutputPath(relativePath)
                    || RepositoryRoot.IsGeneratedSourceFile(relativePath))
                {
                    continue;
                }

                files.Add(relativePath.Replace(Path.DirectorySeparatorChar, '/'));
            }
        }

        return files.OrderBy(file => file, StringComparer.Ordinal);
    }

    #endregion // Methods
}