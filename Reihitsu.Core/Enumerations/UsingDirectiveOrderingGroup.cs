namespace Reihitsu.Core.Enumerations;

/// <summary>
/// Using directive groups
/// </summary>
public enum UsingDirectiveOrderingGroup
{
    /// <summary>
    /// Regular System namespace imports
    /// </summary>
    SystemNamespace,

    /// <summary>
    /// Regular non-System namespace imports
    /// </summary>
    OtherNamespace,

    /// <summary>
    /// Static using directives
    /// </summary>
    Static,

    /// <summary>
    /// Using alias directives
    /// </summary>
    Alias
}