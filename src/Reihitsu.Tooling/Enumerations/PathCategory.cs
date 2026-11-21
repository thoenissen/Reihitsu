namespace Reihitsu.Tooling.Enumerations;

/// <summary>
/// Represents what a changed path means for the lane a diff needs
/// </summary>
public enum PathCategory
{
    /// <summary>
    /// Documentation, specs, templates, or a C# file proven text-only
    /// </summary>
    Text,

    /// <summary>
    /// A change doc under <c>sdd/changes/</c>
    /// </summary>
    ChangeDoc,

    /// <summary>
    /// One of the two resource files that hold the diagnostic titles and messages and the code-fix titles
    /// </summary>
    Wording,

    /// <summary>
    /// A file of a test project
    /// </summary>
    Test,

    /// <summary>
    /// A shipped source file
    /// </summary>
    Production,

    /// <summary>
    /// A script, build setting, CI workflow, repository tool, or agent instruction file
    /// </summary>
    Tooling,

    /// <summary>
    /// A path no rule classifies, so no lane can be derived for it
    /// </summary>
    Unknown
}