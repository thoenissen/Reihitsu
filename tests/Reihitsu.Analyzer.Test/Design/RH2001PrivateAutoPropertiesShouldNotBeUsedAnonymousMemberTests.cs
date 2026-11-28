using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Design;
using Reihitsu.Analyzer.Rules.Design;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Design;

/// <summary>
/// Test methods for <see cref="RH2001PrivateAutoPropertiesShouldNotBeUsedCodeFixProvider"/> on anonymous-object members
/// that take their name from a reference to the property, whose name the fix writes explicitly
/// </summary>
[TestClass]
public class RH2001PrivateAutoPropertiesShouldNotBeUsedAnonymousMemberTests : BatchCodeFixTestsBase<RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzer, RH2001PrivateAutoPropertiesShouldNotBeUsedCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that the fix writes the name of an anonymous-object member that the bare reference to the property names,
    /// in an expression body
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH2001-012")]
    public async Task VerifyFixWritesTheNameOfAMemberNamedByTheBareReference(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    object M() => new { Value };
                                }
                                """;

        const string resultData = """
                                  class C
                                  {
                                      private int _value;

                                      object M() => new { Value = _value };
                                  }
                                  """;

        await Verify(ConvertEndOfLine(testData, endOfLine), ConvertEndOfLine(resultData, endOfLine));
    }

    /// <summary>
    /// Verifies that the fix writes the name of an anonymous-object member that the bare reference names and that code
    /// reads by that name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-012")]
    public async Task VerifyFixWritesTheNameOfAMemberNamedByTheBareReferenceThatCodeReads()
    {
        const string testData = """
                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    int Get()
                                    {
                                        var pair = new { Value };

                                        return pair.Value;
                                    }
                                }
                                """;

        const string resultData = """
                                  class Sample
                                  {
                                      private int _value;

                                      int Get()
                                      {
                                          var pair = new { Value = _value };

                                          return pair.Value;
                                      }
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies that the fix writes the name of an anonymous-object member that a reference qualified with
    /// <see langword="this"/> names
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-012")]
    public async Task VerifyFixWritesTheNameOfAMemberNamedByAThisQualifiedReference()
    {
        const string testData = """
                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    int Get()
                                    {
                                        var x = new { this.Value };

                                        return x.Value;
                                    }
                                }
                                """;

        const string resultData = """
                                  class Sample
                                  {
                                      private int _value;

                                      int Get()
                                      {
                                          var x = new { Value = this._value };

                                          return x.Value;
                                      }
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies that the fix writes the name of an anonymous-object member that a reference through another instance of
    /// the type names
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-012")]
    public async Task VerifyFixWritesTheNameOfAMemberNamedByAReferenceThroughAnotherInstance()
    {
        const string testData = """
                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    int Get(Sample other)
                                    {
                                        var x = new { other.Value };

                                        return x.Value;
                                    }
                                }
                                """;

        const string resultData = """
                                  class Sample
                                  {
                                      private int _value;

                                      int Get(Sample other)
                                      {
                                          var x = new { Value = other._value };

                                          return x.Value;
                                      }
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies the specification example of a member access and a conditional access whose members are read by their
    /// names, next to a member with a name of its own that is only renamed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-012")]
    public async Task VerifyFixWritesTheNamesOfMembersNamedByAMemberAccessAndAConditionalAccess()
    {
        const string testData = """
                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    int Get(Sample other)
                                    {
                                        var item = new { this.Value, Other = other?.Value };
                                        var copy = new { other?.Value };

                                        return item.Value + (item.Other ?? 0) + (copy.Value ?? 0);
                                    }
                                }
                                """;

        const string resultData = """
                                  class Sample
                                  {
                                      private int _value;

                                      int Get(Sample other)
                                      {
                                          var item = new { Value = this._value, Other = other?._value };
                                          var copy = new { Value = other?._value };

                                          return item.Value + (item.Other ?? 0) + (copy.Value ?? 0);
                                      }
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies that the fix writes the name of a member of an anonymous object in a LINQ projection, which a later
    /// projection reads
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-012")]
    public async Task VerifyFixWritesTheNameOfAMemberOfAnAnonymousObjectInAQueryProjection()
    {
        const string testData = """
                                using System.Linq;

                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    int[] Get(int[] items)
                                    {
                                        return items.Select(item => new { Value, item }).Select(pair => pair.Value + pair.item).ToArray();
                                    }
                                }
                                """;

        const string resultData = """
                                  using System.Linq;

                                  class Sample
                                  {
                                      private int _value;

                                      int[] Get(int[] items)
                                      {
                                          return items.Select(item => new { Value = _value, item }).Select(pair => pair.Value + pair.item).ToArray();
                                      }
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies that the fix writes the name of an anonymous-object member whose name <c>ToString()</c> prints, in a block
    /// body
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-012")]
    public async Task VerifyFixWritesTheNameOfAMemberThatToStringPrints()
    {
        const string testData = """
                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    string Get()
                                    {
                                        return new { Value }.ToString();
                                    }
                                }
                                """;

        const string resultData = """
                                  class Sample
                                  {
                                      private int _value;

                                      string Get()
                                      {
                                          return new { Value = _value }.ToString();
                                      }
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies that the fix writes the name of an anonymous-object member whose name <c>ToString()</c> prints, in an
    /// expression body
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-012")]
    public async Task VerifyFixWritesTheNameOfAMemberThatToStringPrintsInAnExpressionBody()
    {
        const string testData = """
                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    string Describe() => new { Value }.ToString();
                                }
                                """;

        const string resultData = """
                                  class Sample
                                  {
                                      private int _value;

                                      string Describe() => new { Value = _value }.ToString();
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies the specification example of Fix All over two properties whose names <c>ToString()</c> prints, which writes
    /// the name of both members in one pass
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-012")]
    public async Task VerifyFixAllWritesTheNamesOfTheMembersOfTwoProperties()
    {
        const string testData = """
                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    private string {|RH2001:Name|} { get; }

                                    string Describe() => new { Value, Name }.ToString();
                                }
                                """;

        const string resultData = """
                                  class Sample
                                  {
                                      private int _value;

                                      private readonly string _name;

                                      string Describe() => new { Value = _value, Name = _name }.ToString();
                                  }
                                  """;

        await Verify(testData, resultData, static test => test.NumberOfFixAllIterations = 1);
    }

    /// <summary>
    /// Verifies that Fix All writes the name of a member whose access chain ends with one fixed property and passes through
    /// another, which is only renamed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-012")]
    public async Task VerifyFixAllWritesTheNameOfAMemberWhoseChainPassesThroughAnotherFixedProperty()
    {
        const string testData = """
                                class Sample
                                {
                                    private Sample {|RH2001:Inner|} { get; set; }

                                    private int {|RH2001:Value|} { get; set; }

                                    int? Get(Sample other)
                                    {
                                        var item = new { other?.Inner.Value };

                                        return item.Value;
                                    }
                                }
                                """;

        const string resultData = """
                                  class Sample
                                  {
                                      private Sample _inner;

                                      private int _value;

                                      int? Get(Sample other)
                                      {
                                          var item = new { Value = other?._inner._value };

                                          return item.Value;
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, static test => test.NumberOfFixAllIterations = 1);
    }

    /// <summary>
    /// Verifies that Fix All writes the name of a member that the last of two fixed properties names, where the first
    /// property is the receiver of the member's expression and is only renamed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-012")]
    public async Task VerifyFixAllWritesTheNameOfAMemberNamedByAPropertyReadThroughAnotherFixedProperty()
    {
        const string testData = """
                                class Sample
                                {
                                    private Sample {|RH2001:Value|} { get; set; }

                                    private string {|RH2001:Name|} { get; set; }

                                    string Get()
                                    {
                                        var item = new { Value.Name };

                                        return item.Name;
                                    }
                                }
                                """;

        const string resultData = """
                                  class Sample
                                  {
                                      private Sample _value;

                                      private string _name;

                                      string Get()
                                      {
                                          var item = new { Name = _value._name };

                                          return item.Name;
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, static test => test.NumberOfFixAllIterations = 1);
    }

    /// <summary>
    /// Verifies the specification example of a reference that the renamer qualifies with <see langword="this"/> next to a
    /// local named like the field, whose member keeps its name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-012")]
    public async Task VerifyFixWritesTheNameInFrontOfAThisQualifiedReferenceNextToALocalNamedLikeTheField()
    {
        const string testData = """
                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    int Get()
                                    {
                                        var _value = 1;
                                        var item = new { Value };

                                        return item.Value + _value;
                                    }
                                }
                                """;

        const string resultData = """
                                  class Sample
                                  {
                                      private int _value;

                                      int Get()
                                      {
                                          var _value = 1;
                                          var item = new { Value = this._value };

                                          return item.Value + _value;
                                      }
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies that the fix writes the name in front of a reference to a static property that the renamer qualifies with
    /// the type, next to a local named like the field
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-012")]
    public async Task VerifyFixWritesTheNameInFrontOfATypeQualifiedReferenceNextToALocalNamedLikeTheField()
    {
        const string testData = """
                                class Sample
                                {
                                    private static int {|RH2001:Value|} { get; set; }

                                    static object Get()
                                    {
                                        var _value = 1;

                                        return new { Value, Local = _value };
                                    }
                                }
                                """;

        const string resultData = """
                                  class Sample
                                  {
                                      private static int _value;

                                      static object Get()
                                      {
                                          var _value = 1;

                                          return new { Value = Sample._value, Local = _value };
                                      }
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies the specification example of a member whose name the compiler takes from another identifier, which is only
    /// renamed, next to a member with a name of its own
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-012")]
    public async Task VerifyFixOnlyRenamesAMemberWhoseNameComesFromAnotherIdentifier()
    {
        const string testData = """
                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    int Get()
                                    {
                                        var item = new { Value.ToString().Length, Item = Value };

                                        return item.Length + item.Item;
                                    }
                                }
                                """;

        const string resultData = """
                                  class Sample
                                  {
                                      private int _value;

                                      int Get()
                                      {
                                          var item = new { _value.ToString().Length, Item = _value };

                                          return item.Length + item.Item;
                                      }
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies that the fix writes the name of a member that the last of two references to the property names, where the
    /// property is the receiver of its own member, and only renames the receiver
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-012")]
    public async Task VerifyFixWritesTheNameOfAMemberNamedByThePropertyReadThroughItself()
    {
        const string testData = """
                                class Sample
                                {
                                    private Sample {|RH2001:Value|} { get; set; }

                                    Sample Get()
                                    {
                                        var item = new { Value.Value };

                                        return item.Value;
                                    }
                                }
                                """;

        const string resultData = """
                                  class Sample
                                  {
                                      private Sample _value;

                                      Sample Get()
                                      {
                                          var item = new { Value = _value._value };

                                          return item.Value;
                                      }
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies that the fix writes the name of a member of an anonymous object nested in another one, and leaves the
    /// outer member's own name alone
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-012")]
    public async Task VerifyFixWritesTheNameOfAMemberOfANestedAnonymousObject()
    {
        const string testData = """
                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    int Get()
                                    {
                                        var item = new { Inner = new { Value } };

                                        return item.Inner.Value;
                                    }
                                }
                                """;

        const string resultData = """
                                  class Sample
                                  {
                                      private int _value;

                                      int Get()
                                      {
                                          var item = new { Inner = new { Value = _value } };

                                          return item.Inner.Value;
                                      }
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies that the fix writes the name of a member whose anonymous type a <see langword="with"/> expression copies,
    /// and leaves the member named in the <see langword="with"/> expression as written
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-012")]
    public async Task VerifyFixWritesTheNameOfAMemberThatAWithExpressionSets()
    {
        const string testData = """
                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    int Get()
                                    {
                                        var item = new { Value };
                                        var copy = item with { Value = 3 };

                                        return copy.Value;
                                    }
                                }
                                """;

        const string resultData = """
                                  class Sample
                                  {
                                      private int _value;

                                      int Get()
                                      {
                                          var item = new { Value = _value };
                                          var copy = item with { Value = 3 };

                                          return copy.Value;
                                      }
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies that the fix writes the names of the members of the two anonymous objects a <see langword="join"/> clause
    /// compares, which have to stay of the same anonymous type
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-012")]
    public async Task VerifyFixWritesTheNamesOfTheMembersAJoinClauseCompares()
    {
        const string testData = """
                                using System.Linq;

                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    int Count(Sample[] left, Sample[] right)
                                    {
                                        return (from l in left
                                                join r in right on new { l.Value } equals new { r.Value }
                                                select l).Count();
                                    }
                                }
                                """;

        const string resultData = """
                                  using System.Linq;

                                  class Sample
                                  {
                                      private int _value;

                                      int Count(Sample[] left, Sample[] right)
                                      {
                                          return (from l in left
                                                  join r in right on new { Value = l._value } equals new { Value = r._value }
                                                  select l).Count();
                                      }
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies that the fix keeps the comments in front of and behind a member whose name it writes where they were
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-012")]
    public async Task VerifyFixKeepsTheCommentsAroundAMemberWhoseNameItWrites()
    {
        const string testData = """
                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    object Get() => new { /* a */ Value /* b */ };
                                }
                                """;

        const string resultData = """
                                  class Sample
                                  {
                                      private int _value;

                                      object Get() => new { /* a */ Value = _value /* b */ };
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies that the fix keeps a comment on the line above a member of a multi-line anonymous object creation whose name
    /// it writes, under LF and CRLF
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH2001-012")]
    public async Task VerifyFixKeepsACommentOnTheLineAboveAMemberWhoseNameItWrites(string endOfLine)
    {
        const string testData = """
                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    object Get()
                                    {
                                        return new
                                               {
                                                   // leading
                                                   Value,
                                                   Other = 1
                                               };
                                    }
                                }
                                """;

        const string resultData = """
                                  class Sample
                                  {
                                      private int _value;

                                      object Get()
                                      {
                                          return new
                                                 {
                                                     // leading
                                                     Value = _value,
                                                     Other = 1
                                                 };
                                      }
                                  }
                                  """;

        await Verify(ConvertEndOfLine(testData, endOfLine), ConvertEndOfLine(resultData, endOfLine));
    }

    /// <summary>
    /// Verifies that the fix writes the name of a member that a static property gives its name in the property's own
    /// initializer, which becomes the initializer of the field
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-012")]
    public async Task VerifyFixWritesTheNameOfAMemberInThePropertysOwnInitializer()
    {
        const string testData = """
                                class Sample
                                {
                                    private static object {|RH2001:Value|} { get; } = new { Value };
                                }
                                """;

        const string resultData = """
                                  class Sample
                                  {
                                      private static readonly object _value = new
                                                                              {
                                                                                  Value = _value
                                                                              };
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies that Fix All over two <c>partial</c> parts in two documents fixes both properties in one pass, when the fix
    /// in the first document writes member names in the second document that move the second property's declaration
    /// further than the gap in front of it; Fix All in document fixes one document per pass
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-012")]
    [Requirement("SHARED-FIX-002")]
    public async Task VerifyFixAllFixesAPropertyInAnotherPartialPartWhoseDeclarationTheFixOfTheFirstPartMoves()
    {
        const string source = """
                              partial class Sample
                              {
                                  private int {|RH2001:Value|} { get; set; }
                              }
                              """;
        const string fixedSource = """
                                   partial class Sample
                                   {
                                       private int _value;
                                   }
                                   """;
        const string otherSource = """
                                   partial class Sample
                                   {
                                       object First() => new { Value };

                                       object Second() => new { Value };

                                       object Third() => new { Value };

                                       private int {|RH2001:Name|} { get; set; }
                                   }
                                   """;
        const string fixedOtherSource = """
                                        partial class Sample
                                        {
                                            object First() => new { Value = _value };

                                            object Second() => new { Value = _value };

                                            object Third() => new { Value = _value };

                                            private int _name;
                                        }
                                        """;

        await Verify(source,
                     fixedSource,
                     static test =>
                            {
                                test.TestState.Sources.Add(("/0/Test1.cs", otherSource));
                                test.FixedState.Sources.Add(("/0/Test1.cs", fixedOtherSource));
                                test.NumberOfFixAllInDocumentIterations = 2;
                                test.NumberOfFixAllInProjectIterations = 1;
                                test.NumberOfFixAllIterations = 1;
                            });
    }

    /// <summary>
    /// Verifies the specification example of an anonymous-object member next to a tuple element in a withheld position,
    /// which keeps the fix withheld
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-012")]
    [Requirement("RH2001-011")]
    public async Task VerifyNoFixIsOfferedForAMemberNextToATupleElementInAWithheldPosition()
    {
        const string testData = """
                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    int Get()
                                    {
                                        var item = new { Value };
                                        var pair = (Value, 1);

                                        return item.Value + pair.Value;
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that a lambda parameter in a block body named like the field still withholds the fix for a rename in which
    /// a reference to the property names an anonymous-object member
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-012")]
    [Requirement("RH2001-011")]
    public async Task VerifyNoFixIsOfferedForAMemberWhenALambdaParameterInABlockBodyIsNamedLikeTheField()
    {
        const string testData = """
                                using System;

                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    Func<int, object> Get()
                                    {
                                        return _value =>
                                               {
                                                   return new { Value, Other = _value };
                                               };
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testData = """
                                namespace Reihitsu.Analyzer.Test.Design.Resources
                                {
                                    internal class RH2001
                                    {
                                        public object Describe()
                                        {
                                            return new { Alpha, this.Beta };
                                        }

                                        private int {|#0:Alpha|} { get; set; }

                                        private int {|#1:Beta|} { get; set; }
                                    }
                                }
                                """;

        const string resultData = """
                                  namespace Reihitsu.Analyzer.Test.Design.Resources
                                  {
                                      internal class RH2001
                                      {
                                          public object Describe()
                                          {
                                              return new { Alpha = _alpha, Beta = this._beta };
                                          }

                                          private int _alpha;

                                          private int _beta;
                                      }
                                  }
                                  """;

        return new FixAllScenario(testData, resultData, Diagnostics(RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzer.DiagnosticId, MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}