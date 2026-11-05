using System.IO;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.SelfHosting.Test.Utilities;

namespace Reihitsu.SelfHosting.Test.Repository;

/// <summary>
/// Guards the default branch against temporary working docs: <c>sdd/work/</c> holds a squad run's handovers on its
/// pull request branch only and is deleted before the pull request is ready. The check lives in the self-hosting
/// project because that project runs in CI and not in squad sessions, where the folder legitimately exists
/// </summary>
[TestClass]
public sealed class WorkingDocumentsTests
{
    #region Tests

    /// <summary>
    /// Verifies that no file remains under <c>sdd/work/</c>
    /// </summary>
    [TestMethod]
    public void WorkingDocumentsDirectoryContainsNoFiles()
    {
        // Arrange
        var workDirectory = Path.Combine(RepositoryRoot.Find(), "sdd", "work");

        // Act
        var files = Directory.Exists(workDirectory)
                        ? Directory.EnumerateFiles(workDirectory, "*", SearchOption.AllDirectories)
                                   .Select(obj => Path.GetRelativePath(workDirectory, obj))
                                   .OrderBy(obj => obj)
                                   .ToArray()
                        : [];

        // Assert
        Assert.IsEmpty(files, $"sdd/work/ must be deleted before a pull request is ready, but contains: {string.Join(", ", files)}");
    }

    #endregion // Tests
}