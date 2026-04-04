using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Naming;

/// <summary>
/// Test methods for <see cref="RH0211MethodParameterCasingAnalyzer"/> and <see cref="RH0211MethodParameterCasingCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0211MethodParameterCasingAnalyzerTests : AnalyzerTestsBase<RH0211MethodParameterCasingAnalyzer, RH0211MethodParameterCasingCodeFixProvider>
{
    /// <summary>
    /// Verifying diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnostics()
    {
        const string testCode = """
            using System;

            namespace Reihitsu.Analyzer.Test.Naming.Resources;

            /// <summary>
            /// Test class
            /// </summary>
            /// <param name="PrimaryParameterName">Primary parameter</param>
            public class TestClass(int {|#0:PrimaryParameterName|})
            {
                /// <summary>
                /// Test method
                /// </summary>
                /// <param name="MethodParameterName">Method parameter</param>
                public void TestMethod(int {|#1:MethodParameterName|})
                {
                }
            }

            /// <summary>
            /// Test struct
            /// </summary>
            /// <param name="ParameterName">Parameter</param>
            public struct TestStruct(int {|#2:ParameterName|});
            """;

        const string fixedCode = """
            using System;

            namespace Reihitsu.Analyzer.Test.Naming.Resources;

            /// <summary>
            /// Test class
            /// </summary>
            /// <param name="primaryParameterName">Primary parameter</param>
            public class TestClass(int primaryParameterName)
            {
                /// <summary>
                /// Test method
                /// </summary>
                /// <param name="methodParameterName">Method parameter</param>
                public void TestMethod(int methodParameterName)
                {
                }
            }

            /// <summary>
            /// Test struct
            /// </summary>
            /// <param name="parameterName">Parameter</param>
            public struct TestStruct(int parameterName);
            """;

        await Verify(testCode, fixedCode, Diagnostics(RH0211MethodParameterCasingAnalyzer.DiagnosticId, AnalyzerResources.RH0211MessageFormat, 3));
    }

    /// <summary>
    /// Verifying no diagnostics for record class primary constructor parameters
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForRecordPrimaryConstructor()
    {
        const string testCode = """
            using System;

            namespace Reihitsu.Analyzer.Test.Naming.Resources;

            public record class TestRecord(int ParameterName);

            public record struct TestRecordStruct(int ParameterName);

            public class TestClass(int parameterName);

            public struct TestStruct(int parameterName);
            """;

        await Verify(testCode);
    }
}