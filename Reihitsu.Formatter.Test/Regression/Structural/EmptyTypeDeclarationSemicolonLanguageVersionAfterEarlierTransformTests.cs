using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Structural;

/// <summary>
/// Regression tests for the language-version gate of
/// <see cref="Pipeline.StructuralTransforms.Rewriter.EmptyTypeDeclarationSemicolonTransform"/>: the gate must hold even
/// after an earlier rewriter in the same <see cref="Pipeline.StructuralTransforms.StructuralTransformPhase"/> pass has
/// already replaced the tree, whose replacement no longer carries the source's parse options
/// </summary>
[TestClass]
public class EmptyTypeDeclarationSemicolonLanguageVersionAfterEarlierTransformTests : FormatterTestsBase
{
    #region Constants

    /// <summary>
    /// A class whose unbraced <c>if</c> makes <see cref="Pipeline.StructuralTransforms.Rewriter.ControlFlowBraceTransform"/> replace the tree
    /// </summary>
    private const string ControlFlowTriggerInput = """

                                                   public class B
                                                   {
                                                       public void M(int x)
                                                       {
                                                           if (x > 0)
                                                               x = 0;
                                                       }
                                                   }
                                                   """;

    /// <summary>
    /// The formatted form of <see cref="ControlFlowTriggerInput"/>
    /// </summary>
    private const string ControlFlowTriggerExpected = """

                                                      public class B
                                                      {
                                                          public void M(int x)
                                                          {
                                                              if (x > 0)
                                                              {
                                                                  x = 0;
                                                              }
                                                          }
                                                      }
                                                      """;

    #endregion // Constants

    #region Methods

    /// <summary>
    /// Verifies that an empty class keeps its braced body below C# 12 after the control-flow brace transform rewrote the tree
    /// </summary>
    [TestMethod]
    public void EmptyClassStaysBracedBelowCSharp12AfterControlFlowBraceTransform()
    {
        AssertRuleResult("public class A { }\n" + ControlFlowTriggerInput,
                         "public class A\n{\n}\n" + ControlFlowTriggerExpected,
                         At(LanguageVersion.CSharp11));
    }

    /// <summary>
    /// Verifies that an empty struct keeps its braced body below C# 12 after the control-flow brace transform rewrote the tree
    /// </summary>
    [TestMethod]
    public void EmptyStructStaysBracedBelowCSharp12AfterControlFlowBraceTransform()
    {
        AssertRuleResult("public struct S { }\n" + ControlFlowTriggerInput,
                         "public struct S\n{\n}\n" + ControlFlowTriggerExpected,
                         At(LanguageVersion.CSharp11));
    }

    /// <summary>
    /// Verifies that an empty interface keeps its braced body below C# 12 after the control-flow brace transform rewrote the tree
    /// </summary>
    [TestMethod]
    public void EmptyInterfaceStaysBracedBelowCSharp12AfterControlFlowBraceTransform()
    {
        AssertRuleResult("public interface I { }\n" + ControlFlowTriggerInput,
                         "public interface I\n{\n}\n" + ControlFlowTriggerExpected,
                         At(LanguageVersion.CSharp11));
    }

    /// <summary>
    /// Verifies that an empty record struct keeps its braced body below C# 10 after the control-flow brace transform rewrote the tree
    /// </summary>
    [TestMethod]
    public void EmptyRecordStructStaysBracedBelowCSharp10AfterControlFlowBraceTransform()
    {
        AssertRuleResult("public record struct R { }\n" + ControlFlowTriggerInput,
                         "public record struct R\n{\n}\n" + ControlFlowTriggerExpected,
                         At(LanguageVersion.CSharp9));
    }

    /// <summary>
    /// Verifies that an empty class keeps its braced body below C# 12 when a convertible accessor block is the earlier rewrite
    /// </summary>
    [TestMethod]
    public void EmptyClassStaysBracedBelowCSharp12AfterAccessorExpressionBodyTransform()
    {
        const string input = """
                             public class A { }

                             public class B
                             {
                                 private int _x;

                                 public int X
                                 {
                                     get { return _x; }
                                 }
                             }
                             """;
        const string expected = """
                                public class A
                                {
                                }

                                public class B
                                {
                                    private int _x;

                                    public int X
                                    {
                                        get => _x;
                                    }
                                }
                                """;

        AssertRuleResult(input, expected, At(LanguageVersion.CSharp11));
    }

    /// <summary>
    /// Verifies that an empty class keeps its braced body below C# 12 when an expression-bodied method is the earlier rewrite
    /// </summary>
    [TestMethod]
    public void EmptyClassStaysBracedBelowCSharp12AfterExpressionBodiedMethodTransform()
    {
        const string input = """
                             public class A { }

                             public class B
                             {
                                 public int M() => 1;
                             }
                             """;
        const string expected = """
                                public class A
                                {
                                }

                                public class B
                                {
                                    public int M()
                                    {
                                        return 1;
                                    }
                                }
                                """;

        AssertRuleResult(input, expected, At(LanguageVersion.CSharp11));
    }

    /// <summary>
    /// Verifies that an empty nested class keeps its braced body below C# 12 when the trigger sits in the same containing type
    /// </summary>
    [TestMethod]
    public void EmptyNestedClassStaysBracedBelowCSharp12AfterControlFlowBraceTransform()
    {
        const string input = """
                             namespace N;

                             public class B
                             {
                                 public class A { }

                                 public void M(int x)
                                 {
                                     if (x > 0)
                                         x = 0;
                                 }
                             }
                             """;
        const string expected = """
                                namespace N;

                                public class B
                                {
                                    public class A
                                    {
                                    }

                                    public void M(int x)
                                    {
                                        if (x > 0)
                                        {
                                            x = 0;
                                        }
                                    }
                                }
                                """;

        AssertRuleResult(input, expected, At(LanguageVersion.CSharp11));
    }

    /// <summary>
    /// Verifies that an empty class keeps its braced body below C# 12 when no earlier rewriter changes the tree
    /// </summary>
    [TestMethod]
    public void EmptyClassStaysBracedBelowCSharp12WithoutEarlierTransform()
    {
        AssertRuleResult("public class A { }\n\npublic class B\n{\n}",
                         "public class A\n{\n}\n\npublic class B\n{\n}",
                         At(LanguageVersion.CSharp11));
    }

    /// <summary>
    /// Verifies that an empty class still converts from C# 12 on when an earlier rewriter changed the tree
    /// </summary>
    [TestMethod]
    public void EmptyClassConvertsFromCSharp12AfterControlFlowBraceTransform()
    {
        AssertRuleResult("public class A { }\n" + ControlFlowTriggerInput,
                         "public class A;\n" + ControlFlowTriggerExpected,
                         At(LanguageVersion.CSharp12));
    }

    /// <summary>
    /// Verifies that an empty record struct still converts from C# 10 on when an earlier rewriter changed the tree
    /// </summary>
    [TestMethod]
    public void EmptyRecordStructConvertsFromCSharp10AfterControlFlowBraceTransform()
    {
        AssertRuleResult("public record struct R { }\n" + ControlFlowTriggerInput,
                         "public record struct R;\n" + ControlFlowTriggerExpected,
                         At(LanguageVersion.CSharp10));
    }

    /// <summary>
    /// Verifies that an empty record class still converts from C# 9 on when an earlier rewriter changed the tree
    /// </summary>
    [TestMethod]
    public void EmptyRecordConvertsFromCSharp9AfterControlFlowBraceTransform()
    {
        AssertRuleResult("public record R { }\n" + ControlFlowTriggerInput,
                         "public record R;\n" + ControlFlowTriggerExpected,
                         At(LanguageVersion.CSharp9));
    }

    /// <summary>
    /// Creates parse options for the given language version
    /// </summary>
    /// <param name="languageVersion">The language version</param>
    /// <returns>The parse options</returns>
    private static CSharpParseOptions At(LanguageVersion languageVersion)
    {
        return CSharpParseOptions.Default.WithLanguageVersion(languageVersion);
    }

    #endregion // Methods
}