Role: implementer
Commit: 62bc4d2b8b03da4055e7cdc5f4c642f00264d4e0
Verdict: tests pass

Repair round 1, tooling lane; no spec covers the fixture runner, so the Requirement column names the lead's decision or the audit finding. My prompt carried no intended fix or suspected cause.

## Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Tooling/FixtureRunner.cs` | `GetCompileErrorsAsync` now returns `ImmutableDictionary<string, int>`: per error ID (ordinal), the number of distinct invariant-culture messages. `RunAsync` reports `InvalidResult` when some ID of the result has a count above the fixture's (`GetValueOrDefault`, so a missing ID counts from zero), replacing `IsSubsetOf`. Summary, `<returns>`, and the inline comment above the check restated for the count, renames included | `00-lead.md` decision 2; audit #1 |
| `src/Reihitsu.Tooling/Enumerations/FixtureOutcome.cs` | `InvalidResult` summary: more distinct messages for some error ID than the fixture, a missing ID from zero | decision 2 (FC-031) |
| `scripts/README.md` | `invalid-result` row restated for the per-ID count; repeats and renames do not count. The `--lang-version` and exit-code paragraphs needed no change | decision 2 |

`RunAsync`'s `languageVersion` parameter doc and both `CodeFixRunCommand` param docs do not describe the comparison key and stay as they are.

## Policy owners
`rg -n "GetDiagnostics\(|HasSyntaxErrors|GetCompilationAsync|InvalidResult|IsSubsetOf" src/Reihitsu.Tooling` — the validity check has one owner, `FixtureRunner.RunAsync` with `GetCompileErrorsAsync`; `CodeFixRunCommand` only maps the outcome. `rg "ID and message"` over `src/Reihitsu.Tooling` and `scripts/README.md` finds no remaining source text of the old key (only stale `bin/` XML doc outputs).

## Production diff size
`git diff --shortstat origin/main...HEAD -- src`: 3 files changed, 47 insertions(+), 6 deletions(-). Round delta (`src` only): 2 files changed, 20 insertions(+), 15 deletions(-). No new type, no new public member.

## Deleted special cases
_N/A._

## Tests run
- `scripts/test.sh --configuration Debug --project tooling --filter "FullyQualifiedName~Reihitsu.Tooling.Test.Unit.FixtureRunnerCompilationTests"` — Passed: 16, Failed: 0 (the three rename tests failed before the change, as `06-test-implementer.md` says)
- `scripts/test.sh --configuration Debug --project tooling` — Passed: 243, Failed: 0
- `scripts/test.sh --configuration Debug --project architecture` — Passed: 198, Failed: 0
- `scripts/build.sh --configuration Debug` — 0 warnings, 0 errors

## Disputed tests or scope
_None._

## Architecture findings
_None._

## Follow-up candidates
_None._
