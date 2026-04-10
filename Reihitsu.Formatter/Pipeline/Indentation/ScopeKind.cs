namespace Reihitsu.Formatter.Pipeline.Indentation;

/// <summary>
/// Represents the kind of formatting scope.
/// </summary>
internal enum ScopeKind
{
    /// <summary>
    /// Standard block indentation (namespace, class, method body, etc.)
    /// </summary>
    Block,

    /// <summary>
    /// Continuation alignment (arguments, chain, initializer)
    /// </summary>
    Continuation,

    /// <summary>
    /// Lambda body — creates a new indentation root
    /// </summary>
    LambdaBody,

    /// <summary>
    /// Initializer — creates a new indentation root
    /// </summary>
    Initializer
}