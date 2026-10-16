using Reihitsu.Tooling.Enumerations;

namespace Reihitsu.Tooling;

/// <summary>
/// Confirms a claimed squad lane against the changed paths of a diff
/// </summary>
public static class LaneCheckCommand
{
    #region Methods

    /// <summary>
    /// Executes the lane check. The changed paths are read as <c>git diff --name-status --no-renames</c> lines
    /// </summary>
    /// <param name="args">Command arguments</param>
    /// <param name="changes">Reader that supplies the name-status lines of the diff</param>
    /// <param name="output">Standard output writer</param>
    /// <param name="error">Standard error writer</param>
    /// <returns>
    /// Exit code 0 when the claimed lane covers the diff, 1 when it does not, 3 when no lane admits the diff, or 2
    /// when the command could not run
    /// </returns>
    public static int Execute(string[] args, TextReader changes, TextWriter output, TextWriter error)
    {
        if (TryParseArguments(args, out var claimed, out var isTextOnlyProven, out var showHelp, out var parseError) == false)
        {
            error.WriteLine($"lane-check: {parseError}");
            PrintUsage(error);

            return ExitCodes.Error;
        }

        if (showHelp)
        {
            PrintUsage(output);

            return ExitCodes.Success;
        }

        if (TryReadPaths(changes, out var paths, out var readError) == false)
        {
            error.WriteLine($"lane-check: {readError}");

            return ExitCodes.Error;
        }

        var categories = new List<PathCategory>();

        foreach (var path in paths)
        {
            var category = LaneClassifier.ClassifyPath(path, isTextOnlyProven);

            categories.Add(category);
            output.WriteLine($"{category,-10} {path}");
        }

        if (paths.Count == 0)
        {
            output.WriteLine("no changed files");
        }

        return Decide(claimed, categories, output);
    }

    /// <summary>
    /// Compares the claimed lane with the lane the classified paths need and writes the verdict line
    /// </summary>
    /// <param name="claimed">Lane the lead claimed</param>
    /// <param name="categories">Categories of every changed path</param>
    /// <param name="output">Standard output writer</param>
    /// <returns>The lane check exit code</returns>
    private static int Decide(Lane claimed, List<PathCategory> categories, TextWriter output)
    {
        var claimedName = LaneClassifier.GetName(claimed);

        if (LaneClassifier.TryGetRequiredLane(categories, out var required, out var reason) == false)
        {
            output.WriteLine($"LANE CHECK: no lane — claimed {claimedName}, but {reason}");

            return ExitCodes.NoLane;
        }

        var requiredName = LaneClassifier.GetName(required);

        // A behavior change is defined by its change doc, so a claim of that lane without one is not merely wide
        if (claimed == Lane.BehaviorChange && categories.Contains(PathCategory.ChangeDoc) == false)
        {
            output.WriteLine($"LANE CHECK: mismatch — claimed {claimedName}, but the diff contains no change doc under sdd/changes/; it needs {requiredName}");

            return ExitCodes.LaneMismatch;
        }

        if (LaneClassifier.Covers(claimed, required) == false)
        {
            output.WriteLine($"LANE CHECK: mismatch — claimed {claimedName}, diff needs {requiredName}");

            return ExitCodes.LaneMismatch;
        }

        output.WriteLine(claimed == required
                             ? $"LANE CHECK: confirmed — claimed {claimedName}, diff needs {requiredName}"
                             : $"LANE CHECK: confirmed — claimed {claimedName}, diff needs only {requiredName}");

        return ExitCodes.Success;
    }

    /// <summary>
    /// Reads the changed paths from <c>git diff --name-status --no-renames</c> lines
    /// </summary>
    /// <param name="changes">Reader that supplies the lines</param>
    /// <param name="paths">Changed paths, with forward slashes</param>
    /// <param name="error">Error when a line cannot be read</param>
    /// <returns><see langword="true"/> when every line was read; otherwise, <see langword="false"/></returns>
    private static bool TryReadPaths(TextReader changes, out List<string> paths, out string error)
    {
        paths = [];
        error = null;

        var line = changes.ReadLine();

        while (line != null)
        {
            if (line.Length > 0)
            {
                var fields = line.Split('\t');

                if (fields.Length != 2 || fields[0].Length == 0 || fields[1].Length == 0)
                {
                    error = $"unexpected name-status line: {line}";

                    return false;
                }

                paths.Add(fields[1].Replace('\\', '/'));
            }

            line = changes.ReadLine();
        }

        return true;
    }

    /// <summary>
    /// Prints command usage
    /// </summary>
    /// <param name="writer">Writer that receives the usage text</param>
    private static void PrintUsage(TextWriter writer)
    {
        writer.WriteLine("usage: lane-check --lane <docs|tests|bug-fix|behavior-change|tooling> --text-only <pass|fail>");
        writer.WriteLine("       lane-check --help");
        writer.WriteLine("       reads `git diff --name-status --no-renames` lines from standard input");
    }

    /// <summary>
    /// Parses the command arguments
    /// </summary>
    /// <param name="args">Arguments to parse</param>
    /// <param name="claimed">Claimed lane</param>
    /// <param name="isTextOnlyProven">Whether <c>verify-text-only</c> proved the diff text-only</param>
    /// <param name="showHelp">Whether usage should be printed</param>
    /// <param name="error">Argument error when parsing fails</param>
    /// <returns><see langword="true"/> when the arguments are valid; otherwise, <see langword="false"/></returns>
    private static bool TryParseArguments(string[] args,
                                          out Lane claimed,
                                          out bool isTextOnlyProven,
                                          out bool showHelp,
                                          out string error)
    {
        claimed = Lane.Docs;
        isTextOnlyProven = false;
        showHelp = false;
        error = null;

        string laneName = null;
        string textOnly = null;

        var index = 0;

        while (index < args.Length)
        {
            var argument = args[index++];

            switch (argument)
            {
                case "--help" or "-h":
                    {
                        showHelp = true;
                    }
                    break;

                case "--lane" when index < args.Length && laneName == null:
                    {
                        laneName = args[index++];
                    }
                    break;

                case "--text-only" when index < args.Length && textOnly == null:
                    {
                        textOnly = args[index++];
                    }
                    break;

                default:
                    {
                        error = $"unexpected argument '{argument}'.";

                        return false;
                    }
            }
        }

        if (showHelp)
        {
            return true;
        }

        if (laneName == null || LaneClassifier.TryParseLane(laneName, out claimed) == false)
        {
            error = "--lane expects docs, tests, bug-fix, behavior-change, or tooling.";

            return false;
        }

        if (textOnly is not ("pass" or "fail"))
        {
            error = "--text-only expects pass or fail.";

            return false;
        }

        isTextOnlyProven = textOnly == "pass";

        return true;
    }

    #endregion // Methods
}