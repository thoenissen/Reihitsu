Role: implementer
Commit: 5c2b67b
Verdict: tests pass

Repair round 3, worklist: audit finding #13 of `24-auditor.md` only (decision 16 in `00-lead.md`). The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the audit's suggested change.

## Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Core/OrderingDeclarationUtilities.cs` | `IsLayoutField` and `HasInitializer` changed from `public` to `private`, and moved unchanged (summaries, bodies) behind `FieldOrderSetsLayout`, so the file keeps its public → internal → private member order. No body changed; both summaries still describe the code | Audit #13 (FC-015); decision 15's narrowing of the member-order comparison |

## Policy owners
`rg -n "HasInitializer\b|IsLayoutField" src`: the only callers are the private `ChangesMemberOrder(…, bool fieldOrderSetsLayout, Func<int, int>)` overload in `OrderingDeclarationUtilities` (lines 440–442 before the change). The one other hit is the RH7109 test method name `ReadonlyFieldWithInitializerIsFixedWhenNoPassedOverFieldHasInitializer`, which does not call the member. The member-order policy keeps its one owner, `OrderingDeclarationUtilities.ChangesMemberOrder`; none of its building blocks is reachable from another assembly now, except `ChangesInitializerExecutionOrder` (internal, Core tests only, decision 15).

## Tests run
- `scripts/format.sh src/Reihitsu.Core/OrderingDeclarationUtilities.cs`: Formatted 0 of 1 file(s).
- `scripts/build.sh --configuration Debug`: 0 Warning(s), 0 Error(s). (A first attempt failed with MSB3030 "could not copy …xml" errors, a file race with the concurrent test implementer's build in the shared `bin/` folders; the rerun was clean.)
- `scripts/test.sh --configuration Debug --project core`: Passed! Failed: 0, Passed: 607, Total: 607.
- `scripts/test.sh --configuration Debug --project analyzer`: Passed! Failed: 0, Passed: 6635, Total: 6635.

No test file touched; no assertion changed.

## Disputed tests or scope
_None._

## Architecture findings
_None._

## Follow-up candidates
_None._
