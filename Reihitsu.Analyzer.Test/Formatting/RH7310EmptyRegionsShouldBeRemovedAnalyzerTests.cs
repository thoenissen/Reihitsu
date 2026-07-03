using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Organization;
using Reihitsu.Analyzer.Rules.Organization;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH7310EmptyRegionsShouldBeRemovedAnalyzer"/> and <see cref="RH7310EmptyRegionsShouldBeRemovedCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH7310EmptyRegionsShouldBeRemovedAnalyzerTests : AnalyzerTestsBase<RH7310EmptyRegionsShouldBeRemovedAnalyzer, RH7310EmptyRegionsShouldBeRemovedCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that a region containing only blank lines is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyEmptyRegionIsReported()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    {|#0:#region Methods|}

                                    #endregion // Methods
                                }
                                """;

        await Verify(testData, Diagnostics(RH7310EmptyRegionsShouldBeRemovedAnalyzer.DiagnosticId, AnalyzerResources.RH7310MessageFormat));
    }

    /// <summary>
    /// Verifies that a region containing only a comment is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyRegionContainingOnlyCommentIsReported()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    {|#0:#region Properties|}

                                    // TODO: add properties later

                                    #endregion // Properties
                                }
                                """;

        await Verify(testData, Diagnostics(RH7310EmptyRegionsShouldBeRemovedAnalyzer.DiagnosticId, AnalyzerResources.RH7310MessageFormat));
    }

    /// <summary>
    /// Verifies that two adjacent empty regions are both reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMultipleEmptyRegionsAreReported()
    {
        const string testData = """
                                public class Widget
                                {
                                    {|#0:#region Methods|}

                                    #endregion // Methods

                                    {|#1:#region Properties|}

                                    // TODO: add properties later

                                    #endregion // Properties
                                }
                                """;

        await Verify(testData, Diagnostics(RH7310EmptyRegionsShouldBeRemovedAnalyzer.DiagnosticId, AnalyzerResources.RH7310MessageFormat, 2));
    }

    /// <summary>
    /// Verifies that only the innermost region is reported when an empty region nests another empty region
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNestedEmptyRegionReportsInnermostOnly()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    #region Outer

                                    {|#0:#region Inner|}

                                    #endregion // Inner

                                    #endregion // Outer
                                }
                                """;

        await Verify(testData, Diagnostics(RH7310EmptyRegionsShouldBeRemovedAnalyzer.DiagnosticId, AnalyzerResources.RH7310MessageFormat));
    }

    /// <summary>
    /// Verifies that a region containing a field declaration is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyRegionContainingFieldIsNotReported()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    #region Fields

                                    private int _value;

                                    #endregion // Fields
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a region containing a method declaration is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyRegionContainingMethodIsNotReported()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    #region Methods

                                    public void Run()
                                    {
                                    }

                                    #endregion // Methods
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a region containing a nested type declaration is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyRegionContainingNestedTypeIsNotReported()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    #region Types

                                    private class Nested
                                    {
                                    }

                                    #endregion // Types
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a region wrapping a non-empty nested region is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyRegionWrappingNonEmptyNestedRegionIsNotReported()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    #region Outer

                                    #region Inner

                                    private int _value;

                                    #endregion // Inner

                                    #endregion // Outer
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a region wrapping only conditionally-excluded code is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyRegionWrappingDisabledCodeIsNotReported()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    #region Methods
                                #if false
                                    public void Run()
                                    {
                                    }
                                #endif
                                    #endregion // Methods
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a region wrapping only a non-region directive is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyRegionWrappingDirectiveIsNotReported()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    #region Nullable
                                #nullable enable
                                    #endregion // Nullable
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that an empty region inside a method body is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyEmptyRegionInsideMethodBodyIsNotReported()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    private void Run()
                                    {
                                        #region Body

                                        #endregion // Body
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that an empty region is removed by the code fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyEmptyRegionIsRemoved()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    {|#0:#region Methods|}

                                    #endregion // Methods
                                }
                                """;

        const string resultData = """
                                  internal class TestClass;
                                  """;

        await Verify(testData, resultData, Diagnostics(RH7310EmptyRegionsShouldBeRemovedAnalyzer.DiagnosticId, AnalyzerResources.RH7310MessageFormat));
    }

    /// <summary>
    /// Verifies that a region containing only a comment is removed by the code fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyRegionContainingOnlyCommentIsRemoved()
    {
        const string testData = """
                                public class Widget
                                {
                                    {|#0:#region Methods|}

                                    #endregion // Methods

                                    {|#1:#region Properties|}

                                    // TODO: add properties later

                                    #endregion // Properties
                                }
                                """;

        const string resultData = """
                                  public class Widget;
                                  """;

        await Verify(testData,
                     resultData,
                     onConfigure: config => config.NumberOfFixAllIterations = 2,
                     Diagnostics(RH7310EmptyRegionsShouldBeRemovedAnalyzer.DiagnosticId, AnalyzerResources.RH7310MessageFormat, 2));
    }

    /// <summary>
    /// Verifies that only the empty region is removed while populated regions are preserved
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyPopulatedRegionsArePreservedWhenRemovingEmptyRegion()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    #region Fields

                                    private int _value;

                                    #endregion // Fields

                                    {|#0:#region Methods|}

                                    #endregion // Methods

                                    #region Properties

                                    public int Value => _value;

                                    #endregion // Properties
                                }
                                """;

        const string resultData = """
                                  internal class TestClass
                                  {
                                      #region Fields

                                      private int _value;

                                      #endregion // Fields

                                      #region Properties

                                      public int Value => _value;

                                      #endregion // Properties
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH7310EmptyRegionsShouldBeRemovedAnalyzer.DiagnosticId, AnalyzerResources.RH7310MessageFormat));
    }

    /// <summary>
    /// Verifies that nested empty regions collapse into an empty type after repeated application
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNestedEmptyRegionsAreRemoved()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    #region Outer

                                    {|#0:#region Inner|}

                                    #endregion // Inner

                                    #endregion // Outer
                                }
                                """;

        const string resultData = """
                                  internal class TestClass;
                                  """;

        await Verify(testData,
                     resultData,
                     onConfigure: config =>
                                  {
                                      config.NumberOfIncrementalIterations = 2;
                                      config.NumberOfFixAllIterations = 2;
                                  },
                     Diagnostics(RH7310EmptyRegionsShouldBeRemovedAnalyzer.DiagnosticId, AnalyzerResources.RH7310MessageFormat));
    }

    #endregion // Tests
}