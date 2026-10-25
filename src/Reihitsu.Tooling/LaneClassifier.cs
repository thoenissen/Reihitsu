using Reihitsu.Tooling.Enumerations;

namespace Reihitsu.Tooling;

/// <summary>
/// Derives the squad lane a diff needs from its changed paths, and decides whether a claimed lane covers it
/// </summary>
public static class LaneClassifier
{
    #region Fields

    /// <summary>
    /// Path prefixes whose files are text, checked after the change-doc prefix
    /// </summary>
    private static readonly string[] _textPrefixes = ["sdd/", "documentation/", ".github/ISSUE_TEMPLATE/"];

    /// <summary>
    /// Path prefixes whose files are tooling, checked after the text prefixes
    /// </summary>
    private static readonly string[] _toolingPrefixes = [".claude/", "squad/", "scripts/", ".github/", "binaries/", "src/Reihitsu.Tooling/"];

    /// <summary>
    /// Individual files that are text
    /// </summary>
    private static readonly string[] _textFiles = ["README.MD", "CONTRIBUTING.md", "LICENSE", ".github/PULL_REQUEST_TEMPLATE.md"];

    /// <summary>
    /// Root files that are tooling: agent instructions and solution-wide build settings
    /// </summary>
    private static readonly string[] _toolingRootFiles = ["AGENTS.md", "CLAUDE.md", "Reihitsu.sln", "Reihitsu.sln.DotSettings", ".gitignore", ".gitattributes"];

    /// <summary>
    /// File extensions that are build settings wherever they appear
    /// </summary>
    private static readonly string[] _toolingExtensions = [".csproj", ".props", ".targets", ".ruleset", ".editorconfig"];

    #endregion // Fields

    #region Methods

    /// <summary>
    /// Classifies one changed path
    /// </summary>
    /// <param name="path">Repository-relative path with forward slashes</param>
    /// <param name="isTextOnlyProven">Whether <c>verify-text-only</c> proved the whole diff text-only, which turns every changed C# file into text</param>
    /// <returns>The category of the path</returns>
    public static PathCategory ClassifyPath(string path, bool isTextOnlyProven)
    {
        if (path.StartsWith("sdd/changes/", StringComparison.OrdinalIgnoreCase))
        {
            return PathCategory.ChangeDoc;
        }

        if (_textPrefixes.Any(prefix => path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            || _textFiles.Contains(path, StringComparer.OrdinalIgnoreCase))
        {
            return PathCategory.Text;
        }

        // Build settings are checked before the project folders, so a test project's .csproj is tooling rather than a test
        if (_toolingPrefixes.Any(prefix => path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            || _toolingRootFiles.Contains(path, StringComparer.OrdinalIgnoreCase)
            || _toolingExtensions.Any(extension => path.EndsWith(extension, StringComparison.OrdinalIgnoreCase)))
        {
            return PathCategory.Tooling;
        }

        if (path.StartsWith("src/", StringComparison.OrdinalIgnoreCase))
        {
            return ClassifySourcePath(path, isTextOnlyProven);
        }

        return PathCategory.Unknown;
    }

    /// <summary>
    /// Derives the narrowest lane that admits every category of a diff
    /// </summary>
    /// <param name="categories">Categories of every changed path</param>
    /// <param name="lane">The narrowest admitting lane, when one exists</param>
    /// <param name="reason">Why no lane admits the diff, when none does</param>
    /// <returns><see langword="true"/> when a lane admits the diff; otherwise, <see langword="false"/></returns>
    public static bool TryGetRequiredLane(IReadOnlyCollection<PathCategory> categories, out Lane lane, out string reason)
    {
        lane = Lane.Docs;
        reason = null;

        if (categories.Contains(PathCategory.Unknown))
        {
            reason = "the diff contains paths no lane rule classifies";

            return false;
        }

        var hasChangeDoc = categories.Contains(PathCategory.ChangeDoc);

        if (categories.Contains(PathCategory.Production))
        {
            if (categories.Contains(PathCategory.Tooling) && hasChangeDoc == false)
            {
                reason = "production and tooling paths without a change doc fit neither the bug-fix nor the behavior-change lane";

                return false;
            }

            lane = hasChangeDoc ? Lane.BehaviorChange : Lane.BugFix;

            return true;
        }

        if (categories.Contains(PathCategory.Tooling))
        {
            lane = Lane.Tooling;
        }
        else if (hasChangeDoc)
        {
            lane = Lane.BehaviorChange;
        }
        else if (categories.Contains(PathCategory.Test))
        {
            lane = Lane.Tests;
        }

        return true;
    }

    /// <summary>
    /// Determines whether a claimed lane runs every role the required lane runs. Lanes widen along two orders —
    /// docs, tests, bug fix, behavior change; and docs, tests, tooling, behavior change — so bug fix and tooling
    /// do not cover each other
    /// </summary>
    /// <param name="claimed">Lane the lead claimed</param>
    /// <param name="required">Lane the diff needs</param>
    /// <returns><see langword="true"/> when the claimed lane covers the required one; otherwise, <see langword="false"/></returns>
    public static bool Covers(Lane claimed, Lane required)
    {
        return claimed == required
               || required == Lane.Docs
               || (required == Lane.Tests && claimed != Lane.Docs)
               || claimed == Lane.BehaviorChange;
    }

    /// <summary>
    /// Parses a lane name as the scripts spell it
    /// </summary>
    /// <param name="name">Lane name: <c>docs</c>, <c>tests</c>, <c>bug-fix</c>, <c>behavior-change</c>, or <c>tooling</c></param>
    /// <param name="lane">The parsed lane</param>
    /// <returns><see langword="true"/> when the name is known; otherwise, <see langword="false"/></returns>
    public static bool TryParseLane(string name, out Lane lane)
    {
        switch (name)
        {
            case "docs":
                {
                    lane = Lane.Docs;

                    return true;
                }

            case "tests":
                {
                    lane = Lane.Tests;

                    return true;
                }

            case "bug-fix":
                {
                    lane = Lane.BugFix;

                    return true;
                }

            case "behavior-change":
                {
                    lane = Lane.BehaviorChange;

                    return true;
                }

            case "tooling":
                {
                    lane = Lane.Tooling;

                    return true;
                }

            default:
                {
                    lane = Lane.Docs;

                    return false;
                }
        }
    }

    /// <summary>
    /// Gets the name of a lane as the scripts spell it
    /// </summary>
    /// <param name="lane">Lane to name</param>
    /// <returns>The lane name</returns>
    public static string GetName(Lane lane)
    {
        return lane switch
               {
                   Lane.Tests => "tests",
                   Lane.BugFix => "bug-fix",
                   Lane.BehaviorChange => "behavior-change",
                   Lane.Tooling => "tooling",
                   _ => "docs"
               };
    }

    /// <summary>
    /// Classifies a path below <c>src/</c> that is neither tooling nor a build setting
    /// </summary>
    /// <param name="path">Repository-relative path with forward slashes</param>
    /// <param name="isTextOnlyProven">Whether the whole diff was proven text-only</param>
    /// <returns>The category of the path</returns>
    private static PathCategory ClassifySourcePath(string path, bool isTextOnlyProven)
    {
        if (path.EndsWith(".md", StringComparison.OrdinalIgnoreCase)
            || (isTextOnlyProven && path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase)))
        {
            return PathCategory.Text;
        }

        var segments = path.Split('/');

        if (segments.Length > 2
            && (segments[1].EndsWith(".Test", StringComparison.OrdinalIgnoreCase)
                || segments[1].Equals("Reihitsu.ArchitectureTests", StringComparison.OrdinalIgnoreCase)))
        {
            return PathCategory.Test;
        }

        return PathCategory.Production;
    }

    #endregion // Methods
}