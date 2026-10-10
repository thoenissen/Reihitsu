using System.Collections.Immutable;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CodeFixes;

namespace Reihitsu.Tooling.Test.Helpers;

/// <summary>
/// A code fix provider that declares <see cref="FakeDiagnostic.Id"/> fixable and registers nothing
/// </summary>
internal sealed class FirstFakeCodeFix : CodeFixProvider
{
    #region CodeFixProvider

    /// <inheritdoc/>
    public override ImmutableArray<string> FixableDiagnosticIds => [FakeDiagnostic.Id];

    /// <inheritdoc/>
    public override Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        return Task.CompletedTask;
    }

    #endregion // CodeFixProvider
}