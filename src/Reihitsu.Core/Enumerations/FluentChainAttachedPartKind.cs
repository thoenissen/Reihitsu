namespace Reihitsu.Core.Enumerations;

/// <summary>
/// Kinds of chain parts that belong to the element in front of them and are never a chain link
/// </summary>
public enum FluentChainAttachedPartKind
{
    /// <summary>
    /// An argument list <c>(...)</c>
    /// </summary>
    ArgumentList,

    /// <summary>
    /// An element access <c>[...]</c>
    /// </summary>
    ElementAccess,

    /// <summary>
    /// A conditional element access <c>?[...]</c>, including a null-forgiving operator directly in front of it
    /// </summary>
    ConditionalElementAccess,

    /// <summary>
    /// A null-forgiving operator <c>!</c> that is not followed by <c>.</c> or <c>?.</c>
    /// </summary>
    NullForgiving
}