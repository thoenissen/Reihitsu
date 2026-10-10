using System.Threading;

using Microsoft.CodeAnalysis;

using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Pipeline;

namespace Reihitsu.Formatter.Test.Unit.Pipeline;

/// <summary>
/// Test phase that raises a caller-supplied exception
/// </summary>
internal sealed class ThrowingPhase : IFormattingPhase
{
    #region Fields

    /// <summary>
    /// Exception raised when the phase executes
    /// </summary>
    private readonly Exception _exception;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="ThrowingPhase"/> class
    /// </summary>
    /// <param name="exception">Exception to raise</param>
    public ThrowingPhase(Exception exception)
    {
        _exception = exception;
    }

    #endregion // Constructor

    #region IFormattingPhase

    /// <inheritdoc/>
    public SyntaxNode Execute(SyntaxNode root, FormattingContext context, CancellationToken cancellationToken)
    {
        throw _exception;
    }

    #endregion // IFormattingPhase
}