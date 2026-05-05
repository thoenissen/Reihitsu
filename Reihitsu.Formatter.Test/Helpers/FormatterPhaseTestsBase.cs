using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reihitsu.Formatter.Test.Helpers;

/// <summary>
/// Base class for formatter phase unit tests that execute a single phase against parsed C# source
/// </summary>
public abstract class FormatterPhaseTestsBase
{
    #region Properties

    /// <summary>
    /// Gets or sets the test context for the current test
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Applies the formatter phase to the given input
    /// </summary>
    /// <param name="input">Input source text</param>
    /// <returns>The phase output</returns>
    protected string ApplyPhase(string input)
    {
        var cancellationToken = TestContext.CancellationTokenSource.Token;
        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: cancellationToken);
        var result = ExecutePhase(tree.GetRoot(cancellationToken), cancellationToken);

        return result.ToFullString();
    }

    /// <summary>
    /// Executes the formatter phase for the given syntax root
    /// </summary>
    /// <param name="root">Syntax root</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The transformed syntax root</returns>
    protected abstract SyntaxNode ExecutePhase(SyntaxNode root, CancellationToken cancellationToken);

    #endregion // Methods
}