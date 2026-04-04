using Microsoft.CodeAnalysis;

namespace Reihitsu.Formatter.Rules;

/// <summary>
/// Interface for an individual formatting rule.
/// Each rule handles one specific aspect of code formatting.
/// Implementations are instantiated per pipeline execution to ensure thread safety.
/// </summary>
internal interface IFormattingRule
{
    /// <summary>
    /// The phase in the formatting pipeline this rule belongs to.
    /// </summary>
    FormattingPhase Phase { get; }

    /// <summary>
    /// Applies this formatting rule to the given syntax node.
    /// </summary>
    /// <param name="node">The syntax node to format.</param>
    /// <returns>The formatted syntax node.</returns>
    SyntaxNode Apply(SyntaxNode node);
}