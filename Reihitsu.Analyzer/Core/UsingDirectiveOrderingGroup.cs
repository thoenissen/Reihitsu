namespace Reihitsu.Analyzer.Core;

/// <summary>
/// Using directive groups
/// </summary>
internal enum UsingDirectiveOrderingGroup
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