using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Indentation;

/// <summary>
/// Tests for <see cref="Reihitsu.Formatter.Pipeline.FormattingPipeline"/> — dictionary indexer key alignment.
/// Regression coverage for issue #313 (multi-line dictionary indexer key mis-indented inside brackets)
/// </summary>
[TestClass]
public class DictionaryIndexerKeyAlignmentTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies the issue #313 reproduction: a multi-line indexer key with a data-class
    /// object initializer keeps the key body indented one level deeper than the opening
    /// bracket and the closing bracket aligned with the opening bracket
    /// </summary>
    [TestMethod]
    public void MultiLineDataClassKeyWithObjectInitializerAligns()
    {
        // Arrange
        const string input = """
                             public class Data
                             {
                                 public int A { get; set; }

                                 public int B { get; set; }
                             }

                             public class C
                             {
                                 public void M()
                                 {
                                     Dictionary<Data, string> v;

                                     v = new Dictionary<Data, string>()
                                         {
                                             [
                                              new Data
                                              {
                                                  A = 1,
                                                  B = 2
                                              }
                                              ] = "123"
                                         };
                                 }
                             }
                             """;

        const string expected = """
                                public class Data
                                {
                                    public int A { get; set; }

                                    public int B { get; set; }
                                }

                                public class C
                                {
                                    public void M()
                                    {
                                        Dictionary<Data, string> v;

                                        v = new Dictionary<Data, string>()
                                            {
                                                [
                                                    new Data
                                                    {
                                                        A = 1,
                                                        B = 2
                                                    }
                                                ] = "123"
                                            };
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an indexer key whose value is constructed via a multi-line constructor
    /// invocation places the constructor body one level deeper than the opening bracket
    /// and aligns the closing bracket with the opening bracket
    /// </summary>
    [TestMethod]
    public void MultiLineDataClassKeyWithMultilineConstructorAligns()
    {
        // Arrange
        const string input = """
                             public class Data
                             {
                                 public Data(int a, int b)
                                 {
                                 }
                             }

                             public class C
                             {
                                 public void M()
                                 {
                                     Dictionary<Data, string> v;

                                     v = new Dictionary<Data, string>()
                                         {
                                             [
                                              new Data(
                                                       1,
                                                       2)
                                              ] = "123"
                                         };
                                 }
                             }
                             """;

        const string expected = """
                                public class Data
                                {
                                    public Data(int a, int b)
                                    {
                                    }
                                }

                                public class C
                                {
                                    public void M()
                                    {
                                        Dictionary<Data, string> v;

                                        v = new Dictionary<Data, string>()
                                            {
                                                [
                                                    new Data(1,
                                                             2)
                                                ] = "123"
                                            };
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a data-class indexer key without an initializer or arguments stays
    /// on a single line and is not broken into a multi-line layout
    /// </summary>
    [TestMethod]
    public void DataClassKeyWithoutInitializerStaysSingleLine()
    {
        // Arrange
        const string input = """
                             public class Data
                             {
                                 public int A { get; set; }
                             }

                             public class C
                             {
                                 public void M()
                                 {
                                     Dictionary<Data, string> v;

                                     v = new Dictionary<Data, string>()
                                         {
                                             [new Data()] = "123"
                                         };
                                 }
                             }
                             """;

        const string expected = input;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a native string indexer key stays on a single line
    /// </summary>
    [TestMethod]
    public void NativeStringKeyStaysSingleLine()
    {
        // Arrange
        const string input = """
                             public class C
                             {
                                 public void M()
                                 {
                                     Dictionary<string, int> v;

                                     v = new Dictionary<string, int>()
                                         {
                                             ["abc"] = 1
                                         };
                                 }
                             }
                             """;

        const string expected = input;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a collection initializer containing one single-line indexer key
    /// and one multi-line indexer key aligns the multi-line entry correctly while
    /// leaving the single-line entry untouched
    /// </summary>
    [TestMethod]
    public void MixedSingleLineAndMultiLineKeysAlign()
    {
        // Arrange
        const string input = """
                             public class Data
                             {
                                 public int A { get; set; }
                             }

                             public class C
                             {
                                 public void M()
                                 {
                                     Dictionary<object, string> v;

                                     v = new Dictionary<object, string>()
                                         {
                                             [1] = "single",
                                             [
                                              new Data
                                              {
                                                  A = 1
                                              }
                                              ] = "multi"
                                         };
                                 }
                             }
                             """;

        const string expected = """
                                public class Data
                                {
                                    public int A { get; set; }
                                }

                                public class C
                                {
                                    public void M()
                                    {
                                        Dictionary<object, string> v;

                                        v = new Dictionary<object, string>()
                                            {
                                                [1] = "single",
                                                [
                                                    new Data
                                                    {
                                                        A = 1
                                                    }
                                                ] = "multi"
                                            };
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that several entries with interleaved single-line and multi-line indexer keys
    /// keep each entry independently aligned
    /// </summary>
    [TestMethod]
    public void MultipleSingleLineAndMultiLineKeysInterleavedAlign()
    {
        // Arrange
        const string input = """
                             public class Data
                             {
                                 public int A { get; set; }
                             }

                             public class C
                             {
                                 public void M()
                                 {
                                     Dictionary<object, string> v;

                                     v = new Dictionary<object, string>()
                                         {
                                             ["a"] = "1",
                                             [
                                              new Data
                                              {
                                                  A = 1
                                              }
                                              ] = "2",
                                             ["b"] = "3",
                                             [
                                              new Data
                                              {
                                                  A = 2
                                              }
                                              ] = "4"
                                         };
                                 }
                             }
                             """;

        const string expected = """
                                public class Data
                                {
                                    public int A { get; set; }
                                }

                                public class C
                                {
                                    public void M()
                                    {
                                        Dictionary<object, string> v;

                                        v = new Dictionary<object, string>()
                                            {
                                                ["a"] = "1",
                                                [
                                                    new Data
                                                    {
                                                        A = 1
                                                    }
                                                ] = "2",
                                                ["b"] = "3",
                                                [
                                                    new Data
                                                    {
                                                        A = 2
                                                    }
                                                ] = "4"
                                            };
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a native int indexer key stays on a single line
    /// </summary>
    [TestMethod]
    public void NativeIntKeyStaysSingleLine()
    {
        // Arrange
        const string input = """
                             public class C
                             {
                                 public void M()
                                 {
                                     Dictionary<int, string> v;

                                     v = new Dictionary<int, string>()
                                         {
                                             [42] = "123"
                                         };
                                 }
                             }
                             """;

        const string expected = input;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Methods
}