using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Clarity;
using Reihitsu.Analyzer.Rules.Clarity;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Clarity;

/// <summary>
/// Test methods for <see cref="RH3103UseShorthandForNullableTypesAnalyzer"/> and <see cref="RH3103UseShorthandForNullableTypesCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH3103UseShorthandForNullableTypesAnalyzerTests : AnalyzerTestsBase<RH3103UseShorthandForNullableTypesAnalyzer, RH3103UseShorthandForNullableTypesCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying qualified Nullable generic type is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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
    /// Verifying Nullable generic in typeof expression is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NullableGenericInTypeofIsNotReported()
    {
        const string testCode = """
                                using System;

                                public class Test
                                {
                                    public Type GetType()
                                    {
                                        return typeof(Nullable<int>);
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying Nullable generic in nameof expression is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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

    #endregion // Tests
}