using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Naming;
using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;
using Reihitsu.Analyzer.Test.Verifiers;

namespace Reihitsu.Analyzer.Test.Naming;

/// <summary>
/// Test methods for <see cref="RH4115LocalVariableCasingAnalyzer"/> and <see cref="RH4115LocalVariableCasingCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH4115LocalVariableCasingAnalyzerTests : AnalyzerTestsBase<RH4115LocalVariableCasingAnalyzer, RH4115LocalVariableCasingCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies diagnostics are reported for local variables that are not camelCase and that references are renamed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticsForLocalVariableAndReferenceAreFixed()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class DataLoader
                                    {
                                        public int Load()
                                        {
                                            int {|#0:ResultCount|} = 42;

                                            return ResultCount;
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     public class DataLoader
                                     {
                                         public int Load()
                                         {
                                             int resultCount = 42;

                                             return resultCount;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH4115LocalVariableCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4115MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported for camelCase local variables
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForCamelCaseLocalVariable()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class DataLoader
                                    {
                                        public int Load()
                                        {
                                            int resultCount = 42;

                                            return resultCount;
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies multiple local variables in a single declaration can produce multiple diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticsForMultipleLocalVariables()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class DataLoader
                                    {
                                        public void Load()
                                        {
                                            int {|#0:ResultCount|} = 42, {|#1:RetryCount|} = 2;
                                        }
                                    }
                                }
                                """;

        await Verify(testCode, Diagnostics(RH4115LocalVariableCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4115MessageFormat, 2));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for a foreach variable that is not camelCase and that references are renamed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForForEachVariableAndReferenceIsFixed()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class DataLoader
                                    {
                                        public int Load(int[] values)
                                        {
                                            var result = 0;

                                            foreach (var {|#0:CurrentValue|} in values)
                                            {
                                                result += CurrentValue;
                                            }

                                            return result;
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     public class DataLoader
                                     {
                                         public int Load(int[] values)
                                         {
                                             var result = 0;

                                             foreach (var currentValue in values)
                                             {
                                                 result += currentValue;
                                             }

                                             return result;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH4115LocalVariableCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4115MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for a for-initializer variable that is not camelCase and that references are
    /// renamed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForForInitializerVariableAndReferenceIsFixed()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class DataLoader
                                    {
                                        public void Load()
                                        {
                                            for (var {|#0:CurrentIndex|} = 0; CurrentIndex < 2; CurrentIndex++)
                                            {
                                            }
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     public class DataLoader
                                     {
                                         public void Load()
                                         {
                                             for (var currentIndex = 0; currentIndex < 2; currentIndex++)
                                             {
                                             }
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH4115LocalVariableCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4115MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for a using-statement variable that is not camelCase and that references are
    /// renamed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForUsingStatementVariableAndReferenceIsFixed()
    {
        const string testCode = """
                                using System.IO;

                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class DataLoader
                                    {
                                        public long Load()
                                        {
                                            using (var {|#0:DataStream|} = new MemoryStream())
                                            {
                                                return DataStream.Length;
                                            }
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System.IO;

                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     public class DataLoader
                                     {
                                         public long Load()
                                         {
                                             using (var dataStream = new MemoryStream())
                                             {
                                                 return dataStream.Length;
                                             }
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH4115LocalVariableCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4115MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for a fixed-statement variable that is not camelCase and that references are
    /// renamed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForFixedStatementVariableAndReferenceIsFixed()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class DataLoader
                                    {
                                        public unsafe char Load(string value)
                                        {
                                            fixed (char* {|#0:ValuePointer|} = value)
                                            {
                                                return *ValuePointer;
                                            }
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     public class DataLoader
                                     {
                                         public unsafe char Load(string value)
                                         {
                                             fixed (char* valuePointer = value)
                                             {
                                                 return *valuePointer;
                                             }
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode,
                     fixedCode,
                     AllowUnsafe,
                     Diagnostics(RH4115LocalVariableCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4115MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for an out variable that is not camelCase and that references are renamed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForOutVariableAndReferenceIsFixed()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class DataLoader
                                    {
                                        public int Load()
                                        {
                                            int.TryParse("42", out var {|#0:ResultCount|});

                                            return ResultCount;
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     public class DataLoader
                                     {
                                         public int Load()
                                         {
                                             int.TryParse("42", out var resultCount);

                                             return resultCount;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH4115LocalVariableCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4115MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for a pattern variable that is not camelCase and that references are renamed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForPatternVariableAndReferenceIsFixed()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class DataLoader
                                    {
                                        public int Load(object value)
                                        {
                                            if (value is int {|#0:ResultCount|})
                                            {
                                                return ResultCount;
                                            }

                                            return 0;
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     public class DataLoader
                                     {
                                         public int Load(object value)
                                         {
                                             if (value is int resultCount)
                                             {
                                                 return resultCount;
                                             }

                                             return 0;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH4115LocalVariableCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4115MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for a catch variable that is not camelCase and that references are renamed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForCatchVariableAndReferenceIsFixed()
    {
        const string testCode = """
                                using System;

                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class DataLoader
                                    {
                                        public string Load()
                                        {
                                            try
                                            {
                                                throw new InvalidOperationException();
                                            }
                                            catch (InvalidOperationException {|#0:CaughtException|})
                                            {
                                                return CaughtException.Message;
                                            }
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     public class DataLoader
                                     {
                                         public string Load()
                                         {
                                             try
                                             {
                                                 throw new InvalidOperationException();
                                             }
                                             catch (InvalidOperationException caughtException)
                                             {
                                                 return caughtException.Message;
                                             }
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH4115LocalVariableCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4115MessageFormat));
    }

    /// <summary>
    /// Verifies mixed local declaration forms are renamed together by Fix All in one iteration
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMixedLocalDeclarationFormsAreFixedInOneFixAllIteration()
    {
        const string testCode = """
                                using System;

                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class DataLoader
                                    {
                                        public int Load(int[] values)
                                        {
                                            var result = 0;

                                            foreach (var {|#0:CurrentValue|} in values)
                                            {
                                                result += CurrentValue;
                                            }

                                            int.TryParse("42", out var {|#1:ParsedValue|});

                                            try
                                            {
                                                throw new InvalidOperationException();
                                            }
                                            catch (InvalidOperationException {|#2:CaughtException|})
                                            {
                                                result += CaughtException.Message.Length;
                                            }

                                            return result + ParsedValue;
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     public class DataLoader
                                     {
                                         public int Load(int[] values)
                                         {
                                             var result = 0;

                                             foreach (var currentValue in values)
                                             {
                                                 result += currentValue;
                                             }

                                             int.TryParse("42", out var parsedValue);

                                             try
                                             {
                                                 throw new InvalidOperationException();
                                             }
                                             catch (InvalidOperationException caughtException)
                                             {
                                                 result += caughtException.Message.Length;
                                             }

                                             return result + parsedValue;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode,
                     fixedCode,
                     static config => config.NumberOfFixAllIterations = 1,
                     Diagnostics(RH4115LocalVariableCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4115MessageFormat, 3));
    }

    /// <summary>
    /// Verifies deconstruction variables remain the responsibility of RH4117
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForDeconstructionVariables()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class DataLoader
                                    {
                                        public void Load()
                                        {
                                            var (ResultCount, RetryCount) = (42, 2);
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no code fix is offered for a letterless local variable whose conversion cannot produce a valid identifier
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NoCodeFixForLetterlessLocalVariable()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class DataLoader
                                    {
                                        public void Load()
                                        {
                                            int _ = 42;
                                        }
                                    }
                                }
                                """;

        var actions = await GetCodeFixActionsAsync(testCode,
                                                   RH4115LocalVariableCasingAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<VariableDeclaratorSyntax>()
                                                               .Single(declarator => declarator.Identifier.ValueText == "_")
                                                               .Identifier
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    #endregion // Tests

    #region Methods

    /// <summary>
    /// Enables unsafe code for the verifier so fixed statements compile
    /// </summary>
    /// <param name="test">Test</param>
    private static void AllowUnsafe(CSharpCodeFixVerifierTest<RH4115LocalVariableCasingAnalyzer, RH4115LocalVariableCasingCodeFixProvider> test)
    {
        test.SolutionTransforms.Add((solution, projectId) => solution.GetProject(projectId)?.CompilationOptions is CSharpCompilationOptions compilationOptions
                                                                 ? solution.WithProjectCompilationOptions(projectId, compilationOptions.WithAllowUnsafe(true))
                                                                 : solution);
    }

    #endregion // Methods
}