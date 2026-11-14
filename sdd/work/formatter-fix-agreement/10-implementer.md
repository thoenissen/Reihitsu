Role: implementer
Commit: (this commit)
Verdict: test disputed

Repair round 1, worklist item (b). My prompt carried the auditor's suggested change (delete the flag, its boundary branch, keep `RootContinuesChain`); I re-derived it from finding #1 and the changed FMT-PIPE-024 rather than taking it as given. FMT-PIPE-024 now names only the chain-continuation fact and leaves an assignment behind `?` as the root of `FormatNodeInDocumentAsync` undecided, so the second fact has no requirement left.

## thoenissen/Reihitsu#1529

### Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Core/FluentChainBoundary.cs` | `RootIsConditionalAccessBranch`, its constructor parameter and its argument in `From` deleted; type summary describes the one remaining fact | FMT-PIPE-024 |
| `src/Reihitsu.Core/FluentChain.cs` | `IsConditionalAccessBranch` (and its `node.Parent == null` boundary branch) deleted; its one remaining read is inlined into the assignment arm of `ContinuesChain` as `assignment.Parent is ConditionalAccessExpressionSyntax conditionalAccess && conditionalAccess.WhenNotNull == assignment`; summary updated: an assignment that is the root has no branch above it, so the arm answers `false` there, as the default boundary did before | FMT-PIPE-024, SHARED-CHAIN-001 |

### Policy owners
`rg -n -i "ConditionalAccessBranch|conditional.access branch|WhenNotNull ==" -g '*.cs' src` (excluding bin/obj): only the inlined arm in `FluentChain.ContinuesChain` remains. `rg -n "FluentChainBoundary\(" -g '*.cs' src`: only `FluentChainBoundary.From`.

### Production diff size
Run total: `7 files changed, 27 insertions(+), 98 deletions(-)` (was `5 files changed, 17 insertions(+), 64 deletions(-)`). Round delta: `2 files changed, 10 insertions(+), 34 deletions(-)`. No new type or public member; the public constructor `FluentChainBoundary(bool, bool)` becomes `FluentChainBoundary(bool)` (its only caller is `From`).

### Deleted special cases
_N/A._ No shape declared unsupported.

### Tests run
- `scripts/build.sh --configuration Debug`: 0 warnings, 0 errors.
- `scripts/test.sh --configuration Debug --project core`: Passed 651/651.
- `scripts/test.sh --configuration Debug --project analyzer`: Passed 5207/5207.
- `scripts/test.sh --configuration Debug --project architecture`: Passed 117/117.
- `scripts/test.sh --configuration Debug --project formatter`: Failed 1, Passed 4849 — the disputed test below; it passed before this change.

### Disputed tests or scope
- `src/Reihitsu.Formatter.Test/Unit/ReihitsuFormatterDetachedRootTests.cs`, `ConditionalAccessAssignmentTargetKeepsTheWrappedLinkOfItsAssignedMember` (tagged FMT-PIPE-023, FMT-PIPE-024; on `main` since before this run). It calls `FormatNodeInDocumentAsync` with the assignment `a?.B⏎ .C = 5` (the `WhenNotNull` of a conditional access) as target and asserts `.C` stays on its own line. That is exactly the root the changed FMT-PIPE-024 says no caller passes and the requirement does not decide; it was the only consumer of the deleted fact, so `08-auditor.md` ("no test sets it") missed it — the auditor searched for `FluentChainBoundary`, and this test reaches the fact through the entry point. After the deletion the output is `a?.B.C = 5;`. Under `AGENTS.md`, *Supported inputs*, a case no caller reaches needs no test; the test either goes (my reading — its other asserted facts are covered by the FMT-PIPE-023 siblings and by RH5111-006 tests through the context entry point) or the spec row comes back. I did not change the test. Route to the test implementer.

### Architecture findings
_None._

### Follow-up candidates
_None._

## thoenissen/Reihitsu#1543

Nothing to do in this repair round.
