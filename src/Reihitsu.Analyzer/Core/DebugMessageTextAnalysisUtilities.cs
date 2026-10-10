using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Analyzer.Core;

/// <summary>
/// Utilities for determining whether a Debug message argument provides usable text
/// </summary>
internal static class DebugMessageTextAnalysisUtilities
{
    #region Methods

    /// <summary>
    /// Determines whether the argument is a constant message without usable text
    /// </summary>
    /// <param name="messageExpression">Message expression</param>
    /// <param name="semanticModel">Semantic model</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns><see langword="true"/> when the message is <see langword="null"/>, empty, or whitespace</returns>
    internal static bool IsConstantMessageWithoutText(ExpressionSyntax messageExpression, SemanticModel semanticModel, CancellationToken cancellationToken)
    {
        var constantValue = semanticModel.GetConstantValue(messageExpression, cancellationToken);

        return constantValue.HasValue
               && (constantValue.Value is null
                   || (constantValue.Value is string text && string.IsNullOrWhiteSpace(text)));
    }

    #endregion // Methods
}