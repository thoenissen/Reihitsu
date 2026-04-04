using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Reihitsu.Formatter.Rules;

/// <summary>
/// Base class for formatting rules that operate as syntax rewriters.
/// Instances are created per pipeline execution to ensure thread safety.
/// </summary>
internal abstract class FormattingRuleBase : CSharpSyntaxRewriter, IFormattingRule
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="context">The formatting context.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    protected FormattingRuleBase(FormattingContext context, CancellationToken cancellationToken)
    {
        Context = context;
        CancellationToken = cancellationToken;
    }

    #endregion // Constructor

    #region IFormattingRule

    /// <inheritdoc/>
    public abstract FormattingPhase Phase { get; }

    /// <inheritdoc/>
    public SyntaxNode Apply(SyntaxNode node)
    {
        return Visit(node);
    }

    #endregion // IFormattingRule

    #region Properties

    /// <summary>
    /// The current formatting context.
    /// </summary>
    protected FormattingContext Context { get; }

    /// <summary>
    /// Cancellation token for the current operation.
    /// </summary>
    protected CancellationToken CancellationToken { get; }

    #endregion // Properties
}