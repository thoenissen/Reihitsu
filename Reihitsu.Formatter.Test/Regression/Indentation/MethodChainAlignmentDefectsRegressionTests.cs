using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Indentation;

/// <summary>
/// Regression tests for three separate method-chain alignment defects: collapsing a wrapped
/// non-invoked first dot onto the chain root, aligning a chain rooted in a single-line initializer,
/// and rejoining a trailing dot immediately followed by a line break. Each test is asserted against
/// its own literal expected output so a failing run shows the exact observed-vs-expected difference
/// </summary>
[TestClass]
public class MethodChainAlignmentDefectsRegressionTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that a chain whose own first wrapped dot is a plain, non-invoked property access
    /// preceding the first invoked link collapses onto the chain root
    /// </summary>
    [TestMethod]
    public void WrappedNonInvokedFirstDotCollapsesOntoRoot()
    {
        // Arrange — a chain whose first wrapped dot is a plain, non-invoked property access
        const string input = """
                             internal sealed class Example
                             {
                                 internal static void Run(object a)
                                 {
                                     var x = a
                                         .Prop?.ToString()
                                               .Trim();
                                 }
                             }
                             """;

        // the expected collapse of the chain's first wrapped dot onto its root
        const string expected = """
                                internal sealed class Example
                                {
                                    internal static void Run(object a)
                                    {
                                        var x = a.Prop?.ToString()
                                                      .Trim();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a chain rooted in a single-line object/array/collection initializer aligns its
    /// continuation dot the same way a multi-line initializer already does
    /// </summary>
    [TestMethod]
    public void ChainRootedInSingleLineInitializerAlignsCorrectly()
    {
        // Arrange — a chain rooted in a single-line object initializer
        const string input = """
                             using System.Collections.Generic;
                             using System.Linq;

                             internal sealed class Example
                             {
                                 internal sealed class Wrapper
                                 {
                                     public int Value { get; set; }
                                 }

                                 internal static List Convert()
                                 {
                                     var result = new[] { 1, 2, 3 }.Select(item => new Wrapper
                                     {
                                         Value = item
                                     })
                                                                    .ToList();

                                     return result;
                                 }
                             }
                             """;

        // the expected alignment of the continuation dot to the initializer's opening brace column
        const string expected = """
                                using System.Collections.Generic;
                                using System.Linq;

                                internal sealed class Example
                                {
                                    internal sealed class Wrapper
                                    {
                                        public int Value { get; set; }
                                    }

                                    internal static List Convert()
                                    {
                                        var result = new[] { 1, 2, 3 }.Select(item => new Wrapper
                                                                                      {
                                                                                          Value = item
                                                                                      })
                                                                      .ToList();

                                        return result;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a plain member-access dot immediately followed by a line break, with the member
    /// name on the next line (<c>a.</c> ⏎ <c>Prop</c>), rejoins onto one line — the minimum target
    /// for this exact fixture is <c>a.Prop.Call();</c>, matching how <c>x</c> ⏎ <c>.Prop</c> already
    /// rejoins today. This is the plain, non-conditional case; the sibling test below covers the
    /// initializer-rooted, conditional-access variant
    /// </summary>
    [TestMethod]
    public void TrailingDotBeforeLineBreakRejoinsWithMemberName()
    {
        // Arrange — the plain, non-conditional case: a trailing dot immediately before a line break
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var a1 = a.
                                         Prop.Call();
                                 }
                             }
                             """;

        // the minimum target for this fixture: "a.Prop.Call();"
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var a1 = a.Prop.Call();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the initializer-rooted, conditional-access counterpart to the sibling test above: a
    /// conditional-access dot immediately followed by a line break (<c>}.</c> ⏎ <c>Choices</c>) must
    /// not leave <c>Choices</c> orphaned on its own continuation line separated from its own dot.
    /// <para>
    /// No full target shape is dictated for this fixture beyond that minimum. The expected output
    /// below is derived from the chain's reference column — <c>.ToList()</c> aligns to the <c>?</c>
    /// of the first invoked link, the same column <c>RH5201MethodChainsShouldBeAlignedAnalyzer</c>
    /// computes — and is byte-identical to the expected output the repository already asserts for the
    /// same chain written with its initializer on one line
    /// </para>
    /// </summary>
    [TestMethod]
    public void TrailingDotAfterInitializerCloseBraceDoesNotOrphanMemberName()
    {
        // Arrange — the initializer-rooted, conditional-access fixture
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var x = new Request
                                             {
                                                 Choices = data
                                             }.
                                                 Choices?.Select(item => new Wrapper
                                                                         {
                                                                             Value = item
                                                                         })
                                                        .ToList();
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var x = new Request
                                                {
                                                    Choices = data
                                                }.Choices?.Select(item => new Wrapper
                                                                          {
                                                                              Value = item
                                                                          })
                                                         .ToList();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Methods
}