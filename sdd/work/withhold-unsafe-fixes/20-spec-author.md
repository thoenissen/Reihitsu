Role: spec author
Commit: 1817f3b
Verdict: spec written

Repair round 2 (lead decisions 12 and 13). Worklist: audit finding #10 (`18-auditor.md`), and the documentation side of finding #9. The prompt carried no intended fix or hoped-for verdict beyond decisions 12 and 13, which I checked against SHARED-FIX-003 and the measured outputs below.

## Changed requirements

- SHARED-ORDER-007 — Initializer execution order is kept — changed. The normative sentence now refuses a move when *a member that the move moves* has a runtime initializer and a crossed member of its context has one. Two bullets state what counts as moved and as crossed. A member of excluded code in the crossed span is crossed. A member of excluded code in the moved declaration's own leading trivia moves with it (SHARED-ORDER-002) and crosses what the declaration crosses, also when the declaration has no initializer. A paragraph states that the layout guard of SHARED-FIX-003 *Run time* counts instance fields the same way, and that a static member reorders instance fields only through such excluded code, and only when the move crosses an instance field. The old sentence "Excluded code in the moved declaration's own leading trivia travels with it … and is not crossed" is gone. The *Why* now explains why excluded code that moves counts as moved. Examples added: fixture a (RH7109, `_b` without initializer, `_c = Log("c")` in its trivia, crossing `_a = Log("a")`: not offered), and fixture h (RH7103, struct, excluded `_y` in `_s`'s trivia, crossing `_x`: not offered). The positive example (crossed `_a`, `_d` without initializers) keeps its code; its explanation now gives the condition.
- RH7103-007 — The fix is withheld when the move is unsafe — changed. It adds "or when it would reorder the instance fields of a type whose field order sets its layout (SHARED-FIX-003)", as RH7109-006 states it, and that a static member does this only through excluded instance fields that move with it (SHARED-ORDER-007). Fixture h is the example, and the text says the fix is offered without the excluded field. SHARED-FIX-003 is added to *Uses*.
- RH7109-006: unchanged. Its layout clause, read with the new SHARED-ORDER-007 counting, already covers fixture c.
- Change doc `sdd/changes/2026-10/withhold-unsafe-fixes.md` (stays `Accepted`). The RH7103 and RH7109 rows of *What* now say that excluded code in front of the moved member moves with it and crosses what it crosses, for initializers and, in a layout type, for instance fields. Case 10 is renamed and its wrong sentence ("stays offered … when the `#if` pair stands directly in front of the moved member") is corrected. It gains three inputs with their `main` outputs: fixture a (RH7109 initializer), fixture c (RH7109 layout: `struct`, `static readonly _r` with excluded `_y` in its trivia, crossing `_s` and `_x`), and fixture h (RH7103 layout). The decision "Excluded code counts in the initializer guard" becomes "… initializer and layout guards". It records that the approved SHARED-FIX-003 *Build configurations* bullet demands every withheld fix of case 10, so no new question was asked, and that `main` offers every fix of case 10. Decision "One shared condition" now names RH7103 with RH7109 for the layout guard, and says that SHARED-ORDER-007 states only which members move and which are crossed.

Before outputs: `scripts/apply-fix.sh RH7109`/`RH7103` on a temporary worktree of `a59a003` (removed afterwards), LF and CRLF alike: "fixed (1 action(s), 1 iteration(s))" for a, c, h. On this branch at `1817f3b`, a and c give no-fix-offered. h is still fixed, which is finding #9. Decision 12's structural change has not landed yet. The counter-example `struct S { int _x; static int _s; }` is fixed on the branch, as RH7103-007 now states.

`scripts/test.sh --project architecture --filter Sdd`: "Passed!  - Failed: 0, Passed: 51, Skipped: 0, Total: 51".

## Examples for the tests

- SHARED-ORDER-007, second negative example (fixture a, RH7109): no code fix.
- SHARED-ORDER-007 and RH7103-007, layout example (fixture h, RH7103): no code fix. Without `_y`, the fix moves `_s` in front of `_x`.
- Change doc case 10, struct input with `_r` (fixture c, RH7109): no code fix.

## Output questions

_None._ Decisions 12 and 13 cover the withheld fixes under the approved *Build configurations* bullet. Every "after" in case 10 is "no code fix is offered", which that bullet demands.

## Internal choices

- I stated the layout counting inside SHARED-ORDER-007 and did not add a new SHARED-ORDER-008 requirement for the layout guard. Doing so would restate the guard that SHARED-FIX-003 *Run time* owns, and the change doc's decision says it is not restated. The alternative is a separate requirement that owns layout move-safety.
- I kept the title of SHARED-ORDER-007. "Initializer execution order is kept" undersells the layout paragraph. The alternative is "Initializer order and field layout are kept", a rename that earlier handovers would no longer match.
- I extended case 10 instead of adding case 11. It is the same mechanism and the same requirement.

## Follow-up candidates

_None new._ The RH7102 hint of `18-auditor.md` (no member-order guard) is the auditor's candidate and stays with the lead.

## Architecture findings

_None._ Finding #9's structural cause is decision 12. This repair needs RH7103-007 to hold once that change lands.
