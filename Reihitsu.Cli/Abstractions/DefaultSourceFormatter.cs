using Microsoft.CodeAnalysis;

using Reihitsu.Formatter;

namespace Reihitsu.Cli.Abstractions;

/// <summary>
/// Default source formatter implementation that delegates to <see cref="ReihitsuFormatter"/>.
/// </summary>
internal sealed class DefaultSourceFormatter : ISourceFormatter
{
    #region ISourceFormatter

    /// <inheritdoc/>
    public SyntaxTree FormatSyntaxTree(SyntaxTree syntaxTree, CancellationToken cancellationToken)
    {
        return ReihitsuFormatter.FormatSyntaxTree(syntaxTree, cancellationToken);
    }

    #endregion // ISourceFormatter
}