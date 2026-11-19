using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Clarity;
using Reihitsu.Analyzer.Rules.Clarity;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH3209GetOnlyPropertiesShouldUseAnExpressionBodyAnalyzer"/>
/// </summary>
[TestClass]
public class RH3209GetOnlyPropertiesShouldUseAnExpressionBodyAnalyzerTests : BatchCodeFixTestsBase<RH3209GetOnlyPropertiesShouldUseAnExpressionBodyAnalyzer, RH3209GetOnlyPropertiesShouldUseAnExpressionBodyCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying that the diagnostic carries the rule's message and lies on the accessor list
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-GETONLY-004")]
    public async Task DiagnosticCarriesTheRuleMessageOnTheAccessorList()
    {
        const string source = """
                              class C
                              {
                                  private int _value;

                                  public int A {|#0:{ get => _value; }|}
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                       private int _value;

                                       public int A => _value;
                                   }
                                   """;

        await Verify(source, fixedSource, Diagnostics(RH3209GetOnlyPropertiesShouldUseAnExpressionBodyAnalyzer.DiagnosticId, AnalyzerResources.RH3209MessageFormat));
    }

    /// <summary>
    /// Verifying the example of SHARED-GETONLY-001: an expression-bodied getter, a block getter over several lines, a
    /// throwing getter, and a <c>ref</c> getter are reported on their accessor lists and converted
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-GETONLY-001")]
    [Requirement("SHARED-GETONLY-004")]
    [Requirement("SHARED-GETONLY-005")]
    [Requirement("RH3209-001")]
    public async Task GetOnlyPropertiesAreReportedAndConverted()
    {
        const string source = """
                              using System;

                              class C
                              {
                                  private int _value;

                                  public int A {|RH3209:{ get => _value; }|}

                                  public int B
                                  {|RH3209:{
                                      get
                                      {
                                          return _value;
                                      }
                                  }|}

                                  public int D {|RH3209:{ get { throw new InvalidOperationException(); } }|}

                                  public ref int E {|RH3209:{ get { return ref _value; } }|}
                              }
                              """;

        const string fixedSource = """
                                   using System;

                                   class C
                                   {
                                       private int _value;

                                       public int A => _value;

                                       public int B => _value;

                                       public int D => throw new InvalidOperationException();

                                       public ref int E => ref _value;
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifying the example of SHARED-GETONLY-005: the fix writes the formatter's arrow clause and carries a comment
    /// behind the closing brace behind the new semicolon
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-GETONLY-005")]
    public async Task FixWritesTheFormattersExpressionBody()
    {
        const string source = """
                              using System;

                              class C
                              {
                                  private int _value;

                                  public int A {|RH3209:{ get => _value; }|}

                                  public int B
                                  {|RH3209:{
                                      get
                                      {
                                          return _value;
                                      }
                                  }|}

                                  public int D {|RH3209:{ get { throw new InvalidOperationException(); } }|}

                                  public int E {|RH3209:{ get => _value; }|} // a
                              }
                              """;

        const string fixedSource = """
                                   using System;

                                   class C
                                   {
                                       private int _value;

                                       public int A => _value;

                                       public int B => _value;

                                       public int D => throw new InvalidOperationException();

                                       public int E => _value; // a
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifying that the fix converts only the accessor list and keeps the spacing of the signature as written
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-GETONLY-005")]
    public async Task FixKeepsTheSignatureAsWritten()
    {
        const string source = """
                              class C
                              {
                                  private int _value;

                                  public  int  A {|RH3209:{ get { return _value; } }|}
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                       private int _value;

                                       public  int  A => _value;
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifying that the fix keeps the documentation comment, the attribute, and a comment between the attribute and
    /// the modifiers of the property as written
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-GETONLY-005")]
    public async Task FixKeepsDocumentationAttributesAndCommentsInFrontOfTheAccessorList()
    {
        const string source = """
                              using System;

                              class C
                              {
                                  private int _value;

                                  /// <summary>
                                  /// The value
                                  /// </summary>
                                  [Obsolete] /* c */ public int A {|RH3209:{ get => _value; }|}
                              }
                              """;

        const string fixedSource = """
                                   using System;

                                   class C
                                   {
                                       private int _value;

                                       /// <summary>
                                       /// The value
                                       /// </summary>
                                       [Obsolete] /* c */ public int A => _value;
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifying that the arrow follows the property name on its line when the accessor list starts on the next line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-GETONLY-005")]
    public async Task FixJoinsTheArrowOntoTheSignaturesLine()
    {
        const string source = """
                              class C
                              {
                                  private int _value;

                                  public int A
                                  {|RH3209:{
                                      get => _value;
                                  }|}
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                       private int _value;

                                       public int A => _value;
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifying that a thrown conditional expression is wrapped in parentheses, as the formatter wraps it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-GETONLY-005")]
    public async Task FixParenthesizesAThrownConditionalExpression()
    {
        const string source = """
                              using System;

                              class C
                              {
                                  private bool _flag;

                                  public int A {|RH3209:{ get { throw _flag ? new InvalidOperationException() : new ArgumentException(); } }|}
                              }
                              """;

        const string fixedSource = """
                                   using System;

                                   class C
                                   {
                                       private bool _flag;

                                       public int A => throw (_flag ? new InvalidOperationException() : new ArgumentException());
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifying that a thrown null-coalescing expression stays bare, as the formatter leaves it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-GETONLY-005")]
    public async Task FixLeavesAThrownCoalescingExpressionBare()
    {
        const string source = """
                              using System;

                              class C
                              {
                                  private Exception _error;

                                  public int A {|RH3209:{ get { throw _error ?? new InvalidOperationException(); } }|}
                              }
                              """;

        const string fixedSource = """
                                   using System;

                                   class C
                                   {
                                       private Exception _error;

                                       public int A => throw _error ?? new InvalidOperationException();
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifying that a getter whose expression spans several lines is converted to what the formatter writes
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-GETONLY-005")]
    public async Task FixOfAMultiLineExpressionMatchesTheFormatter()
    {
        const string source = """
                              using System;

                              class C
                              {
                                  private int _value;

                                  public int A
                                  {|RH3209:{
                                      get
                                      {
                                          return Math.Max(_value,
                                                          _value + 1);
                                      }
                                  }|}
                              }
                              """;

        await VerifyCodeFixMatchesFormatter(source);
    }

    /// <summary>
    /// Verifying the second example of SHARED-GETONLY-003: a comment behind only the getter's semicolon or behind only
    /// the closing brace is carried behind the new semicolon
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-GETONLY-003")]
    [Requirement("SHARED-GETONLY-005")]
    public async Task CommentBehindOnlyOneOfSemicolonAndBraceFollowsTheNewSemicolon()
    {
        const string source = """
                              class C
                              {
                                  private int _value;

                                  public int A {|RH3209:{ get => _value; }|} // a

                                  public int B
                                  {|RH3209:{
                                      get => _value; // b
                                  }|}
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                       private int _value;

                                       public int A => _value; // a

                                       public int B => _value; // b
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifying that a comment behind the statement's semicolon in a getter block follows the new semicolon
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-GETONLY-003")]
    [Requirement("SHARED-GETONLY-005")]
    public async Task CommentBehindTheReturnStatementFollowsTheNewSemicolon()
    {
        const string source = """
                              class C
                              {
                                  private int _value;

                                  public int A
                                  {|RH3209:{
                                      get
                                      {
                                          return _value; // a
                                      }
                                  }|}
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                       private int _value;

                                       public int A => _value; // a
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifying that a delimited comment that trails the getter's expression on its line stays behind the expression,
    /// as the formatter keeps it there
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-GETONLY-005")]
    public async Task DelimitedCommentTrailingTheExpressionStaysBehindIt()
    {
        const string source = """
                              class C
                              {
                                  private int _value;

                                  public int A {|RH3209:{ get => _value /* c */; }|}

                                  public int B {|RH3209:{ get { return _value /* d */; } }|}
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                       private int _value;

                                       public int A => _value /* c */;

                                       public int B => _value /* d */;
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifying the fix under CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-GETONLY-005")]
    public async Task FixKeepsCarriageReturnLineFeedLineEndings()
    {
        const string source = """
                              class C
                              {
                                  private int _value;

                                  public int A
                                  {|RH3209:{
                                      get
                                      {
                                          return _value;
                                      }
                                  }|}
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                       private int _value;

                                       public int A => _value;
                                   }
                                   """;

        await VerifyCodeFixWithEndOfLine(source, fixedSource, "\r\n");
    }

    /// <summary>
    /// Verifying the fix under CRLF line endings, with a trailing comment carried behind the new semicolon
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-GETONLY-005")]
    public async Task FixKeepsCarriageReturnLineFeedLineEndingsWithTrailingComment()
    {
        const string source = """
                              class C
                              {
                                  private int _value;

                                  public int B
                                  {|RH3209:{
                                      get => _value; // b
                                  }|}
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                       private int _value;

                                       public int B => _value; // b
                                   }
                                   """;

        await VerifyCodeFixWithEndOfLine(source, fixedSource, "\r\n");
    }

    /// <summary>
    /// Verifying the fix under LF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-GETONLY-005")]
    public async Task FixKeepsLineFeedLineEndings()
    {
        const string source = """
                              class C
                              {
                                  private int _value;

                                  public int A
                                  {|RH3209:{
                                      get
                                      {
                                          return _value;
                                      }
                                  }|}
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                       private int _value;

                                       public int A => _value;
                                   }
                                   """;

        await VerifyCodeFixWithEndOfLine(source, fixedSource, "\n");
    }

    /// <summary>
    /// Verifying the example of RH3209-001: an interface member with a body, a static property, and an explicit interface
    /// implementation are reported, and an auto-property and an expression-bodied property are not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3209-001")]
    [Requirement("SHARED-GETONLY-001")]
    public async Task InterfaceStaticAndExplicitImplementationPropertiesAreReported()
    {
        const string source = """
                              interface I
                              {
                                  int P {|RH3209:{ get => 1; }|}

                                  int Q { get; }
                              }

                              struct S : I
                              {
                                  private static int s_count;

                                  public static int Count {|RH3209:{ get { return s_count; } }|}

                                  int I.P {|RH3209:{ get { return 2; } }|}

                                  public int Q => 3;
                              }
                              """;

        const string fixedSource = """
                                   interface I
                                   {
                                       int P => 1;

                                       int Q { get; }
                                   }

                                   struct S : I
                                   {
                                       private static int s_count;

                                       public static int Count => s_count;

                                       int I.P => 2;

                                       public int Q => 3;
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifying that a static virtual interface property with a body is reported and converted
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3209-001")]
    public async Task StaticVirtualInterfacePropertyIsReportedAndConverted()
    {
        const string source = """
                              interface I
                              {
                                  static virtual int S {|RH3209:{ get { return 1; } }|}
                              }
                              """;

        const string fixedSource = """
                                   interface I
                                   {
                                       static virtual int S => 1;
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifying that properties in a record, a record struct, a nested type, and an overriding property are reported
    /// and converted, and that a <c>readonly</c> property keeps its modifier
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3209-001")]
    public async Task PropertiesInEveryContainerAreReportedAndConverted()
    {
        const string source = """
                              class Base
                              {
                                  public virtual int P { get; set; }
                              }

                              class Derived : Base
                              {
                                  public override int P {|RH3209:{ get { return 1; } }|}

                                  public class Nested
                                  {
                                      public int Q {|RH3209:{ get { return 2; } }|}
                                  }
                              }

                              record R
                              {
                                  public int P {|RH3209:{ get { return 3; } }|}
                              }

                              record struct RS
                              {
                                  private int _value;

                                  public readonly int P {|RH3209:{ get { return _value; } }|}
                              }
                              """;

        const string fixedSource = """
                                   class Base
                                   {
                                       public virtual int P { get; set; }
                                   }

                                   class Derived : Base
                                   {
                                       public override int P => 1;

                                       public class Nested
                                       {
                                           public int Q => 2;
                                       }
                                   }

                                   record R
                                   {
                                       public int P => 3;
                                   }

                                   record struct RS
                                   {
                                       private int _value;

                                       public readonly int P => _value;
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifying that the implementing part of a partial property is reported and converted, and its defining part is not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3209-001")]
    public async Task ImplementingPartOfAPartialPropertyIsReportedAndConverted()
    {
        const string source = """
                              partial class C
                              {
                                  public partial int P { get; }
                              }

                              partial class C
                              {
                                  private int _p;

                                  public partial int P {|RH3209:{ get => _p; }|}
                              }
                              """;

        const string fixedSource = """
                                   partial class C
                                   {
                                       public partial int P { get; }
                                   }

                                   partial class C
                                   {
                                       private int _p;

                                       public partial int P => _p;
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifying that a property in an extension block is reported and converted
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3209-001")]
    public async Task ExtensionBlockPropertyIsReportedAndConverted()
    {
        const string source = """
                              static class E
                              {
                                  extension(string s)
                                  {
                                      public int Length2 {|RH3209:{ get { return s.Length; } }|}
                                  }
                              }
                              """;

        const string fixedSource = """
                                   static class E
                                   {
                                       extension(string s)
                                       {
                                           public int Length2 => s.Length;
                                       }
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifying that a getter that returns the <c>field</c> keyword is reported and converted on a property without an
    /// initializer
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-GETONLY-001")]
    public async Task FieldKeywordGetterWithoutInitializerIsReportedAndConverted()
    {
        const string source = """
                              class C
                              {
                                  public int P {|RH3209:{ get => field; }|}
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                       public int P => field;
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifying that indexers are not reported by the property rule
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3209-001")]
    public async Task IndexersAreNotReported()
    {
        const string source = """
                              class C
                              {
                                  public int this[int index] { get => index; }

                                  public int this[long index]
                                  {
                                      get
                                      {
                                          return (int)index;
                                      }
                                  }
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifying that under C# 6 a block getter that returns is reported and converted, and a getter that throws is not
    /// reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-GETONLY-001")]
    public async Task UnderCSharp6OnlyAReturningGetterIsReported()
    {
        const string source = """
                              using System;

                              class C
                              {
                                  private int _value;

                                  public int B
                                  {|RH3209:{
                                      get
                                      {
                                          return _value;
                                      }
                                  }|}

                                  public int D { get { throw new InvalidOperationException(); } }
                              }
                              """;

        const string fixedSource = """
                                   using System;

                                   class C
                                   {
                                       private int _value;

                                       public int B => _value;

                                       public int D { get { throw new InvalidOperationException(); } }
                                   }
                                   """;

        await Verify(source, fixedSource, test => test.SolutionTransforms.Add((solution, projectId) => ApplyLanguageVersion(solution, projectId, LanguageVersion.CSharp6)));
    }

    /// <summary>
    /// Verifying that under C# 7 a getter that throws is reported and converted
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-GETONLY-001")]
    public async Task UnderCSharp7AThrowingGetterIsReported()
    {
        const string source = """
                              using System;

                              class C
                              {
                                  public int D {|RH3209:{ get { throw new InvalidOperationException(); } }|}
                              }
                              """;

        const string fixedSource = """
                                   using System;

                                   class C
                                   {
                                       public int D => throw new InvalidOperationException();
                                   }
                                   """;

        await Verify(source, fixedSource, test => test.SolutionTransforms.Add((solution, projectId) => ApplyLanguageVersion(solution, projectId, LanguageVersion.CSharp7)));
    }

    /// <summary>
    /// Verifying that under C# 5 a get-only property is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-GETONLY-001")]
    public async Task UnderCSharp5NothingIsReported()
    {
        const string source = """
                              using System;

                              class C
                              {
                                  private int _value;

                                  public int B
                                  {
                                      get
                                      {
                                          return _value;
                                      }
                                  }

                                  public int D { get { throw new InvalidOperationException(); } }
                              }
                              """;

        await Verify(source, test => test.SolutionTransforms.Add((solution, projectId) => ApplyLanguageVersion(solution, projectId, LanguageVersion.CSharp5)));
    }

    /// <summary>
    /// Verifying the example of SHARED-GETONLY-002: members whose shape the formatter keeps are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-GETONLY-002")]
    public async Task MembersWhoseShapeTheFormatterKeepsAreNotReported()
    {
        const string source = """
                              using System;

                              class C
                              {
                                  private int _value;

                                  public int A
                                  {
                                      get => _value;
                                      set => _value = value;
                                  }

                                  public int B { get; }

                                  public int D
                                  {
                                      [Obsolete]
                                      get => _value;
                                  }

                                  public int E
                                  {
                                      get
                                      {
                                          var value = _value;

                                          return value;
                                      }
                                  }

                                  public int F { get => field; } = 5;

                                  public int G => _value;
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifying that a getter next to an <c>init</c> accessor is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-GETONLY-002")]
    public async Task GetterNextToInitAccessorIsNotReported()
    {
        const string source = """
                              class C
                              {
                                  private int _value;

                                  public int A
                                  {
                                      get => _value;
                                      init => _value = value;
                                  }
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifying that a block getter with an attribute is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-GETONLY-002")]
    public async Task BlockGetterWithAttributeIsNotReported()
    {
        const string source = """
                              using System;

                              class C
                              {
                                  private int _value;

                                  public int A
                                  {
                                      [Obsolete]
                                      get
                                      {
                                          return _value;
                                      }
                                  }
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifying that a getter whose block holds a <c>yield return</c>, or a <c>try</c> statement with a rethrow without an
    /// operand, is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-GETONLY-002")]
    public async Task GetterWithAStatementWithoutExpressionFormIsNotReported()
    {
        const string source = """
                              using System;
                              using System.Collections.Generic;

                              class C
                              {
                                  public IEnumerable<int> A
                                  {
                                      get
                                      {
                                          yield return 1;
                                      }
                                  }

                                  public int B
                                  {
                                      get
                                      {
                                          try
                                          {
                                              return 1;
                                          }
                                          catch (InvalidOperationException)
                                          {
                                              throw;
                                          }
                                      }
                                  }
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifying that an auto-property with an initializer is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-GETONLY-002")]
    public async Task PropertyWithInitializerIsNotReported()
    {
        const string source = """
                              class C
                              {
                                  public int A { get; } = 1;

                                  public int B
                                  {
                                      get
                                      {
                                          return field;
                                      }
                                  } = 2;
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifying the first example of SHARED-GETONLY-003: comments the formatter keeps the accessor list or the getter
    /// block for are reported, and no fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-GETONLY-003")]
    public async Task MembersWithCommentsTheFormatterKeepsAreReportedWithoutFix()
    {
        const string source = """
                              class C
                              {
                                  private int _value;

                                  public int A // note
                                  {|RH3209:{
                                      get => _value;
                                  }|}

                                  public int B
                                  {|RH3209:{
                                      // the value
                                      get => _value;
                                  }|}

                                  public int D
                                  {|RH3209:{
                                      get => _value; // a
                                  }|} // b

                                  public int E
                                  {|RH3209:{
                                      get
                                      {
                                          // cached
                                          return _value;
                                      }
                                  }|}
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifying that a comment behind the opening brace of the accessor list withholds the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-GETONLY-003")]
    public async Task CommentBehindOpeningBraceWithholdsFix()
    {
        const string source = """
                              class C
                              {
                                  private int _value;

                                  public int A
                                  {|RH3209:{ // c
                                      get => _value;
                                  }|}

                                  public int B {|RH3209:{ /* c */ get => _value; }|}
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifying that a comment in front of the getter's semicolon on its own line withholds the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-GETONLY-003")]
    public async Task CommentInFrontOfTheGettersSemicolonWithholdsFix()
    {
        const string source = """
                              class C
                              {
                                  private int _value;

                                  public int A
                                  {|RH3209:{
                                      get => _value // c
                                          ;
                                  }|}
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifying that a comment in front of the closing brace of the accessor list withholds the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-GETONLY-003")]
    public async Task CommentInFrontOfTheClosingBraceWithholdsFix()
    {
        const string source = """
                              class C
                              {
                                  private int _value;

                                  public int A
                                  {|RH3209:{
                                      get => _value;
                                      // c
                                  }|}
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifying that a delimited comment behind both the getter's semicolon and the closing brace withholds the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-GETONLY-003")]
    public async Task DelimitedCommentsBehindSemicolonAndBraceWithholdFix()
    {
        const string source = """
                              class C
                              {
                                  private int _value;

                                  public int A {|RH3209:{ get => _value; /* c */ }|} // d
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifying that a comment behind the <c>get</c> keyword of a block getter withholds the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-GETONLY-003")]
    public async Task CommentBehindGetKeywordOfABlockGetterWithholdsFix()
    {
        const string source = """
                              class C
                              {
                                  private int _value;

                                  public int A {|RH3209:{ get /* c */ { return _value; } }|}

                                  public int B
                                  {|RH3209:{
                                      get // c
                                      {
                                          return _value;
                                      }
                                  }|}
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifying that a comment behind <c>return</c> in a getter block withholds the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-GETONLY-003")]
    public async Task CommentBehindReturnWithholdsFix()
    {
        const string source = """
                              class C
                              {
                                  private int _value;

                                  public int A {|RH3209:{ get { return /* c */ _value; } }|}
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifying that a single-line comment between the returned expression and the semicolon of a getter block
    /// withholds the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-GETONLY-003")]
    public async Task SingleLineCommentInFrontOfTheReturnSemicolonWithholdsFix()
    {
        const string source = """
                              class C
                              {
                                  private int _value;

                                  public int A
                                  {|RH3209:{
                                      get
                                      {
                                          return _value // c
                                              ;
                                      }
                                  }|}
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifying that a comment in front of the closing brace of a getter block withholds the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-GETONLY-003")]
    public async Task CommentInFrontOfTheGetterBlocksClosingBraceWithholdsFix()
    {
        const string source = """
                              class C
                              {
                                  private int _value;

                                  public int A
                                  {|RH3209:{
                                      get
                                      {
                                          return _value;
                                          // c
                                      }
                                  }|}
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifying that a comment behind both the return statement and the closing brace of a getter block withholds the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-GETONLY-003")]
    public async Task CommentsBehindReturnStatementAndGetterBlockBraceWithholdFix()
    {
        const string source = """
                              class C
                              {
                                  private int _value;

                                  public int A
                                  {|RH3209:{
                                      get
                                      {
                                          return _value; // a
                                      } // b
                                  }|}
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifying the example of SHARED-GETONLY-006: a comment or a conditional group around the getter's arrow is reported,
    /// and no fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-GETONLY-006")]
    public async Task GetterTheFormatterRefusesIsReportedWithoutFix()
    {
        const string source = """
                              class C
                              {
                                  private int _value;

                                  public int A {|RH3209:{ get /* c */ => _value; }|}

                                  public int B {|RH3209:{ get => /* c */ _value; }|}

                                  public int D
                                  {|RH3209:{
                                      get
                              #if DEBUG
                                          => 1;
                              #else
                                          => 2;
                              #endif
                                  }|}
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifying that a conditional group between the getter's arrow and its expression is reported, and no fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-GETONLY-006")]
    public async Task ConditionalGroupBehindTheArrowIsReportedWithoutFix()
    {
        const string source = """
                              class C
                              {
                                  public int A
                                  {|RH3209:{
                                      get =>
                              #if DEBUG
                                          1;
                              #else
                                          2;
                              #endif
                                  }|}
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifying that a single-line comment between <c>get</c> and the arrow is reported, and no fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-GETONLY-006")]
    public async Task SingleLineCommentInFrontOfTheArrowIsReportedWithoutFix()
    {
        const string source = """
                              class C
                              {
                                  private int _value;

                                  public int A
                                  {|RH3209:{
                                      get // c
                                          => _value;
                                  }|}
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifying that a refused shape inside the getter's expression is reported, and no fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-GETONLY-006")]
    public async Task RefusedShapeInsideTheExpressionIsReportedWithoutFix()
    {
        const string source = """
                              class C
                              {
                                  private int _value;

                                  public int A {|RH3209:{ get => _value + /* c */ 1; }|}
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifying that a refused shape in front of the accessor list, which the member the fix builds still holds, is
    /// reported, and no fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-GETONLY-006")]
    public async Task RefusedShapeInTheSignatureIsReportedWithoutFix()
    {
        const string source = """
                              using System;

                              class C
                              {
                                  private int _value;

                                  [Obsolete(true ? /* c */ "a" : "b")]
                                  public int A {|RH3209:{ get => _value; }|}
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifying the first example of SHARED-GETONLY-007 compiled without <c>WRITABLE</c>: directives and disabled text the
    /// formatter keeps the accessor list or the getter block for are reported, and no fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-GETONLY-007")]
    public async Task MembersWithDirectivesTheFormatterKeepsAreReportedWithoutFix()
    {
        const string source = """
                              class C
                              {
                                  private int _value;

                                  public int A
                                  {|RH3209:{
                                      get => _value;
                              #if WRITABLE
                                      set => _value = value;
                              #endif
                                  }|}

                                  public int B
                                  {|RH3209:{
                              #if DEBUG
                                      get => 1;
                              #else
                                      get => 2;
                              #endif
                                  }|}

                                  public int D
                                  {|RH3209:{
                                      get
                                      {
                                          return _value;
                              #if DEBUG
                                      }
                              #else
                                      }
                              #endif
                                  }|}
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifying that a member whose <c>set</c> accessor stands in an active branch is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-GETONLY-007")]
    [Requirement("SHARED-GETONLY-002")]
    public async Task MemberWithActiveSetterInConditionalGroupIsNotReported()
    {
        const string source = """
                              class C
                              {
                                  private int _value;

                                  public int A
                                  {
                                      get => _value;
                              #if WRITABLE
                                      set => _value = value;
                              #endif
                                  }
                              }
                              """;

        await VerifyWithPreprocessorSymbols(source, "WRITABLE");
    }

    /// <summary>
    /// Verifying that a region inside the accessor list is reported, and no fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-GETONLY-007")]
    public async Task RegionInsideTheAccessorListIsReportedWithoutFix()
    {
        const string source = """
                              class C
                              {
                                  private int _value;

                                  public int A
                                  {|RH3209:{
                                      #region Value

                                      get => _value;

                                      #endregion // Value
                                  }|}
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifying that a pragma directive in front of the getter is reported, and no fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-GETONLY-007")]
    public async Task PragmaInFrontOfTheGetterIsReportedWithoutFix()
    {
        const string source = """
                              class C
                              {
                                  private int _value;

                                  public int A
                                  {|RH3209:{
                              #pragma warning disable CS0168
                                      get => _value;
                                  }|}
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifying that disabled text in front of the opening brace of the accessor list is reported, and no fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-GETONLY-007")]
    public async Task DisabledTextInFrontOfTheAccessorListIsReportedWithoutFix()
    {
        const string source = """
                              class C
                              {
                                  private int _value;

                                  public int A
                              #if NEVER
                                      // disabled
                              #endif
                                  {|RH3209:{
                                      get => _value;
                                  }|}
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifying that a member that holds a directive and a comment the formatter keeps is reported, and no fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-GETONLY-007")]
    public async Task MemberWithDirectiveAndCommentIsReportedWithoutFix()
    {
        const string source = """
                              class C
                              {
                                  private int _value;

                                  public int A
                                  {|RH3209:{
                                      // the value
                              #if WRITABLE
                                      set => _value = value;
                              #endif
                                      get => _value;
                                  }|}
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifying the second example of SHARED-GETONLY-007: a conditional group inside the getter's expression is reported,
    /// and the fix writes what the formatter writes
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-GETONLY-007")]
    [Requirement("SHARED-GETONLY-005")]
    public async Task ConditionalGroupInsideTheExpressionIsReportedAndFixedAsTheFormatterFormats()
    {
        const string source = """
                              class C
                              {
                                  public int A
                                  {|RH3209:{
                                      get
                                      {
                                          return M(
                              #if DEBUG
                                              1
                              #else
                                              2
                              #endif
                                              );
                                      }
                                  }|}

                                  private static int M(int value)
                                  {
                                      return value;
                                  }
                              }
                              """;

        await VerifyCodeFixMatchesFormatter(source);
    }

    /// <summary>
    /// Verifying that no fix is offered in a file with a syntax error, while the diagnostic is still reported, and that it
    /// is offered once the syntax error is gone
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-011")]
    public async Task NoFixIsOfferedInAFileWithASyntaxError()
    {
        const string source = """
                              class C
                              {
                                  public int A { get => 1; }

                                  public int B { get { return 2; } }
                              }
                              """;

        await VerifyNoCodeFixIsOfferedOnceASyntaxErrorIsAdded(source);
    }

    /// <summary>
    /// Verifying the negative example <c>B</c> of SHARED-GETONLY-003: a delimited comment on a line of its own in front of
    /// the getter's semicolon withholds the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-GETONLY-003")]
    public async Task DelimitedCommentOnALineOfItsOwnInFrontOfTheGettersSemicolonWithholdsFix()
    {
        const string source = """
                              class C
                              {
                                  private int _value;

                                  public int B
                                  {|RH3209:{
                                      get => _value
                                          /* d */ ;
                                  }|}
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifying that a single-line comment on a line of its own in front of the getter's semicolon withholds the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-GETONLY-003")]
    public async Task SingleLineCommentOnALineOfItsOwnInFrontOfTheGettersSemicolonWithholdsFix()
    {
        const string source = """
                              class C
                              {
                                  private int _value;

                                  public int B
                                  {|RH3209:{
                                      get => _value
                                          // d
                                          ;
                                  }|}
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifying the block variant of the negative example <c>B</c> of SHARED-GETONLY-003: a delimited comment on a line of
    /// its own in front of the semicolon of the getter block's <c>return</c> statement withholds the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-GETONLY-003")]
    public async Task DelimitedCommentOnALineOfItsOwnInFrontOfTheReturnSemicolonWithholdsFix()
    {
        const string source = """
                              class C
                              {
                                  private int _value;

                                  public int B
                                  {|RH3209:{
                                      get
                                      {
                                          return _value
                                              /* e */ ;
                                      }
                                  }|}
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifying that a member behind the closing brace of the converted accessor list, on the same line, starts on the next
    /// line, and that the blank line above the converted member stays, under LF and CRLF
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-FIX-007")]
    [Requirement("SHARED-GETONLY-008")]
    public async Task FixPutsAMemberBehindTheConvertedMemberOnTheNextLine(string endOfLine)
    {
        const string source = """
                              class C
                              {
                                  private int _value;

                                  public int A {|RH3209:{ get => _value; }|} public int B => 1;
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                       private int _value;

                                       public int A => _value;
                                       public int B => 1;
                                   }
                                   """;

        await VerifyCodeFixWithEndOfLine(source, fixedSource, endOfLine);
    }

    /// <summary>
    /// Verifying that the converted member starts on the line after a member in front of it on the same line, under LF and
    /// CRLF
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-FIX-007")]
    [Requirement("SHARED-GETONLY-008")]
    public async Task FixPutsTheConvertedMemberOnTheLineAfterAMemberInFrontOfIt(string endOfLine)
    {
        const string source = """
                              class C
                              {
                                  private int _value;

                                  public int B => 1; public int A {|RH3209:{ get { return _value; } }|}
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                       private int _value;

                                       public int B => 1;
                                       public int A => _value;
                                   }
                                   """;

        await VerifyCodeFixWithEndOfLine(source, fixedSource, endOfLine);
    }

    /// <summary>
    /// Verifying that a member whose signature shares its line with a member in front of it, and whose accessor list spans
    /// several lines, starts on the line after that member, under LF and CRLF
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-FIX-007")]
    [Requirement("SHARED-GETONLY-008")]
    public async Task FixPutsAMultiLineConvertedMemberOnTheLineAfterAMemberInFrontOfIt(string endOfLine)
    {
        const string source = """
                              class C
                              {
                                  private int _value;

                                  public int B => 1; public int A
                                  {|RH3209:{
                                      get => _value;
                                  }|}
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                       private int _value;

                                       public int B => 1;
                                       public int A => _value;
                                   }
                                   """;

        await VerifyCodeFixWithEndOfLine(source, fixedSource, endOfLine);
    }

    /// <summary>
    /// Verifying that a member behind the closing brace of a multi-line accessor list whose getter's semicolon carries a
    /// single-line comment starts on the next line in the column of the converted member, and the comment follows the new
    /// semicolon, under LF and CRLF
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-FIX-007")]
    [Requirement("SHARED-GETONLY-005")]
    [Requirement("SHARED-GETONLY-008")]
    public async Task FixPutsAMemberBehindTheClosingBraceOnTheNextLineWhenTheSemicolonCarriesAComment(string endOfLine)
    {
        const string source = """
                              class C
                              {
                                  public int A
                                  {|RH3209:{
                                      get => 1; // a
                                  }|} public int B => 2;
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                       public int A => 1; // a
                                       public int B => 2;
                                   }
                                   """;

        await VerifyCodeFixWithEndOfLine(source, fixedSource, endOfLine);
    }

    /// <summary>
    /// Verifying that the fix is withheld when a delimited comment behind the closing brace of the accessor list separates
    /// the member from a member behind it on the same line, under LF and CRLF
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-GETONLY-008")]
    public async Task CommentBetweenTheMemberAndAMemberBehindItOnTheSameLineWithholdsFix(string endOfLine)
    {
        const string source = """
                              class C
                              {
                                  public int A {|RH3209:{ get => 1; }|} /* c */ public int B => 2;
                              }
                              """;

        await VerifyNoCodeFixIsOffered(ConvertEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying that the fix is withheld when a delimited comment separates the member from a member in front of it on the
    /// same line, under LF and CRLF
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-GETONLY-008")]
    public async Task CommentBetweenTheMemberAndAMemberInFrontOfItOnTheSameLineWithholdsFix(string endOfLine)
    {
        const string source = """
                              class C
                              {
                                  public int B => 2; /* c */ public int A {|RH3209:{ get => 1; }|}
                              }
                              """;

        await VerifyNoCodeFixIsOffered(ConvertEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying that the fix leaves the members on the line of a member of a type written on one line, in front of it and
    /// behind it, under LF and CRLF
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-GETONLY-008")]
    public async Task FixLeavesTheMembersOnTheLineOfAMemberOfATypeWrittenOnOneLine(string endOfLine)
    {
        const string source = """
                              namespace N
                              {
                                  class C { private int _value; public int A {|RH3209:{ get => _value; }|} private int _x; }
                              }
                              """;

        const string fixedSource = """
                                   namespace N
                                   {
                                       class C { private int _value; public int A => _value; private int _x; }
                                   }
                                   """;

        await VerifyCodeFixWithEndOfLine(source, fixedSource, endOfLine);
    }

    /// <summary>
    /// Verifying the specification example: a converted member with a sibling behind it, and one with a sibling in front of
    /// it, each on one line, are put on lines of their own, one at a time and with Fix All, under LF and CRLF
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-FIX-007")]
    [Requirement("SHARED-GETONLY-008")]
    public async Task VerifySpecificationExampleOfMembersSharingTheirLine(string endOfLine)
    {
        const string source = """
                              class C
                              {
                                  public int A {|RH3209:{ get => 1; }|} public int B => 2;

                                  public int D => 3; public int E {|RH3209:{ get => 4; }|}
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                       public int A => 1;
                                       public int B => 2;

                                       public int D => 3;
                                       public int E => 4;
                                   }
                                   """;

        await Verify(ConvertEndOfLine(source, endOfLine), ConvertEndOfLine(fixedSource, endOfLine));
    }

    /// <summary>
    /// Verifying the specification example: a member of a type written on one line, with only a member in front of it, is
    /// converted and stays on the type's line, under LF and CRLF
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-GETONLY-008")]
    public async Task VerifySpecificationExampleOfAMemberOfATypeWrittenOnOneLine(string endOfLine)
    {
        const string source = """
                              namespace N
                              {
                                  class C { private int _value; public int A {|RH3209:{ get => _value; }|} }
                              }
                              """;

        const string fixedSource = """
                                   namespace N
                                   {
                                       class C { private int _value; public int A => _value; }
                                   }
                                   """;

        await VerifyCodeFixWithEndOfLine(source, fixedSource, endOfLine);
    }

    /// <summary>
    /// Verifying that the fix is withheld when a delimited comment separates a member of a type written on one line from a
    /// member behind it, although that line does not begin with a member, under LF and CRLF
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-GETONLY-008")]
    public async Task CommentBetweenTheMemberAndAMemberBehindItInATypeWrittenOnOneLineWithholdsFix(string endOfLine)
    {
        const string source = """
                              namespace N
                              {
                                  class C { private int _value; public int A {|RH3209:{ get => _value; }|} /* c */ private int _x; }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(ConvertEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying the specification example: a delimited comment behind the converted member whose sibling stands on the
    /// next line does not withhold the fix, and the comment follows the new semicolon, under LF and CRLF
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-GETONLY-005")]
    [Requirement("SHARED-GETONLY-008")]
    public async Task CommentBehindTheMemberWithTheSiblingOnTheNextLineKeepsTheFix(string endOfLine)
    {
        const string source = """
                              class C
                              {
                                  public int A {|RH3209:{ get => 1; }|} /* c */
                                  public int B => 2;
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                       public int A => 1; /* c */
                                       public int B => 2;
                                   }
                                   """;

        await VerifyCodeFixWithEndOfLine(source, fixedSource, endOfLine);
    }

    /// <summary>
    /// Verifying that the fix is withheld when a delimited documentation comment behind the closing brace of the accessor list
    /// separates the member from a member behind it on the same line, in every documentation mode, under LF and CRLF
    /// </summary>
    /// <param name="documentationMode">Documentation mode of the syntax tree</param>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow(DocumentationMode.Parse, "\n")]
    [DataRow(DocumentationMode.Parse, "\r\n")]
    [DataRow(DocumentationMode.Diagnose, "\n")]
    [DataRow(DocumentationMode.Diagnose, "\r\n")]
    [DataRow(DocumentationMode.None, "\n")]
    [DataRow(DocumentationMode.None, "\r\n")]
    [Requirement("SHARED-GETONLY-008")]
    public async Task DocumentationCommentBetweenTheMemberAndAMemberBehindItOnTheSameLineWithholdsFix(DocumentationMode documentationMode, string endOfLine)
    {
        const string source = """
                              class C
                              {
                                  public int A {|RH3209:{ get => 1; }|} /** c */ public int B => 2;
                              }
                              """;

        await VerifyNoCodeFixIsOffered(ConvertEndOfLine(source, endOfLine), documentationMode);
    }

    /// <summary>
    /// Verifying that the fix is withheld when a delimited documentation comment separates the member from a member in front
    /// of it on the same line, in every documentation mode, under LF and CRLF
    /// </summary>
    /// <param name="documentationMode">Documentation mode of the syntax tree</param>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow(DocumentationMode.Parse, "\n")]
    [DataRow(DocumentationMode.Parse, "\r\n")]
    [DataRow(DocumentationMode.Diagnose, "\n")]
    [DataRow(DocumentationMode.Diagnose, "\r\n")]
    [DataRow(DocumentationMode.None, "\n")]
    [DataRow(DocumentationMode.None, "\r\n")]
    [Requirement("SHARED-GETONLY-008")]
    public async Task DocumentationCommentBetweenTheMemberAndAMemberInFrontOfItOnTheSameLineWithholdsFix(DocumentationMode documentationMode, string endOfLine)
    {
        const string source = """
                              class C
                              {
                                  public int D => 3; /** c */ public int E {|RH3209:{ get => 4; }|}
                              }
                              """;

        await VerifyNoCodeFixIsOffered(ConvertEndOfLine(source, endOfLine), documentationMode);
    }

    /// <summary>
    /// Verifying that the fix is withheld when a delimited comment whose line break lies inside the comment separates the
    /// member from a member behind it, which therefore shares the line the comment ends on, under LF and CRLF
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-GETONLY-008")]
    public async Task MultiLineCommentBetweenTheMemberAndAMemberBehindItWithholdsFix(string endOfLine)
    {
        const string source = """
                              class C
                              {
                                  public int A {|RH3209:{ get => 1; }|} /* a
                                         b */ public int B => 2;
                              }
                              """;

        await VerifyNoCodeFixIsOffered(ConvertEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying that the fix is withheld when a delimited comment whose line break lies inside the comment separates the
    /// member from a member in front of it, which therefore shares the line the comment starts on, under LF and CRLF
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-GETONLY-008")]
    public async Task MultiLineCommentBetweenTheMemberAndAMemberInFrontOfItWithholdsFix(string endOfLine)
    {
        const string source = """
                              class C
                              {
                                  public int D => 3; /* a
                                         b */ public int E {|RH3209:{ get => 4; }|}
                              }
                              """;

        await VerifyNoCodeFixIsOffered(ConvertEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifying that a delimited comment with a line break inside it, behind the converted member whose sibling stands on
    /// the next line, does not withhold the fix, and the comment follows the new semicolon, under LF and CRLF
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-GETONLY-005")]
    [Requirement("SHARED-GETONLY-008")]
    public async Task MultiLineCommentBehindTheMemberWithTheSiblingOnTheNextLineKeepsTheFix(string endOfLine)
    {
        const string source = """
                              class C
                              {
                                  public int A {|RH3209:{ get => 1; }|} /* a
                                         b */
                                  public int B => 2;
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                       public int A => 1; /* a
                                              b */
                                       public int B => 2;
                                   }
                                   """;

        await VerifyCodeFixWithEndOfLine(source, fixedSource, endOfLine);
    }

    /// <summary>
    /// Verifying that fixing two reported members that share one line, one at a time and with Fix All, gives each of them a
    /// line of its own, under LF and CRLF
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-FIX-007")]
    [Requirement("SHARED-GETONLY-008")]
    public async Task FixAllPutsTwoConvertedMembersOfOneLineOnLinesOfTheirOwn(string endOfLine)
    {
        const string source = """
                              class C
                              {
                                  private int _value;

                                  public int A {|RH3209:{ get => _value; }|} public int B {|RH3209:{ get { return 2; } }|}
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                       private int _value;

                                       public int A => _value;
                                       public int B => 2;
                                   }
                                   """;

        await Verify(ConvertEndOfLine(source, endOfLine), ConvertEndOfLine(fixedSource, endOfLine));
    }

    #endregion // Tests

    #region Methods

    /// <summary>
    /// Sets the language version of the test project
    /// </summary>
    /// <param name="solution">Solution</param>
    /// <param name="projectId">Project ID</param>
    /// <param name="languageVersion">Language version</param>
    /// <returns>The solution with the language version applied</returns>
    private static Solution ApplyLanguageVersion(Solution solution, ProjectId projectId, LanguageVersion languageVersion)
    {
        return solution.GetProject(projectId)?.ParseOptions is CSharpParseOptions parseOptions
                   ? solution.WithProjectParseOptions(projectId, parseOptions.WithLanguageVersion(languageVersion))
                   : solution;
    }

    #endregion // Methods

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string source = """
                              using System;

                              class C
                              {
                                  private int _value;

                                  public int A {|#0:{ get => _value; }|}

                                  public int B
                                  {|#1:{
                                      get
                                      {
                                          return _value;
                                      }
                                  }|}

                                  public int this[int index] { get => index; }

                                  public class Nested
                                  {
                                      public int D {|#2:{ get { throw new InvalidOperationException(); } }|}
                                  }
                              }
                              """;

        const string fixedSource = """
                                   using System;

                                   class C
                                   {
                                       private int _value;

                                       public int A => _value;

                                       public int B => _value;

                                       public int this[int index] { get => index; }

                                       public class Nested
                                       {
                                           public int D => throw new InvalidOperationException();
                                       }
                                   }
                                   """;

        return new FixAllScenario(source,
                                  fixedSource,
                                  Diagnostics(RH3209GetOnlyPropertiesShouldUseAnExpressionBodyAnalyzer.DiagnosticId, AnalyzerResources.RH3209MessageFormat, 3));
    }

    #endregion // BatchCodeFixTestsBase
}