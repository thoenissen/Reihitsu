using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Indentation;

/// <summary>
/// Reproduction-gate tests for issues #683, #684 and #685 — asserted against each issue's own
/// reported "Expected Formatted Output" so a failing run shows the issue's own observed-vs-expected
/// difference
/// </summary>
[TestClass]
public class Issue683To685ReproductionTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Issue #683 — a chain whose own first wrapped dot is a plain, non-invoked property access
    /// preceding the first invoked link should collapse onto the chain root, per the issue's own
    /// reported Input/Expected Formatted Output
    /// </summary>
    [TestMethod]
    public void Issue683WrappedNonInvokedFirstDotCollapsesOntoRoot()
    {
        // Arrange — verbatim from issue #683's "Input Code"
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

        // verbatim from issue #683's "Expected Formatted Output"
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
    /// Issue #684 — a chain rooted in a single-line object/array/collection initializer should align
    /// its continuation dot the same way a multi-line initializer already does, per the issue's own
    /// reported Input/Expected Formatted Output
    /// </summary>
    [TestMethod]
    public void Issue684ChainRootedInSingleLineInitializerAlignsCorrectly()
    {
        // Arrange — verbatim from issue #684's "Input Code"
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

        // verbatim from issue #684's "Expected Formatted Output"
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
    /// Issue #685 — a plain member-access dot immediately followed by a line break, with the member
    /// name on the next line (<c>a.</c> ⏎ <c>Prop</c>), should rejoin onto one line. The issue's own
    /// "Expected Formatted Output" section states this is not yet fully determined, but gives a
    /// literal minimum target for this exact fixture: <c>a.Prop.Call();</c>, "matching how x ⏎ .Prop
    /// already rejoins today"
    /// </summary>
    [TestMethod]
    public void Issue685TrailingDotBeforeLineBreakRejoinsWithMemberName()
    {
        // Arrange — first fixture from issue #685's "Input Code" (the plain, non-conditional case)
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

        // verbatim minimum given in issue #685's "Expected Formatted Output": "a.Prop.Call();"
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
    /// Issue #685 — the second, initializer-rooted fixture from the issue: a conditional-access dot
    /// immediately followed by a line break (<c>}.</c> ⏎ <c>Choices</c>) must not leave
    /// <c>Choices</c> orphaned on its own continuation line separated from its own dot.
    /// <para>
    /// The issue states no full target shape for this fixture, only that minimum. The expected output
    /// below is derived from the chain's reference column — <c>.ToList()</c> aligns to the <c>?</c> of
    /// the first invoked link, the same column <c>RH5201MethodChainsShouldBeAlignedAnalyzer</c>
    /// computes — and is byte-identical to the expected output the repository already asserts for the
    /// same chain written with its initializer on one line
    /// </para>
    /// </summary>
    [TestMethod]
    public void Issue685TrailingDotAfterInitializerCloseBraceDoesNotOrphanMemberName()
    {
        // Arrange — second fixture from issue #685's "Input Code"
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