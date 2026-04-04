using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reihitsu.Cli.Test.Unit;

/// <summary>
/// Tests for <see cref="Program.ParseArguments"/>.
/// </summary>
[TestClass]
public class ProgramTests
{
    #region Methods

    /// <summary>
    /// Verifies that calling <see cref="Program.ParseArguments"/> with no arguments returns an empty paths list,
    /// all flags set to <see langword="false"/>, and no unknown option.
    /// </summary>
    [TestMethod]
    public void ParseArgumentsNoArgumentsReturnsEmptyPaths()
    {
        var result = Program.ParseArguments([]);

        Assert.IsFalse(result.CheckOnly);
        Assert.IsFalse(result.DryRun);
        Assert.IsFalse(result.Verbose);
        Assert.IsFalse(result.ShowHelp);
        Assert.IsEmpty(result.Paths);
        Assert.IsNull(result.UnknownOption);
    }

    /// <summary>
    /// Verifies that <c>--check</c> sets <see cref="ParseResult.CheckOnly"/> to <see langword="true"/>.
    /// </summary>
    [TestMethod]
    public void ParseArgumentsCheckFlagSetsCheckOnly()
    {
        var result = Program.ParseArguments(["--check"]);

        Assert.IsTrue(result.CheckOnly);
        Assert.IsFalse(result.DryRun);
        Assert.IsFalse(result.Verbose);
        Assert.IsFalse(result.ShowHelp);
        Assert.IsEmpty(result.Paths);
        Assert.IsNull(result.UnknownOption);
    }

    /// <summary>
    /// Verifies that <c>--dry-run</c> sets <see cref="ParseResult.DryRun"/> to <see langword="true"/>.
    /// </summary>
    [TestMethod]
    public void ParseArgumentsDryRunFlagSetsDryRun()
    {
        var result = Program.ParseArguments(["--dry-run"]);

        Assert.IsFalse(result.CheckOnly);
        Assert.IsTrue(result.DryRun);
        Assert.IsFalse(result.Verbose);
        Assert.IsFalse(result.ShowHelp);
        Assert.IsEmpty(result.Paths);
        Assert.IsNull(result.UnknownOption);
    }

    /// <summary>
    /// Verifies that <c>--verbose</c> sets <see cref="ParseResult.Verbose"/> to <see langword="true"/>.
    /// </summary>
    [TestMethod]
    public void ParseArgumentsVerboseFlagSetsVerbose()
    {
        var result = Program.ParseArguments(["--verbose"]);

        Assert.IsFalse(result.CheckOnly);
        Assert.IsFalse(result.DryRun);
        Assert.IsTrue(result.Verbose);
        Assert.IsFalse(result.ShowHelp);
        Assert.IsEmpty(result.Paths);
        Assert.IsNull(result.UnknownOption);
    }

    /// <summary>
    /// Verifies that <c>--help</c> sets <see cref="ParseResult.ShowHelp"/> to <see langword="true"/>.
    /// </summary>
    [TestMethod]
    public void ParseArgumentsHelpFlagSetsShowHelp()
    {
        var result = Program.ParseArguments(["--help"]);

        Assert.IsFalse(result.CheckOnly);
        Assert.IsFalse(result.DryRun);
        Assert.IsFalse(result.Verbose);
        Assert.IsTrue(result.ShowHelp);
        Assert.IsEmpty(result.Paths);
        Assert.IsNull(result.UnknownOption);
    }

    /// <summary>
    /// Verifies that <c>-h</c> sets <see cref="ParseResult.ShowHelp"/> to <see langword="true"/>.
    /// </summary>
    [TestMethod]
    public void ParseArgumentsShortHelpFlagSetsShowHelp()
    {
        var result = Program.ParseArguments(["-h"]);

        Assert.IsFalse(result.CheckOnly);
        Assert.IsFalse(result.DryRun);
        Assert.IsFalse(result.Verbose);
        Assert.IsTrue(result.ShowHelp);
        Assert.IsEmpty(result.Paths);
        Assert.IsNull(result.UnknownOption);
    }

    /// <summary>
    /// Verifies that an unrecognized option like <c>--foo</c> is returned as <see cref="ParseResult.UnknownOption"/>.
    /// </summary>
    [TestMethod]
    public void ParseArgumentsUnknownOptionReturnsUnknownOption()
    {
        var result = Program.ParseArguments(["--foo"]);

        Assert.AreEqual("--foo", result.UnknownOption);
    }

    /// <summary>
    /// Verifies that multiple flags are combined correctly when passed together.
    /// </summary>
    [TestMethod]
    public void ParseArgumentsMultipleFlagsCombine()
    {
        var result = Program.ParseArguments(["--check", "--verbose"]);

        Assert.IsTrue(result.CheckOnly);
        Assert.IsFalse(result.DryRun);
        Assert.IsTrue(result.Verbose);
        Assert.IsFalse(result.ShowHelp);
        Assert.IsEmpty(result.Paths);
        Assert.IsNull(result.UnknownOption);
    }

    /// <summary>
    /// Verifies that non-option arguments are collected as paths.
    /// </summary>
    [TestMethod]
    public void ParseArgumentsPathsAreCollected()
    {
        var result = Program.ParseArguments(["file.cs", "dir/"]);

        Assert.HasCount(2, result.Paths);
        Assert.AreEqual("file.cs", result.Paths[0]);
        Assert.AreEqual("dir/", result.Paths[1]);
        Assert.IsFalse(result.CheckOnly);
        Assert.IsFalse(result.DryRun);
        Assert.IsFalse(result.Verbose);
        Assert.IsFalse(result.ShowHelp);
        Assert.IsNull(result.UnknownOption);
    }

    /// <summary>
    /// Verifies that flags and paths are correctly parsed when mixed together.
    /// </summary>
    [TestMethod]
    public void ParseArgumentsMixedFlagsAndPaths()
    {
        var result = Program.ParseArguments(["--verbose", "file.cs", "--check"]);

        Assert.IsTrue(result.CheckOnly);
        Assert.IsFalse(result.DryRun);
        Assert.IsTrue(result.Verbose);
        Assert.IsFalse(result.ShowHelp);
        Assert.HasCount(1, result.Paths);
        Assert.AreEqual("file.cs", result.Paths[0]);
        Assert.IsNull(result.UnknownOption);
    }

    #endregion // Methods
}