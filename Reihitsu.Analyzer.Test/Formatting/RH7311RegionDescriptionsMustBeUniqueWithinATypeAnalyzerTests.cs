using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Organization;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH7311RegionDescriptionsMustBeUniqueWithinATypeAnalyzer"/>
/// </summary>
[TestClass]
public class RH7311RegionDescriptionsMustBeUniqueWithinATypeAnalyzerTests : AnalyzerTestsBase<RH7311RegionDescriptionsMustBeUniqueWithinATypeAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that two regions sharing the same description produce a diagnostic on the second one
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDuplicateRegionDescriptionIsReported()
    {
        const string testData = """
                                public class C
                                {
                                    #region Methods

                                    public void First()
                                    {
                                    }

                                    #endregion // Methods

                                    {|#0:#region Methods|}

                                    public void Second()
                                    {
                                    }

                                    #endregion // Methods
                                }
                                """;

        await Verify(testData, Diagnostics(RH7311RegionDescriptionsMustBeUniqueWithinATypeAnalyzer.DiagnosticId, CreateMessage("Methods")));
    }

    /// <summary>
    /// Verifies that region descriptions are compared case-insensitively
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDuplicateRegionDescriptionIsReportedCaseInsensitively()
    {
        const string testData = """
                                public class C
                                {
                                    #region Methods

                                    public void First()
                                    {
                                    }

                                    #endregion // Methods

                                    {|#0:#region methods|}

                                    public void Second()
                                    {
                                    }

                                    #endregion // methods
                                }
                                """;

        await Verify(testData, Diagnostics(RH7311RegionDescriptionsMustBeUniqueWithinATypeAnalyzer.DiagnosticId, CreateMessage("methods")));
    }

    /// <summary>
    /// Verifies that the third and every further repeat of a description is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyEveryFurtherDuplicateRegionDescriptionIsReported()
    {
        const string testData = """
                                public class C
                                {
                                    #region Methods

                                    public void First()
                                    {
                                    }

                                    #endregion // Methods

                                    {|#0:#region Methods|}

                                    public void Second()
                                    {
                                    }

                                    #endregion // Methods

                                    {|#1:#region Methods|}

                                    public void Third()
                                    {
                                    }

                                    #endregion // Methods
                                }
                                """;

        await Verify(testData, Diagnostics(RH7311RegionDescriptionsMustBeUniqueWithinATypeAnalyzer.DiagnosticId, CreateMessage("Methods"), 2));
    }

    /// <summary>
    /// Verifies that duplicate region descriptions are reported inside an interface declaration
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDuplicateRegionDescriptionIsReportedInInterface()
    {
        const string testData = """
                                public interface IWidget
                                {
                                    #region Methods

                                    void First();

                                    #endregion // Methods

                                    {|#0:#region Methods|}

                                    void Second();

                                    #endregion // Methods
                                }
                                """;

        await Verify(testData, Diagnostics(RH7311RegionDescriptionsMustBeUniqueWithinATypeAnalyzer.DiagnosticId, CreateMessage("Methods")));
    }

    /// <summary>
    /// Verifies that distinct region descriptions do not produce a diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForDistinctRegionDescriptions()
    {
        const string testData = """
                                public class C
                                {
                                    #region Fields

                                    private int _value;

                                    #endregion // Fields

                                    #region Methods

                                    public void First()
                                    {
                                    }

                                    #endregion // Methods
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a single region does not produce a diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForSingleRegion()
    {
        const string testData = """
                                public class C
                                {
                                    #region Methods

                                    public void First()
                                    {
                                    }

                                    #endregion // Methods
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that identical region descriptions in two unrelated types do not produce a diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForDuplicateDescriptionsInDifferentTypes()
    {
        const string testData = """
                                public class First
                                {
                                    #region Methods

                                    public void Method()
                                    {
                                    }

                                    #endregion // Methods
                                }

                                public class Second
                                {
                                    #region Methods

                                    public void Method()
                                    {
                                    }

                                    #endregion // Methods
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a region in a nested type does not collide with the same description in the outer type
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForDuplicateDescriptionInNestedType()
    {
        const string testData = """
                                public class Outer
                                {
                                    #region Methods

                                    public void Method()
                                    {
                                    }

                                    #endregion // Methods

                                    #region Nested

                                    public class Inner
                                    {
                                        #region Methods

                                        public void Method()
                                        {
                                        }

                                        #endregion // Methods
                                    }

                                    #endregion // Nested
                                }
                                """;

        await Verify(testData);
    }

    #endregion // Tests

    #region Methods

    /// <summary>
    /// Creates the expected diagnostic message for the given region description
    /// </summary>
    /// <param name="regionDescription">Region description</param>
    /// <returns>Diagnostic message</returns>
    private static string CreateMessage(string regionDescription)
    {
        return string.Format(AnalyzerResources.RH7311MessageFormat, regionDescription);
    }

    #endregion // Methods
}