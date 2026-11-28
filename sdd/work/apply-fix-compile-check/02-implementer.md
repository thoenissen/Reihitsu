Role: implementer
Commit: 57bfc75422401d16e650a5757631e39850560db2
Verdict: tests pass

Tooling lane; no spec under `sdd/specs/` covers the fixture runner, so the Requirement column names the lead's decision or the issue. My prompt carried no intended fix or suspected cause.

## Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Tooling/FixtureRunner.cs` | New private `GetCompileErrorsAsync`: compiles the source in the same ad-hoc project `CreateDocument` builds (same references, language version, document path) and returns its errors, parse errors included, as an `ImmutableHashSet<(Id, Message)>` with invariant-culture messages. `RunAsync` collects the fixture's set once after the parse check and, after each applied fix, reports `InvalidResult` when the result's set is not a subset of the fixture's. This replaces the former `HasSyntaxErrors` check of the result; the fixture still parses (checked before), so every parse error of a result is new. `HasSyntaxErrors` stays for the fixture's own parse check. `languageVersion` parameter doc updated | #1977; `00-lead.md` decision 1 (set of ID and message) |
| `src/Reihitsu.Tooling/Enumerations/FixtureOutcome.cs` | `InvalidResult` doc: "no longer parses or compiles", new error by ID and message under the run's language version | #1977 |
| `src/Reihitsu.Tooling/CodeFixRunCommand.cs` | Two `languageVersion` param docs: "every parse and compilation" | doc only |
| `scripts/README.md` | `--lang-version` paragraph, the `invalid-result` row, and the exit-code paragraph describe the compile check instead of the parse-only check | #1977 |

The check runs after every iteration (as the parse check did), not only on the final result; the handover left that open.

## Policy owners
`rg -n "GetDiagnostics\(|HasSyntaxErrors|GetCompilationAsync" src/Reihitsu.Tooling` — `FixtureRunner.HasSyntaxErrors` (fixture parse check, unchanged), `FixtureRunner.AnalyzeAsync` (compilation for analyzers), and the new `FixtureRunner.GetCompileErrorsAsync`. No other copy of a fix-result validity check exists in the tooling project. The project setup has one owner, `CreateDocument`, which the new method reuses.

## Production diff size
`git diff --shortstat origin/main...HEAD -- src`: 3 files changed, 42 insertions(+), 6 deletions(-). No new type, no new public member.

## Deleted special cases
_N/A._

## Tests run
- `scripts/test.sh --configuration Debug --project tooling --filter "FullyQualifiedName~Reihitsu.Tooling.Test.Unit.FixtureRunnerCompilationTests"` — Passed: 11, Failed: 0
- `scripts/test.sh --configuration Debug --project tooling` — Passed: 238, Failed: 0
- `scripts/test.sh --configuration Debug --project architecture` — Passed: 198, Failed: 0
- `scripts/build.sh --configuration Debug` — 0 warnings, 0 errors

## Disputed tests or scope
_None._ Cost note: each fixture arm now compiles once more at the start and once more per applied fix (a fresh ad-hoc workspace each time); the tooling suite ran in 16 s.

## Architecture findings
_None._

## Follow-up candidates
_None._
