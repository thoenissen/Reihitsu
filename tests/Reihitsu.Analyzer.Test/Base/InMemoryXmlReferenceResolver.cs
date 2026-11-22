using System.Collections.Generic;
using System.IO;
using System.Text;

using Microsoft.CodeAnalysis;

namespace Reihitsu.Analyzer.Test.Base;

/// <summary>
/// Resolves the files that <c>&lt;include&gt;</c> elements of documentation comments name from memory, so an analyzer
/// test can provide included documentation without writing files to disk. A file that was not provided is not
/// resolved, as an included file that does not exist
/// </summary>
internal sealed class InMemoryXmlReferenceResolver : XmlReferenceResolver
{
    #region Fields

    /// <summary>
    /// Contents of the provided files, by the path an <c>&lt;include&gt;</c> element names
    /// </summary>
    private readonly IReadOnlyDictionary<string, string> _files;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="files">Contents of the provided files, by the path an <c>&lt;include&gt;</c> element names</param>
    public InMemoryXmlReferenceResolver(IReadOnlyDictionary<string, string> files)
    {
        _files = files;
    }

    #endregion // Constructor

    #region XmlReferenceResolver

    /// <inheritdoc/>
    public override string ResolveReference(string path, string baseFilePath)
    {
        return path != null && _files.ContainsKey(path) ? path : null;
    }

    /// <inheritdoc/>
    public override Stream OpenRead(string resolvedPath)
    {
        if (_files.TryGetValue(resolvedPath, out var content) == false)
        {
            throw new FileNotFoundException("The file was not provided to the resolver", resolvedPath);
        }

        return new MemoryStream(Encoding.UTF8.GetBytes(content));
    }

    #endregion // XmlReferenceResolver

    #region Object

    /// <inheritdoc/>
    public override bool Equals(object other)
    {
        return ReferenceEquals(this, other);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return _files.GetHashCode();
    }

    #endregion // Object
}