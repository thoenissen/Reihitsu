Role: implementer
Commit: see the commit that adds this file (subject "Compute each initializer value's edge tokens once per declaration [skip ci]")
Verdict: tests pass

The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the run's own handovers and the issue's goal, which `00-lead.md` decision 1 fixes as the scope.

## Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/Pipeline/Precheck/PrecheckPhase.cs` | `FindCommentInDeclaration` builds, once per declaration, a list of `(Span, FirstToken, LastToken)` tuples for its initializer values and compares each token against the cached edge tokens instead of calling `value.GetFirstToken()`/`GetLastToken()` per token. The lookup stays a linear `FindIndex` over the values (term 3 kept, decision 2); the nested-declaration re-walk stays (decision 2). XML summary extended with why the edge tokens are taken once (FC-031). | FMT-UNSUP-004 (verdicts unchanged) |

## Policy owners
`rg -n "GetFirstToken\(\)|GetLastToken\(\)" src/Reihitsu.Formatter/Pipeline/Precheck/` — the only value-edge lookup for FMT-UNSUP-004 is the one changed; the two other hits (lines 213, 215) are the FMT-UNSUP-008 conditional check, one call per conditional, not per token. No other owner of the declaration scan.

## Production diff size
`git diff --shortstat` over `src` before commit: `1 file changed, 6 insertions(+), 4 deletions(-)`. No new type (a value tuple).

## Deleted special cases
_N/A._

## Tests run
- `scripts/test.sh --configuration Debug --project formatter --filter "FullyQualifiedName~Reihitsu.Formatter.Test.Unit.Precheck"` — `Passed! - Failed: 0, Passed: 249`
- `scripts/test.sh --configuration Debug --project formatter` — `Passed! - Failed: 0, Passed: 5257`
- `scripts/build.sh --configuration Debug` — succeeded, no warning in `PrecheckPhase.cs`.
- `scripts/format.sh` on the changed file — no change.

The tests are characterization tests (time-only change); they passed before and after, as the test implementer's handover expects. Timing was not re-measured; the skeptic's `declCached` variant in the harness is the same transformation.

## Disputed tests or scope
_None._

## Architecture findings
_None._

## Follow-up candidates
_None_ beyond those already in `00-lead.md`.
