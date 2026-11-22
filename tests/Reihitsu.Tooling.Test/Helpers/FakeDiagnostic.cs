namespace Reihitsu.Tooling.Test.Helpers;

/// <summary>
/// The diagnostic the resolver test doubles are built around
/// </summary>
internal static class FakeDiagnostic
{
    #region Constants

    /// <summary>
    /// Diagnostic ID reported by <see cref="FakeAnalyzer"/> and declared fixable by the fake providers
    /// </summary>
    public const string Id = "RH0000";

    #endregion // Constants
}