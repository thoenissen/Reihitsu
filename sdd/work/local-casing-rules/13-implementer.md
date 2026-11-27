Role: implementer
Commit: see the commit that adds this handover (base 6db59b3b)
Verdict: tests pass

Repair round 2, decision 8 of `00-lead.md` (auditor finding 2, the architecture finding of `11-auditor.md`). Inputs: `00-lead.md`, `05-implementer.md`, `11-auditor.md`, `12-test-implementer.md`. The prompt carried no intended fix beyond pointing at decision 8, which is the lead's recorded decision.

## Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer.CodeFixes/Base/CasingAnonymousMemberUtilities.cs` | `GetMemberNamedByReference` no longer walks member access, member binding, and conditional access by hand. It takes the nearest `AnonymousObjectMemberDeclaratorSyntax` around the reference and returns it when it has no `NameEquals`, its expression's last token is the reference token, and `SyntaxFacts.TryGetInferredMemberName` returns a name for the expression. XML summary and remarks rewritten to describe this (FC-031). `WriteMemberName` unchanged; it already reads the name from the expression's last token. | SHARED-CASING-015; FC-044 |

## Policy owners
`rg -n "TryGetInferredMemberName|AnonymousObjectMemberDeclaratorSyntax" src --type cs`:
- `CasingAnonymousMemberUtilities.GetMemberNamedByReference` and `RH2001…CodeFixProvider.GivesInferredMemberNameAsync` both now read the compiler's inferred-name rule through `SyntaxFacts.TryGetInferredMemberName`; nothing restates that rule anymore.
- `RH3002…Analyzer.cs:339` (parentheses inside an unnamed member), `LineBreakAssignmentRewriter`, and `JoinedGapUtilities` (layout of `Name =`) handle other decisions.

## Production diff size
`git diff --shortstat origin/main...HEAD -- src` before this round: 15 files changed, 494 insertions(+), 84 deletions(-). Delta of this round: 1 file changed, 9 insertions(+), 17 deletions(-). No new type, public member, or dependency.

## Deleted special cases
_N/A._ (No shape is declared unsupported. The deleted hand walk is the second statement of the compiler's rule that finding 2 names.)

## Tests run
- `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Naming"`: `Failed: 0, Passed: 1004, Total: 1004`
- `scripts/test.sh --configuration Debug --project analyzer`: `Failed: 0, Passed: 12349, Total: 12349`
- `scripts/test.sh --configuration Debug --project architecture`: `Failed: 0, Passed: 198, Total: 198`
- Mutations, each reverted at once (`git diff --stat` confirmed afterwards):
  - Replacing the last-token check with `true` makes `RH4106…FixWritesNoNameForAnAnonymousObjectMemberWhoseConditionalAccessReceiverIsTheField` fail (69 of 70). The check is guarded.
  - Replacing the `TryGetInferredMemberName` check with `true` fails no test (1004 of 1004). This is expected. In compiling code, every anonymous member without `Name =` has an inferred name, or the compiler reports CS0746. So no compiling input separates this check from the last-token check: `x + Value`, `(Value)`, `-Value`, and `p->Value` all fail to compile as unnamed members. The check stays as the decision states. It reads the compiler's rule instead of assuming it, and no caller reaches an input that would need a test (*Supported inputs*).

## Disputed tests or scope
_None._

## Architecture findings
_None._ Finding 2 is resolved: both fixes read the one compiler rule.

## Follow-up candidates
_None_ new. RH2001-011 (already in `00-lead.md`) could now reuse `CasingAnonymousMemberUtilities` directly, since both read the same rule.
