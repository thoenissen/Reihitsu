namespace Reihitsu.Tooling.Enumerations;

/// <summary>
/// Represents a squad lane, which decides the roles a run passes through
/// </summary>
public enum Lane
{
    /// <summary>
    /// Documentation or proven text-only change; the spec author writes it and the spec checker verifies it
    /// </summary>
    Docs,

    /// <summary>
    /// Change of the diagnostic and code-fix texts in the two analyzer resource files, with the text that repeats
    /// them; the spec author writes it and the spec checker verifies it
    /// </summary>
    Wording,

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