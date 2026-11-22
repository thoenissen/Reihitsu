using System;
using System.IO;
using System.Reflection;

using Microsoft.CodeAnalysis;

namespace Reihitsu.SelfHosting.Test.Utilities;

/// <summary>
/// Analyzer assembly loader that hands Roslyn's analyzer discovery the analyzer assembly already loaded into the test
/// process, so the discovery reads the same assembly the self-hosting rows run
/// </summary>
internal sealed class LoadedAnalyzerAssemblyLoader : IAnalyzerAssemblyLoader
{
    #region Fields

    /// <summary>
    /// The analyzer assembly already loaded into the test process
    /// </summary>
    private readonly Assembly _analyzerAssembly;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="analyzerAssembly">The analyzer assembly already loaded into the test process</param>
    internal LoadedAnalyzerAssemblyLoader(Assembly analyzerAssembly)
    {
        _analyzerAssembly = analyzerAssembly;
    }

    #endregion // Constructor

    #region IAnalyzerAssemblyLoader

    /// <inheritdoc/>
    public Assembly LoadFromPath(string fullPath)
    {
        if (string.Equals(Path.GetFullPath(fullPath), Path.GetFullPath(_analyzerAssembly.Location), StringComparison.Ordinal) is false)
        {
            throw new InvalidOperationException($"Only the analyzer assembly '{_analyzerAssembly.Location}' can be loaded, not '{fullPath}'.");
        }

        return _analyzerAssembly;
    }

    /// <inheritdoc/>
    public void AddDependencyLocation(string fullPath)
    {
    }

    #endregion // IAnalyzerAssemblyLoader
}