namespace Reihitsu.Core;

/// <summary>
/// What a member of a type's body, in one build configuration or in all of them, contributes to the orders that a member reordering fix
/// keeps: the order of the static and of the instance runtime initializers, and the order of the instance fields that
/// set a type's layout. The member counts at its position in the body
/// </summary>
internal readonly struct OrderedMember
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="position">Position the member counts at</param>
    /// <param name="hasStaticInitializer">Whether the member has a static runtime initializer</param>
    /// <param name="hasInstanceInitializer">Whether the member has an instance runtime initializer</param>
    /// <param name="isLayoutField">Whether the member is an instance field of the type's layout</param>
    public OrderedMember(int position, bool hasStaticInitializer, bool hasInstanceInitializer, bool isLayoutField)
    {
        Position = position;
        HasStaticInitializer = hasStaticInitializer;
        HasInstanceInitializer = hasInstanceInitializer;
        IsLayoutField = isLayoutField;
    }

    #endregion // Constructor

    #region Properties

    /// <summary>
    /// Position the member counts at
    /// </summary>
    public int Position { get; }

    /// <summary>
    /// Whether the member has a static runtime initializer
    /// </summary>
    public bool HasStaticInitializer { get; }

    /// <summary>
    /// Whether the member has an instance runtime initializer in some build configuration
    /// </summary>
    public bool HasInstanceInitializer { get; }

    /// <summary>
    /// Whether the member is an instance field of the type's layout
    /// </summary>
    public bool IsLayoutField { get; }

    #endregion // Properties
}