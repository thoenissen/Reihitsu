using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reihitsu.Core.Test;

/// <summary>
/// Contains unit tests for <see cref="OrderingMoveSafety"/>
/// </summary>
[TestClass]
public class OrderingMoveSafetyTests
{
    #region Tests

    /// <summary>
    /// Verifies that an unusable root refuses the move instead of reporting no position sensitive directives
    /// </summary>
    [TestMethod]
    public void ContainsPositionSensitiveDirectivesThatBlockRelocationReturnsTrueForMissingRoot()
    {
        Assert.IsTrue(OrderingMoveSafety.ContainsPositionSensitiveDirectivesThatBlockRelocation(null, TextSpan.FromBounds(0, 1), [], 0));
    }

    #endregion // Tests
}