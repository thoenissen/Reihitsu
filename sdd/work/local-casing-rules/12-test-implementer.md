Role: test implementer
Commit: 2e67119e (base; this handover's commit follows it)
Verdict: tests written

Repair round 2, decision 9 of `00-lead.md`: the cells of SHARED-CASING-015 that `11-auditor.md` finding 1 names as missing. Inputs: `00-lead.md`, `04-test-implementer.md`, `06-test-implementer.md`, `11-auditor.md` (finding 1), `sdd/specs/shared/casing.md` (SHARED-CASING-015). The prompt carried no intended fix or hoped-for verdict. No production code changed; no existing test changed.

Expected values come from SHARED-CASING-015: a member whose name the compiler takes from the renamed reference — the reference ends the member access or conditional access that is the member's whole expression — gets `Name = ` with its old name; a member whose name the compiler takes from another identifier (`Length` in `new { Value.Length }`) gets no name.

## Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH4106PrivateFieldCasingAnalyzerTests.FixKeepsTheNameOfAnAnonymousObjectMemberNamedByTheTailOfAConditionalAccess` — `new { other?._next.Value }`, `new { other?._next?.Value }`, `new { other!.Value }` → `Value = other?._next._value`, `Value = other?._next?._value`, `Value = other!._value` | SHARED-CASING-015 | `Verify(source, fixed)` (single fix, Fix All, recompile) | passes |
| `RH4106PrivateFieldCasingAnalyzerTests.FixWritesNoNameForAnAnonymousObjectMemberWhoseConditionalAccessReceiverIsTheField` — `new { Text?.Length }`, `new { this.Text?.Length }`, `new { other?._next.Text.Length }` → `_text?.Length`, `this._text?.Length`, `other?._next._text.Length`, no name written | SHARED-CASING-015 | `Verify(source, fixed)` | passes |

Both pass on this head, as they must (existing behavior; the auditor's probe showed the same outputs). Watched failing for the right reason by temporary mutations of `src/Reihitsu.Analyzer.CodeFixes/Base/CasingAnonymousMemberUtilities.cs`, each reverted at once (`git diff --quiet src` confirmed before the final run):

- `&& conditionalAccess.WhenNotNull == expression` replaced by `&& true` (the boundary finding 1 names): only `FixWritesNoName…ConditionalAccessReceiverIsTheField` fails (69 of 70 RH4106 tests pass).
- `while (expression.Parent is ConditionalAccessExpressionSyntax …)` replaced by `if (…)` (one climb only): only `FixKeepsTheName…TheTailOfAConditionalAccess` fails (nested `other?._next?.Value`; 1003 of 1004 naming tests pass).

These tests guard the implementer's refactor of decision 8 (reading `SyntaxFacts.TryGetInferredMemberName`): both are expressed in outputs, not in the walk.

## Scenarios (bug fix)
_N/A — behavior-change repair round._

## Not covered, and why
- One rule only (RH4106): every casing fix reaches `CasingAnonymousMemberUtilities.GetMemberNamedByReference` through the shared base (`rg "GetMemberNamedByReference|NameAnonymousObjectMembersAsync" src` — the single caller is `CasingCodeFixProviderBase{T}`), so the cells are rule-independent; RH4106 is where the auditor's probe ran and where the other conditional-access test lives.
- Pointer member access `new { p->Value }` (auditor's finding 2 cell): does not compile as an anonymous member without a name, so no caller produces it (*Supported inputs*).
- Line endings: the naming edit inserts tokens on one line and moves no line; the CRLF case of SHARED-CASING-015 is covered by `RH4115…FixKeepsTheLayoutAndCommentsOfAMultiLineAnonymousObjectMember`.

## Commands
```shell
scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Naming.RH4106"
scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Naming"
```
Result on this head: `Failed: 0, Passed: 1004, Total: 1004`.

## Follow-up candidates
_None._
