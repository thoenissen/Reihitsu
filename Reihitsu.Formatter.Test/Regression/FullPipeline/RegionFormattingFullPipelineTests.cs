using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.FullPipeline;

/// <summary>
/// Tests for <see cref="Reihitsu.Formatter.Pipeline.FormattingPipeline"/>
/// </summary>
[TestClass]
public class RegionFormattingFullPipelineTests : FormatterTestsBase
{
    #region Constants

    /// <summary>
    /// Input source used for region-formatting scenarios
    /// </summary>
    private const string TestData = """
                                    internal class RegionFormattingTestData
                                    {
                                        #region fields

                                        private int _value;

                                        #endregion

                                        #region Constructor

                                        public RegionFormattingTestData()
                                        {
                                            _value = 0;
                                        }

                                        #endregion // constructor

                                        #region methods

                                        public int GetValue()
                                        {
                                            return _value;
                                        }

                                        #endregion // Methods
                                    }
                                    """;

    /// <summary>
    /// Expected formatter output for region-formatting scenarios
    /// </summary>
    private const string ResultData = """
                                      internal class RegionFormattingTestData
                                      {
                                          #region Fields

                                          private int _value;

                                          #endregion // Fields

                                          #region Constructor

                                          public RegionFormattingTestData()
                                          {
                                              _value = 0;
                                          }

                                          #endregion // Constructor

                                          #region Methods

                                          public int GetValue()
                                          {
                                              return _value;
                                          }

                                          #endregion // Methods
                                      }
                                      """;

    #endregion // Constants

    #region Methods

    /// <summary>
    /// Verifies that region directives are formatted correctly under both LF and CRLF line endings
    /// </summary>
    [TestMethod]
    public void FormatsRegionDirectives()
    {
        AssertRuleResult(TestData, ResultData);
    }

    /// <summary>
    /// Verifies that region directives inside a property accessor list are preserved
    /// </summary>
    [TestMethod]
    public void RegionInsidePropertyAccessorListIsPreserved()
    {
        // Arrange
        const string input = """
                             internal class T
                             {
                                 public int V
                                 {
                                     #region R
                                     get;
                                     #endregion
                                     set;
                                 }
                             }
                             """;
        const string expected = """
                                internal class T
                                {
                                    public int V
                                    {
                                        #region R

                                        get;

                                        #endregion // R

                                        set;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that region directives inside an indexer accessor list are preserved
    /// </summary>
    [TestMethod]
    public void RegionInsideIndexerAccessorListIsPreserved()
    {
        // Arrange
        const string input = """
                             internal class T
                             {
                                 public int this[int index]
                                 {
                                     #region R
                                     get;
                                     #endregion
                                     set;
                                 }
                             }
                             """;
        const string expected = """
                                internal class T
                                {
                                    public int this[int index]
                                    {
                                        #region R

                                        get;

                                        #endregion // R

                                        set;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that region directives inside a custom-event accessor list are preserved
    /// </summary>
    [TestMethod]
    public void RegionInsideCustomEventAccessorListIsPreserved()
    {
        // Arrange
        const string input = """
                             internal class T
                             {
                                 public event System.Action Changed
                                 {
                                     #region R
                                     add
                                     {
                                     }
                                     #endregion
                                     remove
                                     {
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                internal class T
                                {
                                    public event System.Action Changed
                                    {
                                        #region R

                                        add
                                        {
                                        }

                                        #endregion // R

                                        remove
                                        {
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that naming still formats a preserved accessor-list region pair
    /// </summary>
    [TestMethod]
    public void RegionInsideAccessorListStillUsesStandardNaming()
    {
        // Arrange
        const string input = """
                             internal class T
                             {
                                 public int V
                                 {
                                     #region lower
                                     get;
                                     #endregion // Wrong
                                     set;
                                 }
                             }
                             """;
        const string expected = """
                                internal class T
                                {
                                    public int V
                                    {
                                        #region Lower

                                        get;

                                        #endregion // Lower

                                        set;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that nested region pairs directly inside an accessor list are preserved and formatted
    /// </summary>
    [TestMethod]
    public void NestedRegionsInsideAccessorListArePreservedAndFormatted()
    {
        // Arrange
        const string input = """
                             internal class T
                             {
                                 public int V
                                 {
                                     #region outer
                                     #region inner
                                     get;
                                     #endregion
                                     #endregion
                                     set;
                                 }
                             }
                             """;
        const string expected = """
                                internal class T
                                {
                                    public int V
                                    {
                                        #region Outer

                                        #region Inner

                                        get;

                                        #endregion // Inner

                                        #endregion // Outer

                                        set;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an unnamed region pair directly inside an accessor list is preserved
    /// </summary>
    [TestMethod]
    public void UnnamedRegionInsideAccessorListIsPreserved()
    {
        // Arrange
        const string input = """
                             internal class T
                             {
                                 public int V
                                 {
                                     #region
                                     get;
                                     #endregion
                                     set;
                                 }
                             }
                             """;
        const string expected = """
                                internal class T
                                {
                                    public int V
                                    {
                                        #region

                                        get;

                                        #endregion

                                        set;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an inactive region inside an accessor list retains the existing naming behavior
    /// </summary>
    [TestMethod]
    public void InactiveRegionInsideAccessorListIsStillNamed()
    {
        // Arrange
        const string input = """
                             internal class T
                             {
                                 public int V
                                 {
                             #if false
                             #region lower
                             #endregion
                             #endif
                                     get;
                                     set;
                                 }
                             }
                             """;
        const string expected = """
                                internal class T
                                {
                                    public int V
                                    {
                                #if false
                                #region Lower
                                #endregion // Lower
                                #endif
                                        get;
                                        set;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a region inside an accessor body is still removed
    /// </summary>
    [TestMethod]
    public void RegionInsideAccessorBodyIsRemoved()
    {
        // Arrange
        const string input = """
                             internal class T
                             {
                                 public int V
                                 {
                                     get
                                     {
                                         #region Inner
                                         return 1;
                                         #endregion
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                internal class T
                                {
                                    public int V
                                    {
                                        get
                                        {
                                            return 1;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Methods
}