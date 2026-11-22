using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.LineBreaks;

/// <summary>
/// Full-pipeline tests for the line breaks inside an interpolation of a non-verbatim interpolated string: with C# 10 or
/// earlier, such an interpolation cannot hold a line break, so the formatter writes none into it at any depth, while every
/// other requirement still applies there; with C# 11 or later, and in a verbatim interpolated string, the other requirements
/// break lines as usual
/// </summary>
[TestClass]
public class NonVerbatimInterpolationLineBreakTests : FormatterTestsBase
{
    #region Constants

    /// <summary>
    /// The input of the FMT-BREAK-044 example
    /// </summary>
    private const string SpecInput = """
                                     using System;

                                     class C
                                     {
                                         int F(Func<int> f)
                                         {
                                             return f();
                                         }

                                         string M()
                                         {
                                             var a = $"a {F(delegate { return 1; })} b";
                                             var b = $@"a {F(delegate { return 1; })} b";
                                             var c = $"a {$@"x {F(delegate { return 1; })}"} b";
                                             var d = $@"a {$"x {F(delegate { return 1; })}"} b";
                                             var e = F(delegate { return $"x {1}".Length; });

                                             return a + b + c + d + e;
                                         }
                                     }
                                     """;

    /// <summary>
    /// The formatted form of <see cref="SpecInput"/> under C# 10
    /// </summary>
    private const string SpecCSharp10Expected = """
                                                using System;

                                                class C
                                                {
                                                    int F(Func<int> f)
                                                    {
                                                        return f();
                                                    }

                                                    string M()
                                                    {
                                                        var a = $"a {F(delegate { return 1; })} b";
                                                        var b = $@"a {F(delegate
                                                        {
                                                            return 1;
                                                        })} b";
                                                        var c = $"a {$@"x {F(delegate { return 1; })}"} b";
                                                        var d = $@"a {$"x {F(delegate { return 1; })}"} b";
                                                        var e = F(delegate
                                                        {
                                                            return $"x {1}".Length;
                                                        });

                                                        return a + b + c + d + e;
                                                    }
                                                }
                                                """;

    /// <summary>
    /// The formatted form of <see cref="SpecInput"/> under C# 11
    /// </summary>
    private const string SpecCSharp11Expected = """
                                                using System;

                                                class C
                                                {
                                                    int F(Func<int> f)
                                                    {
                                                        return f();
                                                    }

                                                    string M()
                                                    {
                                                        var a = $"a {F(delegate
                                                        {
                                                            return 1;
                                                        })} b";
                                                        var b = $@"a {F(delegate
                                                        {
                                                            return 1;
                                                        })} b";
                                                        var c = $"a {$@"x {F(delegate
                                                        {
                                                            return 1;
                                                        })}"} b";
                                                        var d = $@"a {$"x {F(delegate
                                                        {
                                                            return 1;
                                                        })}"} b";
                                                        var e = F(delegate
                                                        {
                                                            return $"x {1}".Length;
                                                        });

                                                        return a + b + c + d + e;
                                                    }
                                                }
                                                """;

    #endregion // Constants

    #region Methods

    /// <summary>
    /// Verifies the FMT-BREAK-044 example under C# 10: the interpolations of <c>a</c>, <c>c</c>, and <c>d</c> keep their
    /// line, the verbatim-only <c>b</c> and the anonymous method of <c>e</c>, which stands outside any interpolation, are
    /// broken
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-044")]
    public void SpecExampleKeepsNonVerbatimInterpolationsOnOneLineInCSharp10()
    {
        AssertRuleResult(SpecInput, SpecCSharp10Expected, At(LanguageVersion.CSharp10));
    }

    /// <summary>
    /// Verifies the FMT-BREAK-044 example under C# 11: every interpolation is laid out as the other requirements state
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-044")]
    public void SpecExampleBreaksEveryInterpolationInCSharp11()
    {
        AssertRuleResult(SpecInput, SpecCSharp11Expected, At(LanguageVersion.CSharp11));
    }

    /// <summary>
    /// Verifies the reported scenario under C# 10: the anonymous method in the interpolation keeps its line, and the blank
    /// line in front of the <c>return</c>, which stands outside the interpolation, is still written
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-044")]
    public void AnonymousMethodInInterpolationKeepsItsLineInCSharp10()
    {
        const string input = """
                             class C
                             {
                                 string M()
                                 {
                                     var b1 = $"J {M(delegate { return 1; })}";
                                     return b1;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    string M()
                                    {
                                        var b1 = $"J {M(delegate { return 1; })}";

                                        return b1;
                                    }
                                }
                                """;

        AssertRuleResult(input, expected, At(LanguageVersion.CSharp10));
    }

    /// <summary>
    /// Verifies that the condition holds for every version before C# 11, not only C# 10: under C# 7.3 an anonymous method in
    /// the interpolation keeps its line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-044")]
    public void AnonymousMethodInInterpolationKeepsItsLineInCSharp73()
    {
        AssertKept("var s = $\"a {F(delegate { return 1; })} b\";", LanguageVersion.CSharp7_3);
    }

    /// <summary>
    /// Verifies that an expression-bodied method whose expression holds the interpolation still gets its block body under
    /// C# 10, while the anonymous method in the interpolation keeps its line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-044")]
    public void ExpressionBodiedMethodGetsBlockBodyWhileInterpolationKeepsItsLineInCSharp10()
    {
        const string input = """
                             using System;

                             class C
                             {
                                 int F(Func<int> f)
                                 {
                                     return f();
                                 }

                                 string M() => $"a {F(delegate { return 1; })} b";
                             }
                             """;
        const string expected = """
                                using System;

                                class C
                                {
                                    int F(Func<int> f)
                                    {
                                        return f();
                                    }

                                    string M()
                                    {
                                        return $"a {F(delegate { return 1; })} b";
                                    }
                                }
                                """;

        AssertRuleResult(input, expected, At(LanguageVersion.CSharp10));
    }

    /// <summary>
    /// Verifies that an empty anonymous method body in the interpolation keeps its line under C# 10
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-044")]
    public void EmptyAnonymousMethodInInterpolationKeepsItsLineInCSharp10()
    {
        AssertKept("var s = $\"a {F(delegate { })} b\";");
    }

    /// <summary>
    /// Verifies that a lambda block in the interpolation keeps its line under C# 10
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-044")]
    public void LambdaBlockInInterpolationKeepsItsLineInCSharp10()
    {
        AssertKept("var s = $\"a {F(() => { return 1; })} b\";");
    }

    /// <summary>
    /// Verifies that an empty lambda block in the interpolation keeps its line under C# 10
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-044")]
    public void EmptyLambdaBlockInInterpolationKeepsItsLineInCSharp10()
    {
        AssertKept("var s = $\"a {F(() => { })} b\";");
    }

    /// <summary>
    /// Verifies that the statements of a lambda block in the interpolation stay on one line under C# 10
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-044")]
    public void LambdaBlockStatementsInInterpolationKeepTheirLineInCSharp10()
    {
        AssertKept("var s = $\"a {F(() => { var a = 1; return a; })} b\";");
    }

    /// <summary>
    /// Verifies that an embedded <c>if</c> statement in a lambda in the interpolation still gets its braces under C# 10,
    /// on the line of the interpolation
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-044")]
    public void IfStatementInInterpolationGetsBracesOnItsLineInCSharp10()
    {
        AssertResult("var s = $\"a {F(() => { if (b) return 1; return 2; })} b\";",
                     "var s = $\"a {F(() => { if (b) {return 1; }return 2; })} b\";",
                     "bool b");
    }

    /// <summary>
    /// Verifies that a switch statement in a lambda in the interpolation keeps its line under C# 10
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-044")]
    public void SwitchStatementInInterpolationKeepsItsLineInCSharp10()
    {
        AssertKept("var s = $\"a {F(() => { switch (x) { case 1: return 1; default: return 2; } })} b\";", parameters: "int x");
    }

    /// <summary>
    /// Verifies that a switch section with a statement and a <c>return</c> on one line in the interpolation is not
    /// multi-line under C# 10, so the switch-case brace phase leaves the switch unbraced and on its line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-044")]
    public void SwitchSectionWithStatementAndReturnInInterpolationStaysUnbracedInCSharp10()
    {
        AssertKept("var s = $\"a {F(() => { switch (x) { case 1: G(); return 1; default: return 2; } })} b\";", parameters: "int x");
    }

    /// <summary>
    /// Verifies that an expression-bodied local function in a lambda in the interpolation still gets its block body under
    /// C# 10, on the line of the interpolation
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-044")]
    public void LocalFunctionInInterpolationGetsBlockBodyOnItsLineInCSharp10()
    {
        AssertResult("var s = $\"a {F(() => { int G() => 1; return G(); })} b\";",
                     "var s = $\"a {F(() => { int G() {return 1; } return G(); })} b\";");
    }

    /// <summary>
    /// Verifies that the constraint clause of a local function in the interpolation stays on the line of its declaration
    /// under C# 10
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-044")]
    public void LocalFunctionConstraintInInterpolationKeepsItsLineInCSharp10()
    {
        AssertResult("var s = $\"a {F(() => { int G<T>() where T : class => 1; return G<string>(); })} b\";",
                     "var s = $\"a {F(() => { int G<T>() where T : class {return 1; } return G<string>(); })} b\";");
    }

    /// <summary>
    /// Verifies that the attribute list of a local function in the interpolation stays on the line of its declaration under
    /// C# 10
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-044")]
    public void LocalFunctionAttributeInInterpolationKeepsItsLineInCSharp10()
    {
        AssertResult("var s = $\"a {F(() => { [A] int G() => 1; return G(); })} b\";",
                     "var s = $\"a {F(() => { [A] int G() {return 1; } return G(); })} b\";");
    }

    /// <summary>
    /// Verifies that a <c>try</c> statement with a <c>catch</c> clause in the interpolation keeps its line under C# 10
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-044")]
    public void TryCatchInInterpolationKeepsItsLineInCSharp10()
    {
        AssertKept("var s = $\"a {F(() => { try { return 1; } catch { return 2; } })} b\";");
    }

    /// <summary>
    /// Verifies that an embedded <c>using</c> statement in the interpolation keeps its unbraced body and its line under
    /// C# 10
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-044")]
    public void UsingStatementInInterpolationKeepsItsLineInCSharp10()
    {
        AssertKept("var s = $\"a {F(() => { using (var m = new MemoryStream()) return 1; })} b\";");
    }

    /// <summary>
    /// Verifies that a lambda block nested in an expression-bodied lambda in the interpolation keeps its line under C# 10
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-044")]
    public void NestedLambdaBlockInInterpolationKeepsItsLineInCSharp10()
    {
        AssertKept("var s = $\"a {F(() => F(() => { return 1; }))} b\";");
    }

    /// <summary>
    /// Verifies that an object initializer in the interpolation keeps its line under C# 10
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-044")]
    public void ObjectInitializerInInterpolationKeepsItsLineInCSharp10()
    {
        AssertKept("var s = $\"a {new P { A = 1, B = 2 }.A} b\";");
    }

    /// <summary>
    /// Verifies that a nested object initializer in the interpolation keeps its line under C# 10
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-044")]
    public void NestedObjectInitializerInInterpolationKeepsItsLineInCSharp10()
    {
        AssertKept("var s = $\"a {new P { Q = { A = 1 } }.Q.A} b\";");
    }

    /// <summary>
    /// Verifies that a collection initializer in the interpolation keeps its line under C# 10
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-044")]
    public void CollectionInitializerInInterpolationKeepsItsLineInCSharp10()
    {
        AssertKept("var s = $\"a {new List<int> { 1, 2 }.Count} b\";");
    }

    /// <summary>
    /// Verifies that an anonymous object creation in the interpolation keeps its line under C# 10
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-044")]
    public void AnonymousObjectInInterpolationKeepsItsLineInCSharp10()
    {
        AssertKept("var s = $\"a {new { A = 1, B = 2 }.A} b\";");
    }

    /// <summary>
    /// Verifies that a nested anonymous object creation in the interpolation keeps its line under C# 10
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-044")]
    public void NestedAnonymousObjectInInterpolationKeepsItsLineInCSharp10()
    {
        AssertKept("var s = $\"a {new { A = 1, B = new { C = 2 } }.B.C} b\";");
    }

    /// <summary>
    /// Verifies that the initializer of a <c>with</c> expression in the interpolation keeps its line under C# 10
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-044")]
    public void WithInitializerInInterpolationKeepsItsLineInCSharp10()
    {
        AssertKept("var s = $\"a {(r with { X = 1 }).X} b\";", parameters: "R r");
    }

    /// <summary>
    /// Verifies that a switch expression in the interpolation keeps its line under C# 10
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-044")]
    public void SwitchExpressionInInterpolationKeepsItsLineInCSharp10()
    {
        AssertKept("var s = $\"a {x switch { 1 => \"one\", _ => \"other\" }} b\";", parameters: "int x");
    }

    /// <summary>
    /// Verifies that a switch expression arm with a <c>when</c> clause in the interpolation keeps its line under C# 10
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-044")]
    public void SwitchExpressionArmWithWhenInInterpolationKeepsItsLineInCSharp10()
    {
        AssertKept("var s = $\"a {o switch { int i when i > 1 => 1, _ => 2 }} b\";", parameters: "object o");
    }

    /// <summary>
    /// Verifies that the attribute list of a lambda in the interpolation stays on the line of the lambda under C# 10
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-044")]
    public void LambdaAttributeInInterpolationKeepsItsLineInCSharp10()
    {
        AssertKept("Func<int> f = null;\nvar s = $\"a {(f = [A] () => 1)} b\";");
    }

    /// <summary>
    /// Verifies that a verbatim interpolated string nested in an interpolation of a non-verbatim one keeps the anonymous
    /// method in its own interpolation on one line under C# 10
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-044")]
    public void VerbatimStringNestedInNonVerbatimInterpolationKeepsItsLineInCSharp10()
    {
        AssertKept("var s = $\"a {$@\"x {F(delegate { return 1; })}\"} b\";");
    }

    /// <summary>
    /// Verifies that a non-verbatim interpolated string nested in an interpolation of a verbatim one keeps the anonymous
    /// method in its own interpolation on one line under C# 10
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-044")]
    public void NonVerbatimStringNestedInVerbatimInterpolationKeepsItsLineInCSharp10()
    {
        AssertKept("var s = $@\"a {$\"x {F(delegate { return 1; })}\"} b\";");
    }

    /// <summary>
    /// Verifies that an interpolation of a verbatim interpolated string written <c>@$</c> is laid out as the other
    /// requirements state under C# 10
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-044")]
    public void AtDollarVerbatimInterpolationIsBrokenInCSharp10()
    {
        AssertResult("var s = @$\"a {F(delegate { return 1; })} b\";",
                     "var s = @$\"a {F(delegate\n{\n    return 1;\n})} b\";");
    }

    /// <summary>
    /// Verifies that a property pattern in the interpolation stays on one line under C# 10
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-044")]
    public void PropertyPatternInInterpolationKeepsItsLineInCSharp10()
    {
        AssertKept("var s = $\"a {x is { Length: 1 }} b\";", parameters: "string x");
    }

    /// <summary>
    /// Verifies that an array initializer in the interpolation stays on one line under C# 10
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-044")]
    public void ArrayInitializerInInterpolationKeepsItsLineInCSharp10()
    {
        AssertKept("var s = $\"a {new int[] { 1, 2 }.Length} b\";");
    }

    /// <summary>
    /// Verifies that a query expression in the interpolation stays on one line under C# 10
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-044")]
    public void QueryExpressionInInterpolationKeepsItsLineInCSharp10()
    {
        AssertKept("var s = $\"a {(from x in xs where x > 1 select x).Count()} b\";", parameters: "int[] xs");
    }

    /// <summary>
    /// Verifies that a comment behind the expression of the interpolation stays on its line under C# 10
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-044")]
    public void CommentInInterpolationKeepsItsLineInCSharp10()
    {
        AssertKept("var s = $\"a {x /* c */} b\";", parameters: "int x");
    }

    /// <summary>
    /// Verifies that a delimited documentation comment in front of the expression of the interpolation stays on its line
    /// under C# 10
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-044")]
    public void DelimitedDocumentationCommentInInterpolationKeepsItsLineInCSharp10()
    {
        AssertKept("var s = $\"a {/** d */ x} b\";", parameters: "int x");
    }

    /// <summary>
    /// Verifies that a switch with a multi-line section in a lambda in the interpolation is left as written under C# 10:
    /// the switch-case brace phase inserts no braces, and no line break is written into the interpolation
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-044")]
    [Requirement("FMT-PIPE-003")]
    [Requirement("FMT-SWITCH-017")]
    public void SwitchSectionWithTwoStatementsInInterpolationStaysAsWrittenInCSharp10()
    {
        AssertKept("var s = $\"a {F(() => { switch (x) { case 1: G(); G(); return 1; default: return 2; } })} b\";", parameters: "int x");
    }

    /// <summary>
    /// Verifies that a delimited documentation comment in front of a statement that follows another statement in the
    /// interpolation stays on its line under C# 10: the documentation-comment phase starts no new line there
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-044")]
    public void DocumentationCommentInFrontOfStatementInInterpolationKeepsItsLineInCSharp10()
    {
        AssertKept("var s = $\"a {F(() => { var a = 1; /** d */ return a; })} b\";");
    }

    /// <summary>
    /// Verifies that a delimited documentation comment in front of a declaration statement that follows another statement
    /// in the interpolation stays on its line under C# 10
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-044")]
    public void DocumentationCommentInFrontOfDeclarationStatementInInterpolationKeepsItsLineInCSharp10()
    {
        AssertKept("var s = $\"a {F(() => { var a = 1; /** d */ var b = 2; return a + b; })} b\";");
    }

    /// <summary>
    /// Verifies that a delimited documentation comment in front of a statement that follows a local function's block in the
    /// interpolation stays on its line under C# 10
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-044")]
    public void DocumentationCommentBehindLocalFunctionBlockInInterpolationKeepsItsLineInCSharp10()
    {
        AssertKept("var s = $\"a {F(() => { int G() { return 1; } /** d */ return G(); })} b\";");
    }

    /// <summary>
    /// Verifies that a delimited documentation comment in front of a local function that follows a statement in the
    /// interpolation stays on its line under C# 10, while the local function still gets its block body
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-044")]
    public void DocumentationCommentInFrontOfLocalFunctionInInterpolationKeepsItsLineInCSharp10()
    {
        AssertResult("var s = $\"a {F(() => { var a = 1; /** d */ int G() => a; return G(); })} b\";",
                     "var s = $\"a {F(() => { var a = 1; /** d */ int G() {return a; } return G(); })} b\";");
    }

    /// <summary>
    /// Verifies that a delimited documentation comment in front of an object-initializer member that follows a comma in the
    /// interpolation stays on its line under C# 10
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-044")]
    public void DocumentationCommentInFrontOfObjectInitializerMemberInInterpolationKeepsItsLineInCSharp10()
    {
        AssertKept("var s = $\"a {new P { A = 1, /** d */ B = 2 }.A} b\";");
    }

    /// <summary>
    /// Verifies that a delimited documentation comment in front of an anonymous-object member that follows a comma in the
    /// interpolation stays on its line under C# 10
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-044")]
    public void DocumentationCommentInFrontOfAnonymousObjectMemberInInterpolationKeepsItsLineInCSharp10()
    {
        AssertKept("var s = $\"a {new { A = 1, /** d */ B = 2 }.A} b\";");
    }

    /// <summary>
    /// Verifies that a delimited documentation comment in front of an argument that follows a comma in the interpolation
    /// stays on its line under C# 10
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-044")]
    public void DocumentationCommentInFrontOfArgumentInInterpolationKeepsItsLineInCSharp10()
    {
        AssertKept("var s = $\"a {K(1, /** d */ 2)} b\";");
    }

    /// <summary>
    /// Verifies that a delimited documentation comment in front of a switch label that follows a section in the
    /// interpolation stays on its line under C# 10
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-044")]
    public void DocumentationCommentInFrontOfSwitchLabelInInterpolationKeepsItsLineInCSharp10()
    {
        AssertKept("var s = $\"a {F(() => { switch (x) { case 1: return 1; /** d */ default: return 2; } })} b\";", parameters: "int x");
    }

    /// <summary>
    /// Verifies that an <c>if</c> body led by a delimited documentation comment in the interpolation stays unbraced and on
    /// its line under C# 10: the inserted <c>{</c> would need a line break in front of the comment, which the interpolation
    /// cannot hold
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-044")]
    public void IfBodyLedByDocumentationCommentInInterpolationKeepsItsLineInCSharp10()
    {
        AssertKept("var s = $\"a {F(() => { if (b) /** d */ return 1; return 2; })} b\";", parameters: "bool b");
    }

    /// <summary>
    /// Verifies that an <c>else</c> body led by a delimited documentation comment in the interpolation stays unbraced and on
    /// its line under C# 10, while the <c>if</c> body, which no comment leads, still gets its braces
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-044")]
    public void ElseBodyLedByDocumentationCommentInInterpolationKeepsItsLineInCSharp10()
    {
        AssertResult("var s = $\"a {F(() => { if (b) return 1; else /** d */ return 2; })} b\";",
                     "var s = $\"a {F(() => { if (b) {return 1; }else /** d */ return 2; })} b\";",
                     "bool b");
    }

    /// <summary>
    /// Verifies that an <c>if</c> body followed by a statement led by a delimited documentation comment in the interpolation
    /// stays unbraced and on its line under C# 10: the inserted <c>}</c> would need a line break in front of the comment,
    /// which the interpolation cannot hold
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-044")]
    public void IfBodyFollowedByDocumentationCommentInInterpolationKeepsItsLineInCSharp10()
    {
        AssertKept("var s = $\"a {F(() => { if (b) return 1; /** d */ return 2; })} b\";", parameters: "bool b");
    }

    /// <summary>
    /// Verifies that a delimited documentation comment directly behind the opening brace of a lambda block in the
    /// interpolation stays on its line under C# 10
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-044")]
    public void DocumentationCommentBehindOpeningBraceInInterpolationKeepsItsLineInCSharp10()
    {
        AssertKept("var s = $\"a {F(() => { /** d */ return 1; })} b\";");
    }

    /// <summary>
    /// Verifies that a delimited documentation comment directly behind the opening brace of an object initializer in the
    /// interpolation stays on its line under C# 10
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-044")]
    public void DocumentationCommentBehindInitializerBraceInInterpolationKeepsItsLineInCSharp10()
    {
        AssertKept("var s = $\"a {new P { /** d */ A = 1 }.A} b\";");
    }

    /// <summary>
    /// Verifies that a delimited documentation comment in front of the closing brace of a lambda block in the interpolation
    /// stays on its line under C# 10
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-044")]
    public void DocumentationCommentInFrontOfClosingBraceInInterpolationKeepsItsLineInCSharp10()
    {
        AssertKept("var s = $\"a {F(() => { return 1; /** d */ })} b\";");
    }

    /// <summary>
    /// Verifies that a delimited documentation comment in front of the closing brace of the interpolation stays on its line
    /// under C# 10
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-044")]
    public void DocumentationCommentInFrontOfInterpolationEndKeepsItsLineInCSharp10()
    {
        AssertKept("var s = $\"a {K(1, 2) /** d */} b\";");
    }

    /// <summary>
    /// Verifies that a delimited documentation comment in front of a binary operator in the interpolation stays on its line
    /// under C# 10
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-044")]
    public void DocumentationCommentInFrontOfBinaryOperatorInInterpolationKeepsItsLineInCSharp10()
    {
        AssertKept("var s = $\"a {x /** d */ + 1} b\";", parameters: "int x");
    }

    /// <summary>
    /// Verifies that an <c>if</c> body behind an ordinary delimited comment in the interpolation gets its braces on its line
    /// under C# 10, since the comment trails the <c>)</c> and needs no line break
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-044")]
    public void IfBodyBehindOrdinaryCommentInInterpolationGetsBracesOnItsLineInCSharp10()
    {
        AssertResult("var s = $\"a {F(() => { if (b) /* c */ return 1; return 2; })} b\";",
                     "var s = $\"a {F(() => { if (b) /* c */ {return 1; }return 2; })} b\";",
                     "bool b");
    }

    /// <summary>
    /// Verifies that an ordinary delimited comment between two statements in the interpolation stays on its line under
    /// C# 10
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-044")]
    public void OrdinaryCommentBetweenStatementsInInterpolationKeepsItsLineInCSharp10()
    {
        AssertKept("var s = $\"a {F(() => { var a = 1; /* c */ var b = 2; return a + b; })} b\";");
    }

    /// <summary>
    /// Verifies that a braced <c>if</c> body followed by a statement behind an ordinary delimited comment in the
    /// interpolation gets its braces on its line under C# 10
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-044")]
    public void IfBodyFollowedByOrdinaryCommentInInterpolationGetsBracesOnItsLineInCSharp10()
    {
        AssertResult("var s = $\"a {F(() => { if (b) return 1; /* c */ return 2; })} b\";",
                     "var s = $\"a {F(() => { if (b) {return 1; /* c */ }return 2; })} b\";",
                     "bool b");
    }

    /// <summary>
    /// Verifies that a delimited documentation comment in front of a statement that follows another statement in an
    /// interpolation of a non-verbatim interpolated string starts a new line under C# 11
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-044")]
    public void DocumentationCommentInFrontOfStatementInInterpolationIsBrokenInCSharp11()
    {
        const string expected = """
                                using System;

                                class C
                                {
                                    int F(Func<int> f)
                                    {
                                        return f();
                                    }

                                    string M()
                                    {
                                        var s = $"a {F(() =>
                                                       {
                                                           var a = 1;

                                                           /** d */ return a;
                                                       })} b";

                                        return s;
                                    }
                                }
                                """;

        AssertRuleResult(Build("var s = $\"a {F(() => { var a = 1; /** d */ return a; })} b\";", string.Empty), expected, At(LanguageVersion.CSharp11));
    }

    /// <summary>
    /// Verifies that an <c>if</c> body led by a delimited documentation comment in an interpolation of a non-verbatim
    /// interpolated string gets its braces on lines of their own under C# 11
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-044")]
    public void IfBodyLedByDocumentationCommentInInterpolationIsBracedInCSharp11()
    {
        const string expected = """
                                using System;

                                class C
                                {
                                    int F(Func<int> f)
                                    {
                                        return f();
                                    }

                                    string M(bool b)
                                    {
                                        var s = $"a {F(() =>
                                                       {
                                                           if (b)
                                                           {
                                                               /** d */ return 1;
                                                           }

                                                           return 2;
                                                       })} b";

                                        return s;
                                    }
                                }
                                """;

        AssertRuleResult(Build("var s = $\"a {F(() => { if (b) /** d */ return 1; return 2; })} b\";", "bool b"), expected, At(LanguageVersion.CSharp11));
    }

    /// <summary>
    /// Verifies that a lambda block in an interpolation of a non-verbatim interpolated string is broken under C# 11
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-044")]
    public void LambdaBlockInInterpolationIsBrokenInCSharp11()
    {
        AssertResult("var s = $\"a {F(() => { return 1; })} b\";",
                     "var s = $\"a {F(() =>\n               {\n                   return 1;\n               })} b\";",
                     languageVersion: LanguageVersion.CSharp11);
    }

    /// <summary>
    /// Verifies that an object initializer in an interpolation of a non-verbatim interpolated string is broken under C# 11
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-044")]
    public void ObjectInitializerInInterpolationIsBrokenInCSharp11()
    {
        AssertResult("var s = $\"a {new P { A = 1, B = 2 }.A} b\";",
                     "var s = $\"a {new P\n             {\n                 A = 1,\n                 B = 2\n             }.A} b\";",
                     languageVersion: LanguageVersion.CSharp11);
    }

    /// <summary>
    /// Verifies that a switch expression in an interpolation of a non-verbatim interpolated string is broken under C# 11
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-044")]
    public void SwitchExpressionInInterpolationIsBrokenInCSharp11()
    {
        AssertResult("var s = $\"a {x switch { 1 => \"one\", _ => \"other\" }} b\";",
                     "var s = $\"a {x switch\n             {\n                 1 => \"one\", _ => \"other\"\n             }} b\";",
                     "int x",
                     LanguageVersion.CSharp11);
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatter.FormatDocumentAsync"/> keeps the interpolation on one line in a C# 10 project
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-BREAK-044")]
    public async Task FormatDocumentAsyncKeepsInterpolationOnOneLineInCSharp10Project()
    {
        foreach (var endOfLine in _lineEndings)
        {
            using (var workspace = new AdhocWorkspace())
            {
                var document = CreateDocument(workspace, NormalizeLineEndings(SpecInput, endOfLine), LanguageVersion.CSharp10);
                var result = (await ReihitsuFormatter.FormatDocumentAsync(document, TestContext.CancellationToken)).Output;
                var resultText = (await result.GetTextAsync(TestContext.CancellationToken)).ToString();

                Assert.AreEqual(NormalizeLineEndings(SpecCSharp10Expected, endOfLine), resultText, $"FormatDocumentAsync output mismatch under {DescribeLineEnding(endOfLine)} line endings.");
            }
        }
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatter.FormatNodeInDocumentAsync"/>, as a code fix calls it, keeps the interpolation
    /// on one line in a C# 10 project
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-BREAK-044")]
    public async Task FormatNodeInDocumentAsyncKeepsInterpolationOnOneLineInCSharp10Project()
    {
        foreach (var endOfLine in _lineEndings)
        {
            using (var workspace = new AdhocWorkspace())
            {
                var document = CreateDocument(workspace, NormalizeLineEndings(SpecInput, endOfLine), LanguageVersion.CSharp10);
                var root = await document.GetSyntaxRootAsync(TestContext.CancellationToken);
                var target = root?.DescendantNodes().OfType<MethodDeclarationSyntax>().First(method => method.Identifier.Text == "M");

                Assert.IsNotNull(target);

                var result = (await ReihitsuFormatter.FormatNodeInDocumentAsync(document, target, TestContext.CancellationToken)).Output;
                var resultText = (await result.GetTextAsync(TestContext.CancellationToken)).ToString();

                Assert.AreEqual(NormalizeLineEndings(SpecCSharp10Expected, endOfLine), resultText, $"FormatNodeInDocumentAsync output mismatch under {DescribeLineEnding(endOfLine)} line endings.");
            }
        }
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

    /// <summary>
    /// Creates a document in a project with the given language version
    /// </summary>
    /// <param name="workspace">The workspace</param>
    /// <param name="source">The source text</param>
    /// <param name="languageVersion">The language version of the project</param>
    /// <returns>The document</returns>
    private static Document CreateDocument(AdhocWorkspace workspace, string source, LanguageVersion languageVersion)
    {
        var project = workspace.AddProject("TestProject", LanguageNames.CSharp)
                               .WithParseOptions(CSharpParseOptions.Default.WithLanguageVersion(languageVersion));

        return project.AddDocument("Test.cs", SourceText.From(source));
    }

    /// <summary>
    /// Builds a formatted class whose method <c>M</c> holds the given statements, followed by a blank line and <c>return s;</c>
    /// </summary>
    /// <param name="statements">The statements, one per line, without indentation</param>
    /// <param name="parameters">The parameter list of <c>M</c></param>
    /// <returns>The source text</returns>
    private static string Build(string statements, string parameters)
    {
        var body = string.Join("\n", statements.Split('\n').Select(line => $"        {line}"));

        return $$"""
                 using System;

                 class C
                 {
                     int F(Func<int> f)
                     {
                         return f();
                     }

                     string M({{parameters}})
                     {
                 {{body}}

                         return s;
                     }
                 }
                 """;
    }

    /// <summary>
    /// Verifies that the statements, already formatted outside their interpolations, come back unchanged
    /// </summary>
    /// <param name="statements">The statements, one per line, without indentation</param>
    /// <param name="languageVersion">The language version</param>
    /// <param name="parameters">The parameter list of <c>M</c></param>
    private static void AssertKept(string statements, LanguageVersion languageVersion = LanguageVersion.CSharp10, string parameters = "")
    {
        AssertRuleResult(Build(statements, parameters), null, At(languageVersion));
    }

    /// <summary>
    /// Verifies that the statements are formatted to the expected statements
    /// </summary>
    /// <param name="statements">The statements, one per line, without indentation</param>
    /// <param name="expectedStatements">The expected statements, one per line, indented relative to the method body</param>
    /// <param name="parameters">The parameter list of <c>M</c></param>
    /// <param name="languageVersion">The language version</param>
    private static void AssertResult(string statements, string expectedStatements, string parameters = "", LanguageVersion languageVersion = LanguageVersion.CSharp10)
    {
        AssertRuleResult(Build(statements, parameters), Build(expectedStatements, parameters), At(languageVersion));
    }

    #endregion // Methods
}