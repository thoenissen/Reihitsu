using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reihitsu.Formatter.Test;

/// <summary>
/// Base class for formatter tests with normalized string assertions.
/// </summary>
public abstract class FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Asserts that two strings are equal after line-ending normalization.
    /// </summary>
    /// <param name="expected">The expected value.</param>
    /// <param name="actual">The actual value.</param>
    /// <param name="message">The optional assertion message.</param>
    protected static void AssertNormalized(string expected, string actual, string message = "")
    {
        Assert.AreEqual(Normalize(expected), Normalize(actual), message);
    }

    /// <summary>
    /// Normalizes line endings in the specified text to LF.
    /// </summary>
    /// <param name="text">The text to normalize.</param>
    /// <returns>The normalized text.</returns>
    private static string Normalize(string text)
    {
        return text.Replace("\r\n", "\n").Replace("\r", "\n");
    }

    #endregion // Methods
}