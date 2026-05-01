using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Clarity;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Clarity;

/// <summary>
/// Test methods for <see cref="RH0008DoNotUseDefaultValueTypeConstructorAnalyzer"/> and <see cref="RH0008DoNotUseDefaultValueTypeConstructorCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0008DoNotUseDefaultValueTypeConstructorAnalyzerTests : AnalyzerTestsBase<RH0008DoNotUseDefaultValueTypeConstructorAnalyzer, RH0008DoNotUseDefaultValueTypeConstructorCodeFixProvider>
{
    #region Members

    /// <summary>
    /// Verifying default value type constructor with explicit type is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task DefaultValueTypeConstructorIsReportedAndFixed()
    {
        const string testCode = """
                                using System;

                                public class Test
                                {
                                    public Guid Run()
                                    {
                                        return new {|#0:Guid|}();
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 public class Test
                                 {
                                     public Guid Run()
                                     {
                                         return default(Guid);
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0008DoNotUseDefaultValueTypeConstructorAnalyzer.DiagnosticId, "Do not use default value type constructor."));
    }

    /// <summary>
    /// Verifying default int constructor is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task DefaultIntConstructorIsReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    public int Run()
                                    {
                                        return new {|#0:int|}();
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public int Run()
                                     {
                                         return default(int);
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0008DoNotUseDefaultValueTypeConstructorAnalyzer.DiagnosticId, "Do not use default value type constructor."));
    }

    /// <summary>
    /// Verifying implicit object creation for value type is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ImplicitValueTypeCreationIsReportedAndFixed()
    {
        const string testCode = """
                                using System;

                                public class Test
                                {
                                    public Guid Run()
                                    {
                                        Guid value = {|#0:new|}();
                                        return value;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 public class Test
                                 {
                                     public Guid Run()
                                     {
                                         Guid value = default(Guid);
                                         return value;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0008DoNotUseDefaultValueTypeConstructorAnalyzer.DiagnosticId, "Do not use default value type constructor."));
    }

    /// <summary>
    /// Verifying default DateTime constructor is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task DefaultDateTimeConstructorIsReportedAndFixed()
    {
        const string testCode = """
                                using System;

                                public class Test
                                {
                                    public DateTime Run()
                                    {
                                        return new {|#0:DateTime|}();
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 public class Test
                                 {
                                     public DateTime Run()
                                     {
                                         return default(DateTime);
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0008DoNotUseDefaultValueTypeConstructorAnalyzer.DiagnosticId, "Do not use default value type constructor."));
    }

    /// <summary>
    /// Verifying default custom struct constructor is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task DefaultCustomStructConstructorIsReportedAndFixed()
    {
        const string testCode = """
                                public struct Point
                                {
                                    public int X { get; set; }
                                    public int Y { get; set; }
                                }

                                public class Test
                                {
                                    public Point Run()
                                    {
                                        return new {|#0:Point|}();
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public struct Point
                                 {
                                     public int X { get; set; }
                                     public int Y { get; set; }
                                 }

                                 public class Test
                                 {
                                     public Point Run()
                                     {
                                         return default(Point);
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0008DoNotUseDefaultValueTypeConstructorAnalyzer.DiagnosticId, "Do not use default value type constructor."));
    }

    /// <summary>
    /// Verifying value type constructor with arguments is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ValueTypeConstructorWithArgumentsIsNotReported()
    {
        const string testCode = """
                                using System;

                                public class Test
                                {
                                    public DateTime Run()
                                    {
                                        return new DateTime(2024, 1, 1);
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying value type constructor with initializer is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ValueTypeConstructorWithInitializerIsNotReported()
    {
        const string testCode = """
                                public struct Point
                                {
                                    public int X { get; set; }
                                    public int Y { get; set; }
                                }

                                public class Test
                                {
                                    public Point Run()
                                    {
                                        return new Point { X = 1, Y = 2 };
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying reference type constructor is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ReferenceTypeConstructorIsNotReported()
    {
        const string testCode = """
                                public class MyClass
                                {
                                }

                                public class Test
                                {
                                    public MyClass Run()
                                    {
                                        return new MyClass();
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying default value type constructor in field is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task DefaultValueTypeConstructorInFieldIsReportedAndFixed()
    {
        const string testCode = """
                                using System;

                                public class Test
                                {
                                    private Guid _id = new {|#0:Guid|}();
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 public class Test
                                 {
                                     private Guid _id = default(Guid);
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0008DoNotUseDefaultValueTypeConstructorAnalyzer.DiagnosticId, "Do not use default value type constructor."));
    }

    /// <summary>
    /// Verifying default value type constructor in array is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task DefaultValueTypeConstructorInArrayIsReportedAndFixed()
    {
        const string testCode = """
                                using System;

                                public class Test
                                {
                                    public Guid[] Run()
                                    {
                                        return new[] { new {|#0:Guid|}() };
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 public class Test
                                 {
                                     public Guid[] Run()
                                     {
                                         return new[] { default(Guid) };
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0008DoNotUseDefaultValueTypeConstructorAnalyzer.DiagnosticId, "Do not use default value type constructor."));
    }

    /// <summary>
    /// Verifying default value type constructor as method argument is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task DefaultValueTypeConstructorAsArgumentIsReportedAndFixed()
    {
        const string testCode = """
                                using System;

                                public class Test
                                {
                                    public void Method(Guid id)
                                    {
                                    }

                                    public void Run()
                                    {
                                        Method(new {|#0:Guid|}());
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 public class Test
                                 {
                                     public void Method(Guid id)
                                     {
                                     }

                                     public void Run()
                                     {
                                         Method(default(Guid));
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0008DoNotUseDefaultValueTypeConstructorAnalyzer.DiagnosticId, "Do not use default value type constructor."));
    }

    #endregion // Members
}