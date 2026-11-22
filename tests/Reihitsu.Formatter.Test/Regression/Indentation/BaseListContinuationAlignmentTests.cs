using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Indentation;

/// <summary>
/// Full-pipeline tests for a line inside a type of a base list, which a comment keeps from being joined: it aligns with
/// that type's first token, whatever the number of types in the list (FMT-INDENT-013)
/// </summary>
[TestClass]
public class BaseListContinuationAlignmentTests : FormatterTestsBase
{
    #region Tests

    /// <summary>
    /// Verifies the specification example: a line inside the only type of a class base list and inside an enum's
    /// underlying type aligns with that type's first token
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-013")]
    public void LineInsideOnlyBaseTypeAndEnumUnderlyingTypeAlignsWithTheType()
    {
        const string input = """
                             class B : System. // c
                                 IDisposable
                             {
                                 public void Dispose()
                                 {
                                 }
                             }

                             enum E : System. // c
                                 Int32
                             {
                                 A
                             }
                             """;
        const string expected = """
                                class B : System. // c
                                          IDisposable
                                {
                                    public void Dispose()
                                    {
                                    }
                                }

                                enum E : System. // c
                                         Int32
                                {
                                    A
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the specification example and the reported input: a line inside the later type of a base list with two
    /// types aligns with that type's first token
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-013")]
    public void LineInsideLaterBaseTypeAlignsWithTheType()
    {
        const string input = """
                             class C : System.IDisposable,
                                       System. // c
                                 ICloneable
                             {
                                 public void Dispose()
                                 {
                                 }

                                 public object Clone()
                                 {
                                     return null;
                                 }
                             }
                             """;
        const string expected = """
                                class C : System.IDisposable,
                                          System. // c
                                          ICloneable
                                {
                                    public void Dispose()
                                    {
                                    }

                                    public object Clone()
                                    {
                                        return null;
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a line inside the later type of a base list inside a namespace aligns with that type's first token
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-013")]
    public void LineInsideLaterBaseTypeInNamespaceAlignsWithTheType()
    {
        const string input = """
                             namespace N
                             {
                                 class C : System.IDisposable,
                                           System. // c
                                 ICloneable
                                 {
                                     public void Dispose()
                                     {
                                     }

                                     public object Clone()
                                     {
                                         return null;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                namespace N
                                {
                                    class C : System.IDisposable,
                                              System. // c
                                              ICloneable
                                    {
                                        public void Dispose()
                                        {
                                        }

                                        public object Clone()
                                        {
                                            return null;
                                        }
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a line inside the first type of a base list with two types aligns with that type's first token
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-013")]
    public void LineInsideFirstOfTwoBaseTypesAlignsWithTheType()
    {
        const string input = """
                             class D : System. // c
                                 IDisposable, System.ICloneable
                             {
                                 public void Dispose()
                                 {
                                 }

                                 public object Clone()
                                 {
                                     return null;
                                 }
                             }
                             """;
        const string expected = """
                                class D : System. // c
                                          IDisposable, System.ICloneable
                                {
                                    public void Dispose()
                                    {
                                    }

                                    public object Clone()
                                    {
                                        return null;
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a line inside the only type of an interface base list aligns with that type's first token
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-013")]
    public void LineInsideInterfaceBaseTypeAlignsWithTheType()
    {
        const string input = """
                             interface J : System. // c
                                 IDisposable
                             {
                                 void M();
                             }
                             """;
        const string expected = """
                                interface J : System. // c
                                              IDisposable
                                {
                                    void M();
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a line inside the only type of a struct base list aligns with that type's first token
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-013")]
    public void LineInsideStructBaseTypeAlignsWithTheType()
    {
        const string input = """
                             struct S : System. // c
                                 IDisposable
                             {
                                 public void Dispose()
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                struct S : System. // c
                                           IDisposable
                                {
                                    public void Dispose()
                                    {
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a line inside the only type of a record base list aligns with that type's first token
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-013")]
    public void LineInsideRecordBaseTypeAlignsWithTheType()
    {
        const string input = """
                             record R : System. // c
                                 IDisposable
                             {
                                 public void Dispose()
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                record R : System. // c
                                           IDisposable
                                {
                                    public void Dispose()
                                    {
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the argument line of a primary-constructor base argument list keeps its alignment after the opening
    /// parenthesis
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-012")]
    public void PrimaryConstructorBaseArgumentStaysAlignedAfterParenthesis()
    {
        const string input = """
                             class Base(int x);

                             class P(int x) : Base( // c
                                                   x);
                             """;

        AssertRuleResult(input);
    }

    #endregion // Tests
}