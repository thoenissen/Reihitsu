Role: implementer
Commit: 36ac553b
Verdict: tests pass

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Changes
| File | What changed | Requirement |
|---|---|---|
| `Release.ruleset` | Added `Action="Error"` entries for RH5113, RH5308, RH5417, RH6023, RH6024, RH7110, RH7308, RH7309, RH7310, RH7311, RH7409, RH7410, RH7411, RH7412, each inserted in ID order next to its neighbours (BOM and LF line endings kept). RH7305A stays on the test's exemption list (lead decision 1). | issue #1890 "Done when"; `ReleaseRulesetListsEveryShippedRule`, `ReleaseRulesetReportsRuleAsError` |

The role's "no ruleset change" rule targets warning suppression. In this tooling run, the ruleset is the subject of the change, so this edit is the production change, not a suppression.

## Policy owners
`rg -l "Release.ruleset"` finds one consumer that turns it on, `Directory.Build.props` (Release configuration, with `TreatWarningsAsErrors`), plus the self-hosting tests. No other copy of the rule list exists.

## Production diff size
`git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'`: empty. Outside `src`, only `Release.ruleset` changed: 14 insertions. No new type.

## Deleted special cases
_N/A._

## Tests run
- `scripts/test.sh --project selfhosting --filter "FullyQualifiedName~Reihitsu.SelfHosting.Test.Repository.RulesetTests"`: before the change, `Failed: 15, Passed: 9, Total: 24` (as handed over). After it, `Passed! - Failed: 0, Passed: 24, Skipped: 0, Total: 24`.
- Build check, as the lead asked:
  - `scripts/build.sh` (Release): `Build succeeded. 0 Warning(s) 0 Error(s)`.
  - `dotnet build Reihitsu.sln -c Release --no-incremental` (a forced full recompile, so the 33 s build above was not an incremental skip): `0 Warning(s) 0 Error(s)`.
  - The self-hosting analyzers under `binaries/Reihitsu.Analyzer.dll` contain all 14 rule IDs, so they are live in this build. The repository's own source already satisfies them, and nothing needed fixing.
- `scripts/lane-check.sh tooling`: confirmed.

## Disputed tests or scope
_None._

## Architecture findings
_None._

## Follow-up candidates
_None._
