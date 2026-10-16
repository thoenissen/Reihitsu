namespace Reihitsu.Tooling.Enumerations;

/// <summary>
/// Represents a squad lane, which decides the roles a run passes through
/// </summary>
public enum Lane
{
    /// <summary>
    /// Documentation or proven text-only change; only the spec checker runs
    /// </summary>
    Docs,

    /// <summary>
    /// Tests for behavior that already exists, without a production change
    /// </summary>
    Tests,

    /// <summary>
    /// Production change that brings the code back to its spec, without a change doc
    /// </summary>
    BugFix,

    /// <summary>
    /// Production change of intended behavior, backed by a change doc
    /// </summary>
    BehaviorChange,

    /// <summary>
    /// Change to scripts, build settings, CI, repository tooling, or the agent instructions
    /// </summary>
    Tooling
}