namespace Reihitsu.Formatter.Pipeline.Indentation;

/// <summary>
/// Represents an indentation context in the formatting scope hierarchy.
/// Scopes track the base column for content and enable nested indentation
/// contexts (e.g., lambdas inside initializers inside argument lists)
/// </summary>
internal sealed class FormattingScope
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="baseColumn">The base column for content within this scope</param>
    /// <param name="kind">The kind of scope</param>
    /// <param name="parent">The parent scope (null for root scope)</param>
    public FormattingScope(int baseColumn, ScopeKind kind = ScopeKind.Block, FormattingScope parent = null)
    {
        BaseColumn = baseColumn;
        Kind = kind;
        Parent = parent;
    }

    #endregion // Constructor

    #region Properties

    /// <summary>
    /// The parent scope (null for root scope)
    /// </summary>
    public FormattingScope Parent { get; }

    /// <summary>
    /// The base column (0-based) for content within this scope
    /// </summary>
    public int BaseColumn { get; }

    /// <summary>
    /// The kind of scope
    /// </summary>
    public ScopeKind Kind { get; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Creates a child scope with the given base column and kind
    /// </summary>
    /// <param name="baseColumn">The base column for the child scope</param>
    /// <param name="kind">The kind of child scope</param>
    /// <returns>A new child scope</returns>
    public FormattingScope CreateChild(int baseColumn, ScopeKind kind = ScopeKind.Block)
    {
        return new FormattingScope(baseColumn, kind, this);
    }

    #endregion // Methods
}