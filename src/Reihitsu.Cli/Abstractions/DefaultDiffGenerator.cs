namespace Reihitsu.Cli.Abstractions;

/// <summary>
/// Default diff generator implementation that delegates to <see cref="DiffGenerator"/> and names the file in the diff header
/// by where it lies relative to the process's current directory (CLI-024)
/// </summary>
internal sealed class DefaultDiffGenerator : IDiffGenerator
{
    #region Methods

    /// <summary>
    /// Reads the process's current directory. A current directory that cannot be read, such as one that has been deleted or
    /// whose parent cannot be searched, contains no file the CLI can name, so it is reported as absent
    /// </summary>
    /// <returns>The full path of the current directory; <see langword="null"/> when it cannot be read</returns>
    private static string TryGetCurrentDirectory()
    {
        try
        {
            return Directory.GetCurrentDirectory();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return null;
        }
    }

    /// <summary>
    /// Determines the file's path relative to the directory when the directory is one of the directories the file's full path
    /// passes through. The two paths are compared as written, without resolving symbolic links, with the CLI's path comparer
    /// </summary>
    /// <param name="directory">The full path of the directory</param>
    /// <param name="filePath">The full path of the file</param>
    /// <param name="relativePath">The file's path relative to the directory, with <c>/</c> as the directory separator; <see langword="null"/> when the file is not inside the directory</param>
    /// <returns><see langword="true"/> when the file is inside the directory; otherwise, <see langword="false"/></returns>
    private static bool TryGetPathInside(string directory, string filePath, out string relativePath)
    {
        // The directory prefix ends with a separator, so a sibling whose name only starts with the directory's name, such as
        // "/work-old" for "/work", is not inside it. A root directory such as "/" already ends with one.
        var prefix = Path.EndsInDirectorySeparator(directory) ? directory : directory + Path.DirectorySeparatorChar;

        if (filePath.Length > prefix.Length
            && PathComparison.Comparer.Equals(filePath[..prefix.Length], prefix))
        {
            // Only the platform's directory separator becomes "/"; on Linux and macOS a backslash is part of a name.
            relativePath = filePath[prefix.Length..].Replace(Path.DirectorySeparatorChar, '/');

            return true;
        }

        relativePath = null;

        return false;
    }

    #endregion // Methods

    #region IDiffGenerator

    /// <inheritdoc/>
    public string Generate(string filePath, string originalContent, string formattedContent)
    {
        var currentDirectory = TryGetCurrentDirectory();

        // Without a readable current directory, no file is inside it, and every file is named by its full path.
        if (currentDirectory != null
            && TryGetPathInside(currentDirectory, filePath, out var relativePath))
        {
            return DiffGenerator.Generate(relativePath, originalContent, formattedContent);
        }

        return DiffGenerator.GenerateWithNames(filePath, filePath, originalContent, formattedContent);
    }

    #endregion // IDiffGenerator
}