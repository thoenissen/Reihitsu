using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
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

    /// <summary>
    /// Verifies an unrelated compiler error elsewhere in the document does not suppress a safe rename
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task UnrelatedDocumentCompilerErrorDoesNotSuppressSafeRename()
    {
        const string testCode = """
                                internal class TestClass
                                {
                                    MissingType ExistingError;

                                    void Method()
                                    {
                                        int BadName = 1;
                                    }
                                }
                                """;

        var actions = await GetCodeFixActionsAsync(testCode,
                                                   RH4115LocalVariableCasingAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<VariableDeclaratorSyntax>()
                                                               .Single(declarator => declarator.Identifier.ValueText == "BadName")
                                                               .Identifier
                                                               .GetLocation());

        Assert.HasCount(1, actions);
    }

    /// <summary>
    /// Verifies the custom provider retains the three scopes supported by the prior batch provider
    /// </summary>
    [TestMethod]
    public void FixAllProviderSupportsDocumentProjectAndSolutionScopes()
    {
        var provider = new RH4115LocalVariableCasingCodeFixProvider();
        var scopes = provider.GetFixAllProvider().GetSupportedFixAllScopes().ToArray();

        Assert.AreSequenceEqual(new[] { FixAllScope.Document, FixAllScope.Project, FixAllScope.Solution }, scopes);
    }

    /// <summary>
    /// Verifies broader Fix All scopes retrieve and apply their aggregate diagnostics
    /// </summary>
    /// <param name="scope">Fix All scope</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow(FixAllScope.Project)]
    [DataRow(FixAllScope.Solution)]
    public async Task BroaderFixAllScopesApplyUniqueRename(FixAllScope scope)
    {
        const string testCode = """
                                internal class TestClass
                                {
                                    int Method()
                                    {
                                        int BadName = 1;

                                        return BadName;
                                    }
                                }
                                """;
        const string fixedCode = """
                                 internal class TestClass
                                 {
                                     int Method()
                                     {
                                         int badName = 1;

                                         return badName;
                                     }
                                 }
                                 """;

        var (fixedSource, initialAnalyzerDiagnostics, postFixAnalyzerDiagnostics, compilerErrors) = await ApplyFixAllAsync(testCode, scope);

        Assert.HasCount(1, initialAnalyzerDiagnostics);
        Assert.IsEmpty(postFixAnalyzerDiagnostics);
        Assert.AreEqual(fixedCode, fixedSource);
        Assert.IsEmpty(compilerErrors);
    }

    /// <summary>
    /// Verifies a local-variable casing fix does not introduce a duplicate declaration in a nested block
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task Issue469LocalRenameDoesNotProduceDuplicateDeclaration()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class DataLoader
                                    {
                                        public void Load()
                                        {
                                            int badName = 0;

                                            {
                                                int BadName = 1;
                                            }
                                        }
                                    }
                                }
                                """;

        var actions = await GetCodeFixActionsAsync(testCode,
                                                   RH4115LocalVariableCasingAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<VariableDeclaratorSyntax>()
                                                               .Single(declarator => declarator.Identifier.ValueText == "BadName")
                                                               .Identifier
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifies collisions are refused for each declaration-node form supported by RH4115
    /// </summary>
    /// <param name="declaration">Declaration that would collide with the enclosing local</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("foreach (var BadName in new int[0]) { }")]
    [DataRow("if (new object() is int BadName) { }")]
    [DataRow("try { } catch (System.Exception BadName) { }")]
    public async Task CollisionIsRefusedForSupportedLocalDeclarationForms(string declaration)
    {
        var testCode = $$"""
                         internal class TestClass
                         {
                             void Method()
                             {
                                 int badName = 0;
                                 {{declaration}}
                             }
                         }
                         """;

        var actions = await GetCodeFixActionsAsync(testCode,
                                                   RH4115LocalVariableCasingAnalyzer.DiagnosticId,
                                                   root => root.DescendantTokens()
                                                               .Single(token => token.ValueText == "BadName")
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifies multiple colliding diagnostics are refused independently
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task MultipleCollidingDiagnosticsAreRefusedIndependently()
    {
        const string testCode = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        int firstName = 0;
                                        int secondName = 0;

                                        {
                                            int FirstName = 1;
                                            int SecondName = 2;
                                        }
                                    }
                                }
                                """;

        foreach (var identifier in new[] { "FirstName", "SecondName" })
        {
            var actions = await GetCodeFixActionsAsync(testCode,
                                                       RH4115LocalVariableCasingAnalyzer.DiagnosticId,
                                                       root => root.DescendantTokens()
                                                                   .Single(token => token.ValueText == identifier)
                                                                   .GetLocation());

            Assert.IsEmpty(actions);
        }
    }

    /// <summary>
    /// Verifies diagnostics that normalize to one shared target are refused as a group by Fix All
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task DuplicateNormalizedTargetGroupIsRefusedBeforeFixAll()
    {
        const string testCode = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        int BadName = 1;
                                        int BAD_NAME = 2;
                                    }
                                }
                                """;

        var (fixedSource, initialAnalyzerDiagnostics, postFixAnalyzerDiagnostics, compilerErrors) = await ApplyFixAllAsync(testCode);

        Assert.HasCount(2, initialAnalyzerDiagnostics);
        Assert.AreEqual(testCode, fixedSource);
        Assert.HasCount(2, postFixAnalyzerDiagnostics);
        Assert.IsEmpty(compilerErrors);
    }

    /// <summary>
    /// Verifies equal normalized targets in independent sibling declaration spaces remain fixable together
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task DuplicateNormalizedTargetsInSiblingScopesRemainFixableTogether()
    {
        const string testCode = """
                                internal class TestClass
                                {
                                    void Method(bool condition)
                                    {
                                        if (condition)
                                        {
                                            int BadName = 1;
                                        }

                                        if (!condition)
                                        {
                                            int BAD_NAME = 2;
                                        }
                                    }
                                }
                                """;
        const string fixedCode = """
                                 internal class TestClass
                                 {
                                     void Method(bool condition)
                                     {
                                         if (condition)
                                         {
                                             int badName = 1;
                                         }

                                         if (!condition)
                                         {
                                             int badName = 2;
                                         }
                                     }
                                 }
                                 """;

        var (fixedSource, initialAnalyzerDiagnostics, postFixAnalyzerDiagnostics, compilerErrors) = await ApplyFixAllAsync(testCode);

        Assert.HasCount(2, initialAnalyzerDiagnostics);
        Assert.IsEmpty(postFixAnalyzerDiagnostics);
        Assert.AreEqual(fixedCode, fixedSource);
        Assert.IsEmpty(compilerErrors);
    }

    /// <summary>
    /// Verifies distinct normalized targets remain fixable together in one Fix All iteration
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task UniqueNormalizedTargetsRemainFixableTogether()
    {
        const string testCode = """
                                internal class TestClass
                                {
                                    int Method()
                                    {
                                        int {|#0:BadName|} = 1;
                                        int {|#1:OTHER_NAME|} = 2;

                                        return BadName + OTHER_NAME;
                                    }
                                }
                                """;
        const string fixedCode = """
                                 internal class TestClass
                                 {
                                     int Method()
                                     {
                                         int badName = 1;
                                         int otherName = 2;

                                         return badName + otherName;
                                     }
                                 }
                                 """;

        var plainTestCode = testCode.Replace("{|#0:", string.Empty)
                                    .Replace("{|#1:", string.Empty)
                                    .Replace("|}", string.Empty);
        var (fixedSource, initialAnalyzerDiagnostics, postFixAnalyzerDiagnostics, compilerErrors) = await ApplyFixAllAsync(plainTestCode);

        Assert.HasCount(2, initialAnalyzerDiagnostics);
        Assert.IsEmpty(postFixAnalyzerDiagnostics);
        Assert.AreEqual(fixedCode, fixedSource);
        Assert.IsEmpty(compilerErrors, string.Join(Environment.NewLine, compilerErrors));

        await Verify(testCode,
                     fixedCode,
                     static config => config.NumberOfFixAllIterations = 1,
                     Diagnostics(RH4115LocalVariableCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4115MessageFormat, 2));
    }

    #endregion // Tests

    #region Methods

    /// <summary>
    /// Enables unsafe code for the verifier so fixed statements compile
    /// </summary>
    /// <param name="test">Test</param>
    private static void AllowUnsafe(CSharpCodeFixVerifierTest<RH4115LocalVariableCasingAnalyzer, RH4115LocalVariableCasingCodeFixProvider> test)
    {
        test.SolutionTransforms.Add(ApplyAllowUnsafeToTestProject);
    }

    /// <summary>
    /// Applies the provider's requested Fix All action to every supplied analyzer diagnostic
    /// </summary>
    /// <param name="source">Source text</param>
    /// <param name="scope">Fix All scope</param>
    /// <returns>The fixed source, initial and post-fix analyzer diagnostics, and compiler errors after Fix All</returns>
    private static async Task<(string FixedSource,
    ImmutableArray<Diagnostic> InitialAnalyzerDiagnostics,
    ImmutableArray<Diagnostic> PostFixAnalyzerDiagnostics,
    ImmutableArray<Diagnostic> CompilerErrors)> ApplyFixAllAsync(string source, FixAllScope scope = FixAllScope.Document)
    {
        using (var workspace = new AdhocWorkspace())
        {
            var projectId = ProjectId.CreateNewId();
            var documentId = DocumentId.CreateNewId(projectId);
            var solution = workspace.CurrentSolution
                                    .AddProject(projectId, "TestProject", "TestProject", LanguageNames.CSharp)
                                    .WithProjectCompilationOptions(projectId, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                                    .AddMetadataReferences(projectId, GetMetadataReferences())
                                    .AddDocument(documentId, "Test.cs", SourceText.From(source));
            var document = solution.GetDocument(documentId)
                               ?? throw new InvalidOperationException("Failed to create test document.");
            var compilation = await document.Project.GetCompilationAsync(CancellationToken.None).ConfigureAwait(false)
                                  ?? throw new InvalidOperationException("Failed to compile test document.");
            var analyzerDiagnostics = await compilation.WithAnalyzers(ImmutableArray.Create<DiagnosticAnalyzer>(new RH4115LocalVariableCasingAnalyzer()))
                                                       .GetAnalyzerDiagnosticsAsync(CancellationToken.None)
                                                       .ConfigureAwait(false);
            var provider = new RH4115LocalVariableCasingCodeFixProvider();
            var context = new FixAllContext(document,
                                            provider,
                                            scope,
                                            provider.GetType().Name,
                                            provider.FixableDiagnosticIds,
                                            new TestFixAllDiagnosticProvider(analyzerDiagnostics),
                                            CancellationToken.None);
            var action = await provider.GetFixAllProvider()
                                       .GetFixAsync(context)
                                       .ConfigureAwait(false)
                             ?? throw new InvalidOperationException("Failed to create the Fix All action.");

            Assert.AreEqual(provider.GetType().Name, action.EquivalenceKey);

            var operation = (await action.GetOperationsAsync(CancellationToken.None).ConfigureAwait(false)).OfType<ApplyChangesOperation>()
                                                                                                           .Single();
            var fixedDocument = operation.ChangedSolution.GetDocument(documentId)
                                    ?? throw new InvalidOperationException("Failed to get the fixed document.");
            var fixedCompilation = await fixedDocument.Project.GetCompilationAsync(CancellationToken.None).ConfigureAwait(false)
                                       ?? throw new InvalidOperationException("Failed to compile the fixed document.");
            var compilerErrors = fixedCompilation.GetDiagnostics(CancellationToken.None)
                                                 .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
                                                 .ToImmutableArray();
            var postFixAnalyzerDiagnostics = await fixedCompilation.WithAnalyzers(ImmutableArray.Create<DiagnosticAnalyzer>(new RH4115LocalVariableCasingAnalyzer()))
                                                                   .GetAnalyzerDiagnosticsAsync(CancellationToken.None)
                                                                   .ConfigureAwait(false);
            var fixedSource = await fixedDocument.GetTextAsync(CancellationToken.None).ConfigureAwait(false);

            return (fixedSource.ToString(), analyzerDiagnostics, postFixAnalyzerDiagnostics, compilerErrors);
        }
    }

    /// <summary>
    /// Gets the platform references needed by the aggregate code-fix document
    /// </summary>
    /// <returns>Metadata references</returns>
    private static IEnumerable<MetadataReference> GetMetadataReferences()
    {
        var trustedPlatformAssemblies = AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string;
        var referencePaths = trustedPlatformAssemblies?.Split(Path.PathSeparator)
                                 ?? [];

        return referencePaths.Select(static path => MetadataReference.CreateFromFile(path));
    }

    #endregion // Methods

    #region Types

#pragma warning disable RH2101 // The provider is a focused test adapter owned by this fixture
    /// <summary>
    /// Supplies the document diagnostics used by the focused Fix All tests
    /// </summary>
    private sealed class TestFixAllDiagnosticProvider : FixAllContext.DiagnosticProvider
    {
        #region Fields

        /// <summary>
        /// Diagnostics
        /// </summary>
        private readonly ImmutableArray<Diagnostic> _diagnostics;

        #endregion // Fields

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="diagnostics">Diagnostics</param>
        public TestFixAllDiagnosticProvider(ImmutableArray<Diagnostic> diagnostics)
        {
            _diagnostics = diagnostics;
        }

        #endregion // Constructor

        #region DiagnosticProvider

        /// <inheritdoc/>
        public override Task<IEnumerable<Diagnostic>> GetDocumentDiagnosticsAsync(Document document, CancellationToken cancellationToken)
        {
            return Task.FromResult<IEnumerable<Diagnostic>>(_diagnostics);
        }

        /// <inheritdoc/>
        public override Task<IEnumerable<Diagnostic>> GetProjectDiagnosticsAsync(Project project, CancellationToken cancellationToken)
        {
            return Task.FromResult<IEnumerable<Diagnostic>>([]);
        }

        /// <inheritdoc/>
        public override Task<IEnumerable<Diagnostic>> GetAllDiagnosticsAsync(Project project, CancellationToken cancellationToken)
        {
            return Task.FromResult<IEnumerable<Diagnostic>>(_diagnostics);
        }

        #endregion // DiagnosticProvider
    }

#pragma warning restore RH2101

    #endregion // Types
}