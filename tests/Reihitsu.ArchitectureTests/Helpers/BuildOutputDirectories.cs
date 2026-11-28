using System;
using System.IO;
using System.Linq;

namespace Reihitsu.ArchitectureTests.Helpers;

/// <summary>
/// Decides which files of a scan lie in build output: a directory named <c>bin</c> or <c>obj</c>, in any casing,
/// below the root the scan walks. Directories above that root never count, so a checkout or a temporary fixture
/// that itself sits under such a directory is still scanned. Any further directory a scan skips, such as
/// <c>.git</c>, is that scan's own decision
/// </summary>
internal static class BuildOutputDirectories
{
    #region Fields

    /// <summary>
    /// Both directory separators, so a path written with either one splits into its segments
    /// </summary>
    private static readonly char[] _directorySeparators = [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar];

    #endregion // Fields

    #region Methods

    /// <summary>
    /// Determines whether a directory name names a build output directory
    /// </summary>
    /// <param name="directoryName">Name of one directory, not a path</param>
    /// <returns><see langword="true"/> for <c>bin</c> and <c>obj</c> in any casing; otherwise, <see langword="false"/></returns>
    internal static bool IsBuildOutputDirectoryName(string directoryName)
    {
        return string.Equals(directoryName, "bin", StringComparison.OrdinalIgnoreCase)
               || string.Equals(directoryName, "obj", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Determines whether a file lies in a build output directory below the scanned root
    /// </summary>
    /// <param name="rootDirectory">Root directory the scan walks</param>
    /// <param name="filePath">Path of a file below <paramref name="rootDirectory"/></param>
    /// <returns><see langword="true"/> if a directory between the root and the file is build output; otherwise, <see langword="false"/></returns>
    internal static bool Contains(string rootDirectory, string filePath)
    {
        var segments = Path.GetRelativePath(rootDirectory, filePath).Split(_directorySeparators);

        return segments.Take(segments.Length - 1)
                       .Any(IsBuildOutputDirectoryName);
    }

    #endregion // Methods
}