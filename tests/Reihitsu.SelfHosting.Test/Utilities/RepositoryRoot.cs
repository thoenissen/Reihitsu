using System;
using System.IO;

namespace Reihitsu.SelfHosting.Test.Utilities;

/// <summary>
/// Locates the repository whose own source code the self-hosting scans inspect
/// </summary>
internal static class RepositoryRoot
{
    #region Methods

    /// <summary>
    /// Finds the solution root directory by walking up from the test assembly's location
    /// </summary>
    /// <returns>The absolute path to the solution root directory</returns>
    internal static string Find()
    {
        var directory = AppContext.BaseDirectory;

        while (directory != null)
        {
            if (File.Exists(Path.Combine(directory, "Reihitsu.sln")))
            {
                return directory;
            }

            directory = Directory.GetParent(directory)?.FullName;
        }

        throw new InvalidOperationException("Could not find solution root (Reihitsu.sln).");
    }

    /// <summary>
    /// Gets the directories that hold the repository's projects: <c>src</c> for the shipped and tooling projects and
    /// <c>tests</c> for the test projects
    /// </summary>
    /// <param name="solutionRoot">The absolute path to the solution root</param>
    /// <returns>The absolute paths of both project directories</returns>
    internal static string[] GetProjectContainerDirectories(string solutionRoot)
    {
        return [Path.Combine(solutionRoot, "src"), Path.Combine(solutionRoot, "tests")];
    }

    /// <summary>
    /// Finds the directory of a repository project by its name, below <c>src</c> or <c>tests</c>
    /// </summary>
    /// <param name="solutionRoot">The absolute path to the solution root</param>
    /// <param name="projectName">Project name</param>
    /// <returns>The absolute path of the project directory</returns>
    internal static string FindProjectDirectory(string solutionRoot, string projectName)
    {
        foreach (var containerDirectory in GetProjectContainerDirectories(solutionRoot))
        {
            var projectDirectory = Path.Combine(containerDirectory, projectName);

            if (Directory.Exists(projectDirectory))
            {
                return projectDirectory;
            }
        }

        throw new DirectoryNotFoundException("Project directory not found: " + projectName);
    }

    /// <summary>
    /// Determines whether a path is within a build output directory
    /// </summary>
    /// <param name="path">Path to inspect</param>
    /// <returns><see langword="true"/> if the path is inside bin or obj; otherwise, <see langword="false"/></returns>
    internal static bool IsInBuildOutputPath(string path)
    {
        return path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
               || path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Determines whether a file is generated source that the self-hosting scans skip
    /// </summary>
    /// <param name="path">Path to inspect</param>
    /// <returns><see langword="true"/> for <c>.Designer.cs</c>, <c>.g.cs</c>, and <c>.g.i.cs</c> files; otherwise, <see langword="false"/></returns>
    internal static bool IsGeneratedSourceFile(string path)
    {
        return path.EndsWith(".Designer.cs", StringComparison.OrdinalIgnoreCase)
               || path.EndsWith(".g.cs", StringComparison.OrdinalIgnoreCase)
               || path.EndsWith(".g.i.cs", StringComparison.OrdinalIgnoreCase);
    }

    #endregion // Methods
}