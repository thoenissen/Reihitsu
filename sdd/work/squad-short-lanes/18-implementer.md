Role: implementer
Commit: 80109c0 (base of this round; the round's commit carries this handover)
Verdict: tests pass

Repair round 2 of decision 26: RH2008 on `src/Reihitsu.Tooling/LaneCheckCommand.cs:178` (`TryParseArguments`, 65 > 60 lines), reported by the self-hosting line-count row in 17-validator. My prompt named no intended fix beyond "repair without changing behavior"; the repair below is my own choice.

## Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Tooling/LaneCheckCommand.cs` | The validation that follows the argument loop of `TryParseArguments` (help short-circuit, `--wording-paths` exclusivity, `--lane` parse, `--text-only` parse) moved unchanged into a new private method `TryValidateArguments(laneName, textOnly, showHelp, listWordingPaths, out claimed, out isTextOnlyProven, out error)`; `TryParseArguments` keeps the loop and returns the helper's result. Same order of checks, same error texts, same out values (`claimed` stays `Lane.Docs` and `isTextOnlyProven` `false` on help, `--wording-paths`, and every error). XML summary written for the new method; `TryParseArguments`' summary still describes it (FC-031). | none — RH2008 line limit (self-hosting), no behavior change |

## Policy owners
`rg -n "TryParseLane|--wording-paths takes" src --glob '!**/*Test*/**'`: lane-name parsing has one owner, `LaneClassifier.TryParseLane` (`LaneClassifier.cs:188`), called only from the new `TryValidateArguments`; the `--wording-paths` exclusivity check exists only there. No copy introduced.

## Production diff size
`git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'` (before this round): 4 files changed, 114 insertions(+), 16 deletions(-).
Delta of this round: 1 file changed, 26 insertions(+) (one new private method, mostly its XML documentation and signature). No new type.

## Deleted special cases
_N/A._

## Tests run
- `scripts/format.sh src/Reihitsu.Tooling/LaneCheckCommand.cs` — Formatted 0 of 1 file(s).
- `scripts/build.sh` (Release) — Build succeeded, 0 Warning(s), 0 Error(s).
- `scripts/test.sh --no-build --project tooling` — Passed! Failed: 0, Passed: 204, Skipped: 0, Total: 204.
- `scripts/test.sh --no-build --project selfhosting --filter "Name~RH2008|Name~RH2111"` (the line-count rows, `scripts/README.md`) — Passed! Failed: 0, Passed: 18, Skipped: 0, Total: 18, 41 s. The previously failing `AnalyzerReportsNoViolationsOnOwnSourceCode("RH2008MethodsShouldNotExceedTheLineLimitAnalyzer")` now passes.

Logs: `/tmp/claude-0/-home-user-Reihitsu/ba32ea5f-3902-5d32-8481-e4d5998124ec/scratchpad/build18.log`, `tooling18.log`, `linecount18.log` in the same directory.

## Disputed tests or scope
_None._

## Architecture findings
_None._

## Follow-up candidates
_None_ beyond the lead's existing candidate 2 (line-count rows for production files), which this finding already strengthens.
