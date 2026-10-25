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