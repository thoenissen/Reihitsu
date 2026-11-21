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
    private static readonly string[] _textPrefixes = ["sdd/", "squad/notes/", "documentation/", ".github/ISSUE_TEMPLATE/"];

    /// <summary>
    /// Path prefixes whose files are tooling, checked after the text prefixes
    /// </summary>
    private static readonly string[] _toolingPrefixes = [".claude/", "squad/", "scripts/", ".github/", "binaries/", "src/Reihitsu.Tooling/"];

    /// <summary>
    /// Individual files that are text, besides the Markdown files at the repository root
    /// </summary>
    private static readonly string[] _textFiles = ["LICENSE", ".github/PULL_REQUEST_TEMPLATE.md"];

    /// <summary>
    /// Root files that are tooling: agent instructions, solution-wide build settings, and the repository's own analyzer configuration
    /// </summary>
    private static readonly string[] _toolingRootFiles = ["AGENTS.md", "CLAUDE.md", "Reihitsu.sln", "Reihitsu.sln.DotSettings", ".gitignore", ".gitattributes", "reihitsu.json"];

    /// <summary>
    /// File extensions that are build settings wherever they appear
    /// </summary>
    private static readonly string[] _toolingExtensions = [".csproj", ".props", ".targets", ".ruleset", ".editorconfig"];

    /// <summary>
    /// The resource files that hold the diagnostic titles and messages and the code-fix titles; every other resource
    /// file keeps the category of where it lives
    /// </summary>
    private static readonly string[] _wordingFiles = ["src/Reihitsu.Analyzer/AnalyzerResources.resx", "src/Reihitsu.Analyzer.CodeFixes/CodeFixResources.resx"];

    #endregion // Fields

    #region Methods

    /// <summary>
    /// Classifies one changed path
    /// </summary>
    /// <param name="path">Repository-relative path with forward slashes</param>
    /// <param name="isTextOnlyProven">Whether <c>verify-text-only</c> proved the whole diff apart from the wording paths text-only, which turns every changed C# file into text</param>
    /// <returns>The category of the path</returns>
    public static PathCategory ClassifyPath(string path, bool isTextOnlyProven)
    {
        if (path.StartsWith("sdd/changes/", StringComparison.OrdinalIgnoreCase))
        {
            return PathCategory.ChangeDoc;
        }

        if (_textPrefixes.Any(prefix => path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            || _textFiles.Contains(path, StringComparer.OrdinalIgnoreCase)
            || IsRootMarkdownText(path))
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
    /// Derives the narrowest lane that admits every category of a diff. The first rule that applies decides: an
    /// unknown path admits no lane; production needs bug fix, or behavior change with a change doc, and admits no
    /// lane next to tooling without one; tooling needs the tooling lane, or behavior change next to wording, which
    /// admits no lane without a change doc; wording needs the wording lane, or the tests lane next to test paths,
    /// which becomes behavior change with a change doc; otherwise a change doc needs behavior change, test paths
    /// the tests lane, and text alone the docs lane
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
        var hasWording = categories.Contains(PathCategory.Wording);

        if (categories.Contains(PathCategory.Production))
        {
            return TryGetProductionLane(categories, hasChangeDoc, out lane, out reason);
        }

        if (categories.Contains(PathCategory.Tooling))
        {
            return TryGetToolingLane(hasWording, hasChangeDoc, out lane, out reason);
        }

        lane = GetTextLane(categories, hasWording, hasChangeDoc);

        return true;
    }

    /// <summary>
    /// Determines whether a claimed lane runs every role the required lane runs. Lanes widen along two orders —
    /// docs, wording, tests, bug fix, behavior change; and docs, tests, tooling, behavior change — so bug fix and
    /// tooling do not cover each other, tooling does not cover wording, and wording covers only docs
    /// </summary>
    /// <param name="claimed">Lane the lead claimed</param>
    /// <param name="required">Lane the diff needs</param>
    /// <returns><see langword="true"/> when the claimed lane covers the required one; otherwise, <see langword="false"/></returns>
    public static bool Covers(Lane claimed, Lane required)
    {
        return claimed == required
               || required == Lane.Docs
               || (required == Lane.Wording && claimed is (Lane.Tests or Lane.BugFix))
               || (required == Lane.Tests && claimed is not (Lane.Docs or Lane.Wording))
               || claimed == Lane.BehaviorChange;
    }

    /// <summary>
    /// Parses a lane name as the scripts spell it
    /// </summary>
    /// <param name="name">Lane name: <c>docs</c>, <c>wording</c>, <c>tests</c>, <c>bug-fix</c>, <c>behavior-change</c>, or <c>tooling</c></param>
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

            case "wording":
                {
                    lane = Lane.Wording;

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
                   Lane.Wording => "wording",
                   Lane.Tests => "tests",
                   Lane.BugFix => "bug-fix",
                   Lane.BehaviorChange => "behavior-change",
                   Lane.Tooling => "tooling",
                   _ => "docs"
               };
    }

    /// <summary>
    /// Determines whether a path is a Markdown file at the repository root other than the agent instructions, which
    /// are tooling
    /// </summary>
    /// <param name="path">Repository-relative path with forward slashes</param>
    /// <returns><see langword="true"/> when the path is root Markdown that is text; otherwise, <see langword="false"/></returns>
    private static bool IsRootMarkdownText(string path)
    {
        return path.Contains('/') == false
               && path.EndsWith(".md", StringComparison.OrdinalIgnoreCase)
               && _toolingRootFiles.Contains(path, StringComparer.OrdinalIgnoreCase) == false;
    }

    /// <summary>
    /// Classifies a path below <c>src/</c> that is neither tooling nor a build setting; the two resource files that
    /// hold the diagnostic and code-fix texts are wording whatever the proof found
    /// </summary>
    /// <param name="path">Repository-relative path with forward slashes</param>
    /// <param name="isTextOnlyProven">Whether the whole diff apart from the wording paths was proven text-only</param>
    /// <returns>The category of the path</returns>
    private static PathCategory ClassifySourcePath(string path, bool isTextOnlyProven)
    {
        if (_wordingFiles.Contains(path, StringComparer.OrdinalIgnoreCase))
        {
            return PathCategory.Wording;
        }

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

    /// <summary>
    /// Derives the lane of a diff that holds production paths: bug fix, or behavior change with a change doc; next to
    /// tooling paths, no lane admits it without a change doc
    /// </summary>
    /// <param name="categories">Categories of every changed path</param>
    /// <param name="hasChangeDoc">Whether the diff holds a change doc</param>
    /// <param name="lane">The narrowest admitting lane, when one exists</param>
    /// <param name="reason">Why no lane admits the diff, when none does</param>
    /// <returns><see langword="true"/> when a lane admits the diff; otherwise, <see langword="false"/></returns>
    private static bool TryGetProductionLane(IReadOnlyCollection<PathCategory> categories, bool hasChangeDoc, out Lane lane, out string reason)
    {
        lane = Lane.Docs;
        reason = null;

        if (categories.Contains(PathCategory.Tooling) && hasChangeDoc == false)
        {
            reason = "production and tooling paths without a change doc fit neither the bug-fix nor the behavior-change lane";

            return false;
        }

        lane = hasChangeDoc ? Lane.BehaviorChange : Lane.BugFix;

        return true;
    }

    /// <summary>
    /// Derives the lane of a diff that holds tooling paths and no production path: the tooling lane, or behavior change
    /// next to wording, which no lane admits without a change doc
    /// </summary>
    /// <param name="hasWording">Whether the diff holds wording paths</param>
    /// <param name="hasChangeDoc">Whether the diff holds a change doc</param>
    /// <param name="lane">The narrowest admitting lane, when one exists</param>
    /// <param name="reason">Why no lane admits the diff, when none does</param>
    /// <returns><see langword="true"/> when a lane admits the diff; otherwise, <see langword="false"/></returns>
    private static bool TryGetToolingLane(bool hasWording, bool hasChangeDoc, out Lane lane, out string reason)
    {
        lane = Lane.Docs;
        reason = null;

        if (hasWording == false)
        {
            lane = Lane.Tooling;

            return true;
        }

        if (hasChangeDoc == false)
        {
            reason = "wording and tooling paths without a change doc fit neither the tooling nor the behavior-change lane";

            return false;
        }

        lane = Lane.BehaviorChange;

        return true;
    }

    /// <summary>
    /// Derives the lane of a diff that holds neither production nor tooling paths: wording needs the wording lane, or the
    /// tests lane next to test paths, which becomes behavior change with a change doc; otherwise a change doc needs
    /// behavior change, test paths the tests lane, and text alone the docs lane
    /// </summary>
    /// <param name="categories">Categories of every changed path</param>
    /// <param name="hasWording">Whether the diff holds wording paths</param>
    /// <param name="hasChangeDoc">Whether the diff holds a change doc</param>
    /// <returns>The narrowest admitting lane</returns>
    private static Lane GetTextLane(IReadOnlyCollection<PathCategory> categories, bool hasWording, bool hasChangeDoc)
    {
        if (hasWording)
        {
            if (categories.Contains(PathCategory.Test))
            {
                return hasChangeDoc ? Lane.BehaviorChange : Lane.Tests;
            }

            return Lane.Wording;
        }

        if (hasChangeDoc)
        {
            return Lane.BehaviorChange;
        }

        return categories.Contains(PathCategory.Test) ? Lane.Tests : Lane.Docs;
    }

    #endregion // Methods
}