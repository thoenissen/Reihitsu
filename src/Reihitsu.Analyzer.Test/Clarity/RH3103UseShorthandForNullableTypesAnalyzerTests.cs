using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Clarity;
using Reihitsu.Analyzer.Rules.Clarity;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Clarity;

/// <summary>
/// Test methods for <see cref="RH3103UseShorthandForNullableTypesAnalyzer"/> and <see cref="RH3103UseShorthandForNullableTypesCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH3103UseShorthandForNullableTypesAnalyzerTests : BatchCodeFixTestsBase<RH3103UseShorthandForNullableTypesAnalyzer, RH3103UseShorthandForNullableTypesCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying qualified Nullable generic type is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3103-001")]
    [Requirement("RH3103-003")]
    [Requirement("RH3103-004")]
    public async Task QualifiedNullableGenericIsReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    private {|#0:System.Nullable<int>|} _value;
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private int? _value;
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3103UseShorthandForNullableTypesAnalyzer.DiagnosticId, "Use shorthand for nullable types."));
    }

    /// <summary>
    /// Verifying unqualified Nullable generic type is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3103-001")]
    [Requirement("RH3103-003")]
    [Requirement("RH3103-004")]
    public async Task UnqualifiedNullableGenericIsReportedAndFixed()
    {
        const string testCode = """
                                using System;

                                public class Test
                                {
                                    private {|#0:Nullable<int>|} _value;
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 public class Test
                                 {
                                     private int? _value;
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3103UseShorthandForNullableTypesAnalyzer.DiagnosticId, "Use shorthand for nullable types."));
    }

    /// <summary>
    /// Verifying Nullable generic as return type is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3103-001")]
    [Requirement("RH3103-003")]
    [Requirement("RH3103-004")]
    public async Task NullableGenericAsReturnTypeIsReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    public {|#0:System.Nullable<int>|} GetValue()
                                    {
                                        return null;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public int? GetValue()
                                     {
                                         return null;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3103UseShorthandForNullableTypesAnalyzer.DiagnosticId, "Use shorthand for nullable types."));
    }

    /// <summary>
    /// Verifying Nullable generic as parameter type is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3103-001")]
    [Requirement("RH3103-003")]
    [Requirement("RH3103-004")]
    public async Task NullableGenericAsParameterTypeIsReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    public void Method({|#0:System.Nullable<int>|} value)
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public void Method(int? value)
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3103UseShorthandForNullableTypesAnalyzer.DiagnosticId, "Use shorthand for nullable types."));
    }

    /// <summary>
    /// Verifying Nullable generic in local variable is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3103-001")]
    [Requirement("RH3103-003")]
    [Requirement("RH3103-004")]
    public async Task NullableGenericInLocalVariableIsReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    public void Run()
                                    {
                                        {|#0:System.Nullable<double>|} result = null;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public void Run()
                                     {
                                         double? result = null;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3103UseShorthandForNullableTypesAnalyzer.DiagnosticId, "Use shorthand for nullable types."));
    }

    /// <summary>
    /// Verifying Nullable generic with struct type is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3103-001")]
    [Requirement("RH3103-003")]
    [Requirement("RH3103-004")]
    public async Task NullableGenericWithStructIsReportedAndFixed()
    {
        const string testCode = """
                                using System;

                                public class Test
                                {
                                    private {|#0:Nullable<Guid>|} _id;
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 public class Test
                                 {
                                     private Guid? _id;
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3103UseShorthandForNullableTypesAnalyzer.DiagnosticId, "Use shorthand for nullable types."));
    }

    /// <summary>
    /// Verifying Nullable generic with custom struct is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3103-001")]
    [Requirement("RH3103-003")]
    [Requirement("RH3103-004")]
    public async Task NullableGenericWithCustomStructIsReportedAndFixed()
    {
        const string testCode = """
                                using System;

                                public struct Point
                                {
                                    public int X { get; set; }
                                    public int Y { get; set; }
                                }

                                public class Test
                                {
                                    private {|#0:Nullable<Point>|} _location;
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 public struct Point
                                 {
                                     public int X { get; set; }
                                     public int Y { get; set; }
                                 }

                                 public class Test
                                 {
                                     private Point? _location;
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3103UseShorthandForNullableTypesAnalyzer.DiagnosticId, "Use shorthand for nullable types."));
    }

    /// <summary>
    /// Verifying Nullable generic in array is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3103-001")]
    [Requirement("RH3103-003")]
    [Requirement("RH3103-004")]
    public async Task NullableGenericInArrayIsReportedAndFixed()
    {
        const string testCode = """
                                using System;

                                public class Test
                                {
                                    private {|#0:Nullable<int>|}[] _values;
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 public class Test
                                 {
                                     private int?[] _values;
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3103UseShorthandForNullableTypesAnalyzer.DiagnosticId, "Use shorthand for nullable types."));
    }

    /// <summary>
    /// Verifying Nullable generic in typeof expression is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3103-001")]
    [Requirement("RH3103-003")]
    [Requirement("RH3103-004")]
    public async Task NullableGenericInTypeofIsReportedAndFixed()
    {
        const string testCode = """
                                using System;

                                public class Test
                                {
                                    public Type GetType()
                                    {
                                        return typeof({|#0:Nullable<int>|});
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 public class Test
                                 {
                                     public Type GetType()
                                     {
                                         return typeof(int?);
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3103UseShorthandForNullableTypesAnalyzer.DiagnosticId, "Use shorthand for nullable types."));
    }

    /// <summary>
    /// Verifying Nullable generic nested in typeof expression is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3103-001")]
    [Requirement("RH3103-003")]
    [Requirement("RH3103-004")]
    public async Task NullableGenericNestedInTypeofIsReportedAndFixed()
    {
        const string testCode = """
                                using System;
                                using System.Collections.Generic;

                                public class Test
                                {
                                    public Type GetType()
                                    {
                                        return typeof(List<{|#0:Nullable<int>|}>);
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;
                                 using System.Collections.Generic;

                                 public class Test
                                 {
                                     public Type GetType()
                                     {
                                         return typeof(List<int?>);
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3103UseShorthandForNullableTypesAnalyzer.DiagnosticId, "Use shorthand for nullable types."));
    }

    /// <summary>
    /// Verifying comments inside a Nullable generic in typeof are preserved by the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3103-004")]
    public async Task NullableGenericInTypeofPreservesInnerComments()
    {
        const string testCode = """
                                using System;

                                public class Test
                                {
                                    public Type GetType()
                                    {
                                        return typeof({|#0:Nullable</* before */ int /* after */>|});
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 public class Test
                                 {
                                     public Type GetType()
                                     {
                                         return typeof(/* before */ int /* after */?);
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3103UseShorthandForNullableTypesAnalyzer.DiagnosticId, "Use shorthand for nullable types."));
    }

    /// <summary>
    /// Verifying a fix is not offered when a Nullable generic in typeof contains an own-line block comment
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3103-005")]
    public async Task NullableGenericInTypeofWithOwnLineBlockCommentDoesNotOfferFix()
    {
        const string testCode = """
                                using System;

                                public class Test
                                {
                                    public Type GetType()
                                    {
                                        return typeof(Nullable<
                                            /* keep */
                                            int>);
                                    }
                                }
                                """;

        var actions = await GetCodeFixActionsAsync(testCode,
                                                   RH3103UseShorthandForNullableTypesAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<GenericNameSyntax>()
                                                               .Single()
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifying a fix is not offered when a Nullable generic in typeof contains directives
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3103-005")]
    public async Task NullableGenericInTypeofWithDirectivesDoesNotOfferFix()
    {
        const string testCode = """
                                using System;

                                public class Test
                                {
                                    public Type GetType()
                                    {
                                        return typeof(Nullable<
                                #if DEBUG
                                            int
                                #else
                                            long
                                #endif
                                        >);
                                    }
                                }
                                """;

        var actions = await GetCodeFixActionsAsync(testCode,
                                                   RH3103UseShorthandForNullableTypesAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<GenericNameSyntax>()
                                                               .Single()
                                                               .GetLocation(),
                                                   "DEBUG");

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifying a fix is not offered when a qualified Nullable generic contains a wrapper comment
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3103-005")]
    public async Task QualifiedNullableGenericWithWrapperCommentDoesNotOfferFix()
    {
        const string testCode = """
                                using System;

                                public class Test
                                {
                                    public Type GetType()
                                    {
                                        return typeof(System /* keep */ .Nullable<int>);
                                    }
                                }
                                """;

        var actions = await GetCodeFixActionsAsync(testCode,
                                                   RH3103UseShorthandForNullableTypesAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<QualifiedNameSyntax>()
                                                               .Single()
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifying a fix is not offered when a qualified Nullable generic contains wrapper directives
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3103-005")]
    public async Task QualifiedNullableGenericWithWrapperDirectivesDoesNotOfferFix()
    {
        const string testCode = """
                                using System;

                                public class Test
                                {
                                    public Type GetType()
                                    {
                                        return typeof(System
                                #if DEBUG
                                #endif
                                            .Nullable<int>);
                                    }
                                }
                                """;

        var actions = await GetCodeFixActionsAsync(testCode,
                                                   RH3103UseShorthandForNullableTypesAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<QualifiedNameSyntax>()
                                                               .Single()
                                                               .GetLocation(),
                                                   "DEBUG");

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifying alias-qualified Nullable generic in typeof is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3103-001")]
    [Requirement("RH3103-004")]
    public async Task AliasQualifiedNullableGenericInTypeofIsReportedAndFixed()
    {
        const string testCode = """
                                using S = System;

                                public class Test
                                {
                                    public System.Type GetType()
                                    {
                                        return typeof(S::{|#0:Nullable<int>|});
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using S = System;

                                 public class Test
                                 {
                                     public System.Type GetType()
                                     {
                                         return typeof(int?);
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3103UseShorthandForNullableTypesAnalyzer.DiagnosticId, "Use shorthand for nullable types."));
    }

    /// <summary>
    /// Verifying wrapper whitespace is normalized when a Nullable generic in typeof is fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3103-004")]
    public async Task NullableGenericInTypeofNormalizesWrapperWhitespace()
    {
        const string testCode = """
                                using System;

                                public class Test
                                {
                                    public Type GetType()
                                    {
                                        return typeof({|#0:Nullable< int >|});
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 public class Test
                                 {
                                     public Type GetType()
                                     {
                                         return typeof(int?);
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3103UseShorthandForNullableTypesAnalyzer.DiagnosticId, "Use shorthand for nullable types."));
    }

    /// <summary>
    /// Verifying Nullable generic in nameof expression is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3103-002")]
    public async Task NullableGenericInNameofIsNotReported()
    {
        const string testCode = """
                                using System;

                                public class Test
                                {
                                    public string GetName()
                                    {
                                        return nameof(Nullable<int>);
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying Nullable generic nested in a nameof expression is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3103-001")]
    [Requirement("RH3103-004")]
    public async Task NullableGenericNestedInNameofIsReportedAndFixed()
    {
        const string testCode = """
                                using System;
                                using System.Collections.Generic;

                                public class Test
                                {
                                    public string GetName()
                                    {
                                        return nameof(List<{|#0:Nullable<int>|}>);
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;
                                 using System.Collections.Generic;

                                 public class Test
                                 {
                                     public string GetName()
                                     {
                                         return nameof(List<int?>);
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3103UseShorthandForNullableTypesAnalyzer.DiagnosticId, "Use shorthand for nullable types."));
    }

    /// <summary>
    /// Verifying Nullable generic in an escaped nameof method call is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3103-001")]
    [Requirement("RH3103-004")]
    public async Task NullableGenericInEscapedNameofMethodCallIsReportedAndFixed()
    {
        const string testCode = """
                                using System;

                                public class Test
                                {
                                    private string @nameof(int? value)
                                    {
                                        return value.ToString();
                                    }

                                    public string GetName()
                                    {
                                        return @nameof(default({|#0:Nullable<int>|}));
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 public class Test
                                 {
                                     private string @nameof(int? value)
                                     {
                                         return value.ToString();
                                     }

                                     public string GetName()
                                     {
                                         return @nameof(default(int?));
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3103UseShorthandForNullableTypesAnalyzer.DiagnosticId, "Use shorthand for nullable types."));
    }

    /// <summary>
    /// Verifying Nullable shorthand is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NullableShorthandIsNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    private int? _value;
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying Nullable generic in property is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3103-001")]
    [Requirement("RH3103-003")]
    [Requirement("RH3103-004")]
    public async Task NullableGenericInPropertyIsReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    public {|#0:System.Nullable<long>|} Value { get; set; }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public long? Value { get; set; }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3103UseShorthandForNullableTypesAnalyzer.DiagnosticId, "Use shorthand for nullable types."));
    }

    /// <summary>
    /// Verifies unqualified, qualified, and alias-qualified <c>Nullable&lt;T&gt;</c> in every listed type position is
    /// reported over the whole name and fixed to <c>T?</c>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3103-001")]
    [Requirement("RH3103-003")]
    public async Task NullableGenericInEveryListedTypePositionIsReportedOverTheWholeName()
    {
        const string testCode = """
                                using System;
                                using System.Collections.Generic;

                                class C
                                {
                                    {|RH3103:Nullable<int>|} _a;
                                    {|RH3103:System.Nullable<DateTime>|} _b;
                                    {|RH3103:global::System.Nullable<long>|} _c;
                                    {|RH3103:Nullable<int>|} P { get; }

                                    {|RH3103:Nullable<int>|} M({|RH3103:Nullable<int>|} p)
                                    {
                                        var t = typeof({|RH3103:Nullable<int>|});
                                        var d = default({|RH3103:Nullable<int>|});
                                        {|RH3103:Nullable<int>|}[] arr = null;
                                        var n = nameof(List<{|RH3103:Nullable<int>|}>);
                                        return p;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;
                                 using System.Collections.Generic;

                                 class C
                                 {
                                     int? _a;
                                     DateTime? _b;
                                     long? _c;
                                     int? P { get; }

                                     int? M(int? p)
                                     {
                                         var t = typeof(int?);
                                         var d = default(int?);
                                         int?[] arr = null;
                                         var n = nameof(List<int?>);
                                         return p;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies the argument of <c>nameof</c> in an expression-bodied method is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3103-002")]
    public async Task NullableGenericAsNameofArgumentInExpressionBodyIsNotReported()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    string M() => nameof(Nullable<int>);
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies the fix writes <c>T?</c> for unqualified and qualified names and keeps the block comments inside the
    /// angle brackets next to the type argument
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3103-004")]
    public async Task FixWritesTheShorthandAndKeepsTheCommentsAroundTheTypeArgument()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    {|RH3103:Nullable<int>|} _a;
                                    {|RH3103:System.Nullable<DateTime>|} _b;
                                    {|RH3103:Nullable</* a */ int /* b */>|} _c;
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 class C
                                 {
                                     int? _a;
                                     DateTime? _b;
                                     /* a */ int /* b */? _c;
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies a comment between the parts of a qualified name keeps the diagnostic and withholds the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3103-005")]
    public async Task CommentInsideTheQualifiedFieldTypeIsReportedWithoutACodeFix()
    {
        const string testCode = """
                                class C
                                {
                                    {|RH3103:System./* a */Nullable<int>|} _a;
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies a block comment on a line of its own inside the angle brackets keeps the diagnostic and withholds
    /// the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3103-005")]
    public async Task OwnLineBlockCommentInsideTheFieldTypeIsReportedWithoutACodeFix()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    {|RH3103:Nullable<
                                        /* a */
                                        int>|} _a;
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Characterizes that an unbound <c>Nullable&lt;&gt;</c> in <c>typeof</c> is reported and the fix writes a
    /// question mark without a type
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task FixOfUnboundNullableGenericInTypeofWritesAQuestionMarkOnly()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    Type M() => typeof(Nullable<>);
                                }
                                """;

        const string expectedCode = """
                                    using System;

                                    class C
                                    {
                                        Type M() => typeof(?);
                                    }
                                    """;

        Assert.AreEqual(expectedCode, await ApplyCodeFixAsync(testCode));
    }

    /// <summary>
    /// Characterizes that a <c>Nullable&lt;T&gt;</c> that qualifies a member inside <c>nameof</c> is reported and
    /// fixed to the shorthand
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task FixOfNullableGenericQualifyingAMemberInNameofWritesTheShorthand()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    string M() => nameof(Nullable<int>.HasValue);
                                }
                                """;

        const string expectedCode = """
                                    using System;

                                    class C
                                    {
                                        string M() => nameof(int?.HasValue);
                                    }
                                    """;

        Assert.AreEqual(expectedCode, await ApplyCodeFixAsync(testCode));
    }

    /// <summary>
    /// Characterizes that a <c>Nullable&lt;T&gt;</c> that receives a static method call is reported and fixed to
    /// the shorthand
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task FixOfNullableGenericReceivingAStaticCallWritesTheShorthand()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    bool M() => Nullable<int>.Equals(1, 2);
                                }
                                """;

        const string expectedCode = """
                                    using System;

                                    class C
                                    {
                                        bool M() => int?.Equals(1, 2);
                                    }
                                    """;

        Assert.AreEqual(expectedCode, await ApplyCodeFixAsync(testCode));
    }

    /// <summary>
    /// Characterizes that a <c>Nullable&lt;T&gt;</c> in a using alias is reported and fixed to the shorthand
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task FixOfNullableGenericInUsingAliasWritesTheShorthand()
    {
        const string testCode = """
                                using N = System.Nullable<int>;

                                class C
                                {
                                    N _a;
                                }
                                """;

        const string expectedCode = """
                                    using N = int?;

                                    class C
                                    {
                                        N _a;
                                    }
                                    """;

        Assert.AreEqual(expectedCode, await ApplyCodeFixAsync(testCode));
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                using System;

                                public class Test
                                {
                                    public Type GetType()
                                    {
                                        return typeof(({|#0:Nullable<int>|}, {|#1:Nullable<long>|}));
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 public class Test
                                 {
                                     public Type GetType()
                                     {
                                         return typeof((int?, long?));
                                     }
                                 }
                                 """;

        // Verifying multiple Nullable generics in one typeof are fixed in one Fix All iteration
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH3103UseShorthandForNullableTypesAnalyzer.DiagnosticId, "Use shorthand for nullable types.", 2),
                                  static config => config.NumberOfFixAllIterations = 1);
    }

    #endregion // BatchCodeFixTestsBase
}