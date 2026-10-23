using System.Collections.Generic;
using System.IO;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Unit.Pipeline;

/// <summary>
/// Tests for the invariants every formatter phase keeps: user-authored content survives and the program's meaning is unchanged
/// </summary>
[TestClass]
public class PipelineInvariantTests : FormatterTestsBase
{
    #region Constants

    /// <summary>
    /// Unformatted source with comments, directives, disabled text, and literals of every kind, none of which a phase spec
    /// allows to be normalized
    /// </summary>
    private const string UserAuthoredContentSource = """"
                                                     using System;
                                                     class C{
                                                         // keep  me
                                                       /* first   line
                                                            second   line */
                                                         void M(int v){
                                                     #if NEVER
                                                        this   is   disabled   text
                                                           and   so   is   this
                                                     #else
                                                             var s="a  b"; /* inline  comment */
                                                     #endif
                                                     #pragma warning disable CS0168
                                                             var c='\t';
                                                             var t=@"x  y
                                                           z";
                                                             var i=$"{v,5}  {v:X}";
                                                             var r="""
                                                                   raw   text
                                                                   """;
                                                             var u="""  single  """u8;
                                                             var q=$"""
                                                                   {v}   interpolated   raw
                                                                   """;
                                                             var w="""
                                                                   utf8   raw
                                                                   """u8;
                                                             var n=0x1F+1_000+1.5e3;
                                                     #pragma warning restore CS0168
                                                         }
                                                     }
                                                     """";

    /// <summary>
    /// Unformatted source whose formatting runs the syntax-rewriting phases: expression bodies, braces, field splitting,
    /// using ordering, and switch-section braces
    /// </summary>
    private const string RewrittenSyntaxSource = """
                                                 using System.Text;
                                                 using System;
                                                 public class C
                                                 {
                                                     private int _a, _b;
                                                     public int A { get { return _a; } }
                                                     public int B { get { return _b; } set { _b = value; } }
                                                     public int Sum() => _a + _b;
                                                     public C(int a) => _a = a;
                                                     public string Describe(int kind)
                                                     {
                                                         if (kind > 2) return "big";
                                                         else if (kind > 1) return "medium";
                                                         var builder = new StringBuilder();
                                                         switch (kind)
                                                         {
                                                             case 0:
                                                                 builder.Append("zero");
                                                                 break;
                                                             case 1:
                                                             {
                                                                 builder.Append("one");
                                                                 break;
                                                             }
                                                             default:
                                                                 builder.Append(Math.Abs(kind));
                                                                 break;
                                                         }
                                                         foreach (var c in builder.ToString()) if (c == 'z') return "z";
                                                         return builder.ToString();
                                                     }
                                                 }
                                                 """;

    #endregion // Constants

    #region Properties

    /// <summary>
    /// Test context for the current test
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies the spec example: comments, disabled text, and literals keep their text while the code around them is
    /// reformatted
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-002")]
    public void CommentsDisabledTextAndLiteralsKeepTheirText()
    {
        const string input = """
                             class C{
                                 // keep  me
                                 void M(){
                             #if NEVER
                                this   is   disabled   text
                             #else
                                     var s="a  b"; /* inline  comment */
                             #endif
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    // keep  me
                                    void M()
                                    {
                                #if NEVER
                                   this   is   disabled   text
                                #else
                                        var s = "a  b"; /* inline  comment */
                                #endif
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the text of every comment, directive, disabled-text span, and literal of the input occurs unchanged and
    /// in the same order in the formatted output, under LF and CRLF; only the indentation of multi-line raw string content may
    /// move, so its value is compared instead
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-002")]
    public void EveryCommentDirectiveDisabledTextAndLiteralTextSurvives()
    {
        foreach (var endOfLine in _lineEndings)
        {
            var input = NormalizeLineEndings(UserAuthoredContentSource, endOfLine);
            var inputTree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);
            var outputTree = ReihitsuFormatter.FormatSyntaxTree(inputTree, TestContext.CancellationToken);

            Assert.AreNotEqual(input, outputTree.ToString(), "The input must be reformatted for the test to be meaningful.");
            Assert.AreSequenceEqual(CollectUserAuthoredContent(inputTree.GetRoot(TestContext.CancellationToken)),
                                    CollectUserAuthoredContent(outputTree.GetRoot(TestContext.CancellationToken)),
                                    $"User-authored content changed under {DescribeLineEnding(endOfLine)} line endings.");
        }
    }

    /// <summary>
    /// Verifies that formatting code that the syntax-rewriting phases change compiles to the same assembly as the input
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-003")]
    public void RewrittenCodeCompilesToTheSameAssembly()
    {
        foreach (var endOfLine in _lineEndings)
        {
            var input = NormalizeLineEndings(RewrittenSyntaxSource, endOfLine);
            var inputTree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);
            var outputTree = ReihitsuFormatter.FormatSyntaxTree(inputTree, TestContext.CancellationToken);
            var output = outputTree.ToString();

            Assert.Contains("public int A => _a;", output, "The get-only property must be rewritten for the test to be meaningful.");
            Assert.Contains("private int _b;", output, "The field must be split for the test to be meaningful.");
            Assert.IsLessThan(output.IndexOf("using System.Text;", StringComparison.Ordinal),
                              output.IndexOf("using System;", StringComparison.Ordinal),
                              "The usings must be reordered for the test to be meaningful.");

            var inputAssembly = Compile(input);
            var outputAssembly = Compile(output);

            Assert.AreSequenceEqual(inputAssembly, outputAssembly, $"The formatted code compiles differently under {DescribeLineEnding(endOfLine)} line endings.");
        }
    }

    /// <summary>
    /// Collects the text of every comment, directive, disabled-text span, and literal token in document order, taking the value
    /// instead of the text for multi-line raw string content
    /// </summary>
    /// <param name="root">The syntax root</param>
    /// <returns>The collected content</returns>
    private static List<string> CollectUserAuthoredContent(SyntaxNode root)
    {
        var content = new List<string>();

        foreach (var token in root.DescendantTokens(descendIntoTrivia: true))
        {
            foreach (var trivia in token.LeadingTrivia.Concat(token.TrailingTrivia))
            {
                AddTriviaContent(trivia, content);
            }

            if (token.Parent is LiteralExpressionSyntax
                || token.IsKind(SyntaxKind.InterpolatedStringTextToken))
            {
                content.Add(IsMultiLineRawStringContent(token)
                                ? $"raw:{token.ValueText}"
                                : $"literal:{token.Text}");
            }
        }

        return content;
    }

    /// <summary>
    /// Adds the content of one trivia to the list when it is user-authored
    /// </summary>
    /// <param name="trivia">The trivia</param>
    /// <param name="content">The collected content</param>
    private static void AddTriviaContent(SyntaxTrivia trivia, List<string> content)
    {
        switch (trivia.Kind())
        {
            case SyntaxKind.SingleLineCommentTrivia:
            case SyntaxKind.MultiLineCommentTrivia:
                {
                    content.Add($"comment:{trivia}");
                }
                break;

            case SyntaxKind.DisabledTextTrivia:
                {
                    content.Add($"disabled:{trivia}");
                }
                break;

            default:
                {
                    if (trivia.IsDirective)
                    {
                        content.Add($"directive:{trivia.ToString().Trim()}");
                    }
                }
                break;
        }
    }

    /// <summary>
    /// Determines whether the token is the content of a multi-line raw string literal, plain, UTF-8, or interpolated. Raw-string
    /// alignment may shift the indentation of such content (FMT-RAW), so only its value has to survive; the text of every other
    /// literal token must survive as written
    /// </summary>
    /// <param name="token">The literal token</param>
    /// <returns><see langword="true"/> if the token is multi-line raw string content</returns>
    private static bool IsMultiLineRawStringContent(SyntaxToken token)
    {
        if (token.IsKind(SyntaxKind.MultiLineRawStringLiteralToken)
            || token.IsKind(SyntaxKind.Utf8MultiLineRawStringLiteralToken))
        {
            return true;
        }

        return token.IsKind(SyntaxKind.InterpolatedStringTextToken)
               && token.Parent?.FirstAncestorOrSelf<InterpolatedStringExpressionSyntax>()?.StringStartToken.IsKind(SyntaxKind.InterpolatedMultiLineRawStringStartToken) == true;
    }

    /// <summary>
    /// Compiles the given source deterministically into a library and returns the emitted bytes
    /// </summary>
    /// <param name="source">The source to compile</param>
    /// <returns>The emitted assembly</returns>
    private byte[] Compile(string source)
    {
        var references = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")).Split(Path.PathSeparator)
                                                                                    .Select(path => MetadataReference.CreateFromFile(path));
        var compilation = CSharpCompilation.Create("Equivalence",
                                                   [CSharpSyntaxTree.ParseText(source, cancellationToken: TestContext.CancellationToken)],
                                                   references,
                                                   new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,
                                                                                optimizationLevel: OptimizationLevel.Release,
                                                                                deterministic: true));

        using (var stream = new MemoryStream())
        {
            var result = compilation.Emit(stream, cancellationToken: TestContext.CancellationToken);

            Assert.IsTrue(result.Success, string.Join(Environment.NewLine, result.Diagnostics));

            return stream.ToArray();
        }
    }

    #endregion // Methods
}