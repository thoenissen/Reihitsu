using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.LineBreaks;

/// <summary>
/// A single-line auto-property accessor list must stay on the property declaration line even
/// when the source uses CRLF line endings and enough content precedes the property
/// </summary>
[TestClass]
public class AutoPropertyAccessorListCrlfTests : FormatterTestsBase
{
    #region Constants

    /// <summary>
    /// Source whose auto-property is preceded by a long using directive and file-scoped namespace
    /// </summary>
    private const string TestData = """
                                    using System.ComponentModel.DataAnnotations.Schema;

                                    namespace Reihitsu.Sample.Entities.Tables.GameData.Items;

                                    public class C
                                    {
                                        #region Properties

                                        /// <summary>
                                        /// Id of the item
                                        /// </summary>
                                        public int ItemId { get; set; }

                                        #endregion // Properties
                                    }
                                    """;

    #endregion // Constants

    #region Tests

    /// <summary>
    /// Verifies that an auto-property preceded by a long using directive and file-scoped namespace keeps its accessor
    /// list on the property line, under CRLF as well as LF line endings
    /// </summary>
    [TestMethod]
    public void AutoPropertyAccessorListStaysOnPropertyLineWithCrlf()
    {
        AssertRuleResult(TestData);
    }

    #endregion // Tests
}