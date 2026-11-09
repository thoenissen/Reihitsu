Role: spec author
Commit: d2745cf (head, includes ba93a6f); changes committed with this handover
Verdict: #1597: spec written; #1598: nothing to do; #1016: nothing to do; #1015: nothing to do; #1084: nothing to do

Repair round 1, small follow-up, lead decision 17. The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the decision itself. Wording only; no requirement change, no new ID, no change doc, no rule page change.

## thoenissen/Reihitsu#1597

### Changed requirements

- SHARED-CASING-012 — The fix is withheld when a caller-argument expression names the old or the new name — clarified (`sdd/specs/shared/casing.md`), normative text unchanged (the C# 11 condition stays):
  - *Why*: removed "before C# 11 it refers to a member of the type, such as a `const` of the same name, which the rename leaves as it is", which `12-test-implementer.md` showed false on this repository's compiler. Now: the exemption relies on the scope C# 11 gives a `nameof` in a parameter-list attribute; for an earlier language version the exemption is not taken, the check does not decide what the operand refers to, and the fix is refused from the document's language version alone, a conservative refusal.
  - C# 10 example's closing sentence: removed "before C# 11, `nameof(Condition)` refers to the field `Guard.Condition` … so the attribute would name no parameter"; now "before C# 11 the argument is not exempt, so the fix is refused without deciding what `nameof(Condition)` refers to." The C# 11 side of the sentence is kept (backed by the green C# 11 test).
- `grep -rn "before C# 11\|C# 10" sdd/specs`: no other sentence asserts how `nameof` binds before C# 11 (RH4105.md, RH4120.md refer to SHARED-CASING-012 without restating it).

### Examples for the tests

None new. The existing tests for example (d) and its C# 11 counterpart assert the unchanged requirement.

### Output questions

_None._

### Internal choices

1. Kept the statement that from C# 11 an unqualified `nameof` in a parameter-list attribute refers to the parameter, since the C# 11 test and the scratchpad check of `11-spec-author.md` back it. Alternative: drop every binding statement — rejected, the exemption then has no stated reason.

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1598

Nothing to do in this round.

## thoenissen/Reihitsu#1016

Nothing to do in this round.

## thoenissen/Reihitsu#1015

Nothing to do in this round.

## thoenissen/Reihitsu#1084

Nothing to do in this round.

## Whole run

- Files: `sdd/specs/shared/casing.md`.
- `scripts/test.sh --configuration Debug --project architecture` → Passed 117, Failed 0.
