using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Naming;
using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Naming;

/// <summary>
/// Test methods for <see cref="RH4002ClassNameCasingAnalyzer"/> and <see cref="RH4002ClassNameCasingCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH4002ClassNameCasingAnalyzerTests : BatchCodeFixTestsBase<RH4002ClassNameCasingAnalyzer, RH4002ClassNameCasingCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4002-001")]
    [Requirement("RH4002-002")]
    public async Task VerifyDiagnostics()
    {
        const string testCode = """
                                using System;
                                using System.Collections.Generic;
                                using System.Linq;
                                using System.Text;
                                using System.Threading.Tasks;

                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    /// <summary>
                                    /// Test class
                                    /// </summary>
                                    public class {|#0:testClass|}
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;
                                 using System.Collections.Generic;
                                 using System.Linq;
                                 using System.Text;
                                 using System.Threading.Tasks;

                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     /// <summary>
                                     /// Test class
                                     /// </summary>
                                     public class TestClass
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH4002ClassNameCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4002MessageFormat));
    }

    /// <summary>
    /// Verifying no diagnostics for a PascalCase class name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4002-001")]
    public async Task VerifyNoDiagnosticsForPascalCaseClass()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class OrderProcessor
                                    {
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying diagnostics for an abstract class with wrong casing
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4002-001")]
    [Requirement("RH4002-002")]
    public async Task VerifyDiagnosticsForAbstractClassWrongCasing()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public abstract class {|#0:abstractService|}
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     public abstract class AbstractService
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH4002ClassNameCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4002MessageFormat));
    }

    /// <summary>
    /// Verifying diagnostics for a static class with wrong casing
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4002-001")]
    [Requirement("RH4002-002")]
    public async Task VerifyDiagnosticsForStaticClassWrongCasing()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public static class {|#0:helperUtils|}
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     public static class HelperUtils
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH4002ClassNameCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4002MessageFormat));
    }

    /// <summary>
    /// Verifying diagnostics for a sealed class with wrong casing
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4002-001")]
    [Requirement("RH4002-002")]
    public async Task VerifyDiagnosticsForSealedClassWrongCasing()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public sealed class {|#0:sealedImpl|}
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     public sealed class SealedImpl
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH4002ClassNameCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4002MessageFormat));
    }

    /// <summary>
    /// Verifying diagnostics for a nested class with wrong casing
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4002-001")]
    [Requirement("RH4002-002")]
    public async Task VerifyDiagnosticsForNestedClassWrongCasing()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class OuterClass
                                    {
                                        public class {|#0:nestedHelper|}
                                        {
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     public class OuterClass
                                     {
                                         public class NestedHelper
                                         {
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH4002ClassNameCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4002MessageFormat));
    }

    /// <summary>
    /// Verifying no diagnostics for a PascalCase generic class
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4002-001")]
    public async Task VerifyNoDiagnosticsForGenericClass()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class Container<T>
                                    {
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying diagnostics for a generic class with wrong casing
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4002-001")]
    [Requirement("RH4002-002")]
    public async Task VerifyDiagnosticsForGenericClassWrongCasing()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class {|#0:container|}<T>
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     public class Container<T>
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH4002ClassNameCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4002MessageFormat));
    }

    /// <summary>
    /// Verifying diagnostics for an internal class with wrong casing
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4002-001")]
    [Requirement("RH4002-002")]
    public async Task VerifyDiagnosticsForInternalClassWrongCasing()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    internal class {|#0:internalHelper|}
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     internal class InternalHelper
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH4002ClassNameCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4002MessageFormat));
    }

    /// <summary>
    /// Verifying no diagnostics for an internal PascalCase class
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4002-001")]
    public async Task VerifyNoDiagnosticsForInternalPascalCaseClass()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    internal class InternalHelper
                                    {
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that PascalCase accepts an uppercase start followed by letters and digits, an acronym included, and rejects a lowercase start, a separator, and a leading underscore
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-001")]
    [Requirement("RH4002-001")]
    public async Task PascalCaseAcceptsLettersAndDigitsAndRejectsLowercaseStartsAndSeparators()
    {
        const string testCode = """
                                class Order2
                                {
                                }

                                class HTTPClient
                                {
                                }

                                class {|RH4002:order|}
                                {
                                }

                                class {|RH4002:Order_Line|}
                                {
                                }

                                class {|RH4002:_Invoice|}
                                {
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that nested, static, generic, and internal classes are checked and reported on their identifier
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4002-001")]
    public async Task ClassesAreCheckedWhateverTheirModifiersTypeParametersOrNesting()
    {
        const string testCode = """
                                public class OrderProcessor
                                {
                                    public class {|RH4002:nestedHelper|}
                                    {
                                    }
                                }

                                public static class {|RH4002:helperUtils|}
                                {
                                }

                                public class {|RH4002:container|}<T>
                                {
                                }

                                internal class InternalHelper
                                {
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that the fix renames each class to PascalCase together with its uses as a field type and in an object creation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4002-002")]
    public async Task FixRenamesClassAndEveryReference()
    {
        const string testCode = """
                                public class {|RH4002:orderProcessor|}
                                {
                                    private readonly paymentGateway _gateway = new paymentGateway();

                                    public int Total => _gateway.Amount;
                                }

                                public class {|RH4002:paymentGateway|}
                                {
                                    public int Amount { get; set; }
                                }
                                """;

        const string fixedCode = """
                                 public class OrderProcessor
                                 {
                                     private readonly PaymentGateway _gateway = new PaymentGateway();

                                     public int Total => _gateway.Amount;
                                 }

                                 public class PaymentGateway
                                 {
                                     public int Amount { get; set; }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that the class is still reported but no fix is offered when another file declares a class of the new name in the same namespace
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-009")]
    public async Task NoFixIsOfferedWhenAnotherFileOfTheNamespaceDeclaresTheNewName()
    {
        const string testCode = """
                                namespace N
                                {
                                    class {|RH4002:order|}
                                    {
                                    }
                                }
                                """;

        const string otherFileCode = """
                                     namespace N
                                     {
                                         class Order
                                         {
                                         }
                                     }
                                     """;

        await Verify(testCode,
                     testCode,
                     test =>
                     {
                         test.TestState.Sources.Add(("/0/Test1.cs", otherFileCode));
                         test.FixedState.Sources.Add(("/0/Test1.cs", otherFileCode));
                     });
    }

    /// <summary>
    /// Verifies that a file-local type of the new name in another file does not collide, so the class is renamed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-009")]
    public async Task FixIsOfferedWhenAnotherFileDeclaresFileLocalTypeOfTheNewName()
    {
        const string testCode = """
                                namespace N
                                {
                                    class {|RH4002:order|}
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace N
                                 {
                                     class Order
                                     {
                                     }
                                 }
                                 """;

        const string otherFileCode = """
                                     namespace N
                                     {
                                         file class Order
                                         {
                                         }
                                     }
                                     """;

        await Verify(testCode,
                     fixedCode,
                     test =>
                     {
                         test.TestState.Sources.Add(("/0/Test1.cs", otherFileCode));
                         test.FixedState.Sources.Add(("/0/Test1.cs", otherFileCode));
                     });
    }

    /// <summary>
    /// Verifies that a type of the new name with another number of type parameters does not collide, so the generic class is renamed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-009")]
    public async Task FixIsOfferedWhenTheTypeOfTheNewNameHasAnotherNumberOfTypeParameters()
    {
        const string testCode = """
                                namespace N
                                {
                                    class {|RH4002:order|}<T>
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace N
                                 {
                                     class Order<T>
                                     {
                                     }
                                 }
                                 """;

        const string otherFileCode = """
                                     namespace N
                                     {
                                         class Order
                                         {
                                         }
                                     }
                                     """;

        await Verify(testCode,
                     fixedCode,
                     test =>
                     {
                         test.TestState.Sources.Add(("/0/Test1.cs", otherFileCode));
                         test.FixedState.Sources.Add(("/0/Test1.cs", otherFileCode));
                     });
    }

    /// <summary>
    /// Verifies that a file-local type of the new name in the same file collides, so the class is still reported but no fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-009")]
    public async Task NoFixIsOfferedWhenFileLocalTypeOfTheNewNameIsInTheSameFile()
    {
        const string testCode = """
                                namespace N
                                {
                                    class {|RH4002:order|}
                                    {
                                    }

                                    file class Order
                                    {
                                    }
                                }
                                """;

        await Verify(testCode, testCode);
    }

    /// <summary>
    /// Verifies that a nested class is still reported but no fix is offered when the new name would equal the name of one of its members, which adds a compile error inside the containing type
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-009")]
    public async Task NoFixIsOfferedWhenRenamingANestedClassAddsACompileErrorInItsContainingType()
    {
        const string testCode = """
                                class Outer
                                {
                                    class {|RH4002:order|}
                                    {
                                        public int Order;
                                    }
                                }
                                """;

        await Verify(testCode, testCode);
    }

    /// <summary>
    /// Verifies that a class declared directly in a namespace keeps its diagnostic but gets no fix when its new name is the name of one of its fields
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-009")]
    [Requirement("SHARED-FIX-003")]
    public async Task NoFixIsOfferedWhenANamespaceClassWouldHaveAFieldOfItsName()
    {
        const string testCode = """
                                namespace N
                                {
                                    class {|RH4002:order|}
                                    {
                                        public int Order;
                                    }
                                }
                                """;

        await Verify(testCode, testCode);
    }

    /// <summary>
    /// Verifies that a class declared in a file-scoped namespace keeps its diagnostic but gets no fix when its new name is the name of one of its methods
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-009")]
    [Requirement("SHARED-FIX-003")]
    public async Task NoFixIsOfferedWhenAFileScopedNamespaceClassWouldHaveAMethodOfItsName()
    {
        const string testCode = """
                                namespace N;

                                class {|RH4002:order|}
                                {
                                    public void Order()
                                    {
                                    }
                                }
                                """;

        await Verify(testCode, testCode);
    }

    /// <summary>
    /// Verifies that a class declared at file level keeps its diagnostic but gets no fix when its new name is the name of one of its nested types
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-009")]
    [Requirement("SHARED-FIX-003")]
    public async Task NoFixIsOfferedWhenAFileLevelClassWouldHaveANestedTypeOfItsName()
    {
        const string testCode = """
                                class {|RH4002:order|}
                                {
                                    class Order
                                    {
                                    }
                                }
                                """;

        await Verify(testCode, testCode);
    }

    /// <summary>
    /// Verifies that a class declared directly in a namespace keeps its diagnostic but gets no fix when its new name is the name of one of its type parameters
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-009")]
    [Requirement("SHARED-FIX-003")]
    public async Task NoFixIsOfferedWhenANamespaceClassWouldHaveATypeParameterOfItsName()
    {
        const string testCode = """
                                namespace N
                                {
                                    class {|RH4002:order|}<Order>
                                    {
                                    }
                                }
                                """;

        await Verify(testCode, testCode);
    }

    /// <summary>
    /// Verifies that a class is renamed when a reference to it would bind to a nested type of the new name, because the renamer qualifies that reference so it keeps referring to the class
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-009")]
    public async Task FixQualifiesAReferenceThatANestedTypeOfTheNewNameWouldCapture()
    {
        const string testCode = """
                                namespace N
                                {
                                    class X
                                    {
                                        class Order
                                        {
                                        }

                                        order _value;
                                    }

                                    class {|RH4002:order|}
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace N
                                 {
                                     class X
                                     {
                                         class Order
                                         {
                                         }

                                         N.Order _value;
                                     }

                                     class Order
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that a class keeps its diagnostic but gets no fix when code in an excluded <c>#if</c> branch references it, because the rename would leave that reference behind
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-007")]
    [Requirement("SHARED-FIX-003")]
    public async Task NoFixIsOfferedWhenExcludedCodeReferencesTheClass()
    {
        const string testCode = """
                                public class C
                                {
                                #if NEVER
                                    public order Create() => new order();
                                #endif

                                    public order Get() => new order();
                                }

                                public class {|RH4002:order|}
                                {
                                }
                                """;

        await Verify(testCode, testCode);
    }

    /// <summary>
    /// Verifies that a partial class keeps its diagnostic but gets no fix when another part, declared in another file, has a member of its new name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-009")]
    [Requirement("SHARED-FIX-003")]
    public async Task NoFixIsOfferedWhenAnotherPartOfAPartialClassHasAMemberOfTheNewName()
    {
        const string testCode = """
                                namespace N
                                {
                                    partial class {|RH4002:order|}
                                    {
                                    }
                                }
                                """;

        const string otherFileCode = """
                                     namespace N
                                     {
                                         partial class {|RH4002:order|}
                                         {
                                             public int Order;
                                         }
                                     }
                                     """;

        await Verify(testCode,
                     testCode,
                     test =>
                     {
                         test.TestState.Sources.Add(("/0/Test1.cs", otherFileCode));
                         test.FixedState.Sources.Add(("/0/Test1.cs", otherFileCode));
                     });
    }

    /// <summary>
    /// Verifies that a class keeps its diagnostic but gets no fix when code in an excluded <c>#if</c> branch of another file references it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-007")]
    [Requirement("SHARED-FIX-003")]
    public async Task NoFixIsOfferedWhenExcludedCodeOfAnotherFileReferencesTheClass()
    {
        const string testCode = """
                                public class {|RH4002:order|}
                                {
                                }
                                """;

        const string otherFileCode = """
                                     public class C
                                     {
                                     #if NEVER
                                         public order Create() => new order();
                                     #endif
                                     }
                                     """;

        await Verify(testCode,
                     testCode,
                     test =>
                     {
                         test.TestState.Sources.Add(("/0/Test1.cs", otherFileCode));
                         test.FixedState.Sources.Add(("/0/Test1.cs", otherFileCode));
                     });
    }

    /// <summary>
    /// Verifies that a class declared directly in a namespace is renamed when only an inherited member has its new name, because a member that the class does not declare may have its name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-009")]
    public async Task FixIsOfferedWhenOnlyAnInheritedMemberHasTheNewName()
    {
        const string testCode = """
                                namespace N
                                {
                                    class Base
                                    {
                                        public int Order;
                                    }

                                    class {|RH4002:order|} : Base
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace N
                                 {
                                     class Base
                                     {
                                         public int Order;
                                     }

                                     class Order : Base
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class {|#0:orderProcessor|}
                                    {
                                        private readonly paymentGateway _gateway = new paymentGateway();

                                        public int Total => _gateway.Amount;
                                    }

                                    public class {|#1:paymentGateway|}
                                    {
                                        public int Amount { get; set; }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     public class OrderProcessor
                                     {
                                         private readonly PaymentGateway _gateway = new PaymentGateway();

                                         public int Total => _gateway.Amount;
                                     }

                                     public class PaymentGateway
                                     {
                                         public int Amount { get; set; }
                                     }
                                 }
                                 """;

        // One reported class is used as the field type of the other, so the batch has to rewrite the
        // reference together with both declarations
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH4002ClassNameCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4002MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}