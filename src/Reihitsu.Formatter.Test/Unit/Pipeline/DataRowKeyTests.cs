using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;
using Reihitsu.Formatter.Test.Helpers.Enumerations;

namespace Reihitsu.Formatter.Test.Unit.Pipeline;

/// <summary>
/// Tests for the text that keys a data-driven test execution in the phase-level stability check: different argument
/// lists get different texts, and an argument the text cannot represent fails with a message that names it
/// </summary>
[TestClass]
public class DataRowKeyTests
{
    #region Methods

    /// <summary>
    /// Verifies that a test without data has no data row
    /// </summary>
    [TestMethod]
    public void RowWithoutArgumentsHasNoKey()
    {
        // Act & Assert
        Assert.IsNull(DataRowKey.Format(null));
        Assert.IsNull(DataRowKey.Format([]));
    }

    /// <summary>
    /// Verifies the token of each supported scalar
    /// </summary>
    [TestMethod]
    public void ScalarsAreWrittenAsTheirTokens()
    {
        // Act
        var formatted = DataRowKey.Format([null, "a", 'b', true, false, 1, 1.5, typeof(int)]);

        // Assert
        Assert.AreEqual("null, \"a\", 'b', true, false, 1, 1.5d, typeof(\"System.Int32\")", formatted);
    }

    /// <summary>
    /// Verifies that arrays with different elements get different keys
    /// </summary>
    [TestMethod]
    public void ArraysWithDifferentElementsGetDifferentKeys()
    {
        // Act
        var first = DataRowKey.Format([new[] { 1 }]);
        var second = DataRowKey.Format([new[] { 2 }]);

        // Assert
        Assert.AreEqual("System.Int32[1]", first);
        Assert.AreEqual("System.Int32[2]", second);
    }

    /// <summary>
    /// Verifies that an array argument and the same values as separate arguments get different keys
    /// </summary>
    [TestMethod]
    public void ArrayAndItsElementsAsArgumentsGetDifferentKeys()
    {
        // Act & Assert
        AssertAllDistinct([new object[] { new[] { 1, 2 } }, [1, 2]]);
    }

    /// <summary>
    /// Verifies that arrays of different element types with equal values get different keys
    /// </summary>
    [TestMethod]
    public void ArraysOfDifferentElementTypesGetDifferentKeys()
    {
        // Act & Assert
        AssertAllDistinct([new object[] { new[] { 1 } }, [new object[] { 1 }], [new[] { 1L }], [new[] { "1" }]]);
    }

    /// <summary>
    /// Verifies that nested arrays are keyed element by element, so a different split of the same values gets a
    /// different key
    /// </summary>
    [TestMethod]
    public void NestedArraysAreKeyedElementByElement()
    {
        // Act
        var first = DataRowKey.Format([new[] { new[] { 1 }, new[] { 2, 3 } }]);
        var second = DataRowKey.Format([new[] { new[] { 1, 2 }, new[] { 3 } }]);

        // Assert
        Assert.AreEqual("System.Int32[][System.Int32[1], System.Int32[2, 3]]", first);
        Assert.AreNotEqual(first, second);
        AssertAllDistinct([[new int[0][]], [new int[0]], [new[] { new int[0] }], [new int[0][][]], [new[] { new int[0][] }]]);
    }

    /// <summary>
    /// Verifies that strings holding quotes, backslashes, and separators get keys different from each other and from the
    /// rows their unescaped text would read as
    /// </summary>
    [TestMethod]
    public void StringsWithQuotesBackslashesAndSeparatorsGetDifferentKeys()
    {
        // Act & Assert
        AssertAllDistinct([
                              ["a\", \"b"],
                              ["a", "b"],
                              ["a\\"],
                              ["a\\\\"],
                              ["a\\\", \"b"],
                              ["a\\", "b"],
                              ["a, b"],
                              ["a", " b"],
                              [new[] { "a", "b" }],
                              [new[] { "a\", \"b" }]
                          ]);
        Assert.AreEqual("\"a\\\", \\\"b\"", DataRowKey.Format(["a\", \"b"]));
    }

    /// <summary>
    /// Verifies that characters holding quotes and backslashes are escaped, and differ from strings of the same text
    /// </summary>
    [TestMethod]
    public void CharactersAreEscapedAndDifferFromStrings()
    {
        // Act & Assert
        AssertAllDistinct([['a'], ["a"], ['\''], ["'"], ['\\'], ["\\"], ['"'], ["\""]]);
        Assert.AreEqual("'\\''", DataRowKey.Format(['\'']));
    }

    /// <summary>
    /// Verifies that the same number of different numeric types gets different keys
    /// </summary>
    [TestMethod]
    public void NumbersOfDifferentTypesGetDifferentKeys()
    {
        // Act & Assert
        AssertAllDistinct([[1], [1u], [1L], [1UL], [(short)1], [(ushort)1], [(byte)1], [(sbyte)1], [1f], [1d], [1m]]);
    }

    /// <summary>
    /// Verifies that numbers are written in invariant culture with every digit a round trip needs, so close values stay
    /// apart
    /// </summary>
    [TestMethod]
    public void NumbersKeepTheirFullPrecision()
    {
        // Act & Assert
        AssertAllDistinct([[0.1], [0.1 + 0.2], [0.3], [1.0m], [1.00m], [-0d], [0d], [double.NaN], [double.PositiveInfinity]]);
        Assert.AreEqual("-1.5d", DataRowKey.Format([-1.5]));
    }

    /// <summary>
    /// Verifies that scalars and strings with the same text get different keys
    /// </summary>
    [TestMethod]
    public void ScalarsAndStringsWithTheSameTextGetDifferentKeys()
    {
        // Act & Assert
        AssertAllDistinct([[null], ["null"], [true], ["true"], [1], ["1"], [typeof(int)], ["System.Int32"], [PhaseReversalLineEndings.Lf], ["Lf"]]);
    }

    /// <summary>
    /// Verifies that enums are keyed by their type and underlying value, so flag combinations hold no separator and
    /// undefined values are kept apart
    /// </summary>
    [TestMethod]
    public void EnumsAreKeyedByTypeAndUnderlyingValue()
    {
        // Act
        var both = DataRowKey.Format([PhaseReversalLineEndings.Both]);

        // Assert
        Assert.AreEqual("Reihitsu.Formatter.Test.Helpers.Enumerations.PhaseReversalLineEndings(3)", both);
        AssertAllDistinct([[PhaseReversalLineEndings.Lf], [PhaseReversalLineEndings.Crlf], [PhaseReversalLineEndings.Both], [(PhaseReversalLineEndings)8], [StringComparison.CurrentCultureIgnoreCase]]);
    }

    /// <summary>
    /// Verifies that types are keyed by their full name and differ from each other
    /// </summary>
    [TestMethod]
    public void TypesAreKeyedByFullName()
    {
        // Act & Assert
        AssertAllDistinct([[typeof(int)], [typeof(long)], [typeof(List<int>)], [typeof(List<long>)], [typeof(int[])]]);
    }

    /// <summary>
    /// Verifies that the arguments of a row are keyed one by one, so moving a value between arguments changes the key
    /// </summary>
    [TestMethod]
    public void RowsWithTheSameValuesInOtherArgumentsGetDifferentKeys()
    {
        // Act & Assert
        AssertAllDistinct([[1, 2], [2, 1], [new[] { 1 }, 2], [1, new[] { 2 }], [null, 1], [1, null]]);
    }

    /// <summary>
    /// Verifies that a value tuple, which a dynamic data source can supply, is refused with a message that names it
    /// </summary>
    [TestMethod]
    public void ValueTupleArgumentIsRefused()
    {
        // Act & Assert
        AssertRefused([(1, 2)], "System.ValueTuple`2", "argument 1");
    }

    /// <summary>
    /// Verifies that an object of a class without a key text is refused with a message that names it
    /// </summary>
    [TestMethod]
    public void ClassObjectArgumentIsRefused()
    {
        // Act & Assert
        AssertRefused([1, new object()], "System.Object", "argument 2");
    }

    /// <summary>
    /// Verifies that a date, a value no data row attribute can hold but a dynamic data source can, is refused
    /// </summary>
    [TestMethod]
    public void DateArgumentIsRefused()
    {
        // Act & Assert
        AssertRefused([DateTime.UnixEpoch], "System.DateTime", "argument 1");
    }

    /// <summary>
    /// Verifies that a multi-dimensional array is refused
    /// </summary>
    [TestMethod]
    public void MultiDimensionalArrayArgumentIsRefused()
    {
        // Act & Assert
        AssertRefused([new int[1, 1]], "System.Int32[,]", "argument 1");
    }

    /// <summary>
    /// Verifies that an array holding an unsupported element is refused, naming the element
    /// </summary>
    [TestMethod]
    public void ArrayWithUnsupportedElementIsRefused()
    {
        // Act & Assert
        AssertRefused([new object[] { 1, Guid.Empty }], "System.Guid", "argument 1, element 2");
    }

    /// <summary>
    /// Verifies that an array whose element type is generic, whose full name holds separators and brackets, is refused
    /// </summary>
    [TestMethod]
    public void ArrayWithGenericElementTypeIsRefused()
    {
        // Act & Assert
        AssertRefused([new List<int>[0]], "System.Collections.Generic.List`1", "argument 1");
    }

    /// <summary>
    /// Asserts that the given rows get pairwise different keys
    /// </summary>
    /// <param name="rows">The rows</param>
    private static void AssertAllDistinct(object[][] rows)
    {
        var keys = rows.Select(DataRowKey.Format).ToList();

        Assert.HasCount(keys.Count, keys.Distinct(), $"Two rows share a key: {string.Join(" | ", keys)}");
    }

    /// <summary>
    /// Asserts that a row is refused with a message that names the argument's type and position
    /// </summary>
    /// <param name="row">The row</param>
    /// <param name="typeName">The start of the refused argument's type name</param>
    /// <param name="position">The position of the refused argument</param>
    private static void AssertRefused(object[] row, string typeName, string position)
    {
        var failure = Assert.Throws<AssertFailedException>(() => DataRowKey.Format(row));

        Assert.Contains("cannot key this data row", failure.Message);
        Assert.Contains($"{position} is a {typeName}", failure.Message);
    }

    #endregion // Methods
}