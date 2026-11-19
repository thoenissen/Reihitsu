using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Design;
using Reihitsu.Analyzer.Rules.Design;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Design;

/// <summary>
/// Test methods for <see cref="RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzer"/> and <see cref="RH2001PrivateAutoPropertiesShouldNotBeUsedCodeFixProvider"/>
/// </summary>
public partial class RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzerTests
{
    #region Tests

    /// <summary>
    /// Verifies the specification example of a group behind a property with an initializer: the property ends in
    /// <c>;</c>, so the excluded <c>static</c> belongs to the next member in every build, and the fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH2001-004")]
    [Requirement("RH2001-010")]
    public async Task VerifyFixKeepsAConditionalModifierBehindAPropertyWithAnInitializer(string endOfLine)
    {
        const string source = """
                              class C
                              {
                                  private int {|RH2001:Value|} { get; } = 5;
                              #if DEBUG
                                  static
                              #endif
                                  private int _other;

                                  int Read() => Value;
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                       private readonly int _value = 5;
                                   #if DEBUG
                                       static
                                   #endif
                                       private int _other;

                                       int Read() => _value;
                                   }
                                   """;

        await Verify(WithEndOfLine(source, endOfLine), WithEndOfLine(fixedSource, endOfLine));
    }

    /// <summary>
    /// Verifies the specification example of a group of a whole member in front of the first modifier inside the active
    /// branch of a group that wraps the file: the wrapping group has no directive in front of the first modifier or behind
    /// the property, so it is not read, and the fix is offered as in multi-targeting code
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH2001-004")]
    [Requirement("RH2001-010")]
    public async Task VerifyFixKeepsAGroupOfAWholeMemberInsideTheActiveBranchOfAGroupThatWrapsTheFile(string endOfLine)
    {
        const string source = """
                              #define NET
                              #if NET
                              class C
                              {
                              #if DEBUG
                                  private int _trace;
                              #endif
                                  private int {|RH2001:Value|} { get; }

                                  int Read() => Value;
                              }
                              #endif
                              """;

        const string fixedSource = """
                                   #define NET
                                   #if NET
                                   class C
                                   {
                                   #if DEBUG
                                       private int _trace;
                                   #endif
                                       private readonly int _value;

                                       int Read() => _value;
                                   }
                                   #endif
                                   """;

        await Verify(WithEndOfLine(source, endOfLine), WithEndOfLine(fixedSource, endOfLine));
    }

    /// <summary>
    /// Verifies that Fix All replaces the property behind which a group of a whole member stands and leaves the property
    /// behind which an excluded initializer stands reported without a fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-010")]
    [Requirement("SHARED-FIX-002")]
    public async Task VerifyFixAllSkipsThePropertyBehindWhichAnExcludedInitializerStands()
    {
        const string testData = """
                                class C
                                {
                                    private int {|RH2001:Second|} { get; }
                                #if DEBUG
                                    private int _trace;
                                #endif

                                    private int {|RH2001:Third|} { get; }

                                    private int {|RH2001:First|} { get; }
                                #if X
                                    = 5;
                                #endif

                                    int Read() => First + Second + Third;
                                }
                                """;

        const string resultData = """
                                  class C
                                  {
                                      private readonly int _second;
                                  #if DEBUG
                                      private int _trace;
                                  #endif

                                      private readonly int _third;

                                      private int {|RH2001:First|} { get; }
                                  #if X
                                      = 5;
                                  #endif

                                      int Read() => First + _second + _third;
                                  }
                                  """;

        await Verify(testData,
                     resultData,
                     test =>
                     {
                         // The diagnostic whose fix is withheld stays and is marked in the expected source
                         test.FixedState.MarkupHandling = MarkupMode.Allow;
                         test.BatchFixedState.MarkupHandling = MarkupMode.Allow;
                     });
    }

    /// <summary>
    /// Verifies that Fix All replaces the property behind a group of a whole member and leaves the property behind a group
    /// holding <c>extern</c> reported without a fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-010")]
    [Requirement("SHARED-FIX-002")]
    public async Task VerifyFixAllSkipsThePropertyBehindAConditionalExternInFrontOfTheFirstModifier()
    {
        const string testData = """
                                class C
                                {
                                #if DEBUG
                                    extern
                                #endif
                                    private int {|RH2001:First|} { get; }

                                #if DEBUG
                                    private int _trace;
                                #endif
                                    private int {|RH2001:Second|} { get; }

                                    private int {|RH2001:Third|} { get; }

                                    int Read() => First + Second + Third;
                                }
                                """;

        const string resultData = """
                                  class C
                                  {
                                  #if DEBUG
                                      extern
                                  #endif
                                      private int {|RH2001:First|} { get; }

                                  #if DEBUG
                                      private int _trace;
                                  #endif
                                      private readonly int _second;

                                      private readonly int _third;

                                      int Read() => First + _second + _third;
                                  }
                                  """;

        await Verify(testData,
                     resultData,
                     test =>
                     {
                         // The diagnostic whose fix is withheld stays and is marked in the expected source
                         test.FixedState.MarkupHandling = MarkupMode.Allow;
                         test.BatchFixedState.MarkupHandling = MarkupMode.Allow;
                     });
    }

    /// <summary>
    /// Verifies that the fix, which lays out its result through the formatter, is not offered in a file with a syntax error,
    /// while the diagnostic is still reported, and that it is offered once the syntax error is gone
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-011")]
    public async Task NoFixIsOfferedInAFileWithASyntaxError()
    {
        const string source = """
                              namespace Reihitsu.Analyzer.Test.Design.Resources
                              {
                                  internal class RH2001
                                  {
                                      public int Sum()
                                      {
                                          return Alpha + Beta;
                                      }

                                      private int Alpha { get; set; }

                                      private int Beta { get; set; }
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOfferedOnceASyntaxErrorIsAdded(source);
    }

    /// <summary>
    /// Verifies that the fix is withheld for an inherited field that is read unqualified, which the rename would rewrite to
    /// <c>base._value</c>, while the property is still reported
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH2001-011")]
    public async Task VerifyNoFixIsOfferedWhenAnInheritedFieldIsReadUnqualified(string endOfLine)
    {
        const string testData = """
                                class Base
                                {
                                    protected int _value;
                                }

                                class Sample : Base
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    int Get()
                                    {
                                        return Value + _value;
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(testData, endOfLine));
    }

    /// <summary>
    /// Verifies that the fix is withheld for an inherited field read through <see langword="this"/> and through a receiver
    /// of the type, which would then read the new field
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyNoFixIsOfferedWhenAnInheritedFieldIsReadThroughThisAndThroughAReceiver()
    {
        const string testData = """
                                class Base
                                {
                                    protected int _value;
                                }

                                class Sample : Base
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    int Get(Sample other)
                                    {
                                        return Value + this._value + other._value;
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that the fix is withheld for an inherited field read only through <see langword="this"/>, whose text the
    /// rename would leave as written while it would then read the new field
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyNoFixIsOfferedWhenAnInheritedFieldIsReadOnlyThroughThis()
    {
        const string testData = """
                                class Base
                                {
                                    protected int _value;
                                }

                                class Sample : Base
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    int Get()
                                    {
                                        return Value + this._value;
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that the fix is withheld for an inherited field read only through a receiver of the type, whose text the
    /// rename would leave as written while it would then read the new field
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyNoFixIsOfferedWhenAnInheritedFieldIsReadOnlyThroughAReceiver()
    {
        const string testData = """
                                class Base
                                {
                                    protected int _value;
                                }

                                class Sample : Base
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    int Get(Sample other)
                                    {
                                        return Value + other._value;
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that the fix is withheld for an inherited property named like the field, read through
    /// <see langword="this"/>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyNoFixIsOfferedWhenAnInheritedPropertyIsReadThroughThis()
    {
        const string testData = """
                                class Base
                                {
                                    protected int _value { get; set; }
                                }

                                class Sample : Base
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    int Get()
                                    {
                                        return Value + this._value;
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that the fix is withheld for an inherited <see langword="protected internal"/> field read through a
    /// receiver inside a nested type, which would then read the new field
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyNoFixIsOfferedWhenAnInheritedFieldIsReadThroughAReceiverInANestedType()
    {
        const string testData = """
                                class Base
                                {
                                    protected internal int _value;
                                }

                                class Sample : Base
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    int Get() => Value;

                                    class Nested
                                    {
                                        int Read(Sample sample)
                                        {
                                            return sample._value;
                                        }
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that the fix is withheld for an extension method named like the field of a delegate-typed property, whose
    /// call the rename would turn into a static call
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyNoFixIsOfferedWhenAnExtensionMethodIsNamedLikeTheFieldOfADelegateTypedProperty()
    {
        const string testData = """
                                using System;

                                static class Extensions
                                {
                                    public static int _value(this Sample sample) => 1;
                                }

                                class Sample
                                {
                                    private Func<int> {|RH2001:Value|} { get; set; } = () => 2;

                                    int Get()
                                    {
                                        return Value() + this._value();
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that the fix is offered next to an extension method named like the field of a property whose type is not
    /// invocable, whose call keeps its text and its binding
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyFixIsOfferedNextToAnExtensionMethodNamedLikeTheFieldWhoseCallKeepsItsBinding()
    {
        const string testData = """
                                static class Extensions
                                {
                                    public static int _value(this Sample sample) => 1;
                                }

                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    int Get()
                                    {
                                        return Value + this._value();
                                    }
                                }
                                """;

        const string resultData = """
                                  static class Extensions
                                  {
                                      public static int _value(this Sample sample) => 1;
                                  }

                                  class Sample
                                  {
                                      private int _value;

                                      int Get()
                                      {
                                          return _value + this._value();
                                      }
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies that the fix is withheld for a statically imported member named like the field, which the rename would
    /// qualify with its type
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyNoFixIsOfferedWhenAStaticallyImportedMemberIsNamedLikeTheField()
    {
        const string testData = """
                                using static Holder;

                                static class Holder
                                {
                                    public static int _value = 1;
                                }

                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    int Get()
                                    {
                                        return Value + _value;
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that the fix is withheld for a static member of the enclosing type named like the field, which the rename
    /// would qualify with the enclosing type
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyNoFixIsOfferedWhenAMemberOfTheEnclosingTypeIsNamedLikeTheField()
    {
        const string testData = """
                                class Outer
                                {
                                    private static int _value = 1;

                                    class Sample
                                    {
                                        private int {|RH2001:Value|} { get; set; }

                                        int Get()
                                        {
                                            return Value + _value;
                                        }
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that the fix is withheld when the renamed property reference would read the parameter of a simple lambda
    /// with an inferred type, returned from a block body, instead of the field
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyNoFixIsOfferedWhenASimpleLambdaParameterInABlockBodyIsNamedLikeTheField()
    {
        const string testData = """
                                using System;

                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    Func<int, int> Add()
                                    {
                                        return _value => Value + _value;
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that the fix is offered for a parameter of a simple lambda with an inferred type in an expression body
    /// named like the field, and qualifies the renamed property reference with <see langword="this"/>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyFixQualifiesThePropertyReferenceNextToASimpleLambdaParameterInAnExpressionBodyNamedLikeTheField()
    {
        const string testData = """
                                using System;

                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    Func<int, int> Add() => _value => Value + _value;
                                }
                                """;

        const string resultData = """
                                  using System;

                                  class Sample
                                  {
                                      private int _value;

                                      Func<int, int> Add() => _value => this._value + _value;
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies that the fix is withheld when the renamed property reference would read the parameter of a parenthesized
    /// lambda with an inferred type, returned from a block body, instead of the field
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyNoFixIsOfferedWhenAParenthesizedLambdaParameterWithAnInferredTypeInABlockBodyIsNamedLikeTheField()
    {
        const string testData = """
                                using System;

                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    Func<int, int> Add()
                                    {
                                        return (_value) => Value + _value;
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that the fix is offered for a parameter of a parenthesized lambda with an inferred type in an expression
    /// body named like the field, and qualifies the renamed property reference with <see langword="this"/>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyFixQualifiesThePropertyReferenceNextToAParenthesizedLambdaParameterWithAnInferredTypeInAnExpressionBodyNamedLikeTheField()
    {
        const string testData = """
                                using System;

                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    Func<int, int> Add() => (_value) => Value + _value;
                                }
                                """;

        const string resultData = """
                                  using System;

                                  class Sample
                                  {
                                      private int _value;

                                      Func<int, int> Add() => (_value) => this._value + _value;
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies that the fix is withheld for a static property when the renamed reference inside the block body of a static
    /// method would read the parameter of a simple lambda instead of the field
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyNoFixIsOfferedWhenALambdaParameterInTheBlockBodyOfAStaticMethodIsNamedLikeTheFieldOfAStaticProperty()
    {
        const string testData = """
                                using System;

                                class Sample
                                {
                                    private static int {|RH2001:Value|} { get; set; }

                                    static Func<int, int> Add()
                                    {
                                        return _value => Value + _value;
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that the fix is offered for a static property next to a lambda parameter named like the field in the
    /// expression body of a static method, and qualifies the renamed reference with the type
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyFixQualifiesTheStaticPropertyReferenceWithTheTypeNextToALambdaParameterInAnExpressionBodyNamedLikeTheField()
    {
        const string testData = """
                                using System;

                                class Sample
                                {
                                    private static int {|RH2001:Value|} { get; set; }

                                    static Func<int, int> Add() => _value => Value + _value;
                                }
                                """;

        const string resultData = """
                                  using System;

                                  class Sample
                                  {
                                      private static int _value;

                                      static Func<int, int> Add() => _value => Sample._value + _value;
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies that the fix is withheld when the renamed property reference would read a variable that
    /// <c>out var</c> declares in the same expression instead of the field
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyNoFixIsOfferedWhenAnOutVarVariableOfTheSameExpressionIsNamedLikeTheField()
    {
        const string testData = """
                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    int Parse(string input) => int.TryParse(input, out var _value) ? Value + _value : 0;
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that the fix is withheld when the renamed property reference would read a variable that an <c>out</c>
    /// argument with an explicit type declares in the same expression instead of the field
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyNoFixIsOfferedWhenAnExplicitlyTypedOutVariableOfTheSameExpressionIsNamedLikeTheField()
    {
        const string testData = """
                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    int Parse(string input) => int.TryParse(input, out int _value) ? Value + _value : 0;
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that the fix is withheld when the renamed property reference would read a primary-constructor parameter
    /// named like the field instead of the field
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyNoFixIsOfferedWhenAPrimaryConstructorParameterIsNamedLikeTheField()
    {
        const string testData = """
                                class Sample(int _value)
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    int Get() => Value + _value;
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that the fix is withheld when the reference whose binding the rename would change stands in another
    /// document, in another <c>partial</c> part of the type: an inherited field read through <see langword="this"/>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyNoFixIsOfferedWhenAnInheritedFieldIsReadInAPartialPartInAnotherDocument()
    {
        const string source = """
                              class Base
                              {
                                  protected int _value;
                              }

                              partial class Sample : Base
                              {
                                  private int {|RH2001:Value|} { get; set; }

                                  int Get() => Value;
                              }
                              """;
        const string otherSource = """
                                   partial class Sample
                                   {
                                       int Read()
                                       {
                                           return this._value;
                                       }
                                   }
                                   """;

        await Verify(source,
                     source,
                     test =>
                     {
                         test.TestState.Sources.Add(("/0/Test1.cs", otherSource));
                         test.FixedState.Sources.Add(("/0/Test1.cs", otherSource));
                     });
    }

    /// <summary>
    /// Verifies that the fix is withheld when the renamed property reference that would read a lambda parameter instead of
    /// the field stands in a block body in another document, in another <c>partial</c> part of the type
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyNoFixIsOfferedWhenALambdaParameterInAPartialPartInAnotherDocumentIsNamedLikeTheField()
    {
        const string source = """
                              partial class Sample
                              {
                                  private int {|RH2001:Value|} { get; set; }
                              }
                              """;
        const string otherSource = """
                                   using System;

                                   partial class Sample
                                   {
                                       Func<int, int> Add()
                                       {
                                           return _value => Value + _value;
                                       }
                                   }
                                   """;

        await Verify(source,
                     source,
                     test =>
                     {
                         test.TestState.Sources.Add(("/0/Test1.cs", otherSource));
                         test.FixedState.Sources.Add(("/0/Test1.cs", otherSource));
                     });
    }

    /// <summary>
    /// Verifies that the fix is withheld when the renamed property reference would read the parameter of a lambda with an
    /// inferred type that initializes a local declaration instead of the field
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyNoFixIsOfferedWhenALambdaParameterInALocalDeclarationIsNamedLikeTheField()
    {
        const string testData = """
                                using System;

                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    int Get()
                                    {
                                        Func<int, int> add = _value => Value + _value;

                                        return add(1);
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that the fix is withheld when the renamed property reference would write the parameter of a lambda that is
    /// assigned in a statement instead of the field
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyNoFixIsOfferedWhenALambdaParameterInAnAssignmentStatementIsNamedLikeTheField()
    {
        const string testData = """
                                using System;

                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    int Get()
                                    {
                                        Func<int, int> add;
                                        add = _value => Value = _value;

                                        return add(1);
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that the fix is withheld when the renamed property reference would read the parameter of a lambda that a
    /// constructor assigns instead of the field
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyNoFixIsOfferedWhenALambdaParameterInAConstructorIsNamedLikeTheField()
    {
        const string testData = """
                                using System;

                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    private readonly Func<int, int> _add;

                                    Sample()
                                    {
                                        _add = _value => Value + _value;
                                    }

                                    int Get() => _add(1);
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that the fix is withheld when the renamed property reference would read the parameter of an explicitly
    /// typed lambda in a block body instead of the field
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyNoFixIsOfferedWhenAnExplicitlyTypedLambdaParameterInABlockBodyIsNamedLikeTheField()
    {
        const string testData = """
                                using System;

                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    int Get()
                                    {
                                        Func<int, int> add = (int _value) => Value + _value;

                                        return add(1);
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that the fix is withheld when the renamed property reference would read a query range variable named like
    /// the field instead of the field
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyNoFixIsOfferedWhenAQueryRangeVariableIsNamedLikeTheField()
    {
        const string testData = """
                                using System.Linq;

                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    int Sum() => (from _value in Enumerable.Range(0, 3) select Value + _value).Sum();
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that the fix is offered when a lambda parameter named like the field stands in an expression body in
    /// another document, in another <c>partial</c> part of the type, and qualifies the renamed property reference there
    /// with <see langword="this"/>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyFixQualifiesThePropertyReferenceNextToALambdaParameterInAnExpressionBodyInAPartialPartInAnotherDocument()
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
                                   using System;

                                   partial class Sample
                                   {
                                       Func<int, int> Add() => _value => Value + _value;
                                   }
                                   """;
        const string fixedOtherSource = """
                                        using System;

                                        partial class Sample
                                        {
                                            Func<int, int> Add() => _value => this._value + _value;
                                        }
                                        """;

        await Verify(source,
                     fixedSource,
                     test =>
                     {
                         test.TestState.Sources.Add(("/0/Test1.cs", otherSource));
                         test.FixedState.Sources.Add(("/0/Test1.cs", fixedOtherSource));
                     });
    }

    /// <summary>
    /// Verifies that Fix All leaves a property whose fix is withheld because an inherited field is named like its field,
    /// and replaces another property of the same type
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyFixAllLeavesAPropertyWhoseFieldNameIsBoundAndReplacesTheOthers()
    {
        const string testData = """
                                class Base
                                {
                                    protected int _value;
                                }

                                class Sample : Base
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    private int {|RH2001:Count|} { get; set; }

                                    private int {|RH2001:Total|} { get; set; }

                                    int Get()
                                    {
                                        return Value + _value + Count + Total;
                                    }
                                }
                                """;

        const string resultData = """
                                  class Base
                                  {
                                      protected int _value;
                                  }

                                  class Sample : Base
                                  {
                                      private int {|RH2001:Value|} { get; set; }

                                      private int _count;

                                      private int _total;

                                      int Get()
                                      {
                                          return Value + _value + _count + _total;
                                      }
                                  }
                                  """;

        await Verify(testData,
                     resultData,
                     test =>
                     {
                         test.FixedState.MarkupHandling = MarkupMode.Allow;
                         test.BatchFixedState.MarkupHandling = MarkupMode.Allow;
                     });
    }

    /// <summary>
    /// Verifies that the fix is offered for a method parameter named like the field next to an inherited field of that
    /// name, and qualifies the renamed property reference with <see langword="this"/>
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH2001-011")]
    public async Task VerifyFixQualifiesThePropertyReferenceNextToAParameterNamedLikeTheFieldAndAnInheritedField(string endOfLine)
    {
        const string testData = """
                                class Base
                                {
                                    protected int _value;
                                }

                                class Sample : Base
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    int Get(int _value)
                                    {
                                        return Value + _value;
                                    }
                                }
                                """;

        const string resultData = """
                                  class Base
                                  {
                                      protected int _value;
                                  }

                                  class Sample : Base
                                  {
                                      private int _value;

                                      int Get(int _value)
                                      {
                                          return this._value + _value;
                                      }
                                  }
                                  """;

        await Verify(WithEndOfLine(testData, endOfLine), WithEndOfLine(resultData, endOfLine));
    }

    /// <summary>
    /// Verifies that the fix is offered next to an inherited method named like the field, which the call still reaches
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyFixIsOfferedNextToAnInheritedMethodNamedLikeTheField()
    {
        const string testData = """
                                class Base
                                {
                                    protected int _value() => 1;
                                }

                                class Sample : Base
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    int Get()
                                    {
                                        return Value + _value();
                                    }
                                }
                                """;

        const string resultData = """
                                  class Base
                                  {
                                      protected int _value() => 1;
                                  }

                                  class Sample : Base
                                  {
                                      private int _value;

                                      int Get()
                                      {
                                          return _value + _value();
                                      }
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies that the fix is offered next to an inherited field named like the field that no reference names
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyFixIsOfferedNextToAnInheritedFieldThatNoReferenceNames()
    {
        const string testData = """
                                class Base
                                {
                                    protected int _value;
                                }

                                class Sample : Base
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    int Get()
                                    {
                                        return Value;
                                    }
                                }
                                """;

        const string resultData = """
                                  class Base
                                  {
                                      protected int _value;
                                  }

                                  class Sample : Base
                                  {
                                      private int _value;

                                      int Get()
                                      {
                                          return _value;
                                      }
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies that the fix is offered for a local declared by a statement and named like the field, and qualifies the
    /// renamed property reference with <see langword="this"/>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyFixQualifiesThePropertyReferenceNextToALocalNamedLikeTheField()
    {
        const string testData = """
                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    int Get()
                                    {
                                        var _value = 1;

                                        return Value + _value;
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

                                          return this._value + _value;
                                      }
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies that the fix is offered for a method parameter named like the field in a type without a base type, and
    /// qualifies the renamed property reference with <see langword="this"/>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyFixQualifiesThePropertyReferenceNextToAMethodParameterNamedLikeTheField()
    {
        const string testData = """
                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    int Get(int _value) => Value + _value;
                                }
                                """;

        const string resultData = """
                                  class Sample
                                  {
                                      private int _value;

                                      int Get(int _value) => this._value + _value;
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies that the fix is offered for a parameter of an explicitly typed lambda named like the field, and qualifies
    /// the renamed property reference with <see langword="this"/>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyFixQualifiesThePropertyReferenceNextToAnExplicitlyTypedLambdaParameterNamedLikeTheField()
    {
        const string testData = """
                                using System;

                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    Func<int, int> Add() => (int _value) => Value + _value;
                                }
                                """;

        const string resultData = """
                                  using System;

                                  class Sample
                                  {
                                      private int _value;

                                      Func<int, int> Add() => (int _value) => this._value + _value;
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies that the fix is offered for a parameter of an anonymous method named like the field, and qualifies the
    /// renamed property reference with <see langword="this"/>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyFixQualifiesThePropertyReferenceNextToAnAnonymousMethodParameterNamedLikeTheField()
    {
        const string testData = """
                                using System;

                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    Func<int, int> Add()
                                    {
                                        return delegate (int _value) { return Value + _value; };
                                    }
                                }
                                """;

        const string resultData = """
                                  using System;

                                  class Sample
                                  {
                                      private int _value;

                                      Func<int, int> Add()
                                      {
                                          return delegate (int _value) { return this._value + _value; };
                                      }
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies that the fix is offered for a parameter of a local function named like the field, and qualifies the
    /// renamed property reference with <see langword="this"/>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyFixQualifiesThePropertyReferenceNextToALocalFunctionParameterNamedLikeTheField()
    {
        const string testData = """
                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    int Get()
                                    {
                                        return Local(1);

                                        int Local(int _value) => Value + _value;
                                    }
                                }
                                """;

        const string resultData = """
                                  class Sample
                                  {
                                      private int _value;

                                      int Get()
                                      {
                                          return Local(1);

                                          int Local(int _value) => this._value + _value;
                                      }
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies that the fix is offered for pattern variables named like the field, declared by a type pattern and by a
    /// <see langword="var"/> pattern, and qualifies the renamed property references with <see langword="this"/>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyFixQualifiesThePropertyReferenceNextToAPatternVariableNamedLikeTheField()
    {
        const string testData = """
                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    int Get(object input)
                                    {
                                        if (input is int _value)
                                        {
                                            return Value + _value;
                                        }

                                        return 0;
                                    }

                                    int Read(int input) => input is var _value ? Value + _value : 0;
                                }
                                """;

        const string resultData = """
                                  class Sample
                                  {
                                      private int _value;

                                      int Get(object input)
                                      {
                                          if (input is int _value)
                                          {
                                              return this._value + _value;
                                          }

                                          return 0;
                                      }

                                      int Read(int input) => input is var _value ? this._value + _value : 0;
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies that the fix is offered for a variable that <c>out var</c> declares in a statement in front of the use, and
    /// qualifies the renamed property reference with <see langword="this"/>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyFixQualifiesThePropertyReferenceNextToAnOutVarVariableOfAnEarlierStatement()
    {
        const string testData = """
                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    int Parse(string input)
                                    {
                                        int.TryParse(input, out var _value);

                                        return Value + _value;
                                    }
                                }
                                """;

        const string resultData = """
                                  class Sample
                                  {
                                      private int _value;

                                      int Parse(string input)
                                      {
                                          int.TryParse(input, out var _value);

                                          return this._value + _value;
                                      }
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies that the fix is offered for <see langword="foreach"/>, <see langword="catch"/>, query range, and
    /// deconstruction variables named like the field, and qualifies the renamed property references with
    /// <see langword="this"/>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyFixQualifiesThePropertyReferenceNextToOtherInnerScopeVariablesNamedLikeTheField()
    {
        const string testData = """
                                using System;
                                using System.Collections.Generic;
                                using System.Linq;

                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    int Sum(List<int> items)
                                    {
                                        var sum = 0;

                                        foreach (var _value in items)
                                        {
                                            sum += Value + _value;
                                        }

                                        return sum;
                                    }

                                    int Fail()
                                    {
                                        try
                                        {
                                            return 0;
                                        }
                                        catch (Exception _value)
                                        {
                                            return Value + _value.Message.Length;
                                        }
                                    }

                                    IEnumerable<int> Query(List<int> items) => from _value in items select Value + _value;

                                    int Split()
                                    {
                                        var (_value, other) = (1, 2);

                                        return Value + _value + other;
                                    }
                                }
                                """;

        const string resultData = """
                                  using System;
                                  using System.Collections.Generic;
                                  using System.Linq;

                                  class Sample
                                  {
                                      private int _value;

                                      int Sum(List<int> items)
                                      {
                                          var sum = 0;

                                          foreach (var _value in items)
                                          {
                                              sum += this._value + _value;
                                          }

                                          return sum;
                                      }

                                      int Fail()
                                      {
                                          try
                                          {
                                              return 0;
                                          }
                                          catch (Exception _value)
                                          {
                                              return this._value + _value.Message.Length;
                                          }
                                      }

                                      IEnumerable<int> Query(List<int> items) => from _value in items select this._value + _value;

                                      int Split()
                                      {
                                          var (_value, other) = (1, 2);

                                          return this._value + _value + other;
                                      }
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies that the fix is offered for a local function named like the field, and qualifies the renamed property
    /// reference with <see langword="this"/> while the call still reaches the local function
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyFixQualifiesThePropertyReferenceNextToALocalFunctionNamedLikeTheField()
    {
        const string testData = """
                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    int Get()
                                    {
                                        return Value + _value();

                                        int _value() => 1;
                                    }
                                }
                                """;

        const string resultData = """
                                  class Sample
                                  {
                                      private int _value;

                                      int Get()
                                      {
                                          return this._value + _value();

                                          int _value() => 1;
                                      }
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies that the fix is offered for an outer local named like the field and captured by a lambda, and qualifies the
    /// renamed property reference with <see langword="this"/>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyFixQualifiesThePropertyReferenceNextToACapturedLocalNamedLikeTheField()
    {
        const string testData = """
                                using System;

                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    Func<int> Get()
                                    {
                                        var _value = 1;

                                        return () => Value + _value;
                                    }
                                }
                                """;

        const string resultData = """
                                  using System;

                                  class Sample
                                  {
                                      private int _value;

                                      Func<int> Get()
                                      {
                                          var _value = 1;

                                          return () => this._value + _value;
                                      }
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies that the fix is offered next to a member named like the field that a nested type declares and uses, which
    /// keeps its text and its binding
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyFixIsOfferedNextToAMemberOfANestedTypeNamedLikeTheField()
    {
        const string testData = """
                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    int Get() => Value + new Inner().Read();

                                    class Inner
                                    {
                                        private int _value = 1;

                                        public int Read() => _value;
                                    }
                                }
                                """;

        const string resultData = """
                                  class Sample
                                  {
                                      private int _value;

                                      int Get() => _value + new Inner().Read();

                                      class Inner
                                      {
                                          private int _value = 1;

                                          public int Read() => _value;
                                      }
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies that no code fix is offered when a reference to the property gives an anonymous-object member its inferred
    /// name, which the rename would change so that a read of the member no longer compiles
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyNoFixIsOfferedWhenAPropertyReferenceGivesAnAnonymousObjectMemberItsInferredName()
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

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that no code fix is offered when a reference to the property qualified with <c>this.</c> gives an
    /// anonymous-object member its inferred name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyNoFixIsOfferedWhenAThisQualifiedPropertyReferenceGivesAnAnonymousObjectMemberItsInferredName()
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

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that no code fix is offered when a reference to the property through another instance of the type gives an
    /// anonymous-object member its inferred name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyNoFixIsOfferedWhenAPropertyReferenceThroughAnotherInstanceGivesAnAnonymousObjectMemberItsInferredName()
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

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that no code fix is offered when a reference to the property gives a member of an anonymous object in a
    /// LINQ projection its inferred name, which a later projection reads
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyNoFixIsOfferedWhenAPropertyReferenceGivesAMemberOfAnAnonymousObjectInAQueryProjectionItsInferredName()
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

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that no code fix is offered when a reference to the property gives a tuple element its inferred name, which
    /// the rename would change so that a read of the element no longer compiles
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyNoFixIsOfferedWhenAPropertyReferenceGivesATupleElementItsInferredName()
    {
        const string testData = """
                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    int Get()
                                    {
                                        var pair = (Value, 1);

                                        return pair.Value;
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that no code fix is offered when a reference to the property gives an anonymous-object member its inferred
    /// name that <c>ToString()</c> prints, which the rename would change at run time although the code still compiles
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyNoFixIsOfferedWhenAPropertyReferenceGivesAnAnonymousObjectMemberItsInferredNameThatToStringPrints()
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

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that the fix is offered when a reference to the property is the value of an anonymous-object member with an
    /// explicit name, which keeps its name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyFixIsOfferedWhenAPropertyReferenceIsTheValueOfAnExplicitlyNamedAnonymousObjectMember()
    {
        const string testData = """
                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    int Get()
                                    {
                                        var x = new { Value = Value };

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
                                          var x = new { Value = _value };

                                          return x.Value;
                                      }
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies that the fix is offered when a reference to the property is the value of a tuple element with an explicit
    /// name, which keeps its name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyFixIsOfferedWhenAPropertyReferenceIsTheValueOfAnExplicitlyNamedTupleElement()
    {
        const string testData = """
                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    int Get()
                                    {
                                        var t = (Value: Value, 1);

                                        return t.Value;
                                    }
                                }
                                """;

        const string resultData = """
                                  class Sample
                                  {
                                      private int _value;

                                      int Get()
                                      {
                                          var t = (Value: _value, 1);

                                          return t.Value;
                                      }
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies that no code fix is offered when a reference to the property in an expression body gives an
    /// anonymous-object member its inferred name that <c>ToString()</c> prints
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyNoFixIsOfferedWhenAPropertyReferenceInAnExpressionBodyGivesAnAnonymousObjectMemberItsInferredName()
    {
        const string testData = """
                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    string Describe() => new { Value }.ToString();
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies the specification example of an anonymous-object member and a tuple element with explicit names next to
    /// each other, which keep their names while the fix renames the references to the property
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyFixIsOfferedWhenPropertyReferencesAreTheValuesOfAnExplicitlyNamedAnonymousObjectMemberAndTupleElement()
    {
        const string testData = """
                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    int Get()
                                    {
                                        var pair = new { Value = Value };
                                        var tuple = (Value: Value, 1);

                                        return pair.Value + tuple.Value;
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
                                          var tuple = (Value: _value, 1);

                                          return pair.Value + tuple.Value;
                                      }
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies that the fix is offered when the property is the receiver of a member of another type named like the field,
    /// which keeps its binding while the receiver is renamed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyFixIsOfferedWhenThePropertyIsTheReceiverOfAMemberNamedLikeTheField()
    {
        const string testData = """
                                class Outer
                                {
                                    private int _value;

                                    class Inner
                                    {
                                        private Outer {|RH2001:Value|} { get; set; }

                                        int Get()
                                        {
                                            return Value._value;
                                        }
                                    }
                                }
                                """;

        const string resultData = """
                                  class Outer
                                  {
                                      private int _value;

                                      class Inner
                                      {
                                          private Outer _value;

                                          int Get()
                                          {
                                              return _value._value;
                                          }
                                      }
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies that the fix is offered when the property, qualified with <c>this.</c>, is the receiver of a member of
    /// another type named like the field
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyFixIsOfferedWhenTheThisQualifiedPropertyIsTheReceiverOfAMemberNamedLikeTheField()
    {
        const string testData = """
                                class Outer
                                {
                                    private int _value;

                                    class Inner
                                    {
                                        private Outer {|RH2001:Value|} { get; set; }

                                        int Get()
                                        {
                                            return this.Value._value;
                                        }
                                    }
                                }
                                """;

        const string resultData = """
                                  class Outer
                                  {
                                      private int _value;

                                      class Inner
                                      {
                                          private Outer _value;

                                          int Get()
                                          {
                                              return this._value._value;
                                          }
                                      }
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies that the fix is offered when the property with the null-forgiving operator is the receiver of a member of
    /// another type named like the field
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyFixIsOfferedWhenTheNullForgivingPropertyIsTheReceiverOfAMemberNamedLikeTheField()
    {
        const string testData = """
                                #nullable enable
                                class Outer
                                {
                                    private int _value;

                                    class Inner
                                    {
                                        private Outer? {|RH2001:Value|} { get; set; }

                                        int Get()
                                        {
                                            return Value!._value;
                                        }
                                    }
                                }
                                """;

        const string resultData = """
                                  #nullable enable
                                  class Outer
                                  {
                                      private int _value;

                                      class Inner
                                      {
                                          private Outer? _value;

                                          int Get()
                                          {
                                              return _value!._value;
                                          }
                                      }
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies that the fix is offered when the property, cast to another type, is the receiver of a member of that type
    /// named like the field
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyFixIsOfferedWhenTheCastPropertyIsTheReceiverOfAMemberNamedLikeTheField()
    {
        const string testData = """
                                class Outer
                                {
                                    private int _value;

                                    class Inner
                                    {
                                        private object {|RH2001:Value|} { get; set; }

                                        int Get()
                                        {
                                            return ((Outer)Value)._value;
                                        }
                                    }
                                }
                                """;

        const string resultData = """
                                  class Outer
                                  {
                                      private int _value;

                                      class Inner
                                      {
                                          private object _value;

                                          int Get()
                                          {
                                              return ((Outer)_value)._value;
                                          }
                                      }
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies that the fix is offered when the property is the conditional receiver of a member of another type named
    /// like the field
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyFixIsOfferedWhenThePropertyIsTheConditionalReceiverOfAMemberNamedLikeTheField()
    {
        const string testData = """
                                class Outer
                                {
                                    private int _value;

                                    class Inner
                                    {
                                        private Outer {|RH2001:Value|} { get; set; }

                                        int? Get()
                                        {
                                            return Value?._value;
                                        }
                                    }
                                }
                                """;

        const string resultData = """
                                  class Outer
                                  {
                                      private int _value;

                                      class Inner
                                      {
                                          private Outer _value;

                                          int? Get()
                                          {
                                              return _value?._value;
                                          }
                                      }
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies that no code fix is offered when a <c>cref</c> in a documentation comment names an inherited member like
    /// the field, which would bind to the new field after the rename
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyNoFixIsOfferedWhenADocumentationCommentReferenceNamesAnInheritedMemberLikeTheField()
    {
        const string testData = """
                                class Base
                                {
                                    protected int _value;
                                }

                                class Sample : Base
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    /// <summary>
                                    /// Reads <see cref="_value"/>.
                                    /// </summary>
                                    public int Get()
                                    {
                                        return Value;
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that the fix is offered when a <c>cref</c> in a documentation comment names an inherited member like the
    /// field through its type, which keeps its binding
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyFixIsOfferedWhenADocumentationCommentReferenceNamesAnInheritedMemberLikeTheFieldThroughItsType()
    {
        const string testData = """
                                class Base
                                {
                                    protected int _value;
                                }

                                class Sample : Base
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    /// <summary>
                                    /// Reads <see cref="Base._value"/>
                                    /// </summary>
                                    /// <returns>Value</returns>
                                    public int Get()
                                    {
                                        return Value;
                                    }
                                }
                                """;

        const string resultData = """
                                  class Base
                                  {
                                      protected int _value;
                                  }

                                  class Sample : Base
                                  {
                                      private int _value;

                                      /// <summary>
                                      /// Reads <see cref="Base._value"/>
                                      /// </summary>
                                      /// <returns>Value</returns>
                                      public int Get()
                                      {
                                          return _value;
                                      }
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies that the fix renames a <c>cref</c> in a documentation comment that names the property
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyFixRenamesADocumentationCommentReferenceToTheProperty()
    {
        const string testData = """
                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    /// <summary>
                                    /// Reads <see cref="Value"/>
                                    /// </summary>
                                    /// <returns>Value</returns>
                                    public int Get()
                                    {
                                        return Value;
                                    }
                                }
                                """;

        const string resultData = """
                                  class Sample
                                  {
                                      private int _value;

                                      /// <summary>
                                      /// Reads <see cref="_value"/>
                                      /// </summary>
                                      /// <returns>Value</returns>
                                      public int Get()
                                      {
                                          return _value;
                                      }
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies that no code fix is offered when the property is read through another construction of its own generic
    /// type, a shape whose fix this rule withholds
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyNoFixIsOfferedWhenThePropertyIsReadThroughAnotherConstructionOfItsGenericType()
    {
        const string testData = """
                                class Sample<T>
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    int Get(Sample<int> other)
                                    {
                                        return other.Value + Value;
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that no code fix is offered when the property is assigned in an object initializer of another construction
    /// of its own generic type, a shape whose fix this rule withholds
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyNoFixIsOfferedWhenThePropertyIsAssignedInAnObjectInitializerOfAnotherConstructionOfItsGenericType()
    {
        const string testData = """
                                class Sample<T>
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    Sample<TOther> Cast<TOther>()
                                    {
                                        return new Sample<TOther> { Value = Value };
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that no code fix is offered when the property, read through another construction of its own generic type,
    /// gives an anonymous-object member its inferred name that code reads
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyNoFixIsOfferedWhenThePropertyReadThroughAnotherConstructionOfItsGenericTypeGivesAnAnonymousObjectMemberItsInferredName()
    {
        const string testData = """
                                class Sample<T>
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    int Get(Sample<int> other)
                                    {
                                        var pair = new { other.Value };

                                        return pair.Value;
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that no code fix is offered when the property is the receiver of a member of another type named like the
    /// field and a local named like the field is in scope, so the rename has to qualify the receiver
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyNoFixIsOfferedWhenThePropertyIsTheReceiverOfAMemberNamedLikeTheFieldNextToALocalNamedLikeTheField()
    {
        const string testData = """
                                class Outer
                                {
                                    private int _value;

                                    class Inner
                                    {
                                        private Outer {|RH2001:Value|} { get; set; }

                                        int Get()
                                        {
                                            var _value = 1;

                                            return Value._value + _value;
                                        }
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that the fix is offered when a constructor deconstructs its parameters into the properties, since a
    /// deconstruction target gives no tuple element a name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyFixIsOfferedWhenAConstructorDeconstructsItsParametersIntoTheProperties()
    {
        const string testData = """
                                class Sample
                                {
                                    internal Sample(int value, string name) => (Value, Name) = (value, name);

                                    private int {|RH2001:Value|} { get; }

                                    private string {|RH2001:Name|} { get; }

                                    string Describe()
                                    {
                                        return Name + Value;
                                    }
                                }
                                """;

        const string resultData = """
                                  class Sample
                                  {
                                      internal Sample(int value, string name) => (_value, _name) = (value, name);

                                      private readonly int _value;

                                      private readonly string _name;

                                      string Describe()
                                      {
                                          return _name + _value;
                                      }
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies that the fix is offered when the property is an element of a tuple converted to the unnamed tuple type a
    /// method returns, which drops the inferred element name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyFixIsOfferedWhenThePropertyIsAnElementOfATupleReturnedAsAnUnnamedTupleType()
    {
        const string testData = """
                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    (int, int) Get()
                                    {
                                        return (Value, 1);
                                    }
                                }
                                """;

        const string resultData = """
                                  class Sample
                                  {
                                      private int _value;

                                      (int, int) Get()
                                      {
                                          return (_value, 1);
                                      }
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies that the fix is offered when the property is an element of a tuple assigned to a local of an explicitly
    /// named tuple type, which replaces the inferred element name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyFixIsOfferedWhenThePropertyIsAnElementOfATupleAssignedToAnExplicitlyTypedTupleLocal()
    {
        const string testData = """
                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    int Get()
                                    {
                                        (int First, int Second) pair = (Value, 1);

                                        return pair.First;
                                    }
                                }
                                """;

        const string resultData = """
                                  class Sample
                                  {
                                      private int _value;

                                      int Get()
                                      {
                                          (int First, int Second) pair = (_value, 1);

                                          return pair.First;
                                      }
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies that the fix is offered when the property is an element of a tuple passed to a parameter of an unnamed
    /// tuple type, which drops the inferred element name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyFixIsOfferedWhenThePropertyIsAnElementOfATuplePassedAsAnUnnamedTupleArgument()
    {
        const string testData = """
                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    int Get()
                                    {
                                        return Use((Value, 1));
                                    }

                                    private static int Use((int, int) pair)
                                    {
                                        return pair.Item1;
                                    }
                                }
                                """;

        const string resultData = """
                                  class Sample
                                  {
                                      private int _value;

                                      int Get()
                                      {
                                          return Use((_value, 1));
                                      }

                                      private static int Use((int, int) pair)
                                      {
                                          return pair.Item1;
                                      }
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies that the fix is offered when the property is a target of a deconstruction assignment, which gives no tuple
    /// element a name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyFixIsOfferedWhenThePropertyIsADeconstructionTarget()
    {
        const string testData = """
                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    void Set((int, int) pair)
                                    {
                                        int other;

                                        (Value, other) = pair;
                                    }

                                    int Get()
                                    {
                                        return Value;
                                    }
                                }
                                """;

        const string resultData = """
                                  class Sample
                                  {
                                      private int _value;

                                      void Set((int, int) pair)
                                      {
                                          int other;

                                          (_value, other) = pair;
                                      }

                                      int Get()
                                      {
                                          return _value;
                                      }
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies that the fix is offered when the property is an element of the tuple a deconstruction assignment reads,
    /// which gives no tuple element a name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyFixIsOfferedWhenThePropertyIsAnElementOfTheSourceOfADeconstruction()
    {
        const string testData = """
                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    void Deconstruct(out int value, out int count)
                                    {
                                        (value, count) = (Value, 1);
                                    }
                                }
                                """;

        const string resultData = """
                                  class Sample
                                  {
                                      private int _value;

                                      void Deconstruct(out int value, out int count)
                                      {
                                          (value, count) = (_value, 1);
                                      }
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies that no code fix is offered when the property is an element of a tuple a switch expression matches,
    /// a tuple element with an inferred name whose fix this rule withholds
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyNoFixIsOfferedWhenThePropertyIsAnElementOfATupleASwitchExpressionMatches()
    {
        const string testData = """
                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    int Get(int input)
                                    {
                                        return (Value, input) switch
                                               {
                                                   (1, 2) => 3,
                                                   _ => 0
                                               };
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that no code fix is offered when the property is an element of a tuple a switch statement matches,
    /// a tuple element with an inferred name whose fix this rule withholds
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyNoFixIsOfferedWhenThePropertyIsAnElementOfATupleASwitchStatementMatches()
    {
        const string testData = """
                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    int Get(int input)
                                    {
                                        switch (Value, input)
                                        {
                                            case (1, 2):
                                                return 3;
                                            default:
                                                return 0;
                                        }
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that no code fix is offered when the property is an element of a tuple nested in another tuple, even when
    /// the outer tuple is converted to a type without element names
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyNoFixIsOfferedWhenThePropertyIsAnElementOfATupleNestedInAnotherTuple()
    {
        const string testData = """
                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    int Get()
                                    {
                                        ((int, int), int) pair = ((Value, 1), 2);

                                        return pair.Item1.Item1;
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that no code fix is offered when the property is an element of a tuple that is the operand of a cast,
    /// even when the cast's type has no element names
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyNoFixIsOfferedWhenThePropertyIsAnElementOfATupleThatIsTheOperandOfACast()
    {
        const string testData = """
                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    object Get()
                                    {
                                        return ((int, int))(Value, 1);
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that no code fix is offered when the property is an element of a tuple that is a branch of a conditional
    /// expression, even when the conditional is returned as a type without element names
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyNoFixIsOfferedWhenThePropertyIsAnElementOfATupleThatIsABranchOfAConditionalExpression()
    {
        const string testData = """
                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    (int, int) Get(bool flag)
                                    {
                                        return flag ? (Value, 1) : (Value, 2);
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that the fix is offered when the property is an element of a tuple nested in a deconstruction target
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyFixIsOfferedWhenThePropertyIsAnElementOfATupleNestedInADeconstructionTarget()
    {
        const string testData = """
                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    int Get(((int, int), int) source)
                                    {
                                        int other;
                                        int x;

                                        ((Value, other), x) = source;

                                        return Value + other + x;
                                    }
                                }
                                """;

        const string resultData = """
                                  class Sample
                                  {
                                      private int _value;

                                      int Get(((int, int), int) source)
                                      {
                                          int other;
                                          int x;

                                          ((_value, other), x) = source;

                                          return _value + other + x;
                                      }
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies that the fix is offered when the property of a generic type is read through a reference typed as the
    /// type's own construction
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyFixIsOfferedWhenThePropertyIsReadThroughTheOwnConstructionOfItsGenericType()
    {
        const string testData = """
                                class Sample<T>
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    int Get(Sample<T> other)
                                    {
                                        return other.Value + Value;
                                    }
                                }
                                """;

        const string resultData = """
                                  class Sample<T>
                                  {
                                      private int _value;

                                      int Get(Sample<T> other)
                                      {
                                          return other._value + _value;
                                      }
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies that the fix is offered when the property is an element of a tuple that is a branch of a conditional
    /// expression whose branches carry different names, so that the conditional's type has no name at that position
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyFixIsOfferedWhenThePropertyIsAnElementOfATupleThatIsABranchOfAConditionalExpressionWithDifferentlyNamedBranches()
    {
        const string testData = """
                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    (int, int) Get(bool flag)
                                    {
                                        return flag ? (Value, 1) : (2, 3);
                                    }
                                }
                                """;

        const string resultData = """
                                  class Sample
                                  {
                                      private int _value;

                                      (int, int) Get(bool flag)
                                      {
                                          return flag ? (_value, 1) : (2, 3);
                                      }
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies that the fix is offered when the property is an element of a tuple whose own conversion goes to
    /// <see cref="object"/>, which is not a tuple type and so keeps no element name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyFixIsOfferedWhenThePropertyIsAnElementOfATupleConvertedToObject()
    {
        const string testData = """
                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    object Get()
                                    {
                                        object pair = (Value, 1);

                                        return pair;
                                    }
                                }
                                """;

        const string resultData = """
                                  class Sample
                                  {
                                      private int _value;

                                      object Get()
                                      {
                                          object pair = (_value, 1);

                                          return pair;
                                      }
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies that the fix is offered when the property is an element of a tuple whose own conversion goes to a
    /// nullable tuple type without element names, which is not a tuple type
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyFixIsOfferedWhenThePropertyIsAnElementOfATupleConvertedToANullableUnnamedTupleType()
    {
        const string testData = """
                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    object Get()
                                    {
                                        (int, int)? pair = (Value, 1);

                                        return pair;
                                    }
                                }
                                """;

        const string resultData = """
                                  class Sample
                                  {
                                      private int _value;

                                      object Get()
                                      {
                                          (int, int)? pair = (_value, 1);

                                          return pair;
                                      }
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies that the fix is offered when the property is an element of a tuple whose own conversion goes to a
    /// nullable tuple type that names the element, which is not a tuple type
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyFixIsOfferedWhenThePropertyIsAnElementOfATupleConvertedToANullableNamedTupleType()
    {
        const string testData = """
                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    int Get()
                                    {
                                        (int Value, int Other)? pair = (Value, 1);

                                        return pair.Value.Value;
                                    }
                                }
                                """;

        const string resultData = """
                                  class Sample
                                  {
                                      private int _value;

                                      int Get()
                                      {
                                          (int Value, int Other)? pair = (_value, 1);

                                          return pair.Value.Value;
                                      }
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies that the fix, which replaces the property by a field, keeps the one-line documentation comment above it
    /// and inserts no blank line between the field in front of it and that comment
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-FIX-006")]
    public async Task VerifyFixKeepsTheOneLineDocumentationCommentAndInsertsNoBlankLine(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    private int _a;
                                    /// <summary>The value.</summary>
                                    private int {|RH2001:Value|} { get; set; }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     private int _a;
                                     /// <summary>The value.</summary>
                                     private int _value;
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testData, fixedData, endOfLine);
    }

    /// <summary>
    /// Verifies that a property the fix replaces by a field, and which shares its line with a field in front of it,
    /// starts on the next line in the column of that line
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-FIX-007")]
    public async Task VerifyFixGivesAFieldThatSharesItsLineWithAFieldInFrontALineOfItsOwn(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    private int _a; private int {|RH2001:Value|} { get; set; }

                                    public int M()
                                    {
                                        Value = 1;
                                        return _a + Value;
                                    }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     private int _a;
                                     private int _value;

                                     public int M()
                                     {
                                         _value = 1;
                                         return _a + _value;
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testData, fixedData, endOfLine);
    }

    /// <summary>
    /// Verifies that a property the fix replaces by a field, and which shares its line with a field behind it, keeps
    /// its own line, and that the field behind it starts on the next line
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-FIX-007")]
    public async Task VerifyFixGivesAFieldThatSharesItsLineWithAFieldBehindItALineOfItsOwn(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    private int {|RH2001:Value|} { get; set; } private int _b;

                                    public int M()
                                    {
                                        Value = 1;
                                        return Value + _b;
                                    }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     private int _value;
                                     private int _b;

                                     public int M()
                                     {
                                         _value = 1;
                                         return _value + _b;
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testData, fixedData, endOfLine);
    }

    /// <summary>
    /// Verifies that the fix for a get-only auto-property with the preview <c>safe</c> modifier writes <c>readonly</c> in
    /// front of <c>safe</c>, the order RH7105 asks for, and that the field compiles
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-004")]
    public async Task VerifyPrivateGetOnlySafeAutoPropertyCodeFixWritesReadonlyInFrontOfSafe()
    {
        const string testData = """
                                internal class RH2001
                                {
                                    private safe int {|#0:Count|} { get; } = 1;
                                }
                                """;

        const string resultData = """
                                  internal class RH2001
                                  {
                                      private readonly safe int _count = 1;
                                  }
                                  """;

        await Verify(testData,
                     resultData,
                     test => test.SolutionTransforms.Add((solution, projectId) => solution.GetProject(projectId)?.ParseOptions is Microsoft.CodeAnalysis.CSharp.CSharpParseOptions parseOptions
                                                                                      ? solution.WithProjectParseOptions(projectId, parseOptions.WithLanguageVersion(Microsoft.CodeAnalysis.CSharp.LanguageVersion.Preview))
                                                                                      : solution),
                     Diagnostics(RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies the specification example: a comment in front of an argument list of the initializer's value withholds the fix,
    /// because the formatter refuses the field the fix builds, and the diagnostic stays
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-007")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task VerifyNoFixIsOfferedForACommentInFrontOfAnArgumentListOfTheInitializerValue(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    private int {|RH2001:Value|} { get; set; } = Offset /* three */ (3);

                                    private static int Offset(int value)
                                    {
                                        return value;
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(ConvertEndOfLine(testData, endOfLine));
    }

    /// <summary>
    /// Verifies the change example: a line comment that keeps the argument list of the initializer's value on the line below
    /// withholds the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-007")]
    public async Task VerifyNoFixIsOfferedForALineCommentInFrontOfAnArgumentListOfTheInitializerValue()
    {
        const string testData = """
                                class C
                                {
                                    private int {|RH2001:Value|} { get; set; } = Offset // three
                                                                                   (3);

                                    private static int Offset(int value)
                                    {
                                        return value;
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that a comment in front of an element access, in front of a null-forgiving operator, or between the <c>?</c> and the
    /// <c>[</c> of a conditional element access in the initializer's value withholds the fix
    /// </summary>
    /// <param name="value">The initializer's value</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-007")]
    [DataRow("Values /* c */ [0]", DisplayName = "In front of an element access")]
    [DataRow("(Text /* c */ !).Length", DisplayName = "In front of a null-forgiving operator")]
    [DataRow("Values? /* c */ [0] ?? 0", DisplayName = "Between the question mark and the bracket")]
    public async Task VerifyNoFixIsOfferedForACommentInFrontOfAnAttachedPartOfTheInitializerValue(string value)
    {
        var testData = $"#nullable enable\n\nclass C\n{{\n    private int {{|RH2001:Value|}} {{ get; set; }} = {value};\n\n    private static int[] Values = [];\n\n    private static string? Text = \"\";\n}}";

        await VerifyNoCodeFixIsOffered(testData);
    }

    #endregion // Tests
}