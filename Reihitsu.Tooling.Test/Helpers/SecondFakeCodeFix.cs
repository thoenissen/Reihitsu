using System.Collections.Immutable;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CodeFixes;

namespace Reihitsu.Tooling.Test.Helpers;

/// <summary>
/// A second provider claiming <see cref="FakeDiagnostic.Id"/>, so the ambiguous resolution can be observed
/// </summary>
internal sealed class SecondFakeCodeFix : CodeFixProvider
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