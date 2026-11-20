using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

using Microsoft.CodeAnalysis.Diagnostics;

namespace Reihitsu.SelfHosting.Test.Utilities;

/// <summary>
/// Analyzer options of a project as the build gives them: the <c>build_property.*</c> entries MSBuild writes for the
/// properties the analyzers see
/// </summary>
internal sealed class SelfHostingAnalyzerConfigOptions : AnalyzerConfigOptions
{
    #region Fields

    /// <summary>
    /// Options by key
    /// </summary>
    private readonly ImmutableDictionary<string, string> _options;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="SelfHostingAnalyzerConfigOptions"/> class
    /// </summary>
    /// <param name="options">Options by key</param>
    internal SelfHostingAnalyzerConfigOptions(ImmutableDictionary<string, string> options)
    {
        ArgumentNullException.ThrowIfNull(options);

        _options = options;
    }

    #endregion // Constructor

    #region AnalyzerConfigOptions

    /// <inheritdoc/>
    public override IEnumerable<string> Keys => _options.Keys;

    /// <inheritdoc/>
    public override bool TryGetValue(string key, [NotNullWhen(true)] out string value)
    {
        return _options.TryGetValue(key, out value);
    }

    #endregion // AnalyzerConfigOptions
}