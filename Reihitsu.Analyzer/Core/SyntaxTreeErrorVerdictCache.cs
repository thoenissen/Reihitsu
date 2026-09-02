using System.Collections.Concurrent;
using System.Linq;
using System.Threading;

using Microsoft.CodeAnalysis;

namespace Reihitsu.Analyzer.Core;

/// <summary>
/// Caches, per syntax tree, whether the tree contains a syntax error
/// </summary>
/// <remarks>
/// Rules that suppress themselves for a whole file with malformed syntax need a verdict about the entire tree while
/// analyzing a single node. Asking the tree for its diagnostics once per node would scan the file repeatedly, so the
/// verdict is computed once per tree and reused. Instances are created inside a compilation-start callback and are
/// never stored in analyzer fields, so no state outlives the compilation being analyzed.
/// </remarks>
internal sealed class SyntaxTreeErrorVerdictCache
{
    #region Fields

    /// <summary>
    /// Verdict per syntax tree
    /// </summary>
    private readonly ConcurrentDictionary<SyntaxTree, bool> _verdicts = new();

    #endregion // Fields

    #region Methods

    /// <summary>
    /// Determines whether the syntax tree contains a syntax error
    /// </summary>
    /// <param name="tree">Tree to inspect</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns><see langword="true"/> when the tree contains an error; otherwise, <see langword="false"/></returns>
    internal bool ContainsError(SyntaxTree tree, CancellationToken cancellationToken)
    {
        if (_verdicts.TryGetValue(tree, out var containsError) == false)
        {
            containsError = tree.GetDiagnostics(cancellationToken).Any(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);

            _verdicts.TryAdd(tree, containsError);
        }

        return containsError;
    }

    #endregion // Methods
}