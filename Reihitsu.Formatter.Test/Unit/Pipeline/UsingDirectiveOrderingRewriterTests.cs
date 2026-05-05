using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Pipeline.UsingDirectives;
using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Unit.Pipeline;

/// <summary>
/// Tests for <see cref="UsingDirectiveOrderingPhase"/> using directive ordering
/// </summary>
[TestClass]
public class UsingDirectiveOrderingRewriterTests : FormatterPhaseTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that regular usings without trivia are reordered
    /// </summary>
    [TestMethod]
    public void RegularUsingsWithoutTriviaAreReordered()
    {
        // Arrange
        const string input = """
                             using System.Linq;
                             using System;
                             """;
        var expected = "using System;using System.Linq;" + Environment.NewLine;

        // Assert
        Assert.AreEqual(expected, ApplyPhase(input));
    }

    /// <summary>
    /// Verifies that alias directives without trivia are reordered
    /// </summary>
    [TestMethod]
    public void AliasDirectivesWithoutTriviaAreReordered()
    {
        // Arrange
        const string input = """
                             using L = System.Linq;
                             using C = System.Collections;
                             """;
        var expected = "using C = System.Collections;using L = System.Linq;" + Environment.NewLine;

        // Assert
        Assert.AreEqual(expected, ApplyPhase(input));
    }

    /// <summary>
    /// Verifies that using static directives without trivia are reordered
    /// </summary>
    [TestMethod]
    public void UsingStaticDirectivesWithoutTriviaAreReordered()
    {
        // Arrange
        const string input = """
                             using static System.Math;
                             using static System.Console;
                             """;
        var expected = "using static System.Console;using static System.Math;" + Environment.NewLine;

        // Assert
        Assert.AreEqual(expected, ApplyPhase(input));
    }

    /// <summary>
    /// Verifies that conditional directives skip reordering
    /// </summary>
    [TestMethod]
    public void ConditionalDirectiveSkipsReordering()
    {
        // Arrange
        const string input = """
                             using System;
                             #if DEBUG
                             using System.Linq;
                             #endif
                             """;

        // Assert
        Assert.AreEqual(input, ApplyPhase(input));
    }

    /// <summary>
    /// Verifies that a nullable directive on a later directive skips reordering
    /// </summary>
    [TestMethod]
    public void NullableDirectiveSkipsReordering()
    {
        // Arrange
        const string input = """
                             using System;
                             #nullable enable
                             using System.Linq;
                             """;

        // Assert
        Assert.AreEqual(input, ApplyPhase(input));
    }

    /// <summary>
    /// Verifies that a pragma directive on a later directive skips reordering
    /// </summary>
    [TestMethod]
    public void PragmaDirectiveSkipsReordering()
    {
        // Arrange
        const string input = """
                             using System;
                             #pragma warning disable CS8019
                             using System.Linq;
                             """;

        // Assert
        Assert.AreEqual(input, ApplyPhase(input));
    }

    /// <summary>
    /// Verifies that a single using directive is not processed
    /// </summary>
    [TestMethod]
    public void SingleUsingDirectiveIsNotProcessed()
    {
        // Arrange
        const string input = """
                             using System;
                             """;

        // Assert
        Assert.AreEqual(input, ApplyPhase(input));
    }

    /// <inheritdoc/>
    protected override SyntaxNode ExecutePhase(SyntaxNode root, CancellationToken cancellationToken)
    {
        var context = new FormattingContext(Environment.NewLine);

        return UsingDirectiveOrderingPhase.Execute(root, context, cancellationToken);
    }

    #endregion // Methods
}