using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Indentation;

/// <summary>
/// Tests for <see cref="Reihitsu.Formatter.Pipeline.FormattingPipeline"/> — the anchor of a chain whose first wrapped link
/// follows links that are not invoked: the wrapped links align under the last link in front of the first wrapped link
/// </summary>
[TestClass]
public class PrefixChainAnchorTests : FormatterTestsBase
{
    #region Constants

    /// <summary>
    /// A chain whose root is followed by a tab, a delimited documentation comment and two links, with its third link wrapped
    /// </summary>
    private const string TabPrefixInput = "class C\n{\n    void M()\n    {\n        var x = a\t/** c */.B.B\n.G();\n    }\n}";

    /// <summary>
    /// Expected result of formatting <see cref="TabPrefixInput"/>: the tab becomes four spaces and the wrapped link stands
    /// under the second <c>.B</c>
    /// </summary>
    private const string TabPrefixExpected = "class C\n{\n    void M()\n    {\n        var x = a    /** c */.B.B\n                               .G();\n    }\n}";

    #endregion // Constants

    #region Methods

    /// <summary>
    /// Verifies the example of FMT-INDENT-029 for a prefix of property reads: the wrapped link aligns under the last of them,
    /// with or without a later call
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-029")]
    [Requirement("SHARED-CHAIN-007")]
    public void PrefixOfPropertyReadsAlignsToTheLastOfThemWithOrWithoutCall()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int[] Items = new int[0];

                                 object M(dynamic order)
                                 {
                                     var s = this.Items.Length
                                         .ToString();

                                     return order.Customer.Address
                                         .City;
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    int[] Items = new int[0];

                                    object M(dynamic order)
                                    {
                                        var s = this.Items.Length
                                                          .ToString();

                                        return order.Customer.Address
                                                             .City;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the example of SHARED-CHAIN-007: a first call in front of the first wrapped link anchors the chain, also when
    /// later calls share its line; otherwise the last link in front of the first wrapped link does, and a link behind the first
    /// wrapped link never does; a comment that keeps the first link on its own line aligns the chain to its root
    /// </summary>
    [TestMethod]
    [Requirement("SHARED-CHAIN-007")]
    public void WrappedLinksAlignToTheAnchorOfTheSharedChainExample()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var a = items.Where(x => x.A)
                                         .Select(x => x.B);
                                     var b = this.Settings.Items
                                         .Where(x => x.A);
                                     var c = items // c
                                         .Where(x => x.A);
                                     var d = x.Items.Count
                                         .ToString();
                                     var e = order.Customer.Address
                                         .City;
                                     var f = x.Items
                                         .Count.Length
                                         .ToString();
                                     var g = items.Where(x => x.A).Select(x => x.B)
                                         .ToList();
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var a = items.Where(x => x.A)
                                                     .Select(x => x.B);
                                        var b = this.Settings.Items
                                                             .Where(x => x.A);
                                        var c = items // c
                                                .Where(x => x.A);
                                        var d = x.Items.Count
                                                       .ToString();
                                        var e = order.Customer.Address
                                                              .City;
                                        var f = x.Items
                                                 .Count.Length
                                                 .ToString();
                                        var g = items.Where(x => x.A)
                                                     .Select(x => x.B)
                                                     .ToList();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a wrapped call behind a prefix of two property reads aligns under the last of them
    /// </summary>
    [TestMethod]
    [Requirement("SHARED-CHAIN-007")]
    public void WrappedCallBehindTwoPropertyReadsAlignsUnderTheLastOfThem()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public object M(dynamic x)
                                 {
                                     var s = x.Items.Length
                                         .ToString();

                                     return s;
                                 }
                             }
                             """;

        const string expected = """
                                public class Sample
                                {
                                    public object M(dynamic x)
                                    {
                                        var s = x.Items.Length
                                                       .ToString();

                                        return s;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a chain without a call aligns every wrapped link under the last link in front of its first wrapped link,
    /// also when a later wrapped line holds two links
    /// </summary>
    [TestMethod]
    [Requirement("SHARED-CHAIN-007")]
    public void ChainWithoutCallAlignsUnderTheLastLinkInFrontOfItsFirstWrappedLink()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public object M(dynamic order)
                                 {
                                     var c = order.Customer.Address
                                         .City;
                                     var j = order.Customer.Address
                                         .City.Street
                                         .Name;

                                     return c ?? j;
                                 }
                             }
                             """;

        const string expected = """
                                public class Sample
                                {
                                    public object M(dynamic order)
                                    {
                                        var c = order.Customer.Address
                                                              .City;
                                        var j = order.Customer.Address
                                                              .City.Street
                                                              .Name;

                                        return c ?? j;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that later prefix wraps and conditional links align to the same anchor as the first wrapped link, and that a
    /// first link the formatter joins to the root's line counts as standing in front of the first wrapped link
    /// </summary>
    [TestMethod]
    [Requirement("SHARED-CHAIN-007")]
    public void LaterPrefixWrapsConditionalLinksAndJoinedFirstLinkFollowTheSameAnchor()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public object M(dynamic x)
                                 {
                                     var d = x.Next.Next
                                         .Next
                                         .Self();
                                     var g = x.Next?.Next
                                         ?.Name
                                         .Trim();
                                     var h = x
                                         .Next.Next
                                         .Self();

                                     return d ?? g ?? h;
                                 }
                             }
                             """;

        const string expected = """
                                public class Sample
                                {
                                    public object M(dynamic x)
                                    {
                                        var d = x.Next.Next
                                                      .Next
                                                      .Self();
                                        var g = x.Next?.Next
                                                      ?.Name
                                                      .Trim();
                                        var h = x.Next.Next
                                                      .Self();

                                        return d ?? g ?? h;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that of three or more links in front of the first wrapped link the last one is the anchor, not the second:
    /// with and without a later call, behind a conditional link, and when the first link joins the root's line
    /// </summary>
    [TestMethod]
    [Requirement("SHARED-CHAIN-007")]
    public void ThreeOrMoreLinksInFrontOfTheFirstWrappedLinkAlignUnderTheLastOfThem()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public object M(dynamic x, dynamic order)
                                 {
                                     var a = x.Next.Next.Next
                                         .Self();
                                     var b = x.Next.Next.Next.Next
                                         .Next;
                                     var c = x.Next?.Next.Next
                                         .Self();
                                     var d = order.Customer.Address.Street
                                         .Name;
                                     var h = x
                                         .Next.Next.Next
                                         .Self();

                                     return a ?? b ?? c ?? d ?? h;
                                 }
                             }
                             """;

        const string expected = """
                                public class Sample
                                {
                                    public object M(dynamic x, dynamic order)
                                    {
                                        var a = x.Next.Next.Next
                                                           .Self();
                                        var b = x.Next.Next.Next.Next
                                                                .Next;
                                        var c = x.Next?.Next.Next
                                                            .Self();
                                        var d = order.Customer.Address.Street
                                                                      .Name;
                                        var h = x.Next.Next.Next
                                                           .Self();

                                        return a ?? b ?? c ?? d ?? h;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the side of the boundary the change leaves alone: a chain with one link in front of its first wrapped link
    /// aligns under that link, also behind a conditional element access, and a chain with a call in front of its first
    /// wrapped link aligns under that call, also when a property read and another call share its line
    /// </summary>
    [TestMethod]
    [Requirement("SHARED-CHAIN-007")]
    public void OneLinkOrAFirstCallInFrontOfTheFirstWrappedLinkKeepsItsAnchor()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public object M(dynamic x)
                                 {
                                     var a = x.Next
                                         .Next.Next
                                         .Self();
                                     var b = x?[0, 1].Next
                                         .Self();
                                     var c = x.Next.Self()
                                         .Next;
                                     var d = x.Next.Next.Self().Next
                                         .Self();

                                     return a ?? b ?? c ?? d;
                                 }
                             }
                             """;

        const string expected = """
                                public class Sample
                                {
                                    public object M(dynamic x)
                                    {
                                        var a = x.Next
                                                 .Next.Next
                                                 .Self();
                                        var b = x?[0, 1].Next
                                                        .Self();
                                        var c = x.Next.Self()
                                                      .Next;
                                        var d = x.Next.Next.Self()
                                                           .Next
                                                           .Self();

                                        return a ?? b ?? c ?? d;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the last link in front of the first wrapped link is the anchor behind a <c>!.</c> first link, behind
    /// <c>?.</c> links, and behind a namespace-qualified root
    /// </summary>
    [TestMethod]
    [Requirement("SHARED-CHAIN-007")]
    public void LastLinkInFrontOfTheFirstWrappedLinkIsTheAnchorBehindEveryOperatorKind()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public object M(dynamic x)
                                 {
                                     var a = x!.Next.Next
                                         .Self();
                                     var b = x?.Next?.Next
                                         ?.Self();
                                     var c = System.String.Empty
                                         .Trim();

                                     return a ?? b ?? c;
                                 }
                             }
                             """;

        const string expected = """
                                public class Sample
                                {
                                    public object M(dynamic x)
                                    {
                                        var a = x!.Next.Next
                                                       .Self();
                                        var b = x?.Next?.Next
                                                       ?.Self();
                                        var c = System.String.Empty
                                                             .Trim();

                                        return a ?? b ?? c;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the anchor is the last link in front of the first wrapped link behind a root that spans lines: a
    /// multi-line invocation, and an object creation whose initializer's closing brace starts a line
    /// </summary>
    [TestMethod]
    [Requirement("SHARED-CHAIN-007")]
    public void LastLinkInFrontOfTheFirstWrappedLinkIsTheAnchorBehindARootThatSpansLines()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public object M(dynamic get)
                                 {
                                     var a = get(1,
                                         2
                                         ).Next.Next
                                         .Self();
                                     var b = new C
                                     {
                                     }.Next.Next
                                         .Self();

                                     return a ?? b;
                                 }
                             }
                             """;

        const string expected = """
                                public class Sample
                                {
                                    public object M(dynamic get)
                                    {
                                        var a = get(1,
                                                    2
                                                    ).Next.Next
                                                          .Self();
                                        var b = new C
                                                {
                                                }.Next.Next
                                                      .Self();

                                        return a ?? b;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the anchor is the last link in front of the first wrapped link behind the closing quotes of a multi-line
    /// raw string literal, measured after the literal moved
    /// </summary>
    [TestMethod]
    [Requirement("SHARED-CHAIN-007")]
    public void LastLinkInFrontOfTheFirstWrappedLinkIsTheAnchorBehindARawStringRoot()
    {
        // Arrange
        const string input = """"
                             public class Sample
                             {
                                 public object M()
                                 {
                                     var r = """
                                         text
                                         """.Length.Next
                                         .Self();

                                     return r;
                                 }
                             }
                             """";

        const string expected = """"
                                public class Sample
                                {
                                    public object M()
                                    {
                                        var r = """
                                                text
                                                """.Length.Next
                                                          .Self();

                                        return r;
                                    }
                                }
                                """";

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a directive between the prefix and the wrapped links leaves the last link in front of the first wrapped
    /// link as the anchor, and the disabled link as written
    /// </summary>
    [TestMethod]
    [Requirement("SHARED-CHAIN-007")]
    public void DirectiveInFrontOfTheFirstWrappedLinkKeepsTheLastPrefixLinkAsAnchor()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public object M(dynamic x)
                                 {
                                     var a = x.Next.Next
                             #if DEBUG
                                         .Self()
                             #endif
                                         .Self();

                                     return a;
                                 }
                             }
                             """;

        const string expected = """
                                public class Sample
                                {
                                    public object M(dynamic x)
                                    {
                                        var a = x.Next.Next
                                #if DEBUG
                                            .Self()
                                #endif
                                                      .Self();

                                        return a;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a link behind the closing <c>*/</c> of a comment that starts its own line starts a line, so the link in
    /// front of the comment stays the anchor
    /// </summary>
    [TestMethod]
    [Requirement("SHARED-CHAIN-007")]
    public void LinkBehindCommentThatStartsItsOwnLineStartsALineForTheAnchor()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public object M(dynamic x)
                                 {
                                     var a = x.Next

                                              /* c
                                     */ .Next
                                         .Self();

                                     return a;
                                 }
                             }
                             """;

        const string expected = """
                                public class Sample
                                {
                                    public object M(dynamic x)
                                    {
                                        var a = x.Next

                                                 /* c
                                        */
                                                 .Next
                                                 .Self();

                                        return a;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a chain behind a prefix of property reads aligns under the last of them inside a lambda body and inside
    /// an argument
    /// </summary>
    [TestMethod]
    [Requirement("SHARED-CHAIN-007")]
    public void PrefixChainInsideLambdaBodyAndArgumentAlignsUnderTheLastPrefixLink()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public void M(dynamic x)
                                 {
                                     System.Func<object> l = () => x.Next.Next
                                         .Self();

                                     Use(x.Next.Next
                                         .Self());
                                 }
                             }
                             """;

        const string expected = """
                                public class Sample
                                {
                                    public void M(dynamic x)
                                    {
                                        System.Func<object> l = () => x.Next.Next
                                                                            .Self();

                                        Use(x.Next.Next
                                                  .Self());
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a tab in front of a delimited documentation comment in the middle of the line, ahead of a prefix of two
    /// links, counts as four columns when the wrapped link aligns under the second link, through whole-document formatting
    /// </summary>
    [TestMethod]
    [Requirement("SHARED-CHAIN-007")]
    public void PrefixAnchorBehindTabAndDelimitedDocumentationCommentAlignsInOnePass()
    {
        AssertRuleResult(TabPrefixInput, TabPrefixExpected);
    }

    /// <summary>
    /// Verifies that detached-node formatting aligns the wrapped link under the second prefix link behind a tab and a
    /// delimited documentation comment in one pass, as whole-document formatting does
    /// </summary>
    [TestMethod]
    [Requirement("SHARED-CHAIN-007")]
    public void FormatNodeAlignsPrefixAnchorBehindTabAndDelimitedDocumentationCommentInOnePass()
    {
        foreach (var endOfLine in _lineEndings)
        {
            var root = CSharpSyntaxTree.ParseText(NormalizeLineEndings(TabPrefixInput, endOfLine), cancellationToken: TestContext.CancellationToken)
                                       .GetRoot(TestContext.CancellationToken);

            var actual = ReihitsuFormatter.FormatNode(root, cancellationToken: TestContext.CancellationToken).Output.ToFullString();

            Assert.AreEqual(NormalizeLineEndings(TabPrefixExpected, endOfLine), actual, $"FormatNode output mismatch under {DescribeLineEnding(endOfLine)} line endings.");
        }
    }

    /// <summary>
    /// Verifies that document-scoped node formatting aligns the wrapped link under the second prefix link behind a tab and a
    /// delimited documentation comment in one pass, as whole-document formatting does
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("SHARED-CHAIN-007")]
    public async Task FormatNodeInDocumentAlignsPrefixAnchorBehindTabAndDelimitedDocumentationCommentInOnePass()
    {
        foreach (var endOfLine in _lineEndings)
        {
            using (var workspace = new AdhocWorkspace())
            {
                var project = workspace.AddProject("TestProject", LanguageNames.CSharp);
                var document = project.AddDocument("Test.cs", SourceText.From(NormalizeLineEndings(TabPrefixInput, endOfLine)));
                var root = await document.GetSyntaxRootAsync(TestContext.CancellationToken);
                var method = root?.DescendantNodes().OfType<MethodDeclarationSyntax>().Single();

                if (method == null)
                {
                    Assert.Fail("Expected a method declaration in the test document.");
                }

                var result = (await ReihitsuFormatter.FormatNodeInDocumentAsync(document, method, TestContext.CancellationToken)).Output;
                var actual = (await result.GetTextAsync(TestContext.CancellationToken)).ToString();

                Assert.AreEqual(NormalizeLineEndings(TabPrefixExpected, endOfLine), actual, $"FormatNodeInDocumentAsync output mismatch under {DescribeLineEnding(endOfLine)} line endings.");
            }
        }
    }

    #endregion // Methods
}