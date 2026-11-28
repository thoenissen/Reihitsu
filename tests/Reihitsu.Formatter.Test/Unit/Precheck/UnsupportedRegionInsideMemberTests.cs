using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Unit.Precheck;

/// <summary>
/// Tests for the unsupported shape of a region directive inside a member (FMT-UNSUP-023): every position inside a member and in
/// the header of a type or an extension block next to the positions between members on its other side, nested types and
/// extension blocks, files with top-level statements, active and disabled directives, the shapes that keep their own reason,
/// node-level inputs and the context entry point, and the closure of the formatter's output, each under LF and CRLF line endings
/// </summary>
[TestClass]
public class UnsupportedRegionInsideMemberTests : FormatterTestsBase
{
    #region Constants

    /// <summary>
    /// The first example of FMT-UNSUP-023: region directives in a method body, a field initializer, a parameter list, and the
    /// arguments of a member's attribute
    /// </summary>
    private const string RefusedSpecExample = """
                                              class A : System.Attribute
                                              {
                                                  public A(int a, int b)
                                                  {
                                                  }
                                              }

                                              class C
                                              {
                                                  void M(int x)
                                                  {
                                                      #region Setup
                                                      var a = x;
                                                      #endregion
                                                  }

                                                  int[] F = [
                                                                #region R
                                                                1,
                                                                #endregion
                                                                2
                                                            ];

                                                  void N(int a,
                                                         #region P
                                                         int b)
                                                         #endregion
                                                  {
                                                  }

                                                  [A(1,
                                                     #region At
                                                     2)]
                                                     #endregion
                                                  void O()
                                                  {
                                                  }
                                              }
                                              """;

    /// <summary>
    /// The top-level example of FMT-UNSUP-023: a file with top-level statements, regions between the statements, and regions
    /// between the members of a class it declares
    /// </summary>
    private const string TopLevelSpecExample = """
                                               using System;

                                               #region Setup
                                               var a = 1;
                                               #endregion

                                               Console.WriteLine(a);

                                               class D
                                               {
                                                   #region Fields
                                                   private int _b;
                                                   #endregion
                                               }
                                               """;

    #endregion // Constants

    #region Methods

    /// <summary>
    /// Verifies the specification example: region directives in a method body, a field initializer, a parameter list, and the
    /// arguments of a member's attribute are refused, the input is returned unchanged, and the first one, which starts in line 12,
    /// column 9, is reported
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-023")]
    public void SpecExampleRegionDirectivesInsideMembersAreUnsupported()
    {
        AssertUnsupportedFor(RegionInsideMemberReason, RefusedSpecExample, "#region Setup");
        AssertStartsAt(RefusedSpecExample, 11, 8);
    }

    /// <summary>
    /// Verifies the specification example of the other side: region directives around the usings, between the members of a type
    /// and of a nested type, in front of a type's closing brace, and in disabled text inside a method are not refused, and the
    /// input is a fixed point of the formatter
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-023")]
    public void SpecExampleRegionDirectivesBetweenMembersAreSupported()
    {
        const string input = """
                             #region Usings

                             using System;

                             #endregion // Usings

                             class C
                             {
                                 #region Types

                                 class D
                                 {
                                     #region Fields

                                     private int _a;

                                     #endregion // Fields
                                 }

                                 #endregion // Types

                                 #region Methods

                                 void M()
                                 {
                             #if NEVER
                                     #region Disabled
                                     #endregion // Disabled
                             #endif
                                 }

                                 #endregion // Methods
                             }
                             """;

        AssertSupported(input);
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies the specification example: the regions between the members of an extension block are supported, and the input is a
    /// fixed point of the formatter
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-023")]
    public void SpecExampleRegionDirectivesBetweenTheMembersOfAnExtensionBlockAreSupported()
    {
        const string input = """
                             static class Ext
                             {
                                 extension(int x)
                                 {
                                     #region Members

                                     public int Twice()
                                     {
                                         return x * 2;
                                     }

                                     #endregion // Members
                                 }
                             }
                             """;

        AssertSupported(input);
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies the visible output change of an extension block: the regions between its members, written as the author wrote them,
    /// are formatted as before
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-023")]
    public void RegionDirectivesBetweenTheMembersOfAnExtensionBlockAreFormatted()
    {
        const string input = """
                             static class Ext
                             {
                                 extension(int x)
                                 {
                                     #region Members
                                     public int Twice() => x * 2;
                                     #endregion
                                 }
                             }
                             """;
        const string expected = """
                                static class Ext
                                {
                                    extension(int x)
                                    {
                                        #region Members

                                        public int Twice()
                                        {
                                            return x * 2;
                                        }

                                        #endregion // Members
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the specification example: in a file with top-level statements every region directive is refused, also the ones
    /// between the statements and between the members of the class, and the first one, which starts in line 3, column 1, is reported
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-023")]
    public void SpecExampleEveryRegionDirectiveOfAFileWithTopLevelStatementsIsUnsupported()
    {
        AssertUnsupportedFor(RegionInsideMemberReason, TopLevelSpecExample, "#region Setup");
        AssertStartsAt(TopLevelSpecExample, 2, 0);
    }

    /// <summary>
    /// Verifies that a file with top-level statements is refused for the regions between the members of a class it declares, when
    /// no other region directive stands in it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-023")]
    public void RegionBetweenTheMembersOfATypeInAFileWithTopLevelStatementsIsUnsupported()
    {
        const string input = """
                             using System;

                             var a = 1;

                             Console.WriteLine(a);

                             class D
                             {
                                 #region Fields
                                 private int _b;
                                 #endregion
                             }
                             """;

        AssertUnsupportedFor(RegionInsideMemberReason, input, "#region Fields");
    }

    /// <summary>
    /// Verifies that every position of a region directive in a file with top-level statements refuses it: in front of the first
    /// statement, between two statements, inside a statement, around a using directive, and at the end of the file
    /// </summary>
    /// <param name="input">The source text</param>
    /// <param name="reported">The text that is reported</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-023")]
    [DataRow("#region Setup\nvar a = 1;\n#endregion\n", "#region Setup", DisplayName = "Around the first statement")]
    [DataRow("var a = 1;\n\n#region R\nSystem.Console.WriteLine(a);\n#endregion\n", "#region R", DisplayName = "Between two statements")]
    [DataRow("var a = 1;\n\nif (a > 0)\n{\n    #region Body\n    a++;\n    #endregion\n}\n", "#region Body", DisplayName = "Inside a statement")]
    [DataRow("#region Usings\nusing System;\n#endregion\n\nConsole.WriteLine(1);\n", "#region Usings", DisplayName = "Around a using directive")]
    [DataRow("System.Console.WriteLine(1);\n\n#region R\n#endregion\n", "#region R", DisplayName = "At the end of the file")]
    public void EveryRegionDirectiveOfAFileWithTopLevelStatementsIsUnsupported(string input, string reported)
    {
        AssertUnsupportedFor(RegionInsideMemberReason, input, reported);
    }

    /// <summary>
    /// Verifies the other side of the top-level rule: the same class and usings in a file without a top-level statement are
    /// decided by the positions between members and are not refused
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-023")]
    public void RegionsOfAFileWithoutATopLevelStatementAreSupported()
    {
        const string input = """
                             #region Usings

                             using System;

                             #endregion // Usings

                             class D
                             {
                                 #region Fields

                                 private int _b;

                                 #endregion // Fields
                             }
                             """;

        AssertSupported(input);
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies the visible output change of a region in a method body: the input is returned unchanged and the directive, which
    /// starts in line 5, column 9, is reported
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-023")]
    public void RegionInAMethodBodyIsUnsupported()
    {
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     #region Setup
                                     var a = x;
                                     #endregion
                                 }
                             }
                             """;

        AssertUnsupportedFor(RegionInsideMemberReason, input, "#region Setup");
        AssertStartsAt(input, 4, 8);
    }

    /// <summary>
    /// Verifies the visible output change of a region in a field's initializer: the input is returned unchanged and the directive,
    /// which starts in line 4, column 19, is reported
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-023")]
    public void RegionInAFieldInitializerIsUnsupported()
    {
        const string input = """
                             class C
                             {
                                 int[] F = [
                                               #region R
                                               1,
                                               #endregion
                                               2
                                           ];
                             }
                             """;

        AssertUnsupportedFor(RegionInsideMemberReason, input, "#region R");
        AssertStartsAt(input, 3, 18);
    }

    /// <summary>
    /// Verifies the visible output change of a region in a parameter list: the input is returned unchanged and the directive, which
    /// starts in line 4, column 12, is reported
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-023")]
    public void RegionInAParameterListIsUnsupported()
    {
        const string input = """
                             class C
                             {
                                 void M(int a,
                                        #region R
                                        int b)
                                        #endregion
                                 {
                                 }
                             }
                             """;

        AssertUnsupportedFor(RegionInsideMemberReason, input, "#region R");
        AssertStartsAt(input, 3, 11);
    }

    /// <summary>
    /// Verifies the visible output change of a region in a member's attribute arguments: the input is returned unchanged and the
    /// directive, which starts in line 11, column 8, is reported
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-023")]
    public void RegionInTheAttributeArgumentsOfAMemberIsUnsupported()
    {
        const string input = """
                             class A : System.Attribute
                             {
                                 public A(int a, int b)
                                 {
                                 }
                             }

                             class C
                             {
                                 [A(1,
                                    #region At
                                    2)]
                                    #endregion
                                 void N()
                                 {
                                 }
                             }
                             """;

        AssertUnsupportedFor(RegionInsideMemberReason, input, "#region At");
        AssertStartsAt(input, 10, 7);
    }

    /// <summary>
    /// Verifies the visible output change of a region in the header of a type: a region in a base list and in a primary-constructor
    /// parameter list is refused, the input is returned unchanged, and the first directive, which starts in line 10, column 11, is
    /// reported
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-023")]
    public void RegionInTheHeaderOfATypeIsUnsupported()
    {
        const string input = """
                             interface I
                             {
                             }

                             class B
                             {
                             }

                             class C : B,
                                       #region Interfaces
                                       I
                                       #endregion
                             {
                             }

                             record R(int A,
                                      #region More
                                      int B);
                                      #endregion
                             """;

        AssertUnsupportedFor(RegionInsideMemberReason, input, "#region Interfaces");
        AssertStartsAt(input, 9, 10);
    }

    /// <summary>
    /// Verifies that a region directive refuses the input at every position inside a member: in a body, a nested block, a local
    /// function, a lambda, a switch section and in front of a switch label, between an if block and its else, in a method chain,
    /// in an initializer, an object initializer, a collection expression, a switch expression, a pattern, an argument list, an
    /// accessor list, an accessor body, an event's accessor list, a parameter list, an attribute's arguments, between two attribute
    /// lists, between a parameter list and the body, between an expression body and its semicolon, in an enum member, and in a
    /// constructor, an indexer, an operator, and a delegate
    /// </summary>
    /// <param name="members">The members of the class <c>C</c></param>
    /// <param name="reported">The text that is reported</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-023")]
    [DataRow("    void M()\n    {\n        #region R\n        M();\n        #endregion\n    }", "#region R", DisplayName = "Method body")]
    [DataRow("    void M()\n    {\n        {\n            #region R\n            #endregion\n        }\n    }", "#region R", DisplayName = "Empty statement block")]
    [DataRow("    void M()\n    {\n        void L()\n        {\n            #region R\n            #endregion\n        }\n    }", "#region R", DisplayName = "Local function body")]
    [DataRow("    void M()\n    {\n        #region R\n        void L()\n        {\n        }\n        #endregion\n    }", "#region R", DisplayName = "Around a local function")]
    [DataRow("    System.Action F = () =>\n    {\n        #region R\n        #endregion\n    };", "#region R", DisplayName = "Lambda body in a field initializer")]
    [DataRow("    void M(int a)\n    {\n        switch (a)\n        {\n            case 1:\n                #region R\n                break;\n                #endregion\n        }\n    }", "#region R", DisplayName = "Switch section")]
    [DataRow("    void M(int a)\n    {\n        switch (a)\n        {\n            #region R\n            case 1:\n                break;\n            #endregion\n        }\n    }", "#region R", DisplayName = "In front of a switch label")]
    [DataRow("    void M(bool a)\n    {\n        if (a)\n        {\n        }\n        #region R\n        else\n        {\n        }\n        #endregion\n    }", "#region R", DisplayName = "Between an if block and its else")]
    [DataRow("    void M(string a)\n    {\n        var x = a\n            #region R\n            .Trim();\n            #endregion\n    }", "#region R", DisplayName = "Method chain")]
    [DataRow("    int[] F = [\n        #region R\n        1,\n        #endregion\n        2\n    ];", "#region R", DisplayName = "Collection expression of a field")]
    [DataRow("    int[] P { get; } = [\n        #region R\n        1,\n        #endregion\n        2\n    ];", "#region R", DisplayName = "Collection expression of a property")]
    [DataRow("    int[] F = {\n        #region R\n        1,\n        #endregion\n        2\n    };", "#region R", DisplayName = "Array initializer of a field")]
    [DataRow("    object M()\n    {\n        return new System.Collections.Generic.List<int>\n        {\n            #region R\n            1,\n            #endregion\n            2\n        };\n    }", "#region R", DisplayName = "Collection initializer")]
    [DataRow("    object M()\n    {\n        return new { A = 1,\n            #region R\n            B = 2\n            #endregion\n        };\n    }", "#region R", DisplayName = "Anonymous object")]
    [DataRow("    int M(int a)\n    {\n        return a switch\n        {\n            #region R\n            1 => 2,\n            #endregion\n            _ => 3\n        };\n    }", "#region R", DisplayName = "Switch expression")]
    [DataRow("    bool M(object o)\n    {\n        return o is [\n            #region R\n            1,\n            #endregion\n            2\n        ];\n    }", "#region R", DisplayName = "List pattern")]
    [DataRow("    bool M(object o)\n    {\n        return o is (\n            #region R\n            1 or 2\n            #endregion\n        );\n    }", "#region R", DisplayName = "Parenthesized pattern")]
    [DataRow("    void M(int a, int b)\n    {\n        M(\n            #region R\n            1,\n            #endregion\n            2);\n    }", "#region R", DisplayName = "Argument list")]
    [DataRow("    int P\n    {\n        #region R\n        get;\n        #endregion\n        set;\n    }", "#region R", DisplayName = "Property accessor list")]
    [DataRow("    int P\n    {\n        get\n        {\n            #region R\n            return 1;\n            #endregion\n        }\n    }", "#region R", DisplayName = "Accessor body")]
    [DataRow("    int this[int i]\n    {\n        #region R\n        get => i;\n        #endregion\n    }", "#region R", DisplayName = "Indexer accessor list")]
    [DataRow("    event System.Action E\n    {\n        #region R\n        add\n        {\n        }\n        #endregion\n        remove\n        {\n        }\n    }", "#region R", DisplayName = "Event accessor list")]
    [DataRow("    void M(int a,\n           #region R\n           int b)\n           #endregion\n    {\n    }", "#region R", DisplayName = "Parameter list")]
    [DataRow("    [System.Obsolete(\n        #region R\n        \"a\")]\n        #endregion\n    void M()\n    {\n    }", "#region R", DisplayName = "Attribute arguments")]
    [DataRow("    [System.Obsolete]\n    #region R\n    [System.CLSCompliant(false)]\n    #endregion\n    void M()\n    {\n    }", "#region R", DisplayName = "Between two attribute lists")]
    [DataRow("    void M()\n    #region R\n    {\n    }\n    #endregion", "#region R", DisplayName = "Between the parameter list and the body")]
    [DataRow("    int M() => 1\n    #region R\n    #endregion\n    ;", "#region R", DisplayName = "Between an expression body and its semicolon")]
    [DataRow("    C()\n    {\n        #region R\n        #endregion\n    }", "#region R", DisplayName = "Constructor body")]
    [DataRow("    public static C operator +(C a, C b)\n    {\n        #region R\n        return a;\n        #endregion\n    }", "#region R", DisplayName = "Operator body")]
    [DataRow("    delegate void D(int a,\n                    #region R\n                    int b);\n    #endregion", "#region R", DisplayName = "Delegate parameter list")]
    public void RegionInsideAMemberIsUnsupported(string members, string reported)
    {
        AssertUnsupportedFor(RegionInsideMemberReason, WrapInClass(members), reported);
    }

    /// <summary>
    /// Verifies that a region directive between an enum member's name and its value refuses the input, while the one in front of
    /// the enum's closing brace does not decide it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-023")]
    public void RegionInsideAnEnumMemberIsUnsupported()
    {
        AssertUnsupportedFor(RegionInsideMemberReason, "enum E\n{\n    A\n    #region R\n    = 1\n    #endregion\n}", "#region R");
    }

    /// <summary>
    /// Verifies that a region opened between members and closed inside a member refuses the input at its <c>#endregion</c>
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-023")]
    public void RegionClosedInsideAMemberIsUnsupportedAtItsEndRegion()
    {
        const string input = """
                             class C
                             {
                                 #region Members

                                 void M()
                                 {
                                     M();

                                     #endregion
                                 }
                             }
                             """;

        AssertUnsupportedFor(RegionInsideMemberReason, input, "#endregion");
    }

    /// <summary>
    /// Verifies the specification's other side of a member's end: a region whose <c>#endregion</c> stands behind a member's last
    /// token, in front of the type's closing brace or of the next member, is supported
    /// </summary>
    /// <param name="input">The source text</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-023")]
    [DataRow("class C\n{\n    #region Methods\n    void M()\n    {\n    }\n    #endregion\n}", DisplayName = "In front of the type's closing brace")]
    [DataRow("class C\n{\n    #region Methods\n    void M()\n    {\n    }\n    #endregion\n    void N()\n    {\n    }\n}", DisplayName = "In front of the next member")]
    public void RegionThatEndsBehindAMembersLastTokenIsSupported(string input)
    {
        AssertSupported(input);
    }

    /// <summary>
    /// Verifies that a region directive in the header of a type, a namespace, or an extension block refuses the input: in a base
    /// list, a type parameter list, a primary-constructor parameter list, a constraint clause, between the type's attribute lists,
    /// in front of a type's, a namespace's, or an extension block's opening brace, and in an extension block's parameter list
    /// </summary>
    /// <param name="input">The source text</param>
    /// <param name="reported">The text that is reported</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-023")]
    [DataRow("interface I\n{\n}\n\nclass C : object,\n    #region R\n    I\n    #endregion\n{\n}", "#region R", DisplayName = "Base list")]
    [DataRow("class C<\n    #region R\n    T\n    #endregion\n    >\n{\n}", "#region R", DisplayName = "Type parameter list")]
    [DataRow("record R(int A,\n    #region R\n    int B);\n    #endregion", "#region R", DisplayName = "Primary-constructor parameter list")]
    [DataRow("class C<T>\n    #region R\n    where T : class\n    #endregion\n{\n}", "#region R", DisplayName = "Constraint clause")]
    [DataRow("[System.Obsolete]\n#region R\n[System.Serializable]\n#endregion\nclass C\n{\n}", "#region R", DisplayName = "Between a type's attribute lists")]
    [DataRow("class C\n#region R\n{\n    #endregion\n}", "#region R", DisplayName = "In front of a type's opening brace")]
    [DataRow("namespace N\n#region R\n{\n    #endregion\n    class C\n    {\n    }\n}", "#region R", DisplayName = "In front of a namespace's opening brace")]
    [DataRow("static class Ext\n{\n    extension(\n        #region R\n        int x)\n        #endregion\n    {\n    }\n}", "#region R", DisplayName = "Extension block's parameter list")]
    [DataRow("static class Ext\n{\n    extension(int x)\n    #region R\n    {\n        #endregion\n    }\n}", "#region R", DisplayName = "In front of an extension block's opening brace")]
    public void RegionInTheHeaderOfATypeANamespaceOrAnExtensionBlockIsUnsupported(string input, string reported)
    {
        AssertUnsupportedFor(RegionInsideMemberReason, input, reported);
    }

    /// <summary>
    /// Verifies that every position between members is supported: around an assembly and a module attribute list, an extern alias,
    /// a using directive inside a block-scoped namespace, the types of a file-scoped namespace up to the end of the file, the
    /// members of an enum up to its closing brace, the types of a namespace up to its closing brace, a delegate, an event field, an
    /// attributed member, the members of an interface and of a struct, and a nested type at depth two and its members
    /// </summary>
    /// <param name="input">The source text</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-023")]
    [DataRow("#region Attributes\n[assembly: System.CLSCompliant(false)]\n#endregion\n\nclass C\n{\n}", DisplayName = "Assembly attribute list")]
    [DataRow("#region Attributes\n[module: System.CLSCompliant(false)]\n#endregion\n\nclass C\n{\n}", DisplayName = "Module attribute list")]
    [DataRow("#region Aliases\nextern alias A;\n#endregion\n\nclass C\n{\n}", DisplayName = "Extern alias")]
    [DataRow("namespace N\n{\n    #region Usings\n    using System;\n    #endregion\n\n    class C\n    {\n    }\n}", DisplayName = "Using directive inside a namespace")]
    [DataRow("namespace N;\n\n#region Types\nclass C\n{\n}\n#endregion", DisplayName = "File-scoped namespace up to the end of the file")]
    [DataRow("enum E\n{\n    #region Values\n    A,\n    B\n    #endregion\n}", DisplayName = "Members of an enum up to its closing brace")]
    [DataRow("namespace N\n{\n    #region Types\n    class C\n    {\n    }\n    #endregion\n}", DisplayName = "Types of a namespace up to its closing brace")]
    [DataRow("class C\n{\n    #region Delegates\n    delegate void D(int a);\n    #endregion\n}", DisplayName = "Delegate")]
    [DataRow("class C\n{\n    #region Events\n    event System.Action E;\n    #endregion\n}", DisplayName = "Event field")]
    [DataRow("class C\n{\n    #region Methods\n    [System.Obsolete]\n    void M()\n    {\n    }\n    #endregion\n}", DisplayName = "Attributed member")]
    [DataRow("interface I\n{\n    #region Methods\n    void M();\n    #endregion\n}", DisplayName = "Members of an interface")]
    [DataRow("struct S\n{\n    #region Fields\n    int _a;\n    #endregion\n}", DisplayName = "Members of a struct")]
    [DataRow("class A\n{\n    class B\n    {\n        #region Types\n        class D\n        {\n            #region Fields\n            int _a;\n            #endregion\n        }\n        #endregion\n    }\n}", DisplayName = "Nested type at depth two and its members")]
    [DataRow("#region Types\nclass C\n{\n}\n#endregion", DisplayName = "Type up to the end of the file")]
    public void RegionBetweenMembersIsSupported(string input)
    {
        AssertSupported(input);
    }

    /// <summary>
    /// Verifies that a region directive inside a method of a nested type at depth two refuses the input, while the ones between its
    /// members do not
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-023")]
    public void RegionInsideAMethodOfANestedTypeIsUnsupported()
    {
        const string input = """
                             class A
                             {
                                 class B
                                 {
                                     #region Methods

                                     void M()
                                     {
                                         #region Body
                                         M();
                                         #endregion
                                     }

                                     #endregion
                                 }
                             }
                             """;

        AssertUnsupportedFor(RegionInsideMemberReason, input, "#region Body");
    }

    /// <summary>
    /// Verifies that a region directive inside a member of an extension block refuses the input, while the ones between the
    /// extension block's members and in front of its closing brace do not
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-023")]
    public void RegionInsideAMemberOfAnExtensionBlockIsUnsupported()
    {
        const string input = """
                             static class Ext
                             {
                                 extension(int x)
                                 {
                                     #region Members

                                     public int Twice()
                                     {
                                         #region Body
                                         return x * 2;
                                         #endregion
                                     }

                                     #endregion
                                 }
                             }
                             """;

        AssertUnsupportedFor(RegionInsideMemberReason, input, "#region Body");
    }

    /// <summary>
    /// Verifies that only active region directives are checked: where <c>X</c> is not defined, a region inside a method in the
    /// branch of <c>#if X</c> is disabled text and the input is formatted, and where it is defined, the input is refused
    /// </summary>
    /// <param name="defineX">Whether <c>X</c> is defined</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-023")]
    [DataRow(false, DisplayName = "X not defined")]
    [DataRow(true, DisplayName = "X defined")]
    public void RegionInDisabledTextIsNotChecked(bool defineX)
    {
        var parseOptions = defineX ? CSharpParseOptions.Default.WithPreprocessorSymbols("X") : CSharpParseOptions.Default;
        var input = WrapInClass("    void M()\n    {\n#if X\n        #region R\n        M();\n        #endregion\n#endif\n    }");

        if (defineX)
        {
            AssertUnsupportedFor(RegionInsideMemberReason, input, "#region R", parseOptions: parseOptions);
        }
        else
        {
            AssertSupported(input, parseOptions);
        }
    }

    /// <summary>
    /// Verifies that other directives inside a member are no region directives and do not refuse the input under this requirement
    /// </summary>
    /// <param name="members">The members of the class <c>C</c></param>
    [TestMethod]
    [Requirement("FMT-UNSUP-023")]
    [DataRow("    void M()\n    {\n#if DEBUG\n        M();\n#endif\n    }", DisplayName = "Conditional group")]
    [DataRow("    void M()\n    {\n#pragma warning disable CS0168\n        int a;\n#pragma warning restore CS0168\n    }", DisplayName = "Pragma")]
    [DataRow("    void M()\n    {\n#nullable enable\n        M();\n#nullable restore\n    }", DisplayName = "Nullable directive")]
    public void OtherDirectiveInsideAMemberIsSupported(string members)
    {
        AssertSupported(WrapInClass(members));
    }

    /// <summary>
    /// Verifies the order of the shapes: a region directive inside a field declaration with several declarators keeps the reason
    /// of FMT-UNSUP-010, one between a list element and a comma on a later line keeps FMT-UNSUP-009's, one between the arrow of an
    /// expression body and its expression keeps FMT-UNSUP-012's, one in front of the arrow keeps FMT-UNSUP-013's, and one behind a
    /// modifier keeps FMT-UNSUP-017's
    /// </summary>
    /// <param name="reason">The reason that is given</param>
    /// <param name="members">The members of the class <c>C</c></param>
    /// <param name="reported">The text that is reported</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-023")]
    [DataRow(FieldDeclarationDirectiveReason, "    private int _a = 1,\n        #region R\n        _b;\n    #endregion", "#region R", DisplayName = "Field declaration with several declarators")]
    [DataRow(ListSeparatorCommentReason, "    int[] F = [\n        1\n        #region R\n        , 2\n        #endregion\n    ];", "#region R", DisplayName = "Between a list element and a comma on a later line")]
    [DataRow(ExpressionBodyArrowCommentReason, "    int M() =>\n        #region R\n        1;\n    #endregion", "#region R", DisplayName = "Between the arrow of an expression body and its expression")]
    [DataRow(ExpressionBodyArrowLeadingCommentReason, "    int M()\n        #region R\n        => 1;\n    #endregion", "#region R", DisplayName = "In front of the arrow of an expression body")]
    [DataRow(ModifierCommentReason, "    public\n    #region R\n    static\n    #endregion\n    void M()\n    {\n    }", "#region R", DisplayName = "Between two modifiers")]
    public void OtherShapesKeepTheirReason(string reason, string members, string reported)
    {
        AssertUnsupportedFor(reason, WrapInClass(members), reported);
    }

    /// <summary>
    /// Verifies the first token of a node-level input: a statement and its expression below a region directive hold the directive in
    /// the leading trivia of their first token and are not refused, while the method and the document that hold the statement are,
    /// and a statement that holds a region directive behind its first token is refused as well
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-003")]
    [Requirement("FMT-UNSUP-023")]
    public void NodeLevelInputIsNotRefusedForTheRegionInFrontOfItsFirstToken()
    {
        foreach (var endOfLine in _lineEndings)
        {
            var text = NormalizeLineEndings(WrapInClass("    void M(int a)\n    {\n        #region R\n        a++;\n        #endregion\n        if (a > 0)\n        {\n            #region S\n            a--;\n            #endregion\n        }\n    }"), endOfLine);
            var root = CSharpSyntaxTree.ParseText(text, cancellationToken: TestContext.CancellationToken).GetRoot(TestContext.CancellationToken);
            var statement = root.DescendantNodes().OfType<ExpressionStatementSyntax>().First();
            var ifStatement = root.DescendantNodes().OfType<IfStatementSyntax>().Single();
            var method = root.DescendantNodes().OfType<MethodDeclarationSyntax>().Single();

            Assert.IsNull(ReihitsuFormatter.FindUnsupportedSyntax(statement), $"The statement holds the region directive in front of its first token under {DescribeLineEnding(endOfLine)} line endings.");
            Assert.IsNull(ReihitsuFormatter.FindUnsupportedSyntax(statement.Expression));
            Assert.IsNull(ReihitsuFormatter.FormatNode(statement, cancellationToken: TestContext.CancellationToken).Unsupported);
            Assert.AreEqual(RegionInsideMemberReason, ReihitsuFormatter.FindUnsupportedSyntax(ifStatement)?.Reason, "The if statement holds a region directive behind its first token.");
            Assert.AreEqual(RegionInsideMemberReason, ReihitsuFormatter.FindUnsupportedSyntax(method)?.Reason);
            Assert.AreEqual(RegionInsideMemberReason, ReihitsuFormatter.FindUnsupportedSyntax(root)?.Reason);
            AssertLocationIsShape(ReihitsuFormatter.FindUnsupportedSyntax(method).Location, text, "#region R", 0, DescribeLineEnding(endOfLine));
        }
    }

    /// <summary>
    /// Verifies that a method below a region directive between members gets the same answer through every entry point: the
    /// document, the method passed to <see cref="ReihitsuFormatter.FormatNode"/>, and the method as the target of
    /// <see cref="ReihitsuFormatter.FormatNodeInDocumentAsync"/> are not refused
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-UNSUP-002")]
    [Requirement("FMT-UNSUP-023")]
    public async Task MethodBelowARegionBetweenMembersIsNotRefusedByAnyEntryPoint()
    {
        const string input = """
                             class C
                             {
                                 #region Methods

                                 void M()
                                 {
                                 }

                                 #endregion // Methods
                             }
                             """;

        foreach (var endOfLine in _lineEndings)
        {
            using (var workspace = new AdhocWorkspace())
            {
                var document = CreateDocument(workspace, NormalizeLineEndings(input, endOfLine));
                var root = await document.GetSyntaxRootAsync(TestContext.CancellationToken);
                var method = root.DescendantNodes().OfType<MethodDeclarationSyntax>().Single();

                Assert.IsNull((await ReihitsuFormatter.FormatDocumentAsync(document, TestContext.CancellationToken)).Unsupported);
                Assert.IsNull(ReihitsuFormatter.FormatNode(method, cancellationToken: TestContext.CancellationToken).Unsupported);
                Assert.IsNull((await ReihitsuFormatter.FormatNodeInDocumentAsync(document, method, TestContext.CancellationToken)).Unsupported);
            }
        }
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatter.FormatNodeInDocumentAsync"/> with a method that holds a region in its body as
    /// target returns the document unchanged and reports the directive in the document's syntax tree
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-UNSUP-002")]
    [Requirement("FMT-UNSUP-023")]
    public async Task FormatNodeInDocumentAsyncRefusesAMethodThatHoldsARegionInItsBody()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     #region    setup

                                     var x=1;

                                     #endregion
                                 }
                             }
                             """;

        foreach (var endOfLine in _lineEndings)
        {
            var text = NormalizeLineEndings(input, endOfLine);

            using (var workspace = new AdhocWorkspace())
            {
                var document = CreateDocument(workspace, text);
                var root = await document.GetSyntaxRootAsync(TestContext.CancellationToken);

                var result = await ReihitsuFormatter.FormatNodeInDocumentAsync(document, root.DescendantNodes().OfType<MethodDeclarationSyntax>().Single(), TestContext.CancellationToken);

                Assert.AreEqual(text, (await result.Output.GetTextAsync(TestContext.CancellationToken)).ToString());
                Assert.AreEqual(RegionInsideMemberReason, result.Unsupported?.Reason);
                AssertLocationIsShape(result.Unsupported.Location, text, "#region    setup", 0, DescribeLineEnding(endOfLine));
            }
        }
    }

    /// <summary>
    /// Verifies that a type declared in a file with top-level statements is decided by its own members: the file is refused, while
    /// the class as a node-level input is not, through <see cref="ReihitsuFormatter.FindUnsupportedSyntax"/>,
    /// <see cref="ReihitsuFormatter.FormatNode"/>, and <see cref="ReihitsuFormatter.FormatNodeInDocumentAsync"/>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-UNSUP-003")]
    [Requirement("FMT-UNSUP-023")]
    public async Task TypeOfAFileWithTopLevelStatementsIsDecidedByItsOwnMembers()
    {
        foreach (var endOfLine in _lineEndings)
        {
            var text = NormalizeLineEndings(TopLevelSpecExample, endOfLine);

            using (var workspace = new AdhocWorkspace())
            {
                var document = CreateDocument(workspace, text);
                var root = await document.GetSyntaxRootAsync(TestContext.CancellationToken);
                var type = root.DescendantNodes().OfType<ClassDeclarationSyntax>().Single();

                Assert.AreEqual(RegionInsideMemberReason, (await ReihitsuFormatter.FormatDocumentAsync(document, TestContext.CancellationToken)).Unsupported?.Reason);
                Assert.IsNull(ReihitsuFormatter.FindUnsupportedSyntax(type));
                Assert.IsNull(ReihitsuFormatter.FormatNode(type, cancellationToken: TestContext.CancellationToken).Unsupported);

                var result = await ReihitsuFormatter.FormatNodeInDocumentAsync(document, type, TestContext.CancellationToken);

                Assert.IsNull(result.Unsupported, $"The class holds its regions between its own members under {DescribeLineEnding(endOfLine)} line endings.");
            }
        }
    }

    /// <summary>
    /// Verifies the context entry point with a method as context and one of its parameters as target: the input is the context, so
    /// the region directive in front of the parameter refuses it, the document is returned unchanged, and the directive is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-UNSUP-002")]
    [Requirement("FMT-UNSUP-023")]
    public async Task ContextEntryPointRefusesAMethodContextForARegionInFrontOfTheTargetParameter()
    {
        const string input = """
                             class C
                             {
                                 void M(int a,
                                        #region R
                                        int b)
                                        #endregion
                                 {
                                 }
                             }
                             """;

        foreach (var endOfLine in _lineEndings)
        {
            var text = NormalizeLineEndings(input, endOfLine);

            using (var workspace = new AdhocWorkspace())
            {
                var document = CreateDocument(workspace, text);
                var root = await document.GetSyntaxRootAsync(TestContext.CancellationToken);
                var method = root.DescendantNodes().OfType<MethodDeclarationSyntax>().Single();
                var parameter = method.ParameterList.Parameters[1];

                var result = await ReihitsuFormatter.FormatNodeInDocumentWithContextAsync(document, parameter, method, TestContext.CancellationToken);

                Assert.AreEqual(text, (await result.Output.GetTextAsync(TestContext.CancellationToken)).ToString());
                Assert.AreEqual(RegionInsideMemberReason, result.Unsupported?.Reason);
                AssertLocationIsShape(result.Unsupported.Location, text, "#region R", 0, DescribeLineEnding(endOfLine));
            }
        }
    }

    /// <summary>
    /// Verifies the context entry point with an enum as context and one of its members as target: the region directive in front of
    /// the member stands in front of a member's first token and does not refuse the input
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-UNSUP-002")]
    [Requirement("FMT-UNSUP-023")]
    public async Task ContextEntryPointDoesNotRefuseAnEnumContextForARegionInFrontOfTheTargetMember()
    {
        const string input = """
                             enum E
                             {
                                 A,

                                 #region R

                                 B = 1

                                 #endregion // R
                             }
                             """;

        foreach (var endOfLine in _lineEndings)
        {
            using (var workspace = new AdhocWorkspace())
            {
                var document = CreateDocument(workspace, NormalizeLineEndings(input, endOfLine));
                var root = await document.GetSyntaxRootAsync(TestContext.CancellationToken);
                var enumDeclaration = root.DescendantNodes().OfType<EnumDeclarationSyntax>().Single();

                var result = await ReihitsuFormatter.FormatNodeInDocumentWithContextAsync(document, enumDeclaration.Members[1], enumDeclaration, TestContext.CancellationToken);

                Assert.IsNull(result.Unsupported, $"The region stands in front of an enum member's first token under {DescribeLineEnding(endOfLine)} line endings.");
            }
        }
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatter.FindUnsupportedSyntax"/> answers for a node a code fix builds: a method built with
    /// a region in its body is refused, and a class built with a region between its members is not
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-023")]
    public void FindUnsupportedSyntaxAnswersForABuiltNode()
    {
        var method = SyntaxFactory.ParseMemberDeclaration("void M()\n{\n    #region R\n    M();\n    #endregion\n}");
        var type = SyntaxFactory.ParseMemberDeclaration("class C\n{\n    #region R\n    int _a;\n    #endregion\n}");

        var unsupported = ReihitsuFormatter.FindUnsupportedSyntax(method);

        Assert.IsNotNull(unsupported);
        Assert.AreEqual(RegionInsideMemberReason, unsupported.Reason);
        AssertLocationIsShape(unsupported.Location, method.SyntaxTree.GetText(TestContext.CancellationToken).ToString(), "#region R", 0, "LF");
        Assert.IsNull(ReihitsuFormatter.FindUnsupportedSyntax(type));
    }

    /// <summary>
    /// Verifies that the formatter never writes a region directive into a member: the removal of an empty type's braces, of an
    /// enum's trailing comma, the split of an attribute list and of a field declaration, the ordering of using directives, the block
    /// body of an expression-bodied method, and the expression body of a get-only property leave every region directive between
    /// members, and the output, formatted again, is neither refused nor changed
    /// </summary>
    /// <param name="input">The source text</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-005")]
    [Requirement("FMT-UNSUP-023")]
    [DataRow("class Empty\n{\n    #region R\n    #endregion\n}", DisplayName = "Type that holds only a region")]
    [DataRow("enum E\n{\n    A,\n    #region B\n    B,\n    #endregion\n}", DisplayName = "Enum trailing comma in front of an endregion")]
    [DataRow("class C\n{\n    #region A\n    [System.Obsolete, System.CLSCompliant(false)] void M() { }\n    #endregion\n}", DisplayName = "Split of an attribute list")]
    [DataRow("class C\n{\n    #region F\n    private int _a, _b;\n    #endregion\n}", DisplayName = "Split of a field declaration")]
    [DataRow("#region Text\nusing System.Text;\n#endregion\nusing System;\n\nclass C\n{\n}", DisplayName = "Ordering of using directives")]
    [DataRow("class C\n{\n    #region M\n    int M() => 1;\n    #endregion\n}", DisplayName = "Block body of an expression-bodied method")]
    [DataRow("class C\n{\n    #region P\n    int P { get { return 1; } }\n    #endregion\n}", DisplayName = "Expression body of a get-only property")]
    [DataRow("class C\n{\n    #region M\n    /// <summary>M</summary>\n    void M()\n    {\n    }\n    #endregion\n}", DisplayName = "Documentation comment of a member below a region")]
    public void OutputOfTheFormatterIsNotUnsupported(string input)
    {
        foreach (var endOfLine in _lineEndings)
        {
            var endingName = DescribeLineEnding(endOfLine);
            var first = ReihitsuFormatter.FormatSyntaxTree(CSharpSyntaxTree.ParseText(NormalizeLineEndings(input, endOfLine), cancellationToken: TestContext.CancellationToken), TestContext.CancellationToken);

            Assert.IsNull(first.Unsupported, $"The input must be formatted under {endingName} line endings: {first.Unsupported?.Reason}");

            var output = first.Output.ToString();
            var second = ReihitsuFormatter.FormatSyntaxTree(CSharpSyntaxTree.ParseText(output, cancellationToken: TestContext.CancellationToken), TestContext.CancellationToken);

            Assert.IsNull(second.Unsupported, $"The formatted output must not be refused under {endingName} line endings:{endOfLine}{output}");
            Assert.AreEqual(output, second.Output.ToString(), $"The formatted output must be stable under {endingName} line endings.");
        }
    }

    /// <summary>
    /// Wraps members in a class <c>C</c>
    /// </summary>
    /// <param name="members">The members, indented by four spaces</param>
    /// <returns>The source text</returns>
    private static string WrapInClass(string members)
    {
        return $"class C\n{{\n{members}\n}}";
    }

    /// <summary>
    /// Creates a document with the given text in a new project of the workspace
    /// </summary>
    /// <param name="workspace">The workspace</param>
    /// <param name="text">The document's text</param>
    /// <returns>The document</returns>
    private static Document CreateDocument(AdhocWorkspace workspace, string text)
    {
        return workspace.AddProject("TestProject", LanguageNames.CSharp).AddDocument("Test.cs", SourceText.From(text));
    }

    /// <summary>
    /// Asserts that the formatter reports the shape of the input in the given line and column, under LF and CRLF line endings
    /// </summary>
    /// <param name="input">The source text</param>
    /// <param name="line">The zero-based line</param>
    /// <param name="character">The zero-based column</param>
    private void AssertStartsAt(string input, int line, int character)
    {
        foreach (var endOfLine in _lineEndings)
        {
            var tree = CSharpSyntaxTree.ParseText(NormalizeLineEndings(input, endOfLine), cancellationToken: TestContext.CancellationToken);

            Assert.AreEqual(new LinePosition(line, character), ReihitsuFormatter.FormatSyntaxTree(tree, TestContext.CancellationToken).Unsupported.Location.GetLineSpan().StartLinePosition);
        }
    }

    #endregion // Methods
}