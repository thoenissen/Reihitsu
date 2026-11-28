using System;
using System.IO;

namespace Reihitsu.ArchitectureTests.Helpers;

/// <summary>
/// Locates the checked-out repository whose files the architecture tests read
/// </summary>
internal static class RepositoryRoot
{
    #region Methods

    /// <summary>
    /// Finds the repository root by walking up from the test output directory to the directory that holds
    /// <c>Reihitsu.sln</c>
    /// </summary>
    /// <returns>Absolute repository-root path</returns>
    internal static string Find()
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