Role: implementer
Commit: see the commit that adds this file (on top of 1904c09)
Verdict: tests pass

CI repair round 1 (SonarCloud new-code coverage). The prompt carried no intended fix, no suspected cause, and no hoped-for verdict beyond decision 18, which is the lead's recorded decision and which I carried out.

## Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/Pipeline/BlankLines/Rewriter/BlankLineTriviaBoundaryRewriter.cs` | Removed the trailing-trivia `#endregion` path: the predicate `HasEndRegionDirectiveInTrailingTrivia`, the method `EnsureBlankLineBeforeEndRegionsInTrailingTrivia`, and its dispatch in `VisitToken`. These are the 21 lines SonarCloud reported (301–345, 377). Leading-trivia `#endregion` handling through `BlankLineEditor.EnsureBlankLineBeforeRegionDirectives` is unchanged. `main` held the same path as `EnsureBlankLineBeforeFirstEndRegionInTrailingTrivia`, so this deletes it outright and does not just revert the run's rewrite of it. | FMT-BLANK-013 / SHARED-BLANK-005, unchanged. Decision 18; *Supported inputs* (no caller reaches the path) |

No other production file changed. `BlankLineEditor.cs` is unchanged; see *Disputed tests or scope*.

FC-031: the class summary ("blank-line boundaries before comments and #endregion directives") still holds for the leading-trivia paths. No summary or comment names the removed members: `rg -n "EnsureBlankLineBeforeEndRegionsInTrailingTrivia|HasEndRegionDirectiveInTrailingTrivia|EnsureBlankLineBeforeFirstEndRegion" --glob '!sdd/work/**' .` finds nothing. No spec text describes the trailing path.

## Policy owners
- `rg -n "IsMissingRequiredBlankLineBefore" src --glob '!*Test*'` finds the Core decision `RegionDirectiveBlankLineUtilities.IsMissingRequiredBlankLineBefore(bool, bool, bool)` (with the text overload delegating to it, called by RH5031's analyzer and code fix base), and the formatter reader `BlankLineEditor.IsMissingRequiredBlankLineBeforeRegionDirective`. After the deletion, the reader has one caller, `BlankLineEditor.EnsureBlankLineBeforeRegionDirectives`, which `BlankLineRegionDirectiveRewriter` and `BlankLineTriviaBoundaryRewriter` call. That still makes one owner of the decision. The reader is still `public static`. It could now be `private`, but I left the visibility alone because narrowing it is a change the coverage finding does not need.

## Tests run
All in Debug, on the working tree with the deletion:
- `scripts/test.sh --configuration Debug --project formatter`: `Passed! - Failed: 0, Passed: 4402`
- `--project analyzer`: `Passed! - Failed: 0, Passed: 6411`
- `--project cli`: `Passed! - Failed: 0, Passed: 251`
- `--project core`: `Passed! - Failed: 0, Passed: 607`
- `--project architecture`: `Passed! - Failed: 0, Passed: 92`
- `scripts/format.sh` on the changed file: `Formatted 0 of 1 file(s)`. `scripts/format.sh src --check`: `0 of 1843 file(s) need formatting`.

Reachability probe for `BlankLineEditor.cs:98`: I temporarily instrumented line 98 to throw `PROBE-LINE-98`, rebuilt, ran the checks below, and then restored the file byte for byte. The tree carries no instrumentation.
- The formatter, CLI, and analyzer suites against the instrumented build passed 4402 / 251 / 6411, with no `PROBE-LINE-98`. **No existing suite input reaches the line.**
- The CLI (`scripts/format.sh <fixture> --dry-run`) on this input threw `PROBE-LINE-98`, so **the line is reachable through the CLI**. In the input below, the line between `_a;` and `#region` holds four spaces:

```csharp
namespace N;

internal class C
{
    private int _a;
    
    #region Methods

    public void M()
    {
    }

    #endregion // Methods
}
```

## Disputed tests or scope

**`BlankLineEditor.cs:98` needs no code change, but it needs a test, and none can come from existing inputs.** Line 98 is the step of the whitespace skip in `IsMissingRequiredBlankLineBeforeRegionDirective`. It runs when the line directly above a region directive is a whitespace-only line, which is common in hand-written files with trailing spaces. The skip is load-bearing. Without it, the reader would see the `WhitespaceTrivia` instead of the line break in front of it, would read the blank line above as a content line, and would insert a second blank line. On the real build the probe input gives the correct result: the whitespace-only line stays the one blank line (cleanup strips its spaces), and nothing is inserted. That is the same as `main`, whose `HasBlankLineImmediatelyBeforeIndex` skipped whitespace the same way. So this is not a defect and not a candidate for deletion. No existing suite input reaches it (probe above), so as instructed I wrote no test. A regression test would be a `RegionDirectiveLineAboveTests` case with the input above, and its expected output is the input with the whitespace-only line emptied. The output is `main`'s output, so nothing visible is in question. That routing is the lead's call (test implementer).

Coverage estimate, not measured: SonarCloud reported 23 uncovered new lines at 62.7 %. That puts roughly 62 new coverable lines in its count. The deletion removes 21 of them, all uncovered, which leaves about 41 lines with 2 uncovered (line 98 and the one other line the lead did not attribute), or about 95 %. That would clear the 80 % gate even without a test for line 98, if SonarCloud's condition coverage does not shift the figure much.

No test disputed.

## Architecture findings
_None._

## Follow-up candidates
_None._
