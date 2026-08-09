namespace Reihitsu.Tooling.Enumerations;

/// <summary>
/// Represents how one fixture ended under one line-ending arm
/// </summary>
public enum FixtureOutcome
{
    /// <summary>
    /// The analyzer never reported the diagnostic, so the fixture does not reproduce it
    /// </summary>
    NoDiagnostic,

    /// <summary>
    /// The analyzer reported the diagnostic and no code action was registered for it. This is a supported end
    /// state rather than a failure: a rule may deliberately leave a construct to manual correction
    /// </summary>
    NoFixOffered,

    /// <summary>
    /// The code fix was applied until the diagnostic was no longer reported
    /// </summary>
    Fixed,

    /// <summary>
    /// The diagnostic was still reported after the iteration cap was reached
    /// </summary>
    NotConverged,

    /// <summary>
    /// The fixture itself does not parse, so nothing could be analyzed
    /// </summary>
    ParseError,

    /// <summary>
    /// Applying the code fix produced source that no longer parses
    /// </summary>
    InvalidResult
}