using System;
using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Reihitsu.SelfHosting.Test.Utilities;

/// <summary>
/// Provides the analyzer options of a project as the build gives them: its global options, and no options per file
/// </summary>
internal sealed class SelfHostingAnalyzerConfigOptionsProvider : AnalyzerConfigOptionsProvider
{
    #region Fields

    /// <summary>
    /// Options of a single file; the repository sets none
    /// </summary>
    private static readonly AnalyzerConfigOptions _emptyOptions = new SelfHostingAnalyzerConfigOptions(ImmutableDictionary<string, string>.Empty);

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="SelfHostingAnalyzerConfigOptionsProvider"/> class
    /// </summary>
    /// <param name="globalOptions">Global options of the project</param>
    internal SelfHostingAnalyzerConfigOptionsProvider(AnalyzerConfigOptions globalOptions)
    {
        ArgumentNullException.ThrowIfNull(globalOptions);

        GlobalOptions = globalOptions;
    }

    #endregion // Constructor

    #region AnalyzerConfigOptionsProvider

    /// <inheritdoc/>
    public override AnalyzerConfigOptions GlobalOptions { get; }

    /// <inheritdoc/>
    public override AnalyzerConfigOptions GetOptions(SyntaxTree tree)
    {
        return _emptyOptions;
    }

    /// <inheritdoc/>
    public override AnalyzerConfigOptions GetOptions(AdditionalText textFile)
    {
        return _emptyOptions;
    }

    #endregion // AnalyzerConfigOptionsProvider
}