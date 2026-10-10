namespace Reihitsu.Core.Enumerations;

/// <summary>
/// Category of a top-level region relative to the type that declares it
/// </summary>
public enum RegionCategory
{
    /// <summary>
    /// Region with a self-defined (custom) name, such as <c>Constants</c>, <c>Fields</c> or <c>Methods</c>
    /// </summary>
    Custom = 0,

    /// <summary>
    /// Region grouping override members by the declaring base type
    /// </summary>
    BaseTypeOverride = 1,

    /// <summary>
    /// Region grouping members by the implemented interface
    /// </summary>
    InterfaceImplementation = 2
}