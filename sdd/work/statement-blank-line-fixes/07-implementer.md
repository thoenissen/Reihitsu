Role: implementer
Commit: (this commit; parent e9c0361)
Verdict: tests pass

Scope: decisions #19 and #20 of `00-lead.md`, both cleanup of the run's own change; no issue's behavior changes. My prompt carried no intended fix or hoped-for verdict beyond the two decisions. No test file touched.

## thoenissen/Reihitsu#946

Nothing to do in this round (see *Whole run*).

## thoenissen/Reihitsu#972

Nothing to do in this round (see *Whole run*).

## thoenissen/Reihitsu#1096

Nothing to do in this round (see *Whole run*).

## thoenissen/Reihitsu#1076

Nothing to do in this round (see *Whole run*).

## thoenissen/Reihitsu#1626

Nothing to do in this round (see *Whole run*).

## thoenissen/Reihitsu#1625

Nothing to do in this round (see *Whole run*).

## Whole run

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Core/SyntaxTriviaUtilities.cs` | Deleted `ContainsBlankLine` (no caller left; its tests went in 859aced) | decision #19 (decision #8: one blank-line measure) |
| `src/Reihitsu.Core/RegionDirectiveBlankLineUtilities.cs` | New `RequiresBlankLineBelow(bool isEndRegion, bool doesLineBelowBeginWithCloseBrace)`: the one statement of the `#endregion`-before-`}` exemption, fact-based like `IsMissingRequiredBlankLineBefore(bool, bool, bool)`. `IsMissingRequiredBlankLineAfter` now takes `isEndRegion` and the full close-brace line set and applies it. `FollowsRegionDirectiveRequiringBlankLine` applies it too; its target fact is now token-based (the target line's first token is a `}`), the same fact RH5032 reads, instead of the line's first character. Private `GetContentStart` shares the line-content position both lookups need. | SHARED-BLANK-006, decision #20 |
| `src/Reihitsu.Analyzer/Base/RegionDirectiveBlankLineAnalyzerBase.cs` | Dropped the per-directive choice between the close-brace set and an empty set (the second copy of the exemption); passes `isEndRegion` instead. Unused `System.Collections.Generic` using removed. | SHARED-BLANK-006 |
| `src/Reihitsu.Analyzer.CodeFixes/Base/RegionDirectiveBlankLineCodeFixProviderBase.cs` | Same as the analyzer base (it held a third copy of the selection). Unused using removed. | SHARED-BLANK-006 |

No output change: RH5032 computes the same boolean as before (`#region` → exemption set empty ≡ `isEndRegion == false`). For RH5024–RH5027 the target line always starts with the target token (`}`, `{`, `else`/`catch`/`finally`, `while`), so "first character is `}`" and "first token is a `}`" agree on every input those rules pass.

### Policy owners
`rg -n "EndRegionDirectiveTrivia|IsMissingRequiredBlankLineAfter|RequiresBlankLineBelow|closeBraceStartLineIndices" src --glob '!*Test*/**'`:
- `RegionDirectiveBlankLineUtilities.RequiresBlankLineBelow` — the analyzers' owner (RH5032 via `IsMissingRequiredBlankLineAfter`, RH5024–RH5027 via `FollowsRegionDirectiveRequiringBlankLine`).
- `BlankLineRegionDirectiveRewriter.EnsureBlankLineAfterRegionDirectives` (formatter) — states the exemption a third time on trivia (`ownsClosingBoundary && EndRegionDirectiveTrivia`, also covering end of file). Outside decision #20's scope; see *Follow-up candidates*.

### Production diff size
This round: `git diff --shortstat -- src` = `4 files changed, 51 insertions(+), 73 deletions(-)`. No new type; one new public Core member (`RequiresBlankLineBelow`), one public signature change (`IsMissingRequiredBlankLineAfter` gains `bool isEndRegion`; both callers are in this diff), one private helper. One public Core member deleted (`ContainsBlankLine`).
Run total: `git diff --shortstat origin/main -- src ':(exclude,glob)src/*Test*/**'` = `24 files changed, 432 insertions(+), 275 deletions(-)`.

### Deleted special cases
_N/A._

### Comments (FC-031)
Summaries of `IsMissingRequiredBlankLineAfter` and `FollowsRegionDirectiveRequiringBlankLine` rewritten to point at `RequiresBlankLineBelow`; the callers' bodies carry no comments.

### Tests run
```shell
scripts/format.sh <the 4 files>                               # Formatted 0 of 4 file(s)
scripts/build.sh --configuration Debug                        # 0 Warning(s), 0 Error(s)
scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH5024|…RH5025|…RH5026|…RH5027|…RH5031|…RH5032"
#   Passed: 147, Failed: 0
scripts/test.sh --configuration Debug --project analyzer      # Passed: 8211, Failed: 0
scripts/test.sh --configuration Debug --project core          # Passed: 655, Failed: 0
scripts/test.sh --configuration Debug --project formatter     # Passed: 4837, Failed: 0
scripts/test.sh --configuration Debug --project architecture  # Passed: 117, Failed: 0
```

### Disputed tests or scope
_None._

### Architecture findings
_None new._

### Follow-up candidates
- The formatter's `BlankLineRegionDirectiveRewriter.EnsureBlankLineAfterRegionDirectives` could call `RegionDirectiveBlankLineUtilities.RequiresBlankLineBelow` the way `BlankLineEditor` already calls `IsMissingRequiredBlankLineBefore(bool, bool, bool)`, making SHARED-BLANK-006's exemption one statement across analyzer and formatter. Small (one condition), no output change expected; not done because decision #20 names only `RegionDirectiveBlankLineUtilities`.
