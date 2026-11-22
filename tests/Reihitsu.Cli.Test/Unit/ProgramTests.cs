using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Cli.Test.Helpers;
using Reihitsu.Formatter.Utilities;

namespace Reihitsu.Cli.Test.Unit;

/// <summary>
/// Tests for <see cref="Program.ParseArguments"/>
/// </summary>
[TestClass]
public class ProgramTests
{
    #region Methods

    /// <summary>
    /// Verifies that calling <see cref="Program.ParseArguments"/> with no arguments returns an empty paths list,
    /// all flags set to <see langword="false"/>, and no unknown option
    /// </summary>
    [TestMethod]
    [Requirement("CLI-001")]
    public void ParseArgumentsNoArgumentsReturnsEmptyPaths()
    {
        var result = Program.ParseArguments([]);

        Assert.IsFalse(result.CheckOnly);
        Assert.IsFalse(result.DryRun);
        Assert.IsFalse(result.Verbose);
        Assert.IsFalse(result.Force);
        Assert.IsFalse(result.Utf8Bom);
        Assert.IsFalse(result.ShowHelp);
        Assert.IsFalse(result.ShowVersion);
        Assert.IsEmpty(result.Paths);
        Assert.IsNull(result.UnknownOption);
    }

    /// <summary>
    /// Verifies that <c>--check</c> sets <see cref="ParseResult.CheckOnly"/> to <see langword="true"/>
    /// </summary>
    [TestMethod]
    [Requirement("CLI-001")]
    public void ParseArgumentsCheckFlagSetsCheckOnly()
    {
        var result = Program.ParseArguments(["--check"]);

        Assert.IsTrue(result.CheckOnly);
        Assert.IsFalse(result.DryRun);
        Assert.IsFalse(result.Verbose);
        Assert.IsFalse(result.Force);
        Assert.IsFalse(result.Utf8Bom);
        Assert.IsFalse(result.ShowHelp);
        Assert.IsFalse(result.ShowVersion);
        Assert.IsEmpty(result.Paths);
        Assert.IsNull(result.UnknownOption);
    }

    /// <summary>
    /// Verifies that <c>--dry-run</c> sets <see cref="ParseResult.DryRun"/> to <see langword="true"/>
    /// </summary>
    [TestMethod]
    [Requirement("CLI-001")]
    public void ParseArgumentsDryRunFlagSetsDryRun()
    {
        var result = Program.ParseArguments(["--dry-run"]);

        Assert.IsFalse(result.CheckOnly);
        Assert.IsTrue(result.DryRun);
        Assert.IsFalse(result.Verbose);
        Assert.IsFalse(result.Force);
        Assert.IsFalse(result.Utf8Bom);
        Assert.IsFalse(result.ShowHelp);
        Assert.IsFalse(result.ShowVersion);
        Assert.IsEmpty(result.Paths);
        Assert.IsNull(result.UnknownOption);
    }

    /// <summary>
    /// Verifies that <c>--verbose</c> sets <see cref="ParseResult.Verbose"/> to <see langword="true"/>
    /// </summary>
    [TestMethod]
    [Requirement("CLI-001")]
    public void ParseArgumentsVerboseFlagSetsVerbose()
    {
        var result = Program.ParseArguments(["--verbose"]);

        Assert.IsFalse(result.CheckOnly);
        Assert.IsFalse(result.DryRun);
        Assert.IsTrue(result.Verbose);
        Assert.IsFalse(result.Force);
        Assert.IsFalse(result.Utf8Bom);
        Assert.IsFalse(result.ShowHelp);
        Assert.IsFalse(result.ShowVersion);
        Assert.IsEmpty(result.Paths);
        Assert.IsNull(result.UnknownOption);
    }

    /// <summary>
    /// Verifies that <c>--force</c> sets <see cref="ParseResult.Force"/> to <see langword="true"/>
    /// </summary>
    [TestMethod]
    [Requirement("CLI-001")]
    public void ParseArgumentsForceFlagSetsForce()
    {
        var result = Program.ParseArguments(["--force"]);

        Assert.IsFalse(result.CheckOnly);
        Assert.IsFalse(result.DryRun);
        Assert.IsFalse(result.Verbose);
        Assert.IsTrue(result.Force);
        Assert.IsFalse(result.Utf8Bom);
        Assert.IsFalse(result.ShowHelp);
        Assert.IsFalse(result.ShowVersion);
        Assert.IsEmpty(result.Paths);
        Assert.IsNull(result.UnknownOption);
    }

    /// <summary>
    /// Verifies that <c>--utf8-bom</c> sets <see cref="ParseResult.Utf8Bom"/> to <see langword="true"/>
    /// </summary>
    [TestMethod]
    [Requirement("CLI-001")]
    public void ParseArgumentsUtf8BomFlagSetsUtf8Bom()
    {
        var result = Program.ParseArguments(["--utf8-bom"]);

        Assert.IsFalse(result.CheckOnly);
        Assert.IsFalse(result.DryRun);
        Assert.IsFalse(result.Verbose);
        Assert.IsFalse(result.Force);
        Assert.IsTrue(result.Utf8Bom);
        Assert.IsFalse(result.ShowHelp);
        Assert.IsFalse(result.ShowVersion);
        Assert.IsEmpty(result.Paths);
        Assert.IsNull(result.UnknownOption);
    }

    /// <summary>
    /// Verifies that <c>--suppress-unsupported</c> sets <see cref="ParseResult.SuppressUnsupported"/> to <see langword="true"/> and
    /// no other option
    /// </summary>
    [TestMethod]
    [Requirement("CLI-001")]
    public void ParseArgumentsSuppressUnsupportedFlagSetsSuppressUnsupported()
    {
        var result = Program.ParseArguments(["--suppress-unsupported"]);

        Assert.IsTrue(result.SuppressUnsupported);
        Assert.IsFalse(result.CheckOnly);
        Assert.IsFalse(result.DryRun);
        Assert.IsFalse(result.Verbose);
        Assert.IsFalse(result.Force);
        Assert.IsFalse(result.Utf8Bom);
        Assert.IsFalse(result.ShowHelp);
        Assert.IsFalse(result.ShowVersion);
        Assert.IsEmpty(result.Paths);
        Assert.IsNull(result.UnknownOption);
    }

    /// <summary>
    /// Verifies that <c>--suppress-unsupported</c> is accepted in any order, mixed with paths and other options, and is off
    /// without it
    /// </summary>
    [TestMethod]
    [Requirement("CLI-001")]
    public void ParseArgumentsSuppressUnsupportedFlagMixesWithPathsAndOptions()
    {
        var result = Program.ParseArguments(["src", "--suppress-unsupported", "--check"]);
        var without = Program.ParseArguments(["--check", "src"]);

        Assert.IsTrue(result.SuppressUnsupported);
        Assert.IsTrue(result.CheckOnly);
        Assert.AreSequenceEqual(new[] { "src" }, result.Paths.ToArray());
        Assert.IsNull(result.UnknownOption);
        Assert.IsFalse(without.SuppressUnsupported);
    }

    /// <summary>
    /// Verifies that <c>--help</c> sets <see cref="ParseResult.ShowHelp"/> to <see langword="true"/>
    /// </summary>
    [TestMethod]
    [Requirement("CLI-001")]
    [Requirement("CLI-003")]
    public void ParseArgumentsHelpFlagSetsShowHelp()
    {
        var result = Program.ParseArguments(["--help"]);

        Assert.IsFalse(result.CheckOnly);
        Assert.IsFalse(result.DryRun);
        Assert.IsFalse(result.Verbose);
        Assert.IsFalse(result.Force);
        Assert.IsFalse(result.Utf8Bom);
        Assert.IsTrue(result.ShowHelp);
        Assert.IsFalse(result.ShowVersion);
        Assert.IsEmpty(result.Paths);
        Assert.IsNull(result.UnknownOption);
    }

    /// <summary>
    /// Verifies that <c>-h</c> sets <see cref="ParseResult.ShowHelp"/> to <see langword="true"/>
    /// </summary>
    [TestMethod]
    [Requirement("CLI-001")]
    [Requirement("CLI-003")]
    public void ParseArgumentsShortHelpFlagSetsShowHelp()
    {
        var result = Program.ParseArguments(["-h"]);

        Assert.IsFalse(result.CheckOnly);
        Assert.IsFalse(result.DryRun);
        Assert.IsFalse(result.Verbose);
        Assert.IsFalse(result.Force);
        Assert.IsFalse(result.Utf8Bom);
        Assert.IsTrue(result.ShowHelp);
        Assert.IsFalse(result.ShowVersion);
        Assert.IsEmpty(result.Paths);
        Assert.IsNull(result.UnknownOption);
    }

    /// <summary>
    /// Verifies that <c>--version</c> sets <see cref="ParseResult.ShowVersion"/> to <see langword="true"/>
    /// </summary>
    [TestMethod]
    [Requirement("CLI-001")]
    [Requirement("CLI-003")]
    public void ParseArgumentsVersionFlagSetsShowVersion()
    {
        var result = Program.ParseArguments(["--version"]);

        Assert.IsFalse(result.CheckOnly);
        Assert.IsFalse(result.DryRun);
        Assert.IsFalse(result.Verbose);
        Assert.IsFalse(result.Force);
        Assert.IsFalse(result.Utf8Bom);
        Assert.IsFalse(result.ShowHelp);
        Assert.IsTrue(result.ShowVersion);
        Assert.IsEmpty(result.Paths);
        Assert.IsNull(result.UnknownOption);
    }

    /// <summary>
    /// Verifies that an unrecognized option like <c>--foo</c> is returned as <see cref="ParseResult.UnknownOption"/>
    /// </summary>
    [TestMethod]
    [Requirement("CLI-001")]
    [Requirement("CLI-002")]
    public void ParseArgumentsUnknownOptionReturnsUnknownOption()
    {
        var result = Program.ParseArguments(["--foo"]);

        Assert.AreEqual("--foo", result.UnknownOption);
    }

    /// <summary>
    /// Verifies that multiple flags are combined correctly when passed together
    /// </summary>
    [TestMethod]
    [Requirement("CLI-001")]
    public void ParseArgumentsMultipleFlagsCombine()
    {
        var result = Program.ParseArguments(["--check", "--verbose", "--utf8-bom"]);

        Assert.IsTrue(result.CheckOnly);
        Assert.IsFalse(result.DryRun);
        Assert.IsTrue(result.Verbose);
        Assert.IsFalse(result.Force);
        Assert.IsTrue(result.Utf8Bom);
        Assert.IsFalse(result.ShowHelp);
        Assert.IsFalse(result.ShowVersion);
        Assert.IsEmpty(result.Paths);
        Assert.IsNull(result.UnknownOption);
    }

    /// <summary>
    /// Verifies that non-option arguments are collected as paths
    /// </summary>
    [TestMethod]
    [Requirement("CLI-001")]
    public void ParseArgumentsPathsAreCollected()
    {
        var result = Program.ParseArguments(["file.cs", "dir/"]);

        Assert.HasCount(2, result.Paths);
        Assert.AreEqual("file.cs", result.Paths[0]);
        Assert.AreEqual("dir/", result.Paths[1]);
        Assert.IsFalse(result.CheckOnly);
        Assert.IsFalse(result.DryRun);
        Assert.IsFalse(result.Verbose);
        Assert.IsFalse(result.Force);
        Assert.IsFalse(result.Utf8Bom);
        Assert.IsFalse(result.ShowHelp);
        Assert.IsFalse(result.ShowVersion);
        Assert.IsNull(result.UnknownOption);
    }

    /// <summary>
    /// Verifies that flags and paths are correctly parsed when mixed together
    /// </summary>
    [TestMethod]
    [Requirement("CLI-001")]
    public void ParseArgumentsMixedFlagsAndPaths()
    {
        var result = Program.ParseArguments(["--verbose", "file.cs", "--check"]);

        Assert.IsTrue(result.CheckOnly);
        Assert.IsFalse(result.DryRun);
        Assert.IsTrue(result.Verbose);
        Assert.IsFalse(result.Force);
        Assert.IsFalse(result.Utf8Bom);
        Assert.IsFalse(result.ShowHelp);
        Assert.IsFalse(result.ShowVersion);
        Assert.HasCount(1, result.Paths);
        Assert.AreEqual("file.cs", result.Paths[0]);
        Assert.IsNull(result.UnknownOption);
    }

    /// <summary>
    /// Verifies that arguments after the <c>--</c> separator are treated as paths even when they start with a dash
    /// </summary>
    [TestMethod]
    [Requirement("CLI-001")]
    public void ParseArgumentsSeparatorTreatsRemainingArgumentsAsPaths()
    {
        var result = Program.ParseArguments(["--check", "--", "--dry-run", "-weird.cs"]);

        Assert.IsTrue(result.CheckOnly);
        Assert.IsFalse(result.DryRun);
        Assert.HasCount(2, result.Paths);
        Assert.AreEqual("--dry-run", result.Paths[0]);
        Assert.AreEqual("-weird.cs", result.Paths[1]);
        Assert.IsNull(result.UnknownOption);
    }

    /// <summary>
    /// Verifies that omitting <c>--lang-version</c> selects the newest supported language version without a warning or error
    /// </summary>
    [TestMethod]
    [Requirement("CLI-004")]
    public void ParseArgumentsWithoutLangVersionSelectsMaxSupportedVersion()
    {
        var result = Program.ParseArguments([]);

        Assert.AreEqual(LanguageVersionResolver.MaxSupportedLanguageVersion, result.LanguageVersion);
        Assert.IsNull(result.ExceedingLanguageVersion);
        Assert.IsNull(result.ArgumentError);
    }

    /// <summary>
    /// Verifies that an explicit major version is selected as given
    /// </summary>
    [TestMethod]
    [Requirement("CLI-004")]
    public void ParseArgumentsLangVersionSelectsExplicitVersion()
    {
        var result = Program.ParseArguments(["--lang-version", "11", "src"]);

        Assert.AreEqual(LanguageVersion.CSharp11, result.LanguageVersion);
        Assert.IsNull(result.ExceedingLanguageVersion);
        Assert.IsNull(result.ArgumentError);
        Assert.HasCount(1, result.Paths);
        Assert.AreEqual("src", result.Paths[0]);
    }

    /// <summary>
    /// Verifies that an explicit version with a minor component is accepted
    /// </summary>
    [TestMethod]
    [Requirement("CLI-004")]
    public void ParseArgumentsLangVersionAcceptsMajorMinorForm()
    {
        var result = Program.ParseArguments(["--lang-version", "12.0"]);

        Assert.AreEqual(LanguageVersion.CSharp12, result.LanguageVersion);
        Assert.IsNull(result.ArgumentError);
    }

    /// <summary>
    /// Verifies that <c>latest</c> selects the newest supported version rather than Roslyn's newest, without a warning
    /// </summary>
    [TestMethod]
    [Requirement("CLI-004")]
    public void ParseArgumentsLangVersionLatestSelectsMaxSupportedVersion()
    {
        var result = Program.ParseArguments(["--lang-version", "latest"]);

        Assert.AreEqual(LanguageVersionResolver.MaxSupportedLanguageVersion, result.LanguageVersion);
        Assert.IsNull(result.ExceedingLanguageVersion);
        Assert.IsNull(result.ArgumentError);
    }

    /// <summary>
    /// Verifies that <c>default</c> selects the newest supported version without a warning
    /// </summary>
    [TestMethod]
    [Requirement("CLI-004")]
    public void ParseArgumentsLangVersionDefaultSelectsMaxSupportedVersion()
    {
        var result = Program.ParseArguments(["--lang-version", "default"]);

        Assert.AreEqual(LanguageVersionResolver.MaxSupportedLanguageVersion, result.LanguageVersion);
        Assert.IsNull(result.ExceedingLanguageVersion);
    }

    /// <summary>
    /// Verifies that a version newer than the newest supported one is clamped and reported for a warning
    /// </summary>
    [TestMethod]
    [Requirement("CLI-004")]
    public void ParseArgumentsLangVersionPreviewIsClampedToMaxSupportedVersion()
    {
        var result = Program.ParseArguments(["--lang-version", "preview"]);

        Assert.AreEqual(LanguageVersionResolver.MaxSupportedLanguageVersion, result.LanguageVersion);
        Assert.AreEqual("preview", result.ExceedingLanguageVersion);
        Assert.IsNull(result.ArgumentError);
    }

    /// <summary>
    /// Verifies that omitting <c>--lang-version</c> selects C# 15 without a warning or error
    /// </summary>
    [TestMethod]
    [Requirement("CLI-004")]
    public void ParseArgumentsWithoutLangVersionSelectsCSharp15()
    {
        var result = Program.ParseArguments([]);

        Assert.AreEqual(LanguageVersion.CSharp15, result.LanguageVersion);
        Assert.IsNull(result.ExceedingLanguageVersion);
        Assert.IsNull(result.ArgumentError);
    }

    /// <summary>
    /// Verifies that <c>latest</c> selects C# 15 without a warning
    /// </summary>
    [TestMethod]
    [Requirement("CLI-004")]
    public void ParseArgumentsLangVersionLatestSelectsCSharp15()
    {
        var result = Program.ParseArguments(["--lang-version", "latest"]);

        Assert.AreEqual(LanguageVersion.CSharp15, result.LanguageVersion);
        Assert.IsNull(result.ExceedingLanguageVersion);
        Assert.IsNull(result.ArgumentError);
    }

    /// <summary>
    /// Verifies that <c>--lang-version 15</c> selects C# 15 without a warning or error
    /// </summary>
    [TestMethod]
    [Requirement("CLI-004")]
    public void ParseArgumentsLangVersion15SelectsCSharp15WithoutWarning()
    {
        var result = Program.ParseArguments(["--lang-version", "15"]);

        Assert.AreEqual(LanguageVersion.CSharp15, result.LanguageVersion);
        Assert.IsNull(result.ExceedingLanguageVersion);
        Assert.IsNull(result.ArgumentError);
    }

    /// <summary>
    /// Verifies that <c>--lang-version 15.0</c> selects C# 15 without a warning or error
    /// </summary>
    [TestMethod]
    [Requirement("CLI-004")]
    public void ParseArgumentsLangVersion15Point0SelectsCSharp15WithoutWarning()
    {
        var result = Program.ParseArguments(["--lang-version", "15.0"]);

        Assert.AreEqual(LanguageVersion.CSharp15, result.LanguageVersion);
        Assert.IsNull(result.ExceedingLanguageVersion);
        Assert.IsNull(result.ArgumentError);
    }

    /// <summary>
    /// Verifies that <c>--lang-version 14</c> selects C# 14 without a warning, below the newest supported version
    /// </summary>
    [TestMethod]
    [Requirement("CLI-004")]
    public void ParseArgumentsLangVersion14SelectsCSharp14WithoutWarning()
    {
        var result = Program.ParseArguments(["--lang-version", "14"]);

        Assert.AreEqual(LanguageVersion.CSharp14, result.LanguageVersion);
        Assert.IsNull(result.ExceedingLanguageVersion);
        Assert.IsNull(result.ArgumentError);
    }

    /// <summary>
    /// Verifies that <c>preview</c> is clamped to C# 15 and reported for a warning
    /// </summary>
    [TestMethod]
    [Requirement("CLI-004")]
    public void ParseArgumentsLangVersionPreviewIsClampedToCSharp15()
    {
        var result = Program.ParseArguments(["--lang-version", "preview"]);

        Assert.AreEqual(LanguageVersion.CSharp15, result.LanguageVersion);
        Assert.AreEqual("preview", result.ExceedingLanguageVersion);
        Assert.IsNull(result.ArgumentError);
    }

    /// <summary>
    /// Verifies that an unrecognized version produces an argument error naming the option and the value
    /// </summary>
    [TestMethod]
    [Requirement("CLI-002")]
    [Requirement("CLI-004")]
    public void ParseArgumentsLangVersionInvalidValueReturnsArgumentError()
    {
        var result = Program.ParseArguments(["--lang-version", "foo"]);

        Assert.IsNotNull(result.ArgumentError);
        Assert.Contains("--lang-version", result.ArgumentError);
        Assert.Contains("'foo'", result.ArgumentError);
    }

    /// <summary>
    /// Verifies that a version Roslyn does not know produces an argument error
    /// </summary>
    [TestMethod]
    [Requirement("CLI-002")]
    [Requirement("CLI-004")]
    public void ParseArgumentsLangVersionUnknownNumberReturnsArgumentError()
    {
        var result = Program.ParseArguments(["--lang-version", "99"]);

        Assert.IsNotNull(result.ArgumentError);
        Assert.Contains("'99'", result.ArgumentError);
    }

    /// <summary>
    /// Verifies that <c>--lang-version</c> without a value produces an argument error
    /// </summary>
    [TestMethod]
    [Requirement("CLI-002")]
    public void ParseArgumentsLangVersionWithoutValueReturnsArgumentError()
    {
        var result = Program.ParseArguments(["--lang-version"]);

        Assert.IsNotNull(result.ArgumentError);
        Assert.Contains("--lang-version", result.ArgumentError);
    }

    /// <summary>
    /// Verifies that the last <c>--lang-version</c> wins when the option is repeated
    /// </summary>
    [TestMethod]
    [Requirement("CLI-004")]
    public void ParseArgumentsRepeatedLangVersionUsesLastValue()
    {
        var result = Program.ParseArguments(["--lang-version", "preview", "--lang-version", "10"]);

        Assert.AreEqual(LanguageVersion.CSharp10, result.LanguageVersion);
        Assert.IsNull(result.ExceedingLanguageVersion);
    }

    /// <summary>
    /// Verifies that <c>--lang-version</c> after the <c>--</c> separator is treated as a path
    /// </summary>
    [TestMethod]
    [Requirement("CLI-001")]
    public void ParseArgumentsLangVersionAfterSeparatorIsPath()
    {
        var result = Program.ParseArguments(["--", "--lang-version", "11"]);

        Assert.AreEqual(LanguageVersionResolver.MaxSupportedLanguageVersion, result.LanguageVersion);
        Assert.HasCount(2, result.Paths);
        Assert.AreEqual("--lang-version", result.Paths[0]);
    }

    /// <summary>
    /// Verifies that without <c>--max-parallelism</c> the degree of parallelism is the number of logical processors
    /// </summary>
    [TestMethod]
    [Requirement("CLI-023")]
    public void ParseArgumentsWithoutMaxParallelismUsesProcessorCount()
    {
        var result = Program.ParseArguments(["--check", "src"]);

        Assert.AreEqual(Environment.ProcessorCount, result.MaxParallelism);
        Assert.IsNull(result.ArgumentError);
    }

    /// <summary>
    /// Verifies that <c>--max-parallelism</c> is accepted among options and paths and sets the degree of parallelism
    /// </summary>
    [TestMethod]
    [Requirement("CLI-001")]
    [Requirement("CLI-023")]
    public void ParseArgumentsMaxParallelismMixesWithPathsAndOptions()
    {
        var result = Program.ParseArguments(["src", "--max-parallelism", "3", "--check", "tests"]);

        Assert.AreEqual(3, result.MaxParallelism);
        Assert.IsTrue(result.CheckOnly);
        Assert.AreSequenceEqual(new[] { "src", "tests" }, result.Paths.ToArray());
        Assert.IsNull(result.UnknownOption);
        Assert.IsNull(result.ArgumentError);
    }

    /// <summary>
    /// Verifies that the accepted counts of the specification are used as given: <c>1</c>, <c>2</c>, <c>1000</c>, the largest
    /// 32-bit integer, and <c>007</c> as seven
    /// </summary>
    /// <param name="value">The value given to the option</param>
    /// <param name="expected">The degree of parallelism the value selects</param>
    [TestMethod]
    [Requirement("CLI-023")]
    [DataRow("1", 1)]
    [DataRow("2", 2)]
    [DataRow("1000", 1000)]
    [DataRow("2147483647", int.MaxValue)]
    [DataRow("007", 7)]
    public void ParseArgumentsMaxParallelismAcceptsDecimalCount(string value, int expected)
    {
        var result = Program.ParseArguments(["--max-parallelism", value]);

        Assert.AreEqual(expected, result.MaxParallelism);
        Assert.IsNull(result.ArgumentError);
        Assert.IsNull(result.UnknownOption);
        Assert.IsEmpty(result.Paths);
    }

    /// <summary>
    /// Verifies that every value that is not decimal digits from 1 to 2147483647 is invalid and gives the error message of the
    /// specification; <c>\u0663</c> is ARABIC-INDIC DIGIT THREE, a digit to <see cref="char.IsDigit(char)"/> but not a decimal digit
    /// <c>0</c>-<c>9</c>
    /// </summary>
    /// <param name="value">The value given to the option</param>
    [TestMethod]
    [Requirement("CLI-002")]
    [Requirement("CLI-023")]
    [DataRow("0")]
    [DataRow("0000")]
    [DataRow("-1")]
    [DataRow("+2")]
    [DataRow(" 2")]
    [DataRow("2 ")]
    [DataRow("two")]
    [DataRow("1.5")]
    [DataRow("1,000")]
    [DataRow("1e3")]
    [DataRow("0x10")]
    [DataRow("99999999999")]
    [DataRow("2147483648")]
    [DataRow("\u0663")]
    [DataRow("")]
    public void ParseArgumentsMaxParallelismInvalidValueReturnsArgumentError(string value)
    {
        var result = Program.ParseArguments(["--max-parallelism", value, "src"]);

        Assert.AreEqual($"Invalid value for --max-parallelism: '{value}'. Expected a whole number of 1 or more.", result.ArgumentError);
        Assert.IsNull(result.UnknownOption);
    }

    /// <summary>
    /// Verifies that <c>--max-parallelism</c> as the last argument gives the missing-value error
    /// </summary>
    [TestMethod]
    [Requirement("CLI-002")]
    public void ParseArgumentsMaxParallelismWithoutValueReturnsArgumentError()
    {
        var result = Program.ParseArguments(["--check", "src", "--max-parallelism"]);

        Assert.AreEqual("Missing value for --max-parallelism.", result.ArgumentError);
    }

    /// <summary>
    /// Verifies that the argument after <c>--max-parallelism</c> is its value even when it looks like an option, so
    /// <c>--max-parallelism --check</c> is an invalid count and does not select check mode
    /// </summary>
    [TestMethod]
    [Requirement("CLI-002")]
    [Requirement("CLI-023")]
    public void ParseArgumentsMaxParallelismConsumesFollowingOptionAsValue()
    {
        var result = Program.ParseArguments(["--max-parallelism", "--check", "src"]);

        Assert.AreEqual("Invalid value for --max-parallelism: '--check'. Expected a whole number of 1 or more.", result.ArgumentError);
        Assert.IsFalse(result.CheckOnly);
    }

    /// <summary>
    /// Verifies that the last valid count wins when <c>--max-parallelism</c> is repeated
    /// </summary>
    [TestMethod]
    [Requirement("CLI-023")]
    public void ParseArgumentsRepeatedMaxParallelismUsesLastValue()
    {
        var result = Program.ParseArguments(["--max-parallelism", "2", "--max-parallelism", "8"]);

        Assert.AreEqual(8, result.MaxParallelism);
        Assert.IsNull(result.ArgumentError);
    }

    /// <summary>
    /// Verifies that an invalid count stays an error when a valid count follows it
    /// </summary>
    [TestMethod]
    [Requirement("CLI-002")]
    public void ParseArgumentsInvalidMaxParallelismFollowedByValidCountReturnsArgumentError()
    {
        var result = Program.ParseArguments(["--max-parallelism", "0", "--max-parallelism", "4"]);

        Assert.AreEqual("Invalid value for --max-parallelism: '0'. Expected a whole number of 1 or more.", result.ArgumentError);
    }

    /// <summary>
    /// Verifies that <c>--max-parallelism</c> after the <c>--</c> separator is a path
    /// </summary>
    [TestMethod]
    [Requirement("CLI-001")]
    public void ParseArgumentsMaxParallelismAfterSeparatorIsPath()
    {
        var result = Program.ParseArguments(["--", "--max-parallelism", "2"]);

        Assert.AreEqual(Environment.ProcessorCount, result.MaxParallelism);
        Assert.AreSequenceEqual(new[] { "--max-parallelism", "2" }, result.Paths.ToArray());
        Assert.IsNull(result.ArgumentError);
    }

    #endregion // Methods
}