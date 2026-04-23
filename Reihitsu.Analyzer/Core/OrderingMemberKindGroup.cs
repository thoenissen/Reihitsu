namespace Reihitsu.Analyzer.Core;

/// <summary>
/// Member kind groups used for ordering comparisons.
/// </summary>
internal enum OrderingMemberKindGroup
{
    /// <summary>
    /// Type declarations.
    /// </summary>
    Type,

    /// <summary>
    /// Delegate declarations.
    /// </summary>
    Delegate,

    /// <summary>
    /// Field declarations.
    /// </summary>
    Field,

    /// <summary>
    /// Constructor declarations.
    /// </summary>
    Constructor,

    /// <summary>
    /// Destructor declarations.
    /// </summary>
    Destructor,

    /// <summary>
    /// Property declarations.
    /// </summary>
    Property,

    /// <summary>
    /// Indexer declarations.
    /// </summary>
    Indexer,

    /// <summary>
    /// Event declarations.
    /// </summary>
    Event,

    /// <summary>
    /// Event field declarations.
    /// </summary>
    EventField,

    /// <summary>
    /// Method declarations.
    /// </summary>
    Method,

    /// <summary>
    /// Operator declarations.
    /// </summary>
    Operator,

    /// <summary>
    /// Conversion operator declarations.
    /// </summary>
    ConversionOperator,

    /// <summary>
    /// Unknown declaration kind.
    /// </summary>
    Unknown
}